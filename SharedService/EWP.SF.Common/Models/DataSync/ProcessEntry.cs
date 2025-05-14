using System.ComponentModel;
using System.Xml.Serialization;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.MesModels;

public enum ProductType
{
	SemiFinishedProduct = 1,
	FinishedProduct = 2
}

public class ProcessEntry : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_product_log");

	public ProcessEntry()
	{
	}

	public ProcessEntry(string id)
	{
		Id = id;
	}

	[EntityColumn("ProductId")]
	public string Id { get; set; }

	public string ComponentId { get; set; }

	public string Code { get; set; }
	public string Name { get; set; }
	public double Quantity { get; set; }
	public double MaxQuantity { get; set; }
	public double MinQuantity { get; set; }
	public UnitType UnitType { get; set; }

	public string Time { get; set; }

	public string Formula { get; set; }
	public string UnitId { get; set; }
	public double Factor { get; set; }
	public DateTime CreationDate { get; set; }
	public User CreatedBy { get; set; }
	public DateTime ModifyDate { get; set; }
	public User ModifiedBy { get; set; }

	public ProductType ProductType { get; set; }
	public string DistUnitId { get; set; }
	public double DistUnitFactor { get; set; }
	public bool IsEditable { get; set; }
	public Status Status { get; set; }
	public double Scrap { get; set; }
	public double SupplyLeadTime { get; set; }
	public DateTime ValidTo { get; set; }
	public DateTime ValidFrom { get; set; }
	public List<ProcessEntryProcess> Processes { get; set; }
	public List<ProcessEntryComponent> Components { get; set; }

	public List<ProcessEntryLabor> Labor { get; set; }

	public List<ProcessEntryTool> Tools { get; set; }

	public List<Activity> Tasks { get; set; }
	public string InstrucctionByPh { get; set; }
	public string InstrucctionbyDensity { get; set; }
	public List<AlternativeComponent> GlobalAlternatives { get; set; }

	public string Warehouse { get; set; }

	public int Version { get; set; }

	public int Sequence { get; set; }

	public bool MakeObsolete { get; set; }
	public bool isNewVersion { get; set; }

	[Description("EarlierVersion")]
	public int EarlierVersion { get; set; }

	public int Schedule { get; set; }

	public string Comments { get; set; }

	public string LogDetailId { get; set; }
	public long BomVersion { get; set; }

	public int BomSequence { get; set; }

	public long RouteVersion { get; set; }

	public int RouteSequence { get; set; }
}

public class ProcessEntryProcess
{
	private static Logger logger;

	public ProcessEntryProcess()
	{
		logger = LogManager.GetCurrentClassLogger();
	}

	public string ProcessId { get; set; }
	public string Name { get; set; }
	public string ProcessTypeId { get; set; }
	public string ProcessSubTypeId { get; set; }
	public int OperationClassId { get; set; }
	public int Step { get; set; }
	public int Sort { get; set; }
	public bool IsOutput { get; set; }
	public string Output { get; set; }
	public double Time { get; set; }
	public string TimeUnit { get; set; }
	public string OutputName { get; set; }
	public string OutputCode { get; set; }
	public string OutputUnitId { get; set; }
	public int OutputUnitType { get; set; }

	[XmlIgnore]
	public List<SubProduct> Subproducts { get; set; }

	[XmlIgnore]
	public List<ProcessEntryAttribute> Attributes { get; set; }

	public double SetupTime { get; set; }
	public string Unit { get; set; }

	public double Quantity { get; set; }
	public string MaxTimeBeforeNextOp { get; set; }
	public string SlackTimeBeforeNextOp { get; set; }
	public string SlackTimeAfterPrevOp { get; set; }
	public string MaxOpSpanIncrease { get; set; }
	public string TransferType { get; set; }
	public double? TransferQty { get; set; }
	public string SpareStringField1 { get; set; }
	public string SpareStringField2 { get; set; }
	public decimal SpareNumberField { get; set; }
	public int? ProcessTimeType { get; set; }

	public string Comments { get; set; }

	[XmlIgnore]
	public List<DeviceSpeed> AvailableDevices { get; set; }

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

	public double TransformFactor { get; set; }
	public double DestFactor { get; set; }

	[JsonIgnore]
	public int TimeInSeconds { get; set; }
}

