using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IDemandRepo
{
    ResponseData MergeDemandBulk(string Json, User systemOperator, bool Validation);
    Task<List<Demand>> ListDemand(string orderNumber = "", CancellationToken cancel = default);
    

}