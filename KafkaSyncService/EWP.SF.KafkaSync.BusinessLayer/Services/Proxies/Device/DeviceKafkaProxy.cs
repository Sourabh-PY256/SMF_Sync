using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Device/Machine operations.
/// </summary>
public class DeviceKafkaProxy : BaseKafkaProxy, IDeviceOperation
{
    public DeviceKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<DeviceKafkaProxy> logger)
         : base(kafkaService, configuration, logger,
             "KafkaSettings:Topics:MACHINE",               // appsettings key
             "shopfloor-machine-sync")                     // fallback topic (matches consumer)
    { }

    public Task<Machine[]> ListDevices(
        bool deleted = false,
        bool listOnly = false,
        bool onlyActive = false,
        DateTime? DeltaDate = null,
        bool showDisabled = false,
        string logId = null) => throw new NotSupportedException("ListDevices cannot be called through the Kafka proxy.");

    public async Task<ResponseData> CreateMachine(
        Machine machineInfo,
        User systemOperator,
        bool validate = false,
        string level = "Success",
        bool notifyOnce = true,
        string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.MACHINE_SERVICE,
            "CreateMachine",
            systemOperator,
            new
            {
                Data = machineInfo,
                Validate = validate,
                Level = level,
                NotifyOnce = notifyOnce
            },
            logId).ConfigureAwait(false);

        return result;
    }

    public async Task<List<ResponseData>> ListUpdateMachine(
        List<MachineExternal> listMachines,
        List<MachineExternal> listMachinesOriginal,
        User systemOperator,
        bool validate,
        string level,
        string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.MACHINE_SERVICE,
            "ListUpdateMachine",
            systemOperator,
            new
            {
                Data = listMachines,
                OriginalData = listMachinesOriginal,
                Validate = validate,
                Level = level
            },
            logId).ConfigureAwait(false);

        return [result];
    }
}
