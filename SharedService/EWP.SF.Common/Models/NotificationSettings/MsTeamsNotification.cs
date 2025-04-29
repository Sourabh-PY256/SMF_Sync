namespace EWP.SF.Common.Models.NotificationSettings;

public class MsTeamsNotification
{
	public string Message { get; set; }
	public string Subject { get; set; }
	public string To { get; set; }
	public bool RequiresConfirm { get; set; }
	public string ProcessId { get; set; }
	public bool Disabled { get; set; }
}
