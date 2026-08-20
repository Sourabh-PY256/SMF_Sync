using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
[GridBDEntityName("AssetShifts")]
public class SchedulingShift : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_shift_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ShiftCode")]
	[GridRequireDecode]
	[GridDrillDown]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ShiftType")]
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
	[GridCustomType(GridColumnType.TIME, "Time", GridColumnFormat.DDHHMMSS)]
	public long Duration { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? ReferenceDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime? CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int? CreationById { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "User", "Id", "DisplayName")]
	public int UserId { get; set; }

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
	[GridRequireTranslate]
	public int Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string StatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool IsCheckInOut { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<SchedulingShiftDetail> ShiftDetails { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingShift()
	{
		ShiftDetails = [];
	}

	/// <summary>
	///
	/// </summary>
	public string ShiftDetailsToJSON()
	{
		string returnValue = string.Empty;
		if (ShiftDetails is not null)
		{
			returnValue = JsonConvert.SerializeObject(ShiftDetails);
		}
		return returnValue;
	}
}

/// <summary>
///
/// </summary>
public class SchedulingShiftExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Shift Code")]
	public string ShiftCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Shift Name")]
	public string ShiftName { get; set; }

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
	[Description("Shift Type")]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Duration in minutes")]
	public long DurationInMin { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Reference Date")]
	public DateTime? RefDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Periods")]
	public List<ShiftPeriods> Periods { get; set; }
}

/// <summary>
///
/// </summary>
public class ShiftPeriods
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Shift Code format.")]
	[Description("Shift Code")]
	public string ShiftCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Shift Status Code")]
	public string ShiftStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Efficiency")]
	public decimal? Efficiency { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Duration in minutes")]
	public long DurationInMin { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[Description("Order")]
	public int Order { get; set; }
}
