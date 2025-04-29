using System.Xml.Serialization;

namespace EWP.SF.Item.BusinessEntities;

[Serializable]
public enum ToolParamType
{
	[XmlEnum("1")]
	Manual = 1,

	[XmlEnum("2")]
	Sensor = 2,

	[XmlEnum("3")]
	Parameter = 3
}
