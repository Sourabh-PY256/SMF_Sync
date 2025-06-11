using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IToolRepo
{
    List<ToolType> ListToolType(string ToolTypeCode, DateTime? DeltaDate = null);
    ResponseData CreateToolType(ToolType toolTypeInfo, User systemOperator, bool Validation);
    List<Tool> ListTools(string ToolCode, DateTime? DeltaDate = null);
    
}