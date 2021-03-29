﻿using Instrument_Communicator_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Connection_Types;
using Instrument_Communicator_Library.Connection_Types.deprecated;
using Instrument_Communicator_Library.Enums;
using Instrument_Communicator_Library.Server_Listeners;
using Instrument_Communicator_Library.Server_Listeners.deprecated;
using Instrument_Communicator_Library.Socket_Clients;

namespace Communication_lib_Test {

	[TestClass]
	public class ListenerAndClientTest {

		[TestMethod]
		public void testSendingNormalStringsVideo() {
			//init vid listener
			int portVideo = 5055;
			IPEndPoint endpointVid = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portVideo);

			ListenerVideo vidListener = new ListenerVideo(endpointVid);

			Thread videoListenerThread = new Thread(() => vidListener.start());
			videoListenerThread.Start();

			//Communicator vid
			InstrumentInformation infoVid = new InstrumentInformation("name", "loc", "type");
			AccessToken accessTokenVid = new AccessToken("access");
			CancellationToken cancellationToken = new CancellationToken(false);

			VideoClient vidCom = new VideoClient("127.0.0.1", portVideo, infoVid, accessTokenVid, cancellationToken);
			Thread vidComThread = new Thread(() => vidCom.run());
			vidComThread.Start();

			ConcurrentQueue<VideoFrame> inputQueue = vidCom.getInputQueue();

			string[] strings = new string[] { "oooooooooooooooooooooooooooooooooooooooooooooooa long string", "s", "Hello !@$£@£$€@€@$${£€$", "12315127651294182491289049009++0" };
			foreach (string s in strings) {
				inputQueue.Enqueue(new VideoFrame(Encoding.ASCII.GetBytes(s)));
			}
			Thread.Sleep(50);
			List<VideoConnection> vidCons = vidListener.getVideoConnectionList();
			lock (vidCons) {
				foreach (VideoConnection con in vidCons) {
					ConcurrentQueue<VideoFrame> queue = con.getOutputQueue();
					foreach (string s in strings) {
						VideoFrame qOut;
						bool hasVal = queue.TryDequeue(out qOut);
						Assert.IsTrue(hasVal);
						Assert.AreEqual(s, Encoding.ASCII.GetString(qOut.value));
					}
				}
			}
		}

		[TestMethod]
		public void testSendingMessagesCrestron() {
			//init crestron Listener
			int portCrest = 5050;
			IPEndPoint endpointCres = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portCrest);
			ListenerCrestron crestronListener = new ListenerCrestron(endpointCres);

			Thread crestronListenerThread = new Thread(() => crestronListener.start());
			crestronListenerThread.Start();

			Thread.Sleep(500);

			//Communicator crestron
			InstrumentInformation infoCrest = new InstrumentInformation("name", "loc", "type");
			AccessToken accessTokenCrest = new AccessToken("access");
			CancellationToken cancellationToken = new CancellationToken(false);

			CrestronClient crestronClient = new CrestronClient("127.0.0.1", portCrest, infoCrest, accessTokenCrest, cancellationToken);
			Thread crestronComThread = new Thread(() => crestronClient.run());
			crestronComThread.Start();

			//wait for auth
			Thread.Sleep(1000);

			string stringy = "Wow this is a very cool test string";

			List<CrestronConnection> listCons = crestronListener.getCrestronConnectionList();
			Assert.AreNotEqual(listCons.Count, 0);
			lock (listCons) {
				foreach (CrestronConnection con in listCons) {
					ConcurrentQueue<Message> inputQueue = con.getSendingQueue();
					Message msg;
					msg = new Message(ProtocolOption.message, stringy);
					inputQueue.Enqueue(msg);
				}
			}

			ConcurrentQueue<string> outputQueue = crestronClient.getCommandOutputQueue();

			Assert.IsNotNull(outputQueue);
			Assert.IsTrue(crestronClient.isSocketConnected);

			string outs;
			bool hasVal = outputQueue.TryDequeue(out outs);
			if (!hasVal) {
				Stopwatch watch = new Stopwatch();
				watch.Start();
				while (!hasVal && watch.ElapsedMilliseconds < 500) {
					hasVal = outputQueue.TryDequeue(out outs);
				}
				if (!hasVal) {
					Assert.Fail("Failed, queue was not filled in time");
				}
				watch.Stop();
			}
			string outString = outs;
			Assert.IsNotNull(outString);
			Assert.AreEqual(stringy, outString);
		}

		[TestMethod]
		public void testExceptions() {
			CrestronClient cCom = new CrestronClient("127.0.0.1", 5090, new InstrumentInformation("name", "location", "type"), new AccessToken("access"), new CancellationToken(false));
			Action startCom = () => cCom.run();

			Assert.ThrowsException<SocketException>(startCom);
		}
	}
}