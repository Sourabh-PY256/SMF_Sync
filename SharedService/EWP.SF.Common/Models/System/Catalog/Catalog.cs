namespace EWP.SF.Common.Models;

public class Catalog
{
	public Catalog()
	{
	}

	public Catalog(int id, string value)
	{
		Id = id;
		Name = value;
	}

	public int Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public List<CatalogDetail> Details { get; set; }

	public override string ToString()
	{
		return Name;
	}
}
