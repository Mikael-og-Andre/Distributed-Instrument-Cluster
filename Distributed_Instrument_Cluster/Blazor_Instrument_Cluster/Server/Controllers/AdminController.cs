using Blazor_Instrument_Cluster.Server.Database;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Controllers {

	[Route("api/Admin")]
	[Authorize(Roles = "Admin")]
	[ApiController]
	public class AdminController : ControllerBase {
		private readonly IServiceProvider service;
		private readonly UserManager<IdentityUser> userManager;
		private readonly IdentityDbContext dbContext;

		public AdminController(IServiceProvider service, UserManager<IdentityUser> userManager) {
			this.service = service;
			this.userManager = userManager;
			dbContext = service.GetRequiredService<AppDbContext>();
		}

		[Route("GetAllUsers")]
		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDataModel>))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<ActionResult<IEnumerable<UserDataModel>>> getAllUsers() {
			var users = userManager.Users.ToList();

			IEnumerable<UserDataModel> userList = Array.Empty<UserDataModel>();
			if (users.Any()) {
				foreach (var user in users) {
					var userDataModel = new UserDataModel();
					userDataModel.email = user.Email;
					IList<string> roles = await userManager.GetRolesAsync(user);
					userDataModel.roleList = roles.ToArray();
					userList.Append(userDataModel);
				}
				return Ok(userList);
			} else {
				return NoContent();
			}
		}

		[Route("SetAdmin")]
		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> setAdmin([FromBody] string email) {
			var user = await userManager.FindByEmailAsync(email);
			if (user is null) {
				return BadRequest();
			}

			var userResult = await userManager.AddToRoleAsync(user, "Admin");
			if (!userResult.Succeeded) {
				return BadRequest();
			}

			return Ok();
		}

		[Route("RemoveAdmin")]
		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> removeAdmin([FromBody] string email) {
			var user = await userManager.FindByEmailAsync(email);
			if (user is null) {
				return BadRequest();
			}

			var userResult = await userManager.RemoveFromRoleAsync(user, "Admin");
			if (!userResult.Succeeded) {
				return BadRequest();
			}

			return Ok();
		}

		[Route("SetControl")]
		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> setControl([FromBody] string email) {
			var user = await userManager.FindByEmailAsync(email);
			if (user is null) {
				return BadRequest();
			}

			var userResult = await userManager.AddToRoleAsync(user, "Control");
			if (!userResult.Succeeded) {
				return BadRequest();
			}

			return Ok();
		}

		[Route("RemoveControl")]
		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> removeControl([FromBody] string email) {
			var user = await userManager.FindByEmailAsync(email);
			if (user is null) {
				return BadRequest();
			}

			var userResult = await userManager.RemoveFromRoleAsync(user, "Control");
			if (!userResult.Succeeded) {
				return BadRequest();
			}

			return Ok();
		}
	}
}