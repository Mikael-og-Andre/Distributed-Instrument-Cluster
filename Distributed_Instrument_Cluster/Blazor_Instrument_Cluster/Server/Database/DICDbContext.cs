using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Database.Models;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Blazor_Instrument_Cluster.Server.Database {
	public class DICDbContext : DbContext {

		private IConfiguration config;

		public DbSet<Models.Account> accounts { get; set; }
		public DbSet<RemoteDeviceDB> devices { get; set; }

		public DICDbContext(DbContextOptions<DICDbContext> options, IConfiguration configuration) : base(options) {
			config = configuration;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {

			string connectionString = this.config.GetConnectionString("DefaultConnection");
			optionsBuilder.UseSqlServer(connectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			
		}
	}
}
