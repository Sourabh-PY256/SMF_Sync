namespace EWP.SF.Common.Models;

public class PrintLabelInfo
{
	public string QueueId { get; set; }
	public int NoCopys { get; set; }
	public string MimeType { get; set; }
	public string PrintingMachine { get; set; }
	public string Buffer { get; set; }
}
