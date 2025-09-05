using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;


/// <summary>
///
/// </summary>
public class Labor
{
	/// <summary>
	///
	/// </summary>
	public Labor()
	{
	}

	/// <summary>
	///
	/// </summary>
	public Labor(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	//public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LaborTime { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Cost { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public Status Status { get; set; }
}
