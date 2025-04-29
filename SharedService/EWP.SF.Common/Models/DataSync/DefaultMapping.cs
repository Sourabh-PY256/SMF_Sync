namespace EWP.SF.Common.Models;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class DefaultMappingEntity(string column) : Attribute
{
	public string EntityName { get; set; } = column;
}

public class DefaultMappingEntityObject
{
	public string Id { get; set; }

	public string Name { get; set; }
}
