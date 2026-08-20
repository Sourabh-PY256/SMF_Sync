namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class MachineConfigurationComplex
{
	/// <summary>
	///
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public int UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ConfigObject { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ConfigWidgetObject { get; set; }
}

/// <summary>
///
/// </summary>
public class SensorValueHistoric
{
	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime Date { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ValueSensor { get; set; }
}

/// <summary>
///
/// </summary>
public class ModelViewSensorValue
{
	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SensorCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Average { get; set; }

	/// <summary>
	///
	/// </summary>
	public string date { get; set; }
}
