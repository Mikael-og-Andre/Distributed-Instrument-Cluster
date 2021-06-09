using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace Blazor_Instrument_Cluster.Server.Database {
	public class AppDbContext : IdentityDbContext {

		private IConfiguration config;
		public DbSet<RemoteDeviceDB> devices { get; set; }

		public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options) {
			config = configuration;
		}

		protected override void OnModelCreating(ModelBuilder builder) {
			base.OnModelCreating(builder);

			string ROLE_ID_USER = "faa9afa0-c382-11eb-8529-0242ac130003";
			string ROLE_ID_ADMIN = "fdf4c924-c382-11eb-8529-0242ac130003";
			string ROLE_ID_CONTROL = "00665ac4-c383-11eb-8529-0242ac130003";

			builder.Entity<IdentityRole>().HasData(new IdentityRole {
				Name = "User",
				NormalizedName = "User",
				Id = ROLE_ID_USER,
				ConcurrencyStamp = ROLE_ID_USER
			});
			builder.Entity<IdentityRole>().HasData(new IdentityRole {
				Name = "Admin",
				NormalizedName = "Admin",
				Id = ROLE_ID_ADMIN,
				ConcurrencyStamp = ROLE_ID_ADMIN
			});
			builder.Entity<IdentityRole>().HasData(new IdentityRole {
				Name = "Control",
				NormalizedName = "Control",
				Id = ROLE_ID_CONTROL,
				ConcurrencyStamp = ROLE_ID_CONTROL
			});

		}
	}
}
