using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using EWP.SF.Common.Attributes;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class PropertyRenameAndIgnoreSerializerContractResolver : DefaultContractResolver
{
	private readonly Dictionary<Type, HashSet<string>> _ignores;
	private readonly Dictionary<Type, Dictionary<string, string>> _renames;

	public PropertyRenameAndIgnoreSerializerContractResolver()
	{
		_ignores = [];
		_renames = [];
	}

	public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
	{
		if (!_ignores.TryGetValue(type, out HashSet<string> value))
		{
			value = [];
			_ignores[type] = value;
		}

		foreach (string prop in jsonPropertyNames)
		{
			_ = value.Add(prop);
		}
	}

	public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
	{
		if (!_renames.TryGetValue(type, out Dictionary<string, string> value))
		{
			value = [];
			_renames[type] = value;
		}

		value[propertyName] = newJsonPropertyName;
	}

	protected override JsonProperty CreateProperty(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
		MemberInfo member,
		MemberSerialization memberSerialization)
	{
		JsonProperty property = base.CreateProperty(member, memberSerialization);
		if (IsIgnored(property.DeclaringType, property.PropertyName))
		{
			property.ShouldSerialize = x => false;
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
		if (type.GetProperty(jsonPropertyName)?.GetCustomAttribute(typeof(JsonIgnoreTransport)) is not null)
		{
			return true;
		}

		return _ignores.TryGetValue(type, out HashSet<string> value) && value.Contains(jsonPropertyName);
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

public class DoubleToIntegerConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(int);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.String)
		{
			double value = Convert.ToDouble(reader.Value);
			return Convert.ToInt32(value);
		}

		throw new JsonSerializationException("Unexpected token type");
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
