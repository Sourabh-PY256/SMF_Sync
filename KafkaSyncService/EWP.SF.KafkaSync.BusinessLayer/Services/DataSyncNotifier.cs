using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.Constants;
using System.Text.RegularExpressions;



namespace EWP.SF.KafkaSync.BusinessLayer;
public sealed class DataSyncNotifier(ISseService sseService) : IDataSyncNotifier
{
    public Task NotifyStartAsync(DataSyncService serviceData, string entityCode)
    {
        if (!string.Equals(serviceData.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        string payload = JsonSerializer.Serialize(new
        {
            Type = 999,
            Value = new { Event = serviceData.EntityId, Entity = entityCode, Status = 1 }
        });

        return sseService.SendEventToAllAsync(payload);
    }

    public Task NotifyStopAsync(DataSyncService serviceData, string entityCode, DateTime initDate)
    {
        if (!string.Equals(serviceData.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        string payload = JsonSerializer.Serialize(new
        {
            Type = 999,
            Value = new { Event = serviceData.EntityId, Entity = entityCode, Status = 0, LastExecDate = initDate }
        });

        return sseService.SendEventToAllAsync(payload);
    }
}