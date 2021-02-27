using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace Blazor_Instrument_Cluster.Server.Worker {

    /// <summary>
    /// Background service for accepting incoming connections from controllers
    /// </summary>
    public class CrestronListenerService : BackgroundService {

        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            throw new NotImplementedException();
        }

    }
}