using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Item.BusinessEntities.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace EWP.SF.Item.BusinessLayer
{
    public class ServiceConsumerManager : IServiceConsumerManager
    {
        private readonly ILogger<ServiceConsumerManager> _logger;
        private readonly IKafkaService _kafkaService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ServiceConsumerManager(
            ILogger<ServiceConsumerManager> logger,
            IKafkaService kafkaService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _kafkaService = kafkaService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Starts the Kafka consumer
        /// </summary>
        public void StartConsumer()
        {
            _logger.LogInformation("Starting ServiceConsumerManager");
            
            // Start Kafka consumers for all sync entities
            foreach (var entityType in GetSyncEntityTypes())
            {
                string topic = $"producer-sync-{entityType.ToLower()}";
                _logger.LogInformation("Starting consumer for topic: {Topic}", topic);
                
                // Start consumer with retry logic (5 retries, starting with 2 second delay)
                _kafkaService.StartConsumer(topic, async (key, value) => 
                {
                    _logger.LogInformation("Received Kafka message: {Key}", key);
                    
                    // Parse the message
                    var message = JsonSerializer.Deserialize<SyncMessage>(value);
                    if (message == null)
                    {
                        _logger.LogWarning("Failed to deserialize Kafka message");
                        return;
                    }
                    
                    // Create a scope to resolve scoped services
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        // Get the operations service from the scope
                        var operations = scope.ServiceProvider.GetRequiredService<IDataSyncServiceOperation>();
                        
                        // Get the service data
                        var serviceData = await operations.GetBackgroundService(message.Service).ConfigureAwait(false);
                        if (serviceData == null)
                        {
                            _logger.LogWarning("Service not found: {Service}", message.Service);
                            return;
                        }
                        
                        // Parse trigger type
                        if (!Enum.TryParse<TriggerType>(message.Trigger, out var triggerType))
                        {
                            triggerType = TriggerType.SmartFactory;
                        }
                        
                        // Parse execution origin
                        var execOrigin = message.ExecutionType == 1 ? 
                            ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton;
                        
                        // Execute the service
                        _logger.LogInformation("Executing service {Service} from Kafka message", message.Service);
                        var response = await SyncERPData(
                            serviceData, 
                            triggerType, 
                            execOrigin, 
                            message.User, 
                            message.EntityCode ?? string.Empty, 
                            message.BodyData ?? string.Empty
                        ).ConfigureAwait(false);
                        
                        // Publish execution result to Kafka if needed
                        // ...
                    }
                }, 5, 2000); // 5 retries with 2 second initial delay (will use exponential backoff)
            }
        }

        /// <summary>
        /// Executes a service manually
        /// </summary>
        public async Task<DataSyncHttpResponse> SyncERPData(
            DataSyncService Data, 
            TriggerType Trigger, 
            ServiceExecOrigin ExecOrigin, 
            User SystemOperator, 
            string EntityCode, 
            string BodyData)
        {
            DataSyncHttpResponse response = new();
            string serviceType = string.Empty;
            try
            {
                EnableType Enable = EnableType.No;
                if (Trigger == TriggerType.Erp)
                {
                    serviceType = "ERP";
                    Enable = Data.ErpTriggerEnable;
                }
                else if (Trigger == TriggerType.SmartFactory || Trigger == TriggerType.DataSyncSettings)
                {
                    serviceType = "Smart Factory";
                    if (ExecOrigin == ServiceExecOrigin.Event)
                    {
                        Enable = Data.SfTriggerEnable;
                    }
                    else
                    {
                        Enable = Data.ManualSyncEnable;
                        serviceType += " Manual";
                    }
                }

                if (Enable == EnableType.Yes)
                {
                    if (Data.Status == ServiceStatus.Active)
                    {
                        // Create a scope for this request
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            // Get the processor service
                            var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                            //ContextCache.SetRunningService(Data.Id, true);
                            // Execute the service
                            response = await processor.SyncExecution(
                                Data,
                                ExecOrigin,
                                Trigger,
                                SystemOperator,
                                EntityCode,
                                BodyData
                            ).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        response.StatusCode = System.Net.HttpStatusCode.Conflict;
                        response.Message = $"{serviceType} {(ContextCache.IsServiceRunning(Data.Id) ? "is being executing" : "status is disabled")}";
                    }
                }
                else
                {
                    response.StatusCode = System.Net.HttpStatusCode.Conflict;
                    response.Message = $"{serviceType} trigger is not enabled";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                response.Message = $"Service Error: {ex.Message}.";
                _logger.LogError(ex, "Service Error: {Message}", ex.Message);
                throw;
            }
            return response;
        }

        /// <summary>
        /// Gets all sync entity types from SyncERPEntity constants
        /// </summary>
        private IEnumerable<string> GetSyncEntityTypes()
        {
            // Get all public constant string fields from SyncERPEntity
            return typeof(SyncERPEntity)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(fi => fi.GetValue(null).ToString())
                .ToList();
        }
    }

    public interface IServiceConsumerManager
    {
        void StartConsumer();
        Task<DataSyncHttpResponse> SyncERPData(
            DataSyncService Data, 
            TriggerType Trigger, 
            ServiceExecOrigin ExecOrigin, 
            User SystemOperator, 
            string EntityCode, 
            string BodyData);
        
    }
}






