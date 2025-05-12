using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.Item.BusinessLayer
{
    public class KafkaService : IKafkaService
    {
        private readonly ProducerConfig _producerConfig;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ILogger<KafkaService> _logger;
        private readonly List<IConsumer<string, string>> _activeConsumers = new();

        public KafkaService(IConfiguration configuration, ILogger<KafkaService> logger)
        {
            _logger = logger;
            
            var bootstrapServers = configuration["KafkaSettings:BootstrapServers"] ?? "localhost:9092";
            var groupId = configuration["KafkaSettings:GroupId"] ?? "ewp-sf-item-group";
            
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                ClientId = "ewp-sf-item-producer",
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 1000
            };
            
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true,
                EnableAutoOffsetStore = false
            };
            
            _logger.LogInformation("KafkaService initialized with bootstrap servers: {Servers}", bootstrapServers);
        }

        public async Task ProduceMessageAsync<T>(string topic, string key, T message)
        {
            try
            {
                _logger.LogInformation("Producing message to topic {Topic} with key {Key}", topic, key);
                
                using var producer = new ProducerBuilder<string, string>(_producerConfig).Build();
                var jsonMessage = JsonSerializer.Serialize(message);
                
                var result = await producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = key,
                    Value = jsonMessage
                });
                
                _logger.LogInformation("Message delivered to {Topic} at partition {Partition}, offset {Offset}", 
                    result.Topic, result.Partition, result.Offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error producing message to topic {Topic}", topic);
                throw;
            }
        }

        public void StartConsumer(string topic, Action<string, string> messageHandler)
        {
            var cts = new CancellationTokenSource();
            var consumer = new ConsumerBuilder<string, string>(_consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
                .Build();
            
            _activeConsumers.Add(consumer);
            
            consumer.Subscribe(topic);
            _logger.LogInformation("Consumer subscribed to topic {Topic}", topic);

            Task.Run(() =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cts.Token);
                            if (consumeResult != null)
                            {
                                _logger.LogInformation("Consumed message from {Topic} with key {Key}", 
                                    consumeResult.Topic, consumeResult.Message.Key);
                                
                                messageHandler(consumeResult.Message.Key, consumeResult.Message.Value);
                                
                                // Store offset after successful processing
                                consumer.StoreOffset(consumeResult);
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, "Error consuming message");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer for topic {Topic} shutting down", topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer for topic {Topic}", topic);
                }
                finally
                {
                    consumer.Close();
                    _activeConsumers.Remove(consumer);
                }
            }, cts.Token);
        }
    }
}
