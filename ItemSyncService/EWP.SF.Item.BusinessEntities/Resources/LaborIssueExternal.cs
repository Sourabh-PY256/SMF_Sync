using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace EWP.SF.Item.BusinessEntities;

public class LaborIssueExternal : ResourceIssueExternal
{
	[Required]
	[Description("Details")]
	[JsonProperty(PropertyName = "Details")]
	public List<LaborJournalDetail> Details { get; set; }
}
public class LaborIssue : ResourceIssueExternal
{
	[Required]
	[Description("Details")]
	[JsonProperty(PropertyName = "Details")]
	public List<LaborJournalDetail> Details { get; set; }
	public string TransactionId { get; set; }
}

public class LaborJournalDetail
{
	public string PositionCode { get; set; }
	public string EmployeeCode { get; set; }
	public double Time { get; set; }
}
