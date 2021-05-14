using System;
using System.Net;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Client.Pages;
using Blazor_Instrument_Cluster.Server.CrestronControl;
using Remote_Server.Crestron;

namespace CrestronReceiver {
	/// <summary>
	/// Class used to receive commands from a CrestronClient
	/// </summary>
	public class Program {
		static void Main(string[] args) {
			TestCrestron crestron = new TestCrestron();
			CrestronListener listener = new CrestronListener(new IPEndPoint(IPAddress.Any, 6981), crestron, 10);

			Task listenerTask = listener.run();

			Console.WriteLine("press any key to exit");
			Console.ReadKey();
		}
	}
}
