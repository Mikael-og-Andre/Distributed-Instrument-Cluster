using System.Collections.Generic;

namespace MAIN_Program {
	/// <summary>
	/// Json for storing launch config info for program on remote device:
	/// Video setting, crestron cable settings, server ip and port settings and device info.
	/// </summary>
	/// <author>Andre Helland</author>
	internal class JsonClasses {

		public CrestronCable crestronCable { get; set; }
		public List<VideoDevice> videoDevices { get; set; }
		public ServerSettings serverSettings { get; set; }

		public class CrestronCable {
			public string portName { get; set; }
			public int largeMagnitude { get; set; }
			public int smallMagnitude { get; set; }
			public int maxDelta { get; set; }
		}

		public class VideoDevice {
			public int deviceIndex { get; set; }
			public int apiIndex { get; set; }
			public int width { get; set; }
			public int height { get; set; }
			public int quality { get; set; }
			public int fps { get; set; }

		}

		public class ServerSettings {
			public string ip { get; set; }
			public int crestronPort { get; set; }
			public int videoPort { get; set; }

		}
	}
}
