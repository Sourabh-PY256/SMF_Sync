using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EWP.SF.KafkaSync.BusinessLayer;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka proxy for Material Transaction operations.
/// Publishes synchronization requests to Kafka instead of direct processing or HTTP forwarding.
/// </summary>
public class OrderTransactionMaterialKafkaProxy : BaseKafkaProxy, IOrderTransactionMaterialOperation
{
    public OrderTransactionMaterialKafkaProxy(
        IKafkaService kafkaService, 
        IConfiguration configuration,
        ILogger<OrderTransactionMaterialKafkaProxy> logger)
        : base(kafkaService, configuration, logger, "KafkaSettings:Topics:MATERIAL", "shopfloor-material-sync")
    {
    }

    public ResponseData MergeOrderTransactionMaterial(OrderTransactionMaterial orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
    {
        return PublishAsync(
            SyncERPEntity.MATERIAL_ISSUE_SERVICE, // Generic base service code
            "Merge",
            systemOperator,
            new
            {
                Data = orderTransactionInfo,
                Validate,
                NotifyOnce
            },
            null).GetAwaiter().GetResult();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialIssue(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.MATERIAL_ISSUE_SERVICE,
            "ListUpdateMaterialIssue",
            systemOperator,
            new
            {
                Data = OrderTransactionList,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return OrderTransactionList.Select(x => new ResponseData { IsSuccess = result.IsSuccess, Message = result.Message, Code = x.DocCode }).ToList();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialReturn(List<MaterialReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.MATERIAL_RETURN_SERVICE,
            "ListUpdateMaterialReturn",
            systemOperator,
            new
            {
                Data = OrderTransactionList,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return OrderTransactionList.Select(x => new ResponseData { IsSuccess = result.IsSuccess, Message = result.Message, Code = x.DocCode }).ToList();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialScrap(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.MATERIAL_SCRAP_SERVICE,
            "ListUpdateMaterialScrap",
            systemOperator,
            new
            {
                Data = OrderTransactionList,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return OrderTransactionList.Select(x => new ResponseData { IsSuccess = result.IsSuccess, Message = result.Message, Code = x.DocCode }).ToList();
    }
}
