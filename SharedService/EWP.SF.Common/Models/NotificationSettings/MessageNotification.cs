namespace EWP.SF.Common.Models.NotificationSettings;

/// <summary>
///
/// </summary>
public class MessageNotification
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string keyConfirm { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MessageApp { get; set; }

	/// <summary>
	/// InApp, Email, MsTeams
	/// </summary>
	public object MessageType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string From { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? ToUserApp { get; set; }

	/// <summary>
	///
	/// </summary>
	public string To { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Cc { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Subject { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Priority { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? SentDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAppExt { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool MessageSent { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool MessageOpen { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool ConfirmRead { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool RequiresConfirm { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? ConfirmBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ConfirmDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ReadDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AuxKey { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TemplateId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DataValues { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> CodeGroups { get; set; }
	public bool IsError { get; set; } = false;
}

