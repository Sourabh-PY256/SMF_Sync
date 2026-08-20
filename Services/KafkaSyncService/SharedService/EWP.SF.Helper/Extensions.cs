using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace EWP.SF.Helper;

/// <summary>
/// Extensions for various types.
/// </summary>
[SuppressMessage("Performance", "RAMU1:Sync method was used instead of async", Justification = "<Pending>")]
public static class Extensions
{
	static readonly JsonSerializerOptions options = new()
	{
		WriteIndented = true,  // Equivalent to Formatting.Indented
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  // Ignore null values
	};

	/// <summary>
	/// Gets a list of all the values of an enum as a ReadOnlyCollection of KeyValuePair string, int.
	/// </summary>
	public static ReadOnlyCollection<KeyValuePair<string, int>> GetEnumList<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
	{
		List<KeyValuePair<string, int>> list = [];
		foreach (object e in Enum.GetValues(typeof(T)).Cast<object>().Where(static x => x is not null))
		{
			list.Add(new KeyValuePair<string, int>(e.ToString()!, (int)e));
		}
		return new ReadOnlyCollection<KeyValuePair<string, int>>(list);
	}

	/// <summary>
	/// Adds a property to an ExpandoObject. If the property already exists, it updates the value.
	/// </summary>
	public static void AddProperty(this ExpandoObject expando, string propertyName, object propertyValue)
	{
		//Take use of the IDictionary implementation
		IDictionary<string, object?> expandoDict = expando;
		if (expandoDict.ContainsKey(propertyName))
		{
			expandoDict[propertyName] = propertyValue;
		}
		else
		{
			expandoDict.Add(propertyName, propertyValue);
		}
	}

	/// <summary>
	/// Converts an object to XML string.
	/// </summary>
	public static string ToXML(this object value)
	{
		XmlSerializerNamespaces emptyNamespaces = new([XmlQualifiedName.Empty]);
		XmlSerializer serializer = new(value.GetType());
		XmlWriterSettings settings = new()
		{
			Indent = true,
			OmitXmlDeclaration = true
		};

		using StringWriter stream = new();
		using XmlWriter writer = XmlWriter.Create(stream, settings);
		serializer.Serialize(writer, value, emptyNamespaces);
		return stream.ToString();
	}

	/// <summary>
	/// Converts an object to JSON string.
	/// </summary>
	public static string ToJSON(this object value)
	{
		string returnValue = string.Empty;
		try
		{
			returnValue = JsonSerializer.Serialize(value, options);
		}
		catch { }
		return returnValue;
	}

	/// <summary>
	/// Clones the properties of an object to a KeyValuePair.
	/// </summary>
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	public static KeyValuePair<string, string> CloneProperties(this object o, string propertyName = "", string owner = "")
	{
		string propName = string.Empty;
		string? value = string.Empty;
		string prepend = string.Empty;
		if (!string.IsNullOrEmpty(owner))
		{
			prepend = owner + ".";
		}
		foreach (PropertyInfo prop in o.GetType().GetProperties())
		{
			if (string.IsNullOrEmpty(propertyName) || prop.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase) || prop.PropertyType.IsEnum)
			{
				if (!string.IsNullOrEmpty(prop.PropertyType.FullName) && prop.PropertyType.FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
				{
					propName = prepend + prop.Name;
					value = prop.GetValue(o)?.ToStr();
				}
				else
				{
					propName = prepend + prop.Name;
					value = prop.Name.ToStr();
				}
			}
		}
		return new KeyValuePair<string, string>(propName, value!);
	}

	/// <summary>
	/// Converts the properties of an object to an ExpandoObject.
	/// </summary>
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	public static ExpandoObject ExpandProperties(this object o, string propertyName = "", string owner = "")
	{
		ExpandoObject returnValue = new();
		string prepend = string.Empty;
		if (!string.IsNullOrEmpty(owner))
		{
			prepend = owner + ".";
		}
		foreach (PropertyInfo prop in o.GetType().GetProperties())
		{
			if (string.IsNullOrEmpty(propertyName) || prop.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrEmpty(prop.PropertyType.FullName) && prop.PropertyType.FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
				{
					returnValue.AddProperty(prepend + prop.Name, prop.GetValue(o) ?? "null");
				}
				else if (prop.PropertyType.IsEnum)
				{
					returnValue.AddProperty(prepend + prop.Name, prop.GetValue(o) ?? "null");
				}
				else
				{
					returnValue.AddProperty(prepend + prop.Name, prop.Name);
				}
			}
		}
		return returnValue;
	}

