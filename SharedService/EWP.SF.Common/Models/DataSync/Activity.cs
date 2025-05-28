using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.Models;

#region ActivityProcess

public class ActivityInstance
{
	public string Id { get; set; }
	public string WorkOrderId { get; set; }
	public string ProcessId { get; set; }
	public string MachineId { get; set; }
	public string Code { get; set; }
	public Status Status { get; set; }
	public string Result { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? PlannedStartDate { get; set; }

	public DateTime? EndDate { get; set; }
	public string IdActivityOrder { get; set; }
	public string FreqMode { get; set; }
	public string FreqValue { get; set; }
	public string StatusDescription { get; set; }
	public string ActiveUser { get; set; }
	public int ActiveUserId { get; set; }

	public string FinishUser { get; set; }
	public string StartedUser { get; set; }

	public string ActiveEmployee { get; set; }
	public DateTime? ActiveDate { get; set; }
	public DateTime? FinishDate { get; set; }
	public bool CanExecute { get; set; }

	public ActivityDescription Activity { get; set; }
}

public class ActivityDescription
{
	public string Name { get; set; }
	public string Class { get; set; }
	public string Type { get; set; }
	public string Intervention { get; set; }
	public string Source { get; set; }
	public string Asset { get; set; }

	public string IncludedAssets { get; set; }
	public bool IsAvailable { get; set; }
	public string Description { get; set; }
	public string Duration { get; set; }
	public string RawMaterials { get; set; }
	//public Procedure CurrentProcessMaster { get; set; }
	public List<ActivityRequirement> Requirements { get; set; }

	//public List<ActivityInstruction> Instructions { get; set; }
	//public List<ProcedureSection> Sections { get; set; }

	//public ResponseInstruction ResponseInstructions { get; set; }
}

public class Activity : ICloneable, ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_activity_log");

	[EntityColumn("ActivityId")]
	public string Id { get; set; }
	public string Name { get; set; }
	public int ActivityClassId { get; set; }
	public string ActivityTypeId { get; set; }
	public string InterventionId { get; set; }
	public string SourceId { get; set; }
	public string Image { get; set; }
	public string AssetId { get; set; }
	public int LevelAssetParent { get; set; }
	public string AssetCode { get; set; }
	public string AssetTypeCode { get; set; }
	public int AssetLevel { get; set; }
	public string EmployeeCode { get; set; }
	public string ProcessId { get; set; }
	public string IncludedAssets { get; set; }
	public bool IsAvailable { get; set; }
	public string Description { get; set; }
	public DateTime CreateDate { get; set; }
	public User CreatedBy { get; set; }
	public DateTime ModifyDate { get; set; }
	public User ModifiedBy { get; set; }
	public Status Status { get; set; }
	public bool RequiresNotifications { get; set; }
	public bool RequiresInstructions { get; set; }
	public int TriggerId { get; set; }
	public string ParentId { get; set; }
	public int VersionProcedure { get; set; }
	public bool EditSeries { get; set; }
	public int SortId { get; set; }
	public bool IsMandatory { get; set; }
	public string RawMaterials { get; set; }
	public bool ManualDelete { get; set; }
	public ActivitySchedule Schedule { get; set; }
	public Procedure CurrentProcessMaster { get; set; }
	public List<ActivityRequirement> Requirements { get; set; }
	
	public List<ProcedureSection> Sections { get; set; }
	public List<ActivityInstruction> Instructions { get; set; }
	public List<ActivityNotification> Notifications { get; set; }
	public List<string> AttachmentIds { get; set; }
	public string InstanceId { get; set; }
	public List<ActivityInstanceCalculateResponse> ListInstaceResponse = [];
	public bool? WorkDay { get; set; }
	public string EventType { get; set; }
	public string Origin { get; set; }
	public string CodeShiftStatus { get; set; }
	public string ShiftCode { get; set; }
	public int AssetLevelCode { get; set; }
	public DateTime FromDate { get; set; }
	public string Color { get; set; }
	public DateTime ToDateParse { get; set; }
	public bool IsParent { get; set; }
	public bool IsShift { get; set; }
	[JsonIgnore]
	public string TargetId { get; set; }

