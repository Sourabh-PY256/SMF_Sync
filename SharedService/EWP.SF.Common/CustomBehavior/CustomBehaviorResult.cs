using EWP.SF.Common.Attributes;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

using Newtonsoft.Json;

namespace EWP.SF.Common.CustomBehavior;

/// <summary>
///
/// </summary>
public class CustomBehaviorResult
{
	/// <summary>
	///
	/// </summary>
	public string Signature { get; set; }

	/// <summary>
	///
	/// </summary>
	public object Value { get; set; }

	/// <summary>
	///
	/// </summary>
	public CustomBehaviorResult()
	{
	}

	/// <summary>
	///
	/// </summary>
	public CustomBehaviorResult(string signature, object value)
	{
		Signature = signature;
		Value = value;
	}
}
