namespace EWP.SF.Common.Models.NotificationSettings;

public class MSTeamSettings
{
	public string Id { get; set; }
	public int? UserId { get; set; }
	public string Code { get; set; }
	public string Description { get; set; }
	public int Type { get; set; }
	public string UrlWebHook { get; set; }
	public string Message { get; set; }
	public int Status { get; set; }
	public bool Disabled { get; set; }
}
