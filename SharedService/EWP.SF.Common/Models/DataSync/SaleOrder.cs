using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

using Newtonsoft.Json;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
[GridBDEntityName("Demand")]
public class SaleOrder
{
	/// <summary>
	///
	/// </summary>
	[EntityColumn("Id")]
	[GridDrillDown]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SalesOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CustomerCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CustomerName { get; set; }

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
	{ get { return Quantity.ToString() + " " + UnitMeasure; } }

	/// <summary>
	///
	/// </summary>
	public DateTime? ExpectedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Priority { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int? UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("Warehouse", "Code")]
	[GridIgnoreProperty]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SaleOrder> Detail { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public SaleOrder()
	{
		Detail = [];
	}
}
