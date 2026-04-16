using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using System.Text.RegularExpressions;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Device/Machine operations.
/// </summary>
public class DeviceOperationProxy : BaseHttpProxy, IDeviceOperation
{
    public DeviceOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public Task<Machine[]> ListDevices(
        bool deleted = false,
        bool listOnly = false,
        bool onlyActive = false,
        DateTime? DeltaDate = null,
        bool showDisabled = false,
        string logId = null) => throw new NotSupportedException("ListDevices is not available through the HTTP proxy.");

    public async Task<ResponseData> CreateMachine(
        Machine machineInfo,
        User systemOperator,
        bool validate = false,
        string level = "Success",
        bool notifyOnce = true,
        string logId = null)
    {
        string endpoint = $"Machine/Create/{validate.ToString().ToLower()}/{level}";
        return await PostAsync<ResponseData>(endpoint, new { machineInfo, systemOperator, notifyOnce }).ConfigureAwait(false);
    }

    public async Task<List<ResponseData>> ListUpdateMachine(
        List<MachineExternal> listMachines,
        List<MachineExternal> listMachinesOriginal,
        User systemOperator,
        bool validate,
        string level,
        string logId = null)
    {
        foreach (var machine in listMachines)
        {
            NormalizeMachine(machine);
        }
        string endpoint = $"Machine/{validate.ToString().ToLower()}/{level}";
        return await PostAsync<List<ResponseData>>(endpoint, listMachines).ConfigureAwait(false);
    }


    private void NormalizeMachine(MachineExternal m)
    {
        if (string.IsNullOrWhiteSpace(m.MachineCode))
            m.MachineCode = Guid.NewGuid().ToString();

        if (string.IsNullOrWhiteSpace(m.MachineName))
            m.MachineName = "Default Machine";

        if (string.IsNullOrWhiteSpace(m.Status) ||
            !Regex.IsMatch(m.Status, "^(Active|Disable)$", RegexOptions.IgnoreCase))
            m.Status = "Active";

        if (string.IsNullOrWhiteSpace(m.Type) ||
            !Regex.IsMatch(m.Type, "^(Process|Auxiliar)$", RegexOptions.IgnoreCase))
            m.Type = "Process";

        if (string.IsNullOrWhiteSpace(m.CapacityMode) ||
            !Regex.IsMatch(m.CapacityMode, "^(Finite|Infinite|InfiniteWithShiftPattern)$"))
            m.CapacityMode = "Finite";

        if (string.IsNullOrWhiteSpace(m.InfiniteModeBehavior) ||
            !Regex.IsMatch(m.InfiniteModeBehavior, "^(Finite|Infinite|InfiniteWithShiftPattern)$"))
            m.InfiniteModeBehavior = "Finite";

        if (string.IsNullOrWhiteSpace(m.ScheduleLevel))
            m.ScheduleLevel = "Primary";

        if (string.IsNullOrWhiteSpace(m.ConcurrentSetupTime))
            m.ConcurrentSetupTime = "No";

        if (string.IsNullOrWhiteSpace(m.Schedule))
            m.Schedule = "Yes";

        if (string.IsNullOrWhiteSpace(m.Planning))
            m.Planning = "Yes";

        if (string.IsNullOrWhiteSpace(m.ProductionType))
            m.ProductionType = "Pieces";

        if (m.MinimumCapacity == 0)
            m.MinimumCapacity = 1;

        if (m.MaximumCapacity == null)
            m.MaximumCapacity = 10;

        if (m.CostPerHour == 0)
            m.CostPerHour = 100;

        if (m.RunTime == null)
            m.RunTime = 60;
    }
}
