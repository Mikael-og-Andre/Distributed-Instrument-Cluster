using System;
using System.Threading;
using System.Threading.Tasks;
using Server_Library;
using Server_Library.Authorization;
using Server_Library.Socket_Clients;

namespace ReceivingClientTester {
    class Program {
        static void Main(string[] args) {

	        Thread.Sleep(20000);
			Console.WriteLine("Starting client...");

	        ReceivingClient<dummyJsonObject> receivingClient = new ReceivingClient<dummyJsonObject>("127.0.0.1", 6981,
		        new ClientInformation("clientTester", "location", "type","crestronControl"), new AccessToken("access"),new CancellationToken(false));

	        Task.Run(() => {
				receivingClient.run();
	        });

	        while (true) {
		        if (receivingClient.getObjectFromClient(out dummyJsonObject output)) {
					Console.WriteLine("Received object number: {0} text: {1}",output.number,output.text);
		        }
	        }

        }
    }
}
