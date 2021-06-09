using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Blazor_Instrument_Cluster.Server.Database {
	public static class SeedUser {

		public static async Task SeedAdmin(UserManager<IdentityUser> userManager, IConfiguration config) {

			if (userManager.FindByEmailAsync("Admin@admin.com").Result == null) {
				IdentityUser user = new IdentityUser {
					UserName = "Admin@admin.com",
					Email = "Admin@admin.com"
				};

				string hashString;
				var pass = config.GetValue<string>("AdminPassword");
				var crypt = new SHA256Managed();
				var hash = new StringBuilder();
				byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(pass));
				foreach (byte theByte in crypto) {
					hash.Append(theByte.ToString("x2"));
				}
				hashString = hash.ToString();


				IdentityResult result = await userManager.CreateAsync(user, hashString);

				if (result.Succeeded) {
					var roleResult= await userManager.AddToRoleAsync(user, "Admin");
					Console.WriteLine("Result of admin seeding {0}",roleResult.Succeeded);
				}
			}
		}

		public static async Task SeedDevices(AppDbContext context) {
			context.Add(new RemoteDeviceDB() {
				crestronPort = 6969,
				hasCrestron = true,
				ip = "127.0.0.1",
				location = "Location",
				name = "testing",
				type = "Testing",
				videoDeviceNumber = 1,
				videoBasePort = 8080,
			});

			await context.SaveChangesAsync();
		}
	}
}
