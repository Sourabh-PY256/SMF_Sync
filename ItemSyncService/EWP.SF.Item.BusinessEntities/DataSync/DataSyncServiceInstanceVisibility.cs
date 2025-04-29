namespace EWP.SF.Item.BusinessEntities;

public class DataSyncServiceInstanceVisibility
{
	public string ServiceInstanceId { get; set; }

	public string ServiceInstanceName { get; set; }

	public bool IsActive { get; set; }

	public bool IsVisible { get; set; }

	public bool PostEnabled { get; set; }

	public bool Running { get; set; }
}
