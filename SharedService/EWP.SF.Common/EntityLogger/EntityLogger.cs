using Newtonsoft.Json;

namespace EWP.SF.Common.EntityLogger;

/// <summary>
///
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class EntityColumnAttribute(string column) : Attribute
{
	/// <summary>
	///
	/// </summary>
	public string ColumnName { get; set; } = column;
}

/// <summary>
///
/// </summary>
public class EntityLoggerConfig
{
	private string _logTable;

	/// <summary>
	///
	/// </summary>
	public Func<string> TableGetter { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogTable
	{
		get => TableGetter is not null ? TableGetter() : _logTable;
		set => _logTable = value;
	}

	/// <summary>
	///
	/// </summary>
	public string LogDate { get; set; } = "LogDate";

	/// <summary>
	///
	/// </summary>
	public string LogType { get; set; } = "LogType";

	/// <summary>
	///
	/// </summary>
	public string Entity { get; set; } = "Entity";

	/// <summary>
	///
	/// </summary>
	public string UserId { get; set; } = "LogUser";

	/// <summary>
	///
	/// </summary>
	public string EmployeeCode { get; set; } = "LogEmployee";

	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig(string logTable)
	{
		LogTable = logTable;
	}

	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig(Func<string> action)
	{
		TableGetter = action;
	}
}

/// <summary>
///
/// </summary>
public interface ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public EntityLoggerConfig EntityLogConfiguration { get; }
}
