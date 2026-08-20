
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IDeviceOperation
{
    Task<Machine[]> ListDevices(
        bool deleted = false,
        bool listOnly = false,
        bool onlyActive = false,
        DateTime? DeltaDate = null,
        bool showDisabled = false,
        string logId = null);
    Task<ResponseData> CreateMachine(
        Machine machineInfo,
        User systemOperator,
        bool validate = false,
        string level = "Success",
        bool notifyOnce = true,
        string logId = null);
    Task<List<ResponseData>> ListUpdateMachine(
        List<MachineExternal> listMachines,
        List<MachineExternal> listMachinesOriginal,
        User systemOperator,
        bool validate,
        string level,
        string logId = null);
}
