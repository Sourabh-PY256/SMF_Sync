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
public class InventoryStatus : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_inventorystatus_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("InventoryStatusCode")]
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User UpdateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime UpdateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsDelivery { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsARInvoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsARCreditMemo { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsReturn { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAPReturn { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAPCreditMemo { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsInventoryIssue { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsMaterialIssue { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAllocation { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsInventoryTransfer { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsInventoryCounting { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsPlanning { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	[GridDisableSorting]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }
}

/// <summary>
///
/// </summary>
public class InventoryStatusExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Inventory Status Code")]
	[JsonProperty(PropertyName = "InventoryStatusCode")]
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Inventory Status Name")]
	[JsonProperty(PropertyName = "InventoryStatusName")]
	public string InventoryStatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Inventory Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsDelivery")]
	public string IsDelivery { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsARInvoice")]
	public string IsARInvoice { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsARCreditMemo")]
	public string IsARCreditMemo { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsReturn")]
	public string IsReturn { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsAPReturn")]
	public string IsAPReturn { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsAPCreditMemo")]
	public string IsAPCreditMemo { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsInventoryIssue")]
	public string IsInventoryIssue { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsMaterialIssue")]
	public string IsMaterialIssue { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsAllocation")]
	public string IsAllocation { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsInventoryTransfer")]
	public string IsInventoryTransfer { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsInventoryCounting")]
	public string IsInventoryCounting { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsPlanning")]
	public string IsPlanning { get; set; }
}
