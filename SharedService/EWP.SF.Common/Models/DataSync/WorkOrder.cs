
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.Constants;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models.MigrationModels;
using EWP.SF.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.Models;


/// <summary>
///
/// </summary>
public class GlobalOrder
{
	/// <summary>
	///
	/// </summary>
	public int OrderNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessEntryId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductName { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	public DateTime PlannedStartUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TotalQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ReceivedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public int BatchCount { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Progress { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string SerializedEntry { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> ProductionLines { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAllocated { get; set; }

	/// <summary>
	///
	/// </summary>
	public Dictionary<int, WorkOrder> Batches { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool APS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderGroup { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessCell { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OriginalQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StockUOM { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Product { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Operation { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Resource { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RatePerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ConvertedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UOM { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RequiredPartNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RequiredPartPendingQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RequiredPartOriginalQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RequiredPartStockUOM { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Formula { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Rate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SuperBatch { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal SlackTimeBeforeNextOperation { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal SlacktimeAfterLastOperation { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MaxTimeBeforeNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal TransferQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal SlackTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Origin { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Revision { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Warehouse { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SalesOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Cost { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Profit { get; set; }
}

/// <summary>
///
/// </summary>
[GridBDEntityName("ProductionOrder")]
public class WorkOrder : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("SF_Order_Log");

	/// <summary>
	///
	/// </summary>
	public WorkOrder()
	{
	}

	/// <summary>
	///
	/// </summary>
	public WorkOrder(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	[EntityColumn("OrderCode")]
	[GridCustomPropertyName("Order")]
	[GridDrillDown("ProductionOrder")]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
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

	/// <summary>
	///
	/// </summary>
	public int Position { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string PendingOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Lot")]
	public string LotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ProcessEntryId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int OrderNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public double PlannedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool IsCurrent { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ScheduleReady")]
	public bool SchedulingReady { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ReceivedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public double AcceptedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RejectedQty { get; set; }

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
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Lot")]
	public string AuxData { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Status")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	[GridIgnoreProperty]
	public DateTime PlannedStartUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	[GridIgnoreProperty]
	public DateTime RealStartUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ParentWorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Product", null, "JsPath")]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Priority { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<OrderProcess> Processes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<OrderComponent> Components { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SubProduct> Subproducts { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ToolValue> ToolValues { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Tasks { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderMachineAttribute> MachineAttributes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderLabor> Labor { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderTool> Tools { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string JsPath { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string SerializedEntry { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Formula { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Type")]
	public string OrderType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SalesOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Warehouse")]
	public string WarehouseId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> ProductionLines { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool IsAllocated { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string OrderSource { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool APS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderGroup { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool hasTasks { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double LotSize { get; set; }
}

/// <summary>
///
/// </summary>
public class OrderProcess
{
	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessSubTypeId { get; set; }
	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }
	/// <summary>
	///
	/// </summary>
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsOutput { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Output { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Operations { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	public DateTime RealStartUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	public DateTime RealEndUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Total { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Received { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Rejected { get; set; }

	/// <summary>
	///
	/// </summary>
	public double MachineReceived { get; set; }

	/// <summary>
	///
	/// </summary>
	public double MachineRejected { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalMachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status MachineStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status OrderStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool IsUpdated { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? PlannedSetupStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? PlannedSetupEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ExecTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double WaitTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }

	/// <summary>
	///
	/// </summary>
	public double IssuedTime { get; set; }
	/// <summary>
	///
	/// </summary>
	public string Class { get; set; }
}

/// <summary>
///
/// </summary>
public class OrderComponent
{
	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalMachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public ComponentType ComponentType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TargetQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TargetUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double InputQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InputUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAuxiliarDevice { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalSourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double NewFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MaterialImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }

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
	public string ManagedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public double QuantityStage { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RequiredQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MaterialType { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ComponentBatch> Batches { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityInstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool isUpdated { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool isSubProduct { get; set; }

	/// <summary>
	///
	/// </summary>
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

/// <summary>
///
/// </summary>
public class OrderProductTransfer
{
	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalMachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public ComponentType ComponentType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TargetQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TargetUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double InputQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InputUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAuxiliarDevice { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalSourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double NewFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MaterialImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }

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
	public string ManagedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public double QuantityStage { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RequiredQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MaterialType { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ComponentBatchTransfer> Batches { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NewInventoryStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool isUpdated { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool isSubProduct { get; set; }

	/// <summary>
	///
	/// </summary>
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

/// <summary>
///
/// </summary>
public class ToolValue
{
	/// <summary>
	///
	/// </summary>
	//public string ProcessId { get; set; }
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToolId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }
}

/// <summary>
///
/// </summary>
public class RoutineInstance
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsDue { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool RequiresNotifications { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	public DateTime StartDateUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TriggerId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NotificationId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ResultId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DurationSeconds { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsMandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityIdNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ActiveDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TaskId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TaskType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeShiftStatus { get; set; }
}

/// <summary>
///
/// </summary>
public class WokCenterProductionLineMachineFloor
{
	/// <summary>
	///
	/// </summary>
	public string WorkCenterId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkCenter { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Machine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FloorId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Floor { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderMachineAttribute
{
	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Attribute1 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Attribute2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Attribute3 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ChangeOverGroup { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Efficiency { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderLabor
{
	/// <summary>
	///
	/// </summary>
	//public string ProcessId { get; set; }

	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LaborId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TimeInMinutes { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LaborTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double LaborTimeCost { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Cost { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double PlannedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }

	/// <summary>
	///
	/// </summary>
	public double IssuedTime { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderTool
{
	/// <summary>
	///
	/// </summary>
	//public string ProcessId { get; set; }
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToolId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double PlannedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }

	/// <summary>
	///
	/// </summary>
	public double IssuedTime { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderResponse
{
	/// <summary>
	///
	/// </summary>
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
	[JsonIgnoreTransport]
	public WorkOrder WorkOrder { get; set; }

	[JsonIgnoreTransport]
	public ProductionOrder ProductionOrder { get; set; }
	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public List<MessageBroker> WorkOrderMessageList { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public List<Activity> WorkOrderActivityList { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Order Code")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Product Code")]
	[JsonProperty(PropertyName = "ProductCode")]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Version")]
	[JsonProperty(PropertyName = "Version")]
	[Range(1, int.MaxValue, ErrorMessage = "Invalid Version")]
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Sequence")]
	[JsonProperty(PropertyName = "Sequence")]
	[Range(1, int.MaxValue, ErrorMessage = "Invalid Sequence")]
	public int Sequence { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	[Range(1, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Inventory UoM")]
	[JsonProperty(PropertyName = "InventoryUoM")]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Formula Code")]
	[JsonProperty(PropertyName = "FormulaCode")]
	public string FormulaCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Order Type")]
	[JsonProperty(PropertyName = "OrderType")]
	public string OrderType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Lot Number")]
	[JsonProperty(PropertyName = "LotNo")]
	public string LotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Order Group")]
	[JsonProperty(PropertyName = "OrderGroup")]
	public string OrderGroup { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Sales Order")]
	[JsonProperty(PropertyName = "SalesOrder")]
	public string SalesOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Order Priority")]
	[JsonProperty(PropertyName = "OrderPriority")]
	public string OrderPriority { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Comments")]
	[JsonProperty(PropertyName = "Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Due Date")]
	[JsonProperty(PropertyName = "DueDate")]
	public DateTime DueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Planned Start Date")]
	[JsonProperty(PropertyName = "PlannedStartDate")]
	public DateTime PlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Planned End Date")]
	[JsonProperty(PropertyName = "PlannedEndDate")]
	public DateTime PlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("New|In Progress|Released|Queued|Cancelled|On Hold|Finished", ErrorMessage = "Invalid Status")]
	[Description("Order Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public List<WorkOrderOperation> Operations { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderOperation
{
	/// <summary>
	///
	/// </summary>
	[Required]
	public double Step { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public double OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(200)]
	public string OperationName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("Res. Specific Op Time|Res. Specific Batch Time|Res. Specific Rate Per Hour", ErrorMessage = "Invalid Operation Time Type")]
	public string OperationTimeType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string TransferType { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TransferQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SlackTimeBefNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SlackTimeAftNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public double MaxTimeBefNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[JsonIgnoreTransport]
	public string OperationType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string OperationSubtype { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string OutputUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public DateTime PlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public DateTime PlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderMachine> Machines { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderItem> Items { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderByProduct> ByProducts { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderAttribute> Attributes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderOperationLabor> Labor { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderOperationTool> Tooling { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderTask> Tasks { get; set; }

	
}

/// <summary>
///
/// </summary>
public class WorkOrderMachine
{
	/// <summary>
	///
	/// </summary>
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
	public double Eficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int LineNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Manual|Backflush", ErrorMessage = "Invalid Issue Mode")]
	public string IssueMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderMachineTool> Tooling { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<WorkOrderMachineLabor> Labor { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderMachineTool
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
	public int LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
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
	[MaxLength(10)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
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
public class WorkOrderMachineLabor
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
	public int LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
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
	[MaxLength(10)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
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
public class WorkOrderOperationTool
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
	public int LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
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
	[MaxLength(10)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
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
public class WorkOrderOperationLabor
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
	public int LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
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
	[MaxLength(10)]
	[RegularExpression("BOM|Formula", ErrorMessage = "Invalid Source")]
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
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
public class WorkOrderItem
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
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OriginalQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int LineId { get; set; }

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
	public List<WorkOrderAlternativeItem> AlternativeItems { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderAlternativeItem
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
public class WorkOrderByProduct
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
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InventoryUoM { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderAttribute
{
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
public class WorkOrderProcessExternal : ProcedureExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderTask
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
}

/// <summary>
///
/// </summary>
public class WorkOrderProcedure
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ProcedureCode { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderSection
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ProcedureCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string SectionCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string SectionName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string SectionDescription { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderInstruction
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ProcedureCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string InstructionCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string InstructionName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionDescription { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderNotification
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OrderCode { get; set; }

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

/// <summary>
///
/// </summary>
public class ProductionOrderChangeStatusExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Order Code")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("New|In Progress|Released|Queued|Cancelled|On Hold|Finished", ErrorMessage = "Invalid Status")]
	[Description("Order Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderChangeStatus
{
	/// <summary>
	///
	/// </summary>
	public string OrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public BMMOrderStatus Status { get; set; }
}

/// <summary>
///
/// </summary>
public class OperatorsOrder
{
	/// <summary>
	///
	/// </summary>
	public string WorkCenterCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkCenterName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkCenterImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLineName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLineImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineImage { get; set; }

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
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseImage { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	public DateTime PlannedStartDateUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime OperationPlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime OperationPlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public double PlannedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public double AcceptedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OperationPlannedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OperationAcceptedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime UpdateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User UpdateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status OrderStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status OperationStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> ProductionLines { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool HasAllocation { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public bool APS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UoM { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeCheckIn { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AvailableStock { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool DowntimeMachine { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool DowntimeInProgress { get; set; }
}

/// <summary>
///
/// </summary>
public class OperatorsKPIs
{
	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime Value2 { get; set; }
}

/// <summary>
///
/// </summary>
public class OrderOperationSchedule
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Order Code")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Order Planned Start Date")]
	[JsonProperty(PropertyName = "OrderPlannedStartDate")]
	public DateTime OrderPlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Order Planned End Date")]
	[JsonProperty(PropertyName = "OrderPlannedEndDate")]
	public DateTime OrderPlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Operation Number")]
	[JsonProperty(PropertyName = "OperationNo")]
	public double OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Operation Planned Start Date")]
	[JsonProperty(PropertyName = "OperationPlannedStartDate")]
	public DateTime OperationPlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Order Planned End Date")]
	[JsonProperty(PropertyName = "OperationPlannedEndDate")]
	public DateTime OperationPlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Setup Start Date")]
	[JsonProperty(PropertyName = "SetupStartDate")]
	public DateTime SetupStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Setup End Date")]
	[JsonProperty(PropertyName = "SetupEndDate")]
	public DateTime SetupEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Machine Code")]
	[JsonProperty(PropertyName = "MachineCode")]
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationSubtypeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductionOrder : ILoggableEntity

{
	public EntityLoggerConfig EntityLogConfiguration => new("SF_Order_Log");

	/// <summary>
	///
	/// </summary>
	public ProductionOrder()
	{
	}

	/// <summary>
	///
	/// </summary>
	[EntityColumn("OrderCode")]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProductionOrder(WorkOrder oldOrder)
	{
		Code = oldOrder.OrderCode;
		Name = oldOrder.ProductName;
		PlannedStartDate = oldOrder.PlannedStart;
		PlannedEndDate = oldOrder.PlannedEnd;
		ActualStartDate = oldOrder.RealStart.Year > 1900 ? oldOrder.RealStart : null;
		ActualEndDate = oldOrder.RealEnd.Year > 1900 ? oldOrder.RealEnd : null;
		ActualStartDateUTC = oldOrder.RealStartUTC.Year > 1900 ? oldOrder.RealStartUTC : null;
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
		LotSize = oldOrder.LotSize;
		Operations = [];
		foreach (OrderProcess oldOp in oldOrder.Processes)
		{
			ProductionOrderOperation elemOperation = Operations.FirstOrDefault(x => x.OperationNo == oldOp.OperationNo.ToDouble());
			bool isNewOperation = elemOperation is null;
			elemOperation ??= new ProductionOrderOperation
			{
				OperationTypeCode = oldOp.ProcessTypeId,
				OperationSubTypeCode = oldOp.ProcessSubTypeId,
				OperationNo = oldOp.OperationNo.ToDouble(),
				Name = oldOp.OperationName,
				PlannedStartDate = oldOp.PlannedStart,
				PlannedEndDate = oldOp.PlannedEnd,
				ActualStartDate = oldOp.RealStart.Year > 1900 ? oldOp.RealStart : null,
				ActualStartDateUTC = oldOp.RealStartUTC.Year > 1900 ? oldOp.RealStartUTC : null,
				ActualEndDate = oldOp.RealEnd.Year > 1900 ? oldOp.RealEnd : null,
				Quantity = oldOp.Total,
				Status = oldOp.Status,
				Machines = [],
				Class = oldOp.Class,
				Byproducts = oldOrder.Subproducts?.Where(x => x.OperationNo == oldOp.OperationNo).Select(x => new ProductionOrderByProduct
				{
					ItemCode = x.ComponentId,
					Quantity = x.Factor,
					LineId = x.LineId,
					LineUID = x.LineUID,
					WarehouseCode = x.WarehouseCode,
					ReceivedQty = x.Quantity,
					Comments = x.Comments
				}).ToList() ?? [],
				Items = oldOrder.Components?.Where(x => x.OperationNo == oldOp.OperationNo).Select(x => new ProductionOrderItem
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
					Class = x.MaterialType,
					ManagedBy = x.ManagedBy,
				}).ToList() ?? [],
				Labor = oldOrder.Labor?.Where(x => x.OperationNo == oldOp.OperationNo && string.IsNullOrEmpty(x.MachineId)).Select(x => new ProductionOrderResource
				{
					Code = x.LaborId,
					LineId = x.LineId,
					LineUID = x.LineUID,
					PlannedQty = x.PlannedQty,
					Usage = x.Usage,
					Source = x.Source,
					Comments = x.Comments,
					IssuedTime = x.IssuedTime,
					Consumption = x.IsBackflush ? 1 : 0
				}).ToList() ?? [],
				ToolingType = oldOrder.Tools?.Where(x => x.OperationNo == oldOp.OperationNo && string.IsNullOrEmpty(x.MachineId)).Select(x => new ProductionOrderResource
				{
					Code = x.ToolId,
					LineId = x.LineId,
					LineUID = x.LineUID,
					PlannedQty = x.PlannedQty,
					Usage = x.Usage,
					Source = x.Source,
					Comments = x.Comments,
					IssuedTime = x.IssuedTime,
					Consumption = x.IsBackflush ? 1 : 0
				}).ToList() ?? [],
				Tasks = oldOrder.Tasks?.Where(x => x.OperationNo == oldOp.OperationNo).ToList() ?? [],
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
				PlannedSetupTime = oldOp.SetupTime,
				PlannedExecTime = oldOp.ExecTime,
				ActualExecTime = oldOp.IssuedTime,
				ActualSetupTime = 0,
				WaitTime = oldOp.WaitTime,
				Consumption = oldOp.IsBackflush ? 1 : 0,
				Labor = [],
				ToolingType = [],
			};
			elemMachine.Labor = oldOrder.Labor?.Where(x => x.OperationNo == oldOp.OperationNo && x.MachineId == oldOp.MachineId).Select(x => new ProductionOrderResource
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

			elemMachine.ToolingType = oldOrder.Tools?.Where(x => x.OperationNo == oldOp.OperationNo && x.MachineId == oldOp.MachineId).Select(x => new ProductionOrderResource
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

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Lot")]
	public string LotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Lot")]
	public double LotSize { get; set; }
	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ActualStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ActualEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ScheduleReady")]
	public bool SchedulingReady { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ReceivedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public double AcceptedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RejectedQty { get; set; }

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
	public string UnitCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Status")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	[GridIgnoreProperty]
	public DateTime PlannedStartDateUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	[GridIgnoreProperty]
	public DateTime? ActualStartDateUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Product", null, "JsPath")]
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Priority { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductionOrderOperation> Operations { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Formula { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Type")]
	public string OrderType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SalesOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Warehouse")]
	public string Warehouse { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> ProductionLines { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool IsAllocated { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string OrderSource { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool APS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderGroup { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string LogDetailId { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductionOrderOperation
{
	/// <summary>
	///
	/// </summary>
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationSubTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsOutput { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ActualStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ActualEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	public DateTime? ActualStartDateUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	[OffsetIgnore]
	public DateTime? ActualEndDateUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Received { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Rejected { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? PlannedSetupStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? PlannedSetupEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ExecTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double WaitTime { get; set; }
	/// <summary>
	///
	/// </summary>
	public string Class { get; set; }
	/// <summary>
	///
	/// </summary>
	public List<ProductionOrderMachine> Machines { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductionOrderItem> Items { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductionOrderByProduct> Byproducts { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductionOrderResource> Labor { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductionOrderResource> ToolingType { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Tasks { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductionOrderMachine
{
	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalMachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Received { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Rejected { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public double PlannedSetupTime { get; set; }
	public double ActualSetupTime { get; set; }
	/// <summary>
	///
	/// </summary>
	public double PlannedExecTime { get; set; }
	public double ActualExecTime { get; set; }
	/// <summary>
	///
	/// </summary>
	public double WaitTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Consumption { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductionOrderResource> Labor { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductionOrderResource> ToolingType { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductionOrderItem
{
	/// <summary>
	///
	/// </summary>
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double IssuedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

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
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Consumption { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Class { get; set; }
	/// <summary>
	///
	/// </summary>
	public string ManagedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductionOrderResource
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Cost { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }
	/// <summary>
	///
	/// </summary>
	public double PlannedQty { get; set; }
	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Consumption { get; set; }

	/// <summary>
	///
	/// </summary>
	public double IssuedTime { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductionOrderByProduct
{
	/// <summary>
	///
	/// </summary>
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ReceivedQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

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
	public string LineUID { get; set; }
}
