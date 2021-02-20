using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


//TODO: clean, complete and document.
namespace Crestron_Library {
	public class CommandParser {
		Stack<string> cursorPosition = new Stack<string>();
		Commands commands = new Commands();
		SerialPortInterface serialPort = new SerialPortInterface("COM5");

		public CommandParser() {
			Thread thread = new Thread(randomThread);
			thread.Start();
		}

		private void randomThread() {
			serialPort.SendBytes(commands.getMakeByte("magnitude large"));
			for (int i = 0; i < 100; i++)
				serialPort.SendBytes(new List<byte> { commands.getMakeByte("left"), commands.getMakeByte("up") });

			serialPort.SendBytes(commands.getMakeByte("magnitude small"));
			while(serialPort.isExecuting())

			while (true) {
				Thread.Sleep(50);
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



		public List<byte> parse(string idk) {
			List<byte> bytes = new List<byte>();





			return bytes;

		}




		public void setCursor(string temp) {
			temp = temp.Substring(1, temp.Length - 2);	// Trim off "( )"
			var position = temp.Split(",");

			int x = int.Parse(position[0]);
			int y = int.Parse(position[1]);

			var deltas = calculateDelta(x, y);

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
		double scaleFactorS = 1.5;
		double scaleFactorL = 12;
		private double[] calculateDelta(int x, int y) {
			double[] deltas = new double[4];
			int dx = x - x0;
			int dy = y - y0;

			deltas[0] = dx / scaleFactorL;
			deltas[1] = dy / scaleFactorL;

			dx = dx % 12;
			dy = dy % 12;

			deltas[2] = dx / scaleFactorS;
			deltas[3] = dy / scaleFactorS;

			x0 = x;
			y0 = y;

			return deltas;
		}

		//TODO: Mix horizontal and vertical movement
		private void executeMove(double x, double y) {
			for (int i = 0; i < Math.Floor(x); i++)
				serialPort.SendBytes(commands.getMakeByte("right"));

			for (int i = 0; i < Math.Floor(y); i++)
				serialPort.SendBytes(commands.getMakeByte("down"));

			if (x < 0) {
				x /= -1;
				for (int i = 0; i < Math.Floor(x); i++)
					serialPort.SendBytes(commands.getMakeByte("left"));
			}

			if (y < 0) {
				y /= -1;
				for (int i = 0; i < Math.Floor(y); i++)
					serialPort.SendBytes(commands.getMakeByte("up"));
			}
		}


		public void spamIn(object sender, DataReceivedEventArgs e) {
			cursorPosition.Push(e.Data);
		}
	}
}