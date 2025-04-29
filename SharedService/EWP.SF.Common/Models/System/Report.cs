namespace EWP.SF.Common.Models;

public class Report
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Script { get; set; }
	public string Args { get; set; }
	public string Description { get; set; }
	public string GroupElement { get; set; }
	public List<Parameter> Parameters { get; set; }

	public string ResultObject { get; set; }
	public string ChartObject { get; set; }

	public string ColumnStyle { get; set; }
	public string RowStyle { get; set; }
}

public class Parameter
{
	public int Id { get; set; }
	public int Parent { get; set; }
	public string Type { get; set; }
	public string Name { get; set; }
	public string DisplayName { get; set; }
	public string Value { get; set; }

	public int CatalogId { get; set; }
	public string DisplayMember { get; set; }
	public string ValueMember { get; set; }
	public bool Multiple { get; set; }
	public string DefaultValue { get; set; }
	public int Sort { get; set; }
	public object Data { get; set; }
}

public class ReportCatalog
{
	public int Id { get; set; }

	public string Script { get; set; }
	public string ValueMember { get; set; }
	public string DisplayMember { get; set; }
	public string Parameter { get; set; }
}
