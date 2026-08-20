namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class LiveGraphOEEComplex
{
	/// <summary>
	///
	/// </summary>
	public int MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineName { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal OEE { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndDate { get; set; }
}