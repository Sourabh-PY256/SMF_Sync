using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

using Newtonsoft.Json;

using NLog;
namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
[GridBDEntityName("Supply")]
public class PurchasesOrder
{
	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string VendorCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string VendorName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string LineNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ItemName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal? Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string UnitMeasure { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string QuantityDesc
	{ get { return Quantity.ToString() + UnitMeasure; } }

	/// <summary>
	///
	/// </summary>
	public DateTime? ExpectedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	[GridIgnoreProperty]
	public int? UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string StatusMessage { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	[GridDrillDown("Warehouse", "Code")]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<PurchasesOrder> Detail { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CreateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CreateEmployee { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public PurchasesOrder()
	{
		Detail = [];
	}
}
