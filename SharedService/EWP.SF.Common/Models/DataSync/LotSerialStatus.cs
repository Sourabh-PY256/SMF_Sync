using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.Constants;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json;
using EWP.SF.Common.EntityLogger;


namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class LotSerialStatus : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_lotserial_status_log");

	/// <summary>
	///
	/// </summary>
	[EntityColumn("LotSerialStatusCode")]
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("CreatedBy")]
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ModifiedBy")]
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User UpdateUser { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime UpdateDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AllowIssue { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }
}

/// <summary>
///
/// </summary>
public class LotSerialStatusExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Lot-Serial Status Code")]
	[JsonProperty(PropertyName = "LotSerialStatusCode")]
	public string LotSerialStatusCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	[Description("Lot-Serial Status Name")]
	[JsonProperty(PropertyName = "LotSerialStatusName")]
	public string LotSerialStatusName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Lot-Serial Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid allowed to use status")]
	[Description("Allowed to issue Status")]
	[JsonProperty(PropertyName = "AllowIssue")]
	public string AllowIssue { get; set; }
}
