using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class ProductionOrderOperationProxy : BaseHttpProxy, IWorkOrderOperation
{
    private readonly ILogger<ProductionOrderOperationProxy> _logger;

    public ProductionOrderOperationProxy(
        HttpClient httpClient, 
        IConfiguration configuration, 
        IAuthenticationService authService, 
        ILogger<ProductionOrderOperationProxy> logger)
        : base(httpClient, configuration, authService)
    {
        _logger = logger;
    }

    public async Task<List<WorkOrderResponse>> ListUpdateProductionOrder(
    List<WorkOrderExternal> workOrderList,
    User systemOperator,
    bool Validate,
    LevelMessage Level,
    bool isDataSynced = false,
    string logId = null)
{
    var request = new
    {
        WorkOrderList = workOrderList,
        SystemOperator = systemOperator,
        IsDataSynced = isDataSynced,
        LogId = logId
    };

    var url = $"WorkOrder/{Validate}/{Level}";

    var response = await PostAsync<List<WorkOrderResponse>>(url, request)
                        .ConfigureAwait(false);

    return response ?? new List<WorkOrderResponse>();
}

    public List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(List<ProductionOrderChangeStatusExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        var request = new
        {
            WorkOrderList = workOrderList,
            SystemOperator = systemOperator,
            Validate = Validate,
            Level = Level,
            LogId = logId
        };

        // This might need a different endpoint, but following the general pattern
        var response = Task.Run(() => PostAsync<List<WorkOrderResponse>>("ProductionOrder/ChangeStatus", request)).Result;
        return response ?? new List<WorkOrderResponse>();
    }

    public Task<double> GetTimezoneOffset(string offSetName = "") => throw new NotSupportedException();
    public Task<List<WorkOrder>> GetWorkOrder(string workOrderId) => throw new NotSupportedException();
    public Task<List<ResponseData>> ListUpdateProductTransfer(List<ProductTransferExternal> transferList, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
    public void AddWorkOrderDatesOffset(WorkOrderExternal order, double offset) => throw new NotSupportedException();
    public Task<string> UpdateWorkOrderComponent(string workOrderId, List<OrderComponent> componentValues, string employeeId, User systemOperator) => throw new NotSupportedException();
    public Task<object> GetMaterialTransactionRequestParams(User systemOperator, CancellationToken cancel = default) => throw new NotSupportedException();
    public List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
}
