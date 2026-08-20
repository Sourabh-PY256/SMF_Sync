namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncServiceLog
{
	public string Id { get; set; }

	public string ServiceInstanceId { get; set; }

	public DateTime? ExecutionInitDate { get; set; }

	public string ExecutionInitDateTime { get; set; }

	public string ExecutionInitDateShort { get; set; }

	public DateTime? SfProcessDate { get; set; }

	public DateTime? ExecutionFinishDate { get; set; }

	public int? FailedRecords { get; set; }

	public int? SuccessRecords { get; set; }

	public string ErpReceivedJson { get; set; }

	public string SfMappedJson { get; set; }

	public string SfResponseJson { get; set; }

	public string ServiceException { get; set; }

	public ServiceExecOrigin ExecutionOrigin { get; set; }

	public string ExecutionOriginDescription { get; set; }

	public int? LogUser { get; set; }

	public string UserName { get; set; }

	public string LogEmployee { get; set; }

	public string EndpointUrl { get; set; }
	public List<DataSyncServiceLogDetail> Details { get; set; }
}
