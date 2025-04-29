using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using EWP.SF.Common.Constants;

namespace EWP.SF.ShopFloor.BusinessEntities;

[GridBDEntityName("Item", "Item")]
[GridBDEntityName("Products", "Product")]
public class Component : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_item_log");

	public Component()
	{
	}

	public Component(string id)
	{
		Id = id;
	}

	[GridDrillDown]
	public string Id { get; set; }

	[EntityColumn("ItemCode")]
	[GridDrillDown]
	public string Code { get; set; }

	public string Name { get; set; }

	public string ExternalId { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	public ComponentType ComponentType { get; set; }

	public UnitType UnitType { get; set; }

	public string UnitId { get; set; }

	public ProcessEntry ProcessEntry { get; set; }
	public string ProcessEntryId { get; set; }
	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	// NOTA: Esta lista esta hardcode en front
	[GridLookUpEntity(null, "CustomItemType", "Id", "Name")]
	[GridRequireTranslate]
	public int? Type { get; set; }

	public string Maker { get; set; }

	[GridCustomPropertyName("InventoryGroup")]
	[GridDrillDown("ItemGroup")]
	public string InventoryId { get; set; }

	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	[GridIgnoreProperty]
	public int? ManagedBy { get; set; }

	[GridCustomPropertyName("ManagedBy")]
	public string ManagedByName { get; set; }

	[GridLookUpEntity(null, "MeasureUnit", "Id", "Name")]
	public string UnitInventory { get; set; }

	[GridLookUpEntity(null, "MeasureUnit", "Id", "Name")]
	public string UnitProduction { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	public string Tracking { get; set; }

	[GridDrillDown("Warehouse", "Id")]
	public string WarehouseId { get; set; }

	public List<string> AttachmentIds { get; set; }
	public int Version { get; set; }
	public int SupplyLeadTime { get; set; }
	public double SafetyQty { get; set; }
	public int? Schedule { get; set; }

	public bool ProductSync { get; set; }
	public List<AlternativeComponent> Alternatives { get; set; }
	public List<ComponentUnit> UnitTypes { get; set; }

	public bool IsStock { get; set; }
	public string LogDetailId { get; set; }
	public List<ProductVerions> Versions { get; set; }

	public string UnitTypesToJSON()
	{
		string returnValue = string.Empty;
		if (UnitTypes is not null)
		{
			returnValue = JsonConvert.SerializeObject(UnitTypes);
		}
		return returnValue;
	}
}

public class ProductVerions
{
	public string Code { get; set; }
	public int Version { get; set; }
	public string Warehouse { get; set; }
	public DateTime UpdateDate { get; set; }
	public int Status { get; set; }
}

public class AlternativeComponent
{
	public string ProcessId { get; set; }

	public string ComponentId { get; set; }

	public string SourceId { get; set; }

	public string UnitId { get; set; }

	public int UnitType { get; set; }
	public double Factor { get; set; }

	public string Name { get; set; }
	public string Code { get; set; }

	public string WarehouseCode { get; set; }
	public double Coincidence { get; set; }

	public string ItemUnit { get; set; }

	public string LineId { get; set; }
	public string LineUID { get; set; }
}

public class ComponentBatch
{
	public string BatchId { get; set; }
	public string ComponentId { get; set; }
	public string Batch { get; set; }
	public string Location { get; set; }

	public string InventoryStatus { get; set; }
	public string Pallet { get; set; }
	public double Quantity { get; set; }
	public DateTime BatchDate { get; set; }
	public string BatchStatus { get; set; }

	[JsonIgnore]
	public string ExternalId { get; set; }

	public string OrderId { get; set; }

	public double Allocated { get; set; }
	public string WarehouseCode { get; set; }

	public string LineId { get; set; }

	public bool IsSelected { get; set; }

	public string Type { get; set; }
	public string UoM { get; set; }

	public string WarehouseName { get; set; }
	public string BinLocationName { get; set; }
	public string InventoryStatusName { get; set; }

	public string Comments { get; set; }
	public string ScrapTypeCode { get; set; }
	public string ProcessId { get; set; }
}

public class ComponentBatchTransfer : ComponentBatch
{
	public string NewBinLocation { get; set; }
	public string NewInventoryStatus { get; set; }
}

public class ComponentStockBatch : ComponentBatch
{
	public string ExtId
	{
		get
		{
			return ExternalId;
		}
	}

	public DateTime ERPDate { get; set; }
	public DateTime SubmitDate { get; set; }
}

public class ComponentWarehouse
{
	public string ComponentId { get; set; }
	public string ComponentName { get; set; }
	public string WarehouseName { get; set; }
	public string WarehouseCode { get; set; }
	public double InStock { get; set; }
	public double Commited { get; set; }
	public double Required { get; set; }
	public double Available { get; set; }
	public double Allocated { get; set; }
}

public class ComponentWarehouseDetail
{
	public string ComponentId { get; set; }
	public string ComponentName { get; set; }
	public string LocationCode { get; set; }
	public string LocationName { get; set; }
	public string LotNo { get; set; }
	public string Pallet { get; set; }
	public string BatchName { get; set; }
	public string BatchStatus { get; set; }
	public string BatchStatusName { get; set; }
	public double Quantity { get; set; }
	public double Allocated { get; set; }
	public string InventoryStatusCode { get; set; }
	public string InventoryStatusName { get; set; }

	public string UnitCode { get; set; }
}

public class ComponentAllocationDetail : ComponentWarehouseDetail
{
	public string OrderCode { get; set; }
	public string OperationNo { get; set; }

	public string OperationName { get; set; }
}

public class ComponentUnit
{
	public string ComponentId { get; set; }
	public string UnitFrom { get; set; }
	public string UnitTo { get; set; }
	public string Operation { get; set; }
	public double Factor { get; set; }
}

public class ComponentExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	[RegularExpression(RegularExpression.EntityCode, ErrorMessage = "Invalid Code format.")]
	public string ItemCode { get; set; }

	[MaxLength(200)]
	[Description("Item Name")]
	[JsonProperty(PropertyName = "ItemName")]
	public string ItemName { get; set; }

	[Required]
	[MaxLength(100)]
	[Description("Inventory UoM")]
	[JsonProperty(PropertyName = "InventoryUoM")]
	[DefaultMappingEntity("MeasureUnit")]
	public string InventoryUoM { get; set; }

	[MaxLength(100)]
	[Description("Production UoM")]
	[JsonProperty(PropertyName = "ProductionUoM")]
	[DefaultMappingEntity("MeasureUnit")]
	public string ProductionUoM { get; set; }

	[RegularExpression("Batch|Serial|None", ErrorMessage = "Invalid Managed By")]
	[Description("Managed By")]
	[JsonProperty(PropertyName = "ManagedBy")]
	public string ManagedBy { get; set; }

	[Required]
	[RegularExpression("Purchase|Production", ErrorMessage = "Invalid Type")]
	[Description("Type")]
	[JsonProperty(PropertyName = "Type")]
	public string Type { get; set; }

	[Description("Item Group Code")]
	[JsonProperty(PropertyName = "ItemGroupCode")]
	[DefaultMappingEntity("ItemGroup")]
	public string ItemGroupCode { get; set; }

	[Description("Supply Leadtime")]
	[JsonProperty(PropertyName = "SupplyLeadtime")]
	public int SupplyLeadTime { get; set; }

	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Item Group Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	[Description("Safety Quantity")]
	[JsonProperty(PropertyName = "SafetyQuantity")]
	public double? SafetyQuantity { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Status")]
	[Description("Sync Product")]
	[JsonProperty(PropertyName = "SyncProduction")]
	public string SyncProduction { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Status")]
	[Description("Schedule")]
	[JsonProperty(PropertyName = "Schedule")]
	public string Schedule { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Status")]
	[Description("Stock")]
	[JsonProperty(PropertyName = "Stock")]
	public string Stock { get; set; }
}

public class ProductExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Product Code")]
	[JsonProperty(PropertyName = "ProductCode")]
	[RegularExpression(RegularExpression.EntityCode, ErrorMessage = "Invalid Code format.")]
	public string ProductCode { get; set; }

	[MaxLength(200)]
	[Description("Product Name")]
	[JsonProperty(PropertyName = "ProductName")]
	public string ProductName { get; set; }

	[Key]
	[Required]
	[Description("Version")]
	[JsonProperty(PropertyName = "Version")]
	public int Version { get; set; }

	[Key]
	[Required]
	[Description("Sequence")]
	[JsonProperty(PropertyName = "Sequence")]
	public int Sequence { get; set; }

	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	[Required]
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	[Required]
	[Description("Inventory UoM")]
	[JsonProperty(PropertyName = "InventoryUoM")]
	public string InventoryUoM { get; set; }

	[Description("Formula Code")]
	[JsonProperty(PropertyName = "FormulaCode")]
	public string FormulaCode { get; set; }

	[RegularExpression(RegularExpression.ProductStatusIntegration, ErrorMessage = "Invalid Status")]
	[Description("Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	[Description("Comments")]
	public string Comments { get; set; }

	[Description("Schedule")]
	[MaxLength(3)]
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	[Description("BomVersion")]
	public string BomVersion { get; set; }

	[Description("BomSequence")]
	public string BomSequence { get; set; }

	[Description("RouteVersion")]
	public string RouteVersion { get; set; }

	[Description("RouteSequence")]
	public string RouteSequence { get; set; }

	[Required]
	[MinLength(1, ErrorMessage = "Operations must contain at least one element")]
	public List<ProductOperation> Operations { get; set; }
}

public class ProductOperation
{
	[Key]
	[Required]
	public double OperationNo { get; set; }

	[Key]
	[MaxLength(100)]
	public string OperationSubtype { get; set; }

	[JsonIgnoreTransport]
	public string OperationType { get; set; }

	[MaxLength(100)]
	[RegularExpression("SpecificOpTime|SpecificRatePerHour|SpecificBatchTime", ErrorMessage = "Invalid OperationTimeType")]
	public string OperationTimeType { get; set; }

	[MaxLength(100)]
	[RegularExpression("AfterTransfer|AfterComplete", ErrorMessage = "Invalid TransferType")]
	public string TransferType { get; set; }

	public double? TransferQuantity { get; set; }

	public double? SlackTimeBefNextOp { get; set; }

	public double? SlackTimeAftNextOp { get; set; }

	public double? MaxTimeBefNextOp { get; set; }
	public double? MaxOpSpanIncrease { get; set; }

	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	[MaxLength(100)]
	public string OutputUoM { get; set; }

	public List<ProductMachine> OperationMachines { get; set; }

	public List<ProductOperationTool> OperationTools { get; set; }

	public List<ProductOperationLabor> OperationLabor { get; set; }

	public List<ProductOperationItem> OperationItems { get; set; }

	public List<ProductOperationAlternativeItem> OperationAlternativeItems { get; set; }

	public List<ProductOperationByProduct> OperationByProducts { get; set; }

	public List<ProductAttribute> Attributes { get; set; }

	public List<ProductTask> Tasks { get; set; }
}

public class ProductMachine
{
	[Key]
	[MaxLength(100)]
	public string MachineCode { get; set; }

	public double SetupTimeInSec { get; set; }

	public double OperationTimeInSec { get; set; }

	public double WaitingTimeInSec { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Primary")]
	public string Primary { get; set; }

	public List<ProductOperationTool> MachineTools { get; set; }

	public List<ProductOperationLabor> MachineLabor { get; set; }

	public string LineID { get; set; }
	public string LineUID { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid AutomaticSequencing")]
	public string AutomaticSequencing { get; set; }

	[Description("Comments")]
	public string Comments { get; set; }

	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

public class ProductMachineTool
{
	[Required]
	[MaxLength(100)]
	public string ToolingCode { get; set; }

	[Required]
	public int LineID { get; set; }

	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	[Description("Comments")]
	public string Comments { get; set; }
}

public class ProductMachineLabor
{
	[Required]
	[MaxLength(100)]
	public string ProfileCode { get; set; }

	[Required]
	public int LineID { get; set; }

	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	[Description("Comments")]
	public string Comments { get; set; }
}

public class ProductOperationTool
{
	[Required]
	[MaxLength(100)]
	public string ToolingCode { get; set; }

	[Required]
	public int LineID { get; set; }

	public string LineUID { get; set; }

	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	[MaxLength(100)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	[Description("Comments")]
	public string Comments { get; set; }

	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

public class ProductOperationLabor
{
	[Required]
	[MaxLength(100)]
	public string ProfileCode { get; set; }

	[Required]
	public int LineID { get; set; }

	public string LineUID { get; set; }

	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	[MaxLength(100)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	[Description("Comments")]
	public string Comments { get; set; }

	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

public class ProductOperationItem
{
	[Required]
	[MaxLength(100)]
	public string ItemCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string WarehouseCode { get; set; }

	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	[MaxLength(200)]
	public string InventoryUoM { get; set; }

	[Required]
	public int LineID { get; set; }

	public string LineUID { get; set; }

	[MaxLength(100)]
	[RegularExpression("Manual|Backflush|Operation Issue", ErrorMessage = "Invalid Issue Method")]
	public string IssueMethod { get; set; }

	[MaxLength(100)]
	[RegularExpression("Material|Consumable", ErrorMessage = "Invalid Type")]
	public string Type { get; set; }

	[MaxLength(100)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	[Description("Comments")]
	public string Comments { get; set; }

	[MaxLength(3)]
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schdule")]
	public string Schedule { get; set; }
}

public class ProductOperationAlternativeItem
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string ItemCode { get; set; }

	[Required]
	public double Quantity { get; set; }

	[Required]
	[MaxLength(100)]
	public string WarehouseCode { get; set; }

	[Required]
	public double Coincidence { get; set; }
}

public class ProductOperationByProduct
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string ItemCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string WarehouseCode { get; set; }

	[Required]
	public double Quantity { get; set; }

	[Required]
	public int LineID { get; set; }

	[Required]
	public string LineUID { get; set; }

	[MaxLength(200)]
	public string InventoryUoM { get; set; }

	[Description("Comments")]
	public string Comments { get; set; }
}

public class ProductAttribute
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string AttributeTypeCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string AttributeCode { get; set; }
}

public class ProductProcessExternal : ProcedureExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }
}

public class ProductTask
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	[Required]
	public int Sort { get; set; }

	[Required]
	[MaxLength(100)]
	public string Name { get; set; }

	[MaxLength(200)]
	[Required]
	public string Description { get; set; }

	[Required]
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Mandatory")]
	[DefaultMappingEntity("Mandatory")]
	public string Mandatory { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Available")]
	[Required]
	public string Available { get; set; }

	[Required]
	public double DurationInSec { get; set; }

	[RegularExpression("Production|Maintenance|Quality|General", ErrorMessage = "Invalid Class")]
	[DefaultMappingEntity("Class")]
	[Required]
	public string Class { get; set; }

	[MaxLength(100)]
	[RegularExpression("Break|Corrective Maintenance|Holidays|Lunch|OtherDowntime|Predictive Maintenance|Preventive Maintenance|Vacation", ErrorMessage = "Invalid Class")]
	public string Type { get; set; }

	[RegularExpression("Calibration|Cleaning|Configuration|Fault search|Inspection|Painting|Reforms|Reparation|Substitution|Tests", ErrorMessage = "Invalid Class")]
	public string InterventionCode { get; set; }

	[RegularExpression("Company experience|External database|Failure analysis|Generic works|Generic works|Manufacturers recommendation|Responsibility Assignment (RAM)|Root Cause Analysis (RCM)", ErrorMessage = "Invalid Source")]
	public string SourceCode { get; set; }

	[RegularExpression("Start|During|End", ErrorMessage = "Invalid Stage")]
	[Required]
	public string Stage { get; set; }

	[MaxLength(100)]
	public string ProcedureCode { get; set; }

	public int Version { get; set; }
	public string FrequencyMode { get; set; }
	public double FreqValue { get; set; }

	public List<TasksMaterials> TasksMaterials { get; set; }
	//public List<ProductProcessExternal> Procedures { get; set; }

	// public ProductProcessExternal Procedure { get; set; }
}

public class TasksMaterials
{
	[Key]
	[Required]
	[MaxLength(100)]
	public int SectionOrder { get; set; }

	[Required]
	public int Sort { get; set; }

	[Required]
	public string ItemCode { get; set; }

	[Required]
	[DefaultMappingEntity("QuantityPercentage")]
	public decimal QuantityPercentage { get; set; }

	[DefaultMappingEntity("Tolerance")]
	public decimal Tolerance { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsRemainingTotal")]
	[DefaultMappingEntity("IsRemainingTotal")]
	public string IsRemainingTotal { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Mandatory")]
	[DefaultMappingEntity("Mandatory")]
	public string Mandatory { get; set; }
}

public class ProductNotification
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	[MaxLength(100)]
	public string NotificationCode { get; set; }

	[MaxLength(100)]
	public string NotificationType { get; set; }
}
