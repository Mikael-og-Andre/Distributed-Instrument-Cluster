using System;
using System.Collections.Generic;

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

		public CommandParser(SerialPortInterface serialPortInterface) {
			serialPort = serialPortInterface;
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
			//key = key.Substring(0, key.IndexOf('\0'));		//Trim off null bytes.

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
					throw new Exception("Pars failed, \"" + operation + "\" was not recognized as an operation");
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

		private int scaleFactorL = 20;	//Threshold for when to use large magnitude movements.
		private int scaleFactorS = 2;	//Threshold for when to use small magnitude movements.

		private int dx = 0;
		private int dy = 0;

		/// <summary>
		/// Receives deltas and convert it to byte code for movement in correct direction and magnitude.
		/// TODO: movement is horrible, fix.
		/// </summary>
		/// <param name="move">String containing deltas, format: (-10,7)</param>
		private void moveCursor(string move) {
			move = move.Substring(move.IndexOf("(") + 1, move.IndexOf(")") - 1); // Trim off "( )"
			string[] moves = move.Split(",");

			dx += int.Parse(moves[0]);
			dy += int.Parse(moves[1]);

			Console.WriteLine("x:" + dx + ",y:" + dy);


			//Ignore move command if serial cable is still executing to avoid over filling command buffer.
			if (serialPort.isExecuting()) return;

			executeMoves(dx,dy);
		}


		private void executeMoves(int x, int y) {
			while (Math.Abs(x) > scaleFactorS || Math.Abs(y) > scaleFactorS) {
				if (Math.Abs(x) >= scaleFactorL) {
					serialPort.SendBytes(commands.getMakeByte("magnitude large"));
					serialPort.SendBytes(commands.getMakeByte(x > 0 ? "right" : "left"));
					x -= scaleFactorL * (x > 0 ? 1 : -1);
					dx -= scaleFactorL * (dx > 0 ? 1 : -1);

				}
				else if (Math.Abs(x) >= scaleFactorS) {
					serialPort.SendBytes(commands.getMakeByte("magnitude small"));
					serialPort.SendBytes(commands.getMakeByte(x > 0 ? "right" : "left"));
					x -= scaleFactorS * (x > 0 ? 1 : -1);
					dx -= scaleFactorS * (dx > 0 ? 1 : -1);
				}

				if (Math.Abs(y) >= scaleFactorL) {
					serialPort.SendBytes(commands.getMakeByte("magnitude large"));
					serialPort.SendBytes(commands.getMakeByte(y > 0 ? "down" : "up"));
					y -= scaleFactorL * (y > 0 ? 1 : -1);
					dy -= scaleFactorL * (dy > 0 ? 1 : -1);
				}
				else if (Math.Abs(y) >= scaleFactorS) {
					serialPort.SendBytes(commands.getMakeByte("magnitude small"));
					serialPort.SendBytes(commands.getMakeByte(y > 0 ? "down" : "up"));
					y -= scaleFactorS * (y > 0 ? 1 : -1);
					dy -= scaleFactorS * (dy > 0 ? 1 : -1);
				}
			}
		}
	}
}