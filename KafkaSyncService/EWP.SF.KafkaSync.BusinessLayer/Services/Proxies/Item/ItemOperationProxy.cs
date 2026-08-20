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
        string endpoint = $"Component/{Validate.ToString().ToLower()}/{Level}";
        
        ResponseModel response = null;

        if (_use2503ForSync)
        {
            response = await PostAsyncPO<ResponseModel>(endpoint, itemList).ConfigureAwait(false);
        }
        else
        {
            response = await PostAsync<ResponseModel>(endpoint, itemList).ConfigureAwait(false);
        }

        if (response != null && response.IsSuccess && response.Data != null)
        {
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response.Data);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<ResponseData>>(jsonString);
        }

        return new List<ResponseData>();
    }
}
