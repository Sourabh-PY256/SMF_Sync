using EWP.SF.Common.Models;
namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncService
{
	public override string ToString()
	{
		return EntityId;
	}

	public string Id { get; set; }

	public string ErpDataId { get; set; }

	public DataSyncErp ErpData { get; set; }

	public string EntityId { get; set; }

	public string EntityCategoryCode { get; set; }

	public Entity Entity { get; set; }

	public DataSyncErpMapping ErpMapping { get; set; }

	public DataSyncErpAuth TokenData { get; set; }

	public string Path { get; set; }

	public string UrlParams { get; set; }

	public string SingleRecordParam { get; set; }

	public string HttpMethod { get; set; }

	public EnableType TimeTriggerEnable { get; set; }

	public int FrequencyMin { get; set; }

	public EnableType ErpTriggerEnable { get; set; }

	public EnableType SfTriggerEnable { get; set; }

	public EnableType ManualSyncEnable { get; set; }

	public EnableType DeltaSync { get; set; }

	public DateTime LastExecutionDate { get; set; }

	public int OffsetMin { get; set; }

	public int RequestTimeoutSecs { get; set; }

	public int RequestReprocFrequencySecs { get; set; }

	public int RequestReprocMaxRetries { get; set; }
	public EnableType RevalidationEnable { get; set; }

	public User CreatedBy { get; set; }

	public DateTime CreationDate { get; set; }

	public User ModifiedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	public ServiceStatus Status { get; set; }

	public string IndividualSyncProperties { get; set; }

	public int EnableDynamicBody { get; set; }
	public bool MultipleParamsRequest { get; set; }
}
