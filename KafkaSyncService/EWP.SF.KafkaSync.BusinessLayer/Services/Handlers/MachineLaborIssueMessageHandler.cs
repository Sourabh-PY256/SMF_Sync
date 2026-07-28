using System.Threading.Tasks;
using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;
using Newtonsoft.Json;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Handlers
{
    public class MachineLaborIssueMessageHandler : ISyncMessageHandler
    {
        private readonly ILogger<MachineLaborIssueMessageHandler> _logger;

        public MachineLaborIssueMessageHandler(ILogger<MachineLaborIssueMessageHandler> logger)
        {
            _logger = logger;
        }

        public string[] SupportedServices => new[] { SyncERPEntity.MACHINE_ISSUE_SERVICE, SyncERPEntity.LABOR_ISSUE_SERVICE };

        public async Task<DataSyncHttpResponse> HandleAsync(SyncMessage message, TriggerType triggerType, DataSyncServiceProcessor processor, System.IServiceProvider serviceProvider)
        {
            if (message.ServiceData.HttpMethod != "POST")
            {
                 return await processor.SyncExecution(
                     message.ServiceData,
                     message.ExecutionType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton,
                     triggerType,
                     message.User,
                     message.EntityCode ?? string.Empty,
                     message.BodyData ?? string.Empty
                 ).ConfigureAwait(false);
            }

            message.ServiceData.HttpMethod = "POST";
            _logger.LogInformation("Processing {Service} message with microservice call", message.Service);

            var workOrderOperation = serviceProvider.GetRequiredService<ProductionOrderOperationProxy>();
            var request = JsonConvert.DeserializeObject<MachineIssueSyncRequest>(message.BodyData);

            var syncData = await workOrderOperation.UpdateMachineIssue(request, message.User).ConfigureAwait(false);

            if (syncData != null)
            {
                var response = await processor.SyncExecution(
                    message.ServiceData,
                    message.ExecutionType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton,
                    triggerType,
                    message.User,
                    message.EntityCode ?? string.Empty,
                    JsonConvert.SerializeObject(syncData)
                ).ConfigureAwait(false);

                _logger.LogInformation("{Service} ERP Sync execution complete via microservice", message.Service);
                return response;
            }
            else
            {
                _logger.LogError("Microservice call for {Service} failed or returned null", message.Service);
                return new DataSyncHttpResponse { StatusCode = System.Net.HttpStatusCode.InternalServerError, Message = "Microservice call failed" };
            }
        }
    }
}
