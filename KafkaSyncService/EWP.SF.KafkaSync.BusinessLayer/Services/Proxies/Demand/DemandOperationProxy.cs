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
    private readonly bool _use2503ForSync;
    public DemandOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
        _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");

    }

    public async Task<List<ResponseData>> ListUpdateDemandBulk(
        List<DemandExternal> demandList,
        List<DemandExternal> demandListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level)
    {
        if (_use2503ForSync)
        {
            string endpoint = $"Demand/{Validate.ToString().ToLower()}/{Level}";
            var validList = demandList.Where(x =>
                !string.IsNullOrWhiteSpace(x.InventoryUoM) &&
                (x.Type == "Sales Order" || x.Type == "Forecast")
            ).ToList();
            return await PostAsyncPO<List<ResponseData>>(endpoint, validList).ConfigureAwait(false);
        }
        else
        {
            // Endpoint: Demand/Bulk/{validate}/{level}
            string endpoint = $"Demand/{Validate.ToString().ToLower()}/{Level}";
            // Sending the list directly as the body
            return await PostAsync<List<ResponseData>>(endpoint, demandList).ConfigureAwait(false);
        }

    }
}
