using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Models;

namespace EWP.SF.Common.Attributes;

/// <summary>
/// Attribute to specify the column index for a property.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class Column(int column) : Attribute
{
	/// <summary>
	/// Gets the column index.
	/// </summary>
	public int ColumnIndex { get; set; } = column;
}

/// <summary>
/// Attribute to specify the catalog name for a property.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
[MetadataType(typeof(Catalog))]
public sealed class CatalogAttribute(string name) : Attribute
{
	/// <summary>
	/// Gets the catalog name.
	/// </summary>
	public string Name { get; internal set; } = name;
}

/// <summary>
/// Attribute to specify that a property should be ignored during JSON serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonIgnoreTransport : Attribute;

/// <summary>
/// Attribute to specify that a property should be ignored during offset calculations.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OffsetIgnore : Attribute;
