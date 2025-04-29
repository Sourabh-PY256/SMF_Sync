using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Sensors;

public class SensorWhen
{
	public string SensorId { get; set; }
	public int Order { get; set; }
	public decimal MinimumValue { get; set; }
	public decimal MaximumValue { get; set; }
	public long Duration { get; set; }
	public List<SensorThen> SensorsThen { get; set; }

	public SensorWhen()
	{
		SensorsThen = [];
	}

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
