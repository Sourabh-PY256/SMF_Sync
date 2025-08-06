namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class MachineContextHistory
{
	/// <summary>
	///
	/// </summary>
	public Dictionary<DateTime, WorkOrderToolContext> ToolContext { get; set; }

	/// <summary>
	///
	/// </summary>
	public Dictionary<DateTime, MachineOEEConfiguration> OeeContext { get; set; }
}

/// <summary>
///
/// </summary>
public class WorkOrderToolContext
{
	/// <summary>
	///
	/// </summary>
	public DateTime Date { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }
}
