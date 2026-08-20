using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IWorkOrderChangeStatusOperation
{
    List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(List<ProductionOrderChangeStatusExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level, string logId = null);
}
