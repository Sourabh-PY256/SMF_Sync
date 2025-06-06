using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IInventoryOperation
{

    Task<List<ResponseData>> ListUpdateInventoryGroup(List<InventoryExternal> inventoryGroupList, List<InventoryExternal> inventoryGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    Task<ResponseData> MergeInventory(Inventory InventoryInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true);

    List<Inventory> ListInventory(User systemOperator, string InventoryCode = "", DateTime? DeltaDate = null);
    
}