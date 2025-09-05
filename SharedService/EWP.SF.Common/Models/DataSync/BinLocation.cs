using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.Constants;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;
/// <summary>
///
/// </summary>
public class BinLocation : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_binlocation_log");

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[EntityColumn("BinLocationCode")]
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Aisle { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Rack { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Level { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Column { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Warehouse", "Id")]
	public string WarehouseId { get; set; }

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
	[GridDrillDown("InventoryStatus", "Code")]
	public List<string> InventoryStatusCodes { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }
}

/// <summary>
///
/// </summary>
public class BinLocationExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Location Code")]
	[JsonProperty(PropertyName = "LocationCode")]
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Location Name")]
	[JsonProperty(PropertyName = "LocationName")]
	public string LocationName { get; set; }

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
	[MaxLength(250)]
	[Description("Aisle")]
	[JsonProperty(PropertyName = "Aisle")]
	public string Aisle { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(250)]
	[Description("Rack")]
	[JsonProperty(PropertyName = "Rack")]
	public string Rack { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(250)]
	[Description("Level")]
	[JsonProperty(PropertyName = "Level")]
	public string Level { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(250)]
	[Description("Column")]
	[JsonProperty(PropertyName = "Column")]
	public string Column { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression(RegularExpression.ProductStatusIntegration, ErrorMessage = "Invalid Status")]
	[Description("Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }
}
