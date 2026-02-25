using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Inventory (ItemGroup) operations (Inventory Microservice).
/// Implements IInventoryOperation by publishing messages to the inventory Kafka topic
/// instead of calling the microservice directly. The dedicated consumer picks them up.
/// </summary>
public class InventoryKafkaProxy : BaseKafkaProxy, IInventoryOperation
{
    public InventoryKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<InventoryKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:ITEMGROUP",     // appsettings key
               "inventory-sync-itemgroup")                    // fallback topic
    { }

  

    public async Task<List<ResponseData>> ListUpdateInventoryGroup(
        List<InventoryExternal> inventoryGroupList,
        List<InventoryExternal> inventoryGroupListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level,
        string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.INVENTORY_SERVICE,
            "ListUpdateInventoryGroup",
            systemOperator,
            new
            {
                Data         = inventoryGroupList,
                OriginalData = inventoryGroupListOriginal,
                Validate,
                Level
            },
            logId).ConfigureAwait(false);

        return [result];
    }

    public async Task<ResponseData> MergeInventory(
        InventoryItemGroup InventoryInfo,
        User systemOperator,
        bool Validate   = false,
        bool NotifyOnce = true,
        string logId = null)
    {
        return await PublishAsync(
            SyncERPEntity.INVENTORY_SERVICE,
            "MergeInventory",
            systemOperator,
            new { Data = InventoryInfo, Validate, NotifyOnce },
            logId
        ).ConfigureAwait(false);
    }

    
    public InventoryItemGroup GetInventory(string Code)
        => throw new NotSupportedException(
            "GetInventory cannot be called through the Kafka proxy. " +
            "Inject IInventoryOperation (non-Kafka) for read operations.");

    public List<InventoryItemGroup> ListInventory(
        User systemOperator, string InventoryCode = "", DateTime? DeltaDate = null)
        => throw new NotSupportedException(
            "ListInventory cannot be called through the Kafka proxy. " +
            "Inject IInventoryOperation (non-Kafka) for read operations.");

    public SaleOrder[] ListSalesOrder(
        string Id, string SalesOrder, string CustomerCode, User systemOperator, 
        bool getAsMasterDetail = false, DateTime? DeltaDate = null)
        => throw new NotSupportedException(
            "ListSalesOrder cannot be called through the Kafka proxy. " +
            "Inject IInventoryOperation (non-Kafka) for read operations.");
}