public class ProcessEntryLabor
{
	public string Id { get; set; }
	public string ProcessId { get; set; }
	public string LaborId { get; set; }
	public string LineId { get; set; }
	public string LineUID { get; set; }
	public string MachineId { get; set; }
	public double Quantity { get; set; }
	public string Usage { get; set; }
	public bool Schedule { get; set; }
	public string Name { get; set; }
	public string LaborTime { get; set; }
	public double Cost { get; set; }
	public string Source { get; set; }

	public string Comments { get; set; }
	public bool IsBackflush { get; set; }
}

public class ProcessEntryTool
{
	public string Id { get; set; }
	public string ProcessId { get; set; }

	public string LineId { get; set; }
	public string LineUID { get; set; }
	public string ToolId { get; set; }
	public double Quantity { get; set; }

	public string MachineId { get; set; }
	public string Usage { get; set; }
	public bool Schedule { get; set; }
	public double Cost { get; set; }

	public string Source { get; set; }

	public string Comments { get; set; }
	public bool IsBackflush { get; set; }
}

public class ProcessEntryAttribute
{
	public string Id { get; set; }
	public string ProcessId { get; set; }
	public string AttributeId { get; set; }
	public bool Selected { get; set; }
	public string Value { get; set; }
}

public class SubProduct
{
	public string ProcessId { get; set; }
	public string ComponentId { get; set; }
	public double Factor { get; set; }

	public double Quantity { get; set; }
	public double Rejected { get; set; }
	public string Pallet { get; set; }
	public string Batch { get; set; }
	public string Name { get; set; }
	public string Code { get; set; }
	public string UnitId { get; set; }
	public int UnitType { get; set; }
	public string LineId { get; set; }
	public string LineUID { get; set; }
	public string Type { get; set; }
	public string WarehouseCode { get; set; }

	[JsonIgnore]
	public bool IsUpdated { get; set; }

	public string ItemImage { get; set; }

	public string ItemUnit { get; set; }

	public string Location { get; set; }
	public string InventoryStatus { get; set; }

	public string Comments { get; set; }
}

public class MaterialIssueProductModel
{
	public string ItemCode { get; set; }
	public string LineId { get; set; }
	public string LineType { get; set; }
	public string Warehouse { get; set; }
	public double Quantity { get; set; }

	public List<ComponentBatch> Lots { get; set; }
}

public class DeviceSpeed
{
	public string Id { get; set; }
	public double Quantity { get; set; }
	public string Unit { get; set; }

	public string LineId { get; set; }
	public string LineUID { get; set; }
	public double SetupTime { get; set; }
	public double ExecTime { get; set; }
	public double WaitTime { get; set; }
	public int TimeUnit { get; set; }
	public bool Schedule { get; set; }
	public bool AutomaticSequencing { get; set; }
	public bool Selected { get; set; }
	public double? LotCapacity { get; set; }

	public string Comments { get; set; }

	public bool IsBackflush { get; set; }
}

public class DeviceSpeedDetail
{
	public string Id { get; set; }
	public string ProductId { get; set; }
	public string ProductCode { get; set; }
	public string ProductName { get; set; }
	public double Quantity { get; set; }
	public string Unit { get; set; }

	public double Time { get; set; }
	public string TimeUnit { get; set; }

	public double TimeFactor { get; set; }

	public double UnitFactor { get; set; }
	public string Image { get; set; }
}

public class ProcessEntryComponent
{
	public string ProcessId { get; set; }
	public ComponentType ComponentType { get; set; }
	public string ProcessTypeId { get; set; }
	public int Step { get; set; }
	public string ComponentId { get; set; }
	public double Quantity { get; set; }
	public string UnitId { get; set; }
	public string ItemUnit { get; set; }
	public string Name { get; set; }
	public string Code { get; set; }

	public int Class { get; set; }
	public string LineId { get; set; }
	public string LineUID { get; set; }
	public string WarehouseCode { get; set; }
	public bool IsBackflush { get; set; }
	public bool IsSchedule { get; set; }

	public string Source { get; set; }
	public int EnableSchedule { get; set; }
	public List<AlternativeComponent> Alternatives { get; set; }
	public string Comments { get; set; }
}

public class ProcessEntryTask
{
	public string ProcessId { get; set; }
	public TaskType TaskType { get; set; }
	public string TaskId { get; set; }
	public string Description { get; set; }
}
