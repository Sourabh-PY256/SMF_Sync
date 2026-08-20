using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace EWP.SF.Helper;

/// <summary>
/// Common is a static class that provides utility methods for file handling, time conversion, and system checks.
/// </summary>
public static class Common
{
	/// <summary>
	/// Checks if a file is locked by attempting to open it with exclusive access.
	/// </summary>
	public static bool IsFileLocked(FileInfo file)
	{
		FileStream? stream = null;

		try
		{
			stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		}
		catch (IOException)
		{
			//the file is unavailable because it is:
			//still being written to
			//or being processed by another thread
			//or does not exist (has already been processed)
			return true;
		}
		finally
		{
			stream?.Close();
		}

		//file is not locked
		return false;
	}

	/// <summary>
	/// Converts seconds to a formatted time string.
	/// </summary>
	public static string SecondsToTimeString(int seconds, bool forceDays)
	{
		int hours = 0;
		int minutes = 0;
		int days = 0;
		while (seconds >= 86400)
		{
			days++;
			seconds -= 86400;
		}
		while (seconds >= 3600)
		{
			hours++;
			seconds -= 3600;
		}
		while (seconds >= 60)
		{
			minutes++;
			seconds -= 60;
		}
		if (seconds < 0)
		{
			seconds = 0;
		}
		return days > 0 || forceDays
			? (days < 10 ? "0" : "") + days.ToStr() + ":" + (hours < 10 ? "0" : "") + hours.ToStr() + ":" + (minutes < 10 ? "0" : "") + minutes.ToStr() + ":" + (seconds < 10 ? "0" : "") + seconds.ToStr()
			: (hours < 10 ? "0" : "") + hours.ToStr() + ":" + (minutes < 10 ? "0" : "") + minutes.ToStr() + ":" + (seconds < 10 ? "0" : "") + seconds.ToStr();
	}

	/// <summary>
	/// Converts a time string to seconds.
	/// </summary>
	public static int TimeStringToSeconds(string time, int def)
	{
		int returnValue = 0;
		if (!string.IsNullOrEmpty(time) && time.Contains(':', StringComparison.InvariantCultureIgnoreCase))
		{
			string[] temp = time.Split(':');

			try
			{
				if (temp.Length == 4)
				{
					returnValue += temp[0].ToInt32() * 86400;
					returnValue += temp[1].ToInt32() * 3600;
					returnValue += temp[2].ToInt32() * 60;
					returnValue += temp[3].ToInt32();
				}
				if (temp.Length == 3)
				{
					returnValue += temp[0].ToInt32() * 3600;
					returnValue += temp[1].ToInt32() * 60;
					returnValue += temp[2].ToInt32();
				}
				else if (temp.Length == 2)
				{
					returnValue += temp[0].ToInt32() * 60;
					returnValue += temp[1].ToInt32();
				}
				else if (temp.Length == 1)
				{
					returnValue += temp[0].ToInt32();
				}
			}
			catch { }
		}
		else if (def >= 0)
		{
			returnValue = def;
		}
		return returnValue;
	}

	/// <summary>
	/// Checks if the current user is an administrator.
	/// </summary>
	public static bool IsAdministrator()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			using WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new(identity);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}
		else //Linux
		{
			return true;
		}
	}

	/// <summary>
	/// Gets the name of the current project by extracting it from the assembly name.
	/// </summary>
	public static string GetProjectName()
	{
		string? assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
		string[]? parts = assemblyName?.Split('.');
		return parts?.Length > 0 ? parts[^1] : "Unknown";
	}
}
