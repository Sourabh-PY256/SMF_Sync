
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EWP.SF.KafkaSync.BusinessLayer.Services;

namespace EWP.SF.KafkaSync.API;

public class DataSyncController : BaseController
{
    private readonly ILogger<DataSyncController> _logger;
    private IWorkOrderOperation _workOrderOperation;

    public DataSyncController(ILogger<DataSyncController> logger, IWorkOrderOperation workOrderOperation)
    {
        _logger = logger;
        _workOrderOperation = workOrderOperation;
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
            return returnValue;
        }

        List<DataSyncExecuteResponse> ServicesResponse = [];
        foreach (string service in ServiceRequest.Services)
        {
            var validation = await ServiceManager.ValidateAndGetService(
                service, TriggerType.Erp, ServiceExecOrigin.KafkaProducer, ServiceRequest.MethodType);

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
                    new SyncMessage
                    {
                        Service = service,
                        Trigger = TriggerType.Erp.ToString(),
                        ExecutionType = (int)ServiceExecOrigin.KafkaProducer,
                        EntityCode = ServiceRequest.EntityCode,
                        BodyData = ServiceRequest.BodyData,
                        ServiceData = validation.ServiceData
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


    // [HttpPost("DataSyncService/SyncSSE")]
    // public async Task<ResponseModel> SyncSseProducer(
    // [FromServices] DataSyncServiceManager ServiceManager,
    // [FromServices] IKafkaService kafkaService,
    // [FromServices] ISyncCompletionRegistry completionRegistry,
    // [FromBody] DataSyncExecuteRequest ServiceRequest)
    // {
    //     ResponseModel returnValue = new();
    //     if (ServiceRequest?.Services == null || !ServiceRequest.Services.Any())
    //     {
    //         returnValue.Message = "No services specified.";
    //         return returnValue;
    //     }

    //     List<DataSyncExecuteResponse> ServicesResponse = [];
    //     List<(string service, Task<DataSyncHttpResponse> task)> waitTasks = [];

    //     foreach (string service in ServiceRequest.Services)
    //     {
    //         var validation = await ServiceManager.ValidateAndGetService(
    //             service, TriggerType.Erp, ServiceExecOrigin.KafkaProducer, ServiceRequest.MethodType);

    //         DataSyncExecuteResponse ServiceResponse = new()
    //         {
    //             Service = service,
    //             IsSuccess = validation.Status == 1,
    //             Response = validation.Message
    //         };

    //         if (validation.Status == 1)
    //         {
    //             string correlationId = $"{service}-{Guid.NewGuid()}";
    //             var tcs = completionRegistry.Register(correlationId);

    //             string topic = $"producer-syncSSE-{service.ToLower()}";
    //             await kafkaService.ProduceMessageAsync(
    //                 topic,
    //                 $"producer-{service}-{correlationId}",
    //                 new SyncMessage
    //                 {
    //                     Service = service,
    //                     Trigger = TriggerType.Erp.ToString(),
    //                     ExecutionType = (int)ServiceExecOrigin.KafkaProducer,
    //                     EntityCode = ServiceRequest.EntityCode,
    //                     BodyData = ServiceRequest.BodyData,
    //                     ServiceData = validation.ServiceData,
    //                     CorrelationId = correlationId
    //                 }
    //             ).ConfigureAwait(false);

    //             waitTasks.Add((service, tcs.Task.WaitAsync(TimeSpan.FromMinutes(5))));
    //             _logger.LogInformation("Published Kafka SSE message for service {Service} correlationId {CorrelationId}", service, correlationId);
    //         }

    //         ServicesResponse.Add(ServiceResponse);
    //     }



    //     // Wait for ALL consumers to finish before returning
    //     foreach (var (service, task) in waitTasks)
    //     {
    //         try
    //         {
    //             DataSyncHttpResponse httpResponse = await task.ConfigureAwait(false);
    //             var resp = ServicesResponse.First(r => r.Service == service);
    //             resp.IsSuccess = true;
    //             resp.Response = "Service Executed";
    //             resp.ResponseHttp = httpResponse;
    //         }
    //         catch (TimeoutException)
    //         {
    //             var resp = ServicesResponse.First(r => r.Service == service);
    //             resp.IsSuccess = false;
    //             resp.Response = "Sync timed out";
    //             _logger.LogWarning("Service {Service} sync timed out", service);
    //         }
    //     }

    //     returnValue.Data = ServicesResponse;
    //     return returnValue;
    // }


    [HttpPost("DataSyncService/SyncSSE")]
    public async Task<ResponseModel> SyncSseProducer(
    [FromServices] DataSyncServiceManager serviceManager,
    [FromServices] KafkaSyncRequestFactory factory,
    [FromBody] DataSyncExecuteRequest request)
    {
        ResponseModel returnValue = new();

        if (request?.Services == null || !request.Services.Any())
        {
            returnValue.Message = "No services specified.";
            return returnValue;
        }

        var tasks = request.Services.Select(async service =>
        {
            var validation = await serviceManager.ValidateAndGetService(
                service, TriggerType.Erp, ServiceExecOrigin.KafkaProducer, request.MethodType);

            var response = new DataSyncExecuteResponse
            {
                Service = service,
                IsSuccess = validation.Status == 1,
                Response = validation.Message
            };

            if (validation.Status != 1)
                return response;

            string correlationId = $"{service}-{Guid.NewGuid()}";

            var message = new SyncMessage
            {
                Service = service,
                Trigger = TriggerType.Erp.ToString(),
                ExecutionType = (int)ServiceExecOrigin.KafkaProducer,
                EntityCode = request.EntityCode,
                BodyData = request.BodyData,
                ServiceData = validation.ServiceData,
                CorrelationId = correlationId
            };

            try
            {
                var result = await factory.SendAndWaitAsync(
                    service,
                    message,
                    TimeSpan.FromMinutes(5));

                response.IsSuccess = true;
                response.Response = "Service Executed";
                response.ResponseHttp = result;
            }
            catch (TimeoutException)
            {
                response.IsSuccess = false;
                response.Response = "Sync timed out";
            }

            return response;
        });

        var results = await Task.WhenAll(tasks);

        returnValue.Data = results.ToList();
        return returnValue;
    }


    #endregion DataSync
}
