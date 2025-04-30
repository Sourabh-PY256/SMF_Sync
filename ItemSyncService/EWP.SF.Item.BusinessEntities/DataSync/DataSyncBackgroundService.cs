


using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

namespace EWP.SF.Item.BusinessEntities;
public class DataSyncBackgroundService
{
	public string Id { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string ServiceKey { get; set; }

	public User CreatedBy { get; set; }

	public DateTime CreationDate { get; set; }

	public User ModifiedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	public Status Status { get; set; }
}
