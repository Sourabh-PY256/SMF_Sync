namespace EWP.SF.Common.Models;

public class LiveGraphOEEComplex
{
	public int MachineId { get; set; }
	public string MachineName { get; set; }
	public decimal OEE { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
}
