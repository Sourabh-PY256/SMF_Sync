using System.Xml.Serialization;

namespace EWP.SF.Item.BusinessEntities;

[Serializable]
public enum ComponentType
{
	[XmlEnum("1")]
	Material = 1,

	[XmlEnum("2")]
	Product = 2
}

[Serializable]
public enum TaskType
{
	[XmlEnum("1")]
	Before = 1,

	[XmlEnum("2")]
	During = 2,

	[XmlEnum("3")]
	After = 3
}
