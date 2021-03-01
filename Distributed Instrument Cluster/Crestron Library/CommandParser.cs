using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;


//TODO: clean, complete and document.
namespace Crestron_Library
{
	/// <summary>
	/// Class pars commands to its corresponding byte code and executes it.
	/// Converts cursor position to movements.
	/// </summary>
	/// <author>Andre Helland</author>
	public class CommandParser {
		//TODO: Deprecate.
		Stack<string> cursorPosition = new Stack<string>();

		private readonly Commands commands = new Commands();
		private readonly SerialPortInterface serialPort;

		public CommandParser(SerialPortInterface serialPortInterface) {
			serialPort = serialPortInterface;


			Thread thread = new Thread(randomThread);
			thread.Start();
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
				//TODO: deprecate potentially.
				case "setcursor":
					setCursor(key);
					break;
				case "movecursor":
					moveCursor(key);
					break;
				default:
					throw new Exception("Pars failed, \"" + operation + "\" was not recognized as an operation");
			}
		}

		//Might be broken.
		private int dx = 0;
		private int dy = 0;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="move"></param>
		private void moveCursor(string move) {
			Console.WriteLine(move);
			move = move.Substring(move.IndexOf("(") + 1, move.IndexOf(")") - 1); // Trim off "( )"
			string[] moves = move.Split(",");

			//TODO: refactor or remove.
			//dx += int.Parse(moves[0]);
			//dy += int.Parse(moves[1]);


			//Ignore move command if serial cable is still executing to avoid over filling command buffer.
			if (serialPort.isExecuting()) return;

			if (Math.Abs(dx) >= scaleFactorL) {
				serialPort.SendBytes(commands.getMakeByte("magnitude large"));
				serialPort.SendBytes(commands.getMakeByte(dx > 0 ? "right" : "left"));
				dx -= (int) Math.Ceiling(scaleFactorL);

			} else {
				serialPort.SendBytes(commands.getMakeByte("magnitude small"));
				serialPort.SendBytes(commands.getMakeByte(dx > 0 ? "right" : "left"));
				dx -= (int) Math.Ceiling(scaleFactorS);
			}

			if (Math.Abs(dy) >= scaleFactorL) {
				serialPort.SendBytes(commands.getMakeByte("magnitude large"));
				serialPort.SendBytes(commands.getMakeByte(dy > 0 ? "down" : "up"));
				dy -= (int) Math.Ceiling(scaleFactorL);

			} else {
				serialPort.SendBytes(commands.getMakeByte("magnitude small"));
				serialPort.SendBytes(commands.getMakeByte(dy > 0 ? "down" : "up"));
				dy -= (int) Math.Ceiling(scaleFactorS);
			}
		}

		//TODO: refactor whole region/deprecate.
		#region Cursor Movement

		private void randomThread() {
			serialPort.SendBytes(commands.getMakeByte("magnitude large"));
			for (int i = 0; i < 200; i++) {
				serialPort.SendBytes(new List<byte> { commands.getMakeByte("left"), commands.getMakeByte("up") });
			}

			while (serialPort.isExecuting());

			while (true) {
					Thread.Sleep(50);
					while (serialPort.isExecuting());
				try {
					string temp = cursorPosition.Pop();
					Console.WriteLine(temp);
					setCursor(temp);
					cursorPosition.Clear();

				} catch {}
				Thread.Sleep(0);
				//cursorPosition.Clear();
			}
		}


		public void setCursor(string temp) {
			temp = temp.Substring(1, temp.Length - 2);  // Trim off "( )"
			string[] position = temp.Split(",");

			int x = int.Parse(position[0]);
			int y = int.Parse(position[1]);

			int[] deltas = calculateDelta(x, y);

			foreach(double d in deltas) {
				Console.WriteLine(d);
			}


			serialPort.SendBytes(commands.getMakeByte("magnitude large"));
			executeMove(deltas[0], deltas[1]);

			serialPort.SendBytes(commands.getMakeByte("magnitude small"));
			executeMove(deltas[2], deltas[3]);
		}

		private int x0 = 0;
		private int y0 = 0;
		private double xe = 0;
		private double ye = 0;

		double scaleFactorS = 1.5;
		double scaleFactorL = 12;
		private int[] calculateDelta(int x, int y) {
			double[] deltas = new double[4];
			int[] deltasInt = new int[4];
			int dx = x - x0;
			int dy = y - y0;

			deltas[0] = dx / scaleFactorL;
			deltas[1] = dy / scaleFactorL;

			dx %= 12;
			dy %= 12;

			deltas[2] = dx / scaleFactorS;
			deltas[3] = dy / scaleFactorS;

			//convert to integer
			for (int i = 0; i<4; i++) {
				if (deltas[i] < 0) {
					deltasInt[i] = (int) Math.Ceiling(deltas[i]);
				} else {
					deltasInt[i] = (int)Math.Floor(deltas[i]);
				}
			}


			//calculate error/drift.
			xe += dx - deltasInt[2] * scaleFactorS;
			ye += dy - deltasInt[3] * scaleFactorS;

			int xc = (int) Math.Floor(xe / scaleFactorS);
			int yc = (int) Math.Floor(ye / scaleFactorS);

			deltasInt[2] += xc;
			deltasInt[3] += yc;

			xe -= xc;
			ye -= yc;

			Console.WriteLine(xe);
			Console.WriteLine(ye);

			x0 = x;
			y0 = y;

			return deltasInt;
		}

		private void executeMove(int x, int y) {
			byte horizontal = commands.getMakeByte("right");
			byte vertical = commands.getMakeByte("down");

			if (x < 0) {
				horizontal = commands.getMakeByte("left");
			}

			if (y < 0) {
				vertical = commands.getMakeByte("up");
			}

			//TODO: mix input.
			x = Math.Abs(x);
			y = Math.Abs(y);


			int[] xy = new int[x + y];
			for (int i = 0; i < x + y; i++) {
				if (i < x) {
					xy[i] = 0;
				} else {
					xy[i] = 1;
				}
			}




			Random rnd = new Random();
			int[] mixed = xy.OrderBy(x => rnd.Next()).ToArray();

			foreach (int VARIABLE in mixed) {
				serialPort.SendBytes(VARIABLE == 0 ? horizontal : vertical);
			}


			//Not working mixing using mod:
			//if (x <= y)
			//{
			//	for (int i = 1; i < (x + y); i++)
			//		serialPort.SendBytes(i % x == 0 ? vertical : horizontal);
			//}
			//else
			//{
			//	for (int i = 1; i < (x + y); i++)
			//		serialPort.SendBytes(i % y == 0 ? horizontal : vertical);
			//}


			//No mixing
			//for (int i = 0; i < Math.Abs(x); i++)
			//	serialPort.SendBytes(horizontal);

			//for (int i = 0; i < Math.Abs(y); i++)
			//	serialPort.SendBytes(vertical);

		}






		public void spamIn(object sender, DataReceivedEventArgs e) {
			cursorPosition.Push(e.Data);
			//test(e.Data);
		}

		#endregion
	}
}