	public Activity()
	{
	}

	public Activity(string id)
	{
		Id = id;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}

public class ActivityNotification
{
	public string Id { get; set; }
	[JsonIgnore]
	public string ActivityId { get; set; }
	public int AlertLevel { get; set; }
	public int AlertMethod { get; set; }
	public double ReminderValue { get; set; }
	public int ReminderUnit { get; set; }
	public double RecurValue { get; set; }
	public int RecurUnit { get; set; }
	public double ForwardValue { get; set; }
	public int ForwardUnit { get; set; }
	public string RoleId { get; set; }
	public string ExternalData { get; set; }
}

public class ActivitySchedule : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_activity_schedule_log");

	[EntityColumn("ActivityId")]
	[JsonIgnore]
	public string ActivityId { get; set; }
	public string SeriesId { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public int DurationInSec { get; set; }
	public double Duration { get; set; }
	public int DurationUnit { get; set; }
	public EndMode EndMode { get; set; }
	public string EndValue { get; set; }
	public string FrequencyMode { get; set; }
	public double FreqValue { get; set; }
	public string DayAuxiliar { get; set; }
	public string WeekDayAuxiliar { get; set; }
	public string MonthAuxiliar { get; set; }
	public DateTime CreateDate { get; set; }
	public User CreatedBy { get; set; }
	public DateTime ModifyDate { get; set; }
	public User ModifiedBy { get; set; }
	public string RecurrenceCode { get; set; }
	public string DailyCode { get; set; }
	public int DailyDays { get; set; }
	public int NoWeeks { get; set; }
	public string OptionsEndCode { get; set; }
	public string OptionsWeekly { get; set; }
	public bool OneTime { get; set; }
	public int MonthlyDay { get; set; }
	public int MonthlyEvery { get; set; }
	public string MonthlyDayCode { get; set; }
	public string MonthlyOptionsCode { get; set; }
	public string MonthlyOrderDaysCode { get; set; }
	public string InstanceId { get; set; }
	public int Occurrences { get; set; }
	public int MonthlyByYearly { get; set; }
	public string YearlyOptionsCode { get; set; }
	public int EveryYear { get; set; }
	public decimal EveryHour { get; set; }
}

public class ActivityRequirement
{
	public int RequirementTypeId { get; set; }
	public string RequirementSourceId { get; set; }
	public double Quantity { get; set; }
}

public class ActivityInstruction
{
	public string Id { get; set; }
	public string ActivityId { get; set; }
	public string SectionId { get; set; }
	public int SortId { get; set; }
	public string CodeInstruction { get; set; }
	public string Instruction { get; set; }
	public bool Mandatory { get; set; }
	public string Section { get; set; }
	public bool AllowedValues { get; set; }
	public string CodeFather { get; set; }
	public string Type { get; set; }
	public ActivityTypeInstructions TypeInstructionsSelected { get; set; }
	public string SectionDescription { get; set; }
	public List<Attachment> Files { get; set; }
	public string URLInstrucction { get; set; }
	public bool RequestEmployee { get; set; }
	public string QueryUser { get; set; }

	public int Status = 1;
	public int IdProcess;
}

public class ActivityTypeInstructions
{
	public int Id { get; set; }
	public int IdType { get; set; }
	//public TypeData TypeDataSelected { get; set; }
	public ActivityQuestionType TypeQuestionSelected { get; set; }
	//public TypesDataR TypesDataReading { get; set; }
}

public class ActivityQuestionType
{
	public int Id { get; set; }
	public int IdType { get; set; }
	public int ValueMax { get; set; }
	public int ValueMin { get; set; }
	public List<ActivityInstructionChoice> MultipleChoice { get; set; }
	public List<ActivityInstructionRange> Range { get; set; }
}

public class ActivityInstructionChoice
{
	public string InstructionId { get; set; }
	public string ActivityId { get; set; }
	public int Id { get; set; }
	public int IdInstruction { get; set; }
	public int Accion { get; set; }
	public string Section { get; set; }
	public string ValueChoice { get; set; }

