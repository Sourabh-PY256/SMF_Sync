using MySqlConnector;

namespace EWP.SF.Helper;

public static class ConnectionHelper
{
	public static void AddCondition(this MySqlParameterCollection myParam, string param, object value, bool condition, string throwEx = null)
	{
		ArgumentNullException.ThrowIfNull(myParam);

		if (condition)
		{
			_ = myParam.AddWithValue(param, value);
		}
		else
		{
			if (string.IsNullOrEmpty(throwEx))
			{
				_ = myParam.AddWithValue(param, null);
			}
			else
			{
				throw new ArgumentException(throwEx);
			}
		}
	}

	public static void AddCondition(this MySqlParameterCollection myParam, string param, Func<object> value, bool condition, string throwEx = null)
	{
		ArgumentNullException.ThrowIfNull(myParam);
		ArgumentNullException.ThrowIfNull(value);

		if (condition)
		{
			_ = myParam.AddWithValue(param, value());
		}
		else
		{
			if (string.IsNullOrEmpty(throwEx))
			{
				_ = myParam.AddWithValue(param, null);
			}
			else
			{
				throw new ArgumentException(throwEx);
			}
		}
	}

	public static void AddNull(this MySqlParameterCollection myParam, string param)
	{
		ArgumentNullException.ThrowIfNull(myParam);
		_ = myParam.AddWithValue(param, null);
	}

	public static object Val(this DateTime? obj)
	{
		return obj ?? (object)null;
	}

	public static object Val(this int? obj)
	{
		return obj ?? (object)null;
	}

	public static object Val(this bool? obj)
	{
		return obj ?? (object)null;
	}
}
