using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Database;
using Blazor_Instrument_Cluster.Server.Database.Models;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Microsoft.AspNetCore.Authentication;

namespace Blazor_Instrument_Cluster.Server.Controllers {
	/// <summary>
	/// https://www.youtube.com/watch?v=B9jKf-Dn0Yg&t=46s
	/// youtube tutorial
	/// </summary>
	[Route("/Account")]
	[ApiController]
	public class AccountController : ControllerBase {

		private DICDbContext dbContext { get; set; }

		public AccountController(DICDbContext dbContext) {
			this.dbContext = dbContext;
		}

		[HttpPost("loginAccount")]
		public async Task<ActionResult<Account>> loginAccount(Login login) {

			Account account = dbContext.accounts.FirstOrDefault(acc => acc.email == login.email && acc.passwordHash == login.passwordHash);
			
			if (account != null) {

				//Create claim
				var claim = new Claim(ClaimTypes.Name, account.email);
				//Create claimsPrincipal
				var claimsIdentity = new ClaimsIdentity(new[] {claim},"serverAuth");
				//create claimsPrincipal
				var claimsPrincipel = new ClaimsPrincipal(claimsIdentity);
				//sign in
				await HttpContext.SignInAsync(claimsPrincipel);
			}


			return await Task.FromResult(account);
		}

		[HttpGet("logoutAccount")]
		public async Task<ActionResult<string>> logoutAccount() {

			await HttpContext.SignOutAsync();
			return "Success";
		}

		[HttpGet("getCurrentAccount")]
		public async Task<ActionResult<Account>> getCurrentAccount() {
			throw new NotImplementedException();
		}

	}
}
