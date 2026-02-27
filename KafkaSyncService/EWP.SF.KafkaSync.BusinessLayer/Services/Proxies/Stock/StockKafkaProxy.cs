using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Stock operations.
/// </summary>
public class StockKafkaProxy : BaseKafkaProxy, IStockOperation
{
    public StockKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<StockKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:STOCK",     // appsettings key
               "shopfloor-stock-sync")    // fallback topic
    { }

    public async Task<ResponseData> ListUpdateStockBulk(List<StockExternal> stockList, User systemOperator, bool Validate, LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.STOCK_SERVICE,
            "ListUpdateStockBulk",
            systemOperator,
            new
            {
                Data = stockList,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return result;
    }
}
