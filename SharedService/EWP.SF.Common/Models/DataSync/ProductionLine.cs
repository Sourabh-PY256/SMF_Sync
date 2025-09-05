using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class ProductionLine : ICloneable, ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_production_line_log");

	/// <summary>
	///
	/// </summary>
	public ProductionLine()
	{
	}

	/// <summary>
	///
	/// </summary>
	public ProductionLine(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridDisabledHiding]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ProductionLineCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDisabledHiding]
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public int WorkingTime { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<DeviceLink> Devices { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	public string Icon { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "WorkCenter", "Id", "Name")]
	[GridCustomPropertyName("WorkCenterName")]
	[GridDrillDown("WorkCenter", "Id"),]
	[GridRequireDecode]
	public string ParentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("WorkCenterCode")]
	[GridDrillDown("WorkCenter", "Code")]
	[GridRequireDecode]
	public string ParentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("WorkCenterCode")]
	[GridDrillDown("WorkCenter", "Code", "ParentCode")]
	[GridRequireDecode]
	public string ParentName { get; set; }

	[GridIgnoreProperty]
	public string FacilityCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentAssetTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Activities { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts Shift { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts ShiftDelete { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	public object Clone() => MemberwiseClone();

	 	/// <summary>
	/// User-defined fields stored as JSON
	/// </summary>
	[GridIgnoreProperty]
	public object UserFields { get; set; }


}
