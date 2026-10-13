using System.Collections.Concurrent;
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

    public class KafkaService : IKafkaService, IDisposable
    {
        private readonly ProducerConfig _producerConfig;
        private readonly ConsumerConfig _consumerConfig;
        private readonly ILogger<KafkaService> _logger;
        private readonly List<IConsumer<string, string>> _activeConsumers = new();
        private readonly int _defaultMaxRetries;
        private readonly int _defaultRetryDelayMs;
        private readonly bool _autoCreateTopics;
        private readonly CancellationTokenSource _cts = new();

        // Long-lived Kafka clients. Building a producer / admin client / consumer per call costs
        // seconds each (broker metadata + connection setup); RequestAsync used to do all of that on
        // every RPC, which capped the relay at roughly one evaluation per 10-20 s. They are
        // thread-safe (producer, admin) or guarded by _infraLock (metadata consumer).
        private readonly object _infraLock = new();
        private IProducer<string, string> _sharedProducer;
        private IAdminClient _adminClient;
        private IConsumer<string, string> _metaConsumer;
        private readonly ConcurrentDictionary<string, List<int>> _topicPartitions = new();
        private bool _disposed = false;

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
            
            _logger.LogInformation("KafkaService initialized with bootstrap servers: {Servers}, GroupId: {GroupId}, AutoOffsetReset: {OffsetReset}", 
                bootstrapServers, groupId, _consumerConfig.AutoOffsetReset);

            // Start heartbeat monitoring
            StartHeartbeat();
        }

        private void StartHeartbeat()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        // Log heartbeat every 5 minutes
                        await Task.Delay(TimeSpan.FromMinutes(5), _cts.Token);
                        
                        int activeCount = 0;
                        lock (_activeConsumers)
                        {
                            activeCount = _activeConsumers.Count;
                        }
                        
                        _logger.LogInformation("[Kafka Heartbeat] Active consumers: {Count}. Service is healthy.", activeCount);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka heartbeat monitor shutting down.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Kafka heartbeat monitor");
                }
            }, _cts.Token);
        }

        public async Task ProduceMessageAsync<T>(string topic, string key, T message)
        {
            try
            {
                _logger.LogInformation("Producing message to topic {Topic} with key {Key}", topic, key);
                
                var producer = GetSharedProducer();
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

        public void StartConsumer(string topic, Func<string, string, Task> messageHandler, int? maxRetries = null, int? retryDelayMs = null, string groupId = null)
        {
            // Use provided values or fall back to defaults from configuration
            int retries = maxRetries ?? _defaultMaxRetries;
            int delay = retryDelayMs ?? _defaultRetryDelayMs;
            
            _logger.LogInformation("Starting consumer for topic {Topic} with {Retries} retries, {Delay}ms delay, and GroupId: {GroupId}", 
                topic, retries, delay, groupId ?? _consumerConfig.GroupId);
            
            // Create a specific config for this consumer if a GroupId is provided
            var config = _consumerConfig;
            if (!string.IsNullOrEmpty(groupId))
            {
                config = new ConsumerConfig(_consumerConfig) { GroupId = groupId };
            }

            try
            {
                var consumer = new ConsumerBuilder<string, string>(config)
                    .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
                    .Build();

                consumer.Subscribe(topic);
                lock (_activeConsumers)
                {
                    _activeConsumers.Add(consumer);
                }
                _logger.LogInformation("Consumer subscribed to topic {Topic}", topic);

                Task.Run(async () =>
                {
                    try
                    {
                        while (!_cts.Token.IsCancellationRequested)
                        {
                            try
                            {
                                _logger.LogDebug("[{Topic}] Waiting for messages...", topic);
                                var consumeResult = consumer.Consume(_cts.Token);
                                if (consumeResult != null)
                                {
                                    _logger.LogInformation("[{Topic}] Received Kafka message with key {Key} and GroupId: {GroupId}", 
                                        topic, consumeResult.Message.Key, config.GroupId);
                                    
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
                                if (ex.Error.IsFatal)
                                {
                                    _logger.LogCritical("Fatal Kafka consumer error: {Error}", ex.Error.Reason);
                                    break;
                                }
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
                        lock (_activeConsumers)
                        {
                            _activeConsumers.Remove(consumer);
                        }
                        consumer.Dispose();
                    }
                }, _cts.Token);
            }
            catch (KafkaException ex)
            {
                _logger.LogError(ex, "Failed to build or subscribe for topic {Topic}.", topic);
            }
        }

        private async Task ProcessMessageWithRetryAsync(string key, string value, Func<string, string, Task> messageHandler, int maxRetries, int retryDelayMs)
        {
            int retryCount = 0;
            bool processed = false;

            while (!processed && retryCount <= maxRetries && !_cts.Token.IsCancellationRequested)
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
                        
                        // Wait before retrying, respecting global cancellation
                        try
                        {
                            await Task.Delay(delay, _cts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
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
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, new CancellationTokenSource(timeout).Token);
                
                while (!linkedCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(linkedCts.Token);
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
                if (_cts.Token.IsCancellationRequested)
                    _logger.LogInformation("Temporary consumer cancelled due to application shutdown.");
                else
                    _logger.LogInformation("Temporary consumer timed out waiting for message on topic {Topic}", topic);
            }
            finally
            {
                consumer.Close();
            }

            return default;
        }


        public async Task<TResponse> RequestAsync<TRequest, TResponse>(
            string requestTopic,
            string key,
            TRequest request,
            string responseTopic,
            TimeSpan timeout,
            Func<ConsumeResult<string, string>, bool> predicate = null)
        {
            await EnsureTopicExistsAsync(responseTopic).ConfigureAwait(false);

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _consumerConfig.BootstrapServers,
                GroupId = $"{responseTopic}-rpc-{Guid.NewGuid():N}",
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                AutoOffsetReset = AutoOffsetReset.Latest
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka RPC consumer error: {Error}", e.Reason))
                .Build();

            try
            {
                // Capture "now" (the high watermark) for every partition and assign from exactly there,
                // BEFORE producing — so the reply can't land in a window the consumer never reads.
                var metadata = GetTopicPartitions(responseTopic);
                var assignments = new List<TopicPartitionOffset>();
                foreach (var partition in metadata)
                {
                    var tp = new TopicPartition(responseTopic, partition);
                    var watermarks = QueryWatermarks(tp);
                    assignments.Add(new TopicPartitionOffset(tp, watermarks.High));
                }
                consumer.Assign(assignments);

                await ProduceMessageAsync(requestTopic, key, request).ConfigureAwait(false);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
                cts.CancelAfter(timeout);

                while (!cts.IsCancellationRequested)
                {
                    ConsumeResult<string, string> result;
                    try
                    {
                        result = consumer.Consume(cts.Token);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogWarning(ex, "RPC consumer error on {Topic}; retrying", responseTopic);
                        await Task.Delay(200, CancellationToken.None).ConfigureAwait(false);
                        continue;
                    }

                    if (result?.Message != null && (predicate == null || predicate(result)))
                    {
                        return JsonSerializer.Deserialize<TResponse>(result.Message.Value);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RPC wait on {Topic} timed out or was cancelled", responseTopic);
            }
            finally
            {
                try { consumer.Close(); } catch { /* best effort */ }
            }

            return default;
        }

        private IProducer<string, string> GetSharedProducer()
        {
            lock (_infraLock)
            {
                return _sharedProducer ??= new ProducerBuilder<string, string>(_producerConfig).Build();
            }
        }

        private IAdminClient GetAdminClient()
        {
            lock (_infraLock)
            {
                return _adminClient ??= new AdminClientBuilder(new AdminClientConfig
                {
                    BootstrapServers = _consumerConfig.BootstrapServers
                }).Build();
            }
        }

        private WatermarkOffsets QueryWatermarks(TopicPartition topicPartition)
        {
            lock (_infraLock)
            {
                _metaConsumer ??= new ConsumerBuilder<string, string>(new ConsumerConfig
                {
                    BootstrapServers = _consumerConfig.BootstrapServers,
                    GroupId = "kafka-sync-rpc-metadata",
                    EnableAutoCommit = false
                }).Build();

                return _metaConsumer.QueryWatermarkOffsets(topicPartition, TimeSpan.FromSeconds(5));
            }
        }

        private List<int> GetTopicPartitions(string topic) =>
            _topicPartitions.TryGetValue(topic, out var partitions) ? partitions : new List<int> { 0 };

        private async Task EnsureTopicExistsAsync(string topic)
        {
            // Resolved once per topic for the life of the process (topics aren't deleted/re-partitioned
            // under a running service), instead of two admin-client round trips per RPC.
            if (_topicPartitions.ContainsKey(topic))
            {
                return;
            }

            var admin = GetAdminClient();
            var meta = admin.GetMetadata(topic, TimeSpan.FromSeconds(5));
            var existing = meta.Topics.FirstOrDefault(x => x.Topic == topic);
            if (existing != null && existing.Error.Code == ErrorCode.NoError && existing.Partitions.Count > 0)
            {
                _topicPartitions[topic] = existing.Partitions.Select(p => p.PartitionId).ToList();
                return;
            }

            try
            {
                await admin.CreateTopicsAsync(new[]
                {
                    new Confluent.Kafka.Admin.TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 }
                }).ConfigureAwait(false);
                _logger.LogInformation("Created missing response topic {Topic}", topic);
            }
            catch (Confluent.Kafka.Admin.CreateTopicsException ex)
                when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                // Created concurrently by another request/broker auto-create — fine.
            }

            _topicPartitions[topic] = new List<int> { 0 };
        }

        public void Dispose()
        {
            if (_disposed) return;

            _logger.LogInformation("KafkaService is disposing. Cancelling all active consumers...");
            _cts.Cancel();
            
            // Wait a moment for consumers to close gracefully
            Thread.Sleep(500);

            try { _sharedProducer?.Flush(TimeSpan.FromSeconds(5)); } catch { /* best effort */ }
            _sharedProducer?.Dispose();
            _adminClient?.Dispose();
            _metaConsumer?.Dispose();

            _cts.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
