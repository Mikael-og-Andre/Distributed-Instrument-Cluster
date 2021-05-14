using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Blazor_Instrument_Cluster.Shared.AuthenticationModels {
	/// <summary>
	/// Model used when logging in to the system
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class AccountModel {

		[Required]
		[EmailAddress]
		[DataType(DataType.EmailAddress)]
		public string email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string password { get; set; }

	}
}
