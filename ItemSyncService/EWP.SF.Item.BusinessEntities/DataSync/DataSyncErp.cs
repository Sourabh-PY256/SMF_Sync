using EWP.SF.Common.Models;
namespace EWP.SF.Item.BusinessEntities;

public class DataSyncErp
{
	public string Id { get; set; }

	public string ErpCode { get; set; }

	public string Erp { get; set; }

	public string ErpVersionCode { get; set; }

	public string ErpVersion { get; set; }

	public string DbCode { get; set; }

	public string Db { get; set; }

	public string DbVersionCode { get; set; }

	public string DbVersion { get; set; }

	public string ManufacturingCode { get; set; }

	public string Manufacturing { get; set; }

	public string ManufacturingVersionCode { get; set; }

	public string ManufacturingVersion { get; set; }

	public string BaseUrl { get; set; }

	public string TokenRequestJson { get; set; }

	public string TokenRequestPath { get; set; }

	public string TokenRequestMapSchema { get; set; }

	public string TokenRequestResultProp { get; set; }

	public EnableType RequiresTokenRenewal { get; set; }

	public string TokenRenewalMapSchema { get; set; }

	public string RequiredHeaders { get; set; }

	public DateTimeFormatType DateTimeFormat { get; set; }

	public string TimeZone { get; set; }

	public int ReprocessingTime { get; set; }

	public int MaxReprocessingTime { get; set; }

	public int ReprocessingTimeOffset { get; set; }

	public User CreateUser { get; set; }

	public DateTime CreateDate { get; set; }

	public User UpdateUser { get; set; }

	public DateTime UpdateDate { get; set; }

	public Status Status { get; set; }

	public List<DataSyncService> Instances { get; set; }
}
