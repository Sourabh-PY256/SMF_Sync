namespace EWP.SF.Common.Models.Sensors;

public class RecipientNotification
{
	public string Id { get; set; }
	public int? userId { get; set; }
	public string employeeId { get; set; }
	public string profileId { get; set; }
	public string userName { get; set; }
	public string displayName { get; set; }
	public string languageCode { get; set; }
	public string language { get; set; }
	public string email { get; set; }
	public string phone { get; set; }

	//public string webhook { get; set; }

	public List<string> notificationGroup { get; set; }
}
