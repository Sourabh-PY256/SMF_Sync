namespace EWP.SF.Common.Models.Sensors;

public class SensorThen
{
	public string SensorId { get; set; }
	public int When { get; set; }
	public int Order { get; set; }
	public string Action { get; set; }
	public int Type { get; set; }
	public string TemplateId { get; set; }
	public long IdleTimeout { get; set; }
	public long Duration { get; set; }
	public int RequiresConfirm { get; set; }
	public bool IsActive { get; set; }
	public List<SensorRecipient> SensorsRecipients { get; set; }

	public SensorThen()
	{
		SensorsRecipients = [];
	}
}
