using System.ComponentModel;

namespace EWP.SF.Common.Models.NotificationSettings;

/// <summary>
///
/// </summary>
public class EmailServerSettings
{
	/// <summary>
	///
	/// </summary>
	[Description("SMTP Server URL")]
	public string Server { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("User name for the SMTP Server")]
	public string Username { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? Port { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Password { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Requires SSL")]
	public bool? SSL { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Sender Name")]
	public string DisplayName { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Disabled { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime AuxDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToEmail { get; set; }
}

/// <summary>
///
/// </summary>
public class EmailData
{
	/// <summary>
	///
	/// </summary>
	public string Subject { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Body { get; set; }

	/// <summary>
	///
	/// </summary>
	public string To { get; set; }
}
