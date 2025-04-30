using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace EWP.SF.Item.BusinessEntities;

public enum ResourceType
{
	Machine = 1,
	Position = 2,
	Tooling = 3,
}

public enum ResourceTransactionType
{
	Manual = 1,
	Automatic = 2,
}
public class ResourceJournal
{
	public ResourceType ResourceType { get; set; }
	public string Id { get; set; }
	public virtual string Code { get; set; }
	public string OrderCode { get; set; }
	public double OperationNo { get; set; }
	public string OperationSubtypeCode { get; set; }

	public DateTime StartDate { get; set; }

	public string StartEvent { get; set; }

	public int StartUser { get; set; }

	public string StartEmployee { get; set; }
	public DateTime StopDate { get; set; }

	public string StopEvent { get; set; }

	public int StopUser { get; set; }

	public string StopEmployee { get; set; }
	public int Time { get; set; }
	public string TransactionId { get; set; }
}

public class ResourceJournalSummary
{
	public string Type { get; set; }
	public string Code { get; set; }
	public string Name { get; set; }
	public string OrderCode { get; set; }
	public string OperationNo { get; set; }
	public double Ellapsed { get; set; }

	public double Remaining { get; set; }

	public bool Live { get; set; }
}
public class ResourceTransaction
{
	public string TransactionId { get; set; }
	public string OrderCode { get; set; }
	public double OperationNo { get; set; }
	public int TimeInSeconds { get; set; }

	public DateTime CreateDate { get; set; }

	public int CreateUser { get; set; }
	public string CreateEmployee { get; set; }
	public string Trigger { get; set; }
	public string ErpStatus { get; set; }
}

public class ResourceTransactionResult
{
	public string Entity { get; set; }
	public bool IsSuccess { get; set; } = true;

	public string ErrorCode { get; set; }
	public string TransactionId { get; set; }
	public string Message { get; set; }

	public ResourceTransactionResult(string entity)
	{
		Entity = entity;
	}
	public ResourceTransactionResult()
	{
	}
}

public class ResourceIssueExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Doc Code")]
	[JsonProperty(PropertyName = "DocCode")]
	public string DocCode { get; set; }

	[MaxLength(500)]
	[Description("Comments")]
	[JsonProperty(PropertyName = "Comments")]
	public string Comments { get; set; }

	[Required]
	[MaxLength(100)]
	[Description("OrderCode")]
	[JsonProperty(PropertyName = "OrderCode")]
	public string OrderCode { get; set; }

	[Required]
	[Description("OperationNo")]
	[JsonProperty(PropertyName = "OperationNo")]
	public double OperationNo { get; set; }

	[Description("Doc Date")]
	[JsonProperty(PropertyName = "DocDate")]
	public DateTime DocDate { get; set; }
}
