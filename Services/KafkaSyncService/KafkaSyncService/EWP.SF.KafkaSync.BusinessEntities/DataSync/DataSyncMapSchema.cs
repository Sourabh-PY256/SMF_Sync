namespace EWP.SF.KafkaSync.BusinessEntities;

public class DataSyncMapSchema
{
	public string OriginProperty { get; set; }

	public string MapProperty { get; set; }

	public string Type { get; set; }

	public string DefaultValue { get; set; }

	public List<DataSyncMapSchema> Children { get; set; }
	   //  Added MappingValues support
    public List<MappingValue> MappingValues { get; set; }
}

public class MappingValue
{
    public string SFValue { get; set; }
    public string ERPValue { get; set; }
}

