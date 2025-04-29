using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.ShopFloor.BusinessEntities;

[GridBDEntityName("Task")]
public class TaskResponse
{
	[GridIgnoreProperty]
	public string Id { get; set; }
	public string Class { get; set; }
	public string Status { get; set; }
	[GridRequireDecode]
	[GridDrillDown]
	[GridDisabledHiding]
	public string TaskId { get; set; }
	[GridDrillDown("ProductionOrder", "Code")]
	public string OrderCode { get; set; }
	public string OperationNo { get; set; }
	[GridDrillDown("User", "Code")]
	public string StartedBy { get; set; }
	[GridDrillDown("User", "Code")]
	public string ActiveUser { get; set; }
	public string LockedUser { get; set; }
	[GridDrillDown("User", "Code")]
	public string FinishUser { get; set; }
	public DateTime PlannedStartDate { get; set; }
	public DateTime PlannedEndDate { get; set; }
	public decimal PlannedDuration { get; set; }
	public string Description { get; set; }
	public DateTime? ActualStart { get; set; }
	public DateTime? ActualEnd { get; set; }
	public decimal ActualDuration { get; set; }
	public decimal FreqValue { get; set; }
	public decimal RepeatEvery { get; set; }
	public decimal EndValue { get; set; }
	public string FrequencyModeBy { get; set; }
	public string Origin { get; set; }
	[GridIgnoreProperty]
	public string ResponseId { get; set; }
	[GridIgnoreProperty]
	public int NextSection { get; set; }
	[GridIgnoreProperty]
	public int SortResponse { get; set; }
	[GridIgnoreProperty]
	public int CurrentSection { get; set; }
	[GridIgnoreProperty]
	public int NoSections { get; set; }
	[GridIgnoreProperty]
	public int NoInstructions { get; set; }
	[GridIgnoreProperty]
	public bool CanStart { get; set; }
	[GridIgnoreProperty]
	public bool CanResume { get; set; }
	[GridIgnoreProperty]
	public bool CanFinish { get; set; }
	[GridIgnoreProperty]
	public string SectionResponseArray { get; set; }
	[GridIgnoreProperty]
	public int SortIdSectionCurrent { get; set; }
	[GridIgnoreProperty]
	public List<LayoutProcedure> Layout { get; set; }
	[GridIgnoreProperty]
	public List<Section> Sections { get; set; }
	[GridIgnoreProperty]
	public List<TaskAttachment> Attachments { get; set; }
	public TaskResponse()
	{
		Sections = [];
	}
}

public class Section
{
	public int SortId { get; set; }
	public string SectionType { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string Observations { get; set; }
	public string SectionStatus { get; set; }

	public int SortResponse { get; set; }
	public List<Instruction> Instructions { get; set; }

	public Section()
	{
		Instructions = [];
	}
}

public class Instruction
{
	public string InstructionId { get; set; }
	public string InstructionName { get; set; }
	public string Description { get; set; }
	public int SortId { get; set; }
	public string Type { get; set; }
	public string TypeInput { get; set; }
	public string ResponseValue { get; set; }
	public bool IsMandatory { get; set; }
	public bool IsGauge { get; set; }
	public decimal Length { get; set; }
	public bool IsDecimal { get; set; }
	public string TypeDataReading { get; set; }
	public TimeSpan Time { get; set; }
	public string DefaultValue { get; set; }
	public decimal MinValue { get; set; }
	public decimal MaxValue { get; set; }
	public decimal TargetValue { get; set; }
	public string CodeAutomatic { get; set; }
	public string SignalCode { get; set; }
	public bool MultiSelect { get; set; }

	public string Query { get; set; }
	public List<ChoiceInstruction> Choices { get; set; }
	public List<MultipleCondition> MultipleConditions { get; set; }
}

public class ChoiceInstruction
{
	public string InstructionId { get; set; }
	public string Id { get; set; }
	public int SortId { get; set; }
	public string Description { get; set; }
	public decimal Min { get; set; }
	public decimal Max { get; set; }
	public string Message { get; set; }
	public string ActionTypeCode { get; set; }
	public bool IsNotify { get; set; }
	public string MessageNotify { get; set; }
	public string Image { get; set; }
	public bool Selected { get; set; }
}

public class MultipleCondition
{
	public int SortIdInstruction { get; set; }
	public int SortIdCondition { get; set; }
	public string Condition { get; set; }
	public string MultipleActionType { get; set; }
	public string MultipleMessage { get; set; }
	public bool MultipleIsNotify { get; set; }
	public string MultipleMessageNotify { get; set; }
	public string ChoiceDescriptions { get; set; }
}

public class TaskAttachment
{
	public int SortSection { get; set; }
	public int SortInstruction { get; set; }
	public int SortChoice { get; set; }
	public string Entity { get; set; }
	public string Type { get; set; }

	public string Extension { get; set; }

	public string Name { get; set; }
	public string PathVisualHelp { get; set; }
}
