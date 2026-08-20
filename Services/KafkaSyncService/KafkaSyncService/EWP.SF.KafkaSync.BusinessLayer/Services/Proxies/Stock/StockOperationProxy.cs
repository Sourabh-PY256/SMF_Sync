using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Stock operations, forwarding requests to the Stock microservice.
/// </summary>
public class StockOperationProxy : BaseHttpProxy, IStockOperation
{
     private readonly bool _use2503ForSync;
    public StockOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
            _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
    }

    /// <summary>
    /// Forwards bulk stock update requests to the Stock microservice.
    /// </summary>
    public async Task<ResponseData> ListUpdateStockBulk(List<StockExternal> stockList, User systemOperator, bool Validate, LevelMessage Level)
    {
        if(_use2503ForSync)
        {
            string endpoint = $"Stock/{Validate.ToString().ToLower()}/{Level}";

            // Sending the list directly as the body
            return await PostAsyncPO<ResponseData>(endpoint, stockList).ConfigureAwait(false);
        }
        else
        {
            string endpoint = $"Stock/Bulk/{Validate.ToString().ToLower()}/{Level}";

            // Sending the list directly as the body
            return await PostAsync<ResponseData>(endpoint, stockList).ConfigureAwait(false);
        }
        
    }
}
