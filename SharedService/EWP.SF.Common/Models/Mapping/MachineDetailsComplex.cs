namespace EWP.SF.Common.Models;

public class MachineDetailsComplex
{
	public string Id { get; set; }
	public bool ApplyAllMachines { get; set; }
	public bool ApplyAllUsers { get; set; }
	public string MachineId { get; set; }
	public string Description { get; set; }
	public int UserId { get; set; }
	public string ConfigGridObject { get; set; }
	public string ConfigWidgetObject { get; set; }
}
