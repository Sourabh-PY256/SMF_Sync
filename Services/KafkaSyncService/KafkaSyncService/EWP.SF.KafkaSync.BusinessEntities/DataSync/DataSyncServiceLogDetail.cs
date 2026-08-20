namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncServiceLogDetail
{
	public string Id { get; set; }

	public string LogId { get; set; }

	public string ServiceInstanceId { get; set; }

	public string RowKey { get; set; }

	public DateTime? ProcessDate { get; set; }

	public string ProcessDateTime { get; set; }

	public string ProcessDateShort { get; set; }

	public string ErpReceivedJson { get; set; }

	public string SfMappedJson { get; set; }

	public string ResponseJson { get; set; }

	public string MessageException { get; set; }

	public int AttemptNo { get; set; }

	public DateTime? LastAttemptDate { get; set; }

	public DataSyncLogType LogType { get; set; }
}

public enum DataSyncLogType
{
	Success = 1,
	Error = 2
}
