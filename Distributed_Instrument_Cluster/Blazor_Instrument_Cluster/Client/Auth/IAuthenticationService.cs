using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Client.Auth.Models;

namespace Blazor_Instrument_Cluster.Client.Auth {
	public interface IAuthenticationService {
			public Task<AuthenticatedAccountModel> Login(AuthenticationAccountModel accountForAuthentication);
			public Task Logout();
		}
	
}
