
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.Models;
public class GlobalOrder
{
	public int OrderNumber { get; set; }
	public string Id { get; set; }
	public string ExternalId { get; set; }

	public string OrderCode { get; set; }
	public string ProcessEntryId { get; set; }
	public string ProductName { get; set; }

	[OffsetIgnore]
	public DateTime PlannedStartUTC { get; set; }

	public DateTime PlannedStart { get; set; }
	public DateTime PlannedEnd { get; set; }

	public DateTime RealStart { get; set; }

	public DateTime RealEnd { get; set; }

	public double TotalQty { get; set; }

	public string Comments { get; set; }

	public double ReceivedQty { get; set; }
	public int BatchCount { get; set; }
	public double Progress { get; set; }
	public Status Status { get; set; }

	[GridIgnoreProperty]
	public string SerializedEntry { get; set; }

	public List<string> ProductionLines { get; set; }
	public bool IsAllocated { get; set; }
	public Dictionary<int, WorkOrder> Batches { get; set; }

	[JsonIgnore]
	public bool APS { get; set; }

	public string OrderGroup { get; set; }
	public string ProcessCell { get; set; }
	public double OriginalQty { get; set; }
	public string StockUOM { get; set; }
	public string Product { get; set; }
	public string Operation { get; set; }
	public string Resource { get; set; }
	public double RatePerHour { get; set; }

	public double ConvertedQty { get; set; }
	public string UOM { get; set; }
	public string RequiredPartNo { get; set; }
	public double RequiredPartPendingQuantity { get; set; }
	public double RequiredPartOriginalQty { get; set; }
	public string RequiredPartStockUOM { get; set; }

	public string Formula { get; set; }
	public double Rate { get; set; }

	public string SuperBatch { get; set; }
	public decimal SlackTimeBeforeNextOperation { get; set; }

	public decimal SlacktimeAfterLastOperation { get; set; }

	public decimal MaxTimeBeforeNextOp { get; set; }
	public decimal TransferQuantity { get; set; }
	public decimal SlackTime { get; set; }

	public string Origin { get; set; }
	public string BatchStatus { get; set; }
	public string Revision { get; set; }
	public string Warehouse { get; set; }
	public string SalesOrder { get; set; }
	public double Cost { get; set; }
	public double Profit { get; set; }
}

[GridBDEntityName("ProductionOrder")]
public class WorkOrder : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("SF_Order_Log");

	public WorkOrder()
	{
	}

	public WorkOrder(string id)
	{
		Id = id;
	}

	[EntityColumn("OrderCode")]
	[GridCustomPropertyName("Order")]
	[GridDrillDown("ProductionOrder")]
	public string Id { get; set; }

	public WorkOrder(WorkOrder global)
	{
		Id = global.Id;
		AcceptedQty = global.AcceptedQty;
		LotNo = global.LotNo;
		Comments = global.Comments;
		if (global.Comments is not null)
		{
			Components = [.. global.Components];
		}
		IsCurrent = global.IsCurrent;
		Id = global.Id;
		ExternalId = global.ExternalId;
		OrderCode = global.OrderCode;
		ProductCode = global.ProductCode;
		OrderNumber = global.OrderNumber;
		ParentWorkOrderId = global.ParentWorkOrderId;
		PendingOrder = global.PendingOrder;

		ProcessEntryId = global.ProcessEntryId;
		PlannedEnd = global.PlannedEnd;
		PlannedStart = global.PlannedStart;
		Status = global.Status;
		PlannedQty = global.PlannedQty;
		if (global.Processes is not null)
		{
			Processes = [.. global.Processes];
		}
		if (global.Tasks is not null)
		{
			Tasks = [.. global.Tasks];
		}
		if (global.ToolValues is not null)
		{
			ToolValues = [.. global.ToolValues];
		}
		SerializedEntry = global.SerializedEntry;
	}

	public int Position { get; set; }

	[GridIgnoreProperty]
	public string PendingOrder { get; set; }

	public string ProductName { get; set; }

	[GridIgnoreProperty]
	public string ExternalId { get; set; }

	[GridCustomPropertyName("Lot")]
	public string LotNo { get; set; }

	[GridIgnoreProperty]
	public string ProcessEntryId { get; set; }

	[GridIgnoreProperty]
	public int OrderNumber { get; set; }

	public DateTime PlannedStart { get; set; }
	public DateTime PlannedEnd { get; set; }
	public double PlannedQty { get; set; }
	public DateTime RealStart { get; set; }
	public DateTime RealEnd { get; set; }
	public DateTime DueDate { get; set; }

	[GridIgnoreProperty]
	public bool IsCurrent { get; set; }

	[GridCustomPropertyName("ScheduleReady")]
	public bool SchedulingReady { get; set; }

	public double ReceivedQty { get; set; }
	public double AcceptedQty { get; set; }
	public double RejectedQty { get; set; }
	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	public string UnitId { get; set; }

	[GridCustomPropertyName("Lot")]
	public string AuxData { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Status")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	[OffsetIgnore]
	[GridIgnoreProperty]
	public DateTime PlannedStartUTC { get; set; }

	[OffsetIgnore]
	[GridIgnoreProperty]
	public DateTime RealStartUTC { get; set; }

	[GridIgnoreProperty]
	public string ParentWorkOrderId { get; set; }

	[GridDrillDown("Product", null, "JsPath")]
	public string ProductCode { get; set; }

	public string Priority { get; set; }
	public List<OrderProcess> Processes { get; set; }
	public List<OrderComponent> Components { get; set; }
	public List<SubProduct> Subproducts { get; set; }
	public List<ToolValue> ToolValues { get; set; }
	public List<Activity> Tasks { get; set; }

	public List<WorkOrderMachineAttribute> MachineAttributes { get; set; }
	public List<WorkOrderLabor> Labor { get; set; }
	public List<WorkOrderTool> Tools { get; set; }

	[GridIgnoreProperty]
	public string JsPath { get; set; }

	[GridIgnoreProperty]
	public string SerializedEntry { get; set; }

	public string Comments { get; set; }

	public string Formula { get; set; }

	[GridCustomPropertyName("Type")]
	public string OrderType { get; set; }

	public string SalesOrder { get; set; }

	[GridIgnoreProperty]
	public string OrderCode { get; set; }

	[GridDrillDown("Warehouse")]
	public string WarehouseId { get; set; }

	public int Version { get; set; }
	public List<string> ProductionLines { get; set; }

	[GridIgnoreProperty]
	public bool IsAllocated { get; set; }

	[GridIgnoreProperty]
	public string OrderSource { get; set; }

	[GridIgnoreProperty]
	public bool APS { get; set; }

	public string OrderGroup { get; set; }

	[GridIgnoreProperty]
	public bool hasTasks { get; set; }

	[GridIgnoreProperty]
	public string LogDetailId { get; set; }
}

