using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Pages {
	/// <summary>
	/// Microsoft
	/// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel {
		/// <summary>
		/// Microsoft
		/// </summary>
        public string RequestId { get; set; }
		/// <summary>
		/// Microsoft
		/// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
		/// <summary>
		/// Microsoft
		/// </summary>
        private readonly ILogger<ErrorModel> _logger;
		/// <summary>
		/// Microsoft
		/// </summary>
		/// <param name="logger"></param>
        public ErrorModel(ILogger<ErrorModel> logger) {
            _logger = logger;
        }
		/// <summary>
		/// Microsoft
		/// </summary>
        public void OnGet() {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}
