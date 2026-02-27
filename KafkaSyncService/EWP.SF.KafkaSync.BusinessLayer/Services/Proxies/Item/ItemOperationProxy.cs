using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Item operations targeting the Item Microservice.
/// Used by the Kafka consumer to forward messages to the real microservice endpoint.
/// </summary>
public class ItemOperationProxy : BaseHttpProxy, IItemOperation
{
    public ItemOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public async Task<List<ResponseData>> ListUpdateComponentBulk(
        List<ComponentExternal> itemList, 
        List<ComponentExternal> itemListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        // Endpoint: Item/Bulk/{validate}/{level}
        string endpoint = $"Item/Bulk/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list directly as the body
        return await PostAsync<List<ResponseData>>(endpoint, itemList).ConfigureAwait(false);
    }
}
