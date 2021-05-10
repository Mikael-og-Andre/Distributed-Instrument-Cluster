using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Blazor_Instrument_Cluster.Server.Controllers {
	[ApiController]
	[Route("api/MJPEG/test")]
	[Produces("text/event-stream")]
	public class MJPEG_Stream_Controller : ControllerBase {


		public MJPEG_Stream_Controller() {
			
		}


		//[HttpGet]
		//[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
		//public byte[] getStream() {

		//	var image = System.IO.File.ReadAllBytes("C:\\Users\\Andre\\Downloads\\download.jpg");


		//	Response.ContentType = "image/jpeg";
		//	Response.ContentLength = (long) image.Length;
		//	Response.StatusCode = 200;
		//	Response.Body.WriteAsync(image);

		//	return image;
		//}

		//[HttpGet]
		//public FileStreamResult getStream() {

		//	var image = System.IO.File.ReadAllBytes("C:\\Users\\Andre\\Downloads\\download.jpg");
		//	var temp = new MemoryStream(image);
			

		//	return new FileStreamResult(temp, new MediaTypeHeaderValue("text/plain")) {
		//		FileDownloadName = "ffs.txt"
		//	};


		//}

		[HttpGet]
		public HttpResponseMessage getStream() {
			var image = System.IO.File.ReadAllBytes("C:\\Users\\Andre\\Downloads\\download.jpg");
			var temp = new MemoryStream(image);

			var response = new HttpResponseMessage();

			//response.Content = new ByteArrayContent(image);
			//response.StatusCode = (HttpStatusCode) 200;
			//response.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/x-mixed-replace");
			//new OutputFormatterWriteContext(Response.HttpContext, (temp, Encoder) => )

			response.StatusCode = HttpStatusCode.Moved;
			response.Headers.Location = new Uri("http://localhost:8080");



			return response;
		}




	}
}
