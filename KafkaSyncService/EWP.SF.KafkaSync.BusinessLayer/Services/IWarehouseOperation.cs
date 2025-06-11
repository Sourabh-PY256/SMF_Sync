using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IWarehouseOperation
{
    Task<List<ResponseData>> ListUpdateWarehouseGroup(List<WarehouseExternal> warehouseGroupList, List<WarehouseExternal> warehouseGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    Task<ResponseData> MergeWarehouse(Warehouse WarehouseInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true);
    Warehouse GetWarehouse(string Code);
    List<Warehouse> ListWarehouse(User systemOperator, string WarehouseCode = "", DateTime? DeltaDate = null);
}