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
public class BinLocation : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_binlocation_log");

	[GridDrillDown]
	[EntityColumn("BinLocationCode")]
	public string LocationCode { get; set; }

	public string Name { get; set; }

	public string Aisle { get; set; }

	public string Rack { get; set; }

	public string Level { get; set; }

	public string Column { get; set; }

	[GridDrillDown("Warehouse", "Id")]
	public string WarehouseId { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreateUser { get; set; }

	public DateTime CreateDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User UpdateUser { get; set; }

	public DateTime UpdateDate { get; set; }

	[GridDrillDown("InventoryStatus", "Code")]
	public List<string> InventoryStatusCodes { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	public List<string> AttachmentIds { get; set; }
	public string LogDetailId { get; set; }
}

public class BinLocationExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Location Code")]
	[JsonProperty(PropertyName = "LocationCode")]
	public string LocationCode { get; set; }

	[MaxLength(500)]
	[Description("Location Name")]
	[JsonProperty(PropertyName = "LocationName")]
	public string LocationName { get; set; }

	[Required]
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	[MaxLength(250)]
	[Description("Aisle")]
	[JsonProperty(PropertyName = "Aisle")]
	public string Aisle { get; set; }

	[MaxLength(250)]
	[Description("Rack")]
	[JsonProperty(PropertyName = "Rack")]
	public string Rack { get; set; }

	[MaxLength(250)]
	[Description("Level")]
	[JsonProperty(PropertyName = "Level")]
	public string Level { get; set; }

	[MaxLength(250)]
	[Description("Column")]
	[JsonProperty(PropertyName = "Column")]
	public string Column { get; set; }

	[RegularExpression(RegularExpression.ProductStatusIntegration, ErrorMessage = "Invalid Status")]
	[Description("Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }
}
