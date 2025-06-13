using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface ISupplyRepo
{
    ResponseData MergeSupply(Supply SupplyInfo, User systemOperator, bool Validation);
    Task<List<Supply>> ListSupply(string orderNumber = "", CancellationToken cancel = default);
    
}