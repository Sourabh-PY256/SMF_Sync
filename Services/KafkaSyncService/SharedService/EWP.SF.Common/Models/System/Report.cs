namespace EWP.SF.Common.Models;
/// <summary>
///
/// </summary>
public class Report
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Script { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Args { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public string GroupElement { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Parameter> Parameters { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ResultObject { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ChartObject { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ColumnStyle { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RowStyle { get; set; }
}

/// <summary>
///
/// </summary>
public class Parameter
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Parent { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DisplayName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	public int CatalogId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DisplayMember { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ValueMember { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Multiple { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DefaultValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Sort { get; set; }

	/// <summary>
	///
	/// </summary>
	public object Data { get; set; }
}

/// <summary>
///
/// </summary>
public class ReportCatalog
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Script { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ValueMember { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DisplayMember { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Parameter { get; set; }
}
