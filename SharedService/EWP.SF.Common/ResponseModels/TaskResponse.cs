
using EWP.SF.Common.Attributes;
using EWP.SF.Common.Models;

namespace EWP.SF.Common.ResponseModels;

/// <summary>
///
/// </summary>
[GridBDEntityName("Task")]
public class TaskResponse
{
	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Class { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDrillDown]
	[GridDisabledHiding]
	public string TaskId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("ProductionOrder", "Code")]
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("User", "Code")]
	public string StartedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("User", "Code")]
	public string ActiveUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LockedUser { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("User", "Code")]
	public string FinishUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal PlannedDuration { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ActualStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ActualEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal ActualDuration { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal FreqValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal RepeatEvery { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal EndValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FrequencyModeBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Origin { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ResponseId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int NextSection { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int SortResponse { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int CurrentSection { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int NoSections { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int NoInstructions { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool CanStart { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool CanResume { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool CanFinish { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string SectionResponseArray { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int SortIdSectionCurrent { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public List<LayoutProcedure> Layout { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public List<Section> Sections { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public List<TaskAttachment> Attachments { get; set; }

	/// <summary>
	///
	/// </summary>
	public TaskResponse()
	{
		Sections = [];
	}
}

/// <summary>
///
/// </summary>
public class Section
{
	/// <summary>
	///
	/// </summary>
	public int SortId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionType { get; set; }

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
	public string Observations { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortResponse { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Instruction> Instructions { get; set; }

	/// <summary>
	///
	/// </summary>
	public Section()
	{
		Instructions = [];
	}
}

/// <summary>
///
/// </summary>
public class Instruction
{
	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeInput { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ResponseValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsMandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsGauge { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Length { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsDecimal { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeDataReading { get; set; }

	/// <summary>
	///
	/// </summary>
	public TimeSpan Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DefaultValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MinValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MaxValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal TargetValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeAutomatic { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SignalCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool MultiSelect { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Query { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ChoiceInstruction> Choices { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<MultipleCondition> MultipleConditions { get; set; }
}

/// <summary>
///
/// </summary>
public class ChoiceInstruction
{
	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Min { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Max { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActionTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsNotify { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MessageNotify { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Selected { get; set; }
}

/// <summary>
///
/// </summary>
public class MultipleCondition
{
	/// <summary>
	///
	/// </summary>
	public int SortIdInstruction { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortIdCondition { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Condition { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MultipleActionType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MultipleMessage { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool MultipleIsNotify { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MultipleMessageNotify { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ChoiceDescriptions { get; set; }
}

/// <summary>
///
/// </summary>
public class TaskAttachment
{
	/// <summary>
	///
	/// </summary>
	public int SortSection { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortInstruction { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Entity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Extension { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PathVisualHelp { get; set; }
}
