using System.Xml.Serialization;

namespace EWP.SF.Common.Enumerators;

/// <summary>
///
/// </summary>
[Serializable]
public enum ToolParamType
{
	/// <summary>
	///
	/// </summary>
	[XmlEnum("1")]
	Manual = 1,

	/// <summary>
	///
	/// </summary>
	[XmlEnum("2")]
	Sensor = 2,

	/// <summary>
	///
	/// </summary>
	[XmlEnum("3")]
	Parameter = 3
}
