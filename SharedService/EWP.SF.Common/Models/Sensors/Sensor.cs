using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Sensors;

/// <summary>
///
/// </summary>
public class Sensor : IBehaviorMatch, ILoggableEntity, ICloneable
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_sensor_log");

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridRequireDecode]
	public string SensorId { get; set; }

	// NOTA: hace override a la herencia de "IBehaviorMatch"
	// ya que no encontrabaja el atributo "EntityColumn"

	/// <summary>
	///
	/// </summary>
	[EntityColumn("SensorCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public new string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("MachineCode")]
	[GridDrillDown("Machine", "Id")]
	public string MachineValueId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("MachineName")]
	[GridDrillDown("Machine", "Id", "MachineValueId")]
	public string MachineDescription { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Location { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("TagTypeCode")]
	[GridDrillDown("TagType")]
	[GridRequireDecode]
	public string TypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("TagType", "Id", "TypeId")]
	[GridRequireDecode]
	public string TagTypeName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(Enumerators.GridColumnType.IMAGE_ROUTE, "Image")]
	public string Picture { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(Enumerators.GridColumnType.ICON_CLASS, "Icon")]
	public string IconId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IgnoreForHistory { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UnitValueId { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? MaximumValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? MinimumValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TagClass { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UrlStreaming { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? LastRead { get; set; }

	/// <summary>
	///
	/// </summary>
	public double? LastValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SensorWhen> SensorsWhen { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SensorLiveViewer> SensorLiveViewer { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SensorThen> SensorThen { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SensorRecipient> SensorRecipient { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

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
	public bool LiveScreen { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool EmailAlert { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Icon { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CodeUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public List<SensorData> Data { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public double AvgValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AttendingNotify { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<AlertLevel> AlertLevels { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Flicker { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(Enumerators.GridColumnType.COLOR_HEX, "Color")]
	public string Color { get; set; }

	/// <summary>
	///
	/// </summary>
	public Sensor()
	{
		SensorsWhen = [];
		SensorLiveViewer = [];
		SensorThen = [];
		SensorRecipient = [];
	}

	/// <summary>
	///
	/// </summary>
	public string SensorsWhenToJSON()
	{
		string returnValue = string.Empty;
		if (SensorsWhen is not null)
		{
			returnValue = JsonConvert.SerializeObject(SensorsWhen);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public string SensorsLiveViewerToJSON()
	{
		string returnValue = string.Empty;
		if (SensorLiveViewer is not null)
		{
			returnValue = JsonConvert.SerializeObject(SensorLiveViewer);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public string SensorThensToJSON()
	{
		string returnValue = string.Empty;
		if (SensorThen is not null)
		{
			returnValue = JsonConvert.SerializeObject(SensorThen);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public string SensorRecipientsToJSON()
	{
		string returnValue = string.Empty;
		if (SensorRecipient is not null)
		{
			returnValue = JsonConvert.SerializeObject(SensorRecipient);
		}
		return returnValue;
	}

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	/// <exception cref="NotImplementedException"></exception>
	public object Clone()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UoM { get; set; }
}

/// <summary>
///
/// </summary>
public class SensorsExternal
{
	/// <summary>
	///
	/// </summary>
	public string SensorCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SensorName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UoM { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TagTypeCode { get; set; }
}
