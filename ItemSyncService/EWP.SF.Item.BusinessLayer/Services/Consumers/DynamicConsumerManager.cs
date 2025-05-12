using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Item.BusinessEntities.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EWP.SF.Item.BusinessLayer.Consumers
{
    /// <summary>
    /// Manages dynamic creation of consumers based on requested services
    /// </summary>
    public class DynamicConsumerManager : IDynamicConsumerManager
    {
        private readonly ILogger<DynamicConsumerManager> _logger;
        private readonly IKafkaService _kafkaService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        
        // Track active consumers by service name
        private readonly ConcurrentDictionary<string, bool> _activeConsumers = new();
        
        public DynamicConsumerManager(
            ILogger<DynamicConsumerManager> logger,
            IKafkaService kafkaService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _kafkaService = kafkaService;
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        /// <summary>
        /// Ensures a consumer exists for the specified service
        /// </summary>
        public void EnsureConsumerExists(string serviceName)
        {
            // If consumer already exists, do nothing
            if (_activeConsumers.ContainsKey(serviceName))
            {
                _logger.LogDebug("Consumer for service {ServiceName} already exists", serviceName);
                return;
            }
            
            // Create a new consumer for this service
            CreateConsumerForService(serviceName);
        }
        
        /// <summary>
        /// Creates a consumer for the specified service
        /// </summary>
        private void CreateConsumerForService(string serviceName)
        {
            try
            {
                _logger.LogInformation("Creating consumer for service: {ServiceName}", serviceName);
                
                // Create a unique topic name for this service
                string topic = $"webhook-{serviceName.ToLower()}";
                
                // Start the consumer
                _kafkaService.StartConsumer(topic, async (key, value) => 
                {
                    try
                    {
                        _logger.LogInformation("Received message for service {ServiceName}: {Key}", 
                            serviceName, key);
                        
                        // Parse the message
                        var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value);
                        if (message == null)
                        {
                            _logger.LogWarning("Failed to deserialize message for service {ServiceName}", 
                                serviceName);
                            return;
                        }
                        
                        // Process the message
                        await ProcessServiceMessageAsync(serviceName, message).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message for service {ServiceName}: {Key}", 
                            serviceName, key);
                    }
                });
                
                // Mark this consumer as active
                _activeConsumers[serviceName] = true;
                
                _logger.LogInformation("Consumer created for service: {ServiceName}", serviceName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create consumer for service: {ServiceName}", serviceName);
            }
        }
        
        /// <summary>
        /// Process a service message
        /// </summary>
        private async Task ProcessServiceMessageAsync(string serviceName, SyncMessage message)
        {
            // Create a scope to resolve scoped services
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    // Get the operations service from the scope
                    var operations = scope.ServiceProvider.GetRequiredService<IDataSyncServiceOperation>();
                    
                    // Get the service data
                    var serviceData = await operations.GetBackgroundService(serviceName).ConfigureAwait(false);
                    if (serviceData == null)
                    {
                        _logger.LogWarning("Service not found: {Service}", serviceName);
                        return;
                    }
                    
                    // Parse trigger type
                    if (!Enum.TryParse<TriggerType>(message.Trigger, out var triggerType))
                    {
                        triggerType = TriggerType.SmartFactory;
                    }
                    
                    // Parse execution origin
                    var execOrigin = (ServiceExecOrigin)message.ExecutionType;
                    
                    // Check if service is already running
                    if (ContextCache.IsServiceRunning(serviceData.Id))
                    {
                        _logger.LogWarning("Service {Service} is already running", serviceName);
                        
                        // Publish execution result to Kafka
                        await PublishResultAsync(
                            serviceData.Id,
                            serviceData.Entity?.Name ?? "Unknown",
                            "Conflict",
                            $"Service {serviceName} is already running",
                            triggerType.ToString(),
                            execOrigin.ToString()
                        ).ConfigureAwait(false);
                        
                        return;
                    }
                    
                    try
                    {
                        // Mark service as running
                        ContextCache.SetRunningService(serviceData.Id, true);
                        
                        // Get the processor service
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        
                        // Execute the service
                        _logger.LogInformation("Executing service {Service} from Kafka message", serviceName);
                        var response = await processor.ExecuteService(
                            serviceData, 
                            execOrigin, 
                            triggerType, 
                            message.User, 
                            message.EntityCode ?? string.Empty, 
                            message.BodyData ?? string.Empty
                        ).ConfigureAwait(false);
                        
                        // Publish execution result to Kafka
                        await PublishResultAsync(
                            serviceData.Id,
                            serviceData.Entity?.Name ?? "Unknown",
                            response.StatusCode.ToString(),
                            response.Message,
                            triggerType.ToString(),
                            execOrigin.ToString()
                        ).ConfigureAwait(false);
                    }
                    finally
                    {
                        // Mark service as not running
                        ContextCache.SetRunningService(serviceData.Id, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message for service {ServiceName}", serviceName);
                }
            }
        }
        
        /// <summary>
        /// Publish results back to Kafka
        /// </summary>
        private async Task PublishResultAsync(string serviceId, string serviceName, string status, string message, string trigger, string origin)
        {
            await _kafkaService.ProduceMessageAsync(
                "sync-results-topic",
                $"result-{serviceId}",
                new {
                    ServiceId = serviceId,
                    ServiceName = serviceName,
                    ExecutionTime = DateTime.UtcNow,
                    Status = status,
                    Message = message,
                    Trigger = trigger,
                    Origin = origin
                }
            ).ConfigureAwait(false);
        }
    }
    
    public interface IDynamicConsumerManager
    {
        void EnsureConsumerExists(string serviceName);
    }
}