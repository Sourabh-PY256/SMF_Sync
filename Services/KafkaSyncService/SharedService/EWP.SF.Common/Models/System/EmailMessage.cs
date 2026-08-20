namespace EWP.SF.Common.Models;
/// <summary>
/// EmailMessage
/// </summary>
public class EmailMessage
{
	/// <summary>
	/// Gets or sets the unique identifier for the email message.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Gets or sets the platform for the email message.
	/// </summary>
	public string Platform { get; set; }

	/// <summary>
	/// Gets or sets the subject for the email message.
	/// </summary>
	public string Subject { get; set; }

	/// <summary>
	/// Gets or sets the body for the email message.
	/// </summary>
	public string Body { get; set; }

	/// <summary>
	/// Gets or sets the recipient for the email message.
	/// </summary>
	public string Recipient { get; set; }

	/// <summary>
	/// Gets or sets the CC for the email message.
	/// </summary>
	public string CC { get; set; }

	/// <summary>
	/// Gets or sets the sender for the email message.
	/// </summary>
	public int MessageId { get; set; }

	/// <summary>
	/// Gets or sets the BCC for the email message.
	/// </summary>
	public string CCO { get; set; }

	/// <summary>
	/// Gets or sets the attachments for the email message.
	/// </summary>
	public User TriggeredBy { get; set; }

	/// <summary>
	/// Gets or sets the date and time when the email message was created.
	/// </summary>
	public bool Sent { get; set; }
}
