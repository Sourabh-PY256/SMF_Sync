using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.Models.Sensors;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IMachineRepo
{
    List<Machine> ListMachines(string machineId = null, bool onlyActive = false, DateTime? DeltaDate = null);
    ResponseData CreateMachine(Machine machineInfo, User systemOperator, bool Validation, string Level);
    List<Sensor> ListSensors(string SensorId = null, string machineId = null);
    List<Sensor> GetSensors(string SensorId, string machineId, DateTime? DeltaDate = null);
    Sensor GetSensorsDetails(string SensorId);
    List<MachineParam> ListMachineParams(string parameterId = null, string machineId = null);

}

