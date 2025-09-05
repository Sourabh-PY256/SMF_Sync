namespace EWP.SF.Common.Models.Sensors;

/// <summary>
///
/// </summary>
public class RecipientNotification
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? userId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string employeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string profileId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string userName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string displayName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string languageCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string language { get; set; }

	/// <summary>
	///
	/// </summary>
	public string email { get; set; }

	/// <summary>
	///
	/// </summary>
	public string phone { get; set; }

	//public string webhook { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> notificationGroup { get; set; }
}
