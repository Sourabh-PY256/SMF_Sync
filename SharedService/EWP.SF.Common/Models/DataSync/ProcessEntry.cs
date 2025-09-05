using System.ComponentModel;
using System.Xml.Serialization;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public enum ProductType
{
	/// <summary>
	///
	/// </summary>
	SemiFinishedProduct = 1,
	/// <summary>
	///
	/// </summary>
	FinishedProduct = 2
}

/// <summary>
///
/// </summary>
public class ProcessEntry : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_product_log");

	/// <summary>
	///
	/// </summary>
	public ProcessEntry()
	{
	}

	/// <summary>
	///
	/// </summary>
	public ProcessEntry(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ProductId")]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

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
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double MaxQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double MinQuantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public UnitType UnitType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Formula { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Factor { get; set; }

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
	public DateTime ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProductType ProductType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string DistUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double DistUnitFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsEditable { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Scrap { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SupplyLeadTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ValidTo { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ValidFrom { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessEntryProcess> Processes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessEntryComponent> Components { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessEntryLabor> Labor { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessEntryTool> Tools { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Tasks { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstrucctionByPh { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InstrucctionbyDensity { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<AlternativeComponent> GlobalAlternatives { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Warehouse { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Sequence { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool MakeObsolete { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool isNewVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("EarlierVersion")]
	public int EarlierVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public long BomVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	public int BomSequence { get; set; }

	/// <summary>
	///
	/// </summary>
	public long RouteVersion { get; set; }

	/// <summary>
	///
	/// </summary>
	public int RouteSequence { get; set; }
	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessEntryProcess
{
	private static Logger logger;

	/// <summary>
	///
	/// </summary>
	public ProcessEntryProcess()
	{
		logger = LogManager.GetCurrentClassLogger();
	}

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessSubTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OperationClassId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Sort { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsOutput { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Output { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TimeUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OutputName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OutputCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string OutputUnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int OutputUnitType { get; set; }

	/// <summary>
	///
	/// </summary>
	[XmlIgnore]
	public List<SubProduct> Subproducts { get; set; }

	/// <summary>
	///
	/// </summary>
	[XmlIgnore]
	public List<ProcessEntryAttribute> Attributes { get; set; }

	/// <summary>
	///
	/// </summary>
	public double SetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MaxTimeBeforeNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SlackTimeBeforeNextOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SlackTimeAfterPrevOp { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MaxOpSpanIncrease { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TransferType { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? TransferQty { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SpareStringField1 { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SpareStringField2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal SpareNumberField { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? ProcessTimeType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	[XmlIgnore]
	public List<DeviceSpeed> AvailableDevices { get; set; }

	/// <summary>
	///
	/// </summary>
	[XmlElement("AvailableDevices")]
	[JsonIgnore]
	public string AvailableMachines
	{
		get => AvailableDevices?.Count > 0 ? JsonConvert.SerializeObject(AvailableDevices) : "[]";
		set
		{
			try
			{
				AvailableDevices = JsonConvert.DeserializeObject<List<DeviceSpeed>>(value, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				AvailableDevices = [];
			}
		}
	}

	/// <summary>
	///
	/// </summary>
	public double TransformFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public double DestFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public int TimeInSeconds { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessEntryLabor
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LaborId { get; set; }

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
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LaborTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Cost { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessEntryTool
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

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
	public string ToolId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Usage { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Cost { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessEntryAttribute
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AttributeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Selected { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }
}

/// <summary>
///
/// </summary>
public class SubProduct
{
	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Factor { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Rejected { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Pallet { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Batch { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UnitType { get; set; }

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
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool IsUpdated { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }
}

/// <summary>
///
/// </summary>
public class MaterialIssueProductModel
{
	/// <summary>
	///
	/// </summary>
	public string ItemCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LineType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Warehouse { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ComponentBatch> Lots { get; set; }
}

/// <summary>
///
/// </summary>
public class DeviceSpeed
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

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
	public double SetupTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double ExecTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double WaitTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TimeUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AutomaticSequencing { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Selected { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? LotCapacity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }
}

/// <summary>
///
/// </summary>
public class DeviceSpeedDetail
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProductName { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TimeUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public double TimeFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public double UnitFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Image { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessEntryComponent
{
	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public ComponentType ComponentType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Step { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Quantity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ItemUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Class { get; set; }

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
	public string WarehouseCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsBackflush { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsSchedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public int EnableSchedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<AlternativeComponent> Alternatives { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Comments { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessEntryTask
{
	/// <summary>
	///
	/// </summary>
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	public TaskType TaskType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TaskId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Description { get; set; }
}
