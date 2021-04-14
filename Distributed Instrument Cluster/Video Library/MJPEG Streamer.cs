using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Video_Library {

	/// <summary>
	/// MJPEG server manages connecting clients and responding to http requests.
	/// Set "image" to send a image to all connected clients.
	/// </summary>
	/// <author>Andre Helland</author>
	public class MJPEG_Streamer {

		private byte[] image;
		public byte[] Image {
			set {
				image = value;
				foreach (var client in clients) {
					client.frameSent = false;
				}
			}

		}
		public int fps { get; set; }
		public int portNumber {private set; get; }
		public bool isPortSet { get; set; } = false;

		private readonly List<Client> clients;
		private Thread thread;
		private CancellationTokenSource disposalTokenSource;

		private const string Boundary = "--boundary";
		private readonly byte[] header;


		/// <summary>
		/// Class for making a http mjpeg stream.
		/// </summary>
		/// <param name="fps">frame rate of stream (how fast images are sent to connected clients)</param>
		/// <param name="port">what port to start server on (default 0 assigns available port automatically</param>
		/// <author>Andre Helland</author>
		public MJPEG_Streamer(int fps=30, int port=0) {
			disposalTokenSource = new CancellationTokenSource();

			this.fps = fps;
			clients = new List<Client>();
			thread = null;
			header = getBytes("HTTP/1.1 200 OK\r\n" + "Content-Type: multipart/x-mixed-replace; boundary=" + Boundary + "\r\n");
			Start(port);
		}

		/// <summary>
		/// Starts mjpeg server.
		/// </summary>
		/// <param name="port">what port to start server on (default 0 assigns available port automatically</param>
		private void Start(int port) {
			lock (this) {
				thread = new Thread(ServerThread) { IsBackground = true };
				thread.Start(port);
			}
		}

		/// <summary>
		/// Starts server thread for handling new clients connecting.
		/// </summary>
		/// <param name="state">port number server is running at</param>
		private void ServerThread(object state) {
			try {
				Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				var endpoint = new IPEndPoint(IPAddress.Any, (int) state);
				portNumber = endpoint.Port;

				isPortSet = true;

				Server.Bind(endpoint);
				Server.Listen(10);

				Console.WriteLine($"MJPEG Server started on port {state}.");

				foreach (Socket socket in Server.IncommingConnectoins())
					ThreadPool.QueueUserWorkItem(ClientThread, new Client(socket));
			}
			catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		/// <summary>
		/// Thread for providing a connected client with http response and mjpeg data.
		/// </summary>
		/// <param name="client"></param>
		private void ClientThread(object Client) {
			var client = (Client) Client;
			var socket = client.socket;

			Console.WriteLine($"New client from {socket.RemoteEndPoint}");

			lock (clients)
				clients.Add(client);

			try {
				using var ns = new NetworkStream(socket, true);
				ns.Write(header);
				ns.Flush();

				while (true) {
					if (image == null) continue;
					if (!client.frameSent) {
						writeImage(ns, image);
						client.frameSent = true;
					}
				}
			}
			catch {
				// ignored
			}
			finally {
				lock (clients)
					clients.Remove(client);
			}
		}

		/// <summary>
		/// Sends jpeg with header info for MJPEG streaming to specified network stream.
		/// </summary>
		/// <param name="ns">network stream data will be sent to</param>
		/// <param name="image">jpeg image to send</param>
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

		/// <summary>
		/// Get byte data from string
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
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

	public class Client {
		public Socket socket { get; }
		public bool frameSent { get; set; }

		public Client(Socket socket) {
			this.socket = socket;
			frameSent = false;
		}
	}

	internal static class SocketExtensions {
		public static IEnumerable<Socket> IncommingConnectoins(this Socket server) {
			while (true)
				yield return server.Accept();
		}
	}
}
