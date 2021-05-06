using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.EventLog;

namespace Blazor_Instrument_Cluster.Server {
	/// <summary>
	/// Entrance point for web server
	/// </summary>
    public class Program {
		/// <summary>
		/// Starts the HostBuilder
		/// </summary>
		/// <param name="args"></param>
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }
		/// <summary>
		/// configure the host builder
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureLogging((context,logging) => {
		            logging.ClearProviders();
		            logging.AddConsole();
	            })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
