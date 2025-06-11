using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncErpMapping
{
	public string Id { get; set; }

	public string ErpId { get; set; }

	public string HttpMethod { get; set; }

	public string EntityId { get; set; }

	public string ResponseMapSchema { get; set; }

	public string RequestMapSchema { get; set; }

	public string ExpectedResult { get; set; }

	public string ResultProperty { get; set; }

	public string ErrorProperty { get; set; }

	public User CreateUser { get; set; }

	public DateTime CreateDate { get; set; }

	public User UpdateUser { get; set; }

	public DateTime UpdateDate { get; set; }

	public Status Status { get; set; }
}
