using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
[GridBDEntityName("AssetShiftsStatus")]
public class SchedulingShiftStatus : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_shiftstatus_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ShiftStatusCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ShiftStatusType")]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Efficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Style { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(Enumerators.GridColumnType.COLOR_HEX, "Color")]
	public string Color { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Factor { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AllowSetup { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int? CreationById { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ModifiedDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int? ModifiedById { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }
}

/// <summary>
///
/// </summary>
public class CalendarStateExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Shift Status Code format.")]
	[Description("Shift Status Code")]
	public string ShiftStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(150)]
	[Description("Shift Status Name")]
	public string ShiftStatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(15)]
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Shift Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(15)]
	[RegularExpression("Asset|Employee", ErrorMessage = "Invalid Type")]
	[Description("Shift Status Type")]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Efficiency in porcent")]
	public decimal? Efficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Factor Cost")]
	public decimal? FactorCost { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(4)]
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Allow Setup Flag")]
	[Description("Allow Setup Flag")]
	public string AllowSetup { get; set; }
}
