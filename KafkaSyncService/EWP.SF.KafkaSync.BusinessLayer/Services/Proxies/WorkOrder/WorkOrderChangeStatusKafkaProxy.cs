using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EWP.SF.KafkaSync.BusinessLayer;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Work Order Change Status operations.
/// </summary>
public class WorkOrderChangeStatusKafkaProxy : BaseKafkaProxy, IWorkOrderChangeStatusOperation
{
    public WorkOrderChangeStatusKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<WorkOrderChangeStatusKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:WORKORDER_CHANGESTATUS",     // appsettings key
               "shopfloor-workorder-changestatus-sync")    // fallback topic
    { }

    public List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(
        List<ProductionOrderChangeStatusExternal> workOrderList, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        var result = PublishAsync(
            SyncERPEntity.PRODUCTION_ORDER_CHANGE_STATUS_SERVICE,
            "ListUpdateWorkOrderChangeStatus",
            systemOperator,
            new
            {
                Data = workOrderList,
                Validate,
                Level
            },
            null).GetAwaiter().GetResult();

        return [new WorkOrderResponse { IsSuccess = result.IsSuccess, Message = result.Message, Code = workOrderList.FirstOrDefault()?.OrderCode }];
    }
}
