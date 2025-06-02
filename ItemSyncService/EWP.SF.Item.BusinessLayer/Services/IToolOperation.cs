using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IToolOperation
{
    Task<List<ResponseData>> ListUpdateToolType(List<ToolTypeExternal> toolTypeList, List<ToolTypeExternal> toolTypeListOriginal, User systemOperator, bool Validate, LevelMessage Level);

}