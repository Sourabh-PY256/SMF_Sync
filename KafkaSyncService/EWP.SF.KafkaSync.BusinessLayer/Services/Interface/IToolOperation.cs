using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IToolOperation
{
    Task<List<ResponseData>> ListUpdateToolType(List<ToolTypeExternal> toolTypeList, List<ToolTypeExternal> toolTypeListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    List<ToolType> ListToolTypes(string ToolTypeCode, DateTime? DeltaDate = null);
    List<Tool> ListTools(string ToolCode, DateTime? DeltaDate = null);

}