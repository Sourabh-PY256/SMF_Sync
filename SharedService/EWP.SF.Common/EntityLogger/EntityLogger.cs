using Newtonsoft.Json;

namespace EWP.SF.Common.EntityLogger;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class EntityColumnAttribute(string column) : Attribute
{
	public string ColumnName { get; set; } = column;
}

public class EntityLoggerConfig
{
	public Func<string> TableGetter { get; set; }
	private string _logTable;

	public string LogTable
	{
		get => TableGetter is not null ? TableGetter() : _logTable;
		set => _logTable = value;
	}

	public string LogDate { get; set; } = "LogDate";
	public string LogType { get; set; } = "LogType";

	public string Entity { get; set; } = "Entity";
	public string UserId { get; set; } = "LogUser";
	public string EmployeeCode { get; set; } = "LogEmployee";

	public EntityLoggerConfig(string logTable)
	{
		LogTable = logTable;
	}

	public EntityLoggerConfig(Func<string> action)
	{
		TableGetter = action;
	}
}

public interface ILoggableEntity
{
	[JsonIgnore]
	public EntityLoggerConfig EntityLogConfiguration { get; }
}
