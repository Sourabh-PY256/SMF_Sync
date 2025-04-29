namespace EWP.SF.Common.Models.Sensors;

public class SensorLiveViewer
{
	public string SensorId { get; set; }
	public int Order { get; set; }
	public decimal MinimumValue { get; set; }
	public decimal MaximumValue { get; set; }
	public long Duration { get; set; }
	public string Color { get; set; }
	public bool Flicker { get; set; }
}
