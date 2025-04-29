using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Constants;
using System.ComponentModel;

namespace EWP.SF.ShopFloor.BusinessEntities;

public class ControlPanel;

public class Procedure : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_procedure_log");

	[EntityColumn("ProcedureId")]
	[GridCustomPropertyName("Id")]
	public string ProcedureId { get; set; }

	[GridIgnoreProperty]
	public string ActivityId { get; set; }

	[GridIgnoreProperty]
	public string ProcedureIdOrigin { get; set; }

	[GridCustomPropertyName("Type")]
	public int ClassType { get; set; }

	[GridIgnoreProperty]
	public bool CreateNewVersion { get; set; }

	[GridIgnoreProperty]
	public bool CreateVersionSync { get; set; }

	[GridIgnoreProperty]
	public bool IsDuplicate { get; set; }

	public int EarlierVersion { get; set; }
	public int LastVersion { get; set; }

	[GridIgnoreProperty]
	public int Id { get; set; }

	[GridCustomPropertyName("Class")]
	[GridRequireTranslate]
	public string ClassName { get; set; }

	public string Name { get; set; }
	public string Description { get; set; }

	//  [EntityColumn("Code")]
	[GridDrillDown]
	public string Code { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public int Status { get; set; }

	[GridIgnoreProperty]
	public string StatusDescription { get; set; }

	//[EntityColumn("Version")]
	public int Version { get; set; }

	[GridIgnoreProperty]
	public bool IsNewProcess { get; set; }

	[GridIgnoreProperty]
	public bool IsProcessNewActivity { get; set; }

	[GridIgnoreProperty]
	public string NameStatus { get; set; }

	[GridIgnoreProperty]
	public string Snapshot { get; set; }

	public string ActivityType { get; set; }

	[GridIgnoreProperty]
	public bool HasIntervention { get; set; }

	[GridIgnoreProperty]
	public bool HasSource { get; set; }

	[GridIgnoreProperty]
	public int IdActivityClass { get; set; }

	[GridIgnoreProperty]
	public string IdTypeClass { get; set; }

	[GridIgnoreProperty]
	public string InterventionId { get; set; }

	[GridIgnoreProperty]
	public string SourceId { get; set; }

	[GridIgnoreProperty]
	public string RowCurrent { get; set; }

	[GridIgnoreProperty]
	public string Original { get; set; }

	public DateTime LastUpdate { get; set; }
	public List<ProcedureSection> Sections { get; set; }

	[GridIgnoreProperty]
	public bool IsByProcedure { get; set; }

	[GridIgnoreProperty]
	public bool IsManualActivity { get; set; }

	public List<ProcedureVerions> Versions { get; set; }

	public List<LayoutProcedure> Layout { get; set; }

	public object Clone()
	{
		return MemberwiseClone();
	}
}
public class LayoutProcedure
{
	public bool LazyLoad { get; set; }
	public decimal PositionX { get; set; }
	public decimal PositionY { get; set; }
	public decimal Height { get; set; }
	public decimal Width { get; set; }
	public string Id { get; set; }
}

public class ProcedureVerions
{
	public string Code { get; set; }
	public int Version { get; set; }
	public DateTime UpdateDate { get; set; }
	public int Status { get; set; }
}

public class ProcedureVerionsBySync
{
	public string Code { get; set; }
	public int Version { get; set; }
	public DateTime UpdateDate { get; set; }
	public int Status { get; set; }
}

public class ProcedureSection
{
	public string SectionId { get; set; }
	public string ProcedureId { get; set; }
	public string Code { get; set; }
	public int Version { get; set; }
	public int TypeSection { get; set; }
	public string SectionType { get; set; }

	public string TypeSectionDescription { get; set; }
	public string Section { get; set; }
	public string Description { get; set; }
	public string Observations { get; set; }
	public int Status { get; set; }
	public string Image { get; set; }
	public int OrderSection { get; set; }
	public DateTime CreationDate { get; set; }
	public int CreatedById { get; set; }
	public DateTime ModifyDate { get; set; }
	public bool IsNewSection { get; set; }
	public string OrderSectionTxt { get; set; }

