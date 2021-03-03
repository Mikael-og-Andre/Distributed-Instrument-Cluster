using System;
using System.Collections.Generic;
using System.Text;
using Instrument_Communicator_Library.Interface;

namespace Instrument_Communicator_Library.Information_Classes {
	/// <summary>
	/// Represents a Frame of video - CURRENTLY NOT SETUP FOR VIDEO TODO: Make video frame
	/// </summary>
	
	public class VideoFrame : ISerializeableObject {
		public string value;

		public VideoFrame(string value) {
			this.value = value;
		}

		public void setFrame(VideoFrame v) {
			value = v.value;
		}

		/// <summary>
		/// Get Bytes representing video frame
		/// </summary>
		/// <returns></returns>
		public byte[] getBytes() {
			return Encoding.ASCII.GetBytes(value);
		}

		/// <summary>
		/// Get object from array of bytes
		/// </summary>
		/// <param name="arrayBytes"></param>
		/// <returns></returns>
		public object getObject(byte[] arrayBytes) {
			string val = Encoding.ASCII.GetString(arrayBytes).Trim('\0');
			return new VideoFrame(val);
		}
	}
}
