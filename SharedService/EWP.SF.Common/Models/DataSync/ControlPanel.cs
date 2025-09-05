using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.Constants;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class ControlPanel;

/// <summary>
///
/// </summary>
public class Procedure : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_procedure_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ProcedureId")]
	[GridCustomPropertyName("Id")]
	public string ProcedureId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ProcedureIdOrigin { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Type")]
	public int ClassType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool CreateNewVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool CreateVersionSync { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool IsDuplicate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EarlierVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	public int LastVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Class")]
	[GridRequireTranslate]
	public string ClassName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	//  [EntityColumn("Code")]
	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	// [GridLookUpEntity(null, "Status", "Id", "Name")]
	// [GridRequireTranslate]
	[GridIgnoreProperty]
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string StatusDescription { get; set; }

	//[EntityColumn("Version")]
	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool IsNewProcess { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool IsProcessNewActivity { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Status")]
	[GridRequireTranslate]
	public string NameStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Snapshot { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityType { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool HasIntervention { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool HasSource { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int IdActivityClass { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string IdTypeClass { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string InterventionId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string SourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string RowCurrent { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Original { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime LastUpdate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcedureSection> Sections { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool IsByProcedure { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool IsManualActivity { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcedureVerions> Versions { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<LayoutProcedure> Layout { get; set; }

	/// <summary>
	///
	/// </summary>
	public object Clone() => MemberwiseClone();
}
/// <summary>
///
/// </summary>
public class LayoutProcedure
{
	/// <summary>
	///
	/// </summary>
	public bool LazyLoad { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal PositionX { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal PositionY { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Height { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Width { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcedureVerions
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime UpdateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcedureVerionsBySync
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime UpdateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcedureSection
{
	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcedureId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TypeSection { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeSectionDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Section { get; set; }

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
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OrderSection { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public int CreatedById { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsNewSection { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderSectionTxt { get; set; }

	/// <summary>
	///
	/// </summary>
	public int ModifiedById { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcedureMasterInstruction> ListInstrucctions { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProcedureMasterSectionAttachment Attachment { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcedureMasterInstruction
{
	/// <summary>
	///
	/// </summary>
	public int? CodeInstruction { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Instruction { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SectionOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionDisplayTitle { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeInstrucction { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? ViewType { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool MultiSelect { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Mandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	//  public DataInst DataInstrucction { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DefaultValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsGauge { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? Long { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActionChoice> ActionCheckBox { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Choice> MultipleChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Choice> MultipleChoiceCheckBox { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Range> Range { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ActionOperator> MultipleActionOperator { get; set; }

	/// <summary>
	///
	/// </summary>
	public string QueryUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SignalCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsDecimal { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? TypeDataReading { get; set; }

	/// <summary>
	///
	/// </summary>
	public long TimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

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
	public string URLInstrucction { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Response { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ComponentInstruction> Components { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<OrderComponentInstruction> OrderComponents { get; set; }
}

/// <summary>
///
/// </summary>
public class OrderComponentInstruction
{
	/// <summary>
	///
	/// </summary>
	public string OrderCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationNo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalMachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public ComponentType ComponentType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BatchId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TargetQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TargetUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double InputQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InputUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAuxiliarDevice { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OriginalSourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double NewFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MaterialImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineUID { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ManagedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public double QuantityStage { get; set; }

	/// <summary>
	///
	/// </summary>
	public double RequiredQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MaterialType { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ComponentBatch> Batches { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BinLocationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Origin { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LotNumber { get; set; }
}

/// <summary>
///
/// </summary>
public class DataInst;

/// <summary>
///
/// </summary>
public class ActionChoice
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> ValueChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

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
	public int OrderChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Selected { get; set; }
	public string ActionTypeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ActionChoiceDB
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ValueChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

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
	public int OrderChoice { get; set; }
	public string ActionTypeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class Choice
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
	public string OldId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ValueChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Operator { get; set; }

	//public string image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

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
	public int OrderChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProcedureMasterSectionAttachment Attachment { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Selected { get; set; }
}

/// <summary>
///
/// </summary>
public class ActionOperator
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
	public string ValueChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Operator { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OrderChoice { get; set; }
}

/// <summary>
///
/// </summary>
public class TypeInstructions
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
	public QuestionType TypeQuestionSelected { get; set; }

	/// <summary>
	///
	/// </summary>
	public TypesDataR TypesDataReading { get; set; }
}

/// <summary>
///
/// </summary>
public class QuestionType
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
	public List<Choice> MultipleChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Range> Range { get; set; }
}

/// <summary>
///
/// </summary>
public class TypeData
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IdType { get; set; }
}

/// <summary>
///
/// </summary>
public class TypesDataR
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
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FallbackValue { get; set; }
}

/// <summary>
///
/// </summary>
public class Range
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Max { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Min { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

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
	public int OrderChoice { get; set; }
}

/// <summary>
///
/// </summary>
public class RequestDeleteSection
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }
}

/// <summary>
///
/// </summary>
public class RequestUpdateExcel
{
	/// <summary>
	///
	/// </summary>
	public string Json { get; set; }
}

/// <summary>
///
/// </summary>
public class QueryUser
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }
	public string Query { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<VariableQuery> Variables { get; set; }
}

/// <summary>
///
/// </summary>
public class VariableQuery
{
	/// <summary>
	///
	/// </summary>
	public int Variable { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DataType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Context { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TableName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TableComment { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ColumnName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ColumnComment { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsForeignKey { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ReferencedTable { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ReferencedColumn { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcedureVersion
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcedureId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcedureIdOrigin { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IdOrigin { get; set; }

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
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StatusDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime Date { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsActivityUsed { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcedureMasterSectionAttachment
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string URL { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Extension { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SectionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsFileOffice { get; set; }

	/// <summary>
	///
	/// </summary>
	public FileAttachment File { get; set; }
}

/// <summary>
///
/// </summary>
public class FileAttachment
{
	/// <summary>
	///
	/// </summary>
	public string name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string FileBase64 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string lastModified { get; set; }

	/// <summary>
	///
	/// </summary>
	public string lastModifiedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string size { get; set; }

	/// <summary>
	///
	/// </summary>
	public string type { get; set; }
}

/// <summary>
///
/// </summary>
public class ComponentInstruction
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcedureId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeComponent { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsSubProduct { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Line { get; set; }

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
	public decimal Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal QuantityIssue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal QuantityAvailable { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Tolerance { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string NameAttachment { get; set; }

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
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Quantitytext { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Mandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsRemainingTotal { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MaterialImage { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessMasterVersionresult
{
	/// <summary>
	///
	/// </summary>
	public string ProcedureId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcedureIdOrigin { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EarlierVersion { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessMasterAttachmentDetail
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttachmentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AuxId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Entity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcedureExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Procedure Code format.")]
	public string ProcedureCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EarlierVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(200)]
	public string ProcedureName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string ProcedureClass { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SectionExternal> Sections { get; set; }
}

/// <summary>
///
/// </summary>
public class SectionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(64)]
	public string ProcedureCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	public int SectionOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string SectionType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(255)]
	public string SectionName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	public string SectionDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeVisualHelp { get; set; }

	/// <summary>
	///
	/// </summary>
	public string VisualHelp { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<InstrucctionExternal> Instrucctions { get; set; }
}

/// <summary>
///
/// </summary>
public class InstrucctionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(64)]
	public string ProcedureCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public int SectionOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public int InstructionOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionTypeInput { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionMandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string ViewType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string MultiSelect { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string DataReadingSignalCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int InstructionLongText { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionTypeDataReading { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string DataReadingCodeAutomatic { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string DataReadingDefaultValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionIsGauge { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string GaugeTargetValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string GaugeMinValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string GaugeMaxValue { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string GaugeAction { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionRange { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionQueryUser { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionSacale { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionTimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string TypeInstrucction { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	public int ChoiceOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ChoiceValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ChoiceAction { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeVisualHelp { get; set; }

	/// <summary>
	///
	/// </summary>
	public string VisualHelp { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcedureExternalSync
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Procedure Code format.")]
	public string ProcedureCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EarlierVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(200)]
	public string ProcedureName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string ClassCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ActivityTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InterventionCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression(RegularExpression.ProductStatusIntegration, ErrorMessage = "Invalid Status")]
	[Description("Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SectionExternalSync> Sections { get; set; }
}

/// <summary>
///
/// </summary>
public class SectionExternalSync
{
	/// <summary>
	///
	/// </summary>
	[Key]
	public int SectionOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string SectionType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(255)]
	public string SectionName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	public string SectionDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Observations { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeVisualHelp { get; set; }

	/// <summary>
	///
	/// </summary>
	public string VisualHelp { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<InstrucctionExternalSync> Instructions { get; set; }
}

/// <summary>
///
/// </summary>
public class InstrucctionExternalSync
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(64)]
	public int InstructionOrder { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public bool InstructionMandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstructionQueryUser { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string InstructionDataType { get; set; }

	/// <summary>
	///
	/// </summary>
	public int InstructionLongText { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeDataReading { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsDecimal { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsGauge { get; set; }

	//falta mapear
	/// <summary>
	///
	/// </summary>
	public string DataReadingSignalCode { get; set; }

	//falta mapear
	/// <summary>
	///
	/// </summary>
	public string CodeAutomatic { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal GaugeTargetValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal GaugeMinValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal GaugeMaxValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MaxValueLinearScale { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal MinValueLinearScale { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TagType { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal DefaultValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public long TimeInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string ViewType { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string MultiSelect { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ChoiceExternalSync> Choices { get; set; }
}

/// <summary>
///
/// </summary>
public class ChoiceExternalSync
{
	/// <summary>
	///
	/// </summary>
	[Key]
	public int OrderChoice { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	public string Action { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsNotify { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OrderSectionGoTo { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MessageNotify { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MessageWarning { get; set; }
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum ClassTypeProcedure
{
	/// <summary>
	///
	/// </summary>
	Production = 1,
	/// <summary>
	///
	/// </summary>
	Maintenance = 2,
	/// <summary>
	///
	/// </summary>
	Quality = 3,
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum SectionType
{
	/// <summary>
	///
	/// </summary>
	Question = 1,
	/// <summary>
	///
	/// </summary>
	Transaction = 2,
	/// <summary>
	///
	/// </summary>
	Timer = 3,
	/// <summary>
	///
	/// </summary>
	Chronometer = 4
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum InstructionType
{
	/// <summary>
	///
	/// </summary>
	Input = 1,
	/// <summary>
	///
	/// </summary>
	MultipleChoice = 2,
	/// <summary>
	///
	/// </summary>
	LinearScale = 3,
	/// <summary>
	///
	/// </summary>
	IssueMaterials = 4,
	/// <summary>
	///
	/// </summary>
	ProductReceipt = 5,
	/// <summary>
	///
	/// </summary>
	ReturnMaterial = 6,
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum ViewTypes
{
	/// <summary>
	///
	/// </summary>
	Tiles = 1,
	/// <summary>
	///
	/// </summary>
	List = 2,
	/// <summary>
	///
	/// </summary>
	RadioButton = 3,
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum InputType
{
	/// <summary>
	///
	/// </summary>
	Text = 1,
	/// <summary>
	///
	/// </summary>
	Integer = 2,
	/// <summary>
	///
	/// </summary>
	Decimal = 3,
	/// <summary>
	///
	/// </summary>
	Date = 4,
	/// <summary>
	///
	/// </summary>
	Time = 5,
	/// <summary>
	///
	/// </summary>
	Query = 7,
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum DataReadingType
{
	/// <summary>
	///
	/// </summary>
	Manual = 1,
	/// <summary>
	///
	/// </summary>
	Automatic = 2,
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum DataReadingTypeChanges
{
	/// <summary>
	///
	/// </summary>
	Manual = 1,
	/// <summary>
	///
	/// </summary>
	Automatic = 2,
}

/// <summary>
///
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum ActionTypeCode
{
	/// <summary>
	///
	/// </summary>
	noaction = 1,
	/// <summary>
	///
	/// </summary>
	block = 2,
	/// <summary>
	///
	/// </summary>
	finish = 3,
	/// <summary>
	///
	/// </summary>
	finisherror = 4,
	/// <summary>
	///
	/// </summary>
	Goto = 5,
	/// <summary>
	///
	/// </summary>
	FinisWithWarning = 6
}