	public int ModifiedById { get; set; }
	public string AttachmentId { get; set; }
	public List<ProcedureMasterInstruction> ListInstrucctions { get; set; }

	public ProcedureMasterSectionAttachment Attachment { get; set; }
}

public class ProcedureMasterInstruction
{
	public int? CodeInstruction { get; set; }
	public string Instruction { get; set; }
	public string Code { get; set; }
	public int Version { get; set; }
	public int SectionOrder { get; set; }

	public string InstructionDisplayTitle { get; set; }
	public string InstructionId { get; set; }
	public string ProcessId { get; set; }
	public string TypeInstrucction { get; set; }
	public int? ViewType { get; set; }
	public bool MultiSelect { get; set; }
	public bool Mandatory { get; set; }
	public string SectionId { get; set; }

	//  public DataInst DataInstrucction { get; set; }
	public string DefaultValue { get; set; }

	public bool IsGauge { get; set; }
	public int? Long { get; set; }

	public List<ActionChoice> ActionCheckBox { get; set; }
	public List<Choice> MultipleChoice { get; set; }
	public List<Choice> MultipleChoiceCheckBox { get; set; }
	public List<Range> Range { get; set; }
	public List<ActionOperator> MultipleActionOperator { get; set; }
	public string QueryUser { get; set; }
	public string SignalCode { get; set; }
	public int? Type { get; set; }
	public bool IsDecimal { get; set; }
	public int? TypeDataReading { get; set; }
	public long TimeInSec { get; set; }
	public int Status { get; set; }
	public decimal MinValue { get; set; }
	public decimal MaxValue { get; set; }
	public decimal TargetValue { get; set; }
	public string CodeAutomatic { get; set; }
	public string URLInstrucction { get; set; }
	public string Response { get; set; }
	public List<ComponentInstruction> Components { get; set; }
	public List<OrderComponentInstruction> OrderComponents { get; set; }
}

public class OrderComponentInstruction
{
	public string OrderCode { get; set; }
	public string MachineId { get; set; }
	public string OperationNo { get; set; }

	public string OriginalMachineId { get; set; }
	public int Step { get; set; }
	public string ProcessTypeId { get; set; }
	public ComponentType ComponentType { get; set; }
	public string SourceTypeId { get; set; }
	public string SourceId { get; set; }
	public string BatchId { get; set; }
	public double TargetQty { get; set; }
	public string TargetUnitId { get; set; }
	public double InputQty { get; set; }
	public string InputUnitId { get; set; }
	public string ProcessId { get; set; }
	public bool IsAuxiliarDevice { get; set; }
	public Status Status { get; set; }
	public string OriginalSourceId { get; set; }
	public double NewFactor { get; set; }
	public string ExternalId { get; set; }
	public string WarehouseCode { get; set; }

	public string ComponentName { get; set; }
	public string ComponentCode { get; set; }
	public string MaterialImage { get; set; }
	public string LineId { get; set; }

	public string LineUID { get; set; }
	public bool IsBackflush { get; set; }

	public string ManagedBy { get; set; }
	public double QuantityStage { get; set; }
	public double RequiredQuantity { get; set; }
	public string Source { get; set; }
	public int MaterialType { get; set; }
	public List<ComponentBatch> Batches { get; set; }
	public string Pallet { get; set; }
	public string BinLocationCode { get; set; }
	public string InventoryStatusCode { get; set; }
	public DateTime ExpDate { get; set; }
	public string Origin { get; set; }
	public string MachineCode { get; set; }
	public string LotNumber { get; set; }
}

public class DataInst;

public class ActionChoice
{
	public string Id { get; set; }
	public string InstructionId { get; set; }
	public List<string> ValueChoice { get; set; }
	public string SectionId { get; set; }
	public string Message { get; set; }
	public bool IsNotify { get; set; }
	public string MessageNotify { get; set; }
	public int OrderChoice { get; set; }
	public bool Selected { get; set; }
}

public class ActionChoiceDB
{
	public string Id { get; set; }
	public string InstructionId { get; set; }
	public string ValueChoice { get; set; }
	public string SectionId { get; set; }
	public string Message { get; set; }
	public bool IsNotify { get; set; }
	public string MessageNotify { get; set; }
	public int OrderChoice { get; set; }
}

