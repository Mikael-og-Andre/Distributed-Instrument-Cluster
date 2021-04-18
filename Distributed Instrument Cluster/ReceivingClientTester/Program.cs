using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Server_Library;
using Server_Library.Authorization;
using Server_Library.Socket_Clients;
using PackageClasses;

namespace ReceivingClientTester {
    class Program {
        static void Main(string[] args) {

	        Thread.Sleep(10000);
			Console.WriteLine("Starting client...");

	        ReceivingClient receivingClient = new ReceivingClient("127.0.0.1", 6981,
		        new ClientInformation("Radar1", "device location", "device type","crestronControl"), new AccessToken("access"),new CancellationToken(false));

	        Task.Run(() => {
				receivingClient.run(0);
	        });

	        while (true) {
		        if (receivingClient.getBytesFromClient(out byte[] output)) {
			        ExampleCrestronMsgObject obj = JsonSerializer.Deserialize<ExampleCrestronMsgObject>(Encoding.UTF32.GetString(output).TrimStart('\0').TrimEnd('\0'));
					Console.WriteLine("Received object text: {0}",obj.msg);
		        }
				Thread.Sleep(10);
	        }

        }
    }
}
