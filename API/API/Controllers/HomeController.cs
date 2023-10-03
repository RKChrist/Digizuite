using API.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Data.Common;
using System.Net.Sockets;
using System.Reflection;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {

        private IModel channel;
        private IRabbitConnection _connection; 

        public HomeController(IRabbitConnection connection)
        {
            _connection = connection;
        }

        [HttpGet]
        public IActionResult Index(HttpRequestMessage request)
        {
            Response.StatusCode = StatusCodes.Status418ImATeapot;
            return View("Im a teapot");
        }

        [HttpPost]
        public async Task<IActionResult> SendToRabbit(IFormFile File, int? wishedWidth, int? wishedHeight)
        {
            string exchange = "e_files";
            string exchangeType = ExchangeType.Headers;
            string key = "x-filetype";
            string queueName = "pdf";
            try
            {
                MemoryStream ms = new MemoryStream(new byte[File.Length]);
                await File.CopyToAsync(ms);

                await File.CopyToAsync(ms);
                Dictionary<string, object> headers = new Dictionary<string, object>();
                headers.Add("x-match", "any");
                headers.Add(key, File.ContentType);
                if (File.ContentType.Contains("image"))
                {
                    using var image = SixLabors.ImageSharp.Image.Load(File.OpenReadStream());
                    headers.Add("Width", image.Width);
                    headers.Add("Height", image.Height);
                    queueName = "Images";
                }
                _connection.SendUsingHeaders("Images", exchange, exchangeType, headers, ms.ToArray());
            }
            catch(Exception ex)
            {
                
            }


            return Ok(File.FileName + ": Hello :" + File.ContentType + ": Hello : " + File.ContentDisposition);

        }
    }
}
