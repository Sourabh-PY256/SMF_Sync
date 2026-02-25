using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Product operations.
/// </summary>
public class ProductKafkaProxy : BaseKafkaProxy, IComponentOperation
{
    public ProductKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<ProductKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:Product",            // appsettings key
               "shopfloor-product-sync")                  // fallback topic
    { }

    public async Task<List<ResponseData>> ListUpdateProduct(
        List<ProductExternal> itemList,
        List<ProductExternal> itemListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.PRODUCT_SERVICE,
            "ListUpdateProduct",
            systemOperator,
            new
            {
                Data = itemList,
                OriginalData = itemListOriginal,
                Validate,
                Level
            }).ConfigureAwait(false);

        return [result];
    }

    public async Task<ResponseData> MergeProduct(
        ActionDB mode,
        Component componentInfo,
        User systemOperator,
        bool Validate = false,
        LevelMessage Level = LevelMessage.Success,
        bool NotifyOnce = true,
        bool isNewVersion = false,
        bool isExternalEndpoint = false,
        IntegrationSource intSource = IntegrationSource.SF)
    {
        return await PublishAsync(
            SyncERPEntity.PRODUCT_SERVICE,
            "MergeProduct",
            systemOperator,
            new
            {
                Mode = mode,
                Component = componentInfo,
                Validate,
                Level,
                NotifyOnce,
                isNewVersion,
                isExternalEndpoint,
                intSource
            }).ConfigureAwait(false);
    }

    // ─── Read operations (not available via Kafka proxy) ─────────────────────

    public Component GetComponentByCode(string Code)
        => throw new NotSupportedException("GetComponentByCode cannot be called through the Kafka proxy.");

    public Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "")
        => throw new NotSupportedException("GetComponents cannot be called through the Kafka proxy.");

    public Task<List<ProcessEntry>> GetProcessEntryById(string processEntryId, User systemOperator)
        => throw new NotSupportedException("GetProcessEntryById cannot be called through the Kafka proxy.");

    public Task<List<ProcessEntry>> GetProcessEntry(string productCode, string warehouseId, int? version, int? sequence, User systemOperator)
        => throw new NotSupportedException("GetProcessEntry cannot be called through the Kafka proxy.");
}
