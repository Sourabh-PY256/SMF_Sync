namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncTokenRenewSchema
{
	public TokenRenewOrigin Origin { get; set; }

	public List<DataSyncMapSchema> MapSchema { get; set; }
}
