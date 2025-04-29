using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json;

namespace EWP.SF.Common.Models;

public class User : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_user_log");

	public User()
	{
		Permissions = [];
		Role = new Role();
	}

	public User(int Id)
	{
		this.Id = Id;
	}

	[EntityColumn("UserId")]
	public int Id { get; set; }

	[Description("Name to Display")]
	public string DisplayName { get; set; }

	public string Email { get; set; }

	public string Username { get; set; }

	public string Password { get; set; }

	[Description("Date of last login")]
	public DateTime LastLoginDate { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	public Status Status { get; set; }

	[Description("Language Code")]
	public string LanguageCode { get; set; }

	[Description("Date of record creation")]
	public DateTime CreationDate { get; set; }

	[Description("User who created the record")]
	public User CreatedBy { get; set; }

	[Description("Date of last edit")]
	public DateTime ModificationDate { get; set; }

	[Description("Last user who edited the record")]
	public User ModifiedBy { get; set; }

	[Description("Internal value")]
	public string RegisterCode { get; set; }

	public string Hash { get; set; }
	public string SecretKey { get; set; }
	public List<Permission> Permissions { get; set; }
	public List<UserPermissions> UserPermissions { get; set; }
	public List<ReqPermissions> PermissionsModule { get; set; }
	public int? DefaultMenuId { get; set; }

	[GridIgnoreProperty]
	public Role Role { get; set; }

	public string WorkCenterId { get; set; }
	public string WorkCenterBlockId { get; set; }

	public string PrintingStation { get; set; }
	public string PrintingMachine { get; set; }
	public string UserTypeCode { get; set; }
	public int DefaultLayout { get; set; }
	public string IP { get; set; }
	public string EmployeeId { get; set; }
	public string Aux { get; set; }
	public string HashDashboards { get; set; }
	public bool AttendanceControl { get; set; }
	public bool ExecutionControl { get; set; }
	public bool HasMultiFactor { get; set; }
	public bool? TemporalPassword { get; set; }
	public object UserFields { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public double TimeZoneOffset { get; set; }

	public string PermissionsModuleToJSON()
	{
		string returnValue = null;
		if (PermissionsModule is not null)
		{
			returnValue = JsonConvert.SerializeObject(PermissionsModule);
		}
		return returnValue;
	}
}

public class UserExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("User name")]
	public string UserName { get; set; }

	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[MaxLength(50)]
	[Description("Status user")]
	public string Status { get; set; }

	[RegularExpression("Terminal|Employee", ErrorMessage = "Invalid UserType")]
	[MaxLength(50)]
	[Description("User type")]
	public string UserType { get; set; }

	[Required]
	[MaxLength(200)]
	[RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Invalid email format.")]
	[Description("Email")]
	public string Email { get; set; }

	[MaxLength(300)]
	[Description("Printer name assigned")]
	public string Printer { get; set; }

	[MaxLength(100)]
	[Description("Employee Code relationship to user")]
	public string EmployeeCode { get; set; }

	[MaxLength(100)]
	[Description("IP connected user")]
	public string IPWhiteList { get; set; }
}
