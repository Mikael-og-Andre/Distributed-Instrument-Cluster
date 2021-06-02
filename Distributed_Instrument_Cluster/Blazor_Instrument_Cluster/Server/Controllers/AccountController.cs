using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;


namespace Blazor_Instrument_Cluster.Server.Controllers {
	[Route("api/Accounts")]
	[ApiController]
	public class AccountController : ControllerBase {

		//private static UserModel LoggedOutUser = new UserModel { IsAuthenticated = false };

		private readonly UserManager<IdentityUser> _userManager;
		private readonly ILogger<AccountController> logger;

		public AccountController(UserManager<IdentityUser> userManager, ILogger<AccountController> logger) {
			_userManager = userManager;
			this.logger = logger;
		}

		[HttpPost]
		public async Task<IActionResult> Register([FromBody] RegisterModel model) {
			try {
				var newUser = new IdentityUser { UserName = model.Email, Email = model.Email };

				var result = await _userManager.CreateAsync(newUser, model.Password);

				if (!result.Succeeded) {
					var errors = result.Errors.Select(x => x.Description);

					return Ok(new RegisterResult { Successful = false, Errors = errors });

				}
				
				var roleResult = await _userManager.AddToRoleAsync(newUser, "User");


				return Ok(new RegisterResult { Successful = true });
			}
			catch (Exception e) {
				logger.LogInformation("Exception in Account Controller: {0}",e.Message);
				return BadRequest();
			}
		}
	}
}
