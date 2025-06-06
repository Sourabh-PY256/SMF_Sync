using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IProcessTypeOperation
{


    List<ResponseData> ListUpdateSuboperationTypes_Bulk(List<SubProcessTypeExternal> clockList, User systemOperator, bool Validate, LevelMessage Level);
    List<ProcessType> GetProcessTypes(string processType, User systemOperator, bool WithTool = false, DateTime? DeltaDate = null);
    List<ProcessTypeDetail> ListMachineProcessTypeDetails(string machineId, User systemOperator);
}