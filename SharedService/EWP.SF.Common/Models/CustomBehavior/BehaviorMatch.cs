using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.Models.Sensors;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public abstract class IBehaviorMatch
{
	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridDisabledHiding]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Name")]
	[GridDisabledHiding]
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public object Value { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public SensorLiveViewer AssignLiveViewer { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool WaitSendLiveViewer { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public SensorWhen AssignSensorWhen { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool WaitSendNotif { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Signature { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public bool IsCurrent { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Aux1 { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Aux2 { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Aux3 { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string FallbackValue
	{
		get; set;
	}

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Formula
	{
		get; set;
	}
}

/// <summary>
///
/// </summary>
public class BehaviorMatch
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public int SourceType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SourceValue { get; set; }
}
