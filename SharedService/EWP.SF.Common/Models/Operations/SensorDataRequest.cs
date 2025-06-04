using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Operations;


/// <summary>
///
/// </summary>
public class SensorDataRequest
{
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
	public int? Limit { get; set; } = 0;

	/// <summary>
	///
	/// </summary>
	public DateTime? StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Parameter { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ParameterValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonProperty(nameof(DateField))]
	public int DateFilter
	{
		set => DateField = (DateField)value;
	}

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public DateField DateField { get; set; } = DateField.LogDate;

	/// <summary>
	///
	/// </summary>
	public int? AvoidPeaks { get; set; }
}

/// <summary>
///
/// </summary>
public class SummarizedSensorDataRequest
{
	/// <summary>
	///
	/// </summary>
	public string SensorId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? EndDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public DateField DateField { get; set; } = DateField.LogDate;
}

/// <summary>
///
/// </summary>
public class CustomDataRequest
{
	/// <summary>
	///
	/// </summary>
	public List<KeyValuePair<string, object>> Parameters { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Program { get; set; }
}
