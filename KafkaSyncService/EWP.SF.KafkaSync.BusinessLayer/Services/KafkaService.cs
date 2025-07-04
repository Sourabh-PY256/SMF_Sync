using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer
{
    // public interface IKafkaService
    // {
    //     Task ProduceMessageAsync<T>(string topic, string key, T message);
    //     void StartConsumer(string topic, Func<string, string, Task> messageHandler, int maxRetries = 3, int retryDelayMs = 1000);
    //     Task<T> ConsumeMessageAsync<T>(string topic, string groupId, TimeSpan timeout, Func<ConsumeResult<string, string>, bool> predicate = null);
    // }

    public class KafkaService : IKafkaService
    {
        private readonly ProducerConfig _producerConfig;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ILogger<KafkaService> _logger;
        private readonly List<IConsumer<string, string>> _activeConsumers = new();
        private readonly int _defaultMaxRetries;
        private readonly int _defaultRetryDelayMs;
        private readonly bool _autoCreateTopics;

        public KafkaService(IConfiguration configuration, ILogger<KafkaService> logger)
        {
            _logger = logger;
            
            var bootstrapServers = configuration["KafkaSettings:BootstrapServers"] ?? "localhost:9092";
            var groupId = configuration["KafkaSettings:GroupId"] ?? "ewp-sf-item-group";
            
            // Read retry settings from configuration
            if (!int.TryParse(configuration["KafkaSettings:Consumer:MaxRetries"], out _defaultMaxRetries))
            {
                _defaultMaxRetries = 3; // Default if not specified
            }
            
            if (!int.TryParse(configuration["KafkaSettings:Consumer:InitialRetryDelayMs"], out _defaultRetryDelayMs))
            {
                _defaultRetryDelayMs = 1000; // Default if not specified
            }
            
            if (!bool.TryParse(configuration["KafkaSettings:Consumer:AutoCreateTopics"], out _autoCreateTopics))
            {
                _autoCreateTopics = false; // Default if not specified
            }
            
            _logger.LogInformation("Kafka consumer configured with MaxRetries: {MaxRetries}, InitialRetryDelayMs: {RetryDelay}", 
                _defaultMaxRetries, _defaultRetryDelayMs);
            
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
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                // Add this if you want to enable auto topic creation at the client level
                AllowAutoCreateTopics = _autoCreateTopics
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

        public void StartConsumer(string topic, Func<string, string, Task> messageHandler, int? maxRetries = null, int? retryDelayMs = null)
        {
            // Use provided values or fall back to defaults from configuration
            int retries = maxRetries ?? _defaultMaxRetries;
            int delay = retryDelayMs ?? _defaultRetryDelayMs;
            
            _logger.LogInformation("Starting consumer for topic {Topic} with {Retries} retries and {Delay}ms initial delay", 
                topic, retries, delay);
            
            var cts = new CancellationTokenSource();
            var consumer = new ConsumerBuilder<string, string>(_consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
                .Build();
            
            _activeConsumers.Add(consumer);
            
            try
            {
                consumer.Subscribe(topic);
                _logger.LogInformation("Consumer subscribed to topic {Topic}", topic);

                Task.Run(async () =>
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
                                    
                                    bool processedSuccessfully = false;
                                    try
                                    {
                                        // Process message with retry logic and await the async operation
                                        await ProcessMessageWithRetryAsync(
                                            consumeResult.Message.Key, 
                                            consumeResult.Message.Value, 
                                            messageHandler, 
                                            retries, 
                                            delay);
                                        
                                        processedSuccessfully = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Failed to process message after all retries. Message will be reprocessed on next consumer start.");
                                        // Don't store or commit offset - let the message be reprocessed
                                        continue; // Skip to the next message
                                    }
                                    
                                    // Only store and commit offset if processing was successful
                                    if (processedSuccessfully)
                                    {
                                        try
                                        {
                                            // Store offset after successful processing
                                            consumer.StoreOffset(consumeResult);
                                            
                                            // Manually commit the offset
                                            consumer.Commit();
                                            _logger.LogDebug("Successfully committed offset for message with key {Key}", consumeResult.Message.Key);
                                        }
                                        catch (KafkaException ex)
                                        {
                                            _logger.LogWarning(ex, "Error storing/committing offset. Message might be reprocessed.");
                                        }
                                    }
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
            catch (KafkaException ex)
            {
                _logger.LogError(ex, "Failed to subscribe to topic {Topic}. Please ensure the topic exists.", topic);
            }
        }

        private async Task ProcessMessageWithRetryAsync(string key, string value, Func<string, string, Task> messageHandler, int maxRetries, int retryDelayMs)
        {
            int retryCount = 0;
            bool processed = false;

            while (!processed && retryCount <= maxRetries)
            {
                try
                {
                    // Process the message asynchronously
                    await messageHandler(key, value);
                    processed = true;
                    
                    if (retryCount > 0)
                    {
                        _logger.LogInformation("Successfully processed message with key {Key} after {RetryCount} retries", key, retryCount);
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    
                    if (retryCount <= maxRetries)
                    {
                        // Calculate exponential backoff delay
                        int delay = retryDelayMs * (int)Math.Pow(2, retryCount - 1);
                        _logger.LogWarning(ex, "Error processing message with key {Key}. Retry {RetryCount}/{MaxRetries} in {Delay}ms", 
                            key, retryCount, maxRetries, delay);
                        
                        // Wait before retrying
                        await Task.Delay(delay);
                    }
                    else
                    {
                        _logger.LogError(ex, "Failed to process message with key {Key} after {MaxRetries} retries", key, maxRetries);
                        // Re-throw the exception to be handled by the caller
                        throw;
                    }
                }
            }
        }

        // Add a method to consume a message with a timeout and predicate
        public async Task<T> ConsumeMessageAsync<T>(string topic, string groupId, TimeSpan timeout, Func<ConsumeResult<string, string>, bool> predicate = null)
        {
            // Create a consumer config with a unique group ID for this request
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _consumerConfig.BootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
                .Build();

            consumer.Subscribe(topic);
            _logger.LogInformation("Temporary consumer subscribed to topic {Topic} with group {GroupId}", topic, groupId);

            try
            {
                using var cts = new CancellationTokenSource(timeout);
                
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cts.Token);
                        if (consumeResult != null)
                        {
                            _logger.LogInformation("Temporary consumer received message with key {Key}", consumeResult.Message.Key);
                            
                            // If no predicate is provided or the predicate returns true, process the message
                            if (predicate == null || predicate(consumeResult))
                            {
                                // Deserialize the message
                                return JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                            }
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message in temporary consumer");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Temporary consumer timed out waiting for message on topic {Topic}", topic);
            }
            finally
            {
                consumer.Close();
            }

            return default;
        }
    }
}
