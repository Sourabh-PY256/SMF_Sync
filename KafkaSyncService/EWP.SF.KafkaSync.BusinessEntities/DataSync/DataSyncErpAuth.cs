namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncErpAuth
{
	public string ErpId { get; set; }

	public string Token { get; set; }

	public int? ExpirationTime { get; set; }

	public DateTime ExpirationDate { get; set; }

	public string TokenType { get; set; } = "Bearer";
}
