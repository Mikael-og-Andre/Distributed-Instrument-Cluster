using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;

namespace Blazor_Instrument_Cluster.Client.Auth {
	public interface IAuthService {

		public Task<RegisterResult> Register(RegisterModel registerModel);

		public Task<LoginResult> Login(LoginModel loginModel);

		public Task Logout();

	}
}
