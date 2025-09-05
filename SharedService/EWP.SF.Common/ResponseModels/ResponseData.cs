using System.Text.Json.Serialization;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json.Converters;

namespace EWP.SF.Common.ResponseModels;


/// <summary>
///
/// </summary>
public class ResponseData
{
	/// <summary>
	///
	/// </summary>
	[JsonIgnoreTransport]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsSuccess { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public ActionDB Action { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public object Entity { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public object EntityAlt { get; set; }
}
