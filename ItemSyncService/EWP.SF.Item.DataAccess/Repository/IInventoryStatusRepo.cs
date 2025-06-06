using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IInventoryStatusRepo
{
    List<InventoryStatus> ListInventoryStatus(string LotSerialStatusCode, DateTime? DeltaDate = null);
    ResponseData MergeInventoryStatus(InventoryStatus InventoryStatusInfo, User systemOperator, bool Validation);
    
}
