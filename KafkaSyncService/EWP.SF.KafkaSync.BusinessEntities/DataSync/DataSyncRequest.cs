namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncExecuteRequest
{
	public List<string> Services { get; set; }

	public string EntityCode { get; set; }

	public string BodyData { get; set; }
}
