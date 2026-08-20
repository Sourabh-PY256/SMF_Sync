using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Demand operations.
/// </summary>
public class DemandKafkaProxy : BaseKafkaProxy, IDemandOperation
{
    public DemandKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<DemandKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:DEMAND",     // appsettings key
               "shopfloor-demand-sync")    // fallback topic
    { }

    public async Task<List<ResponseData>> ListUpdateDemandBulk(
        List<DemandExternal> demandList, 
        List<DemandExternal> demandListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.DEMAND_SERVICE,
            "ListUpdateDemandBulk",
            systemOperator,
            new
            {
                Data         = demandList,
                OriginalData = demandListOriginal,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return [result];
    }
}
