namespace EWP.SF.Common.Models.NotificationSettings;

/// <summary>
///
/// </summary>
public class MSTeamSettings
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UrlWebHook { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Disabled { get; set; }
}
