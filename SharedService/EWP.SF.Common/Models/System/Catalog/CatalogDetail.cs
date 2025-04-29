namespace EWP.SF.Common.Models;

public class CatalogDetail
{
	public CatalogDetail()
	{
	}

	public CatalogDetail(string id, string key, string value, string aux = "")
	{
		Id = id;
		Key = key;
		Value = value;
		Aux = aux;
	}

	public string Id { get; set; }
	public string Key { get; set; }
	public string Value { get; set; }
	public string Aux { get; set; }

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