	/// <summary>
	/// Gets the value of a property from an anonymous object.
	/// </summary>
	public static object? GetAnonymousValue(this object o, string value)
	{
		try
		{
			return o.GetType().GetProperty(value)?.GetValue(o);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Converts an object to string si and if error returns error value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	/// <returns>string</returns>
	public static string ToStr(this object o, string error = "")
	{
		try
		{
			return o is not null ? o.ToString() ?? "" : error;
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to string si and if null returns null value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	public static string? ToStrNull(this object o, string error = "")
	{
		try
		{
			return o?.ToString();
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to string si and if error returns error value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	/// <returns>byte[]</returns>
	public static byte[]? ToBytes(this object o, byte[]? error = null)
	{
		try
		{
			return o is not null ? Encoding.UTF8.GetBytes(o.ToString() ?? "") : [];
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to string si and if null returns null value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	public static byte[]? ToBytesNull(this object o, byte[]? error = null)
	{
		try
		{
			string? oValue = o.ToString();
			return oValue is not null ? Encoding.UTF8.GetBytes(oValue) : null;
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to boolean si and if error returns error value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	public static bool ToBool(this object o, bool error = false)
	{
		try
		{
			bool returnValue = false;
			string stringVal = o.ToStr();
			if (stringVal == "1")
			{
				return true;
			}
			else if (stringVal == "0")
			{
				return false;
			}
			else
			{
				if (!bool.TryParse(o.ToStr(), out returnValue))
				{
					returnValue = error;
				}
				return returnValue;
			}
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to int32 si and if error returns error value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	public static int ToInt32(this object o, int error = 0)
	{
		try
		{
			int retVal;
			if (o is bool)
			{
				return Convert.ToInt32(o, CultureInfo.InvariantCulture);
			}
			else if (o is Enum)
			{
				return Convert.ToInt32(o, CultureInfo.InvariantCulture);
			}
			else if (!int.TryParse(o.ToStr(), out retVal))
			{
				return error;
			}
			return retVal;
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to int32 si and if error returns error value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	public static long ToInt64(this object o, int error = 0)
	{
		try
		{
			if (!long.TryParse(o.ToStr(), out long retVal))
			{
				retVal = error;
			}
			return retVal;
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to decimal si and if error returns error value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	/// <returns>Decimal </returns>
	public static decimal ToDecimal(this object o, decimal error = 0)
	{
		try
		{
			if (!decimal.TryParse(o.ToStr(), out decimal retVal))
			{
				retVal = error;
			}
			return retVal;
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to Double si and if error returns error value
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	/// <returns>Double </returns>
	public static double ToDouble(this object o, double error = 0)
	{
		try
		{
			if (!double.TryParse(o.ToStr(), out double retVal))
			{
				retVal = error;
			}
			return retVal;
		}
		catch
		{
			return error;
		}
	}

	/// <summary>
	/// Converts an object to dateTime
	/// </summary>
	/// <param name="o">Object to convert</param>
	/// <param name="error">Object to return if error</param>
	/// <returns>DateTime </returns>
	public static DateTime ToDate(this object o, DateTime error = new DateTime())
	{
		try
		{
			if (!DateTime.TryParse(o.ToStr(), out DateTime retVal))
			{
				retVal = error < new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc) ? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc) : error;
			}
			return retVal;
		}
		catch
		{
			return error < new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc) ? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc) : error;
		}
	}

	/// <summary>
	/// Converts a base64 to decoded string
	/// </summary>
	/// <param name="o">String to convert</param>
	/// <returns>clear string</returns>
	public static string FromBase64String(this string o) => Encoding.UTF8.GetString(Convert.FromBase64String(o));

	/// <summary>
	/// Converts a string to base64 string
	/// </summary>
	/// <param name="o">string to convert</param>
	/// <returns>base64 string</returns>
	public static string ToBase64String(this string o) => Convert.ToBase64String(Encoding.UTF8.GetBytes(o));

	/// <summary>
	/// Gets the value from a request header
	/// </summary>
	public static string? GetHeader(this HttpRequestMessage request, string key)
	{
		return !request.Headers.TryGetValues(key, out IEnumerable<string>? keys) ? null : keys.First();
	}

	/// <summary>
	/// Transforms parameters contained in a WebRequest into lineal parameters.
	/// </summary>
	/// <param name="RequestParameters">Dictionary of parameters</param>
	/// <returns>Array with the concatenated parameters to their values</returns>
	public static string[] TransformParameters(this Dictionary<string, object> RequestParameters)
	{
		List<string> reqParams = [];
		foreach (string key in RequestParameters.Keys)
		{
			reqParams.Add(key + "=" + RequestParameters[key]);
		}
		return [.. reqParams];
	}

	/// <summary>
	/// Transforms parameters contained in a WebRequest into lineal parameters.
	/// </summary>
	public static string Substr(this string input, int start = 0, int length = 0)
	{
		string result = input;
		if (length == 0)
		{
			length = input.Length < start ? 0 : input.Length - start;
		}

		try
		{
			if (string.IsNullOrEmpty(result))
			{
				return string.Empty;
			}

			result = input.Substring(start, length);
		}
		catch
		{
		}

		return result;
	}

	/// <summary>
	/// Converts a generic stream into a byte array
	/// </summary>
	/// <param name="input">Stream</param>
	/// <returns>byte array</returns>
	public static async Task<byte[]> ToByteArray(this Stream input)
	{
		byte[] buffer = new byte[input.Length];
		await using MemoryStream ms = new();
		int read;
		while ((read = await input.ReadAsync(buffer).ConfigureAwait(false)) > 0)
		{
			await ms.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
		}
		return ms.ToArray();
	}

	/// <summary>
	/// Converts a string to MD5 hash.
	/// </summary>
	public static string ComputeMD5(this string input)
	{
		// step 1, calculate MD5 hash from input
		byte[] inputBytes = Encoding.ASCII.GetBytes(input);
		byte[] hash = MD5.HashData(inputBytes);

		// step 2, convert byte array to hex string
		StringBuilder sb = new();
		for (int i = 0; i < hash.Length; i++)
		{
			sb.Append(hash[i].ToString("X2", CultureInfo.InvariantCulture));
		}
		return sb.ToString();
	}

	/// <summary>
	/// Converts a byte array to hex string.
	/// </summary>
	public static string ToHex(this byte[] bytes, bool upperCase)
	{
		StringBuilder result = new(bytes.Length * 2);

		for (int i = 0; i < bytes.Length; i++)
		{
			result.Append(bytes[i].ToString(upperCase ? "X2" : "x2", CultureInfo.InvariantCulture));
		}

		return result.ToString();
	}

	/// <summary>
	/// Converts a string to MD5 hash.
	/// </summary>
	public static string FromString(this MD5 md5Hash, string input)
	{
		// Convert the input string to a byte array and compute the hash.
		byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

		// Create a new StringBuilder to collect the bytes
		// and create a string.
		StringBuilder sBuilder = new();

		// Loop through each byte of the hashed data
		// and format each one as a hexadecimal string.
		for (int i = 0; i < data.Length; i++)
		{
			sBuilder.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
		}

		// Return the hexadecimal string.
		return sBuilder.ToString();
	}

	/// <summary>
	/// Returns true if the object is null.
	/// </summary>
	public static bool IsNull(this object? obj) => obj is null;

	/// <summary>
	/// Converts a DateTime to a Unix epoch timestamp.
	/// </summary>
	public static double ToEpoch(this DateTime value)
	{
		//create TimeSpan by subtracting the value provided from
		//the Unix Epoch
		TimeSpan span = value - DateTime.UnixEpoch.ToLocalTime();

		//return the total seconds (which is a UNIX timestamp)
		return span.TotalSeconds;
	}

	/// <summary>
	/// Computes the MD5 hash of the input string.
	/// </summary>
	public static string Md5(this string input)
	{
		// Convert the input string to a byte array and compute the hash.
		byte[] data = MD5.HashData(Encoding.Default.GetBytes(input));

		// Create a new StringBuilder to collect the bytes and create a string.
		StringBuilder sBuilder = new();

		// Loop through each byte of the hashed data and format each one as a hexadecimal string.
		for (int i = 0; i < data.Length; i++)
		{
			sBuilder.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
		}

		// Return the hexadecimal string.
		return sBuilder.ToString();
	}

	/// <summary>
	/// Extension method to check if two strings are equal ignoring case.
	/// </summary>
	public static bool EqualsNoCase(this string text, string textToCompare)
	{
		return !string.IsNullOrEmpty(text)
				&& !string.IsNullOrEmpty(textToCompare)
				&& text.Equals(textToCompare, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Extension method to check if a string contains another string ignoring case.
	/// </summary>
	public static bool ContainsNoCase(this string text, string textToCompare)
	{
		return !string.IsNullOrEmpty(text)
				&& !string.IsNullOrEmpty(textToCompare)
				&& text.Contains(textToCompare, StringComparison.OrdinalIgnoreCase);
	}
}
