using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazor_Instrument_Cluster.Client.Auth {
	/// <summary>
	/// https://www.youtube.com/watch?v=2c4p6RGtkps
	/// </summary>
	public class AuthStateProvider : AuthenticationStateProvider {

		private readonly HttpClient httpClient;
		private readonly ILocalStorageService localStorage;
		private readonly AuthenticationState anonymous;

		public AuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage) {
			this.httpClient = httpClient;
			this.localStorage = localStorage;
		}

		public override async Task<AuthenticationState> GetAuthenticationStateAsync() {

			var token = await localStorage.GetItemAsync<string>("authToken");

			if (string.IsNullOrWhiteSpace(token)) {
				return anonymous;
			}

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer",token);

			return new AuthenticationState(
				new ClaimsPrincipal(
					new ClaimsIdentity(
						JwtParser.parseClaimsFromJwt(token),"jwtAuthType")));
		}

		public void notifyUserAuthentication(string token) {
			var authenticatedAccount= new ClaimsPrincipal(new ClaimsIdentity(JwtParser.parseClaimsFromJwt(token), "jwtAuthType"));

			var authState = Task.FromResult(new AuthenticationState(authenticatedAccount));
			NotifyAuthenticationStateChanged(authState);
		}

		public void notifyUserLogout() {
			var authState = Task.FromResult(anonymous);
			NotifyAuthenticationStateChanged(authState);
			httpClient.DefaultRequestHeaders.Authorization = null;
		}



	}

}
