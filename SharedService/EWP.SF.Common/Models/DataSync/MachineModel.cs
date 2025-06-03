using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.CustomBehavior;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models.Sensors;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class MachineModel
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Icon { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationalState { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionLine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StagingBin { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Warehouse { get; set; }
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime LastMaintenanceDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Manufacturer { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SerialNumber { get; set; }

	/// <summary>*****
	///
	/// </summary>
	public string CtrlModel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CtrlSerial { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ManufactureDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PLCManufacturer { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PLCSerial { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PwrSourceModel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RobotArmModel { get; set; }

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
	public string CapacityMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string GroupChange { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Capacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InfinityModeBehavior { get; set; }

	/// <summary>
	///
	/// </summary>
	public double GanttPosition { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TheoricEfficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public double CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ScheduleLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Attribute2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Attribute3Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool ConcurrentSetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	////////////****
	public int AvailabilityMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionType { get; set; }

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
	public bool AdjustTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MinimumCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MaximumCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LotCalculation { get; set; }

	/// <summary>
	///
	/// </summary>
	public int PerformanceMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int PerformanceDefaultType { get; set; }

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
	public string PerformanceDefaultValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public int QualityMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string QualitySourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<MachineProgrammingDetail> ProgrammingDetails { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> Skill { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Activities { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts ShiftDelete { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts Shift { get; set; }
}
