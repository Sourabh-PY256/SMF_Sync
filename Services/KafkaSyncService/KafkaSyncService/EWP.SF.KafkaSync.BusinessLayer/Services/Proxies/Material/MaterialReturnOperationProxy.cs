using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessLayer;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Dedicated HTTP proxy for Material Return operations targeting the Material Microservice.
/// </summary>
public class MaterialReturnOperationProxy : BaseHttpProxy, IOrderTransactionMaterialOperation
{
    public MaterialReturnOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public ResponseData MergeOrderTransactionMaterial(OrderTransactionMaterial orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true, string logId = null)
    {
        return PostAsync<ResponseData>("Material/Merge", new { Data = orderTransactionInfo, Validate, NotifyOnce, LogId = logId }).GetAwaiter().GetResult();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialIssue(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        // Not implemented in dedicated Return proxy
        throw new NotImplementedException();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialReturn(List<MaterialReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        string endpoint = $"Material/Return/Bulk/{Validate.ToString().ToLower()}/{Level}";
        return await PostAsync<List<ResponseData>>(endpoint, new { Data = OrderTransactionList, Validate, Level, LogId = logId }).ConfigureAwait(false);
    }

    public async Task<List<ResponseData>> ListUpdateMaterialScrap(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        // Not implemented in dedicated Return proxy
        throw new NotImplementedException();
    }
}
