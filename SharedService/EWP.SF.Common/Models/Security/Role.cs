using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;

public class Role : ICloneable, ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_role_log");

	[EntityColumn("RoleId")]
	public int Id { get; set; }
	public string Name { get; set; }
	public string Hash { get; set; }
	public Status Status { get; set; }
	public bool System { get; set; }

	[JsonIgnoreTransport]
	public List<Permission> Permissions { get; set; }

	public Role()
	{
	}

	public Role(int id)
	{
		Id = id;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