public class Choice
{
	public string InstructionId { get; set; }
	public string Id { get; set; }
	public string OldId { get; set; }
	public string ValueChoice { get; set; }
	public string SectionId { get; set; }
	public int Operator { get; set; }

	//public string image { get; set; }
	public string Message { get; set; }

	public bool IsNotify { get; set; }
	public string MessageNotify { get; set; }
	public int OrderChoice { get; set; }
	public string AttachmentId { get; set; }
	public ProcedureMasterSectionAttachment Attachment { get; set; }
	public bool Selected { get; set; }
}

public class ActionOperator
{
	public string InstructionId { get; set; }
	public string Id { get; set; }
	public string ValueChoice { get; set; }
	public string SectionId { get; set; }
	public int Operator { get; set; }
	public string Message { get; set; }
	public int OrderChoice { get; set; }
}

public class TypeInstructions
{
	public int Id { get; set; }
	public int IdType { get; set; }
	public TypeData TypeDataSelected { get; set; }
	public QuestionType TypeQuestionSelected { get; set; }
	public TypesDataR TypesDataReading { get; set; }
}

public class QuestionType
{
	public int Id { get; set; }
	public int IdType { get; set; }
	public int ValueMax { get; set; }
	public int ValueMin { get; set; }
	public List<Choice> MultipleChoice { get; set; }
	public List<Range> Range { get; set; }
}

public class TypeData
{
	public int Id { get; set; }
	public int IdType { get; set; }
}

public class TypesDataR
{
	public int Id { get; set; }
	public int IdType { get; set; }
	public string Code { get; set; }
	public string FallbackValue { get; set; }
}

public class Range
{
	public string Id { get; set; }
	public string InstructionId { get; set; }
	public decimal Max { get; set; }
	public decimal Min { get; set; }
	public string SectionId { get; set; }
	public string Message { get; set; }
	public bool IsNotify { get; set; }
	public string MessageNotify { get; set; }
	public int OrderChoice { get; set; }
}

public class RequestDeleteSection
{
	public int Id { get; set; }
	public string SectionId { get; set; }
}

public class RequestUpdateExcel
{
	public string Json { get; set; }
}

public class QueryUser
{
	public string Query { get; set; }
	public List<VariableQuery> Variables { get; set; }
}

public class VariableQuery
{
	public int Variable { get; set; }
	public string Value { get; set; }
	public string DataType { get; set; }
	public string Context { get; set; }
	public string TableName { get; set; }
	public string TableComment { get; set; }
	public string ColumnName { get; set; }
	public string ColumnComment { get; set; }
	public bool IsForeignKey { get; set; }
	public string ReferencedTable { get; set; }
	public string ReferencedColumn { get; set; }
}

public class ProcedureVersion
{
	public int Id { get; set; }
	public string ProcedureId { get; set; }
	public string ProcedureIdOrigin { get; set; }
	public int IdOrigin { get; set; }
	public string Code { get; set; }
	public string Name { get; set; }

	public int Status { get; set; }
	public string StatusDescription { get; set; }

	public int Version { get; set; }

	public string Description { get; set; }
	public DateTime Date { get; set; }
	public bool IsActivityUsed { get; set; }
}

public class ProcedureMasterSectionAttachment
{
	public string Id { get; set; }
	public string TypeCode { get; set; }
	public string Name { get; set; }
	public string URL { get; set; }
	public string Extension { get; set; }
	public string Size { get; set; }
	public string SectionId { get; set; }
	public int Status { get; set; }
	public bool IsFileOffice { get; set; }
	public FileAttachment File { get; set; }
}

public class FileAttachment
{
	public string name { get; set; }
	public string FileBase64 { get; set; }
	public string lastModified { get; set; }
	public string lastModifiedDate { get; set; }
	public string size { get; set; }
	public string type { get; set; }
}

