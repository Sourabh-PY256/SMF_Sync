using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Product operations targeting the external Product microservice.
/// Used by a dedicated Kafka consumer to forward messages to the real microservice endpoint.
/// </summary>
public class ProductOperationProxy : BaseHttpProxy, IComponentOperation
{
    public ProductOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public async Task<List<ResponseData>> ListUpdateProduct(
        List<ProductExternal> itemList,
        List<ProductExternal> itemListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level)
    {
        // Endpoint: product/{validate}/{level}
        string endpoint = $"product/{Validate.ToString().ToLower()}/{Level}";
        return await PostAsync<List<ResponseData>>(endpoint, itemList).ConfigureAwait(false);
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
        return await PostAsync<ResponseData>("Product/Merge", new
        {
            Mode = mode,
            Component = componentInfo,
            systemOperator,
            Validate,
            Level,
            NotifyOnce,
            isNewVersion,
            isExternalEndpoint,
            intSource
        }).ConfigureAwait(false);
    }

    // ─── Read operations (not available via HTTP proxy) ───────────────────────

    public Component GetComponentByCode(string Code)
        => throw new NotSupportedException("GetComponentByCode is not available through the HTTP proxy.");

    public Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "")
        => throw new NotSupportedException("GetComponents is not available through the HTTP proxy.");

    public Task<List<ProcessEntry>> GetProcessEntryById(string processEntryId, User systemOperator)
        => throw new NotSupportedException("GetProcessEntryById is not available through the HTTP proxy.");

    public Task<List<ProcessEntry>> GetProcessEntry(string productCode, string warehouseId, int? version, int? sequence, User systemOperator)
        => throw new NotSupportedException("GetProcessEntry is not available through the HTTP proxy.");
}
