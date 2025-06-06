using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IWorkOrderOperation
{
    List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    Task<double> GetTimezoneOffset(string offSetName = "");
    Task<List<WorkOrder>> GetWorkOrder(string workOrderId);
    List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(List<ProductionOrderChangeStatusExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level);
    Task<List<ResponseData>> ListUpdateProductTransfer(List<ProductTransferExternal> transferList, User systemOperator, bool Validate, LevelMessage Level);



}