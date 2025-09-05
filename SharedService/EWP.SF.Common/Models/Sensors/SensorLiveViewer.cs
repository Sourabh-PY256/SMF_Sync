namespace EWP.SF.Common.Models.Sensors;

/// <summary>
///
/// </summary>
public class SensorLiveViewer
{
	/// <summary>
	///
	/// </summary>
	public string SensorId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Order { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MinimumValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MaximumValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public long Duration { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Color { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Flicker { get; set; }
}
