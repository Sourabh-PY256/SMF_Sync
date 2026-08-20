using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.MigrationModels;


/// <sum/// <summary>
///
/// </summary>
public enum BMMComponentype
{
	/// <summary>
	///
	/// </summary>
	Error = 0,
	/// <summary>
	///
	/// </summary>
	RawMaterial = 1,
	/// <summary>
	///
	/// </summary>
	Product = 2
}

/// <summary>
///
/// </summary>
public class BMMComponent
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public Component Original { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public Component PreviousVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string SourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WareHouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string WarehouseId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Sequence { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public BMMComponentype ComponentType { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public UnitType UnitType { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsMaterial { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double LotQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PresentationUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string PresentationUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double PresentationUnitFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMProcess> Processes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMomponentAlternative> Alternatives { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Line { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Tracking { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMomponentAlternative
{
	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public int UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Factor { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMProcessInstruction
{
	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Origin { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMProcess
{
	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessType { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TimeInSeconds { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public double SetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TransformFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public double DestinationFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OutputProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OutputUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string OutputUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool IsOutput { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMComponent> Materials { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMResource> Resources { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMSubproduct> Subproducts { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMLabor> Labor { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMProcessInstruction> Instructions { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMProcessMachine> Machines { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMResource
{
	/// <summary>
	///
	/// </summary>
	public string ResourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsProductionLine { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string MachineId { get; set; }

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
	public string ChangeoverGroup { get; set; }

	/// <summary>
	///
	/// </summary>
	public double CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Efficiency { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMSubproduct
{
	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? Factor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Line { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMProductionOrder
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public ProcessEntry ProcessEntry { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public WorkOrder Original { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductId { get; set; }

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
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMProcess> Processes { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string WarehouseId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public BMMOrderStatus Status { get; set; }

	//public string Status { get; set; }

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
	public decimal SlackTimeAfterLastOperation { get; set; }

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
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Sequence { get; set; }

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
public enum BMMOrderStatus
{
	/// <summary>
	///
	/// </summary>
	Error = 0,
	/// <summary>
	///
	/// </summary>
	Running = 1,
	/// <summary>
	///
	/// </summary>
	New = 2,
	/// <summary>
	///
	/// </summary>
	Deleted = 3,
	/// <summary>
	///
	/// </summary>
	Approved = 4,
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
	Hold = 8
}

/// <summary>
///
/// </summary>
public class BMMLot
{
	/// <summary>
	///
	/// </summary>
	public int Number { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ReplaceOrderLot { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool IsCurrent { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMComponentStock
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public ComponentStockBatch Original { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LotNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string SourceId { get; set; }

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
	public double? Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Status { get; set; } //New - Accepted - Rejected

	/// <summary>
	///
	/// </summary>
	public string OrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

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
public class BMMQualityEntry
{
	/// <summary>
	///
	/// </summary>
	public string ExternalOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string GlobalOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? Quality { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMTransaction
{
	/// <summary>
	///
	/// </summary>
	public string ExternalOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime TransactionDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Employee { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMTransactionComponent> Components { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMTransactionComponent
{
	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

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
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsProduct { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Tracking { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BMMTransactionLot> Lots { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMTransactionLot
{
	/// <summary>
	///
	/// </summary>
	public string LotNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

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
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpDate { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMWorkOrderSupply
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool HasError { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LotNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

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
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMWorkOrderDemand
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool HasError { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DemandDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderLine { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StringAttribute1 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StringAttribute2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StringAttribute3 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TableAttribute1 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TableAttribute3 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TableAttribute2 { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMLabor
{
	/// <summary>
	///
	/// </summary>
	public string LaborId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

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
	public double Cost { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double LaborTimeCost { get; set; }

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
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool HasError { get; set; }
}

/// <summary>
///
/// </summary>
public class OPCProductionOrder
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public GlobalOrder Original { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public ProcessEntry ProcessEntry { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalOrderId { get; set; }

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
	public List<OPCProcess> Processes { get; set; }
}

/// <summary>
///
/// </summary>
public class OPCProcess
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string ProductionLineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ProcessNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ResourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedEnd { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMProductionLine
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string ParentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	public int WorkingTime { get; set; }
	//public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool HasError { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMDevice
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string ProductionLineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MinimumCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool HasError { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMDeviceCapacity
{
	/// <summary>
	///
	/// </summary>
	public string ResourceCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductCode { get; set; }

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
	public string Time { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool HasError { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public BMMCapacityDetail Detail { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMProcessMachine
{
	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

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
	public int SetupTimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OperationTimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	public int WaitingTimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Primary { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool HasError { get; set; }
}

/// <summary>
///
/// </summary>
public class BMMCapacityDetail
{
	/// <summary>
	///
	/// </summary>
	public string MahchineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessEntryId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TimeUnit { get; set; }
}
