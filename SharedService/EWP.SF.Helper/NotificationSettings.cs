using System.Collections.Concurrent;
using EWP.SF.Common.Models.NotificationSettings;

namespace EWP.SF.Helper;

public class NotificationSettings
{
	public const string noPermission = "User doesn't have permission for this action";
	private const string badParam = "missing  or invalid parameter {0}";
	private const string invalidHash = "invalid hash signature";
	private static readonly TimeSpan delayBetweenMessages = TimeSpan.FromMilliseconds(50); // ~20 messages per second
	private static readonly BlockingCollection<MsTeamsNotification> messageQueue = [];
}
