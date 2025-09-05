using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IInventoryOperation
{

    Task<List<ResponseData>> ListUpdateInventoryGroup(List<InventoryExternal> inventoryGroupList, List<InventoryExternal> inventoryGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    Task<ResponseData> MergeInventory(InventoryItemGroup InventoryInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true);

    List<InventoryItemGroup> ListInventory(User systemOperator, string InventoryCode = "", DateTime? DeltaDate = null);

    InventoryItemGroup GetInventory(string Code);
    SaleOrder[] ListSalesOrder(string Id, string SalesOrder, string CustomerCode, User systemOperator, bool getAsMasterDetail = false, DateTime? DeltaDate = null);

}