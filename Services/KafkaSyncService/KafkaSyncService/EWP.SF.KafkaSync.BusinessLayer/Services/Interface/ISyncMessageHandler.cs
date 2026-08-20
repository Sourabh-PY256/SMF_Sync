using System;
using System.Threading.Tasks;
using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Interface
{
    public interface ISyncMessageHandler
    {
        string[] SupportedServices { get; }
        Task<DataSyncHttpResponse> HandleAsync(SyncMessage message, TriggerType triggerType, DataSyncServiceProcessor processor, IServiceProvider serviceProvider);
    }
}
