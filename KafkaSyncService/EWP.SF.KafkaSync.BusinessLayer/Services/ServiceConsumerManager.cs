using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public ServiceConsumerManager(
            ILogger<ServiceConsumerManager> logger,
            IKafkaService kafkaService,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _kafkaService = kafkaService;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
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
                string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
                string groupId = $"{prefix}-{entityType.ToLower()}-group"; // Unique GroupID per entity with prefix
                _logger.LogInformation("Starting consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

                // Small delay between starting consumers to avoid rebalance storm
                System.Threading.Thread.Sleep(100);

                // Pass the unique GroupId as the 5th argument
                _kafkaService.StartConsumer(topic, async (key, value) => {

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
                }, null, null, groupId);
            }

            // Start dedicated consumers for Inventory Microservice (one per entity)
            StartInvBinLocation();
            StartInvWarehouse();
        }

        /// <summary>
        /// Consumer: Inventory Microservice ← BinLocation
        /// </summary>
        private void StartInvBinLocation()
        {
            string topic = "inventory-sync-binlocation";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-inventory-binlocation-group";
            _logger.LogInformation("Starting BinLocation consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[BinLocation] Received Kafka message. Key: {Key}", key);
                
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);
                
                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[BinLocation] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<BinLocationOperationProxy>();
                var body      = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateBinLocation")
                {
                    var list     = body.Data.ToObject<List<BinLocationExternal>>();
                    var original = body.OriginalData?.ToObject<List<BinLocationExternal>>() ?? new List<BinLocationExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;
                    
                    var responses = await httpProxy.ListUpdateBinLocation(list, original, message.User, validate, level, message.LogId).ConfigureAwait(false);
                    
                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for(int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this item" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].LocationCode, resp).ConfigureAwait(false);
                        }
                    }
                }
                else if (action == "MergeBinLocation")
                {
                    var info      = body.Data.ToObject<BinLocation>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    bool once     = body.NotifyOnce != null && (bool)body.NotifyOnce;
                    
                    var response = await httpProxy.MergeBinLocation(info, message.User, validate, once, message.LogId).ConfigureAwait(false);
                    
                    if (response != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        await processor.UpdateLogDetailAsync(message.LogId, info.LocationCode, response).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("[BinLocation] {Action} forwarded to Inventory Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Inventory Microservice ← Warehouse
        /// Add more actions here as the Inventory Microservice grows.
        /// </summary>
        private void StartInvWarehouse()
        {
            string topic = "inventory-sync-warehouse";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-inventory-warehouse-group";
            _logger.LogInformation("Starting Warehouse consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Warehouse] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Warehouse] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<WarehouseOperationProxy>();
                var body      = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateWarehouseGroup")
                {
                    var list     = body.Data.ToObject<List<WarehouseExternal>>();
                    var original = body.OriginalData?.ToObject<List<WarehouseExternal>>() ?? new List<WarehouseExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;
                    
                    var responses = await httpProxy.ListUpdateWarehouseGroup(list, original, message.User, validate, level, message.LogId).ConfigureAwait(false);
                    
                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this item" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].WarehouseCode, resp).ConfigureAwait(false);
                        }
                    }
                }
                else if (action == "MergeWarehouse")
                {
                    var info      = body.Data.ToObject<Warehouse>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    bool once     = body.NotifyOnce != null && (bool)body.NotifyOnce;
                    
                    var response = await httpProxy.MergeWarehouse(info, message.User, validate, once, message.LogId).ConfigureAwait(false);
                    
                    if (response != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        await processor.UpdateLogDetailAsync(message.LogId, info.WarehouseCode, response).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("[Warehouse] {Action} forwarded to Inventory Microservice", action);
            }, null, null, groupId);
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
    }
}






