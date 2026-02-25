using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for BinLocation operations (Inventory Microservice).
/// Extends BaseKafkaProxy — only the BinLocation-specific action logic lives here.
/// </summary>
public class BinLocationKafkaProxy : BaseKafkaProxy, IBinLocationOperation
{
    public BinLocationKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<BinLocationKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:InventoryBinLocation",   // appsettings key
               "inventory-sync-binlocation")                  // fallback topic
    { }

    public async Task<List<ResponseData>> ListUpdateBinLocation(
        List<BinLocationExternal> binLocationList,
        List<BinLocationExternal> binLocationListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level,
        string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.BIN_LOCATION_SERVICE,
            "ListUpdateBinLocation",
            systemOperator,
            new
            {
                Data         = binLocationList,
                OriginalData = binLocationListOriginal,
                Validate,
                Level
            },
            logId).ConfigureAwait(false);

        return [result];
    }

    public async Task<ResponseData> MergeBinLocation(
        BinLocation BinLocationInfo,
        User systemOperator,
        bool Validate   = false,
        bool NotifyOnce = true,
        string logId = null)
    {
        return await PublishAsync(
            SyncERPEntity.BIN_LOCATION_SERVICE,
            "MergeBinLocation",
            systemOperator,
            new { Data = BinLocationInfo, Validate, NotifyOnce },
            logId
        ).ConfigureAwait(false);
    }
}
