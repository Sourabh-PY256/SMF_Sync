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
public class MachineIcon
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
	public string Tags { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachinePath { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Group { get; set; }
}

/// <summary>
///
/// </summary>
public class Machine : ICloneable, ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_machine_log");

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("MachineCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "ProductionLine", "Id", "Description")]
	[GridCustomPropertyName("ProductionLineName")]
	[GridDrillDown("ProductionLine")]
	[GridRequireDecode]
	public string ProductionLineId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineState { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("OperationType")]
	[GridDrillDown("OperationType", "Code")]
	[GridRequireDecode]
	public string TypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MaximumCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MinimumCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double LotCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string BinLocations { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LotCalculation { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductionType { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Online { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DownSince { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsAuxiliar { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Facility", "Id", "Name")]
	[GridCustomPropertyName("FacilityName")]
	[GridDrillDown("Facility", "Id")]
	[GridRequireDecode]
	public string FacilityId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("FacilityCode")]
	[GridDrillDown("Facility", "Code")]
	[GridRequireDecode]
	public string FacilityCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Floor", "Id", "Name")]
	[GridCustomPropertyName("FloorName")]
	[GridDrillDown("Floor", "Id")]
	[GridRequireDecode]
	public string FloorId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ProductionLineCode")]
	[GridDrillDown("ProductionLine", "Code")]
	[GridRequireDecode]
	public string ParentCode { get; set; }

	// Mismo dato que en TypeId

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public ConcurrentDictionary<DateTime, OEEModel> OEEHistory { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ConfigError { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProcessType ProcessType { get; set; }

	/// <summary>
	///
	/// </summary>
	public MachineOEEConfiguration OEEConfiguration { get; set; }

	/// <summary>
	///
	/// </summary>
	public MachineProgramming Programming { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public MachineEnvironment Environment { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> Skills { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "MachineIcon", "Id", "Path")]
	[GridCustomType(GridColumnType.ICON_ROUTE, "Icon")]
	[GridCustomPropertyName("Icon")]
	public int? LiveIconId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public object Tag { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Sensor> Sensors { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Sensor> SensorDetails { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<MachineParam> Parameters { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBusy { get; set; }

	/// <summary>
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
	public string ManufactureDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string RobotArmModel { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PwrSourceModel { get; set; }

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
	public bool HasTool { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Activities { get; set; }

	//public int Planning { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts Shift { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts ShiftDelete { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeDetail> ProcessTypeDetails { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public Machine()
	{
		OEEHistory = [];
	}

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("WarehouseName")]
	[GridDrillDown("Warehouse", "Name")]
	[GridRequireDecode]
	public string Warehouse { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("WarehouseCode")]
	[GridDrillDown("Warehouse", "Code")]
	[GridRequireDecode]
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public Machine(string id)
	{
		Id = id;
		OEEHistory = [];
	}

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	public object Clone() => MemberwiseClone();
}

//public class Sensor : IBehaviorMatch, ICloneable
//{
//    public string TypeId { get; set; }
//    public string TagTypeName { get; set; }
//    public string Location { get; set; }
//    public double? MaximumValue { get; set; }
//    public double? MinimumValue { get; set; }
//    public bool OutOfRangeAlert { get; set; }
//    public bool ApplicationAlert { get; set; }
//    public bool EmailAlert { get; set; }

//    public bool LiveScreen { get; set; }
//    public bool IgnoreForHistory { get; set; }
//    public bool AttendingNotify { get; set; }
//    public string Icon { get; set; }
//    public string? UnitId { get; set; }
//    public string CodeUnit { get; set; }
//    public string? TagClass { get; set; }
//    public string? UrlStreaming { get; set; }

//    //[JsonIgnore]
//    //public string ValueName { get; set; }

//    [JsonIgnore]
//    public List<SensorData> Data { get; set; }

//    public List<AlertLevel> AlertLevels { get; set; }

//    [JsonIgnore]
//    public double AvgValue { get; set; }

//    [JsonProperty("MachineId")]
//    private string MachineSetter
//    {
//        set { MachineId = value; }
//    }

//    public object Clone()
//    {
//        return base.MemberwiseClone();
//    }
//}

/// <summary>
///
/// </summary>
public class OrderMachine
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DateStart { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DateEnd { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<OrderMachine> OrderMachineId { get; set; }
}

/// <summary>
///
/// </summary>
public class BinLocationWarehouse
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
	public string Aisle { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Rack { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Level { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Col { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<BinLocationWarehouse> BinLocationWarehouseId { get; set; }
}

/// <summary>
///
/// </summary>
public class CapacityProduct
{
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
	public double ExecTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double WaitTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double LotCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<CapacityProduct> CapacityProductId { get; set; }
}

/// <summary>
///
/// </summary>
public class EmployeesMachine360
{
	/// <summary>
	///
	/// </summary>
	public string EmployeeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<EmployeesMachine360> EmployeeMachineId { get; set; }
}

/// <summary>
///
/// </summary>
public class DowntimeMachine360
{
	/// <summary>
	///
	/// </summary>
	public string DowntimeTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Count { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Total { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<DowntimeMachine360> DowntimeMachineId { get; set; }
}

/// <summary>
///
/// </summary>
public class MachineParam : IBehaviorMatch, ICloneable
{
	/// <summary>
	///
	/// </summary>
	public MachineParam(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	public MachineParam()
	{
	}

	/// <summary>
	///
	/// </summary>
	public new string Formula { get; set; }

	/// <summary>
	///
	/// </summary>
	public new string FallbackValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool OutOfRangeAlert { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool ApplicationAlert { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool EmailAlert { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool StoreValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IgnoreForHistory { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool LiveScreen { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool EditableScalar { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DependsSensorId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Icon { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public Dictionary<string, BehaviorMatch> CustomBehaviorMatch { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<AlertLevel> AlertLevels { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CustomBehaviorId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public CustomBehavior CustomBehavior { get; set; }

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	public object Clone() => MemberwiseClone();

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Value is null ? Code : Code + " = " + Value;
	}
}

/// <summary>
///
/// </summary>
public class SensorDataList : List<DeviceParameter>
{
	/// <summary>
	///
	/// </summary>
	public SensorDataList()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="List{T}" /> class that is empty and has the specified initial capacity.</summary>
	/// <param name="capacity">The number of elements that the new list can initially store.</param>
	/// <exception cref="ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than 0.</exception>
	public SensorDataList(int capacity) : base(capacity)
	{
	}
}

/// <summary>
///
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class DeviceParameter
{
	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public DateTime ValueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public DateTime ReadDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[XmlElement("ReadDate")]
	public string ReadDateS
	{
		get => ReadDate.ToString("yyyy-MM-dd HH:mm:ss");
		set => ReadDate = DateTime.Parse(value);
	}

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public DateTime ProcessDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[XmlElement("ProcessDate")]
	public string ProcessDateS
	{
		get => ProcessDate.ToString("yyyy-MM-dd HH:mm:ss");
		set => ProcessDate = DateTime.Parse(value);
	}

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public DateTime SendDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[XmlElement("SendDate")]
	public string SendDateS
	{
		get => SendDate.ToString("yyyy-MM-dd HH:mm:ss");
		set => SendDate = DateTime.Parse(value);
	}

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[XmlElement("ValueDate")]
	public string xmlDate
	{
		get => ValueDate.ToString("yyyy-MM-dd HH:mm:ss");
		set => ValueDate = DateTime.Parse(value);
	}

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public string DateReadApi { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public string DateInsert { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public string DateSend { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty(Required = Required.Always)]
	public bool UTC { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	public string SensorId { get; set; }
}

/// <summary>
///
/// </summary>
public class SensorData
{
	/// <summary>
	///
	/// </summary>
	public DateTime LogDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SensorId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ValueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ReadDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ProcessDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime SendDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ValueDateS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ReadDateS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessDateS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SendDateS { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DateCollector { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DateReadApi { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DateInsert { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DateSendApi { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool UTC { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public object Tag { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ProcessedValue { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Signature { get; set; }
}

/// <summary>
///
/// </summary>
public class SummarizedSensorData
{
	/// <summary>
	///
	/// </summary>
	public string SensorId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Sum { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Average { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Count { get; set; }
}

/// <summary>
///
/// </summary>
public class AlertLevel
{
	/// <summary>
	///
	/// </summary>
	public string ParameterId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Level { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Iterations { get; set; }
}

/// <summary>
///
/// </summary>
public class DeviceProgrammingCatalog

{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Auxiliar { get; set; }
}

/// <summary>
///
/// </summary>
public class MachineExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	public string MachineName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[DefaultMappingEntity("ProductionLine")]
	public string ParentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[MaxLength(50)]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Process|Auxiliar", ErrorMessage = "Invalid Type")]
	[MaxLength(50)]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OperationType { get; set; }

	/// <summary>
	///
	/// </summary>
	public int MinimumCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? MaximumCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Finite|Infinite|InfiniteWithShiftPattern", ErrorMessage = "Invalid Capacity Mode")]
	[MaxLength(100)]
	public string CapacityMode { get; set; }

	/// <summary>
	///
	/// </summary>
	[DefaultMappingEntity("ChangeoverGroup")]
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
	public int? Attribute2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? Attribute3 { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Finite|Infinite|InfiniteWithShiftPattern", ErrorMessage = "Invalid Behavior")]
	[MaxLength(100)]
	public string InfiniteModeBehavior { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Primary|Secondary", ErrorMessage = "Invalid scheduling level")]
	[MaxLength(100)]
	public string ScheduleLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Concurrent Setup Time")]
	[MaxLength(100)]
	public string ConcurrentSetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid value")]
	[MaxLength(100)]
	public string Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid value")]
	[MaxLength(100)]
	public string Planning { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Batch|Pieces", ErrorMessage = "Invalid Production Type")]
	[MaxLength(100)]
	public string ProductionType { get; set; }

	/// <summary>
	///
	/// </summary>
	[DefaultMappingEntity("MeasureUnit")]
	public string EfficiencyUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? RunTime { get; set; }
}

/// <summary>
///
/// </summary>
public class DataCollector
{
	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	public string Machine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SensorId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	public string Sensor { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public DateTime ValueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public DateTime ReadDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[XmlElement("ReadDate")]
	public string ReadDateS
	{
		get => ReadDate.ToString("yyyy-MM-dd HH:mm:ss");
		set => ReadDate = DateTime.Parse(value);
	}

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public DateTime ProcessDate { get; set; } = DateTime.UtcNow;

	/// <summary>
	///
	/// </summary>
	[XmlElement("ProcessDate")]
	public string ProcessDateS
	{
		get => ProcessDate.ToString("yyyy-MM-dd HH:mm:ss");
		set => ProcessDate = DateTime.Parse(value);
	}

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public DateTime SendDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[XmlElement("SendDate")]
	public string SendDateS
	{
		get => SendDate.ToString("yyyy-MM-dd HH:mm:ss");
		set => SendDate = DateTime.Parse(value);
	}

	/// <summary>
	///
	/// </summary>
	[XmlElement("ValueDate")]
	public string xmlDate
	{
		get => ValueDate.ToString("yyyy-MM-dd HH:mm:ss");
		set => ValueDate = DateTime.Parse(value);
	}

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public string DateReadApi { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public string DateInsert { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty]
	[XmlIgnore]
	public string DateSend { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty(Required = Required.Always)]
	public bool UTC { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool error { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Tag { get; set; }
}
