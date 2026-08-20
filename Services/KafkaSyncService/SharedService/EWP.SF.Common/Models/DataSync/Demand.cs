using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models;


/// <summary>
///
/// </summary>
public class Demand : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_demand_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("DemandId")]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CustomerCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CustomerName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("LineNumber")]
	public int LineNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Item", "Code")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Warehouse", "Code")]
	public string WarehouseCode { get; set; }

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
	public int Priority { get; set; }

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
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }
}

/// <summary>
///
/// </summary>
public class DemandExternal
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
	[RegularExpression("Sales Order|Forecast", ErrorMessage = "Invalid Type")]
	[Description("Type")]
	[JsonProperty(PropertyName = "Type")]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Demand Number")]
	[JsonProperty(PropertyName = "DemandNo")]
	public string DemandNo { get; set; }

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
	[Description("Demand Date")]
	[JsonProperty(PropertyName = "DemandDate")]
	public DateTime DemandDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Customer Code")]
	[JsonProperty(PropertyName = "CustomerCode")]
	public string CustomerCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Customer Name")]
	[JsonProperty(PropertyName = "CustomerName")]
	public string CustomerName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(15)]
	[Description("Status")]
	[JsonProperty(PropertyName = "Status")]
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	public string Status { get; set; }
}
