namespace EWP.SF.Common.Models.Sensors;

public class SensorRecipient
{
	public string SensorId { get; set; }
	public int When { get; set; }
	public int Then { get; set; }
	public string Value { get; set; }
	public string Description { get; set; }
}
