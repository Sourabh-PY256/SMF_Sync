using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Item.BusinessEntities;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IDataSyncRepository
{
    // /// <summary>
    // /// Retrieves a list of work centers, optionally filtered by delta date
    // /// </summary>
    // /// <param name="deltaDate">Optional date filter for changes since this date</param>
    // /// <returns>List of work centers</returns>
    // Task<List<WorkCenter>> ListWorkCenter(DateTime? deltaDate = null);

    // /// <summary>
    // /// Retrieves a specific work center by its code
    // /// </summary>
    // /// <param name="workCenterCode">The unique code of the work center</param>
    // /// <returns>Work center details if found, null otherwise</returns>
    // Task<WorkCenter> GetWorkCenter(string workCenterCode);

    // /// <summary>
    // /// Creates a new work center
    // /// </summary>
    // /// <param name="workCenterInfo">Work center information to create</param>
    // /// <param name="systemOperator">User performing the operation</param>
    // /// <param name="validation">Whether to perform validation</param>
    // /// <param name="level">Hierarchical level of the work center</param>
    // /// <returns>Response indicating success or failure</returns>
    // //Task<ResponseData> CreateWorkCenter(WorkCenter workCenterInfo, User systemOperator, bool validation, string level);

    // Task<bool> UpdateWorkCenter(WorkCenter workCenterInfo, User systemOperator);
    // Task<bool> DeleteWorkCenter(WorkCenter workCenterInfo, User systemOperator);
}