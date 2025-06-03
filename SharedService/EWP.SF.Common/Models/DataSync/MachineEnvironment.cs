using System.Collections.Concurrent;
using System.ComponentModel;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
[JsonConverter(typeof(MachineEnvironmentConverter))]
public class MachineEnvironment : IDisposable
{
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string WorkOrderId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string WorkOrderNo { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ProcessId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Total { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Received { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Rejected { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string AvgCycle { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Availability { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Performance { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Quality { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string OEE { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Product { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string StartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ProcessStartDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ProcessTotal { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string ProcessReceived { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string IsCycle { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[IgnoreEnvironment]
	public bool IsOutput { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string Output { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string DowntimeDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string DowntimeId { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string OffTime { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	public string OnTime { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[IgnoreEnvironment]
	public double PerformanceFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[IgnoreEnvironment]
	public double ProcessPerformanceFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[IgnoreEnvironment]
	public Dictionary<string, string> Modifiers { get; set; }

	/// <summary>
	///
	/// </summary>
	public Dictionary<string, string> Values
	{
		get
		{
			try
			{
				return GetType().GetProperties().Where(x => !x.IsDefined(typeof(IgnoreEnvironmentAttribute), false) && x.IsDefined(typeof(JsonIgnoreAttribute), false)).Select(x => new KeyValuePair<string, string>("@" + x.Name, x.GetValue(this, null)?.ToString())).ToDictionary(x => x.Key, x => x.Value);
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				return [];
			}
		}
	}

	/// <summary>
	///
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			// Release managed resources here if needed.
			Modifiers = null;
		}
		// Release unmanaged resources here if needed.
	}

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}

/// <summary>
///
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
[DisplayName("IgnoreEnvironment")]
public sealed class IgnoreEnvironmentAttribute : Attribute;

/// <summary>
///
/// </summary>
public class MachineEnvironmentConverter : JsonConverter
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
		return objectType == typeof(MachineEnvironment);
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
		try
		{
			if (reader.TokenType == JsonToken.String)
			{
				return new MachineEnvironment();
			}
		}
		catch (Exception ex)
		{
			throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType}'.", ex);
		}

		throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing {nameof(MachineEnvironment)}.");
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value is null)
		{
			_ = writer.WriteNullAsync().ConfigureAwait(false);
			return;
		}

		MachineEnvironment secondClass = (MachineEnvironment)value;
		_ = writer.WriteStartObjectAsync().ConfigureAwait(false);
		foreach (string x in secondClass.Values.Keys)
		{
			_ = writer.WritePropertyNameAsync(x).ConfigureAwait(false);
			_ = writer.WriteValueAsync(secondClass.Values[x]).ConfigureAwait(false);
		}
		_ = writer.WriteEndObjectAsync().ConfigureAwait(false);
	}
}
