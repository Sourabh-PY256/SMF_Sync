using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.Models;

#region ActivityProcess

/// <summary>
///
/// </summary>
public class ActivityInstance
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Result { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? PlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IdActivityOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FreqMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FreqValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StatusDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActiveUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ActiveUserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FinishUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StartedUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActiveEmployee { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ActiveDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? FinishDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool CanExecute { get; set; }

	/// <summary>
	///
	/// </summary>
	public ActivityDescription Activity { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivityDescription
{
	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Class { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Intervention { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Asset { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IncludedAssets { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAvailable { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Duration { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RawMaterials { get; set; }

	/// <summary>
	///
	/// </summary>
	public Procedure CurrentProcessMaster { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActivityRequirement> Requirements { get; set; }

	//public List<ActivityInstruction> Instructions { get; set; }
	//public List<ProcedureSection> Sections { get; set; }

	/// <summary>
	///
	/// </summary>
	public ResponseInstruction ResponseInstructions { get; set; }
}

/// <summary>
///
/// </summary>
public class Activity : ICloneable, ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_activity_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ActivityId")]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ActivityClassId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InterventionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int LevelAssetParent { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int AssetLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IncludedAssets { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAvailable { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool RequiresNotifications { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool RequiresInstructions { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TriggerId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int VersionProcedure { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool EditSeries { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsMandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RawMaterials { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool ManualDelete { get; set; }

	/// <summary>
	///
	/// </summary>
	public ActivitySchedule Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public Procedure CurrentProcessMaster { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActivityRequirement> Requirements { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcedureSection> Sections { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActivityInstruction> Instructions { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActivityNotification> Notifications { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActivityInstanceCalculateResponse> ListInstanceResponse = [];

	/// <summary>
	///
	/// </summary>
	public bool? WorkDay { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EventType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Origin { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeShiftStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ShiftCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int AssetLevelCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime FromDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Color { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ToDateParse { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsParent { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsShift { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string TargetId { get; set; }

	/// <summary>
	///
	/// </summary>
	public Activity()
	{
	}

	/// <summary>
	///
	/// </summary>
	public Activity(string id)
	{
		Id = id;
	}

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	public object Clone() => MemberwiseClone();
}

/// <summary>
///
/// </summary>
public class ActivityNotification
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int AlertLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	public int AlertMethod { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ReminderValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ReminderUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RecurValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public int RecurUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ForwardValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ForwardUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RoleId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalData { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivitySchedule : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_activity_schedule_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ActivityId")]
	[JsonIgnore]
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SeriesId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DurationInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Duration { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DurationUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public EndMode EndMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EndValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FrequencyMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double FreqValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DayAuxiliar { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WeekDayAuxiliar { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MonthAuxiliar { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RecurrenceCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DailyCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DailyDays { get; set; }

	/// <summary>
	///
	/// </summary>
	public int NoWeeks { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OptionsEndCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OptionsWeekly { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool OneTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MonthlyDay { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MonthlyEvery { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MonthlyDayCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MonthlyOptionsCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MonthlyOrderDaysCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Occurrences { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MonthlyByYearly { get; set; }

	/// <summary>
	///
	/// </summary>
	public string YearlyOptionsCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EveryYear { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal EveryHour { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivityRequirement
{
	/// <summary>
	///
	/// </summary>
	public int RequirementTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RequirementSourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivityInstruction
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeInstruction { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Instruction { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Mandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Section { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AllowedValues { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeFather { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public ActivityTypeInstructions TypeInstructionsSelected { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Attachment> Files { get; set; }

	/// <summary>
	///
	/// </summary>
	public string URLInstrucction { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool RequestEmployee { get; set; }

	/// <summary>
	///
	/// </summary>
	public string QueryUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status = 1;

	/// <summary>
	///
	/// </summary>
	public int IdProcess;
}

/// <summary>
///
/// </summary>
public class ActivityTypeInstructions
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IdType { get; set; }

	/// <summary>
	///
	/// </summary>
	public TypeData TypeDataSelected { get; set; }

	/// <summary>
	///
	/// </summary>
	public ActivityQuestionType TypeQuestionSelected { get; set; }

	/// <summary>
	///
	/// </summary>
	public TypesDataR TypesDataReading { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivityQuestionType
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IdType { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ValueMax { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ValueMin { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActivityInstructionChoice> MultipleChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActivityInstructionRange> Range { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivityInstructionChoice
{
	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IdInstruction { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Accion { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Section { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ValueChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status = 1;
}

/// <summary>
///
/// </summary>
public class ActivityInstructionRange
{
	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IdInstruction { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Section { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Max { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Min { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status = 1;
}

/// <summary>
///
/// </summary>
public class ActivityClass
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public ActivityClass()
	{
	}

	/// <summary>
	///
	/// </summary>
	public ActivityClass(int id)
	{
		Id = id;
	}
}

/// <summary>
///
/// </summary>
public class ActivityType
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsDowntime { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AffectsOee { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool HasIntervention { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool HasSource { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ClassId { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public ActivityType()
	{
	}

	/// <summary>
	///
	/// </summary>
	public ActivityType(string id)
	{
		Id = id;
	}
}

/// <summary>
///
/// </summary>
public class ActivitySource : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_activitysource_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ActivitySourceCode")]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public ActivitySource()
	{
	}

	/// <summary>
	///
	/// </summary>
	public ActivitySource(string id)
	{
		Id = id;
	}
}

/// <summary>
///
/// </summary>
public class Intervention
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public Intervention(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	public Intervention()
	{
	}
}

#endregion ActivityProcess

#region ActivityNotification

/// <summary>
///
/// </summary>
public class ActivityView
{
	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IncludedAssets { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Duration { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EndMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EndValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public int FreqMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal FreqValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DayAux { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WeekDayAux { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MonthAux { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DurationUnit { get; set; }
}

/// <summary>
///
/// </summary>
public class RoutineInstanceView
{
	/// <summary>
	///
	/// </summary>
	public string InstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Activity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IsDue { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Notifications { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDateUTC { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TriggerId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NotificationId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ResultId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DurationSeconds { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivityInstanceView
{
	/// <summary>
	///
	/// </summary>
	public string InstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Notifications { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NotificationId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ResultId { get; set; }
}

/// <summary>
///
/// </summary>
public class RoutineView
{
	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TriggerId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStartWorkOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime RealStartWorkOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public int StatusWorkOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Duration { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EndMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public object EndValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public FrequencyMode FreqMode { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal FreqValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public object DayAux { get; set; }

	/// <summary>
	///
	/// </summary>
	public object WeekDayAux { get; set; }

	/// <summary>
	///
	/// </summary>
	public object MonthAux { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DurationUnit { get; set; }
}

/// <summary>
///
/// </summary>
[GridBDEntityName("Notification")]
public class NotificationMES
{
	/// <summary>
	///
	/// </summary>
	public string NotificationId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int TypeNotificationId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParamsTitle { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DetailMessage { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IsApp { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SendEmail { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SendSMS { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IsRead { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IsConfirm { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UserRole { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int UserTo { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreatedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Dismiss { get; set; }

	/// <summary>
	///
	/// </summary>
	public int AlertLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkOrderNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Process { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Machine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool RequiresConfirm { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ConfirmBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ConfirmDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? SentDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ReadDate { get; set; }
}

#endregion ActivityNotification

/// <summary>
///
/// </summary>
public class ActivityEventType
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }
}

//public class ActivityInstanceCalculateRequest
//{
//    public string FreqMode { get; set; }
//    public string EndMode { get; set; }
//    public DateTime StartDate { get; set; }
//    public DateTime EndDate { get; set; }
//    public string EndValue { get; set; }
//    public decimal FreqValue { get; set; }
//    public string ActivityId { get; set; }
//}

/// <summary>
///
/// </summary>
public class ActivityInstanceCalculateRequest
{
	/// <summary>
	///
	/// </summary>
	public string RecurrenceCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OptionsEndCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndDateReccurence { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndDate { get; set; }

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
	public string DailyCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DurationInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DailyDays { get; set; }

	/// <summary>
	///
	/// </summary>
	public int NoWeeks { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OptionsWeekly { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool OneTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MonthlyOptionsCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MonthlyDay { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MonthlyEvery { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MonthlyOrderDaysCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MonthlyDayCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool EditSeries { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstanceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsCreateActivity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Occurrences { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MonthlyByYearly { get; set; }

	/// <summary>
	///
	/// </summary>
	public string YearlyOptionsCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EveryYear { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal EveryHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Origin { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Validation { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeShift { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsShift { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivityInstanceCalculateResponse
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Notifications { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NotificationId { get; set; }
}

/// <summary>
///
/// </summary>
public class ActivityInstanceExecuteResponse
{
	/// <summary>
	///
	/// </summary>
	public int ActiveUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActiveEmployee { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsEnable { get; set; }

	/// <summary>
	///
	/// </summary>
	public int StatusOperation { get; set; }
}
