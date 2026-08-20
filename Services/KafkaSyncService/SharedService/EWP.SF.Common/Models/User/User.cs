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
public class User : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_user_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("UserId")]
	public int Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Name to Display")]
	public string DisplayName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Username { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Password { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Date of last login")]
	public DateTime LastLoginDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Language Code")]
	public string LanguageCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Date of record creation")]
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("User who created the record")]
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Date of last edit")]
	public DateTime ModificationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Last user who edited the record")]
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Internal value")]
	public string RegisterCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Hash { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SecretKey { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Permission> Permissions { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<UserPermissions> UserPermissions { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ReqPermissions> PermissionsModule { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? DefaultMenuId { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool? UsedMultiFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public Role Role { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkCenterId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string WorkCenterBlockId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PrintingStation { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PrintingMachine { get; set; }

	/// <summary>
	///
	/// </summary>
	public string UserTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public int DefaultLayout { get; set; }

	/// <summary>
	///
	/// </summary>
	public string IP { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EmployeeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Aux { get; set; }

	/// <summary>
	///
	/// </summary>
	public string HashDashboards { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AttendanceControl { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool ExecutionControl { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool HasMultiFactor { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool? TemporalPassword { get; set; }

	/// <summary>
	///
	/// </summary>
	public object UserFields { get; set; }

	/// <summary>
	///
	/// </summary>
	[System.Text.Json.Serialization.JsonIgnore]
	public double TimeZoneOffset { get; set; }
	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public User()
	{
		Permissions = [];
		Role = new Role();
	}

	/// <summary>
	///
	/// </summary>
	public User(int Id)
	{
		this.Id = Id;
	}

	/// <summary>
	///
	/// </summary>
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

/// <summary>
///
/// </summary>
public class UserExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("User name")]
	public string UserName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[MaxLength(50)]
	[Description("Status user")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Terminal|Employee", ErrorMessage = "Invalid UserType")]
	[MaxLength(50)]
	[Description("User type")]
	public string UserType { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(200)]
	[RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Invalid email format.")]
	[Description("Email")]
	public string Email { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(300)]
	[Description("Printer name assigned")]
	public string Printer { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Employee Code relationship to user")]
	public string EmployeeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("IP connected user")]
	public string IPWhiteList { get; set; }
}
