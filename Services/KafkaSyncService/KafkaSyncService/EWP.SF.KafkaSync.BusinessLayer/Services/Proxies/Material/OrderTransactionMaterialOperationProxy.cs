using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessLayer;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Material Transaction operations targeting the Material Microservice.
/// </summary>
public class OrderTransactionMaterialOperationProxy : BaseHttpProxy, IOrderTransactionMaterialOperation
{
    public OrderTransactionMaterialOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public ResponseData MergeOrderTransactionMaterial(OrderTransactionMaterial orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true, string logId = null)
    {
        // Directional logic usually handled within the microservice or via specific endpoints
        return PostAsync<ResponseData>("Material/Merge", orderTransactionInfo).GetAwaiter().GetResult();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialIssue(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        string endpoint = $"Material/Issue/Bulk/{Validate.ToString().ToLower()}/{Level}";
        return await PostAsync<List<ResponseData>>(endpoint, OrderTransactionList).ConfigureAwait(false);
    }

    public async Task<List<ResponseData>> ListUpdateMaterialReturn(List<MaterialReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        string endpoint = $"Material/Return/Bulk/{Validate.ToString().ToLower()}/{Level}";
        return await PostAsync<List<ResponseData>>(endpoint, OrderTransactionList).ConfigureAwait(false);
    }

    public async Task<List<ResponseData>> ListUpdateMaterialScrap(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        string endpoint = $"Material/Scrap/Bulk/{Validate.ToString().ToLower()}/{Level}";
        return await PostAsync<List<ResponseData>>(endpoint, OrderTransactionList).ConfigureAwait(false);
    }
}
