using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Supply operations.
/// </summary>
public class SupplyKafkaProxy : BaseKafkaProxy, ISupplyOperation
{
    public SupplyKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<SupplyKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:SUPPLY",     // appsettings key
               "shopfloor-supply-sync")    // fallback topic
    { }

    public async Task<List<ResponseData>> ListUpdateSupply(
        List<SupplyExternal> SupplyList, 
        List<SupplyExternal> SupplyListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.SUPPLY_SERVICE,
            "ListUpdateSupply",
            systemOperator,
            new
            {
                Data         = SupplyList,
                OriginalData = SupplyListOriginal,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return [result];
    }
}
