using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Server_Library;
using Server_Library.Authorization;
using Server_Library.Socket_Clients;
using Server_Library.Socket_Clients.Async;

namespace ReceivingClientTester {
	public class ProgramAsync {
		static void Main(string[] args) {

			Thread.Sleep(10000);
			Console.WriteLine("Starting client...");

			AccessToken accessToken = new AccessToken("auth");
			DuplexClientAsync client = new DuplexClientAsync("192.168.50.62", 6981,accessToken);

			Console.WriteLine("Setting up");
			client.setup().Wait();
			Console.WriteLine("Setup Complete");

			while (true) {
				byte[] bytes= client.receiveBytesAsync().Result;
				string rec = Encoding.UTF32.GetString(bytes);
				Console.WriteLine("Received: {0}",rec);
			}

		}
	}
}
