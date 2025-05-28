using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.Catalogs;

public class CatProfile : ILoggableEntity
{
	public CatProfile()
	{
		PositionSkills = [];
		PositionAssets = [];
	}

	public EntityLoggerConfig EntityLogConfiguration => new("sf_position_log");

	[GridIgnoreProperty]
	public string ProfileId { get; set; }

	[GridCustomPropertyName("Name")]
	public string NameProfile { get; set; }

	[EntityColumn("PositionCode")]
	[GridDrillDown]
	[GridDisabledHiding]
	public string Code { get; set; }

	public string Skills { get; set; }
	public bool AuthorizationRequired { get; set; }

	[GridIgnoreProperty]
	public string AssignedAsset { get; set; }

	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	public string Attribute1 { get; set; }
	public decimal? Attribute2 { get; set; }
	public long? Attribute3 { get; set; }

	[GridCustomPropertyName("SchedulingLevel")]
	public string ScheduleLevel { get; set; }

	public bool Schedule { get; set; }
	public decimal? CostPerHour { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	[GridDisableSorting]
	public string Image { get; set; }

	public List<PositionSkills> PositionSkills { get; set; }
	public List<PositionAssets> PositionAssets { get; set; }
	public string LogDetailId { get; set; }

	public string PositionSkillsToJSON()
	{
		string returnValue = string.Empty;
		if (PositionSkills is not null)
		{
			returnValue = JsonConvert.SerializeObject(PositionSkills);
		}
		return returnValue;
	}

	public string PositionAssetsToJSON()
	{
		string returnValue = string.Empty;
		if (PositionAssets is not null)
		{
			returnValue = JsonConvert.SerializeObject(PositionAssets);
		}
		return returnValue;
	}

	[JsonIgnore]
	[GridIgnoreProperty]
	public bool HasError { get; set; }
}

public class PositionSkills
{
	public string SkillCode { get; set; }
}

public class PositionAssets
{
	public string AssetCode { get; set; }
	public string AssetTypeCode { get; set; }
}

public class PositionExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Position Code")]
	[JsonProperty(PropertyName = "PositionCode")]
	public string PositionCode { get; set; }

	[MaxLength(200)]
	[Description("Position Name")]
	[JsonProperty(PropertyName = "PositionName")]
	public string PositionName { get; set; }

	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Position Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid Value for ReqAuthorization")]
	[Description("Requires Authorization")]
	[JsonProperty(PropertyName = "ReqAuthorization")]
	public string ReqAuthorization { get; set; }

	[Description("Cost per hour")]
	[JsonProperty(PropertyName = "CostPerHour")]
	public double CostPerHour { get; set; }

	[Description("Profile Skills")]
	[JsonProperty(PropertyName = "Skills")]
	public List<ProfileSkillExternal> Skills { get; set; }

	[RegularExpression("Primary|Secondary", ErrorMessage = "Invalid scheduling level")]
	[MaxLength(100)]
	public string ScheduleLevel { get; set; }

	[RegularExpression("Yes|No", ErrorMessage = "Invalid value")]
	[MaxLength(100)]
	public string Schedule { get; set; }
}

public class ProfileSkillExternal : SkillExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Profile Code")]
	[JsonProperty(PropertyName = "ProfileCode")]
	public string ProfileCode { get; set; }
}
