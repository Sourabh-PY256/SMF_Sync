using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Demand operations targeting the Demand Microservice.
/// </summary>
public class DemandOperationProxy : BaseHttpProxy, IDemandOperation
{
    public DemandOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public async Task<List<ResponseData>> ListUpdateDemandBulk(
        List<DemandExternal> demandList, 
        List<DemandExternal> demandListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        // Endpoint: Demand/Bulk/{validate}/{level}
        string endpoint = $"Demand/Bulk/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list directly as the body
        return await PostAsync<List<ResponseData>>(endpoint, demandList).ConfigureAwait(false);
    }
}
