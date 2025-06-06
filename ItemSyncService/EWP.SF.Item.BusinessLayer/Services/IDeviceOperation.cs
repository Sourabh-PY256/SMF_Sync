using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IDeviceOperation
{
    Machine[] ListDevices(
        bool deleted = false,
        bool listOnly = false,
        bool onlyActive = false,
        DateTime? DeltaDate = null,
        bool showDisabled = false);
    Task<ResponseData> CreateMachine(
        Machine machineInfo,
        User systemOperator,
        bool validate = false,
        string level = "Success",
        bool notifyOnce = true);
        Task<List<ResponseData>> ListUpdateMachine(
        List<MachineExternal> listMachines,
        List<MachineExternal> listMachinesOriginal,
        User systemOperator,
        bool validate,
        string level);
}
