using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Sensors;

/// <summary>
///
/// </summary>
public class SensorWhen
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
	public List<SensorThen> SensorsThen { get; set; }

	/// <summary>
	///
	/// </summary>
	public SensorWhen()
	{
		SensorsThen = [];
	}

	/// <summary>
	///
	/// </summary>
	public string SensorsThenToJSON()
	{
		string returnValue = string.Empty;
		if (SensorsThen is not null)
		{
			returnValue = JsonConvert.SerializeObject(SensorsThen);
		}
		return returnValue;
	}
}
