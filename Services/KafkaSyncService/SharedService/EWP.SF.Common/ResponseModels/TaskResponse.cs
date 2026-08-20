
using EWP.SF.Common.Attributes;
using EWP.SF.Common.Models;

namespace EWP.SF.Common.ResponseModels;

/// <summary>
/// Represents the basic header of a task in the system.
/// </summary>
[GridBDEntityName("Task")]
public class TaskResponseHeader
{
	/// <summary>
	/// Class or category of the task.
	/// </summary>
	public string Class { get; set; }

	/// <summary>
	/// Status of the task, requires translation.
	/// </summary>
	[GridRequireTranslate]
	public string Status { get; set; }

	/// <summary>
	/// Unique identifier of the task, requires decode and drill-down functionality.
	/// </summary>
	[GridRequireDecode]
	[GridDrillDown]
	[GridDisabledHiding]
	public string TaskId { get; set; }

	/// <summary>
	/// Production order code, with drill-down to ProductionOrder.
	/// </summary>
	[GridDrillDown("ProductionOrder", "Code")]
	public string OrderCode { get; set; }

	/// <summary>
	/// Operation number related to the task.
	/// </summary>
	public string OperationNo { get; set; }

	/// <summary>
	/// User who started the task, with drill-down to User.
	/// </summary>
	[GridDrillDown("User", "Code")]
	public string StartedBy { get; set; }

	/// <summary>
	/// Currently active user, with drill-down to User.
	/// </summary>
	[GridDrillDown("User", "Code")]
	public string ActiveUser { get; set; }

	/// <summary>
	/// User who locked the task.
	/// </summary>
	public string LockedUser { get; set; }

	/// <summary>
	/// User who finished the task, with drill-down to User.
	/// </summary>
	[GridDrillDown("User", "Code")]
	public string FinishUser { get; set; }

	/// <summary>
	/// Description of the task.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Origin of the task.
	/// </summary>
	public string Origin { get; set; }
}

/// <summary>
/// Represents a detailed task with sections and additional metadata.
/// </summary>
public class TaskResponse : TaskResponseHeader
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
	[GridRequireTranslate]
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
	[GridIgnoreProperty]
	public DateTime PlannedStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public DateTime PlannedEndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal PlannedDuration { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public DateTime? ActualStart { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public DateTime? ActualEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal ActualDuration { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal FreqValue { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal RepeatEvery { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public decimal EndValue { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
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
	public bool CanReloadItems { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string FreqUnit { get; set; }

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
	/// List of items related to the task .
	/// </summary>
	[GridIgnoreProperty]
	public List<ItemActivity> Items { get; set; }

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
	public int OriginalSortId { get; set; }
	/// <summary>
	///
	/// </summary>
	public int SortIdItem { get; set; }

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

	public bool Selected { get; set; }
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
public class ItemActivity
{
	/// <summary>
	///
	/// </summary>
	public int SortSection { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemCode { get; set; }
	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SortId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsByProduct { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal AvailableQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal IssueQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Tolerance { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MinQtyAllowed { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MaxQtyAllowed { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Method { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeSignal { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Mandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsRemainingTotal { get; set; }

	/// <summary>
	/// ///
	/// </summary>
	public string TransactionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PathVisualHelp { get; set; }
	/// <summary>
	///
	/// </summary>
	public decimal QuantityStage { get; set; }
	/// <summary>
	///
	/// </summary>
	public decimal TargetQty { get; set; }
	/// <summary>
	///
	/// </summary>
	public bool Passed { get; set; }
}
