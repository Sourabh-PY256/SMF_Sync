using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;


/// <summary>
///
/// </summary>
public class Supply
{
	/// <summary>
	///
	/// </summary>
	[EntityColumn("SupplyId")]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string VendorCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string VendorName { get; set; }

	/// <summary>
	///
	/// </summary>
	public int LineNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpectedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }
}

/// <summary>
///
/// </summary>
public class SupplyExternal
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	public string ItemCode { get; set; }

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
	[Required]
	[MaxLength(100)]
	[RegularExpression("Purchase Order|MRP", ErrorMessage = "Invalid Type")]
	[Description("Type")]
	[JsonProperty(PropertyName = "Type")]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Supply Number")]
	[JsonProperty(PropertyName = "SupplyNo")]
	public string SupplyNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Line ID")]
	[JsonProperty(PropertyName = "LineID")]
	public int LineID { get; set; }

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
	[Required]
	[MaxLength(100)]
	[Description("Inventory UoM")]
	[JsonProperty(PropertyName = "InventoryUoM")]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Supply Date")]
	[JsonProperty(PropertyName = "SupplyDate")]
	public DateTime SupplyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Vendor Code")]
	[JsonProperty(PropertyName = "VendorCode")]
	public string VendorCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Vendor Name")]
	[JsonProperty(PropertyName = "VendorName")]
	public string VendorName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(15)]
	[Description("Status")]
	[JsonProperty(PropertyName = "Status")]
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	public string Status { get; set; }
}
