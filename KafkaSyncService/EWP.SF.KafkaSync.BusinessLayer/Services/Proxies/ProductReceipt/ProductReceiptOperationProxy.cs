using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class ProductReceiptOperationProxy : BaseHttpProxy, IOrderTransactionProductOperation
{
    private readonly ILogger<ProductReceiptOperationProxy> _logger;

    public ProductReceiptOperationProxy(
        HttpClient httpClient, 
        IConfiguration configuration, 
        IAuthenticationService authService, 
        ILogger<ProductReceiptOperationProxy> logger)
        : base(httpClient, configuration, authService)
    {
        _logger = logger;
    }

    public async Task<List<ResponseData>> ListUpdateProductReceipt(List<ProductReceiptExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        var request = new
        {
            OrderTransactionList = OrderTransactionList,
            SystemOperator = systemOperator,
            Validate = Validate,
            Level = Level,
            LogId = logId
        };

        var response = await PostAsync<List<ResponseData>>("ProductReceipt/Bulk", request).ConfigureAwait(false);
        return response ?? new List<ResponseData>();
    }

    public ResponseData MergeOrderTransactionProduct(OrderTransactionProduct orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true) => throw new NotSupportedException();
    public Task<List<ResponseData>> ListUpdateProductReturn(List<ProductReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
}
