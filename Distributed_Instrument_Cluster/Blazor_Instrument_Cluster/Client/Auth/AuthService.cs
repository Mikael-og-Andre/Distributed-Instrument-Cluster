using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazor_Instrument_Cluster.Client.Auth {
	public class AuthService : IAuthService {
		private readonly HttpClient _httpClient;
		private readonly AuthenticationStateProvider _authenticationStateProvider;
		private readonly ILocalStorageService _localStorage;

		public AuthService(HttpClient httpClient,
			AuthenticationStateProvider authenticationStateProvider,
			ILocalStorageService localStorage) {
			_httpClient = httpClient;
			_authenticationStateProvider = authenticationStateProvider;
			_localStorage = localStorage;
		}

		public async Task<RegisterResult> Register(RegisterModel registerModel) {
			var modelAsJson = JsonSerializer.Serialize(registerModel);
			var result = await _httpClient.PostAsync("api/accounts", new StringContent(modelAsJson,Encoding.UTF8,"application/json"));
			RegisterResult registerResult=JsonSerializer.Deserialize<RegisterResult>(await result.Content.ReadAsStringAsync());
			return registerResult;
		}

		public async Task<LoginResult> Login(LoginModel loginModel) {
			var loginAsJson = JsonSerializer.Serialize(loginModel);
			var response = await _httpClient.PostAsync("api/Login", new StringContent(loginAsJson, Encoding.UTF8, "application/json"));
			var loginResult = JsonSerializer.Deserialize<LoginResult>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (!response.IsSuccessStatusCode) {
				return loginResult;
			}

			await _localStorage.SetItemAsync("authToken", loginResult.Token);
			((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginModel.Email);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

			return loginResult;
		}

		public async Task Logout() {
			await _localStorage.RemoveItemAsync("authToken");
			((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
			_httpClient.DefaultRequestHeaders.Authorization = null;
		}
	}
}
