namespace EWP.SF.Common.Models.NotificationSettings;

public class NotificationsTemplatesDetail
{
	public string Code { get; set; }
	public string PlatformDetailCode { get; set; }
	public string LanguageCode { get; set; }
	public string LanguageValue { get; set; }
	public string Subject { get; set; }
	public string Message { get; set; }
	public string MessageInApp { get; set; }
	public bool? Modify { get; set; }
}