	public int Status = 1;
}

public class ActivityInstructionRange
{
	public string InstructionId { get; set; }
	public string ActivityId { get; set; }
	public int Id { get; set; }
	public int IdInstruction { get; set; }
	public string Section { get; set; }
	public int Max { get; set; }
	public int Min { get; set; }
	public int Status = 1;
}

public class ActivityClass
{
	public int Id { get; set; }
	public string Name { get; set; }
	public DateTime CreationDate { get; set; }
	public User CreatedBy { get; set; }
	public Status Status { get; set; }

	public ActivityClass()
	{
	}

	public ActivityClass(int id)
	{
		Id = id;
	}
}

public class ActivityType
{
	public string Id { get; set; }
	public string Name { get; set; }
	public bool IsDowntime { get; set; }
	public bool AffectsOee { get; set; }
	public bool HasIntervention { get; set; }
	public bool HasSource { get; set; }
	public DateTime CreationDate { get; set; }
	public User CreatedBy { get; set; }
	public int ClassId { get; set; }
	public Status Status { get; set; }

	public ActivityType()
	{
	}

	public ActivityType(string id)
	{
		Id = id;
	}
}

public class ActivitySource : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_activitysource_log");

	[EntityColumn("ActivitySourceCode")]
	public string Id { get; set; }
	public string Name { get; set; }
	public DateTime CreationDate { get; set; }
	public User CreatedBy { get; set; }
	public Status Status { get; set; }

	public ActivitySource()
	{
	}

	public ActivitySource(string id)
	{
		Id = id;
	}
}

public class Intervention
{
	public string Id { get; set; }
	public string Name { get; set; }
	public DateTime CreationDate { get; set; }
	public User CreatedBy { get; set; }
	public Status Status { get; set; }

	public Intervention(string id)
	{
		Id = id;
	}

	public Intervention()
	{
	}
}

#endregion ActivityProcess

#region ActivityNotification

