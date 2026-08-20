namespace EWP.SF.Common.Models.IntegrationStaging;

/// <summary>
///
/// </summary>
public class GanttRow
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
	public string IdOrderNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string nameOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public string orderNo { get; set; }

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
	public int Priority { get; set; }

	/// <summary>
	///
	/// </summary>
	public string content { get; set; }

	/// <summary>
	///
	/// </summary>
	public string classestask { get; set; }

	/// <summary>
	///
	/// </summary>
	public string tooltipLabel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string tooltipLabelResource { get; set; }

	/// <summary>
	///
	/// </summary>
	public string toDependency { get; set; }

	/// <summary>
	///
	/// </summary>
	public string jsPlumbDefaults { get; set; }

	/// <summary>
	///
	/// </summary>
	public string colorDependency { get; set; }

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
	public int TotalSetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TotalProcessTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OpNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? SetupStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Efficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Position { get; set; }
}

/// <summary>
///
/// </summary>
public class CalendarTemplate
{
	/// <summary>
	///
	/// </summary>
	public string ResourcesId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Resource { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TemplateId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TemplateName { get; set; }

	/// <summary>
	///
	/// </summary>
	public int StartWeekDay { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StartTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EndWeekDay { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EndTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StateId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string State { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ShiftCheck { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Efficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Color { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OrderByTemplate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Longitud { get; set; }
}

/// <summary>
///
/// </summary>
public class CalendarTemplateByResource
{
	/// <summary>
	///
	/// </summary>
	public string ResourcesId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Resource { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TemplateId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? FromDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ToDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ReferenceDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OrderByTemplateResource { get; set; }
}

/// <summary>
///
/// </summary>
public class ResultGantt
{
	/// <summary>
	///
	/// </summary>
	public List<GanttRow> listGanttRows { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<CalendarTemplate> listCalendarTemplate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<CalendarTemplateByResource> listCalendarTemplateByResource { get; set; }
}
