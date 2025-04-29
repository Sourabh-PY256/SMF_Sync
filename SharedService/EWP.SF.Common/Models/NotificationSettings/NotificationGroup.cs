using EWP.SF.Common.EntityLogger;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.NotificationSettings;

public class NotificationGroup : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_notification_group_log");

	[EntityColumn("NotificationGroupCode")]
	public string Code { get; set; }

	public string Name { get; set; }
	public int Status { get; set; }
	public List<NotificationGroupDetail> GroupsDetails { get; set; }

	public string GroupsDetailsToJSON()
	{
		string returnValue = string.Empty;
		if (GroupsDetails is not null)
		{
			returnValue = JsonConvert.SerializeObject(GroupsDetails);
		}
		return returnValue;
	}
}

public class NotificationGroupDetail
{
	public string CodeGroup { get; set; }
	public string CodePlataform { get; set; }
}
