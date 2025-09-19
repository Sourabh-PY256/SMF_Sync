using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.Constants;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json;
using EWP.SF.Common.EntityLogger;


namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
[GridBDEntityName("Item", "Item")]
[GridBDEntityName("Products", "Product")]
public class Component : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_item_log");

	/// <summary>
	///
	/// </summary>
	public Component()
	{
	}

	/// <summary>
	///
	/// </summary>
	public Component(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ItemCode")]
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public ComponentType ComponentType { get; set; }

	/// <summary>
	///
	/// </summary>
	public UnitType UnitType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProcessEntry ProcessEntry { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessEntryId { get; set; }

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

	// NOTA: Esta lista esta hardcode en front
	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "CustomItemType", "Id", "Name")]
	[GridRequireTranslate]
	public int? Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Maker { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("InventoryGroup")]
	[GridDrillDown("ItemGroup")]
	public string InventoryId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	[GridIgnoreProperty]
	public int? ManagedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ManagedBy")]
	public string ManagedByName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "MeasureUnit", "Id", "Name")]
	public string UnitInventory { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "MeasureUnit", "Id", "Name")]
	public string UnitProduction { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Tracking { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Warehouse", "Id")]
	public string WarehouseId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SupplyLeadTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SafetyQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool ProductSync { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<AlternativeComponent> Alternatives { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ComponentUnit> UnitTypes { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsStock { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductVerions> Versions { get; set; }

	/// <summary>
	///
	/// </summary>
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

/// <summary>
///
/// </summary>
public class ProductVerions
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Warehouse { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime UpdateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }
}

/// <summary>
///
/// </summary>
public class AlternativeComponent
{
	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UnitType { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Factor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Coincidence { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentBatch
{
	/// <summary>
	///
	/// </summary>
	public string BatchId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Batch { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime BatchDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Allocated { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsSelected { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UoM { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BinLocationName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ScrapTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentBatchTransfer : ComponentBatch
{
	/// <summary>
	///
	/// </summary>
	public string NewBinLocation { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NewInventoryStatus { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentStockBatch : ComponentBatch
{
	/// <summary>
	///
	/// </summary>
	public string ExtId
	{
		get
		{
			return ExternalId;
		}
	}

	/// <summary>
	///
	/// </summary>
	public DateTime ERPDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime SubmitDate { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentWarehouse
{
	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double InStock { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Commited { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Required { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Available { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Allocated { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentWarehouseDetail
{
	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LocationName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchStatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Allocated { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentAllocationDetail : ComponentWarehouseDetail
{
	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationName { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentUnit
{
	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitFrom { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitTo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Operation { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Factor { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	[RegularExpression(RegularExpression.EntityCode, ErrorMessage = "Invalid Code format.")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Item Name")]
	[JsonProperty(PropertyName = "ItemName")]
	public string ItemName { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Inventory UoM")]
	[JsonProperty(PropertyName = "InventoryUoM")]
	[DefaultMappingEntity("MeasureUnit")]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Production UoM")]
	[JsonProperty(PropertyName = "ProductionUoM")]
	[DefaultMappingEntity("MeasureUnit")]
	public string ProductionUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Batch|Serial|None", ErrorMessage = "Invalid Managed By")]
	[Description("Managed By")]
	[JsonProperty(PropertyName = "ManagedBy")]
	public string ManagedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[RegularExpression("Purchase|Production", ErrorMessage = "Invalid Type")]
	[Description("Type")]
	[JsonProperty(PropertyName = "Type")]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Item Group Code")]
	[JsonProperty(PropertyName = "ItemGroupCode")]
	[DefaultMappingEntity("ItemGroup")]
	public string ItemGroupCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Supply Leadtime")]
	[JsonProperty(PropertyName = "SupplyLeadtime")]
	public int SupplyLeadTime { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Item Group Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Safety Quantity")]
	[JsonProperty(PropertyName = "SafetyQuantity")]
	public double? SafetyQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Status")]
	[Description("Sync Product")]
	[JsonProperty(PropertyName = "SyncProduction")]
	public string SyncProduction { get; set; }

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
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Status")]
	[Description("Stock")]
	[JsonProperty(PropertyName = "Stock")]
	public string Stock { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Product Code")]
	[JsonProperty(PropertyName = "ProductCode")]
	[RegularExpression(RegularExpression.EntityCode, ErrorMessage = "Invalid Code format.")]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Product Name")]
	[JsonProperty(PropertyName = "ProductName")]
	public string ProductName { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[Description("Version")]
	[JsonProperty(PropertyName = "Version")]
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[Description("Sequence")]
	[JsonProperty(PropertyName = "Sequence")]
	public int Sequence { get; set; }

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
	[Required]
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Inventory UoM")]
	[JsonProperty(PropertyName = "InventoryUoM")]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Formula Code")]
	[JsonProperty(PropertyName = "FormulaCode")]
	public string FormulaCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression(RegularExpression.ProductStatusIntegration, ErrorMessage = "Invalid Status")]
	[Description("Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Schedule")]
	[MaxLength(3)]
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("BomVersion")]
	public string BomVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("BomSequence")]
	public string BomSequence { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("RouteVersion")]
	public string RouteVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("RouteSequence")]
	public string RouteSequence { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MinLength(1, ErrorMessage = "Operations must contain at least one element")]
	public List<ProductOperation> Operations { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductOperation
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public double OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	public string OperationSubtype { get; set; }
	

	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string OperationType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("SpecificOpTime|SpecificRatePerHour|SpecificBatchTime", ErrorMessage = "Invalid OperationTimeType")]
	public string OperationTimeType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("AfterTransfer|AfterComplete", ErrorMessage = "Invalid TransferType")]
	public string TransferType { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? TransferQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? SlackTimeBefNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? SlackTimeAftNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? MaxTimeBefNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? MaxOpSpanIncrease { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string OutputUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string OperationName { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductMachine> OperationMachines { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductOperationTool> OperationTools { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductOperationLabor> OperationLabor { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductOperationItem> OperationItems { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductOperationAlternativeItem> OperationAlternativeItems { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductOperationByProduct> OperationByProducts { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductAttribute> Attributes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductTask> Tasks { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductMachine
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SetupTimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OperationTimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	public double WaitingTimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Primary")]
	public string Primary { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductOperationTool> MachineTools { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductOperationLabor> MachineLabor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid AutomaticSequencing")]
	public string AutomaticSequencing { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductMachineTool
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ToolingCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Comments")]
	public string Comments { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductMachineLabor
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ProfileCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Comments")]
	public string Comments { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductOperationTool
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ToolingCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductOperationLabor
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ProfileCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schedule")]
	public string Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductOperationItem
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("Manual|Backflush|Operation Issue", ErrorMessage = "Invalid Issue Method")]
	public string IssueMethod { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("Material|Consumable", ErrorMessage = "Invalid Type")]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(3)]
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Schdule")]
	public string Schedule { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductOperationAlternativeItem
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public double Coincidence { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductOperationByProduct
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Comments")]
	public string Comments { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductAttribute
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string AttributeTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string AttributeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductProcessExternal : ProcedureExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductTask
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int Sort { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Required]
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Mandatory")]
	[DefaultMappingEntity("Mandatory")]
	public string Mandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Available")]
	[Required]
	public string Available { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public double DurationInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Production|Maintenance|Quality|General", ErrorMessage = "Invalid Class")]
	[DefaultMappingEntity("Class")]
	[Required]
	public string Class { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("Break|Corrective Maintenance|Holidays|Lunch|OtherDowntime|Predictive Maintenance|Preventive Maintenance|Vacation", ErrorMessage = "Invalid Class")]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Calibration|Cleaning|Configuration|Fault search|Inspection|Painting|Reforms|Reparation|Substitution|Tests", ErrorMessage = "Invalid Class")]
	public string InterventionCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Company experience|External database|Failure analysis|Generic works|Generic works|Manufacturers recommendation|Responsibility Assignment (RAM)|Root Cause Analysis (RCM)", ErrorMessage = "Invalid Source")]
	public string SourceCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Start|During|End", ErrorMessage = "Invalid Stage")]
	[Required]
	public string Stage { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string ProcedureCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FrequencyMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double FreqValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<TasksMaterials> TasksMaterials { get; set; }
	//public List<ProductProcessExternal> Procedures { get; set; }

	// public ProductProcessExternal Procedure { get; set; }
}

/// <summary>
///
/// </summary>
public class TasksMaterials
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public int SectionOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int Sort { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[DefaultMappingEntity("QuantityPercentage")]
	public decimal QuantityPercentage { get; set; }

	/// <summary>
	///
	/// </summary>
	[DefaultMappingEntity("Tolerance")]
	public decimal Tolerance { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid IsRemainingTotal")]
	[DefaultMappingEntity("IsRemainingTotal")]
	public string IsRemainingTotal { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Mandatory")]
	[DefaultMappingEntity("Mandatory")]
	public string Mandatory { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductNotification
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string NotificationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string NotificationType { get; set; }
}