public class ComponentInstruction
{
	public string Id { get; set; }
	public string InstructionId { get; set; }
	public string ProcedureId { get; set; }
	public string ActivityId { get; set; }
	public string ComponentId { get; set; }
	public string TypeComponent { get; set; }
	public bool IsSubProduct { get; set; }
	public string Line { get; set; }
	public string Code { get; set; }
	public string Name { get; set; }
	public decimal Quantity { get; set; }
	public decimal QuantityIssue { get; set; }
	public decimal QuantityAvailable { get; set; }
	public decimal Tolerance { get; set; }
	public string AttachmentId { get; set; }
	public string NameAttachment { get; set; }
	public string Method { get; set; }
	public string CodeSignal { get; set; }
	public string UnitId { get; set; }
	public string UnitType { get; set; }
	public string Quantitytext { get; set; }
	public bool Mandatory { get; set; }
	public bool IsRemainingTotal { get; set; }
	public string MaterialImage { get; set; }
}

public class ProcessMasterVersionresult
{
	public string ProcedureId { get; set; }
	public string ProcedureIdOrigin { get; set; }
	public int Status { get; set; }
	public int Version { get; set; }
	public int EarlierVersion { get; set; }
}

public class ProcessMasterAttachmentDetail
{
	public string Id { get; set; }
	public string AttachmentId { get; set; }
	public string AuxId { get; set; }
	public string Entity { get; set; }
	public int Status { get; set; }
}

public class ProcedureExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Procedure Code format.")]
	public string ProcedureCode { get; set; }

	[Key]
	[Required]
	public int Version { get; set; }

	public int EarlierVersion { get; set; }

	[Required]
	[MaxLength(200)]
	public string ProcedureName { get; set; }

	[MaxLength(200)]
	public string ProcedureClass { get; set; }

	public List<SectionExternal> Sections { get; set; }
}

public class SectionExternal
{
	[Key]
	[Required]
	[MaxLength(64)]
	public string ProcedureCode { get; set; }

	[Key]
	public int Version { get; set; }

	[Key]
	public int SectionOrder { get; set; }

	[MaxLength(200)]
	public string SectionType { get; set; }

	[MaxLength(255)]
	public string SectionName { get; set; }

	[MaxLength(500)]
	public string SectionDescription { get; set; }

	public string TypeVisualHelp { get; set; }
	public string VisualHelp { get; set; }
	public List<InstrucctionExternal> Instrucctions { get; set; }
}

public class InstrucctionExternal
{
	[Key]
	[Required]
	[MaxLength(64)]
	public string ProcedureCode { get; set; }

	[Key]
	[Required]
	public int Version { get; set; }

	[Key]
	[Required]
	public int SectionOrder { get; set; }

	[Key]
	[Required]
	public int InstructionOrder { get; set; }

	[MaxLength(200)]
	public string InstructionType { get; set; }

	[MaxLength(200)]
	public string InstructionTypeInput { get; set; }

	public string InstructionDescription { get; set; }

	[MaxLength(200)]
	public string InstructionMandatory { get; set; }

	[MaxLength(200)]
	public string ViewType { get; set; }

	[MaxLength(200)]
	public string MultiSelect { get; set; }

	[MaxLength(200)]
	public string DataReadingSignalCode { get; set; }

	public int InstructionLongText { get; set; }

	[MaxLength(200)]
	public string InstructionTypeDataReading { get; set; }

	[MaxLength(200)]
	public string DataReadingCodeAutomatic { get; set; }

	[MaxLength(200)]
	public string DataReadingDefaultValue { get; set; }

	public string InstructionIsGauge { get; set; }

	[MaxLength(200)]
	public string GaugeTargetValue { get; set; }

	public string GaugeMinValue { get; set; }
	public string GaugeMaxValue { get; set; }

	[MaxLength(200)]
	public string GaugeAction { get; set; }

	[MaxLength(200)]
	public string InstructionRange { get; set; }

	public string InstructionQueryUser { get; set; }

	[MaxLength(200)]
	public string InstructionSacale { get; set; }

	public string InstructionTimeInSec { get; set; }

	[MaxLength(200)]
	public string TypeInstrucction { get; set; }

	[MaxLength(200)]
	public string Type { get; set; }

	[Key]
	public int ChoiceOrder { get; set; }

