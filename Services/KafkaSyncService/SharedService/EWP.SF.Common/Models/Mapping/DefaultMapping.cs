namespace EWP.SF.Common.Models;


/// <summary>
///
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class DefaultMappingEntity(string column) : Attribute
{
	/// <summary>
	///
	/// </summary>
	public string EntityName { get; set; } = column;
}

/// <summary>
///
/// </summary>
public class DefaultMappingEntityObject
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }
}
