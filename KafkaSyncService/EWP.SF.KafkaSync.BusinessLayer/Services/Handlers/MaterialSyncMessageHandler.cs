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
using EWP.SF.KafkaSync.BusinessEntities;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Handlers
{
    public class MaterialSyncMessageHandler : ISyncMessageHandler
    {
        private readonly ILogger<MaterialSyncMessageHandler> _logger;
        private readonly bool _use2503ForSync;

        public MaterialSyncMessageHandler(ILogger<MaterialSyncMessageHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
        }

        public string[] SupportedServices => new[] { SyncERPEntity.MATERIAL_ISSUE_SERVICE, SyncERPEntity.MATERIAL_RETURN_SERVICE };

        public async Task<DataSyncHttpResponse> HandleAsync(SyncMessage message, TriggerType triggerType, DataSyncServiceProcessor processor, System.IServiceProvider serviceProvider)
        {
            if (message.Service == SyncERPEntity.MATERIAL_RETURN_SERVICE && message.ServiceData.HttpMethod != "POST")
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

            object request;
            message.ServiceData.HttpMethod = "POST";
            _logger.LogInformation("Processing {Service} message with microservice call", message.Service);

            var workOrderOperation = serviceProvider.GetRequiredService<ProductionOrderOperationProxy>();
            if(_use2503ForSync)
            {
                 request = JsonConvert.DeserializeObject<TransactionRequest>(message.BodyData);
            }
            else
            {
                 request = JsonConvert.DeserializeObject<TransactionMaterialSyncRequest>(message.BodyData);
            }

            var syncData = await workOrderOperation.UpdateWorkOrderComponent(request, message.User).ConfigureAwait(false);

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
