using EWP.SF.Common.Models;

namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncExecuteRequest
{
	public List<string> Services { get; set; }

	public string EntityCode { get; set; }

	public string BodyData { get; set; }

	public User SystemOperator { get; set; } 

	private string? _methodType;
	public string MethodType
	{
		get => _methodType ?? "GET";
		set => _methodType = value;
	}
}
