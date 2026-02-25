using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Warehouse operations (Inventory Microservice).
/// Implements IWarehouseOperation by publishing messages to the warehouse Kafka topic
/// instead of calling the microservice directly. The dedicated consumer picks them up.
/// </summary>
public class WarehouseKafkaProxy : BaseKafkaProxy, IWarehouseOperation
{
    public WarehouseKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<WarehouseKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:InventoryWarehouse",   // appsettings key
               "inventory-sync-warehouse")                  // fallback topic
    { }

    // ─── Write operations (published to Kafka) ────────────────────────────────

    public async Task<List<ResponseData>> ListUpdateWarehouseGroup(
        List<WarehouseExternal> warehouseGroupList,
        List<WarehouseExternal> warehouseGroupListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level,
        string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.WAREHOUSE_SERVICE,
            "ListUpdateWarehouseGroup",
            systemOperator,
            new
            {
                Data         = warehouseGroupList,
                OriginalData = warehouseGroupListOriginal,
                Validate,
                Level
            },
            logId).ConfigureAwait(false);

        return [result];
    }

    public async Task<ResponseData> MergeWarehouse(
        Warehouse WarehouseInfo,
        User systemOperator,
        bool Validate   = false,
        bool NotifyOnce = true,
        string logId = null)
    {
        return await PublishAsync(
            SyncERPEntity.WAREHOUSE_SERVICE,
            "MergeWarehouse",
            systemOperator,
            new { Data = WarehouseInfo, Validate, NotifyOnce },
            logId
        ).ConfigureAwait(false);
    }

    // ─── Read-only operations (cannot be proxied via Kafka — call DB directly) ─

    public Warehouse GetWarehouse(string Code)
        => throw new NotSupportedException(
            "GetWarehouse cannot be called through the Kafka proxy. " +
            "Inject IWarehouseOperation (non-Kafka) for read operations.");

    public List<Warehouse> ListWarehouse(
        User systemOperator, string WarehouseCode = "", DateTime? DeltaDate = null)
        => throw new NotSupportedException(
            "ListWarehouse cannot be called through the Kafka proxy. " +
            "Inject IWarehouseOperation (non-Kafka) for read operations.");
}
