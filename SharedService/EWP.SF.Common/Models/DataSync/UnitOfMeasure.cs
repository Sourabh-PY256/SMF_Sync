using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class MeasureUnitExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Unit Of Measure Code")]
	[JsonProperty(PropertyName = "UoMCode")]
	public string UoMCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Unit Of Measure Name")]
	[JsonProperty(PropertyName = "UoMName")]
	public string UoMName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Unit Of Measure Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Time|Discrete|Mass|Distance|Volume|Temperature|Frequency|Pressure|Vibration|Density|Conductivity|Flux|Custom", ErrorMessage = "Invalid Unit Type")]
	[Description("Unit Type")]
	[JsonProperty(PropertyName = "UnitType")]
	public string UnitType { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Factor")]
	[JsonProperty(PropertyName = "Factor")]
	public double? Factor { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid value")]
	[Description("IsInventory")]
	[JsonProperty(PropertyName = "IsInventory")]
	public string IsInventory { get; set; }
}
