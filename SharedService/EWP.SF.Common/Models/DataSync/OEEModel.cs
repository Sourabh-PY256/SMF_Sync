using Newtonsoft.Json;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class OEEModel
{
	/// <summary>
	///
	/// </summary>
	public double Availability { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Performance { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quality { get; set; }

	/// <summary>
	///
	/// </summary>
	public double OEE { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string OffTime { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string OnTime { get; set; }
}
