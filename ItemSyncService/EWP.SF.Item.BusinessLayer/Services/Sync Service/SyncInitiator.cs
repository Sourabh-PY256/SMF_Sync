
using EWP.SF.Common.Models;
using NLog;
using EWP.SF.LicenseModule;

#pragma warning disable

namespace EWP.SF.Item.BusinessLayer;

public enum InitType
{
	Automatic,
	Manual
}

public static class SyncInitializer
{
	public static bool Initialized
	{
		get
		{
			return Mode == InitType.Manual;
		}
	}

	public static InitType Mode;

	public static event EventHandler<MessageBroker> onAlertReceived;

	private static Logger logger = LogManager.GetCurrentClassLogger();

	public static void Initialize(InitType mode = InitType.Manual)
	{
		bool returnValue = false;
		Mode = mode;
		string hw = HwId;

		returnValue = true;
	}

	private static string _hwId;

	public static string HwId
	{
		get
		{
			if (string.IsNullOrEmpty(_hwId))
			{
				_hwId = Utility.GetHwIdAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}

			return _hwId;
		}
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="value"></param>
	public static void ForcePush(MessageBroker value)
	{
		if (value is not null && onAlertReceived is not null && Mode == InitType.Manual)
		{
			Task.Run(() =>
			{
				onAlertReceived(null, value);
			});
		}
	}
}
