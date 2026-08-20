using System.Xml.Serialization;

namespace EWP.SF.Common.Enumerators;
/// <summary>
///
/// </summary>
[Serializable]
public enum ComponentType
{
	/// <summary>
	///
	/// </summary>
	[XmlEnum("1")]
	Material = 1,

	/// <summary>
	///
	/// </summary>
	[XmlEnum("2")]
	Product = 2
}

/// <summary>
///
/// </summary>
[Serializable]
public enum TaskType
{
	/// <summary>
	///
	/// </summary>
	[XmlEnum("1")]
	Before = 1,

	/// <summary>
	///
	/// </summary>
	[XmlEnum("2")]
	During = 2,

	/// <summary>
	///
	/// </summary>
	[XmlEnum("3")]
	After = 3
}
