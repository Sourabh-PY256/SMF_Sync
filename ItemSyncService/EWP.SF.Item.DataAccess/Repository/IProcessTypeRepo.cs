using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IProcessTypeRepo
{
    ResponseData SaveSubOperationTypes_Bulk(string paramsJSON, User systemOperator);
    List<ProcessType> GetProcessType(string processTypeId, bool WithTool = false, DateTime? DeltaDate = null);
    List<ProcessTypeDetail> ListMachineProcessTypeDetails(string machineId);
}

