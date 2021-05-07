using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Crestron_Library
{
	/// <summary>
	/// Class pars commands to its corresponding byte code and executes it.
	/// Converts cursor delta to movements.
	/// </summary>
	/// <author>Andre Helland</author>
	public class CommandParser {
		private readonly Commands commands = new();
		private readonly SerialPortInterface serialPort;

		public CommandParser(SerialPortInterface serialPortInterface, int largeMagnitude = 20, int smallMagnitude = 1, int maxDelta = 1000) {
			serialPort = serialPortInterface;
			this.largeMagnitude = largeMagnitude;
			this.smallMagnitude = smallMagnitude;
			this.maxDelta = maxDelta;
		}

		/// <summary>
		/// Pars commands from string to bytes and execute the commands by sending it to the serial port.
		/// </summary>
		/// <param name="toPars">Command to pars.</param>
		public void pars(string toPars) {
			//Try to split string.
			string[] split = toPars.Split(" ");
			if (split.Length < 2) {
				throw new Exception("Parsing failed, could not split string");
			}

			string operation = split[0].ToLower();
			string key = toPars.Substring(operation.Length+1).ToLower();

			switch (operation) {
				case "make":
					serialPort.SendBytes(commands.getMakeByte(key));
					break;
				case "break":
					serialPort.SendBytes(commands.getBreakByte(key));
					break;
				case "movecursor":
					moveCursor(key);
					break;
				case "mouseclick":
					mouseClick(key);
					break;
				default:
					throw new Exception($"Pars failed, \"{operation}\" was not recognized as an operation");
			}
		}


		private void mouseClick(string button) {
			button = button.Substring(button.IndexOf("(") + 1, button.IndexOf(")") - 1);    // Trim off "( )"
			string[] args = button.Split(",");

			string onOff = (args[1] == "1" ? "on" : "off");

			switch (int.Parse(args[0])) {
				case 0:
					serialPort.SendBytes(commands.getMakeByte("left button " + onOff));
					break;
				case 1:
					serialPort.SendBytes(commands.getMakeByte("middle button " + onOff));
					break;
				case 2:
					serialPort.SendBytes(commands.getMakeByte("right button " + onOff));
					break;
				default:
					throw new Exception("Button with index " + button + " is not supported");
			}
		}

		#region MouseMovement

		/// <summary>
		/// Threshold for when to use large magnitude movements.
		/// </summary>
		private readonly int largeMagnitude;
		/// <summary>
		/// Threshold for when to use small magnitude movements.
		/// </summary>
		private readonly int smallMagnitude;
		/// <summary>
		/// How much delta the algorithm will accumulate (per axis) before discarding additional delta.
		/// </summary>
		private readonly int maxDelta;
		
		private readonly ConcurrentQueue<int[]> deltas = new();
		private bool isExecuting = false;

		/// <summary>
		/// Receives deltas and convert it to byte code for movement in correct direction and magnitude.
		/// </summary>
		/// <param name="move">String containing deltas, format: (-10,7)</param>
		private void moveCursor(string move) {
			move = move.Substring(move.IndexOf("(") + 1, move.IndexOf(")") - 1); // Trim off "( )"
			string[] moves = move.Split(",");

			var dx = int.Parse(moves[0]);
			var dy = int.Parse(moves[1]);

			deltas.Enqueue(new int[]{dx,dy});

			if (isExecuting) return;
			var thread = new Thread(executeMovementThread);
			isExecuting = true;
			thread.Start();
		}


		/// <summary>
		/// Thread adds up deltas from queue and executes movement
		/// till queue is empty and deltas are bellow small scale factor.
		/// </summary>
		private void executeMovementThread() {
			var dx = 0;
			var dy = 0;
			while (!deltas.IsEmpty || Math.Abs(dx)>smallMagnitude || Math.Abs(dy)>smallMagnitude) {
				(dx, dy) = fetchDeltas(dx, dy);
				(dx, dy) = executeMovement(dx,dy);
			}
			isExecuting = false;
		}

		/// <summary>
		/// Empties queue containing deltas adds it to current deltas (dx, dy).
		/// Discards additional deltas over "maxDelta" limit.
		/// </summary>
		/// <returns></returns>
		private (int, int) fetchDeltas(int dx, int dy) {
			while (deltas.TryDequeue(out var delta)) {
				if (Math.Abs(dx + delta[0]) < maxDelta) dx += delta[0];
				if (Math.Abs(dy + delta[1]) < maxDelta) dy += delta[1];
			}
			return (dx, dy);
		}

		private bool isMagnitudeLarge = false;

		/// <summary>
		/// Sends movement bytes to serial cable and adjusts magnitude to reduce movement lag as much as possible.
		/// </summary>
		/// <param name="dx">X delta</param>
		/// <param name="dy">Y delta</param>
		/// <returns>Deltas with amount moved subtracted</returns>
		private (int, int) executeMovement(int dx, int dy) {
			//Don't run method if serial port is still executing.
			if(serialPort.isExecuting()) return (dx, dy);

			var xScale = Math.Abs(dx) >= largeMagnitude ? largeMagnitude : smallMagnitude;
			var yScale = Math.Abs(dy) >= largeMagnitude ? largeMagnitude : smallMagnitude;

			//Only change magnitude if deltas are above or bellow current magnitude threshold.
			if ((xScale == largeMagnitude || yScale == largeMagnitude)) {
				if (!isMagnitudeLarge) {
					serialPort.SendBytes(commands.getMakeByte("magnitude large"));
					isMagnitudeLarge = true;
				}
			} else {
				if (isMagnitudeLarge) {
					serialPort.SendBytes(commands.getMakeByte("magnitude small"));
					isMagnitudeLarge = false;
				}
			}

			var executionScale = isMagnitudeLarge ? largeMagnitude : smallMagnitude;
			if (Math.Abs(dx) >= executionScale) {
				serialPort.SendBytes(commands.getMakeByte(dx > 0 ? "right" : "left"));
				dx -= executionScale * (dx > 0 ? 1 : -1);
			}
			if (Math.Abs(dy) >= executionScale) {
				serialPort.SendBytes(commands.getMakeByte(dy > 0 ? "down" : "up"));
				dy -= executionScale * (dy > 0 ? 1 : -1);
			}

			return (dx, dy);
		}

		#endregion
	}
}