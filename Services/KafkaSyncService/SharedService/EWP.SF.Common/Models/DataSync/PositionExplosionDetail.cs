
namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class PositionExplosionDetail
{
	/// <summary>
	///
	/// </summary>
	public string EmployeeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PositionCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PositionName { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime IntervalStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime IntervalEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Efficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsActivity { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool RowIsEmpty { get; set; }
}
