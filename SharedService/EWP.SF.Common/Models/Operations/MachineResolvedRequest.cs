
namespace EWP.SF.Common.Models.Operations;


/// <summary>
///
/// </summary>
public class ResolvedMachine
{
	/// <summary>
	///
	/// </summary>
	public Machine Machine { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IgnoreInsert { get; set; }

	/// <summary>
	///
	/// </summary>
	public ResolvedMachine()
	{
	}

	/// <summary>
	///
	/// </summary>
	public ResolvedMachine(Machine machine)
	{
		Machine = machine;
	}
}
