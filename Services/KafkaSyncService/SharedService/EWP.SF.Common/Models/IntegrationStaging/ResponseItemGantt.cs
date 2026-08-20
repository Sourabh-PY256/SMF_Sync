namespace EWP.SF.Common.Models.IntegrationStaging;

/// <summary>
///
/// </summary>
public class ResponseItemGantt
{
	/// <summary>
	///
	/// </summary>
	public string id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string height { get; set; }

	/// <summary>
	///
	/// </summary>
	public string classes { get; set; }

	/// <summary>
	///
	/// </summary>
	public string tooltipLabelResource { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Efficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Order { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<task> tasks { get; set; }

	/// <summary>
	///
	/// </summary>
	public CalendarByResource CalendarByResourceResult { get; set; }
}

/// <summary>
///
/// </summary>
public class CalendarByResource
{
	/// <summary>
	///
	/// </summary>
	public List<CalendarTemplate> listCalendarTemplate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<CalendarTemplateByResource> listCalendarTemplateByResource { get; set; }
}

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
/// <summary>
///
/// </summary>
public class task

#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
{
	/// <summary>
	///
	/// </summary>
	public string id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string color { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime from { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime to { get; set; }

	/// <summary>
	///
	/// </summary>
	public int priority { get; set; }

	/// <summary>
	///
	/// </summary>
	public string content { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Order { get; set; }

	/// <summary>
	///
	/// </summary>
	public string classes { get; set; }

	/// <summary>
	///
	/// </summary>
	public string tooltipLabel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NoOrden { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NoOperacion { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NameOperation { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Product { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DescProduct { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? DueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Resource { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? SetupStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TotalSetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TotalProcessTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Level { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Dependency> dependencies { get; set; }
}

/// <summary>
///
/// </summary>
public class Dependency
{
	/// <summary>
	///
	/// </summary>
	public string to { get; set; }

	/// <summary>
	///
	/// </summary>
	public string jsPlumbDefaults { get; set; }

	/// <summary>
	///
	/// </summary>
	public string color { get; set; }
}
