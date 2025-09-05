using MySqlConnector;

namespace EWP.SF.Helper;

/// <summary>
///
/// </summary>
public static class ConnectionHelper
{
	/// <summary>
	///
	/// </summary>
	/// <exception cref="ArgumentException"></exception>
	public static void AddCondition(this MySqlParameterCollection myParam, string param, object value, bool condition, string throwEx = null)
	{
		ArgumentNullException.ThrowIfNull(myParam);

		if (condition)
		{
			myParam.AddWithValue(param, value);
		}
		else if (string.IsNullOrEmpty(throwEx))
		{
			myParam.AddWithValue(param, null);
		}
		else
		{
			throw new ArgumentException(throwEx);
		}
	}

	/// <summary>
	///
	/// </summary>
	/// <exception cref="ArgumentException"></exception>
	public static void AddCondition(this MySqlParameterCollection myParam, string param, Func<object> value, bool condition, string throwEx = null)
	{
		ArgumentNullException.ThrowIfNull(myParam);
		ArgumentNullException.ThrowIfNull(value);

		if (condition)
		{
			myParam.AddWithValue(param, value());
		}
		else if (string.IsNullOrEmpty(throwEx))
		{
			myParam.AddWithValue(param, null);
		}
		else
		{
			throw new ArgumentException(throwEx);
		}
	}

	/// <summary>
	///
	/// </summary>
	public static void AddNull(this MySqlParameterCollection myParam, string param)
	{
		ArgumentNullException.ThrowIfNull(myParam);
		myParam.AddWithValue(param, null);
	}

	/// <summary>
	///
	/// </summary>
	public static object Val(this DateTime? obj) => obj ?? (object)null;

	/// <summary>
	///
	/// </summary>
	public static object Val(this int? obj) => obj ?? (object)null;

	/// <summary>
	///
	/// </summary>
	public static object Val(this bool? obj) => obj ?? (object)null;
}
