using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Models;

namespace EWP.SF.Common.Attributes;

[AttributeUsage(AttributeTargets.All)]
public sealed class Column(int column) : Attribute
{
	public int ColumnIndex { get; set; } = column;
}

[AttributeUsage(AttributeTargets.All)]
[MetadataType(typeof(Catalog))]
public sealed class CatalogAttribute(string name) : Attribute
{
	public string Name { get; internal set; } = name;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonIgnoreTransport : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class OffsetIgnore : Attribute
{
}
