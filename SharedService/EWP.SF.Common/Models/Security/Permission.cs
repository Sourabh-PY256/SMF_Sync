namespace EWP.SF.Common.Models;


/// <summary>
/// Represents a permission entity.
/// </summary>
public class Permission
{
	/// <summary>
	/// Gets or sets the ID of the permission.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Gets or sets the name of the permission.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the code of the permission.
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	/// Gets or sets the description of the permission.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets the hash of the permission.
	/// </summary>
	public string Hash { get; set; }

	/// <summary>
	/// Gets or sets the category of the permission.
	/// </summary>
	public int Category { get; set; }

	/// <summary>
	/// Gets or sets the role associated with the permission.
	/// </summary>
	public Role Role { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Permission" /> class.
	/// </summary>
	public Permission()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Permission" /> class with a specified ID.
	/// </summary>
	/// <param name="id">The ID of the permission.</param>
	public Permission(string id)
	{
		Id = id;
	}
}