public class OrderProcess
{
	public int Step { get; set; }
	public string ProcessTypeId { get; set; }
	public string ProcessSubTypeId { get; set; }
	public string ProcessId { get; set; }
	public string ProductionLineId { get; set; }
	public string MachineId { get; set; }
	public bool IsOutput { get; set; }
	public string Output { get; set; }
	public string OperationName { get; set; }
	public string Operations { get; set; }
	public DateTime PlannedStart { get; set; }
	public DateTime PlannedEnd { get; set; }
	public DateTime RealStart { get; set; }
	public DateTime RealEnd { get; set; }

	[OffsetIgnore]
	public DateTime RealStartUTC { get; set; }

	[OffsetIgnore]
	public DateTime RealEndUTC { get; set; }

	public double Total { get; set; }
	public double Received { get; set; }
	public double Rejected { get; set; }
	public double MachineReceived { get; set; }
	public double MachineRejected { get; set; }
	public string OriginalMachineId { get; set; }
	public Status Status { get; set; }
	public Status MachineStatus { get; set; }
	public Status OrderStatus { get; set; }

	public string LineId { get; set; }
	public string LineUID { get; set; }

	[JsonIgnore]
	public bool IsUpdated { get; set; }

	public DateTime? PlannedSetupStart { get; set; }

	public DateTime? PlannedSetupEnd { get; set; }
	public string Comments { get; set; }
	public string OrderCode { get; set; }
	public double SetupTime { get; set; }
	public double ExecTime { get; set; }
	public double WaitTime { get; set; }

	public bool IsBackflush { get; set; }
	public double IssuedTime { get; set; }
}

public class OrderComponent
{
	public string MachineId { get; set; }

	public string OriginalMachineId { get; set; }
	public int Step { get; set; }
	public string ProcessTypeId { get; set; }
	public ComponentType ComponentType { get; set; }
	public string SourceTypeId { get; set; }
	public string SourceId { get; set; }
	public string BatchId { get; set; }
	public double TargetQty { get; set; }
	public string TargetUnitId { get; set; }
	public double InputQty { get; set; }
	public string InputUnitId { get; set; }
	public string ProcessId { get; set; }
	public bool IsAuxiliarDevice { get; set; }
	public Status Status { get; set; }
	public string OriginalSourceId { get; set; }
	public double NewFactor { get; set; }
	public string ExternalId { get; set; }
	public string WarehouseCode { get; set; }

	public string ComponentName { get; set; }
	public string ComponentCode { get; set; }
	public string MaterialImage { get; set; }
	public string LineId { get; set; }

	public string LineUID { get; set; }
	public bool IsBackflush { get; set; }

	public string Location { get; set; }
	public string InventoryStatus { get; set; }
	public string ManagedBy { get; set; }
	public double QuantityStage { get; set; }
	public double RequiredQuantity { get; set; }
	public string Source { get; set; }
	public int MaterialType { get; set; }
	public List<ComponentBatch> Batches { get; set; }
	public string Comments { get; set; }

	public string ActivityInstanceId { get; set; }

	[JsonIgnore]
	public bool isUpdated { get; set; }

	[JsonIgnore]
	public bool isSubProduct { get; set; }

