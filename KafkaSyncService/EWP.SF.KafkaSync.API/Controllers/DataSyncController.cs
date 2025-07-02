

using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace EWP.SF.KafkaSync.API;

public  class DataSyncController : BaseController
{
	private readonly ILogger<DataSyncController> _logger;

    public DataSyncController(ILogger<DataSyncController> logger)
    {
        _logger = logger;
    }
	#region DataSync
[HttpPost("DataSyncService/Producer")]
public async Task<ResponseModel> SyncProducer(
    [FromServices] DataSyncServiceManager ServiceManager, 
    [FromServices] IKafkaService kafkaService,
    [FromBody] DataSyncExecuteRequest ServiceRequest)
{
    ResponseModel returnValue = new();
    if (ServiceRequest?.Services == null || !ServiceRequest.Services.Any())
    {
        returnValue.Message = "No services specified.";
        //returnValue.StatusCode = 400;
        return returnValue;
    }

    List<DataSyncExecuteResponse> ServicesResponse = [];
    foreach (string service in ServiceRequest.Services)
    {
        var validation = await ServiceManager.ValidateAndGetService(
            service, TriggerType.Erp, ServiceExecOrigin.KafkaProducer, "GET");

        DataSyncExecuteResponse ServiceResponse = new()
        {
            Service = service,
            IsSuccess = validation.Status == 1,
            Response = validation.Message
        };

        if (validation.Status == 1)
        {
            string topic = $"producer-sync-{service.ToLower()}";
            await kafkaService.ProduceMessageAsync(
                topic, 
                $"producer-{service}-{Guid.NewGuid()}", 
                new SyncMessage { 
                    Service = service, 
                    Trigger = TriggerType.Erp.ToString(),
                    ExecutionType = (int)ServiceExecOrigin.KafkaProducer,
                    EntityCode = ServiceRequest.EntityCode,
                    BodyData = ServiceRequest.BodyData
                }
            ).ConfigureAwait(false);

            _logger.LogInformation("Published Kafka message for service {Service} triggered by producer", service);
        }
        else
        {
            _logger.LogWarning("Service {Service} not executed: {Reason}", service, validation.Message);
        }

        ServicesResponse.Add(ServiceResponse);
    }

    returnValue.Data = ServicesResponse;
    return returnValue;
}
	
	
	#endregion DataSync
}
