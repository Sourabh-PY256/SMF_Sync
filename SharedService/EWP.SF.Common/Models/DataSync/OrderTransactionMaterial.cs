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
public class OrderTransactionMaterial
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
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Direction { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Operator { get; set; }

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
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<OrderTransactionMaterialDetail> Details { get; set; }

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
public class OrderTransactionMaterialDetail
{
	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalItemId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

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
	public string InventoryStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpDate { get; set; }

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
	public DateTime LogDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ScrapTypeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class MaterialIssueExternal
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
	[Required]
	[Description("Items")]
	[JsonProperty(PropertyName = "Items")]
	public List<ItemMaterialTransactionExternal> Items { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemMaterialTransactionExternal
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
	[Description("Line Id")]
	[JsonProperty(PropertyName = "LineId")]
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
	[JsonProperty(PropertyName = "Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty(PropertyName = "ScrapTypeCode")]
	public string ScrapTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Lots")]
	[JsonProperty(PropertyName = "Lots")]
	public List<ItemLotMaterialTransactionExternal> Lots { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Serial Numbers")]
	[JsonProperty(PropertyName = "SerialNumbers")]
	public List<ItemSerialNumberMaterialTransactionExternal> SerialNumbers { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemLotMaterialTransactionExternal
{
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
	[Description("Pallet")]
	[JsonProperty(PropertyName = "Pallet")]
	public string Pallet { get; set; }

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

	/// <summary>
	///
	/// </summary>
	[JsonProperty(PropertyName = "Comments")]
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty(PropertyName = "ScrapTypeCode")]
	public string ScrapTypeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ItemSerialNumberMaterialTransactionExternal
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
	[Description("Pallet")]
	[JsonProperty(PropertyName = "Pallet")]
	public string Pallet { get; set; }

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
public class MaterialReturnExternal : MaterialIssueExternal;
