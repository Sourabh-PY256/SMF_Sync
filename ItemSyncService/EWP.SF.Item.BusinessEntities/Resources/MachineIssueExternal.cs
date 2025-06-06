using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace EWP.SF.Item.BusinessEntities;
/// <summary>
///
/// </summary>
public class MachineIssueExternal : ResourceIssueExternal
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Details")]
	[JsonProperty(PropertyName = "Details")]
	public List<MachineJournalDetail> Details { get; set; }
}
/// <summary>
///
/// </summary>
public class MachineIssue : ResourceIssueExternal
{
	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Details")]
	[JsonProperty(PropertyName = "Details")]
	public List<MachineJournalDetail> Details { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TransactionId { get; set; }
}

/// <summary>
///
/// </summary>
public class MachineJournalDetail
{
	/// <summary>
	///
	/// </summary>
	public string MachineCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Time { get; set; }
}
