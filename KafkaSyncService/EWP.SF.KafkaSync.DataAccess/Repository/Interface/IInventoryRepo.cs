using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IInventoryRepo
{
    Inventory GetInventory(string Code);
    List<Inventory> ListInventory(string Code = "", DateTime? DeltaDate = null);
    ResponseData MergeInventory(Inventory InventoryInfo, User systemOperator, bool Validation);
    List<SaleOrder> ListSalesOrder(string Id, string SalesOrder, string CustomerCode, DateTime? DeltaDate = null);
}
