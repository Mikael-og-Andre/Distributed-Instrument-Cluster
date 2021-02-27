using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Server_Listener;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using Instrument_Communicator_Library.Remote_Device_side_Communicators;

namespace Communication_lib_Test {

    [TestClass]
    public class MessageTest {

        [TestMethod]
        public void testSendingNormalStringsVideo() {
            //init vid listener
            int portVideo = 5055;
            IPEndPoint endpointVid = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portVideo);

            ListenerVideo<string> vidListener = new ListenerVideo<string>(endpointVid);

            Thread videoListenerThread = new Thread(() => vidListener.start());
            videoListenerThread.Start();

            //Communicator vid
            InstrumentInformation infoVid = new InstrumentInformation("name", "loc", "type");
            AccessToken accessTokenVid = new AccessToken("access");
            CancellationToken cancellationToken = new CancellationToken(false);

            VideoCommunicator<string> vidCom = new VideoCommunicator<string>("127.0.0.1", portVideo, infoVid, accessTokenVid, cancellationToken);
            Thread vidComThread = new Thread(() => vidCom.Start());
            vidComThread.Start();

            ConcurrentQueue<string> inputQueue = vidCom.GetInputQueue();

            string[] strings = new string[] { "oooooooooooooooooooooooooooooooooooooooooooooooa long string", "s", "Hello !@$£@£$€@€@$${£€$", "12315127651294182491289049009++0" };
            foreach (string s in strings) {
                inputQueue.Enqueue(s);
            }
            Thread.Sleep(50);
            List<VideoConnection<string>> vidCons = vidListener.getVideoConnectionList();
            lock (vidCons) {
                foreach (VideoConnection<string> con in vidCons) {
                    ConcurrentQueue<string> queue = con.GetOutputQueue();
                    foreach (string s in strings) {
                        string qOut;
                        bool hasVal = queue.TryDequeue(out qOut);
                        Assert.IsTrue(hasVal);
                        Assert.AreEqual(s, qOut);
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

            CrestronCommunicator crestronCommunicator = new CrestronCommunicator("127.0.0.1", portCrest, infoCrest, accessTokenCrest, cancellationToken);
            Thread crestronComThread = new Thread(() => crestronCommunicator.Start());
            crestronComThread.Start();

            //wait for auth
            Thread.Sleep(1000);

            string[] stringList = new string[] { "this is a test", "HASIDHASIDHAISDHIISDhi" };

            List<CrestronConnection> listCons = crestronListener.getCrestronConnectionList();
            Assert.AreNotEqual(listCons.Count, 0);
            lock (listCons) {
                foreach (CrestronConnection con in listCons) {
                    ConcurrentQueue<Message> inputQueue = con.GetInputQueue();
                    Message msg;
                    msg = new Message(protocolOption.message, stringList);
                    inputQueue.Enqueue(msg);
                }
            }
            

            ConcurrentQueue<string> outputQueue = crestronCommunicator.getCommandOutputQueue();

            Assert.IsNotNull(outputQueue);
            Assert.IsTrue(crestronCommunicator.isSocketConnected);

            foreach (string s in stringList) {
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
                Assert.AreEqual(s, outString);
            }
        }

        [TestMethod]
        public void testExceptions() {
            CrestronCommunicator cCom = new CrestronCommunicator("127.0.0.1", 5090, new InstrumentInformation("name", "location", "type"), new AccessToken("access"),new CancellationToken(false));
            Action startCom = () => cCom.Start();

            Assert.ThrowsException<SocketException>(startCom);
        }
    }
}