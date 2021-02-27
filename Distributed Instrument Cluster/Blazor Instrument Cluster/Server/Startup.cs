using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.Worker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server {

	/// <summary>
	/// Configuration class
	/// </summary>
	public class Startup {

		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void configureServices(IServiceCollection services) {
			//Use controller
			services.AddControllers();
			services.AddRazorPages();
			services.AddResponseCompression(opts => {
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" });
			});

			//Add Connection tracker
			services.AddSingleton<IRemoteDeviceConnections<string>, RemoteDeviceConnection<string>>();
			services.AddHostedService<VideoListenerService<string>>();
			services.AddSingleton<ISocketHandler,WebsocketConnection>();
			//services.AddHostedService<CrestronListenerService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void configure(IApplicationBuilder app, IWebHostEnvironment env) {
			app.UseResponseCompression();
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			} else {
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			//Websocket setup
			var webSocketOptions = new WebSocketOptions() {
				KeepAliveInterval = TimeSpan.FromSeconds(120),
			};

			app.UseWebSockets(webSocketOptions);

			app.Use(async (context, next) => {
				if (context.Request.Path == "/ws") {
					if (context.WebSockets.IsWebSocketRequest) {
						using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync()) {
							var socketFinishedTcs = new TaskCompletionSource<object>();

							WebsocketConnection websocketConnection =
								(WebsocketConnection) app.ApplicationServices.GetService<ISocketHandler>();
							websocketConnection.addSocket(webSocket,socketFinishedTcs);

							await socketFinishedTcs.Task;
						}
					} else {
						context.Response.StatusCode = 400;
					}
				} else {
					await next();
				}
			});

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints => {
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});
		}
	}
}