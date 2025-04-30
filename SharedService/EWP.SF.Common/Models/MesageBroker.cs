
using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;

public class MessageBroker
{
	public MessageBrokerType Type { get; set; }
	public string ElementId { get; set; }
	public string ElementValue { get; set; }
	public string MachineId { get; set; }
	public DateTime LastDate { get; set; }

	public string Aux { get; set; }
	public string Aux2 { get; set; }
	public int IdUser { get; set; }
}
