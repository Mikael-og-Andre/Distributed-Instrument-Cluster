using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Client.Auth;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Microsoft.AspNetCore.Components;

namespace Blazor_Instrument_Cluster.Client.Code.Authentication {
	public class RegistrationPage : ComponentBase {
		[Inject]
		public IAuthService AuthService { get; set; }
		[Inject]
		public NavigationManager NavigationManager { get; set; }

		protected RegisterModel RegisterModel = new RegisterModel();
		protected bool ShowErrors { get; set; }
		protected IEnumerable<string> Errors { get; set; }

		protected async Task HandleRegistration() {
			try {
				ShowErrors = false;
				Errors = new List<string>() { }.AsEnumerable();

				var crypt = new SHA256Managed();
				var hash = new StringBuilder();
				byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(RegisterModel.Password));
				foreach (byte theByte in crypto) {
					hash.Append(theByte.ToString("x2"));
				}
				string hashString = hash.ToString();

				var registerModelHashed = new RegisterModel();
				registerModelHashed.Email = RegisterModel.Email;
				registerModelHashed.Password = hashString;
				registerModelHashed.ConfirmPassword = hashString;

				RegisterResult result = await AuthService.Register(registerModelHashed);
				
				if (result.Successful) {
					NavLogin();
				}
				else {
					ShowErrors = true;
					Errors = result.Errors;
				}
			} catch (Exception e) {
				ShowErrors = true;
				Errors = new List<string>() { "Something went wrong"}.AsEnumerable();
			}
		}

		protected void NavLogin() {
			NavigationManager.NavigateTo("/Login");
		}
	}
}
