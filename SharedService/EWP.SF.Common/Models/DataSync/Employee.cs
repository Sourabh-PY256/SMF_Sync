using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models;

public class Employee : ILoggableEntity
{
	public Employee()
	{
		EmployeeAssets = [];
	}

	public Employee(string Code)
	{
		this.Code = Code;
	}

	public EntityLoggerConfig EntityLogConfiguration => new("sf_employee_log");

	[GridIgnoreProperty]
	public string Id { get; set; }

	[EntityColumn("EmployeeCode")]
	[GridDrillDown]
	[GridDisabledHiding]
	public string Code { get; set; }

	public string Name { get; set; }

	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	[GridIgnoreProperty]
	public string Password { get; set; }

	public DateTime ExpDate { get; set; }

	[GridCustomPropertyName("PasswordChange")]
	public bool FlagPassword { get; set; }

	[GridCustomPropertyName("RequiredCredentials")]
	public bool RequiresCredentials { get; set; }

	[GridIgnoreProperty]
	public bool ShowAllLots { get; set; }

	[GridIgnoreProperty]
	public bool ParallelCheckIn { get; set; }

	public bool ShowQuantityFields { get; set; }

	[GridCustomPropertyName("ReportsTo")]
	[GridDrillDown]
	[GridDisabledHiding]
	public string SupervisorId { get; set; }

	public bool AuthorizationRequired { get; set; }

	[GridIgnoreProperty]
	public string AssetsId { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	[GridDisableSorting]
	public string Image { get; set; }

	public string LastName { get; set; }
	public string Email { get; set; }
	public string PhoneNumber { get; set; }
	public string Street { get; set; }
	public int? Number { get; set; }

	[GridIgnoreProperty]
	public string Region { get; set; }

	public string Country { get; set; }
	public string StateProvince { get; set; }
	public string City { get; set; }
	public string Other { get; set; }
	public int Role { get; set; }
	public string ZipCode { get; set; }

	public string Genre { get; set; }

	[GridCustomPropertyName("Datebirth")]
	public DateTime DateBirth { get; set; }

	public string PlaceBirth { get; set; }
	public string MaritalStatus { get; set; }
	public int? NumberChildren { get; set; }
	public string IDNumber { get; set; }
	public string Nationality { get; set; }

	public string PassportNumber { get; set; }

	public DateTime PassportIssueDate { get; set; }
	public DateTime PassportExpiry { get; set; }
	public string IssuingAuthority { get; set; }

	[GridCustomPropertyName("Tolerance")]
	public decimal TimeTolerance { get; set; }

	[GridCustomPropertyName("GroupNotifications")]
	public string NotificationGroup { get; set; }

	[GridIgnoreProperty]
	public int UserId { get; set; }

	public decimal? CostPerHour { get; set; }

	public List<EmployeeSkills> EmployeeSkills { get; set; }
	public List<AssetsTree> EmployeeAssets { get; set; }

	public List<EmployeeContractsDetail> EmployeeContractsDetail { get; set; }

	[GridCustomPropertyName("ActualCharge")]
	[GridLookUpEntity(null, "Position", "ProfileId", "NameProfile")]
	public string CurrentPositionId { get; set; }

	[GridCustomPropertyName("ExternalID")]
	public string ExternalId { get; set; }

	[GridIgnoreProperty]
	public string LogDetailId { get; set; }

	public string EmployeeAssetsToJSON()
	{
		string returnValue = string.Empty;
		if (EmployeeAssets is not null)
		{
			returnValue = JsonConvert.SerializeObject(EmployeeAssets);
		}
		return returnValue;
	}

	public List<string> AttachmentIds { get; set; }
	public List<Activity> Activities { get; set; }
	public SchedulingCalendarShifts Shift { get; set; }
	public SchedulingCalendarShifts ShiftDelete { get; set; }
}

public class EmployeeExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[RegularExpression("^[a-zA-Z0-9-| ]*$", ErrorMessage = "Invalid Employee Code format.")]
	[Description("Employee Code")]
	public string EmployeeCode { get; set; }

	[MaxLength(500)]
	[Description("Employee Name")]
	public string EmployeeName { get; set; }

	[MaxLength(500)]
	[Description("Employee Last Name")]
	public string EmployeeLastName { get; set; }

	[MaxLength(15)]
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Status")]
	public string Status { get; set; }

	[Description("Cost Per Hour")]
	public decimal CostPerHour { get; set; }

	[MaxLength(100)]
	[Description("Profile Code")]
	public string ProfileCode { get; set; }

	[MaxLength(100)]
	[Description("ExternalId")]
	public string ExternalId { get; set; }

	[MaxLength(200)]
	[Description("Email")]
	public string Email { get; set; }
}
