using System.ComponentModel;

namespace EWP.SF.Common.Models.NotificationSettings;

public class EmailServerSettings
{
	[Description("SMTP Server URL")]
	public string Server { get; set; }

	[Description("User name for the SMTP Server")]
	public string Username { get; set; }

	public int? Port { get; set; }

	public string Password { get; set; }

	[Description("Requires SSL")]
	public bool? SSL { get; set; }

	[Description("Sender Name")]
	public string DisplayName { get; set; }

	public bool Disabled { get; set; }
	public DateTime AuxDate { get; set; }
	public string ToEmail { get; set; }
}

public class EmailData
{
	public string Subject { get; set; }
	public string Body { get; set; }
	public string To { get; set; }
}
