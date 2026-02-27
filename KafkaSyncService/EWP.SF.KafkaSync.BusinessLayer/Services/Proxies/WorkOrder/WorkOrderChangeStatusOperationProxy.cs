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
        LevelMessage Level)
    {
        // Endpoint: WorkOrder/ChangeStatus/Bulk/{validate}/{level}
        string endpoint = $"WorkOrder/ChangeStatus/Bulk/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list directly as the body
        return PostAsync<List<WorkOrderResponse>>(endpoint, workOrderList).GetAwaiter().GetResult();
    }
}
