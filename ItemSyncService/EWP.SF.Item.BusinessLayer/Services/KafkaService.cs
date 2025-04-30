//using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

// namespace EWP.SF.Item.BusinessLayer.Services
// {
//     //public class KafkaService : IKafkaService
//     {
        // private readonly ProducerConfig _producerConfig;
        // private readonly ConsumerConfig _consumerConfig;
        // private readonly IConfiguration _configuration;

        // public KafkaService(IConfiguration configuration)
        // {
        //     _configuration = configuration;
            
        //     _producerConfig = new ProducerConfig
        //     {
        //         BootstrapServers = _configuration["KafkaSettings:BootstrapServers"]
        //     };
            
        //     _consumerConfig = new ConsumerConfig
        //     {
        //         BootstrapServers = _configuration["KafkaSettings:BootstrapServers"],
        //         GroupId = _configuration["KafkaSettings:GroupId"],
        //         AutoOffsetReset = AutoOffsetReset.Earliest
        //     };
        // }

        // public async Task ProduceMessageAsync<T>(string topic, string key, T message)
        // {
        //     using var producer = new ProducerBuilder<string, string>(_producerConfig).Build();
        //     var jsonMessage = JsonSerializer.Serialize(message);
            
        //     await producer.ProduceAsync(topic, new Message<string, string>
        //     {
        //         Key = key,
        //         Value = jsonMessage
        //     });
        // }

        // public void StartConsumer(string topic, Action<string, string> messageHandler)
        // {
        //     var cts = new CancellationTokenSource();
        //     var cancellationToken = cts.Token;

        //     Task.Run(() =>
        //     {
        //         using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
        //         consumer.Subscribe(topic);

        //         try
        //         {
        //             while (!cancellationToken.IsCancellationRequested)
        //             {
        //                 var consumeResult = consumer.Consume(cancellationToken);
        //                 messageHandler(consumeResult.Message.Key, consumeResult.Message.Value);
        //             }
        //         }
        //         catch (OperationCanceledException)
        //         {
        //             consumer.Close();
        //         }
        //     }, cancellationToken);
        // }
    //}
//}