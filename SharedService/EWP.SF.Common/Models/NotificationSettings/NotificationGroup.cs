using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.NotificationSettings;

/// <summary>
///
/// </summary>
public class NotificationGroup : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_notification_group_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("NotificationGroupCode")]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<NotificationGroupDetail> GroupsDetails { get; set; }

	/// <summary>
	///
	/// </summary>
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

/// <summary>
///
/// </summary>
public class NotificationGroupDetail
{
	/// <summary>
	///
	/// </summary>
	public string CodeGroup { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodePlataform { get; set; }
}
