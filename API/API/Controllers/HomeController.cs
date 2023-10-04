using API.RabbitMQ;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Data.Common;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
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
            return Ok("Hello World");
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
                { "x-match", "all" }
            };
            Json json = new Json();
            json.Message = Encoding.ASCII.GetString(ms.ToArray());
            
            if (File.ContentType.Contains("image"))
            {
                using var image = SixLabors.ImageSharp.Image.Load(File.OpenReadStream());
                json.Width = image.Width;
                json.Height = image.Height;
                queueName = "q_images";
                headers.Add(key, "image");
            }
            else{
                headers.Add(key, "pdf");
            }

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json));
            _connection.SendUsingHeaders(queueName, exchange, exchangeType, headers, bytes);
            

            return Ok(File.FileName + ": Hello :" + File.ContentType + ": Hello : " + File.ContentDisposition);

        }
    }
    public class Json
    {
        public int Height { get; set; }
         public int Width { get; set; }
        public string Message { get; set; }

    }
}   
