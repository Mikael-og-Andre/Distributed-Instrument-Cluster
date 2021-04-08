using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using Video_Library;

namespace Video_Demo {
	class test {

		public static void Main(string[] args) {

			//var test = new test();


			var device = new VideoDeviceInterface(1, (VideoCaptureAPIs) 700, 1920, 1080);
			var streamer = new MJPEG_Streamer(8080, 30);

			while (true) {
				if (device.tryReadJpg(out byte[] image)) {
					streamer.image = image;
				}
			}

			//var device = new VideoDeviceInterface(1);

			//Console.WriteLine("ffs");

			////var ffs = VideoWriter.FourCC('M', 'J', 'P', 'G');
			//var ffs = VideoWriter.FourCC('H', '2', '6', '4');


			////var writer = new VideoWriter("http://192.168.1.164:6969/test", ffs, 30.0, new Size(1280, 720));

			//var writer = new VideoWriter("test.mp4", ffs, 60.0, new Size(1920, 1080));
			//int i = 0;
			//while (i<1000) {

			//	//Console.WriteLine("ffs");
			//	if (device.tryReadFrameBuffer(out Mat frame)) {
			//		i++;
			//		writer.Write(frame);
			//	}

			//}
			//writer.Release();
			//Console.WriteLine("done");


		}

		private List<Socket> _Clients;
		private Thread _Thread;
		private string Boundary = "--boundary";
		private VideoDeviceInterface device;

		public test() {
			_Clients = new List<Socket>();
			_Thread = null;
			device = new VideoDeviceInterface(1, VideoCaptureAPIs.DSHOW, 1920, 1080);
			Start(8080);
			
		}

		public void Start(int port) {

			lock (this) {
				_Thread = new Thread(ServerThread) {IsBackground = true};
				_Thread.Start(port);
			}

		}


		private void ServerThread(object state) {

			try {
				Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				Server.Bind(new IPEndPoint(IPAddress.Any, (int) state));
				Server.Listen(10);

				System.Diagnostics.Debug.WriteLine(string.Format("Server started on port {0}.", state));

				foreach (Socket client in Server.IncommingConnectoins())
					ThreadPool.QueueUserWorkItem(ClientThread, client);

			}
			catch (Exception e) {
				Console.WriteLine(e);
			}

		}

		//public IEnumerable<Socket> Clients { get { return _Clients; } }

		private void ClientThread(object client) {

			Socket socket = (Socket)client;

			System.Diagnostics.Debug.WriteLine(string.Format("New client from {0}", socket.RemoteEndPoint.ToString()));

			lock (_Clients)
				_Clients.Add(socket);

			try {
				using (NetworkStream ns = new  NetworkStream(socket, true)) {

					ns.Write(Encoding.ASCII.GetBytes(
						"HTTP/1.1 200 OK\r\n" +
						"Content-Type: multipart/x-mixed-replace; boundary=" +
						this.Boundary +
						"\r\n"
					));
					ns.Flush();

					//// Streams the images from the source to the client.
					//foreach (var imgStream in Screen.Streams(this.ImagesSource)) {
					//	if (this.Interval > 0)
					//		Thread.Sleep(this.Interval);

					//	wr.Write(imgStream);
					//}

					//var image = System.IO.File.ReadAllBytes("download.jpg");

					while (true) {

						//writeImage(ns, image);

						if (device.tryReadJpg(out byte[] image, 70)) {
							//Cv2.ImShow("ffs",Cv2.ImDecode(image, ImreadModes.AnyColor));
							//Cv2.WaitKey(1);
							writeImage(ns, image);
						}
					}
				}
			}
			catch { }
			finally {
				lock (_Clients)
					_Clients.Remove(socket);
			}
		}

		private void writeImage(NetworkStream ns, byte[] image) {
			

			StringBuilder sb = new StringBuilder();

			sb.AppendLine();
			sb.AppendLine(this.Boundary);
			sb.AppendLine("Content-Type: image/jpeg");
			sb.AppendLine("Content-Length: " + image.Length.ToString());
			sb.AppendLine();

			ns.Write(Encoding.ASCII.GetBytes((sb.ToString())));
			ns.Write(image);
			ns.Write(Encoding.ASCII.GetBytes("\r\n"));

			ns.Flush();
		}
	}

	internal static class SocketExtensions {

		public static IEnumerable<Socket> IncommingConnectoins(this Socket server) {
			while (true)
				yield return server.Accept();
		}

	}
}