	[JsonIgnore]
	public string BatchesJson
	{
		get
		{
			string returnValue = string.Empty;
			if (Batches?.Count > 0)
			{
				returnValue = JsonConvert.SerializeObject(Batches, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			}
			return returnValue;
		}
	}
}

public class OrderProductTransfer
{
	public string MachineId { get; set; }

	public string OriginalMachineId { get; set; }
	public int Step { get; set; }
	public string ProcessTypeId { get; set; }
	public ComponentType ComponentType { get; set; }
	public string SourceTypeId { get; set; }
	public string SourceId { get; set; }
	public string BatchId { get; set; }
	public double TargetQty { get; set; }
	public string TargetUnitId { get; set; }
	public double InputQty { get; set; }
	public string InputUnitId { get; set; }
	public string ProcessId { get; set; }
	public bool IsAuxiliarDevice { get; set; }
	public Status Status { get; set; }
	public string OriginalSourceId { get; set; }
	public double NewFactor { get; set; }
	public string ExternalId { get; set; }
	public string WarehouseCode { get; set; }

	public string ComponentName { get; set; }
	public string ComponentCode { get; set; }
	public string MaterialImage { get; set; }
	public string LineId { get; set; }

	public string LineUID { get; set; }
	public bool IsBackflush { get; set; }

	public string Location { get; set; }
	public string InventoryStatus { get; set; }
	public string ManagedBy { get; set; }
	public double QuantityStage { get; set; }
	public double RequiredQuantity { get; set; }
	public string Source { get; set; }
	public int MaterialType { get; set; }
	public List<ComponentBatchTransfer> Batches { get; set; }

	public string NewInventoryStatus { get; set; }

	[JsonIgnore]
	public bool isUpdated { get; set; }

	[JsonIgnore]
	public bool isSubProduct { get; set; }

	[JsonIgnore]
	public string BatchesJson
	{
		get
		{
			string returnValue = string.Empty;
			if (Batches?.Count > 0)
			{
				returnValue = JsonConvert.SerializeObject(Batches, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			}
			return returnValue;
		}
	}
}

public class ToolValue
{
	public string ProcessId { get; set; }
	public int Step { get; set; }
	public string MachineId { get; set; }
	public string ToolId { get; set; }
	public string Code { get; set; }
	public string Value { get; set; }
}

public class RoutineInstance
{
	public string Id { get; set; }
	public string WorkOrderId { get; set; }
	public string ProcessId { get; set; }
	public string MachineId { get; set; }
	public string ActivityId { get; set; }

	public string Title { get; set; }
	public bool IsDue { get; set; }
	public bool RequiresNotifications { get; set; }

	[OffsetIgnore]
	public DateTime StartDateUTC { get; set; }

	public DateTime StartDate { get; set; }
	public double Quantity { get; set; }
	public int TriggerId { get; set; }
	public string NotificationId { get; set; }
	public string ResultId { get; set; }

	public int DurationSeconds { get; set; }
	public Status Status { get; set; }
	public bool IsMandatory { get; set; }
	public string ActivityIdNo { get; set; }
	public string ActivityTypeCode { get; set; }
	public DateTime ActiveDate { get; set; }
	public DateTime PlannedDate { get; set; }
	public string TaskId { get; set; }
	public string TaskType { get; set; }
	public string CodeShiftStatus { get; set; }
}

public class WokCenterProductionLineMachineFloor
{
	public string WorkCenterId { get; set; }
	public string WorkCenter { get; set; }
	public string ProductionLineId { get; set; }
	public string ProductionLine { get; set; }
	public string MachineId { get; set; }
	public string Machine { get; set; }
	public string FloorId { get; set; }
	public string Floor { get; set; }
}

public class WorkOrderMachineAttribute
{
	public string ProcessId { get; set; }
	public string MachineId { get; set; }

	public double CostPerHour { get; set; }
	public string Attribute1 { get; set; }
	public string Attribute2 { get; set; }
	public string Attribute3 { get; set; }

	public string ChangeOverGroup { get; set; }

	public double Efficiency { get; set; }
}

public class WorkOrderLabor
{
	public string ProcessId { get; set; }
	public string LaborId { get; set; }
	public string LineId { get; set; }
	public string LineUID { get; set; }
	public string Name { get; set; }
	public double TimeInMinutes { get; set; }

	public string LaborTime { get; set; }
	public double LaborTimeCost { get; set; }
	public double Cost { get; set; }

	public double Quantity { get; set; }
	public double PlannedQty { get; set; }
	public string MachineId { get; set; }
	public string Source { get; set; }
	public string Comments { get; set; }

	public string Usage { get; set; }

	public bool IsBackflush { get; set; }
	public double IssuedTime { get; set; }
}

public class WorkOrderTool
{
	public string ProcessId { get; set; }
	public string ToolId { get; set; }
	public string LineId { get; set; }
	public string LineUID { get; set; }
	public double Quantity { get; set; }
	public double PlannedQty { get; set; }
	public string MachineId { get; set; }

	public string Source { get; set; }
	public string Comments { get; set; }
	public string Usage { get; set; }

