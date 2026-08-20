namespace EWP.SF.Common.Models.Catalogs;

/// <summary>
///
/// </summary>
public class EntityLog
{
	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Author { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Employee { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime Date { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Entity { get; set; }

	//public Int64 Key { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Key { get; set; }
}
