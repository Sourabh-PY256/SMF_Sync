namespace EWP.SF.KafkaSync.BusinessEntities;

public class Entity
{
	public string Id { get; set; }
	public string Icon { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string Module { get; set; }
	public string ModuleIcon { get; set; }
	public string ModuleDescription { get; set; }
	public bool isVisualHelp { get; set; }
	public string NameEntityExternal { get; set; }
	public List<EntityDocImport> ListDocsImport { get; set; }
}

//public class EntityPropety
//{
//    public string NameDisplay { get; set; }
//    public string NamePropety { get; set; }
//    public string ValueType { get; set; }

//}

//public class ResultValidation
//{
//    public bool Success { get; set; }
//    public string EntityName {get;set;}
//    public string EntityCode { get; set; }
//    public string Error { get; set; }
//}

public class EntityDocImport
{
	public string IdEntity { get; set; }
	public int Order { get; set; }
	public string Path { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsMandatory { get; set; }
	public string NameEntityExternal { get; set; }
	public string NamePropertyExternal { get; set; }
	public string ParentEntityExternal { get; set; }
	public string EntityExternalPath { get; set; }
}

public class Module
{
	public string Code { get; set; }
	public string Name { get; set; }
	public string Icon { get; set; }
	public string Url { get; set; }
}
