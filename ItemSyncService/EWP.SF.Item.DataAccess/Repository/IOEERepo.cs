using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IOEERepo
{

    MachineProgramming GetMachineProgramming(string machineId);
    bool SaveMachineProgramming(string machineId, MachineProgramming configuration, User systemOperator);
    double SaveMachineQuality(string machineId, bool isOutput, string workOrderId, string processId, string shiftId, QualityType type, QualityMode mode, string testId, string sampleId, double sample, double rejected, User systemOperator, string employeeId = "", Action<string> callback = null);

    double SaveSensorAverage(string sensorId, string workOrderId, double value);

    bool SaveMachineAvailability(string machineId, bool online, User systemOperator, OEEMode mode, string downtimeId = "");
    bool SaveMachinePerformance(string machineId, string processId, bool isOutput, string workOrderId, double value, double factor, double deviceFactor, double processFactor, double orderFactor);

    bool SaveMachineOeeConfiguration(string machineId, MachineOEEConfiguration configuration, User systemOperator);
    Task<OEEModel> GetLiveOee(string machineid, DateTime? startDate, CancellationToken cancel);
    MachineOEEConfiguration GetMachineOeeConfiguration(string machineId);
    List<MachineOEEConfiguration> GetMachineOeeConfiguration();
    


}