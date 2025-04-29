namespace EWP.SF.Common.Models;

public class EmailMessage
{
	public string Id { get; set; }
	public string Platform { get; set; }
	public string Subject { get; set; }
	public string Body { get; set; }
	public string Recipient { get; set; }
	public string CC { get; set; }
	public int MessageId { get; set; }
	public string CCO { get; set; }
	public User TriggeredBy { get; set; }

	public bool Sent { get; set; }
}
