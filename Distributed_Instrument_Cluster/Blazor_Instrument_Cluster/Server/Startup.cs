using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Server.Services;
using Blazor_Instrument_Cluster.Server.Stream;
using Blazor_Instrument_Cluster.Server.WebSockets;
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
using Blazor_Instrument_Cluster.Server.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Blazor_Instrument_Cluster.Server {

	/// <summary>
	/// Class that sets up the services and configurations of the web system
	/// </summary>
	public class Startup {

		/// <summary>
		///
		/// </summary>
		/// <param name="configuration"></param>
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		/// <summary>
		/// Configuration
		/// </summary>
		public IConfiguration Configuration { get; }

		/// <summary>
		/// This method gets called by the runtime. Use this method to add services to the container.
		/// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		/// </summary>
		/// <param name="services"></param>
		public void configureServices(IServiceCollection services) {
			
			services.AddSingleton<RemoteDeviceManager>();
			services.AddSingleton<CrestronWebsocketHandler>();


			//Use controller
			services.AddControllers();

			services.AddEntityFrameworkSqlServer().AddDbContext<DICDbContext>();

			services.AddAuthentication(options => {
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			}).AddCookie();


			services.AddResponseCompression(opts => {
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" });
			});



		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		public void configure(IApplicationBuilder app, IWebHostEnvironment env) {
			app.UseResponseCompression();
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			}
			else {
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			//Websocket setup
			var webSocketOptions = new WebSocketOptions() {
				KeepAliveInterval = TimeSpan.FromSeconds(360),
			};

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();
			app.UseAuthentication();

			app.UseWebSockets(webSocketOptions);
			//Websocket middelware
			app.Use(async (context, next) => {
				if (context.Request.Path == "/crestronControl") {
					if (context.WebSockets.IsWebSocketRequest) {
						using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync()) {
							var socketFinishedTcs = new TaskCompletionSource<object>();

							var crestronWebsocketHandler = (CrestronWebsocketHandler)app.ApplicationServices.GetService<CrestronWebsocketHandler>();
							crestronWebsocketHandler?.startProtocol(webSocket, socketFinishedTcs);

							await socketFinishedTcs.Task;
						}
					}
					else {
						context.Response.StatusCode = 400;
					}
				}
				else {
					await next();
				}
			});

			app.UseRouting();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});
		}
	}
}