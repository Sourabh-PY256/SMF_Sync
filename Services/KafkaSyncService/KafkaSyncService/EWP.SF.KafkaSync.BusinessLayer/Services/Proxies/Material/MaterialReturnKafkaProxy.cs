using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EWP.SF.KafkaSync.BusinessLayer;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Dedicated Kafka proxy for Material Return operations.
/// Publishes synchronization requests specifically for Material Return.
/// </summary>
public class MaterialReturnKafkaProxy : BaseKafkaProxy, IOrderTransactionMaterialOperation
{
    public MaterialReturnKafkaProxy(
        IKafkaService kafkaService, 
        IConfiguration configuration,
        ILogger<MaterialReturnKafkaProxy> logger)
        : base(kafkaService, configuration, logger, "KafkaSettings:Topics:MATERIAL_RETURN", "shopfloor-materialreturn-sync")
    {
    }

    public ResponseData MergeOrderTransactionMaterial(OrderTransactionMaterial orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true, string logId = null)
    {
        return PublishAsync(
            SyncERPEntity.MATERIAL_RETURN_SERVICE,
            "Merge",
            systemOperator,
            new
            {
                Data = orderTransactionInfo,
                Validate,
                NotifyOnce
            },
            logId).GetAwaiter().GetResult();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialIssue(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialReturn(List<MaterialReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
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
            logId).ConfigureAwait(false);

        return OrderTransactionList.Select(x => new ResponseData { IsSuccess = result.IsSuccess, Message = result.Message, Code = x.DocCode }).ToList();
    }

    public async Task<List<ResponseData>> ListUpdateMaterialScrap(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        throw new NotImplementedException();
    }
}
