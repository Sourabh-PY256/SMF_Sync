using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IWorkOrderRepo
{
    ResponseData MergeClockInOutBulk(string Json, User systemOperator, bool Validation);
    Task<List<WorkOrder>> GetWorkOrder(string workOrderId, CancellationToken cancel = default);
    List<ReturnMaterialContext> GetProductReturnContext(string workorderId);

    WorkOrderResponse MergeWorkOrderChangeStatus(WorkOrderChangeStatus workorderInfo, User systemOperator, bool Validation, LevelMessage Level);
    WorkOrder GetWorkOrderByCode(string workOrderCode);


}