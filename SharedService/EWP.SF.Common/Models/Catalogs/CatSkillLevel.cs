using EWP.SF.Common.EntityLogger;

namespace EWP.SF.Common.Models.Catalogs;

public class CatSkillLevel
{
	[EntityColumn("SkillsLevelCode")]
	public string SkillLevelId { get; set; }

	public string NameLevel { get; set; }
}
