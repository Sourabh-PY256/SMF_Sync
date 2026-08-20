using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;


/// <summary>
/// Role
/// </summary>
public class Role : ICloneable, ILoggableEntity
{
	/// <summary>
	/// Gets the entity log configuration for the role.
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_role_log");

	/// <summary>
	/// Gets or sets the unique identifier for the role.
	/// </summary>
	[EntityColumn("RoleId")]
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the name of the role.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the description of the role.
	/// </summary>
	public string Hash { get; set; }

	/// <summary>
	/// Gets or sets the status of the role.
	/// </summary>
	public Status Status { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the role is a system role.
	/// </summary>
	public bool System { get; set; }

	/// <summary>
	/// Gets or sets the list of permissions associated with the role.
	/// </summary>
	[JsonIgnoreTransport]
	public List<Permission> Permissions { get; set; }

	/// <summary>
	/// Initializes a new instance of the Role class.
	/// </summary>
	public Role()
	{
	}

	/// <summary>
	/// Initializes a new instance of the Role class with the specified identifier.
	/// </summary>
	public Role(int id)
	{
		Id = id;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
