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
    WorkOrderResponse MergeProductionOrder(ProductionOrder workorderInfo, User systemOperator, bool Validation, LevelMessage Level, ActionDB? mode = null, IntegrationSource intSrc = IntegrationSource.SF);
    bool MergeProductionOrderOperations(ProductionOrder workorderInfo, string processXML, User systemOperator);
    bool MergeProductionOrderComponents(ProductionOrder workorderInfo, string componentJson, User systemOperator);
    bool MergeProductionOrderToolingType(ProductionOrder orderInfo, string toolingJson, User systemOperator);
    bool MergeProductionOrderByProducts(ProductionOrder orderInfo, string byProductXML, User systemOperator);
    bool MergeWorkOrderToolValues(WorkOrder workorderInfo, string toolValuesXML, User systemOperator);
    bool MergeProductionOrderLabor(ProductionOrder orderInfo, string JSONData, User systemOperator);
    string UpdateMaterialManual(string transactionId, OrderComponent request, string employeeId, string workOrderId, string externalId, User systemOperator);
    Task<ProductionOrder> GetProductionOrder(string orderCode, CancellationToken cancel = default);



}