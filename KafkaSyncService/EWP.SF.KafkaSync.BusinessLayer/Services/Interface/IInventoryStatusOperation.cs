using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IInventoryStatusOperation
{
    List<InventoryStatus> ListInventoryStatus(User systemOperator, string LotSerialStatusCode = "", DateTime? DeltaDate = null);
    Task<ResponseData> MergeInventoryStatus(InventoryStatus InventoryStatusInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true);
    Task<List<ResponseData>> ListUpdateInventoryStatus(List<InventoryStatusExternal> inventoryStatusList, List<InventoryStatusExternal> inventoryStatusListOriginal, User systemOperator, bool Validate, LevelMessage Level);

}