using System.Threading.Tasks;
using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Logging;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Handlers
{
    public class DefaultSyncMessageHandler : ISyncMessageHandler
    {
        private readonly ILogger<DefaultSyncMessageHandler> _logger;

        public DefaultSyncMessageHandler(ILogger<DefaultSyncMessageHandler> logger)
        {
            _logger = logger;
        }

        public string[] SupportedServices => System.Array.Empty<string>(); // Empty array indicates fallback

        public async Task<DataSyncHttpResponse> HandleAsync(SyncMessage message, TriggerType triggerType, DataSyncServiceProcessor processor, System.IServiceProvider serviceProvider)
        {
            var response = await processor.SyncExecution(
                message.ServiceData,
                message.ExecutionType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton,
                triggerType,
                message.User,
                message.EntityCode ?? string.Empty,
                message.BodyData ?? string.Empty
            ).ConfigureAwait(false);

            _logger.LogInformation("{Service} ERP Sync execution complete", message.Service);
            return response;
        }
    }
}
