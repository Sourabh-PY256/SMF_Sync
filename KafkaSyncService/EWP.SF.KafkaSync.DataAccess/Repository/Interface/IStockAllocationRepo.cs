using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IStockAllocationRepo

{
    ResponseData MergeStockAllocationBulk(string JSONData, User systemOperator, bool Validation, bool nodelete = false);
    
}