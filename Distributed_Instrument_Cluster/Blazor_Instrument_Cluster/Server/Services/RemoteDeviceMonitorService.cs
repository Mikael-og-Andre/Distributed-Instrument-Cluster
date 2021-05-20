using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Blazor_Instrument_Cluster.Server.Services {
	/// <summary>
	/// Not implemented or used, intended for uptime checking
	/// </summary>
	public class RemoteDeviceMonitorService : BackgroundService {

		private IServiceProvider services { get; }
		private IConfiguration configuration { get; }

		private RemoteDeviceManager remoteDeviceManager { get; set; }

		public RemoteDeviceMonitorService(IServiceProvider services, IConfiguration configuration) {
			this.services = services;
			this.configuration = configuration;

		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			Console.WriteLine("Executing empty Remote Monitor");
		}
	}
}
