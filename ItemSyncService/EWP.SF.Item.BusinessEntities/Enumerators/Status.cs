using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.Item.BusinessEntities;

public enum Status
{
	Empty = 0,
	Active = 1,
	Disabled = 2,
	Deleted = 3,
	Pending = 4,
	Queued = 5,
	Finished = 6,
	Cancelled = 7,
	Hold = 8,
	Failed = 9,
	Execute = 10
}

[JsonConverter(typeof(StringEnumConverter))]
public enum Entities
{
	Facility = 0,
	Floor = 1,
	Workcenter = 2,
	ProductionLine = 3,
	Sensor = 4,
	Inventory = 5,
	User = 6,
	ShiftAsset = 7,
	ShiftStatusAsset = 8,
	BreakCatalog = 9,
	DownTimeCategories = 10,
	Skills = 11,
	Profiles = 12,
	Employees = 13,
	Warehouse = 14,
	Machine = 15,
	Item = 16,
	TagType = 17,
	Tool = 18,
	OperationType = 19,
	ToolType = 20,
	Procedure = 21,
	Product = 22,
	WorkOrder = 23,
	ShiftEmployee = 24,
	ShiftStatusEmployee = 25,
	MeasureUnit = 26,
	LotSerialStatus = 27,
	InventoryStatus = 28,
	StockAllocation = 29,
	Stock = 30,
	Supply = 31,
	Demand = 32,
	MaterialIssue = 33,
	MaterialReturn = 34,
	ProductReceipt = 35,
	ProductReturn = 36,
	ProductionOrderChangeStatus = 37,
	SecondaryContrainstGroup = 38,
	Attribute = 39,
	ShortBreak = 40,
	BinLocation = 41,
	NotificationsTemplates = 42,
	NotificationGroup = 43,
	ManualDowntime = 44,
	ChangeOverGroup = 45,
	NotificationPlatforms = 46,
	NotificationPlatformsDetails = 47,
	Notifications = 48,
	Tag = 49,
	ShopLayout = 50,
	ManageQuery = 51,
	Menus = 52
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ActionDB
{
	NA = -1,
	Create = 0,
	Update = 1,
	Delete = 2,
	IntegrateAll = 99
}

[JsonConverter(typeof(StringEnumConverter))]
public enum LevelMessage
{
	Success = 0,
	Warning = 1,
	Error = 2
}

public enum OriginActivity
{
	Company = 1,
	Asset = 2,
	Employee = 3,
	Operationtype = 4,
	Product = 5,
	Order = 6
}

public enum FrequencyMode
{
	None = 1,
	Time = 2,
	Quantity = 3
}

public enum EndMode
{
	EndAfter = 1,
	EndDate = 2
}

public enum IntegrationSource
{
	SF = 1,
	ERP = 2,
	APS = 3
}
