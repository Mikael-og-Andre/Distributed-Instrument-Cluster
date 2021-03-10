using Instrument_Communicator_Library.Interface;

namespace Instrument_Communicator_Library {

	/// <summary>
	/// Represents a Frame of video - CURRENTLY NOT SETUP FOR VIDEO TODO: Make video frame
	/// </summary>
	public class VideoFrame : ISerializeObject {

		/// <summary>
		/// Value of the video frame in bytes
		/// </summary>
		public byte[] value;

		public VideoFrame(byte[] value) {
			this.value = value;
		}

		/// <summary>
		/// sets the value of the frame to the same as the input frame
		/// </summary>
		/// <param name="v"></param>
		public void setFrame(VideoFrame v) {
			value = v.value;
		}

		/// <summary>
		/// Get Bytes representing video frame
		/// </summary>
		/// <returns></returns>
		public byte[] getBytes() {
			return value;
		}

		/// <summary>
		/// Get object from array of bytes
		/// </summary>
		/// <param name="arrayBytes"></param>
		/// <returns></returns>
		public object getObject(byte[] arrayBytes) {
			return new VideoFrame(arrayBytes);
		}
	}
}