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
public class MasterDetailDataRequest<T>
{
	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<T> Details { get; set; }
}
