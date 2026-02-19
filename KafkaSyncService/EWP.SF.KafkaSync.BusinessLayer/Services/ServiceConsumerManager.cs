using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;
using Newtonsoft.Json;

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
                    _logger.LogInformation("Received Kafka trigger message: {Key}", key);

                    var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value);
                    if (message == null)
                    {
                        _logger.LogWarning("Failed to deserialize Kafka message");
                        return;
                    }

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
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
                            // FOR ALL OTHER SERVICES (including BinLocation triggers):
                            // This fetches data from ERP and then calls the Producers (Kafka Proxies)
                            var response = await processor.SyncExecution(
                                message.ServiceData,
                                message.ExecutionType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton,
                                triggerType,
                                message.User,
                                message.EntityCode ?? string.Empty,
                                message.BodyData ?? string.Empty
                            ).ConfigureAwait(false);
                            
                            _logger.LogInformation("{Service} ERP Sync execution complete", message.Service);
                        }
                    }
                });
            }

            // Start dedicated consumer for Inventory Microservice Data Forwarding
            StartInventorySyncConsumer();
        }

        /// <summary>
        /// Dedicated consumer that bridges Kafka data to the Inventory Microservice (HTTP)
        /// </summary>
        private void StartInventorySyncConsumer()
        {
            string topic = "inventory-sync-binlocation";
            _logger.LogInformation("Starting dedicated Inventory Microservice consumer for topic: {Topic}", topic);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("Received Kafka data for Inventory Microservice: {Key}", key);

                // This is the message sent by BinLocationKafkaProxy
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value);
                if (message == null || string.IsNullOrEmpty(message.BodyData)) return;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var httpProxy = scope.ServiceProvider.GetRequiredService<BinLocationOperationProxy>();
                    var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                    string action = body.Action;

                    if (action == "ListUpdateBinLocation")
                    {
                        var list = body.Data.ToObject<List<BinLocationExternal>>();
                        var original = body.OriginalData?.ToObject<List<BinLocationExternal>>() ?? new List<BinLocationExternal>();
                        bool validate = body.Validate != null && (bool)body.Validate;
                        LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                        await httpProxy.ListUpdateBinLocation(list, original, message.User, validate, level).ConfigureAwait(false);
                    }
                    else if (action == "MergeBinLocation")
                    {
                        var info = body.Data.ToObject<BinLocation>();
                        bool validate = body.Validate != null && (bool)body.Validate;
                        bool notifyOnce = body.NotifyOnce != null && (bool)body.NotifyOnce;

                        await httpProxy.MergeBinLocation(info, message.User, validate, notifyOnce).ConfigureAwait(false);
                    }

                    _logger.LogInformation("Inventory Microservice {Action} call complete", action);
                }
            });
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






