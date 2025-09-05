using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;


/// <summary>
///
/// </summary>
public class ManualOrderProgressContext
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
	public string Machine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderBatchNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Process { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OrderFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OrderNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Rejected { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OrderTotal { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Received { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ActualStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public double EstimatedDuration { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EstimatedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ActualEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime UpdateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime LastRecord { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Skill { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Product { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsOutput { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OutputId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsDown { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDowntime { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DowntimeCreateEmployee { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DowntimeCreateEmployeeName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DowntimeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DowntimesCount { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OutputCount { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IssuedLot { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EntryValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ProductType { get; set; }

	/// <summary>
	///
	/// </summary>
	public object ReceivedSubproducts { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DowntimeCreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DateNowTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ProcessStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DowntimeStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ReportedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ClosedBy { get; set; }
}

/// <summary>
///
/// </summary>
public class ManualOrderProgressRequest
{
	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

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
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartEntry { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndEntry { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Rejected { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Batch { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsPartial { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IssuedLot { get; set; }

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
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SubProduct> Subproducts { get; set; }
}

/// <summary>
///
/// </summary>
public class UpdateOrderProgressToolRequest
{
	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ToolValue> ToolValues { get; set; }
}

/// <summary>
///
/// </summary>
public class UpdateOrderProgressComponentRequest
{
	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<OrderComponent> Components { get; set; }
}

/// <summary>
///
/// </summary>
public class UpdateOrderProgressTransferRequest
{
	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<OrderProductTransfer> Components { get; set; }
}

/// <summary>
///
/// </summary>
public class ManualOrderQualityRequest
{
	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public QualityMode Mode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TestId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SampleId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Sample { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Rejected { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<QualityTestDetail> Details { get; set; }
}

/// <summary>
///
/// </summary>
public class QualityTestDetail
{
	/// <summary>
	///
	/// </summary>
	public string Result { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }
}

/// <summary>
///
/// </summary>
public class ReturnMaterialContext
{
	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalSourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalMachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemImage { get; set; }

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
	public string ComponentImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Lot { get; set; }

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
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

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
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime BatchDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ManagedBy { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductReceived
{
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
	public string OriginalMachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

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
	public bool IsSubproduct { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TempValue { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductReturnIntegration
{
#nullable disable

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
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsSubproduct { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatus { get; set; }
}
