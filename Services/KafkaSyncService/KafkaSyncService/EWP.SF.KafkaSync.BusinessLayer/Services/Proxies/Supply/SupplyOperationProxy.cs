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
    private readonly bool _use2503ForSync;
    public SupplyOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
        _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
    }

    public async Task<List<ResponseData>> ListUpdateSupply(
        List<SupplyExternal> SupplyList,
        List<SupplyExternal> SupplyListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level)
    {
        // Endpoint: Supply/Bulk/{validate}/{level}

        if (_use2503ForSync)
        {
            string endpoint = $"Supply/{Validate.ToString().ToLower()}/{Level}";
            var validList = SupplyList.Where(x => x.Type == "Purchase Order" || x.Type == "MRP").ToList();
            return await PostAsyncPO<List<ResponseData>>(endpoint, validList).ConfigureAwait(false);
        }
        else
        {
            string endpoint = $"Supply/Bulk/{Validate.ToString().ToLower()}/{Level}";
            return await PostAsync<List<ResponseData>>(endpoint, SupplyList).ConfigureAwait(false);
        }
        // Sending the list directly as the body
    }
}
