namespace EWP.SF.Common.Models;

public class SessionInfo
{
	public string Hash { get; set; }
	public string Address { get; set; }
	public string Browser { get; set; }
	public string OS { get; set; }
	public DateTime LoginDate { get; set; }
	public string Token { get; set; }
	public string Location { get; set; }
}
