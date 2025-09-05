using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using EWP.SF.Common.Attributes;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Custom contract resolver for JSON serialization that allows renaming and ignoring properties.
/// </summary>
public class PropertyRenameAndIgnoreSerializerContractResolver : DefaultContractResolver
{
	private readonly Dictionary<Type, HashSet<string>> _ignores;
	private readonly Dictionary<Type, Dictionary<string, string>> _renames;

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyRenameAndIgnoreSerializerContractResolver" /> class.
	/// </summary>
	public PropertyRenameAndIgnoreSerializerContractResolver()
	{
		_ignores = [];
		_renames = [];
	}

	/// <summary>
	/// Ignores the specified properties of the given type during serialization.
	/// </summary>
	public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
	{
		if (!_ignores.TryGetValue(type, out HashSet<string> value))
		{
			value = [];
			_ignores[type] = value;
		}

		foreach (string prop in jsonPropertyNames)
		{
			value.Add(prop);
		}
	}

	/// <summary>
	/// Renames the specified property of the given type during serialization.
	/// </summary>
	public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
	{
		if (!_renames.TryGetValue(type, out Dictionary<string, string> value))
		{
			value = [];
			_renames[type] = value;
		}

		value[propertyName] = newJsonPropertyName;
	}

	/// <summary>
	/// Creates a <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
	/// </summary>
	/// <param name="member">The member to create a <see cref="JsonProperty" /> for.</param>
	/// <param name="memberSerialization">The member's parent <see cref="MemberSerialization" />.</param>
	/// <returns>A created <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.</returns>
	protected override JsonProperty CreateProperty(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
		MemberInfo member,
		MemberSerialization memberSerialization)
	{
		JsonProperty property = base.CreateProperty(member, memberSerialization);
		if (IsIgnored(property.DeclaringType, property.PropertyName))
		{
			property.ShouldSerialize = static x => false;
			property.Ignored = true;
		}

		if (IsRenamed(property.DeclaringType, property.PropertyName, out string newJsonPropertyName))
		{
			property.PropertyName = newJsonPropertyName;
		}

		return property;
	}

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	private bool IsIgnored(Type type, string jsonPropertyName)
	{
		return type.GetProperty(jsonPropertyName)?.GetCustomAttribute(typeof(JsonIgnoreTransport)) is not null || _ignores.TryGetValue(type, out HashSet<string> value) && value.Contains(jsonPropertyName);
	}

	private bool IsRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
	{
		if (!_renames.TryGetValue(type, out Dictionary<string, string> renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
		{
			newJsonPropertyName = null;
			return false;
		}

		return true;
	}
}

/// <summary>
/// Converts a double value to an integer value.
/// </summary>
public class DoubleToIntegerConverter : JsonConverter
{
	/// <summary>
	/// Determines whether this instance can convert the specified object type.
	/// </summary>
	/// <param name="objectType">Type of the object.</param>
	/// <returns>
	/// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
	/// </returns>
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(int);
	}

	/// <summary>
	/// Reads the JSON representation of the object.
	/// </summary>
	/// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
	/// <param name="objectType">Type of the object.</param>
	/// <param name="existingValue">The existing value of object being read.</param>
	/// <param name="serializer">The calling serializer.</param>
	/// <returns>The object value.</returns>
	/// <exception cref="JsonSerializationException"></exception>
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType is JsonToken.Float or JsonToken.Integer or JsonToken.String)
		{
			double value = Convert.ToDouble(reader.Value);
			return Convert.ToInt32(value);
		}

		throw new JsonSerializationException("Unexpected token type");
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	/// <exception cref="NotImplementedException"></exception>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