	public string ChoiceValue { get; set; }
	public string ChoiceAction { get; set; }
	public string TypeVisualHelp { get; set; }
	public string VisualHelp { get; set; }
}

public class ProcedureExternalSync
{
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Procedure Code format.")]
	public string ProcedureCode { get; set; }

	[Key]
	[Required]
	public int Version { get; set; }

	public int EarlierVersion { get; set; }

	[Required]
	[MaxLength(200)]
	public string ProcedureName { get; set; }

	[MaxLength(200)]
	public string ClassCode { get; set; }

	public string ActivityTypeCode { get; set; }
	public string InterventionCode { get; set; }
	public string SourceCode { get; set; }

	[RegularExpression(RegularExpression.ProductStatusIntegration, ErrorMessage = "Invalid Status")]
	[Description("Status")]
	public string Status { get; set; }

	public List<SectionExternalSync> Sections { get; set; }
}

public class SectionExternalSync
{
	[Key]
	public int SectionOrder { get; set; }

	[MaxLength(200)]
	public string SectionType { get; set; }

	[MaxLength(255)]
	public string SectionName { get; set; }

	[MaxLength(500)]
	public string SectionDescription { get; set; }

	public string Observations { get; set; }

	public string TypeVisualHelp { get; set; }
	public string VisualHelp { get; set; }
	public List<InstrucctionExternalSync> Instructions { get; set; }
}

public class InstrucctionExternalSync
{
	[Key]
	[Required]
	[MaxLength(64)]
	public int InstructionOrder { get; set; }

	public string InstructionDescription { get; set; }

	[MaxLength(200)]
	public bool InstructionMandatory { get; set; }

	[MaxLength(200)]
	public string InstructionType { get; set; }

	public string InstructionQueryUser { get; set; }

	[MaxLength(200)]
	public string InstructionDataType { get; set; }

	public int InstructionLongText { get; set; }

	public string TypeDataReading { get; set; }
	public bool IsDecimal { get; set; }
	public bool IsGauge { get; set; }

	//falta mapear
	public string DataReadingSignalCode { get; set; }

	//falta mapear
	public string CodeAutomatic { get; set; }

	public decimal GaugeTargetValue { get; set; }
	public decimal GaugeMinValue { get; set; }
	public decimal GaugeMaxValue { get; set; }
	public decimal MaxValueLinearScale { get; set; }
	public decimal MinValueLinearScale { get; set; }

	public string TagType { get; set; }
	public decimal DefaultValue { get; set; }
	public long TimeInSec { get; set; }

	[MaxLength(200)]
	public string ViewType { get; set; }

	[MaxLength(200)]
	public string MultiSelect { get; set; }

	public List<ChoiceExternalSync> Choices { get; set; }
}

public class ChoiceExternalSync
{
	[Key]
	public int OrderChoice { get; set; }

	[MaxLength(500)]
	public string Value { get; set; }

	[MaxLength(500)]
	public string Action { get; set; }

	public bool IsNotify { get; set; }
	public string OrderSectionGoTo { get; set; }
	public string MessageNotify { get; set; }
	public string MessageWarning { get; set; }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ClassTypeProcedure
{
	Production = 1,
	Maintenance = 2,
	Quality = 3,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum SectionType
{
	Question = 1,
	Transaction = 2,
	Timer = 3,
	Chronometer = 4
}

[JsonConverter(typeof(StringEnumConverter))]
public enum InstructionType
{
	Input = 1,
	MultipleChoice = 2,
	LinearScale = 3,
	IssueMaterials = 4,
	ProductReceipt = 5,
	ReturnMaterial = 6,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ViewTypes
{
	Tiles = 1,
	List = 2,
	RadioButton = 3,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum InputType
{
	Text = 1,
	Integer = 2,
	Decimal = 3,
	Date = 4,
	Time = 5,
	Query = 7,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum DataReadingType
{
	Manual = 1,
	Automatic = 2,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum DataReadingTypeChanges
{
	Manual = 1,
	Automatic = 2,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ActionTypeCode
{
	noaction = 1,
	block = 2,
	finish = 3,
	finisherror = 4,
	Goto = 5,
	FinisWithWarning = 6
}
