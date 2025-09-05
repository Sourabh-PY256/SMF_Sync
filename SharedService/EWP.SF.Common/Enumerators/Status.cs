using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.Enumerators;

/// <summary>
///
/// </summary>
public enum Status
{
	/// <summary>
	///
	/// </summary>
	Empty = 0,
	/// <summary>
	///
	/// </summary>
	Active = 1,
	/// <summary>
	///
	/// </summary>
	Disabled = 2,
	/// <summary>
	///
	/// </summary>
	Deleted = 3,
	/// <summary>
	///
	/// </summary>
	Pending = 4,
	/// <summary>
	///
	/// </summary>
	Queued = 5,
	/// <summary>
	///
	/// </summary>
	Finished = 6,
	/// <summary>
	///
	/// </summary>
	Cancelled = 7,
	/// <summary>
	///
	/// </summary>
	Hold = 8,
	/// <summary>
	///
	/// </summary>
	Failed = 9,
	/// <summary>
	///
	/// </summary>
	Execute = 10
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum Entities
{
	/// <summary>
	///
	/// </summary>
	Facility = 0,
	/// <summary>
	///
	/// </summary>
	Floor = 1,
	/// <summary>
	///
	/// </summary>
	WorkCenter = 2,
	/// <summary>
	///
	/// </summary>
	ProductionLine = 3,
	/// <summary>
	///
	/// </summary>
	Sensor = 4,
	/// <summary>
	///
	/// </summary>
	Inventory = 5,
	/// <summary>
	///
	/// </summary>
	User = 6,
	/// <summary>
	///
	/// </summary>
	ShiftAsset = 7,
	/// <summary>
	///
	/// </summary>
	ShiftStatusAsset = 8,
	/// <summary>
	///
	/// </summary>
	BreakCatalog = 9,
	/// <summary>
	///
	/// </summary>
	DownTimeCategories = 10,
	/// <summary>
	///
	/// </summary>
	Skills = 11,
	/// <summary>
	///
	/// </summary>
	Profiles = 12,
	/// <summary>
	///
	/// </summary>
	Employees = 13,
	/// <summary>
	///
	/// </summary>
	Warehouse = 14,
	/// <summary>
	///
	/// </summary>
	Machine = 15,
	/// <summary>
	///
	/// </summary>
	Item = 16,
	/// <summary>
	///
	/// </summary>
	TagType = 17,
	/// <summary>
	///
	/// </summary>
	Tool = 18,
	/// <summary>
	///
	/// </summary>
	OperationType = 19,
	/// <summary>
	///
	/// </summary>
	ToolType = 20,
	/// <summary>
	///
	/// </summary>
	Procedure = 21,
	/// <summary>
	///
	/// </summary>
	Product = 22,
	/// <summary>
	///
	/// </summary>
	WorkOrder = 23,
	/// <summary>
	///
	/// </summary>
	ShiftEmployee = 24,
	/// <summary>
	///
	/// </summary>
	ShiftStatusEmployee = 25,
	/// <summary>
	///
	/// </summary>
	MeasureUnit = 26,
	/// <summary>
	///
	/// </summary>
	LotSerialStatus = 27,
	/// <summary>
	///
	/// </summary>
	InventoryStatus = 28,
	/// <summary>
	///
	/// </summary>
	StockAllocation = 29,
	/// <summary>
	///
	/// </summary>
	Stock = 30,
	/// <summary>
	///
	/// </summary>
	Supply = 31,
	/// <summary>
	///
	/// </summary>
	Demand = 32,
	/// <summary>
	///
	/// </summary>
	MaterialIssue = 33,
	/// <summary>
	///
	/// </summary>
	MaterialReturn = 34,
	/// <summary>
	///
	/// </summary>
	ProductReceipt = 35,
	/// <summary>
	///
	/// </summary>
	ProductReturn = 36,
	/// <summary>
	///
	/// </summary>
	ProductionOrderChangeStatus = 37,
	/// <summary>
	///
	/// </summary>
	SecondaryContrainstGroup = 38,
	/// <summary>
	///
	/// </summary>
	Attribute = 39,
	/// <summary>
	///
	/// </summary>
	ShortBreak = 40,
	/// <summary>
	///
	/// </summary>
	BinLocation = 41,
	/// <summary>
	///
	/// </summary>
	NotificationsTemplates = 42,
	/// <summary>
	///
	/// </summary>
	NotificationGroup = 43,
	/// <summary>
	///
	/// </summary>
	ManualDowntime = 44,
	/// <summary>
	///
	/// </summary>
	ChangeOverGroup = 45,
	/// <summary>
	///
	/// </summary>
	NotificationPlatforms = 46,
	/// <summary>
	///
	/// </summary>
	NotificationPlatformsDetails = 47,
	/// <summary>
	///
	/// </summary>
	Notifications = 48,
	/// <summary>
	///
	/// </summary>
	Tag = 49,
	/// <summary>
	///
	/// </summary>
	ShopLayout = 50,
	/// <summary>
	///
	/// </summary>
	ManageQuery = 51,
	/// <summary>
	///
	/// </summary>
	Menus = 52,
	/// <summary>
	///
	/// </summary>
	QueryCategory = 53
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum ActionDB
{
	/// <summary>
	///
	/// </summary>
	NA = -1,
	/// <summary>
	///
	/// </summary>
	Create = 0,
	/// <summary>
	///
	/// </summary>
	Update = 1,
	/// <summary>
	///
	/// </summary>
	Delete = 2,
	/// <summary>
	///
	/// </summary>
	IntegrateAll = 99
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum LevelMessage
{
	/// <summary>
	///
	/// </summary>
	Success = 0,
	/// <summary>
	///
	/// </summary>
	Warning = 1,
	/// <summary>
	///
	/// </summary>
	Error = 2
}

/// <summary>
///
/// </summary>
public enum OriginActivity
{
	/// <summary>
	///
	/// </summary>
	Company = 1,
	/// <summary>
	///
	/// </summary>
	Asset = 2,
	/// <summary>
	///
	/// </summary>
	Employee = 3,
	/// <summary>
	///
	/// </summary>
	Operationtype = 4,
	/// <summary>
	///
	/// </summary>
	Product = 5,
	/// <summary>
	///
	/// </summary>
	Order = 6
}

/// <summary>
///
/// </summary>
public enum FrequencyMode
{
	/// <summary>
	///
	/// </summary>
	None = 1,
	/// <summary>
	///
	/// </summary>
	Time = 2,
	/// <summary>
	///
	/// </summary>
	Quantity = 3
}

/// <summary>
///
/// </summary>
public enum EndMode
{
	/// <summary>
	///
	/// </summary>
	EndAfter = 1,
	/// <summary>
	///
	/// </summary>
	EndDate = 2
}

/// <summary>
///
/// </summary>
public enum IntegrationSource
{
	/// <summary>
	///
	/// </summary>
	SF = 1,
	/// <summary>
	///
	/// </summary>
	ERP = 2,
	/// <summary>
	///
	/// </summary>
	APS = 3
}
