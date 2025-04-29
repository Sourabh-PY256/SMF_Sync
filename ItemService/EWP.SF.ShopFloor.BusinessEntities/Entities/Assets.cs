using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.ShopFloor.BusinessEntities;
public class Asset
{
	public Asset()
	{
	}

	public Asset(string id)
	{
		Id = id;
	}

	public string Id { get; set; }
	public string ParentId { get; set; }
	public string Code { get; set; }
	public string Name { get; set; }
	public int Type { get; set; }
	public string Image { get; set; }
	public string AssetTypeCode { get; set; }
	public string ParentCode { get; set; }
}


public class Floor : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_floor_log");

	public Floor()
	{
	}

	public Floor(string id)
	{
		Id = id;
	}

	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Id { get; set; }

	[GridRequireDecode]
	[GridDrillDown]
	[GridDisabledHiding]
	[EntityColumn("FloorCode")]
	public string Code { get; set; }

	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	[GridCustomPropertyName("FacilityCode")]
	[GridDrillDown("Facility", "Code")]
	[GridRequireDecode]
	public string ParentCode { get; set; }

	[GridLookUpEntity(null, "Facility", "Id", "Name")]
	[GridCustomPropertyName("FacilityName")]
	[GridDrillDown("Facility", "Id", "ParentCode")]
	[GridRequireDecode]
	public string ParentId { get; set; }

	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	[GridDisableSorting]
	public string Icon { get; set; }

	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	public List<WorkCenter> Children { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	[GridDisableSorting]
	public string Image { get; set; }

	public string AssetType { get; set; }
	public List<string> AttachmentIds { get; set; }
	public List<Activity> Activities { get; set; }
	public SchedulingCalendarShifts Shift { get; set; }
	public SchedulingCalendarShifts ShiftDelete { get; set; }

	public string LogDetailId { get; set; }
}

public class WorkCenter : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_workcenter_log");

	public WorkCenter()
	{
	}

	public WorkCenter(string id)
	{
		Id = id;
	}

	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Id { get; set; }

	// NOTA: éste daño se llena en el front por ahora
	//[GridLookUpEntity(null, "Floor", "Code", "Name")]
	[GridDrillDown("Floor", "Name")]
	[GridCustomPropertyName("FloorName")]
	[GridRequireDecode]
	public string ParentId { get; set; }

	[EntityColumn("WorkCenterCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	public string Icon { get; set; }

	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	public string AssetType { get; set; }

	[GridDrillDown("Floor", "Code")]
	[GridCustomPropertyName("FloorCode")]
	[GridRequireDecode]
	public string ParentCode { get; set; }

	public List<ProductionLine> Children { get; set; }
	public List<string> AttachmentIds { get; set; }
	public List<Activity> Activities { get; set; }
	public SchedulingCalendarShifts Shift { get; set; }
	public SchedulingCalendarShifts ShiftDelete { get; set; }

	public string LogDetailId { get; set; }
}

public class DeviceLink
{
	public DeviceLink()
	{
	}

	public DeviceLink(string id)
	{
		Id = id;
	}

	public string Id { get; set; }

	public string Name { get; set; }

	public DateTime CreationDate { get; set; }

	public Status Status { get; set; }
}



public class AssetResponse
{
	[JsonIgnoreTransport]
	public string Id { get; set; }

	public string Code { get; set; }
	public bool IsSuccess { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public ActionDB Action { get; set; }

	public string Message { get; set; }
	public string AssetType { get; set; }
}

public class AssetKpi : ILoggableEntity
{
	public AssetKpi()
	{
		Tags = [];
		Orders = [];
	}
	public EntityLoggerConfig EntityLogConfiguration => new("sf_assets_log");

	[GridIgnoreProperty]
	public bool HasKpiData { get; set; }

	public string AssetType { get; set; }
	[GridIgnoreProperty]
	public string Id { get; set; }
	[GridDrillDown]
	public string Code { get; set; }
	public string Name { get; set; }

	[GridIgnoreProperty]
	public int Level { get; set; }

	[GridIgnoreProperty]
	public string ParentType { get; set; }

	[GridIgnoreProperty]
	public string ParentId { get; set; }

	public string ParentCode { get; set; }
	public string ParentName { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	//------ KPIs
	[GridIgnoreProperty]
	public decimal OEE { get; set; }

	[GridIgnoreProperty]
	public decimal Availability { get; set; }

	[GridIgnoreProperty]
	public decimal Performance { get; set; }

	[GridIgnoreProperty]
	public decimal Quantity { get; set; }

	[GridIgnoreProperty]
	public decimal? QuantityUoM { get; set; }

	[GridIgnoreProperty]
	public decimal? WorkingMachines { get; set; }

	[GridIgnoreProperty]
	public string Product { get; set; }

	[GridIgnoreProperty]
	public decimal? ActiveEmployees { get; set; }

	[GridIgnoreProperty]
	public decimal? CheckedEmployees { get; set; }

	[GridIgnoreProperty]
	public decimal? OrdersInProgress { get; set; }

	[GridIgnoreProperty]
	public decimal? LateWorkOrders { get; set; }

	[GridIgnoreProperty]
	public string Downtime { get; set; }
	public List<Tags> Tags { get; set; }
	public List<AssetKpiMachine> Orders { get; set; }
}

public class Tags
{
	[GridIgnoreProperty]
	public string TagCode { get; set; }
	[GridIgnoreProperty]
	public string TagName { get; set; }
	[GridIgnoreProperty]
	public decimal TagValue { get; set; }
	[GridIgnoreProperty]
	public string TagColor { get; set; }
	[GridIgnoreProperty]
	public bool TagFlicker { get; set; }
	[GridIgnoreProperty]
	public string TagUnit { get; set; }
	[GridIgnoreProperty]
	public string TagIcon { get; set; }
	[GridIgnoreProperty]
	public int TagVisible { get; set; }
	[GridIgnoreProperty]
	public int TagScalar { get; set; }
	[GridIgnoreProperty]
	public string TagDataType { get; set; }
}

public class AssetKpiMachine
{
	public string MachineCode { get; set; }
	public string MachineCapacity { get; set; }
	public Status MachineStatus { get; set; }
	public string OrderCode { get; set; }
	public Status OrderStatus { get; set; }
	public string OperationNo { get; set; }
	public Status OperationStatus { get; set; }
}

public class FacilityMapKpi
{
	public bool HasKpi { get; set; }
	public string Code { get; set; }
	public string Name { get; set; }
	public int Status { get; set; }
	public decimal? Latitude { get; set; }
	public decimal? Longitude { get; set; }

	// KPI
	public decimal OEE { get; set; }

	public decimal Availability { get; set; }
	public decimal Performance { get; set; }
	public decimal Quantity { get; set; }
	public decimal OpenWorkOrders { get; set; }
	public decimal LateWorkOrders { get; set; }
	public decimal WorkingMachines { get; set; }
	public decimal WorkingEmployees { get; set; }
}
