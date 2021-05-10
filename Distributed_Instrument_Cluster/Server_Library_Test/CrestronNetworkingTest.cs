using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.CrestronControl;
using Remote_Server.Crestron;
using Server_Library.Socket_Clients;

namespace Server_Library_Test {
	/// <summary>
	/// Test for the netowrking in Remote Server and Blazor Server
	/// </summary>
	[TestClass]
	public class CrestronNetworkingTest {

		[TestMethod]
		public async Task crestronSendReceiveTest() {

			CrestronClient client = new CrestronClient("127.0.0.1", 9879);
			AssertionCrestron crestron = new AssertionCrestron();
			CrestronListener listener = new CrestronListener(new IPEndPoint(IPAddress.Any, 9879), crestron);

			Task listenerTask = listener.run();

			await client.connect();

			List<string> strings = new List<string>() {"hello", "124123#¤%&/", "hello4", "hello5                    i"};

			foreach (var s in strings) {
				await client.send(s);
			}
			

			while (crestron.queue.Count<4) {
				await Task.Delay(100);
			}

			ConcurrentQueue<string> assertionQueue = crestron.queue;
			foreach (var s in strings) {

				if (assertionQueue.TryDequeue(out string result)) {
					Assert.AreEqual(s,result);
				}
				else {
					Assert.Fail("No messages in queue when expecting {0}",s);
				}
			}

		}
	}

	internal class AssertionCrestron : ICrestronControl {

		public ConcurrentQueue<string> queue;

		public AssertionCrestron() {
			queue = new ConcurrentQueue<string>();
		}

		public Task sendCommandToCrestron(string msg) {
			queue.Enqueue(msg);
			return Task.CompletedTask;
		}
	}

}