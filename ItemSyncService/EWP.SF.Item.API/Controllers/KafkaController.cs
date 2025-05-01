using EWP.SF.Item.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace EWP.SF.Item.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly IKafkaService _kafkaService;
        private readonly IConfiguration _configuration;

        public KafkaController(IKafkaService kafkaService, IConfiguration configuration)
        {
            _kafkaService = kafkaService;
            _configuration = configuration;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromBody] KafkaMessageDto message)
        {
            var topic = _configuration["KafkaSettings:Topics:ItemSync"];
            await _kafkaService.ProduceMessageAsync(topic, message.Key, message.Value);
            return Ok(new { message = "Message published successfully" });
        }

        [HttpGet("start-consumer")]
        public IActionResult StartConsumer()
        {
            var topic = _configuration["KafkaSettings:Topics:ItemSync"];
            _kafkaService.StartConsumer(topic, (key, value) =>
            {
                // Process the message
                Console.WriteLine($"Received message - Key: {key}, Value: {value}");
            });
            
            return Ok(new { message = "Consumer started" });
        }
    }

    public class KafkaMessageDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}