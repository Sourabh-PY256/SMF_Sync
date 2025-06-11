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
    WorkOrderResponse MergeWorkOrder(WorkOrder workorderInfo, User systemOperator, bool Validation, LevelMessage Level, ActionDB? mode = null, IntegrationSource intSrc = IntegrationSource.SF);
    bool MergeWorkOrderProcesses(WorkOrder workorderInfo, string processXML, User systemOperator);
    bool MergeWorkOrderComponents(WorkOrder workorderInfo, string componentJson, User systemOperator);
    bool MergeWorkOrderTooling(WorkOrder workorderInfo, string toolingJson, User systemOperator);
    bool MergeWorkOrderSubproducts(WorkOrder workorderInfo, string subproductXML, User systemOperator);
    bool MergeWorkOrderToolValues(WorkOrder workorderInfo, string toolValuesXML, User systemOperator);
    bool MergeWorkOrderLabor(WorkOrder workorderInfo, string JSONData, User systemOperator);



}