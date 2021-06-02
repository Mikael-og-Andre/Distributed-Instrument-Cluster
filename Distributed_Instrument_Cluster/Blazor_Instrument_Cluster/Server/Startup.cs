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
using System.Text;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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

			services.AddDbContext<AppDbContext>(options => {
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
			});

			services.AddDefaultIdentity<IdentityUser>(options => {
					options.User.RequireUniqueEmail = true;
					options.Password.RequireUppercase = false;
					options.Password.RequireNonAlphanumeric = false;
			}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();

			

			//Use controller
			services.AddControllers();


			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer("JwtBearer", jwtBearerOptions => {
				jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["JwtIssuer"],
                ValidAudience = Configuration["JwtAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"]))
            };
			});


			services.AddResponseCompression(opts => {
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" });
			});

			services.AddSingleton<RemoteDeviceManager>();
			services.AddSingleton<CrestronWebsocketHandler>();

		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		public void configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider service, ILogger<Startup> logger) {

			var context = service.GetService<AppDbContext>();
			//check db
			if (!context.Database.CanConnect()) {
				logger.LogCritical("No Database found");
				//Database does not exist error
				Environment.Exit(1065);
			}

			if (Configuration.GetValue<bool>("DeleteDB")) {
				context.Database.EnsureDeleted();
			}

			context.Database.EnsureCreated();

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
			

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			//Websocket setup
			var webSocketOptions = new WebSocketOptions() {
				KeepAliveInterval = TimeSpan.FromSeconds(360),
			};
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

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});
		}
	}
}