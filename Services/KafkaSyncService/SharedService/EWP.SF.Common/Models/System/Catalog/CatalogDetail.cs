namespace EWP.SF.Common.Models;

/// <summary>
/// Catalog Detail
/// </summary>
public class CatalogDetail
{
	/// <summary>
	/// Gets or sets the unique identifier for the catalog detail.
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Gets or sets the key for the catalog detail.
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Gets or sets the value for the catalog detail.
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	/// Gets or sets the auxiliary information for the catalog detail.
	/// </summary>
	public string Aux { get; set; }

	/// <summary>
	/// Gets or sets the unique identifier for the catalog detail.
	/// </summary>
	public CatalogDetail()
	{
	}

	/// <summary>
	/// Initializes a new instance of the CatalogDetail class with the specified identifier, key, value, and auxiliary information.
	/// </summary>
	public CatalogDetail(string id, string key, string value, string aux = "")
	{
		Id = id;
		Key = key;
		Value = value;
		Aux = aux;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Id + "-" + Value;
	}

}


	public class TimeZoneCatalog
{
	public string Key { get; set; }
	public string Value { get; set; }
	public double Offset { get; set; }

	public override string ToString()
	{
		return Key;
	}
}