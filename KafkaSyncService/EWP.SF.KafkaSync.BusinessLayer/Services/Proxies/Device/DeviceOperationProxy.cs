using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

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
        string endpoint = $"Machine/Bulk/{validate.ToString().ToLower()}/{level}";
        return await PostAsync<List<ResponseData>>(endpoint, listMachines).ConfigureAwait(false);
    }
}
