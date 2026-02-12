using MySqlConnector;
namespace EWP.SF.API.DataAccess;

public static class DataReaderExtensions
{
	public static int TryGetOrdinal(this MySqlDataReader reader, string columnName)
	{
		try
		{
			// Intentamos obtener el índice de la columna
			return reader.GetOrdinal(columnName);
		}
		catch
		{
			// Si la columna no existe, devolvemos -1
			return -1;
		}
	}

	// String
	public static string GetSafeString(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
	}

	public static string GetSafeString(this MySqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

	// Int32
	public static int GetSafeInt32(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
	}

	public static int GetSafeInt32(this MySqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);

	// Nullable Int32
	public static int? GetSafeNullableInt32(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
	}

	public static int? GetSafeNullableInt32(this MySqlDataReader reader, int ordinal)
	{
		return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
	}

	// Boolean
	public static bool GetSafeBoolean(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
	}

	public static bool GetSafeBoolean(this MySqlDataReader reader, int ordinal) => !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);

	// Nullable Boolean
	public static bool? GetSafeNullableBoolean(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? null : reader.GetBoolean(ordinal);
	}

	public static bool? GetSafeNullableBoolean(this MySqlDataReader reader, int ordinal)
	{
		return reader.IsDBNull(ordinal) ? null : reader.GetBoolean(ordinal);
	}

	// Double
	public static double GetSafeDouble(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? 0.0 : reader.GetDouble(ordinal);
	}

	public static double GetSafeDouble(this MySqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? 0.0 : reader.GetDouble(ordinal);

	// Nullable Double
	public static double? GetSafeNullableDouble(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? null : reader.GetDouble(ordinal);
	}

	public static double? GetSafeNullableDouble(this MySqlDataReader reader, int ordinal)
	{
		return reader.IsDBNull(ordinal) ? null : reader.GetDouble(ordinal);
	}

	// DateTime
	public static DateTime GetSafeDateTime(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
	}

	public static DateTime GetSafeDateTime(this MySqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);

	// Nullable DateTime
	public static DateTime? GetSafeNullableDateTime(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
	}

	public static DateTime? GetSafeNullableDateTime(this MySqlDataReader reader, int ordinal)
	{
		return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
	}

	// Decimal
	public static decimal GetSafeDecimal(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
	}

	public static decimal GetSafeDecimal(this MySqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);

	// Nullable Decimal
	public static decimal? GetSafeNullableDecimal(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
	}

	public static decimal? GetSafeNullableDecimal(this MySqlDataReader reader, int ordinal)
	{
		return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
	}

	// Byte array (Byte[])
	public static byte[] GetSafeByteArray(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		if (reader.IsDBNull(ordinal))
		{
			return null;
		}

		long size = reader.GetBytes(ordinal, 0, null, 0, 0); // Get the size of the data

		byte[] buffer = new byte[size];
		reader.GetBytes(ordinal, 0, buffer, 0, (int)size);
		return buffer;
	}

	public static byte[] GetSafeByteArray(this MySqlDataReader reader, int ordinal)
	{
		if (reader.IsDBNull(ordinal))
		{
			return null;
		}

		long size = reader.GetBytes(ordinal, 0, null, 0, 0); // Get the size of the data

		byte[] buffer = new byte[size];
		reader.GetBytes(ordinal, 0, buffer, 0, (int)size);
		return buffer;
	}

	// Nullable Byte array (returns byte[] or null)
	public static byte[] GetSafeNullableByteArray(this MySqlDataReader reader, string columnName)
	{
		int ordinal = reader.GetOrdinal(columnName);
		return reader.IsDBNull(ordinal) ? null : GetSafeByteArray(reader, ordinal);
	}

	public static byte[] GetSafeNullableByteArray(this MySqlDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : GetSafeByteArray(reader, ordinal);
}
