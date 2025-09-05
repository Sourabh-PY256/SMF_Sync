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


/// <summary>
///
/// </summary>
public class facilityBin
{
	/// <summary>
	///
	/// </summary>
	public int facilityCount { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> machines { get; set; }
}

/// <summary>
///
/// </summary>
public class Warehouse : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_warehouse_log");

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	public string WarehouseId { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("WarehouseCode")]
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

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
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BinLocation> Details { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Facility", "Code")]
	public string FacilityCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("BinLocation", "BinLocationCode")]
	public string BinLocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsProduction { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public Warehouse()
	{
	}

	/// <summary>
	///
	/// </summary>
	public Warehouse(string id)
	{
		WarehouseId = id;
	}

	/// <summary>
	///
	/// </summary>
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

/// <summary>
///
/// </summary>
public class WarehouseExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Warehouse Name")]
	[JsonProperty(PropertyName = "WarehouseName")]
	public string WarehouseName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Warehouse Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Facility Code")]
	[JsonProperty(PropertyName = "FacilityCode")]
	public string FacilityCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Status")]
	[Description("Schedule")]
	[JsonProperty(PropertyName = "Schedule")]
	public string Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BinLocationExternal> Locations { get; set; }
}
