namespace EWP.SF.Common.Models;

public class Notification
{
	public int Id { get; set; }
	public string Message { get; set; }

	public string Detail { get; set; }
	public string Parameters { get; set; }
	public string Category { get; set; }
	public bool IsError { get; set; }
	public int UserId { get; set; }

	public DateTime CreateDate { get; set; }
	public bool SendEmail { get; set; }
	public bool IsApp { get; set; }
	public bool IsRead { get; set; }
	public int Attempts { get; set; }
	public string Aux1 { get; set; }
	public string Aux2 { get; set; }
}
