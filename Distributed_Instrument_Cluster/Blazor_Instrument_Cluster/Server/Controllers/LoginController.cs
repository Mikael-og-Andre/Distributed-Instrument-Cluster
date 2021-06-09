using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Controllers {

	[Route("api/Login")]
	[ApiController]
	[AllowAnonymous]
	public class LoginController : ControllerBase {
		private readonly IConfiguration _configuration;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly UserManager<IdentityUser> userManager;

		public LoginController(IConfiguration configuration,
			SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) {
			_configuration = configuration;
			_signInManager = signInManager;
			this.userManager = userManager;
		}

		[HttpPost]
		public async Task<IActionResult> Login([FromBody] LoginModel login) {
			var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, false, false);

			if (!result.Succeeded) return BadRequest(new LoginResult { Successful = false, Error = "Username and password are invalid." });

			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Name, login.Email),
			};

			var user = await userManager.FindByEmailAsync(login.Email);

			var roles = await userManager.GetRolesAsync(user);

			foreach (var role in roles) {
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expiry = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

			var token = new JwtSecurityToken(
				_configuration["JwtIssuer"],
				_configuration["JwtAudience"],
				claims.ToArray(),
				expires: expiry,
				signingCredentials: creds
			);

			return Ok(new LoginResult { Successful = true, Token = new JwtSecurityTokenHandler().WriteToken(token) });
		}
	}
}