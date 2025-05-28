using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Catalogs;

public class CatSkills : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_skill_log");

	[GridIgnoreProperty]
	public string SkillId { get; set; }

	[EntityColumn("SkillCode")]
	[GridDrillDown]
	[GridDisabledHiding]
	public string Code { get; set; }

	public string Name { get; set; }

	[GridCustomPropertyName("OperationTypeCode")]
	[GridDrillDown("OperationType")]
	[GridDisabledHiding]
	public string ProcessType { get; set; }

	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }
}

public class SkillExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Skill Code")]
	[JsonProperty(PropertyName = "SkillCode")]
	public string SkillCode { get; set; }

	[MaxLength(200)]
	[Description("Skill Name")]
	[JsonProperty(PropertyName = "SkillName")]
	public string SkillName { get; set; }

	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Skill Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	[MaxLength(100)]
	[Description("Skill Operation Type")]
	[JsonProperty(PropertyName = "OperationType")]
	public string OperationType { get; set; }
}
