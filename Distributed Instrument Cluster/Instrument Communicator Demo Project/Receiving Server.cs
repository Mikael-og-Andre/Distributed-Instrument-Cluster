using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HardwareServer_Demo_Project;
using Server_Library;
using Server_Library.Authorization;
using Server_Library.Server_Listeners;
using Server_Library.Socket_Clients;

namespace serverDemo {

	public class Receiving_Server {

		private static void Main(string[] args) {


			ReceivingListener<exampleObject> receiver = new ReceivingListener<exampleObject>(new IPEndPoint(IPAddress.Parse("127.0.0.1"),5050 ));

			Task serverTask = new Task( () => receiver.start());
			serverTask.Start();

			Thread.Sleep(1000);


			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			SendingClient<exampleObject> sendingClient = new SendingClient<exampleObject>("127.0.0.1", 5050, new ClientInformation("sendingClient","here","teseting"),new AccessToken("access"),cancellationTokenSource.Token);
			Task sendingClientTask = new Task( () => sendingClient.run() );
			sendingClientTask.Start();

			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				
				receiver.

			}

		}
	}
}