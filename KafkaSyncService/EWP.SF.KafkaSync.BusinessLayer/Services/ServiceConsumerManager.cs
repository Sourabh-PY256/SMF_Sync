using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.Common.Models.Catalogs;
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
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EWP.SF.KafkaSync.BusinessLayer.Services;

namespace EWP.SF.KafkaSync.BusinessLayer
{
    public class ServiceConsumerManager : IServiceConsumerManager
    {
        private readonly ILogger<ServiceConsumerManager> _logger;
        private readonly IKafkaService _kafkaService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ISyncCompletionRegistry _completionRegistry;
        private readonly IEnumerable<ISyncMessageHandler> _handlers;

        public ServiceConsumerManager(
            ILogger<ServiceConsumerManager> logger,
            IKafkaService kafkaService,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration, ISyncCompletionRegistry completionRegistry,
            IEnumerable<ISyncMessageHandler> handlers)
        {
            _logger = logger;
            _kafkaService = kafkaService;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _completionRegistry = completionRegistry;
            _handlers = handlers;
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

                        var handler = _handlers.FirstOrDefault(h => h.SupportedServices.Contains(message.Service))
                                      ?? _handlers.FirstOrDefault(h => h.SupportedServices.Length == 0);

                        if (handler != null)
                        {
                            await handler.HandleAsync(message, triggerType, processor, scope.ServiceProvider).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.LogWarning("No handler found for service {Service}", message.Service);
                        }
                    }
                }, null, null, groupId);

                string sseTopic = $"producer-syncSSE-{entityType.ToLower()}";
                string sseGroupId = $"{prefix}-{entityType.ToLower()}-sse-group";
                _logger.LogInformation("Starting SSE consumer for topic: {Topic} with GroupId: {GroupId}", sseTopic, sseGroupId);

                System.Threading.Thread.Sleep(100);

                // _kafkaService.StartConsumer(sseTopic, async (key, value) =>
                // {
                //     _logger.LogInformation("Received Kafka SSE trigger message: {Key}", key);

                //     var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value);
                //     if (message == null)
                //     {
                //         _logger.LogWarning("Failed to deserialize Kafka SSE message");
                //         return;
                //     }

                //     using (var scope = _serviceScopeFactory.CreateScope())
                //     {
                //         TriggerType triggerType;
                //         if (!Enum.TryParse<TriggerType>(message.Trigger, out triggerType))
                //         {
                //             triggerType = TriggerType.SmartFactory;
                //         }

                //         var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();

                //         var response = await processor.SyncExecution(
                //             message.ServiceData,
                //             message.ExecutionType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton,
                //             triggerType,
                //             message.User,
                //             message.EntityCode ?? string.Empty,
                //             message.BodyData ?? string.Empty
                //         ).ConfigureAwait(false);

                //         _logger.LogInformation("{Service} SSE ERP Sync execution complete", message.Service);
                //     }
                // }, null, null, sseGroupId);
                

                _kafkaService.StartConsumer(sseTopic, async (key, value) =>
                {
                    _logger.LogInformation("Received Kafka SSE trigger message: {Key}", key);

                    var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value);
                    if (message == null)
                    {
                        _logger.LogWarning("Failed to deserialize Kafka SSE message");
                        return;
                    }

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        TriggerType triggerType;
                        if (!Enum.TryParse<TriggerType>(message.Trigger, out triggerType))
                        {
                            triggerType = TriggerType.SmartFactory;
                        }
                        DataSyncHttpResponse syncResponse = new();
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        bool success = false;

                        try
                        {
                           syncResponse = await processor.SyncExecution(
                                message.ServiceData,
                                message.ExecutionType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton,
                                triggerType,
                                message.User,
                                message.EntityCode ?? string.Empty,
                                message.BodyData ?? string.Empty
                            ).ConfigureAwait(false);

                            success = true;
                            _logger.LogInformation("{Service} SSE ERP Sync execution complete", message.Service);
                        }
                        catch (Exception ex)
                        {
                            success = false;
                            syncResponse.Message = ex.Message;
                            _logger.LogError(ex, "{Service} SSE ERP Sync execution failed", message.Service);
                        }
                        finally
                        {
                            // 👇 Signal producer that consumer is done (success or fail)
                             if (!string.IsNullOrEmpty(message.CorrelationId))
                                {
                                    _completionRegistry.Complete(message.CorrelationId, syncResponse);
                                    _logger.LogInformation("Completed correlationId {CorrelationId}", message.CorrelationId);
                                }
                        }
                    }
                }, null, null, sseGroupId);
            }

            StartInvBinLocation();
            StartInvWarehouse();
            StartInvItemGroup();
            StartProductConsumer();
            StartItemConsumer();
            StartSupplyConsumer();
            StartDemandConsumer();
            StartMeasureUnitConsumer();
            StartPositionConsumer();
            StartWorkOrderChangeStatusConsumer();
            StartStockConsumer();
            StartMaterialConsumer();
            StartEmpEmployee();
            StartDeviceConsumer();
            StartToolingTypeConsumer();
            StartProductionOrderConsumer();
            StartProductReceiptConsumer();
            StartMaterialReturnConsumer();
            StartMoveEntryConsumer();
        }

        /// <summary>
        /// Consumer: Tooling Type Microservice ← Tooling Type
        /// </summary>
        private void StartToolingTypeConsumer()
        {
            string topic = "shopfloor-toolingtype-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-toolingtype-group";
            _logger.LogInformation("Starting Tooling Type consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[ToolingType] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[ToolingType] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<ToolingTypeOperationProxy>();

                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;
                string logId = message.LogId ?? body.LogId?.ToString();

                if (action == "ListUpdateToolType")
                {
                    var list = body.Data.ToObject<List<ToolTypeExternal>>();
                    var original = body.OriginalData?.ToObject<List<ToolTypeExternal>>() ?? new List<ToolTypeExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var listResponse = await httpProxy.ListUpdateToolType(list, original, message.User, validate, level, logId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(logId) && list != null && listResponse != null)
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = i < listResponse.Count ? listResponse[i] : new ResponseData { IsSuccess = false, Message = "No response for this record" };
                            if (resp != null)
                            {
                                await processor.UpdateLogDetailAsync(logId, list[i].ToolingTypeCode, resp).ConfigureAwait(false);
                            }
                        }
                    }
                }

                _logger.LogInformation("[ToolingType] {Action} forwarded to Tooling Type Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Production Order Microservice ← Production Order
        /// </summary>
        private void StartProductionOrderConsumer()
        {
            string topic = "shopfloor-productionorder-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-productionorder-group";
            _logger.LogInformation("Starting Production Order consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[ProductionOrder] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[ProductionOrder] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<ProductionOrderOperationProxy>();

                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;
                string logId = message.LogId ?? body.LogId?.ToString();

                if (action == "ListUpdateProductionOrder")
                {
                    var list = body.Data.ToObject<List<WorkOrderExternal>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;
                    bool isDataSynced = body.IsDataSynced != null && (bool)body.IsDataSynced;

                    var listResponse = await httpProxy.ListUpdateProductionOrder(list, message.User, validate, level, isDataSynced, logId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(logId) && list != null && listResponse != null)
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = i < listResponse.Count ? listResponse[i] : new WorkOrderResponse { IsSuccess = false, Message = "No response for this record" };
                            if (resp != null)
                            {
                                await processor.UpdateLogDetailAsync(logId, list[i].OrderCode, new ResponseData { IsSuccess = resp.IsSuccess, Message = resp.Message }).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else if (action == "ListUpdateWorkOrderChangeStatus")
                {
                    var list = body.Data.ToObject<List<ProductionOrderChangeStatusExternal>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var listResponse = httpProxy.ListUpdateWorkOrderChangeStatus(list, message.User, validate, level, logId);

                    if (!string.IsNullOrEmpty(logId) && list != null && listResponse != null)
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = i < listResponse.Count ? listResponse[i] : new WorkOrderResponse { IsSuccess = false, Message = "No response for this record" };
                            if (resp != null)
                            {
                                await processor.UpdateLogDetailAsync(logId, list[i].OrderCode, new ResponseData { IsSuccess = resp.IsSuccess, Message = resp.Message }).ConfigureAwait(false);
                            }
                        }
                    }
                }

                _logger.LogInformation("[ProductionOrder] {Action} forwarded to Production Order Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Product Receipt Microservice ← Product Receipt
        /// </summary>
        private void StartProductReceiptConsumer()
        {
            string topic = "shopfloor-productreceipt-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-productreceipt-group";
            _logger.LogInformation("Starting Product Receipt consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[ProductReceipt] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[ProductReceipt] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<ProductReceiptOperationProxy>();

                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;
                string logId = message.LogId ?? body.LogId?.ToString();

                if (action == "ListUpdateProductReceipt")
                {
                    var list = body.Data.ToObject<List<ProductReceiptExternal>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var listResponse = await httpProxy.ListUpdateProductReceipt(list, message.User, validate, level, logId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(logId) && list != null && listResponse != null)
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = i < listResponse.Count ? listResponse[i] : new ResponseData { IsSuccess = false, Message = "No response for this record" };
                            if (resp != null)
                            {
                                await processor.UpdateLogDetailAsync(logId, list[i].DocCode, resp).ConfigureAwait(false);
                            }
                        }
                    }
                }

                _logger.LogInformation("[ProductReceipt] {Action} forwarded to Product Receipt Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Move Entry Microservice ← Move Entry
        /// </summary>
        private void StartMoveEntryConsumer()
        {
            // Note: We listen on shopfloor-moveentry-sync for inbound updates from ERP.
            string topic = "shopfloor-moveentry-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-moveentry-group";
            _logger.LogInformation("Starting Move Entry consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[MoveEntry] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[MoveEntry] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<MoveEntryOperationProxy>();

                List<MoveEntryExternal> list = new List<MoveEntryExternal>();
                string logId = message.LogId;
                
                try
                {
                    var parsedData = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                    logId = logId ?? parsedData.LogId?.ToString();
                    
                    if (parsedData.Data != null)
                    {
                        list = parsedData.Data.ToObject<List<MoveEntryExternal>>();
                    }
                    else if (parsedData.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                    {
                        list = parsedData.ToObject<List<MoveEntryExternal>>();
                    }
                    else
                    {
                        list = [parsedData.ToObject<MoveEntryExternal>()];
                    }
                }
                catch
                {
                    try
                    {
                        list = JsonConvert.DeserializeObject<List<MoveEntryExternal>>(message.BodyData);
                    }
                    catch
                    {
                        _logger.LogWarning("[MoveEntry] Could not parse BodyData as List<MoveEntryExternal>. Key: {Key}", key);
                        return;
                    }
                }

                if (list != null && list.Count > 0)
                {
                    bool validate = false;
                    LevelMessage level = LevelMessage.Success;

                    var listResponse = await httpProxy.ListUpdateMoveEntry(list, message.User, validate, level, logId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(logId) && listResponse != null)
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = i < listResponse.Count ? listResponse[i] : new ResponseData { IsSuccess = false, Message = "No response for this record" };
                            if (resp != null)
                            {
                                await processor.UpdateLogDetailAsync(logId, list[i].DocCode, resp).ConfigureAwait(false);
                            }
                        }
                    }
                }

                _logger.LogInformation("[MoveEntry] Message forwarded to Move Entry Microservice");
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Product Microservice ← Product
        /// Forwards ListUpdateProduct and MergeProduct actions to the external Product HTTP proxy.
        /// </summary>
        private void StartProductConsumer()
        {
            string topic = "shopfloor-product-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-product-group";
            _logger.LogInformation("Starting Product consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Product] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Product] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<ProductOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateProduct")
                {
                    var list = body.Data.ToObject<List<Component>>();
                    var original = body.OriginalData?.ToObject<List<Component>>() ?? new List<Component>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    await httpProxy.ListUpdateProduct(list, original, message.User, validate, level).ConfigureAwait(false);
                }
                else if (action == "MergeProduct")
                {
                    ActionDB mode = body.Mode != null ? (ActionDB)Convert.ToInt32((object)body.Mode) : ActionDB.NA;
                    var comp = body.Component.ToObject<Component>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;
                    bool notifyOnce = body.NotifyOnce != null && (bool)body.NotifyOnce;
                    bool isNewVersion = body.isNewVersion != null && (bool)body.isNewVersion;
                    bool isExternalEndpoint = body.isExternalEndpoint != null && (bool)body.isExternalEndpoint;
                    IntegrationSource intSource = body.intSource != null ? (IntegrationSource)Convert.ToInt32((object)body.intSource) : IntegrationSource.SF;

                    await httpProxy.MergeProduct(mode, comp, message.User, validate, level, notifyOnce, isNewVersion, isExternalEndpoint, intSource).ConfigureAwait(false);
                }

                _logger.LogInformation("[Product] {Action} forwarded to Product Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Device Microservice ← Machine
        /// Forwards Machine actions to the external Device HTTP proxy.
        /// </summary>
        private void StartDeviceConsumer()
        {
            string topic = "shopfloor-machine-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-machine-group";
            _logger.LogInformation("Starting Machine consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Machine] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Machine] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<DeviceOperationProxy>();

                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;
                string logId = message.LogId ?? body.LogId?.ToString();

                if (action == "CreateMachine")
                {
                    var machineInfo = body.Data.ToObject<Machine>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;
                    bool notifyOnce = body.NotifyOnce != null && (bool)body.NotifyOnce;

                    var response = await httpProxy.CreateMachine(machineInfo, message.User, validate, "", notifyOnce, logId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(logId) && machineInfo != null && response != null)
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        await processor.UpdateLogDetailAsync(logId, machineInfo.Code, response).ConfigureAwait(false);
                    }
                }
                else if (action == "ListUpdateMachine")
                {
                    var list = body.Data.ToObject<List<MachineExternal>>();
                    var original = body.OriginalData?.ToObject<List<MachineExternal>>() ?? new List<MachineExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var listResponse = await httpProxy.ListUpdateMachine(list, original, message.User, validate, "", logId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(logId) && list != null && listResponse != null)
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = i < listResponse.Count ? listResponse[i] : new ResponseData { IsSuccess = false, Message = "No response for this record" };
                            if (resp != null)
                            {
                                await processor.UpdateLogDetailAsync(logId, list[i].MachineCode, resp).ConfigureAwait(false);
                            }
                        }
                    }
                }

                _logger.LogInformation("[Machine] {Action} forwarded to Machine Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Item Microservice ← Item
        /// Forwards ListUpdateComponentBulk actions to the external Item HTTP proxy.
        /// </summary>
        private void StartItemConsumer()
        {
            string topic = "shopfloor-item-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-item-group";
            _logger.LogInformation("Starting Item consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Item] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Item] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<ItemOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateComponentBulk")
                {
                    var list = body.Data.ToObject<List<ComponentExternal>>();
                    var original = body.OriginalData?.ToObject<List<ComponentExternal>>() ?? new List<ComponentExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateComponentBulk(list, original, message.User, validate, level).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this item" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].ItemCode, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[Item] {Action} forwarded to Item Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Supply Microservice ← Supply
        /// Forwards ListUpdateSupply actions to the external Supply HTTP proxy.
        /// </summary>
        private void StartSupplyConsumer()
        {
            string topic = "shopfloor-supply-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-supply-group";
            _logger.LogInformation("Starting Supply consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Supply] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Supply] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<SupplyOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateSupply")
                {
                    var list = body.Data.ToObject<List<SupplyExternal>>();
                    var original = body.OriginalData?.ToObject<List<SupplyExternal>>() ?? new List<SupplyExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateSupply(list, original, message.User, validate, level).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this supply" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].SupplyNo, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[Supply] {Action} forwarded to Supply Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Demand Microservice ← Demand
        /// Forwards ListUpdateDemandBulk actions to the external Demand HTTP proxy.
        /// </summary>
        private void StartDemandConsumer()
        {
            string topic = "shopfloor-demand-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-demand-group";
            _logger.LogInformation("Starting Demand consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Demand] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Demand] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<DemandOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateDemandBulk")
                {
                    var list = body.Data.ToObject<List<DemandExternal>>();
                    var original = body.OriginalData?.ToObject<List<DemandExternal>>() ?? new List<DemandExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateDemandBulk(list, original, message.User, validate, level).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this demand" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].DemandNo, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[Demand] {Action} forwarded to Demand Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Measure Unit Microservice ← Measure Unit
        /// Forwards ListUpdateUnitMeasure actions to the external Measure Unit HTTP proxy.
        /// </summary>
        private void StartMeasureUnitConsumer()
        {
            string topic = "shopfloor-measureunit-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-measureunit-group";
            _logger.LogInformation("Starting Measure Unit consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[MeasureUnit] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[MeasureUnit] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<MeasureUnitOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateUnitMeasure")
                {
                    var list = body.Data.ToObject<List<MeasureUnitExternal>>();
                    var original = body.OriginalData?.ToObject<List<MeasureUnitExternal>>() ?? new List<MeasureUnitExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateUnitMeasure(list, original, message.User, validate, level).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this measure unit" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].UoMCode, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[MeasureUnit] {Action} forwarded to Measure Unit Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Position Microservice ← Position
        /// Forwards ListUpdateProfile actions to the external Position HTTP proxy.
        /// </summary>
        private void StartPositionConsumer()
        {
            string topic = "shopfloor-position-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-position-group";
            _logger.LogInformation("Starting Position consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Position] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Position] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<PositionOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateProfile")
                {
                    var list = body.Data.ToObject<List<PositionExternal>>();
                    var original = body.OriginalData?.ToObject<List<PositionExternal>>() ?? new List<PositionExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateProfile(list, original, message.User, validate, level).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this position" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].PositionCode, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[Position] {Action} forwarded to Position Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: WorkOrder Microservice ← WorkOrder Change Status
        /// Forwards ListUpdateWorkOrderChangeStatus actions to the external Work Order HTTP proxy.
        /// </summary>
        private void StartWorkOrderChangeStatusConsumer()
        {
            string topic = "shopfloor-workorder-changestatus-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-workorder-changestatus-group";
            _logger.LogInformation("Starting Work Order Change Status consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[WorkOrderChangeStatus] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[WorkOrderChangeStatus] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<WorkOrderChangeStatusOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateWorkOrderChangeStatus")
                {
                    var list = body.Data.ToObject<List<ProductionOrderChangeStatusExternal>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = httpProxy.ListUpdateWorkOrderChangeStatus(list, message.User, validate, level);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new WorkOrderResponse { IsSuccess = false, Message = "No response for this order" };
                            // Note: WorkOrderResponse doesn't inherit from ResponseData directly but has IsSuccess/Message/OrderCode
                            var mappedResp = new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = resp.IsSuccess, Message = resp.Message, Code = resp.Code };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].OrderCode, mappedResp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[WorkOrderChangeStatus] {Action} forwarded to Work Order Microservice", action);
            }, null, null, groupId);
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
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateBinLocation")
                {
                    var list = body.Data.ToObject<List<BinLocationExternal>>();
                    var original = body.OriginalData?.ToObject<List<BinLocationExternal>>() ?? new List<BinLocationExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateBinLocation(list, original, message.User, validate, level, message.LogId).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this item" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].LocationCode, resp).ConfigureAwait(false);
                        }
                    }
                }
                else if (action == "MergeBinLocation")
                {
                    var info = body.Data.ToObject<BinLocation>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    bool once = body.NotifyOnce != null && (bool)body.NotifyOnce;

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
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateWarehouseGroup")
                {
                    var list = body.Data.ToObject<List<WarehouseExternal>>();
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
                    var info = body.Data.ToObject<Warehouse>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    bool once = body.NotifyOnce != null && (bool)body.NotifyOnce;

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
        /// Consumer: Inventory Microservice ← Inventory (ItemGroup)
        /// </summary>
        private void StartInvItemGroup()
        {
            string topic = "inventory-sync-itemgroup";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-inventory-item-group";
            _logger.LogInformation("Starting Inventory consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Inventory] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Inventory] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                //var inventoryOp = scope.ServiceProvider.GetRequiredService<IInventoryOperation>();
                var httpProxy = scope.ServiceProvider.GetRequiredService<InventoryOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateInventoryGroup")
                {
                    var list = body.Data.ToObject<List<InventoryExternal>>();
                    var original = body.OriginalData?.ToObject<List<InventoryExternal>>() ?? new List<InventoryExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateInventoryGroup(list, original, message.User, validate, level, message.LogId).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this item" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].ItemGroupCode, resp).ConfigureAwait(false);
                        }
                    }
                }
                else if (action == "MergeInventory")
                {
                    var info = body.Data.ToObject<InventoryItemGroup>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    bool once = body.NotifyOnce != null && (bool)body.NotifyOnce;

                    var response = await httpProxy.MergeInventory(info, message.User, validate, once, message.LogId).ConfigureAwait(false);

                    if (response != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        await processor.UpdateLogDetailAsync(message.LogId, info.Code, response).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("[Inventory] {Action} forwarded to Inventory Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Employee Microservice ← Employee
        /// </summary>
        private void StartEmpEmployee()
        {
            string topic = "shopfloor-employee-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-employee-group";
            _logger.LogInformation("Starting Employee consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Employee] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Employee] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<EmployeeOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ImportEmployeesAsync")
                {
                    var list = body.Data.ToObject<List<EmployeeExternal>>();
                    var original = body.OriginalData?.ToObject<List<EmployeeExternal>>() ?? new List<EmployeeExternal>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;
                    bool notifyOnce = body.NotifyOnce != null && (bool)body.NotifyOnce;
                    bool isDataSync = body.IsDataSync != null && (bool)body.IsDataSync;

                    var responses = await httpProxy.ImportEmployeesAsync(list, original, message.User, validate, level, notifyOnce, isDataSync, message.LogId).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this item" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].EmployeeCode, resp).ConfigureAwait(false);
                        }
                    }
                }
                else if (action == "MRGEmployee")
                {
                    var list = body.Data.ToObject<List<Employee>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;
                    bool notifyOnce = body.NotifyOnce != null && (bool)body.NotifyOnce;
                    bool isDataSync = body.IsDataSync != null && (bool)body.IsDataSync;

                    var responses = await httpProxy.MRGEmployee(list, message.User, validate, level, notifyOnce, isDataSync, message.LogId).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new EWP.SF.Common.ResponseModels.ResponseData { IsSuccess = false, Message = "No response for this item" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].Code, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[Employee] {Action} forwarded to Employee Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Stock Microservice ← Stock
        /// Forwards ListUpdateStockBulk actions to the external Stock HTTP proxy.
        /// </summary>
        private void StartStockConsumer()
        {
            string topic = "shopfloor-stock-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-stock-group";
            _logger.LogInformation("Starting Stock consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Stock] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Stock] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<StockOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateStockBulk")
                {
                    var list = body.Data.ToObject<List<StockExternal>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var resp = await httpProxy.ListUpdateStockBulk(list, message.User, validate, level).ConfigureAwait(false);

                    if (resp != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].ItemCode, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[Stock] {Action} forwarded to Stock Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Material Microservice ← Material Issue
        /// Forwards ListUpdateMaterialIssue actions to the external Material Transaction HTTP proxy.
        /// </summary>
        private void StartMaterialIssueConsumer()
        {
            string topic = "shopfloor-materialissue-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-materialissue-group";
            _logger.LogInformation("Starting Material Issue consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[MaterialIssue] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[MaterialIssue] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<OrderTransactionMaterialOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateMaterialIssue")
                {
                    var list = body.Data.ToObject<List<MaterialIssueExternal>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateMaterialIssue(list, message.User, validate, level).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new ResponseData { IsSuccess = false, Message = "No response for this record" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].DocCode, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[MaterialIssue] {Action} forwarded to Material Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Material Microservice ← Material Return
        /// Forwards ListUpdateMaterialReturn actions to the external Material Transaction HTTP proxy.
        /// </summary>
        private void StartMaterialReturnConsumer()
        {
            string topic = "shopfloor-materialreturn-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-materialreturn-group";
            _logger.LogInformation("Starting Material Return consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[MaterialReturn] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[MaterialReturn] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<OrderTransactionMaterialOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateMaterialReturn")
                {
                    var list = body.Data.ToObject<List<MaterialReturnExternal>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateMaterialReturn(list, message.User, validate, level, message.LogId).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new ResponseData { IsSuccess = false, Message = "No response for this record" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].DocCode, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[MaterialReturn] {Action} forwarded to Material Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Material Microservice ← Material Scrap
        /// Forwards ListUpdateMaterialScrap actions to the external Material Transaction HTTP proxy.
        /// </summary>
        private void StartMaterialScrapConsumer()
        {
            string topic = "shopfloor-materialscrap-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-materialscrap-group";
            _logger.LogInformation("Starting Material Scrap consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[MaterialScrap] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[MaterialScrap] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<OrderTransactionMaterialOperationProxy>();
                var body = JsonConvert.DeserializeObject<dynamic>(message.BodyData);
                string action = body.Action;

                if (action == "ListUpdateMaterialScrap")
                {
                    var list = body.Data.ToObject<List<MaterialIssueExternal>>();
                    bool validate = body.Validate != null && (bool)body.Validate;
                    LevelMessage level = body.Level != null ? (LevelMessage)body.Level : LevelMessage.Success;

                    var responses = await httpProxy.ListUpdateMaterialScrap(list, message.User, validate, level).ConfigureAwait(false);

                    if (responses != null && !string.IsNullOrEmpty(message.LogId))
                    {
                        var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var resp = responses.Count > i ? responses[i] : new ResponseData { IsSuccess = false, Message = "No response for this record" };
                            await processor.UpdateLogDetailAsync(message.LogId, list[i].DocCode, resp).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("[MaterialScrap] {Action} forwarded to Material Microservice", action);
            }, null, null, groupId);
        }

        /// <summary>
        /// Consumer: Material Microservice ← Material (Issue, Return, Scrap)
        /// Forwards all material transaction actions to the external Material Transaction HTTP proxy.
        /// </summary>
        private void StartMaterialConsumer()
        {
            string topic = "shopfloor-material-sync";
            string prefix = _configuration["KafkaSettings:GroupIdPrefix"] ?? "sf-sync";
            string groupId = $"{prefix}-material-group";
            _logger.LogInformation("Starting Material consumer for topic: {Topic} with GroupId: {GroupId}", topic, groupId);

            // Small delay to let other consumers stabilize
            System.Threading.Thread.Sleep(500);

            _kafkaService.StartConsumer(topic, async (key, value) =>
            {
                _logger.LogInformation("[Material] Received Kafka message. Key: {Key}", key);

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var message = System.Text.Json.JsonSerializer.Deserialize<SyncMessage>(value, options);

                if (message == null || string.IsNullOrEmpty(message.BodyData))
                {
                    _logger.LogWarning("[Material] Message is null or BodyData is empty. Key: {Key}", key);
                    return;
                }

                using var scope = _serviceScopeFactory.CreateScope();
                var httpProxy = scope.ServiceProvider.GetRequiredService<OrderTransactionMaterialOperationProxy>();
                var body = JsonConvert.DeserializeObject<JObject>(message.BodyData);
                if (body == null) return;

                string action = body["Action"]?.ToString();
                bool validate = body["Validate"]?.ToObject<bool>() ?? false;
                LevelMessage level = body["Level"] != null ? (LevelMessage)body["Level"].ToObject<int>() : LevelMessage.Success;

                List<ResponseData> responses = null;
                List<string> rowKeys = [];

                if (action == "ListUpdateMaterialIssue")
                {
                    var list = body["Data"]?.ToObject<List<MaterialIssueExternal>>() ?? [];
                    rowKeys = list.Select(x => x.DocCode).ToList();
                    responses = await httpProxy.ListUpdateMaterialIssue(list, message.User, validate, level).ConfigureAwait(false);
                }
                else if (action == "ListUpdateMaterialReturn")
                {
                    var list = body["Data"]?.ToObject<List<MaterialReturnExternal>>() ?? [];
                    rowKeys = list.Select(x => x.DocCode).ToList();
                    responses = await httpProxy.ListUpdateMaterialReturn(list, message.User, validate, level).ConfigureAwait(false);
                }
                else if (action == "ListUpdateMaterialScrap")
                {
                    var list = body["Data"]?.ToObject<List<MaterialIssueExternal>>() ?? [];
                    rowKeys = list.Select(x => x.DocCode).ToList();
                    responses = await httpProxy.ListUpdateMaterialScrap(list, message.User, validate, level).ConfigureAwait(false);
                }
                else if (action == "Merge")
                {
                    var item = body["Data"]?.ToObject<OrderTransactionMaterial>();
                    if (item != null)
                    {
                        var resp = httpProxy.MergeOrderTransactionMaterial(item, message.User, validate);
                        responses = [resp];
                        rowKeys = [item.DocCode];
                    }
                }

                if (responses != null && !string.IsNullOrEmpty(message.LogId))
                {
                    var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
                    for (int i = 0; i < rowKeys.Count; i++)
                    {
                        var resp = responses.Count > i ? responses[i] : new ResponseData { IsSuccess = false, Message = "No response for this record" };
                        await processor.UpdateLogDetailAsync(message.LogId, rowKeys[i], resp).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("[Material] {Action} forwarded to Material Microservice", action);
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