	public bool IsBackflush { get; set; }
	public double IssuedTime { get; set; }
}

public class WorkOrderResponse
{
	public string Id { get; set; }
	public string Code { get; set; }
	public bool IsSuccess { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public ActionDB Action { get; set; }

	public string Message { get; set; }

	[JsonIgnoreTransport]
	public WorkOrder WorkOrder { get; set; }

	[JsonIgnoreTransport]
	public List<MessageBroker> WorkOrderMessageList { get; set; }

	[JsonIgnoreTransport]
	public List<Activity> WorkOrderActivityList { get; set; }
}

public class WorkOrderExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Order Code")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	[Required]
	[MaxLength(100)]
	[Description("Product Code")]
	[JsonProperty(PropertyName = "ProductCode")]
	public string ProductCode { get; set; }

	[Required]
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	[Required]
	[Description("Version")]
	[JsonProperty(PropertyName = "Version")]
	[Range(1, int.MaxValue, ErrorMessage = "Invalid Version")]
	public int Version { get; set; }

	[Required]
	[Description("Sequence")]
	[JsonProperty(PropertyName = "Sequence")]
	[Range(1, int.MaxValue, ErrorMessage = "Invalid Sequence")]
	public int Sequence { get; set; }

	[Required]
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	[Range(1, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
	public double Quantity { get; set; }

	[MaxLength(100)]
	[Description("Inventory UoM")]
	[JsonProperty(PropertyName = "InventoryUoM")]
	public string InventoryUoM { get; set; }

	[MaxLength(100)]
	[Description("Formula Code")]
	[JsonProperty(PropertyName = "FormulaCode")]
	public string FormulaCode { get; set; }

	[MaxLength(100)]
	[Description("Order Type")]
	[JsonProperty(PropertyName = "OrderType")]
	public string OrderType { get; set; }

	[MaxLength(100)]
	[Description("Lot Number")]
	[JsonProperty(PropertyName = "LotNo")]
	public string LotNo { get; set; }

	[MaxLength(100)]
	[Description("Order Group")]
	[JsonProperty(PropertyName = "OrderGroup")]
	public string OrderGroup { get; set; }

	[MaxLength(100)]
	[Description("Sales Order")]
	[JsonProperty(PropertyName = "SalesOrder")]
	public string SalesOrder { get; set; }

	[Description("Order Priority")]
	[JsonProperty(PropertyName = "OrderPriority")]
	public string OrderPriority { get; set; }

	[MaxLength(200)]
	[Description("Comments")]
	[JsonProperty(PropertyName = "Comments")]
	public string Comments { get; set; }

	[Description("Due Date")]
	[JsonProperty(PropertyName = "DueDate")]
	public DateTime DueDate { get; set; }

	[Description("Planned Start Date")]
	[JsonProperty(PropertyName = "PlannedStartDate")]
	public DateTime PlannedStartDate { get; set; }

	[Description("Planned End Date")]
	[JsonProperty(PropertyName = "PlannedEndDate")]
	public DateTime PlannedEndDate { get; set; }

	[RegularExpression("New|In Progress|Released|Queued|Cancelled|On Hold|Finished", ErrorMessage = "Invalid Status")]
	[Description("Order Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	[Required]
	public List<WorkOrderOperation> Operations { get; set; }
}

public class WorkOrderOperation
{
	[Required]
	public double Step { get; set; }

	[Required]
	[MaxLength(200)]
	public string OperationName { get; set; }

	[MaxLength(100)]
	[RegularExpression("Res. Specific Op Time|Res. Specific Batch Time|Res. Specific Rate Per Hour", ErrorMessage = "Invalid Operation Time Type")]
	public string OperationTimeType { get; set; }

	[MaxLength(100)]
	public string TransferType { get; set; }

	public double TransferQuantity { get; set; }

	public double SlackTimeBefNextOp { get; set; }

	public double SlackTimeAftNextOp { get; set; }

	public double MaxTimeBefNextOp { get; set; }

	[MaxLength(100)]
	[JsonIgnoreTransport]
	public string OperationType { get; set; }

	[MaxLength(100)]
	public string OperationSubtype { get; set; }

	public double Quantity { get; set; }

	[MaxLength(100)]
	public string OutputUoM { get; set; }

	[Required]
	public DateTime PlannedStartDate { get; set; }

	[Required]
	public DateTime PlannedEndDate { get; set; }

	public List<WorkOrderMachine> Machines { get; set; }

	public List<WorkOrderItem> Items { get; set; }

	public List<WorkOrderByProduct> ByProducts { get; set; }

	public List<WorkOrderAttribute> Attributes { get; set; }

	public List<WorkOrderOperationLabor> Labor { get; set; }

	public List<WorkOrderOperationTool> Tooling { get; set; }
	public List<WorkOrderTask> Tasks { get; set; }
}

public class WorkOrderMachine
{
	[MaxLength(100)]
	public string MachineCode { get; set; }

	public double SetupTimeInSec { get; set; }

	public double OperationTimeInSec { get; set; }

	public double WaitingTimeInSec { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Primary")]
	public string Primary { get; set; }

	public double Eficiency { get; set; }
	public int Quantity { get; set; }
	public int LineNo { get; set; }
	public string LineUID { get; set; }

	public string Comments { get; set; }

	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
	public List<WorkOrderMachineTool> Tooling { get; set; }
	public List<WorkOrderMachineLabor> Labor { get; set; }
}

public class WorkOrderMachineTool
{
	[Required]
	[MaxLength(100)]
	public string ToolingCode { get; set; }

	[Required]
	public int LineId { get; set; }

	public string LineUID { get; set; }

	[Required]
	public double Quantity { get; set; }

	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	[MaxLength(10)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	public string Comments { get; set; }

	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

public class WorkOrderMachineLabor
{
	[Required]
	[MaxLength(100)]
	public string ProfileCode { get; set; }

	[Required]
	public int LineId { get; set; }

	public string LineUID { get; set; }

	[Required]
	public double Quantity { get; set; }

	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	[MaxLength(10)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	public string Comments { get; set; }
	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

public class WorkOrderOperationTool
{
	[Required]
	[MaxLength(100)]
	public string ToolingCode { get; set; }

	[Required]
	public int LineId { get; set; }

	public string LineUID { get; set; }

	[Required]
	public double Quantity { get; set; }

	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	[MaxLength(10)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	public string Comments { get; set; }
	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

public class WorkOrderOperationLabor
{
	[Required]
	[MaxLength(100)]
	public string ProfileCode { get; set; }

	[Required]
	public int LineId { get; set; }

	public string LineUID { get; set; }

	[Required]
	public double Quantity { get; set; }

	[MaxLength(200)]
	[RegularExpression(RegularExpression.UsageRegex, ErrorMessage = "Invalid Usage value")]
	public string Usage { get; set; }

	[MaxLength(10)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	public string Comments { get; set; }
	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }
}

public class WorkOrderItem
{
	[Required]
	[MaxLength(100)]
	public string ItemCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string WarehouseCode { get; set; }

	[Required]
	public double Quantity { get; set; }

	public double OriginalQuantity { get; set; }

	[MaxLength(200)]
	public string InventoryUoM { get; set; }

	[Required]
	public int LineId { get; set; }

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

	public List<WorkOrderAlternativeItem> AlternativeItems { get; set; }
	public string Comments { get; set; }
}

public class WorkOrderAlternativeItem
{
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

public class WorkOrderByProduct
{
	[Required]
	[MaxLength(100)]
	public string ItemCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string WarehouseCode { get; set; }

	[Required]
	public double Quantity { get; set; }

	[Required]
	public int LineId { get; set; }

	public string LineUID { get; set; }

	[MaxLength(200)]
	public string InventoryUoM { get; set; }

	public string Comments { get; set; }
}

public class WorkOrderAttribute
{
	[Required]
	[MaxLength(100)]
	public string AttributeTypeCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string AttributeCode { get; set; }
}

public class WorkOrderProcessExternal : ProcedureExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }
}

public class WorkOrderTask
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
}

public class WorkOrderProcedure
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string ProcedureCode { get; set; }
}

public class WorkOrderSection
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string ProcedureCode { get; set; }

	[MaxLength(100)]
	public string SectionCode { get; set; }

	[MaxLength(100)]
	public string SectionName { get; set; }

	[MaxLength(200)]
	public string SectionDescription { get; set; }
}

public class WorkOrderInstruction
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }

	[Required]
	[MaxLength(100)]
	public string ProcedureCode { get; set; }

	[MaxLength(100)]
	public string InstructionCode { get; set; }

	[MaxLength(100)]
	public string InstructionName { get; set; }

	[MaxLength(200)]
	public string InstructionDescription { get; set; }
}

public class WorkOrderNotification
{
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }

	[MaxLength(100)]
	public string NotificationCode { get; set; }

	[MaxLength(100)]
	public string NotificationType { get; set; }
}

public class ProductionOrderChangeStatusExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Order Code")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	[RegularExpression("New|In Progress|Released|Queued|Cancelled|On Hold|Finished", ErrorMessage = "Invalid Status")]
	[Description("Order Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }
}

public class WorkOrderChangeStatus
{
	public string OrderId { get; set; }

	public string OrderCode { get; set; }

	public BMMOrderStatus Status { get; set; }
}

public class OperatorsOrder
{
	public string WorkcenterCode { get; set; }
	public string WorkcenterName { get; set; }
	public string WorkcenterImage { get; set; }
	public string ProductionLineCode { get; set; }
	public string ProductionLineName { get; set; }
	public string ProductionLineImage { get; set; }
	public string MachineCode { get; set; }
	public string MachineName { get; set; }
	public string MachineImage { get; set; }
	public Status MachineStatus { get; set; }
	public string OrderCode { get; set; }
	public string OperationNo { get; set; }
	public string OperationName { get; set; }
	public string ProductId { get; set; }
	public string ProductCode { get; set; }
	public string ProductName { get; set; }
	public string ProductImage { get; set; }
	public string WarehouseCode { get; set; }
	public string WarehouseName { get; set; }
	public string WarehouseImage { get; set; }

	[OffsetIgnore]
	public DateTime PlannedStartDateUTC { get; set; }

	public DateTime PlannedStartDate { get; set; }
	public DateTime PlannedEndDate { get; set; }
	public DateTime OperationPlannedStartDate { get; set; }
	public DateTime OperationPlannedEndDate { get; set; }
	public DateTime RealStartDate { get; set; }
	public DateTime RealEndDate { get; set; }
	public double PlannedQty { get; set; }
	public double AcceptedQty { get; set; }
	public double OperationPlannedQty { get; set; }
	public double OperationAcceptedQty { get; set; }
	public DateTime CreateDate { get; set; }
	public User CreateUser { get; set; }
	public DateTime UpdateDate { get; set; }
	public User UpdateUser { get; set; }
	public Status OrderStatus { get; set; }
	public Status OperationStatus { get; set; }
	public List<string> ProductionLines { get; set; }
	public bool HasAllocation { get; set; }

	[JsonIgnoreTransport]
	public bool APS { get; set; }

	public string UoM { get; set; }
	public string EmployeeCheckIn { get; set; }
	public string AvailableStock { get; set; }
	public bool DowntimeMachine { get; set; }
	public bool DowntimeInProgress { get; set; }
}

public class OperatorsKPIs
{
	public string Type { get; set; }
	public string Value { get; set; }
	public DateTime Value2 { get; set; }
}

public class OrderOperationSchedule
{
	[Required]
	[MaxLength(100)]
	[Description("Order Code")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	[Required]
	[Description("Order Planned Start Date")]
	[JsonProperty(PropertyName = "OrderPlannedStartDate")]
	public DateTime OrderPlannedStartDate { get; set; }

	[Required]
	[Description("Order Planned End Date")]
	[JsonProperty(PropertyName = "OrderPlannedEndDate")]
	public DateTime OrderPlannedEndDate { get; set; }

	[Required]
	[Description("Operation Number")]
	[JsonProperty(PropertyName = "OperationNo")]
	public double OperationNo { get; set; }

	[Required]
	[Description("Operation Planned Start Date")]
	[JsonProperty(PropertyName = "OperationPlannedStartDate")]
	public DateTime OperationPlannedStartDate { get; set; }

	[Required]
	[Description("Order Planned End Date")]
	[JsonProperty(PropertyName = "OperationPlannedEndDate")]
	public DateTime OperationPlannedEndDate { get; set; }

	[Description("Setup Start Date")]
	[JsonProperty(PropertyName = "SetupStartDate")]
	public DateTime SetupStartDate { get; set; }

	[Description("Setup End Date")]
	[JsonProperty(PropertyName = "SetupEndDate")]
	public DateTime SetupEndDate { get; set; }

	[Required]
	[Description("Machine Code")]
	[JsonProperty(PropertyName = "MachineCode")]
	public string MachineCode { get; set; }

	public string OperationSubtypeCode { get; set; }
}

public class ProductionOrder
{
	public ProductionOrder()
	{
	}
	public string Code { get; set; }

	public ProductionOrder(WorkOrder oldOrder)
	{
		Code = oldOrder.OrderCode;
		PlannedStartDate = oldOrder.PlannedStart;
		PlannedEndDate = oldOrder.PlannedEnd;
		ActualStartDate = oldOrder.RealStart.Year > 1900 ? oldOrder.RealStart : (DateTime?)null;
		ActualEndDate = oldOrder.RealEnd.Year > 1900 ? oldOrder.RealEnd : (DateTime?)null;
		ActualStartDateUTC = oldOrder.RealStartUTC.Year > 1900 ? oldOrder.RealStartUTC : (DateTime?)null;
		Quantity = oldOrder.PlannedQty;
		DueDate = oldOrder.DueDate;
		OrderType = oldOrder.OrderType;
		SchedulingReady = oldOrder.SchedulingReady;
		Status = oldOrder.Status;
		OrderGroup = oldOrder.OrderGroup;
		ProductId = oldOrder.ProcessEntryId;
		ProductCode = oldOrder.ProductCode;
		ReceivedQty = oldOrder.ReceivedQty;
		AcceptedQty = oldOrder.AcceptedQty;
		CreationDate = oldOrder.CreationDate;
		CreatedBy = oldOrder.CreatedBy;
		UnitCode = oldOrder.UnitId;
		ModifiedBy = oldOrder.ModifiedBy;
		ModifyDate = oldOrder.ModifyDate;
		Priority = oldOrder.Priority;
		Comments = oldOrder.Comments;
		Formula = oldOrder.Formula;
		SalesOrder = oldOrder.SalesOrder;
		Warehouse = oldOrder.WarehouseId;
		Version = oldOrder.Version;
		IsAllocated = oldOrder.IsAllocated;
		OrderSource = oldOrder.OrderSource;
		APS = oldOrder.APS;
		LogDetailId = oldOrder.LogDetailId;
		Operations = [];
		foreach (OrderProcess oldOp in oldOrder.Processes)
		{
			ProductionOrderOperation elemOperation = Operations.FirstOrDefault(x => x.OperationNo == oldOp.ProcessId.ToDouble());
			bool isNewOperation = elemOperation is null;
			elemOperation = elemOperation ?? new ProductionOrderOperation
			{
				OperationTypeCode = oldOp.ProcessTypeId,
				OperationSubTypeCode = oldOp.ProcessSubTypeId,
				OperationNo = oldOp.ProcessId.ToDouble(),
				Name = oldOp.OperationName,
				PlannedStartDate = oldOp.PlannedStart,
				PlannedEndDate = oldOp.PlannedEnd,
				ActualStartDate = oldOp.RealStart.Year > 1900 ? oldOp.RealStart : (DateTime?)null,
				ActualStartDateUTC = oldOp.RealStartUTC.Year > 1900 ? oldOp.RealStartUTC : (DateTime?)null,
				ActualEndDate = oldOp.RealEnd.Year > 1900 ? oldOp.RealEnd : (DateTime?)null,
				Quantity = oldOp.Total,
				Status = oldOp.Status,
				Machines = [],
				Items = oldOrder.Components?.Where(x => x.ProcessId == oldOp.ProcessId).Select(x => new ProductionOrderItem
				{
					ItemCode = x.SourceId,
					OriginalItemCode = x.OriginalSourceId,
					LineId = x.LineId,
					LineUID = x.LineUID,
					Quantity = x.TargetQty,
					UnitCode = x.TargetUnitId,
					Source = x.Source,
					Comments = x.Comments,
					IssuedQty = x.InputQty,
					Consumption = x.IsBackflush ? 1 : 0,
					Status = x.Status,
					WarehouseCode = x.WarehouseCode,
					MaterialType = x.MaterialType
				}).ToList() ?? [],
				Labor = oldOrder.Labor?.Where(x => x.ProcessId == oldOp.ProcessId && String.IsNullOrEmpty(x.MachineId)).Select(x => new ProductionOrderResource
				{
					Code = x.LaborId,
					LineId = x.LineId,
					LineUID = x.LineUID,
					Quantity = x.Quantity,
					Usage = x.Usage,
					Source = x.Source,
					Comments = x.Comments,
					IssuedTime = x.IssuedTime,
					Consumption = x.IsBackflush ? 1 : 0
				}).ToList() ?? [],
				ToolingType = oldOrder.Tools?.Where(x => x.ProcessId == oldOp.ProcessId && String.IsNullOrEmpty(x.MachineId)).Select(x => new ProductionOrderResource
				{
					Code = x.ToolId,
					LineId = x.LineId,
					LineUID = x.LineUID,
					Quantity = x.Quantity,
					Usage = x.Usage,
					Source = x.Source,
					Comments = x.Comments,
					IssuedTime = x.IssuedTime,
					Consumption = x.IsBackflush ? 1 : 0
				}).ToList() ?? [],
				Tasks = oldOrder.Tasks?.Where(x => x.ProcessId == oldOp.ProcessId).ToList() ?? [],
				ExecTime = oldOp.ExecTime,
				SetupTime = oldOp.SetupTime,
				WaitTime = oldOp.WaitTime,
				Received = oldOp.Received,
				Rejected = oldOp.Rejected,
				PlannedSetupStart = oldOp.PlannedSetupStart,
				PlannedSetupEnd = oldOp.PlannedSetupEnd,
				Comments = oldOp.Comments,
			};
			ProductionOrderMachine elemMachine = new()
			{
				MachineCode = oldOp.MachineId,
				OriginalMachineCode = oldOp.OriginalMachineId,
				LineId = oldOp.LineId,
				LineUID = oldOp.LineUID,
				Received = oldOp.MachineReceived,
				Rejected = oldOp.MachineRejected,
				Status = oldOp.MachineStatus,
				SetupTime = oldOp.SetupTime,
				ExecTime = oldOp.IssuedTime,
				WaitTime = oldOp.WaitTime,
				Consumption = oldOp.IsBackflush ? 1 : 0,
				Labor = [],
				ToolingType = [],
			};
			elemMachine.Labor = oldOrder.Labor?.Where(x => x.ProcessId == oldOp.ProcessId && x.MachineId == oldOp.MachineId).Select(x => new ProductionOrderResource
			{
				Code = x.LaborId,
				LineId = x.LineId,
				LineUID = x.LineUID,
				Quantity = x.Quantity,
				Usage = x.Usage,
				Source = x.Source,
				Comments = x.Comments,
				IssuedTime = x.IssuedTime,
				Consumption = x.IsBackflush ? 1 : 0,
			}).ToList() ?? [];

			elemMachine.ToolingType = oldOrder.Tools?.Where(x => x.ProcessId == oldOp.ProcessId && x.MachineId == oldOp.MachineId).Select(x => new ProductionOrderResource
			{
				Code = x.ToolId,
				LineId = x.LineId,
				LineUID = x.LineUID,
				Quantity = x.Quantity,
				Usage = x.Usage,
				Source = x.Source,
				Comments = x.Comments,
				IssuedTime = x.IssuedTime,
				Consumption = x.IsBackflush ? 1 : 0
			}).ToList() ?? [];

			elemOperation.Machines.Add(elemMachine);

			if (isNewOperation)
			{
				Operations.Add(elemOperation);
			}
		}
	}

	[GridCustomPropertyName("Lot")]
	public string LotNo { get; set; }

	[GridIgnoreProperty]
	public string ProductId { get; set; }
	public DateTime PlannedStartDate { get; set; }
	public DateTime PlannedEndDate { get; set; }
	public double Quantity { get; set; }
	public DateTime? ActualStartDate { get; set; }
	public DateTime? ActualEndDate { get; set; }
	public DateTime DueDate { get; set; }

	[GridCustomPropertyName("ScheduleReady")]
	public bool SchedulingReady { get; set; }

	public double ReceivedQty { get; set; }
	public double AcceptedQty { get; set; }
	public double RejectedQty { get; set; }
	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	public string UnitCode { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Status")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	[OffsetIgnore]
	[GridIgnoreProperty]
	public DateTime PlannedStartDateUTC { get; set; }

	[OffsetIgnore]
	[GridIgnoreProperty]
	public DateTime? ActualStartDateUTC { get; set; }
	[GridDrillDown("Product", null, "JsPath")]
	public string ProductCode { get; set; }

	public string Priority { get; set; }
	public List<ProductionOrderOperation> Operations { get; set; }

	public string Comments { get; set; }

	public string Formula { get; set; }

	[GridCustomPropertyName("Type")]
	public string OrderType { get; set; }

	public string SalesOrder { get; set; }

	[GridIgnoreProperty]
	public string OrderCode { get; set; }

	[GridDrillDown("Warehouse")]
	public string Warehouse { get; set; }

	public int Version { get; set; }
	public List<string> ProductionLines { get; set; }

	[GridIgnoreProperty]
	public bool IsAllocated { get; set; }

	[GridIgnoreProperty]
	public string OrderSource { get; set; }

	[GridIgnoreProperty]
	public bool APS { get; set; }

	public string OrderGroup { get; set; }

	[GridIgnoreProperty]
	public string LogDetailId { get; set; }
}

public class ProductionOrderOperation
{
	public string OperationTypeCode { get; set; }
	public string OperationSubTypeCode { get; set; }
	public double OperationNo { get; set; }
	public bool IsOutput { get; set; }
	public string Name { get; set; }
	public DateTime PlannedStartDate { get; set; }
	public DateTime PlannedEndDate { get; set; }
	public DateTime? ActualStartDate { get; set; }
	public DateTime? ActualEndDate { get; set; }

	[OffsetIgnore]
	public DateTime? ActualStartDateUTC { get; set; }

	[OffsetIgnore]
	public DateTime? ActualEndDateUTC { get; set; }

	public double Quantity { get; set; }
	public double Received { get; set; }
	public double Rejected { get; set; }

	public Status Status { get; set; }

	public DateTime? PlannedSetupStart { get; set; }

	public DateTime? PlannedSetupEnd { get; set; }
	public string Comments { get; set; }

	public double SetupTime { get; set; }
	public double ExecTime { get; set; }
	public double WaitTime { get; set; }
	public List<ProductionOrderMachine> Machines { get; set; }
	public List<ProductionOrderItem> Items { get; set; }
	public List<ProductionOrderItem> Byproducts { get; set; }
	public List<ProductionOrderResource> Labor { get; set; }
	public List<ProductionOrderResource> ToolingType { get; set; }
	public List<Activity> Tasks { get; set; }
}

public class ProductionOrderMachine
{
	public string MachineCode { get; set; }
	public string OriginalMachineCode { get; set; }
	public string LineId { get; set; }
	public string LineUID { get; set; }

	public string ProductionLineCode { get; set; }
	public double Received { get; set; }
	public double Rejected { get; set; }
	public Status Status { get; set; }
	public double SetupTime { get; set; }
	public double ExecTime { get; set; }
	public double WaitTime { get; set; }
	public int Consumption { get; set; }
	public List<ProductionOrderResource> Labor { get; set; }
	public List<ProductionOrderResource> ToolingType { get; set; }
}

public class ProductionOrderItem
{
	public string ItemCode { get; set; }
	public string OriginalItemCode { get; set; }
	public double Quantity { get; set; }
	public string UnitCode { get; set; }
	public double IssuedQty { get; set; }
	public Status Status { get; set; }
	public string WarehouseCode { get; set; }
	public string LineId { get; set; }
	public string LineUID { get; set; }
	public int Consumption { get; set; }
	public string Source { get; set; }
	public int MaterialType { get; set; }
	public string Comments { get; set; }
}

public class ProductionOrderResource
{
	public string Code { get; set; }
	public string LineId { get; set; }
	public string LineUID { get; set; }
	public double Time { get; set; }
	public double Cost { get; set; }
	public double Quantity { get; set; }
	public string Source { get; set; }
	public string Comments { get; set; }

	public string Usage { get; set; }

	public int Consumption { get; set; }
	public double IssuedTime { get; set; }
}
