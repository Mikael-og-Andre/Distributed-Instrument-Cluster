using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crestron_Library;
using System;

namespace Crestron_Test {
	[TestClass]
	public class CommandsTest {
		Commands commands = new Commands();

		[TestMethod]
		public void getMakeByteTest() {
			Assert.AreEqual(commands.getMakeByte("k"), 0x26);
			Assert.AreEqual(commands.getMakeByte("K"), 0x26);
			Assert.AreEqual(commands.getMakeByte("cAps"), 0x1e);
			Assert.AreEqual(commands.getMakeByte("["), 0x1b);
			Assert.AreEqual(commands.getMakeByte("8 (num)"), 0x60);
			Assert.AreNotEqual(commands.getMakeByte("["), 0xff);
			Assert.AreEqual(commands.getMakeByte("left"), 0x42);
			Assert.AreEqual(commands.getMakeByte("left button on"), 0x49);
			Assert.AreEqual(commands.getMakeByte("scroll up"), 0x57);
			Assert.ThrowsException<ArgumentException>(() => commands.getMakeByte("not a key"));
			Assert.ThrowsException<ArgumentException>(() => commands.getMakeByte("\""));
			Assert.ThrowsException<ArgumentException>(() => commands.getMakeByte("\n"));
		}

		[TestMethod]
		public void getBreakByteTest() {
			Assert.AreEqual(commands.getBreakByte("k"), 0xa6);
			Assert.AreEqual(commands.getBreakByte("K"), 0xa6);
			Assert.AreEqual(commands.getBreakByte("cAps"), 0x9e);
			Assert.AreEqual(commands.getBreakByte("["), 0x9b);
			Assert.AreEqual(commands.getBreakByte("8 (num)"), 0xe0);
			Assert.AreNotEqual(commands.getBreakByte("["), 0xff);
			Assert.ThrowsException<ArgumentException>(() => commands.getBreakByte("left"));
			Assert.ThrowsException<ArgumentException>(() => commands.getBreakByte("left button on"));
			Assert.ThrowsException<ArgumentException>(() => commands.getBreakByte("scroll up"));
			Assert.ThrowsException<ArgumentException>(() => commands.getBreakByte("not a key"));
			Assert.ThrowsException<ArgumentException>(() => commands.getBreakByte("\""));
			Assert.ThrowsException<ArgumentException>(() => commands.getBreakByte("\n"));
		}

		[TestMethod]
		public void getClickBytesTest() {
			CollectionAssert.AreEqual(commands.getClickBytes("k"), new byte[] { 0x26, 0xa6 });
			CollectionAssert.AreEqual(commands.getClickBytes("K"), new byte[] { 0x26, 0xa6 });
			CollectionAssert.AreEqual(commands.getClickBytes("cAps"), new byte[] { 0x1e, 0x9e });
			CollectionAssert.AreEqual(commands.getClickBytes("["), new byte[] { 0x1b, 0x9b });
			CollectionAssert.AreEqual(commands.getClickBytes("8 (num)"), new byte[] { 0x60, 0xe0 });
			CollectionAssert.AreNotEqual(commands.getClickBytes("["), new byte[] { 0x1b, 0xff });
			Assert.ThrowsException<ArgumentException>(() => commands.getClickBytes("left"));
			Assert.ThrowsException<ArgumentException>(() => commands.getClickBytes("left button on"));
			Assert.ThrowsException<ArgumentException>(() => commands.getClickBytes("scroll up"));
			Assert.ThrowsException<ArgumentException>(() => commands.getClickBytes("not a key"));
			Assert.ThrowsException<ArgumentException>(() => commands.getClickBytes("\""));
			Assert.ThrowsException<ArgumentException>(() => commands.getClickBytes("\n"));
		}

	}
}
