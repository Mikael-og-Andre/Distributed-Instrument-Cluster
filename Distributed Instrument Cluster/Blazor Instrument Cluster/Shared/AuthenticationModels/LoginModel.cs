using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Blazor_Instrument_Cluster.Shared.AuthenticationModels {
	/// <summary>
	/// Model used when logging in to the system
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class LoginModel {

		[Required]
		public string username { get; set; }

		[Required]
		public string password { get; set; }

	}
}
