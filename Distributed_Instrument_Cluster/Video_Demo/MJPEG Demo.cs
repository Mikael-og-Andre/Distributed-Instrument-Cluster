using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using Video_Library;
using OpenCvSharp;

namespace Video_Demo {
	class MJPEG_Demo {

		public static void Main(string[] args) {
			var thread = new Thread(startServer);
			thread.Start();
			startClient();
		}

		private static void startServer() {
			var device = new VideoDeviceInterface(0, (VideoCaptureAPIs)700, 1920, 1080);
			var streamer = new MJPEG_Streamer(8080);

			while (true) {
				streamer.Image = device.readJpg(10);
			}
		}

		private static void startClient() {
			var JPGstream = new MJPEG_Streamer(9090);
			var request = (HttpWebRequest) WebRequest.Create("http://localhost:8080/");
			var response = request.GetResponse();

			Console.WriteLine(((HttpWebResponse)response).StatusDescription);


			using (var stream = response.GetResponseStream()) {


				while (true) {

					var buffer = new byte[2];

					var header = new List<byte>();
					bool headerEnded = false;
					int lineBreaks = 0;

					while (!headerEnded) {
						stream.Read(buffer, 0, 1);
						header.Add(buffer[0]);
					
					
						//header.Add(buffer[0]);
						//header.Add(buffer[1]);



						//Console.WriteLine(string.Join(",", buffer));
						if (buffer[0].Equals(10) && buffer[1].Equals(13)) {
							lineBreaks++;
							if (lineBreaks == 4) headerEnded = true;
						}

						buffer[1] = buffer[0];

					}
				
				
					//Console.WriteLine(Encoding.ASCII.GetString(header.ToArray()));

					var headerS = Encoding.ASCII.GetString(header.ToArray());

					var lengthStartIndex = headerS.LastIndexOf(" ");

					int contentLength;
					try {
						contentLength = int.Parse(headerS[lengthStartIndex..]);
					}
					catch (Exception e) {
						Console.WriteLine(e);
						continue;
					}



					//Console.WriteLine($"contentLength {contentLength}");

					//Console.WriteLine(string.Join(",", header));

					contentLength += 6;

					//IEnumerable<byte> image;
					//var image = new List<byte>();

					//stream.Read(buffer, 0, 1);

					//var temp = new byte[1];
					//for (int i = 0; i < contentLength; i++) {
					//	stream.Read(temp, 0, 1);
					//	image[i] = temp[0];
					//}



					//var segmentList = new List<byte[]>();
					//var segmentSize = 100;

					//for (int i = 0; i <= contentLength / segmentSize; i++) {
					//	Thread.Sleep(10);
					//	var segmentSizeL = segmentSize;
					//	if (((i + 1) * segmentSize) > contentLength) {
					//		//Console.WriteLine("triggerd");
					//		segmentSizeL = contentLength - segmentSize * i;
					//		//Console.WriteLine(segmentSizeL);
					//	}

					//	byte[] segment;
					//	stream.Read(segment = new byte[segmentSizeL], 0, segmentSizeL);
					//	segmentList.Add(segment);
					//	//Console.WriteLine(string.Join(",", segment));

					//}

					//var image = new byte[contentLength];
					//image = segmentList.SelectMany(byteArr => byteArr).ToArray();



					//Console.WriteLine("ffs");

					//Console.WriteLine(string.Join(",", segmentList[0]));


					//var index = 0;
					//foreach (var segment in segmentList) {
					//	index += segment.Length;
					//	Array.Copy(segment,index,image,index ,segment.Length);


					//}



					//var image = new byte[contentLength];

					//stream.Read(image[..50], 0, 50);
					//stream.Read(image, 0, contentLength);
					//stream.Read(image, 0, contentLength);
					//Console.WriteLine("okokkokokokoko");

					var rtTemp = new byte[2];
					var rtTemp2 = new byte[contentLength - 2];
					var image = new byte[contentLength];

					//stream.Read(rtTemp, 0, 2);
					//stream.Read(rtTemp2, 0, contentLength - 2);

					Console.WriteLine(string.Join(",", rtTemp2));
					//Console.WriteLine(string.Join(",", image));



					JPGstream.Image = image.ToArray();
					
					return;
				}
				return;
			}



			#region cancer

			using (var stream = response.GetResponseStream()) {


				var reader = new StreamReader(stream);



				var j = 0;
				while (j < 100) {

					var header = new StringBuilder();
					for (int i = 0; i < 4; i++) {
						header.Append(reader.ReadLine()+"\n");
					}

					Console.WriteLine(header.ToString());


					var contentLength = int.Parse(header.ToString().Split("\n")[2][16..]);
					
					Console.WriteLine(contentLength);

					//Console.WriteLine(reader.ReadLine());
					//Console.WriteLine(reader.ReadLine());
					//Console.WriteLine(reader.ReadLine());
					//Console.WriteLine(reader.ReadLine());
					//Console.WriteLine(reader.ReadLine());


					//var bytes = new List<byte>();
					//var ints = new int[contentLength];
					//var buffer = new char[contentLength];
					var bytes = new byte[10000];


					for (int i = 0; i < contentLength+1000; i++) {
						//var temp = BitConverter.GetBytes(reader.Read());
						//bytes.Add(temp[0]);
						//bytes.Add(temp[1]);
						//bytes.Add(temp[2]);
						//bytes.Add(temp[3]);
						//bytes.Add(BitConverter.GetBytes(reader.Read())[0]);
						//bytes.Add(BitConverter.GetBytes(reader.Read())[0]);
						//ints[i] = reader.Read();

						//buffer[i] = (char) reader.Read();
						//reader.Read();
						//bytes.Add((byte)reader.BaseStream.ReadByte());
						

					}
					stream.Read(bytes, 0, 10000);
					Console.WriteLine(string.Join(",", bytes));



					//reader.Read(buffer, 0, contentLength);


					//foreach (var c in buffer) {
					//	Console.Write(c);
					//}

					//return;

					//var bytes = Encoding.ASCII.GetBytes(buffer);

					//Console.WriteLine(bytes.Length);

					//Console.WriteLine(Encoding.ASCII.GetString(bytes.ToArray()));

					//Console.WriteLine(bytes.Count);
					//var utfTrash = Encoding.Convert(Encoding.ASCII, Encoding.UTF7, bytes.ToArray());

					//Console.WriteLine(Encoding.UTF7.GetString(utfTrash));

					JPGstream.Image = bytes.ToArray();

					//try {
					//	var image = Cv2.ImDecode(utfTrash, ImreadModes.AnyColor);
					//	Cv2.ImShow("ffs", image);
					//	Cv2.WaitKey(1);
					//}
					//catch (Exception e) {
					//	Console.WriteLine(e);
					//}


					Console.WriteLine("realLine spam:");
					Console.WriteLine(reader.ReadLine());
					Console.WriteLine(reader.ReadLine());

					return;
					//j++;
				}

				#endregion




			}
		}
	}
}
