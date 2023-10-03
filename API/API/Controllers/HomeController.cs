using API.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Data.Common;
using System.Net;
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
        public IActionResult Index()
        {
            return StatusCode(418);
        }

        [HttpPost]
        public async Task<IActionResult> SendToRabbit(IFormFile File, int? wishedWidth, int? wishedHeight)
        {
            string exchange = "e_files";
            string exchangeType = ExchangeType.Headers;
            string key = "x-filetype";
            string queueName = "q_pdf";
           
            MemoryStream ms = new(new byte[File.Length]);
            await File.CopyToAsync(ms);
            Dictionary<string, object> headers = new Dictionary<string, object>
            {
                { "x-match", "any" },
                { key, File.ContentType }
            };

            if (File.ContentType.Contains("image"))
            {
                using var image = SixLabors.ImageSharp.Image.Load(File.OpenReadStream());
                headers.Add("Width", image.Width);
                headers.Add("Height", image.Height);
                queueName = "q_images";
            }

            _connection.SendUsingHeaders(queueName, exchange, exchangeType, headers, ms.ToArray());
            


            return Ok(File.FileName + ": Hello :" + File.ContentType + ": Hello : " + File.ContentDisposition);

        }
    }
}
