using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Item operations (Item Microservice).
/// Implements IItemOperation by publishing messages to the item Kafka topic
/// instead of calling the microservice directly.
/// </summary>
public class ItemKafkaProxy : BaseKafkaProxy, IItemOperation
{
    public ItemKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<ItemKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:ITEM",     // appsettings key
               "shopfloor-item-sync")    // fallback topic
    { }

    public async Task<List<ResponseData>> ListUpdateComponentBulk(
        List<ComponentExternal> itemList, 
        List<ComponentExternal> itemListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.ITEM_SERVICE,
            "ListUpdateComponentBulk",
            systemOperator,
            new
            {
                Data         = itemList,
                OriginalData = itemListOriginal,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return [result];
    }
}
