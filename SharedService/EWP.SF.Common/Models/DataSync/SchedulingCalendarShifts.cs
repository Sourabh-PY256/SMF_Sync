namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class SchedulingCalendarShifts
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeShift { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeOrigin { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IdAsset { get; set; }

	/// <summary>
	///
	/// </summary>
	public int AssetLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetLevelCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IdParent { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsParent { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime FromDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ToDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Color { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool isEmployee { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeClock { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Origin { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Validation { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SchedulingCalendarShifts> listChildren { get; set; }
	public List<string> Assets { get; set; }
}
