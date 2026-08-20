using System.Threading.Tasks;
using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Logging;
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Handlers
{
    public class OrderTransactionMessageHandler : ISyncMessageHandler
    {
        private readonly ILogger<OrderTransactionMessageHandler> _logger;

        public OrderTransactionMessageHandler(ILogger<OrderTransactionMessageHandler> logger)
        {
            _logger = logger;
        }

        public string[] SupportedServices => new[] { SyncERPEntity.ORDER_TRANSACTION_SERVICE };

        public async Task<DataSyncHttpResponse> HandleAsync(SyncMessage message, TriggerType triggerType, DataSyncServiceProcessor processor, System.IServiceProvider serviceProvider)
        {
            _logger.LogInformation("Processing ORDER_TRANSACTION_SERVICE message");

            var response = await processor.ProcessOrderTransactionService(
                message.BodyData ?? string.Empty,
                message.User ?? new User()
            ).ConfigureAwait(false);

            _logger.LogInformation("ORDER_TRANSACTION_SERVICE processing complete: {Message}", response.Message);
            return response;
        }
    }
}
