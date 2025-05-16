using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;


public class facilityBin
{
	public int facilityCount { get; set; }
	public List<string> machines { get; set; }
}

public class Warehouse : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_warehouse_log");

	[GridDrillDown]
	public string WarehouseId { get; set; }

	[EntityColumn("WarehouseCode")]
	[GridDrillDown]
	public string Code { get; set; }

	[GridDrillDown]
	public string Id { get; set; }

	public string Name { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	public DateTime ModifyDate { get; set; }
	public List<BinLocation> Details { get; set; }

	[GridDrillDown("Facility", "Code")]
	public string FacilityCode { get; set; }

	public bool Schedule { get; set; }
	public List<string> AttachmentIds { get; set; }

	[GridDrillDown("BinLocation", "LocationCode")]
	public string BinLocationCode { get; set; }

	public bool IsProduction { get; set; }

	public string LogDetailId { get; set; }

	public Warehouse()
	{
	}

	public Warehouse(string id)
	{
		WarehouseId = id;
	}

	public string detailToJSON()
	{
		string returnValue = string.Empty;
		if (Details is not null)
		{
			returnValue = JsonConvert.SerializeObject(Details);
		}
		return returnValue;
	}
}

public class WarehouseExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	[MaxLength(200)]
	[Description("Warehouse Name")]
	[JsonProperty(PropertyName = "WarehouseName")]
	public string WarehouseName { get; set; }

	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Warehouse Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	[MaxLength(200)]
	[Description("Facility Code")]
	[JsonProperty(PropertyName = "FacilityCode")]
	public string FacilityCode { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Status")]
	[Description("Schedule")]
	[JsonProperty(PropertyName = "Schedule")]
	public string Schedule { get; set; }
	public List<BinLocationExternal> Locations { get; set; }
}
