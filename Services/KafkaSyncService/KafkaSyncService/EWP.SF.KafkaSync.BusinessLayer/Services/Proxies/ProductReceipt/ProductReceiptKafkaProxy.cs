using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class ProductReceiptKafkaProxy : BaseKafkaProxy, IOrderTransactionProductOperation
{
    public ProductReceiptKafkaProxy(
        IKafkaService kafkaService, 
        IConfiguration configuration, 
        ILogger<ProductReceiptKafkaProxy> logger)
        : base(kafkaService, configuration, logger, 
            "KafkaSettings:Topics:PRODUCT_RECEIPT",       // appsettings key
            "shopfloor-productreceipt-sync")              // fallback topic
    {
    }

    public async Task<List<ResponseData>> ListUpdateProductReceipt(List<ProductReceiptExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.PRODUCT_RECEIPT_SERVICE,
            "ListUpdateProductReceipt",
            systemOperator,
            new
            {
                Data = OrderTransactionList,
                Validate = Validate,
                Level = Level
            },
            logId).ConfigureAwait(false);

        return [new ResponseData { IsSuccess = result.IsSuccess, Message = result.Message }];
    }

    public ResponseData MergeOrderTransactionProduct(OrderTransactionProduct orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true) => throw new NotSupportedException();
    public Task<List<ResponseData>> ListUpdateProductReturn(List<ProductReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
}
