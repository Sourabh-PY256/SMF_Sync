namespace EWP.SF.Common.Models.Sensors;

/// <summary>
///
/// </summary>
public class SensorThen
{
	/// <summary>
	///
	/// </summary>
	public string SensorId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int When { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Order { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Action { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TemplateId { get; set; }

	/// <summary>
	///
	/// </summary>
	public long IdleTimeout { get; set; }

	/// <summary>
	///
	/// </summary>
	public long Duration { get; set; }

	/// <summary>
	///
	/// </summary>
	public int RequiresConfirm { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SensorRecipient> SensorsRecipients { get; set; }

	/// <summary>
	///
	/// </summary>
	public SensorThen()
	{
		SensorsRecipients = [];
	}
}
