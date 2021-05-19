using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement.JsonReading {
	/// <summary>
	/// Class for loading remote devices from json file
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public static class RemoteDeviceLoader {
		public static List<RemoteDevice> getRemoteDevicesFromJsonFile(string pathToJson) {

			RemoteDeviceList list = parsConfigFile(pathToJson);
			List<RemoteDevice> devices = new List<RemoteDevice>();
			int i = 0;
			foreach (var device in list.RemoteDevices) {
				if (device.hasCrestron) {
					devices.Add(new RemoteDevice(i, device.ip,device.CrestronBasePort,device.VideoBasePort,device.VideoDevices,device.name,device.location,device.type));
				}
				else {
					devices.Add(new RemoteDevice(i, device.ip, device.VideoBasePort, device.VideoDevices, device.name, device.location, device.type));
				}
				
				i++;
			}

			return devices;
		}

		/// <summary>
		/// Read from the configuration file, and load the data into a List of Remote devices
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		private static RemoteDeviceList parsConfigFile(string file) {
			var jsonString = File.ReadAllText(file);
			var json = JsonSerializer.Deserialize<RemoteDeviceList>(jsonString, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
			return json;
		}
	}
}
