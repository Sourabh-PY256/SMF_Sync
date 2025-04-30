using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

namespace EWP.SF.Item.BusinessEntities;

public class DataSyncCatalog
{
	public string Code { get; set; }

	public string Name { get; set; }

	public string Icon { get; set; }

	public string ParentCode { get; set; }

	public User CreateUser { get; set; }

	public DateTime CreateDate { get; set; }

	public User UpdateUser { get; set; }

	public DateTime UpdateDate { get; set; }

	public Status Status { get; set; }
}
