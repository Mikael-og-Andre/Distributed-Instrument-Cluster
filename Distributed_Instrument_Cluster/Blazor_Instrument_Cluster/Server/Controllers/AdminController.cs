using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Database;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor_Instrument_Cluster.Server.Controllers {
	[Route("api/Admin")]
	[ApiController]
	[Authorize(Roles = "Admin")]
	public class AdminController : ControllerBase {
		private readonly IServiceProvider service;
		private readonly UserManager<IdentityUser> userManager;
		private readonly IdentityDbContext dbContext;

		public AdminController(IServiceProvider service, UserManager<IdentityUser> userManager) {
			this.service = service;
			this.userManager = userManager;
			dbContext = service.GetRequiredService<AppDbContext>();
		}

		[Route("/GetAllUsers")]
		[HttpGet]
		public async Task<IEnumerable<UserDataModel>> getAllUsers() {
			var users = userManager.Users.AsEnumerable();

			List<UserDataModel> userList = new List<UserDataModel>();
			foreach (var user in users) {
				var email = user.Email;
				var iList = await userManager.GetRolesAsync(user);
				var roles = iList.ToList();

				var userDataModel = new UserDataModel();
				userDataModel.email = email;
				userDataModel.roleList = roles;
				userList.Add(userDataModel);
			}

			return userList.AsEnumerable();
		}

		[Route("/SetAdmin")]
		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> setAdmin([FromBody]string email) {

			var user= await userManager.FindByEmailAsync(email);
			if (user is null) {
				return BadRequest();
			}

			var userResult = await userManager.AddToRoleAsync(user,"Admin");
			if (!userResult.Succeeded) {
				return BadRequest();
			}

			return Ok();
		}

		[Route("/RemoveAdmin")]
		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> removeAdmin([FromBody]string email) {

			var user = await userManager.FindByEmailAsync(email);
			if (user is null) {
				return BadRequest();
			}

			var userResult = await userManager.RemoveFromRoleAsync(user,"Admin");
			if (!userResult.Succeeded) {
				return BadRequest();
			}

			return Ok();
		}

		[Route("/SetControl")]
		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> setControl([FromBody]string email) {

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

		[Route("/RemoveControl")]
		[HttpPost]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> removeControl([FromBody]string email) {

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
