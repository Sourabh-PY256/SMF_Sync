using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.API;

/// <summary>
/// This service's MVC pipeline uses Newtonsoft (AddNewtonsoftJson), which cannot bind or write
/// System.Text.Json's JsonElement — a [FromBody] JsonElement arrives as default(JsonElement)
/// (ValueKind Undefined) and then throws "Operation is not valid due to the current state of the
/// object" when System.Text.Json serializes it for Kafka. Controllers therefore bind a JToken
/// and convert at the Kafka boundary (KafkaService itself serializes with System.Text.Json), and
/// convert replies back to a JToken so ResponseModel.Data is written correctly by Newtonsoft.
/// </summary>
internal static class RelayJson
{
	public static JsonElement ToElement(JToken token)
	{
		using JsonDocument doc = JsonDocument.Parse((token ?? JValue.CreateNull()).ToString(Formatting.None));
		return doc.RootElement.Clone();
	}

	public static JToken ToToken(JsonElement element) =>
		element.ValueKind == JsonValueKind.Undefined ? JValue.CreateNull() : JToken.Parse(element.GetRawText());
}
