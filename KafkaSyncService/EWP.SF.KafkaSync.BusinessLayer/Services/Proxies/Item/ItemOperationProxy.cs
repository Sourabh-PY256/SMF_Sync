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
     private readonly bool _use2503ForSync;
    public ItemOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
            _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
    }

    public async Task<List<ResponseData>> ListUpdateComponentBulk(
        List<ComponentExternal> itemList, 
        List<ComponentExternal> itemListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        // Endpoint: Item/Bulk/{validate}/{level}
        string endpoint = $"Component/Merge/{Validate.ToString().ToLower()}";
        
        if(_use2503ForSync)
        {
            return await PostAsyncPO<List<ResponseData>>(endpoint, itemList).ConfigureAwait(false);
        }
        else
        {
            return await PostAsync<List<ResponseData>>(endpoint, itemList).ConfigureAwait(false);
        }
        // Sending the list directly as the body
        
    }
}
