using System;
using System.Collections.Generic;
using System.Text;
using Instrument_Communicator_Library.Interface;

namespace Instrument_Communicator_Library.Information_Classes {
	public class VideoFrame : ISerializeableObject {
		public string value;

		public VideoFrame(string value) {
			this.value = value;
		}

		public void setFrame(VideoFrame v) {
			value = v.value;
		}

		public byte[] getBytes() {
			return Encoding.ASCII.GetBytes(value);
		}

		public object getObject(byte[] arrayBytes) {
			string val = Encoding.ASCII.GetString(arrayBytes);
			return new VideoFrame(val);
		}
	}
}
