namespace EWP.SF.Item.BusinessEntities;

public enum MigrationType
{
	Material = 1,
	Product = 2,
	Order = 3,
	Stock = 4,
	Quality = 5,
	Catalog = 6,
	Data = 7,
	Demand = 8,
	Supply = 9
}

public class SyncContext
{
	public MigrationType Type { get; set; }
	public string EntityId { get; set; }

	public SyncContext()
	{
	}

	public SyncContext(MigrationType type, string id)
	{
		Type = type;
		EntityId = id;
	}
}
