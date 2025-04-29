namespace EWP.SF.Item.BusinessEntities;

public class DataSyncMapSchema
{
	public string OriginProperty { get; set; }

	public string MapProperty { get; set; }

	public string Type { get; set; }

	public string DefaultValue { get; set; }

	public List<DataSyncMapSchema> Children { get; set; }
}
