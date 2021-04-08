using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Video_Library {
	public class MJPEG_Streamer {

		public byte[] image { get; set; }
		public int fps { get; set; }
		public int portNumber { get; private set; }
		public bool isPortSet { get; set; } = false;

		private readonly List<Socket> clients;
		private Thread thread;
		private CancellationTokenSource disposalTokenSource;

		private const string Boundary = "--boundary";
		private readonly byte[] header;



		public MJPEG_Streamer(int fps=30, int port=0) {
			disposalTokenSource = new CancellationTokenSource();

			this.fps = fps;
			clients = new List<Socket>();
			thread = null;
			header = getBytes("HTTP/1.1 200 OK\r\n" + "Content-Type: multipart/x-mixed-replace; boundary=" + Boundary + "\r\n");
			Start(port);
		}

		private void Start(int port) {
			lock (this) {
				thread = new Thread(ServerThread) { IsBackground = true };
				thread.Start(port);
			}
		}

		private void ServerThread(object state) {
			try {
				Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				var endpoint = new IPEndPoint(IPAddress.Any, (int) state);
				portNumber = endpoint.Port;

				isPortSet = true;

				Server.Bind(endpoint);
				Server.Listen(10);

				Console.WriteLine($"MJPEG Server started on port {state}.");

				foreach (Socket client in Server.IncommingConnectoins())
					ThreadPool.QueueUserWorkItem(ClientThread, client);
			}
			catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		private void ClientThread(object client) {

			Socket socket = (Socket)client;

			Console.WriteLine($"New client from {socket.RemoteEndPoint}");

			lock (clients)
				clients.Add(socket);

			try {
				using var ns = new NetworkStream(socket, true);
				ns.Write(header);
				ns.Flush();

				//TODO: only send new frames.
				while (true) {
					if (image == null) continue;
					Thread.Sleep(1000/fps);
					writeImage(ns, image);
				}
			}
			catch {
				// ignored
			}
			finally {
				lock (clients)
					clients.Remove(socket);
			}
		}

		private void writeImage(NetworkStream ns, byte[] image) {
			var sb = new StringBuilder();

			//Image header
			sb.AppendLine();
			sb.AppendLine(Boundary);
			sb.AppendLine("Content-Type: image/jpeg");
			sb.AppendLine("Content-Length: " + image.Length);
			sb.AppendLine();

			ns.Write(getBytes(sb.ToString()));
			ns.Write(image);
			ns.Write(getBytes("\r\n"));

			ns.Flush();
		}

		private byte[] getBytes(string s) {
			return Encoding.ASCII.GetBytes(s);
		}

		/// <summary>
		/// Get cancellation token
		/// </summary>
		/// <returns></returns>
		public CancellationToken getCancellationToken() {
			return disposalTokenSource.Token;
		}

		public void Dispose() {
			disposalTokenSource.Cancel();
		}
	}

	internal static class SocketExtensions {
		public static IEnumerable<Socket> IncommingConnectoins(this Socket server) {
			while (true)
				yield return server.Accept();
		}
	}

}
