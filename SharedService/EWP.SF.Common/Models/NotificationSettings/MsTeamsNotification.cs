namespace EWP.SF.Common.Models.NotificationSettings;

/// <summary>
///
/// </summary>
public class MsTeamsNotification
{
	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Subject { get; set; }

	/// <summary>
	///
	/// </summary>
	public string To { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool RequiresConfirm { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Disabled { get; set; }
}
