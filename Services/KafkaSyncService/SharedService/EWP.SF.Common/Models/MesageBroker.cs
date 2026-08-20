using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;

public class MessageBroker
{
	/// <summary>
	///
	/// </summary>
	public MessageBrokerType Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ElementId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ElementValue { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime LastDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Aux { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Aux2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public int IdUser { get; set; }
}
