namespace EWP.SF.Common.Models;

/// <summary>
/// Catalog
/// </summary>
public class Catalog
{
	/// <summary>
	/// Gets or sets the unique identifier for the catalog.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the name of the catalog.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the description of the catalog.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets the list of details for the catalog.
	/// </summary>
	public List<CatalogDetail> Details { get; set; }

	/// <summary>
	/// Initializes a new instance of the Catalog class.
	/// </summary>
	public Catalog()
	{
	}

	/// <summary>
	/// Initializes a new instance of the Catalog class with the specified identifier and value.
	/// </summary>
	public Catalog(int id, string value)
	{
		Id = id;
		Name = value;
	}

	/// <summary>
	/// Returns a string that represents the current object.
	/// </summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Name;
	}
}
