using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Employee operations.
/// </summary>
public class EmployeeKafkaProxy : BaseKafkaProxy, IEmployeeOperation
{
    public EmployeeKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<EmployeeKafkaProxy> logger)
         : base(kafkaService, configuration, logger,
             "KafkaSettings:Topics:Employee",               // appsettings key
             "shopfloor-employee-sync")                     // fallback topic (matches consumer)
    { }

    public async Task<List<ResponseData>> ImportEmployeesAsync(
        List<EmployeeExternal> requestValue,
        List<EmployeeExternal> originalValue,
        User systemOperator,
        bool Validate = false,
        LevelMessage Level = 0,
        bool NotifyOnce = true,
        bool isDataSync = false,
        string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.EMPLOYEE_SERVICE,
            "ImportEmployeesAsync",
            systemOperator,
            new
            {
                Data         = requestValue,
                OriginalData = originalValue,
                Validate,
                Level,
                NotifyOnce,
                IsDataSync   = isDataSync
            },
            logId).ConfigureAwait(false);

        return [result];
    }

    public async Task<List<ResponseData>> MRGEmployee(
        List<Employee> requestValue,
        User systemOperator,
        bool Validate = false,
        LevelMessage Level = 0,
        bool NotifyOnce = true,
        bool isDataSync = false,
        string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.EMPLOYEE_SERVICE,
            "MRGEmployee",
            systemOperator,
            new
            {
                Data       = requestValue,
                Validate,
                Level,
                NotifyOnce,
                IsDataSync = isDataSync
            },
            logId).ConfigureAwait(false);

        return [result];
    }

    public List<Employee> GetEmployees(string id, string code, User systemOperator, DateTime? DeltaDate = null)
        => throw new NotSupportedException("GetEmployees cannot be called through the Kafka proxy.");
}
