using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;



namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class OrderTransactionProduct
{
	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IsOutput { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartEntryDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndEntryDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Direction { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal OrderFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal ProcessFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Operator { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityInstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IsPartial { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IssuedLot { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DocCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime DocDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime LogDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<OrderTransactionProductDetail> Details { get; set; }
	/// <summary>
	///
	/// </summary>
	public Double Quantity { get; set; }
	/// <summary>
	///
	/// </summary>
	public string DetailToJSON()
	{
		string returnValue = string.Empty;
		if (Details is not null)
		{
			returnValue = JsonConvert.SerializeObject(Details);
		}
		return returnValue;
	}
}

/// <summary>
///
/// </summary>
public class OrderTransactionProductDetail
{
	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalMachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemId { get; set; }

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
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Warehouse { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductReceiptExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Doc Code")]
	[JsonProperty(PropertyName = "DocCode")]
	public string DocCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Comments")]
	[JsonProperty(PropertyName = "Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Employee ID")]
	[JsonProperty(PropertyName = "EmployeeID")]
	public string EmployeeID { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("OrderCode")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("OperationNo")]
	[JsonProperty(PropertyName = "OperationNo")]
	public double OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Doc Date")]
	[JsonProperty(PropertyName = "DocDate")]
	public DateTime DocDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Items")]
	[JsonProperty(PropertyName = "Items")]
	public List<ItemProdTransactionExternal> Items { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemProdTransactionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Doc Code")]
	[JsonProperty(PropertyName = "DocCode")]
	public string DocCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Line ID")]
	[JsonProperty(PropertyName = "LineID")]
	public int LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("Product|ByProduct", ErrorMessage = "Invalid Line Type")]
	[Description("LineType")]
	[JsonProperty(PropertyName = "LineType")]
	public string LineType { get; set; }

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
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Lots")]
	[JsonProperty(PropertyName = "Lots")]
	public List<ItemLotProdTransactionExternal> Lots { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Serial Numbers")]
	[JsonProperty(PropertyName = "SerialNumbers")]
	public List<ItemSerialNumberProdTransactionExternal> SerialNumbers { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemLotProdTransactionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	[Description("Lot Number")]
	[JsonProperty(PropertyName = "LotNo")]
	public string LotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Location Code")]
	[JsonProperty(PropertyName = "LocationCode")]
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Inventory Status Code")]
	[JsonProperty(PropertyName = "InventoryStatusCode")]
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("ExpDate")]
	[JsonProperty(PropertyName = "ExpDate")]
	public DateTime ExpDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	public decimal Quantity { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemSerialNumberProdTransactionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	[Description("Serial Number")]
	[JsonProperty(PropertyName = "SerialNo")]
	public string SerialNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Location Code")]
	[JsonProperty(PropertyName = "LocationCode")]
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Inventory Status Code")]
	[JsonProperty(PropertyName = "InventoryStatusCode")]
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("ExpDate")]
	[JsonProperty(PropertyName = "ExpDate")]
	public DateTime ExpDate { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductReturnExternal : ProductReceiptExternal
{
	/// <summary>
	///
	/// </summary>
	[Description("Lots")]
	[JsonProperty(PropertyName = "Lots")]
	new public List<ItemProdRetTransactionExternal> Items { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemProdRetTransactionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Doc Code")]
	[JsonProperty(PropertyName = "DocCode")]
	public string DocCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Line ID")]
	[JsonProperty(PropertyName = "LineID")]
	public int LineID { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[RegularExpression("Product|ByProduct", ErrorMessage = "Invalid Line Type")]
	[Description("LineType")]
	[JsonProperty(PropertyName = "LineType")]
	public string LineType { get; set; }

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
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Lots")]
	[JsonProperty(PropertyName = "Lots")]
	public List<ItemLotProdRetTransactionExternal> Lots { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Serial Numbers")]
	[JsonProperty(PropertyName = "SerialNumbers")]
	public List<ItemSerialNumberProdRetTransactionExternal> SerialNumbers { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemLotProdRetTransactionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Lot Number")]
	[JsonProperty(PropertyName = "LotNo")]
	public string LotNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Location Code")]
	[JsonProperty(PropertyName = "LocationCode")]
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Inventory Status Code")]
	[JsonProperty(PropertyName = "InventoryStatusCode")]
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("ExpDate")]
	[JsonProperty(PropertyName = "ExpDate")]
	public DateTime ExpDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Quantity")]
	[JsonProperty(PropertyName = "Quantity")]
	public decimal Quantity { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemSerialNumberProdRetTransactionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Item Code")]
	[JsonProperty(PropertyName = "ItemCode")]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	[Description("Serial Number")]
	[JsonProperty(PropertyName = "SerialNo")]
	public string SerialNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Warehouse Code")]
	[JsonProperty(PropertyName = "WarehouseCode")]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Location Code")]
	[JsonProperty(PropertyName = "LocationCode")]
	public string LocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Inventory Status Code")]
	[JsonProperty(PropertyName = "InventoryStatusCode")]
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("ExpDate")]
	[JsonProperty(PropertyName = "ExpDate")]
	public DateTime ExpDate { get; set; }
}

/// <summary>
///
/// </summary>
public class OrderTransactionProductStatus
{
	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemId { get; set; }

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
	public string BinLocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExternalDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NewInventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NewBinLocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NewWarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemType { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductTransferExternal
{
	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExternalDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductTransferExternalItem> Items { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductTransferExternalItem
{
	/// <summary>
	///
	/// </summary>
	public string ItemCode { get; set; }
	/// <summary>
	///
	/// </summary>
	public string ItemType { get; set; }
	/// <summary>
	///
	/// </summary>
	public string FromWarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToWarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FromBinLocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToBinLocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FromInventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToInventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProductTransferExternalItemLot> Lots { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> Series { get; set; }
}

/// <summary>
///
/// </summary>
public class ProductTransferExternalItemLot
{
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
	public double Quantity { get; set; }
}
