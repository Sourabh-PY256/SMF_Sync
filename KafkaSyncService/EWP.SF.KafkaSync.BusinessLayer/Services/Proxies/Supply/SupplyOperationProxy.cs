using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Supply operations targeting the Supply Microservice.
/// </summary>
public class SupplyOperationProxy : BaseHttpProxy, ISupplyOperation
{
    public SupplyOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public async Task<List<ResponseData>> ListUpdateSupply(
        List<SupplyExternal> SupplyList, 
        List<SupplyExternal> SupplyListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        // Endpoint: Supply/Bulk/{validate}/{level}
        string endpoint = $"Supply/Bulk/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list directly as the body
        return await PostAsync<List<ResponseData>>(endpoint, SupplyList).ConfigureAwait(false);
    }
}
