using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Catalogs;

/// <summary>
///
/// </summary>
public class CatSkills : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_skill_log");

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string SkillId { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("SkillCode")]
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
	[GridCustomPropertyName("OperationTypeCode")]
	[GridDrillDown("OperationType")]
	[GridDisabledHiding]
	public string ProcessType { get; set; }

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
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }
}

/// <summary>
///
/// </summary>
public class SkillExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Skill Code")]
	[JsonProperty(PropertyName = "SkillCode")]
	public string SkillCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Skill Name")]
	[JsonProperty(PropertyName = "SkillName")]
	public string SkillName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Skill Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Description("Skill Operation Type")]
	[JsonProperty(PropertyName = "OperationType")]
	public string OperationType { get; set; }
}
