using Remote_Server.Crestron;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CrestronReceiver {

	/// <summary>
	/// Class used to receive commands from a CrestronClient
	/// </summary>
	public class Program {

		private static void Main(string[] args) {
			TestCrestron crestron = new TestCrestron();
			CrestronListener listener = new CrestronListener(new IPEndPoint(IPAddress.Any, 6981), crestron, 10);

			Task listenerTask = listener.run();

			Console.WriteLine("press any key to exit");
			Console.ReadKey();
		}
	}
}