using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessLayer;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Work Order operations targeting the Work Order Microservice.
/// </summary>
public class WorkOrderChangeStatusOperationProxy : BaseHttpProxy, IWorkOrderChangeStatusOperation
{
    public WorkOrderChangeStatusOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(
        List<ProductionOrderChangeStatusExternal> workOrderList, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level,
        string logId = null)
    {
        // Endpoint: WorkOrder/ChangeStatus/Bulk/{validate}/{level}
        string endpoint = $"WorkOrderChangeStatus/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list with logId in a wrapper if needed, but for now just updating signature
        // If the API expects logId, it should be passed here.
        return PostAsync<List<WorkOrderResponse>>(endpoint, new { Data = workOrderList, LogId = logId }).GetAwaiter().GetResult();
    }
}
