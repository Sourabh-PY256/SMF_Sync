

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class SchedulingShiftDetail : ICloneable
{
	/// <summary>
	///
	/// </summary>
	public string CodeShift { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeShiftStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public long StartOffset { get; set; }

	/// <summary>
	///
	/// </summary>
	public long Duration { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CreationById { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ModifiedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ModifiedById { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Order { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ShiftCheck { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Style { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Color { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Efficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Factor { get; set; }

	object ICloneable.Clone()
	{
		throw new NotImplementedException();
	}
}
