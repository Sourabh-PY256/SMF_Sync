namespace EWP.SF.Common.Models;

public class Permission
{
	public Permission()
	{
	}

	public Permission(string id)
	{
		Id = id;
	}

	public string Id { get; set; }
	public string Name { get; set; }
	public string Code { get; set; }
	public string Description { get; set; }
	public string Hash { get; set; }
	public int Category { get; set; }
	public Role Role { get; set; }
}
