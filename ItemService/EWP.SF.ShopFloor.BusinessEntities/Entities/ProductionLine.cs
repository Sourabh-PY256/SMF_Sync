using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.ShopFloor.BusinessEntities;

public class ProductionLine : ICloneable, ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_production_line_log");

	public ProductionLine()
	{
	}

	public ProductionLine(string id)
	{
		Id = id;
	}

	[GridDrillDown]
	[GridDisabledHiding]
	public string Id { get; set; }

	[EntityColumn("ProductionLineCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	public string AssetTypeCode { get; set; }

	[GridDisabledHiding]
	public string Description { get; set; }

	public int WorkingTime { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	public DateTime? StartDate { get; set; }
	public List<DeviceLink> Devices { get; set; }

	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	public string Icon { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	public string Location { get; set; }
	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	[GridLookUpEntity(null, "WorkCenter", "Id", "Name")]
	[GridCustomPropertyName("WorkCenterName")]
	[GridDrillDown("WorkCenter", "Id"),]
	[GridRequireDecode]
	public string ParentId { get; set; }

	public string AssetType { get; set; }

	[GridCustomPropertyName("WorkCenterCode")]
	[GridDrillDown("WorkCenter", "Code")]
	[GridRequireDecode]
	public string ParentCode { get; set; }

	public string ParentAssetTypeCode { get; set; }
	public List<string> AttachmentIds { get; set; }
	public List<Activity> Activities { get; set; }
	public SchedulingCalendarShifts Shift { get; set; }
	public SchedulingCalendarShifts ShiftDelete { get; set; }

	public string LogDetailId { get; set; }

	public object Clone()
	{
		return MemberwiseClone();
	}
}
