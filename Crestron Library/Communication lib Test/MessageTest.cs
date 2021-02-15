using Microsoft.VisualStudio.TestTools.UnitTesting;
using Instrument_Communicator_Library;
using System.Net;
using System.Threading;

namespace Communication_lib_Test {
    [TestClass]
    class MessageTest {

        [TestInitialize]
        public void testStartStopServer() {

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"),5050);

            InstrumentServer server = new InstrumentServer(endpoint);
            server.StartListening();
            Thread.Sleep(2000);
            bool isRunning = server.isServerRunning;
            Assert.IsTrue(isRunning);
            server.StopServer();
            isRunning = server.isServerRunning;
            Assert.IsFalse(isRunning);
        }

        [TestInitialize]
        public void testClientServer() {

        }


    }
}
