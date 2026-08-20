using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

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

/// <summary>
///
/// </summary>
public class TransactionMaterialSyncRequest
{
	/// <summary>
	///
	/// </summary>
	[JsonProperty("sf_order_transactions_material")]
	public List<OrderTransactionMaterialSync> OrderTransactionsMaterial { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty("sf_order_transactions_material_detail")]
	public List<OrderTransactionMaterialDetailSync> OrderTransactionsMaterialDetail { get; set; }
}

public class TransactionRequest
{
    public string TransactionId { get; set; }
    public string ExternalId { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; }
    public string OrderBatch { get; set; }
    public List<ComponentTransaction> Components { get; set; }
    public string Comments { get; set; }
    public string Employee { get; set; }
}

public class ComponentTransaction
{
    public string OrderCode { get; set; }
    public string OrderType { get; set; }
    public string OperationCode { get; set; }
    public string MachineId { get; set; }
    public string OriginalMachineId { get; set; }
    public int Step { get; set; }
    public int? ProcessTypeId { get; set; }
    public int ComponentType { get; set; }
    public int? SourceTypeId { get; set; }
    public string SourceId { get; set; }
    public string BatchId { get; set; }
    public double TargetQty { get; set; }
    public string TargetUnitId { get; set; }
    public double InputQty { get; set; }
    public string InputUnitId { get; set; }
    public string ProcessId { get; set; }
    public string OperationId { get; set; }
    public bool IsAuxiliarDevice { get; set; }
    public bool IsInlineIssue { get; set; }
    public int Status { get; set; }
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
    public List<BatchTransaction> Batches { get; set; }
    public string Comments { get; set; }
    public string ActivityInstanceId { get; set; }
}

public class BatchTransaction
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



/// <summary>
///
/// </summary>
public class OrderTransactionMaterialSync
{
	public string OperationNo { get; set; }
	public string OrderCode { get; set; }
	public string LineNo { get; set; }
	public double Quantity { get; set; }
	public string EmployeeId { get; set; }
	public int UserId { get; set; }
	public string TransactionId { get; set; }
	public string Comments { get; set; }
	public string ExternalId { get; set; }
	public string ExternalDate { get; set; }
}

/// <summary>
///
/// </summary>
public class OrderTransactionMaterialDetailSync
{
	public string TransactionId { get; set; }
	public string ItemCode { get; set; }
	public double Quantity { get; set; }
	public string LotNumber { get; set; }
	public string Pallet { get; set; }
	public string BinLocationCode { get; set; }
	public string InventoryStatusCode { get; set; }
	public string ExpDate { get; set; }
	public string WarehouseCode { get; set; }
	public string Type { get; set; }
	public string MachineCode { get; set; }
	public string OriginalItem { get; set; }
	public string LineNo { get; set; }
	public string OrderLot { get; set; }
}

/// <summary>
/// 
/// </summary>
public class TransactionProductSyncRequest
{
    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("sf_order_transactions_product")]
    public List<OrderTransactionProductSync> OrderTransactionsProduct { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("sf_order_transactions_product_detail")]
    public List<OrderTransactionProductDetailSync> OrderTransactionsProductDetail { get; set; }
}

public class TransactionProductReceiptSyncRequest
{
    public string TransactionId { get; set; }
    public string ExternalId { get; set; }
    public string ExternalComponentId { get; set; }
    public int OperationNo { get; set; }
    public DateTime Date { get; set; }

    public string Batch { get; set; }
    public string Pallet { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rejected { get; set; }

    public List<CustomData> CustomData { get; set; }
    public List<Product> Products { get; set; }

    public string Comments { get; set; }
    public string Employee { get; set; }
    public string Location { get; set; }
    public string InventoryStatus { get; set; }
}

public class CustomData
{
    public string Key { get; set; }
    public string? Value { get; set; } // nullable (you have null values)
}

public class Product
{
    public string ItemCode { get; set; }
    public string LineId { get; set; }
    public string LineType { get; set; }
    public string Warehouse { get; set; }
    public decimal Quantity { get; set; }

    public List<Lot> Lots { get; set; }
}

public class Lot
{
    public string? BatchId { get; set; }
    public string? ComponentId { get; set; }
    public string Batch { get; set; }
    public string Location { get; set; }
    public string InventoryStatus { get; set; }
    public string Pallet { get; set; }
    public decimal Quantity { get; set; }

    public DateTime BatchDate { get; set; }

    public string? BatchStatus { get; set; }
    public string? OrderId { get; set; }
    public decimal Allocated { get; set; }
    public string WarehouseCode { get; set; }
    public string LineId { get; set; }
    public bool IsSelected { get; set; }

    public string? Type { get; set; }
    public string? UoM { get; set; }
    public string? WarehouseName { get; set; }
    public string? BinLocationName { get; set; }
    public string? InventoryStatusName { get; set; }
    public string? Comments { get; set; }
    public string? ScrapTypeCode { get; set; }
    public string? ProcessId { get; set; }
}

/// <summary>
/// 
/// </summary>
public class OrderTransactionProductSync
{
    public string TransactionId { get; set; }
    public string OperationNo { get; set; }
    public string OrderCode { get; set; }
    public DateTime StartEntryDate { get; set; }
    public DateTime EndEntryDate { get; set; }
    public double Quantity { get; set; }
    public double OrderFactor { get; set; }
    public double ProcessFactor { get; set; }
    public string EmployeeId { get; set; }
    public int UserId { get; set; }
    public string ActivityInstanceId { get; set; }
    public bool IsPartial { get; set; }
    public string IssuedLot { get; set; }
    public string Comments { get; set; }
    public string ExternalId { get; set; }
    public string ExternalDate { get; set; }
}

/// <summary>
/// 
/// </summary>
public class OrderTransactionProductDetailSync
{
    public string TransactionId { get; set; }
    public string ItemCode { get; set; }
    public string LotNumber { get; set; }
    public string Pallet { get; set; }
    public string BinLocationCode { get; set; }
    public double Quantity { get; set; }
    public string LotStatusCode { get; set; }
    public string InventoryStatusCode { get; set; }
    public string LineNo { get; set; }
    public string WarehouseCode { get; set; }
    public string MachineCode { get; set; }
    public string OriginalMachine { get; set; }
}

