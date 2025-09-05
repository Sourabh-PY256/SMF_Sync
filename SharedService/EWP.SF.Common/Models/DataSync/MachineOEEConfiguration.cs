

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public enum OEEMode
{
	/// <summary>
	///
	/// </summary>
	Manual = 1,
	/// <summary>
	///
	/// </summary>
	Automatic = 2,
	/// <summary>
	///
	/// </summary>
	AutomaticSwitch = 3
}

/// <summary>
///
/// </summary>
public class MachineOEEConfiguration
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public OEEMode AvailabilityMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AvailabilitySourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AvailabilityOnValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AvailabilityOffValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public double IdleQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public double IdleSeconds { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DowntimeModifierId { get; set; }

	/// <summary>
	///
	/// </summary>
	public OEEMode PerformanceMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PerformanceTriggerId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PerformanceSourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PerformanceTimeSourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int PerformanceDefaultType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PerformanceDefaultValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PerformanceDefaultUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double PerformanceDefaultTimeQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PerformanceDefaultTimeUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double PerformanceTimeFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public OEEMode QualityMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string QualitySourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public object AvailabilitySensor { get; set; }

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
	public bool AdjustTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionType { get; set; }
}

/// <summary>
///
/// </summary>
public class MachineProgramming
{
	/// <summary>
	///
	/// </summary>
	public string CapacityMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string GroupChange { get; set; }

	/// <summary>
	///
	/// </summary>
	public string GanttPosition { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TheoricEfficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public double CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? Attribute2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? Attribute3 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InfinityModeBehavior { get; set; }

	/// <summary>
	///
	/// </summary>
	public short? ScheduleLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool ConcurrentSetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Planning { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<MachineProgrammingDetail> Details { get; set; }

	/// <summary>
	///
	/// </summary>
	public string GetJsonDetails()
	{
		string returnValue = string.Empty;
		if (Details is not null)
		{
			returnValue = JsonConvert.SerializeObject(Details);
		}
		return returnValue;
	}
}

/// <summary>
///
/// </summary>
public class MachineProgrammingDetail
{
	/// <summary>
	///
	/// </summary>
	public int Sort { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CriteriaType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SortType { get; set; }
}
