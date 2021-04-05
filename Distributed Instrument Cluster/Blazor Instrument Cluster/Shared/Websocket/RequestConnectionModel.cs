using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared {
	/// <summary>
	/// Class used for serializing and sending when requesting a connection from the server
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RequestConnectionModel {
		/// <summary>
		/// Name value of the remote device
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Location value of the remote device
		/// </summary>
		public string location { get; set; }
		/// <summary>
		/// Type value of the remote device
		/// </summary>
		public string type { get; set; }
		/// <summary>
		/// Subname of the specific connection
		/// </summary>
		public string subname { get; set; }

		/// <summary>
		/// Constructor for json
		/// </summary>
		public RequestConnectionModel() {
			
		}

		/// <summary>
		/// Constructor with input
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <param name="subname"></param>
		public RequestConnectionModel(string name, string location, string type, string subname) {
			this.name = name;
			this.location = location;
			this.type = type;
			this.subname = subname;
		}
	}
}