public class ActivityView
{
	public string ActivityId { get; set; }
	public string Name { get; set; }
	public string AssetId { get; set; }
	public string AssetCode { get; set; }
	public string AssetTypeCode { get; set; }
	public string IncludedAssets { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public decimal Duration { get; set; }
	public int EndMode { get; set; }
	public string EndValue { get; set; }
	public int FreqMode { get; set; }
	public decimal FreqValue { get; set; }
	public string DayAux { get; set; }
	public string WeekDayAux { get; set; }
	public string MonthAux { get; set; }
	public int DurationUnit { get; set; }
}

public class RoutineInstanceView
{
	public string InstanceId { get; set; }
	public string WorkOrderId { get; set; }
	public string ProcessId { get; set; }
	public string MachineId { get; set; }
	public string ActivityId { get; set; }
	public string Activity { get; set; }
	public int IsDue { get; set; }
	public int Notifications { get; set; }
	public DateTime StartDateUTC { get; set; }
	public DateTime StartDate { get; set; }
	public decimal Quantity { get; set; }
	public int TriggerId { get; set; }
	public int Status { get; set; }
	public string NotificationId { get; set; }
	public string ResultId { get; set; }
	public int DurationSeconds { get; set; }
}

public class ActivityInstanceView
{
	public string InstanceId { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public string ActivityId { get; set; }
	public int Notifications { get; set; }
	public int Status { get; set; }
	public string NotificationId { get; set; }
	public string ResultId { get; set; }
}

public class RoutineView
{
	public string ActivityId { get; set; }
	public string Name { get; set; }
	public string AssetCode { get; set; }
	public string AssetTypeCode { get; set; }
	public string MachineId { get; set; }
	public string ProcessId { get; set; }
	public string WorkOrderId { get; set; }
	public int TriggerId { get; set; }
	public DateTime CreateDate { get; set; }
	public DateTime PlannedStartWorkOrder { get; set; }
	public DateTime RealStartWorkOrder { get; set; }
	public int StatusWorkOrder { get; set; }
	public decimal Duration { get; set; }
	public int EndMode { get; set; }
	public object EndValue { get; set; }
	public FrequencyMode FreqMode { get; set; }
	public decimal FreqValue { get; set; }
	public object DayAux { get; set; }
	public object WeekDayAux { get; set; }
	public object MonthAux { get; set; }
	public int DurationUnit { get; set; }
}

[GridBDEntityName("Notification")]
public class NotificationMES
{
	public string NotificationId { get; set; }
	[GridIgnoreProperty]
	public int TypeNotificationId { get; set; }
	public string Title { get; set; }
	public string ParamsTitle { get; set; }
	public string Message { get; set; }
	public string DetailMessage { get; set; }
	public int IsApp { get; set; }
	public int SendEmail { get; set; }
	public int SendSMS { get; set; }
	public int IsRead { get; set; }
	public int IsConfirm { get; set; }
	public int UserRole { get; set; }

	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int UserTo { get; set; }
	public DateTime CreatedDate { get; set; }
	public string ActivityId { get; set; }
	public int Dismiss { get; set; }
	public int AlertLevel { get; set; }
	public string WorkOrderId { get; set; }
	public string ProcessId { get; set; }
	public string MachineId { get; set; }
	public string WorkOrderNumber { get; set; }
	public string Process { get; set; }
	public string Machine { get; set; }
	public string InstanceId { get; set; }
	public bool RequiresConfirm { get; set; }
	public string ConfirmBy { get; set; }
	public DateTime? ConfirmDate { get; set; }
	public DateTime? SentDate { get; set; }
	public DateTime? ReadDate { get; set; }
}

#endregion ActivityNotification

public class ActivityEventType
{
	public string Code { get; set; }
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

public class ActivityInstanceCalculateRequest
{
	public string RecurrenceCode { get; set; }
	public string OptionsEndCode { get; set; }
	public DateTime EndDateReccurence { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public string DailyCode { get; set; }
	public string ActivityId { get; set; }
	public int DurationInSec { get; set; }
	public int DailyDays { get; set; }
	public int NoWeeks { get; set; }
	public string OptionsWeekly { get; set; }
	public bool OneTime { get; set; }
	public string MonthlyOptionsCode { get; set; }
	public int MonthlyDay { get; set; }
	public int MonthlyEvery { get; set; }
	public string MonthlyOrderDaysCode { get; set; }
	public string MonthlyDayCode { get; set; }
	public bool EditSeries { get; set; }
	public string Name { get; set; }
	public string InstanceId { get; set; }
	public bool IsCreateActivity { get; set; }
	public int Occurrences { get; set; }
	public int MonthlyByYearly { get; set; }
	public string YearlyOptionsCode { get; set; }
	public int EveryYear { get; set; }
	public decimal EveryHour { get; set; }
	public string Origin { get; set; }
	public bool Validation { get; set; }
	public string CodeShift { get; set; }
	public bool IsShift { get; set; }
}

public class ActivityInstanceCalculateResponse
{
	public string Id { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public string ActivityId { get; set; }
	public string Notifications { get; set; }
	public int Status { get; set; }
	public string NotificationId { get; set; }
}

public class ActivityInstanceExecuteResponse
{
	public int ActiveUser { get; set; }
	public string ActiveEmployee { get; set; }
	public bool IsEnable { get; set; }
	public int StatusOperation { get; set; }
}
