namespace EWP.SF.Common.Models;

public class LabelPrint
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Buffer { get; set; }
	public string Printer { get; set; }
	public int NoCopys { get; set; }
	public string MimeType { get; set; }
	public string Type { get; set; }
	public bool IsIndividual { get; set; }
}
