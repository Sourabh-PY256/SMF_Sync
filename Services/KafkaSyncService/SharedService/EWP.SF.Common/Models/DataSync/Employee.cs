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
public class Employee : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public Employee()
	{
		EmployeeAssets = [];
	}

	/// <summary>
	///
	/// </summary>
	public Employee(string Code)
	{
		this.Code = Code;
	}

	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_employee_log");

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("EmployeeCode")]
	[GridDrillDown]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Password { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ExpDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("PasswordChange")]
	public bool FlagPassword { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("RequiredCredentials")]
	public bool RequiresCredentials { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool ShowAllLots { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public bool ParallelCheckIn { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool ShowQuantityFields { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ReportsTo")]
	[GridDrillDown]
	[GridDisabledHiding]
	public string SupervisorId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AuthorizationRequired { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string AssetsId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	[GridDisableSorting]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LastName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PhoneNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Street { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? Number { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string Region { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Country { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StateProvince { get; set; }

	/// <summary>
	///
	/// </summary>
	public string City { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Other { get; set; }

	/// <summary>
	///
	/// </summary>
	public int Role { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ZipCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Genre { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Datebirth")]
	public DateTime DateBirth { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PlaceBirth { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MaritalStatus { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? NumberChildren { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IDNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Nationality { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PassportNumber { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PassportIssueDate { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime PassportExpiry { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IssuingAuthority { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Tolerance")]
	public decimal TimeTolerance { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("GroupNotifications")]
	public string NotificationGroup { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public int UserId { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal? CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<EmployeeSkills> EmployeeSkills { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<AssetsTree> EmployeeAssets { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<EmployeeContractsDetail> EmployeeContractsDetail { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ActualCharge")]
	[GridLookUpEntity(null, "Position", "ProfileId", "NameProfile")]
	public string CurrentPositionId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ExternalID")]
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string LogDetailId { get; set; }
	/// <summary>
	///
	/// </summary>
	public double? Availability { get; set; }
	/// <summary>
	///
	/// </summary>
	public double? Efficiency { get; set; }
	/// <summary>
	///
	/// </summary>
	public double? Performance { get; set; }
	/// <summary>
	///
	/// </summary>
	public double? Quality { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeAssetsToJSON()
	{
		string returnValue = string.Empty;
		if (EmployeeAssets is not null)
		{
			returnValue = JsonConvert.SerializeObject(EmployeeAssets);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Activities { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts Shift { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts ShiftDelete { get; set; }
}

/// <summary>
///
/// </summary>
public class EmployeeExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Employee Code format.")]
	[Description("Employee Code")]
	public string EmployeeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Employee Name")]
	public string EmployeeName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Employee Last Name")]
	public string EmployeeLastName { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(15)]
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Cost Per Hour")]
	public decimal CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Profile Code")]
	public string ProfileCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("ExternalId")]
	public string ExternalId { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Email")]
	public string Email { get; set; }
}
