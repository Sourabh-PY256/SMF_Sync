namespace EWP.SF.Common.Models;

public class MachineConfigurationComplex
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public string MachineId { get; set; }
	public string Name { get; set; }
	public string ConfigObject { get; set; }
	public string ConfigWidgetObject { get; set; }
}

public class SensorValueHistoric
{
	public string MachineId { get; set; }
	public DateTime Date { get; set; }
	public string ValueSensor { get; set; }
}

public class ModelViewSensorValue
{
	public string MachineId { get; set; }
	public string SensorCode { get; set; }
	public bool Average { get; set; }
	public string date { get; set; }
}
