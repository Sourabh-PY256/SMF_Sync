using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Operations;


/// <summary>
///
/// </summary>
public class NotifyParameterRequest
{
	/// <summary>
	///
	/// </summary>
	public MachineParam Parameter { get; set; }

	/// <summary>
	///
	/// </summary>
	public Machine Machine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Level { get; set; } = 1;

	/// <summary>
	///
	/// </summary>
	public string DetailMessage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Parameters { get; set; }
}
