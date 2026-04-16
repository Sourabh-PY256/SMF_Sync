using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IDataSyncNotifier
{
    Task NotifyStartAsync(DataSyncService serviceData, string entityCode);
    Task NotifyStopAsync(DataSyncService serviceData, string entityCode, DateTime initDate);
}