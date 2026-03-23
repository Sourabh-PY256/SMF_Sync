using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IWorkOrderOperation : IWorkOrderChangeStatusOperation
{
    List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    
    Task<List<WorkOrder>> GetWorkOrder(string workOrderId);
    // ListUpdateWorkOrderChangeStatus moved to IWorkOrderChangeStatusOperation
    Task<List<ResponseData>> ListUpdateProductTransfer(List<ProductTransferExternal> transferList, User systemOperator, bool Validate, LevelMessage Level);
    Task<List<WorkOrderResponse>> ListUpdateProductionOrder(List<WorkOrderExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level, bool isDataSynced = false, string logId = null);
    void AddWorkOrderDatesOffset(WorkOrderExternal order, double offset);
    Task<object> UpdateWorkOrderComponent(TransactionMaterialSyncRequest request, User systemOperator);
    Task<object> UpdateWorkOrderProduct(TransactionProductSyncRequest request, User systemOperator);
    Task<object> GetMaterialTransactionRequestParams(User systemOperator, CancellationToken cancel = default);
    Task<object> UpdateExternalID(string externalId, string requestBody, User systemOperator);




}