using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace EWP.SF.Common.Models;
/// <summary>
///
/// </summary>
public class StockAllocation
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpDate { get; set; }
}

/// <summary>
///
/// </summary>
public class StockAllocationExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Lot Serial Number")]
	[JsonProperty(PropertyName = "LotSerialNo")]
	public string LotSerialNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Pallet")]
	[JsonProperty(PropertyName = "Pallet")]
	public string Pallet { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Location Code")]
	[JsonProperty(PropertyName = "LocationCode")]
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Inventory Status Code")]
	[JsonProperty(PropertyName = "InventoryStatusCode")]
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Exp Date")]
	[JsonProperty(PropertyName = "ExpDate")]
	public DateTime ExpDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Lot Serial Status Code")]
	[JsonProperty(PropertyName = "LotSerialStatusCode")]
	public string LotSerialStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Inventory UoM")]
	[JsonProperty(PropertyName = "InventoryUoM")]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Order Code")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Operation No")]
	[JsonProperty(PropertyName = "OperationNo")]
	public double OperationNo { get; set; }
}
