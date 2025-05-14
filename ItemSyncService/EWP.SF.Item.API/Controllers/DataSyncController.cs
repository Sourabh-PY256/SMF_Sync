

using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Item.BusinessEntities.Kafka;
using EWP.SF.Item.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace EWP.SF.Item.API;

public  class DataSyncController : BaseController
{
	private readonly ILogger<DataSyncController> _logger;
	private  IDataSyncServiceOperation _dataSyncService;

    public DataSyncController(ILogger<DataSyncController> logger, IDataSyncServiceOperation dataSyncService)
    {
        _logger = logger;
		_dataSyncService = dataSyncService;
    }
	#region DataSync

	
	[Produces("application/json")]
	[Consumes("application/json")]
	[HttpPost("DataSyncService/Producer")]
	[Tags("Integrators")]
	public async Task<ResponseModel> DataSyncServiceWebhook(
		[FromServices] DataSyncServiceManager ServiceManager, 
		[FromServices] IKafkaService kafkaService,
		[FromBody] DataSyncExecuteRequest ServiceRequest)
	{
		ResponseModel returnValue = new();
		_logger.LogInformation("DataSyncServiceWebhook received request for services: {Services}", 
			string.Join(", ", ServiceRequest.Services));
		
		List<DataSyncExecuteResponse> ServicesResponse = [];
		foreach (string service in ServiceRequest.Services)
		{
			// Ensure a consumer exists for this service
			//consumerManager.EnsureConsumerExists(service);
			
			// Validate if service can be executed
			int runStatus = await ServiceManager.ValidateExecuteService(service, TriggerType.Erp, ServiceExecOrigin.Webhook, "GET").ConfigureAwait(false);
			
			// Determine response message based on status
			string responseMessage = runStatus switch
			{
				1 => "Service execution request accepted",
				2 => "Service is already running",
				0 => "Service is disabled",
				_ => "Service does not exist!",
			};
			
			// Create response object
			DataSyncExecuteResponse ServiceResponse = new()
			{
				Service = service,
				IsSuccess = runStatus > 0,
				Response = responseMessage
			};
			
			// If service can be executed (status 1), publish message to Kafka
			if (runStatus > 0)
			{
				// Create a unique topic name for this service
				string topic = $"producer-sync-{service.ToLower()}";
				
				// Publish message to Kafka
				await kafkaService.ProduceMessageAsync(
					topic, 
					$"producer-{service}-{Guid.NewGuid()}", 
					new SyncMessage { 
						Service = service, 
						Trigger = TriggerType.Erp.ToString(),
						ExecutionType = (int)ServiceExecOrigin.Webhook,
						EntityCode = ServiceRequest.EntityCode,
						BodyData = ServiceRequest.BodyData
					}
				).ConfigureAwait(false);
				
				_logger.LogInformation("Published Kafka message for service {Service} triggered by webhook", service);
			}
			else if (runStatus == 0)
			{
				_logger.LogWarning("Service {Service} is disabled, not executing", service);
			}
			
			ServicesResponse.Add(ServiceResponse);
		}
		
		returnValue.Data = ServicesResponse;
		return returnValue;
	}
	#endregion DataSync
}
