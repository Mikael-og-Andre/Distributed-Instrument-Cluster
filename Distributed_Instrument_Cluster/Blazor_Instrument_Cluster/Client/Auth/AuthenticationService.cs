using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Client.Auth.Models;
using Blazored.LocalStorage;

namespace Blazor_Instrument_Cluster.Client.Auth {
	/// <summary>
	/// https://www.youtube.com/watch?v=2c4p6RGtkps
	/// </summary>
	public class AuthenticationService : IAuthenticationService {
		private readonly HttpClient httpClient;
		private readonly AuthStateProvider authStateProvider;
		private readonly ILocalStorageService localStorage;

		public AuthenticationService(HttpClient httpClient, AuthStateProvider authStateProvider,
			ILocalStorageService localStorage) {
			this.httpClient = httpClient;
			this.authStateProvider = authStateProvider;
			this.localStorage = localStorage;
		}

		public async Task<AuthenticatedAccountModel> Login(AuthenticationAccountModel accountForAuthentication) {
			var data = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("grant_type", "password"),
				new KeyValuePair<string, string>("username", accountForAuthentication.email),
				new KeyValuePair<string, string>("password", accountForAuthentication.password)
			});

			var authResult = await httpClient.PostAsync("Account/loginAccount",data);
			var authContent = await authResult.Content.ReadAsStringAsync();

			if (authResult.IsSuccessStatusCode == false) {
				return null;
			}

			var result = JsonSerializer.Deserialize<AuthenticatedAccountModel>(authContent,new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

			await localStorage.SetItemAsync("authToken", result.accessToken);

			((AuthStateProvider)authStateProvider).notifyUserAuthentication(result.accessToken);

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.accessToken);
			return result;
		}

		public async Task Logout() {
			await localStorage.RemoveItemAsync("authToken");
			((AuthStateProvider)authStateProvider).notifyUserLogout();
		}
	}
}
