using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;

namespace EWP.SF.Common.Models;

[GridBDEntityName("AssetShiftsStatus")]
public class SchedulingShiftStatus : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_shiftstatus_log");

	[EntityColumn("ShiftStatusCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	[EntityColumn("ShiftStatusType")]
	public string Type { get; set; }

	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	public decimal Efficiency { get; set; }
	public string Style { get; set; }

	[GridCustomType(Enumerators.GridColumnType.COLOR_HEX, "Color")]
	public string Color { get; set; }

	public decimal Factor { get; set; }
	public bool AllowSetup { get; set; }
	public DateTime? CreationDate { get; set; }

	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int UserId { get; set; }

	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int? CreationById { get; set; }

	public DateTime? ModifiedDate { get; set; }

	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int? ModifiedById { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	public int Status { get; set; }
	public string StatusName { get; set; }
	public List<string> AttachmentIds { get; set; }
}

public class CalendarStateExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Shift Status Code format.")]
	[Description("Shift Status Code")]
	public string ShiftStatusCode { get; set; }

	[MaxLength(150)]
	[Description("Shift Status Name")]
	public string ShiftStatusName { get; set; }

	[MaxLength(15)]
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Shift Status")]
	public string Status { get; set; }

	[MaxLength(15)]
	[RegularExpression("Asset|Employee", ErrorMessage = "Invalid Type")]
	[Description("Shift Status Type")]
	public string Type { get; set; }

	[Description("Efficiency in porcent")]
	public decimal? Efficiency { get; set; }

	[Description("Factor Cost")]
	public decimal? FactorCost { get; set; }

	[MaxLength(4)]
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Allow Setup Flag")]
	[Description("Allow Setup Flag")]
	public string AllowSetup { get; set; }
}
