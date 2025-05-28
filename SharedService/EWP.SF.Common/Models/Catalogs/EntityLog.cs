namespace EWP.SF.Common.Models.Catalogs;

public class EntityLog
{
	public string Type { get; set; }
	public string Author { get; set; }

	public string Employee { get; set; }
	public string EmployeeCode { get; set; }
	public int Version { get; set; }
	public DateTime Date { get; set; }

	public string Entity { get; set; }

	//public Int64 Key { get; set; }

	public decimal Key { get; set; }
}
