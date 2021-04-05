using System;
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

	        ReceivingClient<ExampleCrestronMsgObject> receivingClient = new ReceivingClient<ExampleCrestronMsgObject>("127.0.0.1", 6981,
		        new ClientInformation("clientTester", "location", "type","crestronControl"), new AccessToken("access"),new CancellationToken(false));

	        Task.Run(() => {
				receivingClient.run(0);
	        });

	        while (true) {
		        if (receivingClient.getObjectFromClient(out ExampleCrestronMsgObject output)) {
					Console.WriteLine("Received object text: {0}",output.msg);
					continue;
		        }
				Thread.Sleep(10);
	        }

        }
    }
}
