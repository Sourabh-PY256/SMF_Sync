using System.Text.Json.Serialization;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.ResponseModels;

public class ResponseData
{
	[JsonIgnoreTransport]
	public string Id { get; set; }

	public string Code { get; set; }
	public int Version { get; set; }
	public bool IsSuccess { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public ActionDB Action { get; set; }

	public string Message { get; set; }
	public object Entity { get; set; }
	public string Type { get; set; }
	public string Status { get; set; }

	[JsonIgnore]
	public object EntityAlt { get; set; }
}
