using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace EWP.SF.KafkaSync.BusinessLayer
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

                // No need to specify retries and delay - will use values from configuration
                _kafkaService.StartConsumer(topic, async (key, value) =>
{
    _logger.LogInformation("Received Kafka message: {Key}", key);

    var message = JsonSerializer.Deserialize<SyncMessage>(value);
    if (message == null)
    {
        _logger.LogWarning("Failed to deserialize Kafka message");
        return;
    }

    using (var scope = _serviceScopeFactory.CreateScope())
    {
        // Use the centralized validation
        TriggerType triggerType;
        if (!Enum.TryParse<TriggerType>(message.Trigger, out triggerType))
        {
            triggerType = TriggerType.SmartFactory;
        }

        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();

        // ORDER_TRANSACTION_SERVICE doesn't need full SyncExecution - call dedicated method
        if (message.Service == SyncERPEntity.ORDER_TRANSACTION_SERVICE)
        {
            _logger.LogInformation("Processing ORDER_TRANSACTION_SERVICE message");

            var response = await processor.ProcessOrderTransactionService(
                message.BodyData ?? string.Empty,
                message.User ?? new User()
            ).ConfigureAwait(false);

            _logger.LogInformation("ORDER_TRANSACTION_SERVICE processing complete: {Message}", response.Message);
        }
        else
        {
            // For other services, use normal SyncExecution
            var response = await processor.SyncExecution(
                message.ServiceData,
                message.ExecutionType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton,
                Enum.TryParse<TriggerType>(message.Trigger, out  triggerType) ? triggerType : TriggerType.SmartFactory,
                message.User,
                message.EntityCode ?? string.Empty,
                message.BodyData ?? string.Empty
            ).ConfigureAwait(false);
        }

        // Optionally publish execution result to Kafka if needed

    }
});
            }
        }

        /// <summary>
        /// Executes a service manually
        /// </summary>
        // public async Task<DataSyncHttpResponse> SyncERPData(
        //     DataSyncService Data,
        //     TriggerType Trigger,
        //     ServiceExecOrigin ExecOrigin,
        //     User SystemOperator,
        //     string EntityCode,
        //     string BodyData)
        // {
        //     DataSyncHttpResponse response = new();
        //     string serviceType = string.Empty;
        //     try
        //     {
        //         EnableType Enable = EnableType.No;
        //         if (Trigger == TriggerType.Erp)
        //         {
        //             serviceType = "ERP";
        //             Enable = Data.ErpTriggerEnable;
        //         }
        //         else if (Trigger == TriggerType.SmartFactory || Trigger == TriggerType.DataSyncSettings)
        //         {
        //             serviceType = "Smart Factory";
        //             if (ExecOrigin == ServiceExecOrigin.Event)
        //             {
        //                 Enable = Data.SfTriggerEnable;
        //             }
        //             else
        //             {
        //                 Enable = Data.ManualSyncEnable;
        //                 serviceType += " Manual";
        //             }
        //         }

        //         if (Enable == EnableType.Yes)
        //         {
        //             if (Data.Status == ServiceStatus.Active)
        //             {
        //                 // Create a scope for this request
        //                 using (var scope = _serviceScopeFactory.CreateScope())
        //                 {
        //                     // Get the processor service
        //                     var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
        //                     //ContextCache.SetRunningService(Data.Id, true);
        //                     // Execute the service
        //                     response = await processor.SyncExecution(
        //                         Data,
        //                         ExecOrigin,
        //                         Trigger,
        //                         SystemOperator,
        //                         EntityCode,
        //                         BodyData
        //                     ).ConfigureAwait(false);
        //                 }
        //             }
        //             else
        //             {
        //                 response.StatusCode = System.Net.HttpStatusCode.Conflict;
        //                 response.Message = $"{serviceType} {(ContextCache.IsServiceRunning(Data.Id) ? "is being executing" : "status is disabled")}";
        //             }
        //         }
        //         else
        //         {
        //             response.StatusCode = System.Net.HttpStatusCode.Conflict;
        //             response.Message = $"{serviceType} trigger is not enabled";
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
        //         response.Message = $"Service Error: {ex.Message}.";
        //         _logger.LogError(ex, "Service Error: {Message}", ex.Message);
        //         throw;
        //     }
        //     return response;
        // }

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
        // Task<DataSyncHttpResponse> SyncERPData(
        //     DataSyncService Data,
        //     TriggerType Trigger,
        //     ServiceExecOrigin ExecOrigin,
        //     User SystemOperator,
        //     string EntityCode,
        //     string BodyData);

    }
}






