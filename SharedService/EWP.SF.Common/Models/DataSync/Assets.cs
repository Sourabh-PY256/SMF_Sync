using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class Asset
{
	/// <summary>
	///
	/// </summary>
	public Asset()
	{
	}

	/// <summary>
	///
	/// </summary>
	public Asset(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentCode { get; set; }
}

/// <summary>
///
/// </summary>
public class Facility : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_facility_log");

	/// <summary>
	///
	/// </summary>
	public Facility()
	{
	}

	/// <summary>
	///
	/// </summary>
	public Facility(string code)
	{
		Code = code;
	}

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentId { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("FacilityCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	[GridDisableSorting]
	public string Icon { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Street { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ZipCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string City { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StateProvince { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Country", "Code", "Name")]
	public string Country { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	[GridDisableSorting]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PhoneNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Number { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Region { get; set; }

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
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StatusDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SchedulingModel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DefaultLanguage { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Latitude { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Longitude { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PlanningModel { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "CustomFacilityManualCoordinates", "Id", "Name")]
	[GridRequireTranslate]
	public int ManualCoordinates { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Floor> Children { get; set; }

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
	/// <summary>
	/// User-defined fields stored as JSON
	/// </summary>
	public object UserFields { get; set; }

}

/// <summary>
///
/// </summary>
public class Floor : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_floor_log");

	/// <summary>
	///
	/// </summary>
	public Floor()
	{
	}

	/// <summary>
	///
	/// </summary>
	public Floor(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDrillDown]
	[GridDisabledHiding]
	[EntityColumn("FloorCode")]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("FacilityCode")]
	[GridDrillDown("Facility", "Code")]
	[GridRequireDecode]
	public string ParentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Facility", "Id", "Name")]
	[GridCustomPropertyName("FacilityName")]
	[GridDrillDown("Facility", "Id", "ParentCode")]
	[GridRequireDecode]
	public string ParentId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ParentName")]
	[GridDrillDown("Facility", "Code", "ParentCode")]
	[GridRequireDecode]
	public string ParentName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	[GridDisableSorting]
	public string Icon { get; set; }

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
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkCenter> Children { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	[GridDisableSorting]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetType { get; set; }

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

	/// <summary>
	/// User-defined fields stored as JSON
	/// </summary>
	[GridIgnoreProperty]
	public object UserFields { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkCenter : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_workcenter_log");

	/// <summary>
	///
	/// </summary>
	public WorkCenter()
	{
	}

	/// <summary>
	///
	/// </summary>
	public WorkCenter(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Id { get; set; }

	// NOTA: éste daño se llena en el front por ahora
	//[GridLookUpEntity(null, "Floor", "Code", "Name")]
	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Floor", "Name")]
	[GridCustomPropertyName("FloorName")]
	[GridRequireDecode]
	public string ParentId { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("WorkCenterCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	public string Icon { get; set; }

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
	public string AssetType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Floor", "Code")]
	[GridCustomPropertyName("FloorCode")]
	[GridRequireDecode]
	public string ParentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Floor", "Code", "ParentCode")]
	[GridCustomPropertyName("ParentName")]
	[GridRequireDecode]
	public string ParentName { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductionLine> Children { get; set; }

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
}

/// <summary>
///
/// </summary>
public class DeviceLink
{
	/// <summary>
	///
	/// </summary>
	public DeviceLink()
	{
	}

	/// <summary>
	///
	/// </summary>
	public DeviceLink(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }
}

/// <summary>
///
/// </summary>
public class AssetExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Description("Asset Code")]
	[JsonProperty(PropertyName = "AssetCode")]
	[MaxLength(100)]
	[Required]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Code format.")]
	public string AssetCode { get; set; }

	/// <summary>
	/// Asset Name
	/// </summary>
	[JsonProperty(PropertyName = "AssetName")]
	[MaxLength(200)]
	public string AssetName { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty(PropertyName = "ParentCode")]
	[MaxLength(200)]
	public string ParentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty(PropertyName = "AssetType")]
	public string AssetType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(15)]
	[Description("Status")]
	[JsonProperty(PropertyName = "Status")]
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	public string Status { get; set; }
}

/// <summary>
///
/// </summary>
public class AssetResponse
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsSuccess { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public ActionDB Action { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetType { get; set; }
}

/// <summary>
///
/// </summary>
public class AssetKpi : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public AssetKpi()
	{
		Tags = [];
		Orders = [];
	}

	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_assets_log");

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool HasKpiData { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int Level { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ParentType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ParentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	//------ KPIs
	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal OEE { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal Availability { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal Performance { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal Quality { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal PlannedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal AcceptedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string? QuantityUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? WorkingMachines { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Product { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? ActiveEmployees { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? CheckedEmployees { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? OrdersInProgress { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? LateWorkOrders { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Downtime { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Tags> Tags { get; set; }
	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<AssetKpiMachine> Orders { get; set; }
}

/// <summary>
///
/// </summary>
public class LiveScreen
{
	/// <summary>
	///
	/// </summary>
	public LiveScreen()
	{
		Tags = [];
		Orders = [];
	}

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool HasKpiData { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Dinamic_AssetType", "Code")]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int Level { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ParentType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ParentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	//------ KPIs
	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal OEE { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal Availability { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal Performance { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal Quality { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal PlannedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal AcceptedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string? QuantityUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? WorkingMachines { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Product { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? ActiveEmployees { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? CheckedEmployees { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? OrdersInProgress { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? LateWorkOrders { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Downtime { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Tags> Tags { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<AssetKpiMachine> Orders { get; set; }
}

/// <summary>
///
/// </summary>
public class Tags
{
	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string TagCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string TagName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal TagValue { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string TagColor { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool TagFlicker { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string TagUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string TagIcon { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int TagVisible { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int TagScalar { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string TagDataType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string TagType { get; set; }
}

/// <summary>
///
/// </summary>
public class AssetKpiMachine
{
	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status MachineStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status OrderStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status OperationStatus { get; set; }
}

/// <summary>
///
/// </summary>
public class FacilityMapKpi
{
	/// <summary>
	///
	/// </summary>
	public bool HasKpi { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal? Latitude { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal? Longitude { get; set; }

	// KPI
	/// <summary>
	///
	/// </summary>
	public decimal OEE { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Availability { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Performance { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal OpenWorkOrders { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal LateWorkOrders { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal WorkingMachines { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal WorkingEmployees { get; set; }
}
