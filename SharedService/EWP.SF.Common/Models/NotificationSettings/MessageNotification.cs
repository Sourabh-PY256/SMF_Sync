namespace EWP.SF.Common.Models.NotificationSettings;

public class MessageNotification
{
	public string Id { get; set; }
	public string keyConfirm { get; set; }

	//public string ProcessId { get; set; }

	public string Message { get; set; }

	public string MessageApp { get; set; }

	/// <summary>
	/// InApp, Email, MsTeams
	/// </summary>
	public string Type { get; set; }

	public string From { get; set; }
	public int? ToUserApp { get; set; }
	public string To { get; set; }
	public string Cc { get; set; }
	public string Subject { get; set; }
	public string Priority { get; set; }
	public DateTime? SentDate { get; set; }
	public bool IsAppExt { get; set; }
	public bool MessageSent { get; set; }
	public bool MessageOpen { get; set; }
	public bool ConfirmRead { get; set; }
	public bool RequiresConfirm { get; set; }
	public int? ConfirmBy { get; set; }
	public DateTime? ConfirmDate { get; set; }
	public DateTime? ReadDate { get; set; }
	public string AuxKey { get; set; }
	public string TemplateId { get; set; }
	public string DataValues { get; set; }
	public List<string> CodeGroups { get; set; }
}
