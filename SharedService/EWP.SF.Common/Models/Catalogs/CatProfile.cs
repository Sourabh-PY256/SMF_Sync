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
public class CatProfile : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public CatProfile()
	{
		PositionSkills = [];
		PositionAssets = [];
	}

	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_position_log");

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string ProfileId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("Name")]
	public string NameProfile { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("PositionCode")]
	[GridDrillDown]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Skills { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool AuthorizationRequired { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridIgnoreProperty]
	public string AssignedAsset { get; set; }

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

	/// <summary>
	///
	/// </summary>
	public string Attribute1 { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal? Attribute2 { get; set; }

	/// <summary>
	///
	/// </summary>
	public long? Attribute3 { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("SchedulingLevel")]
	public string ScheduleLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Schedule { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal? CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	[GridDisableSorting]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<PositionSkills> PositionSkills { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<PositionAssets> PositionAssets { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string PositionSkillsToJSON()
	{
		string returnValue = string.Empty;
		if (PositionSkills is not null)
		{
			returnValue = JsonConvert.SerializeObject(PositionSkills);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public string PositionAssetsToJSON()
	{
		string returnValue = string.Empty;
		if (PositionAssets is not null)
		{
			returnValue = JsonConvert.SerializeObject(PositionAssets);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	[JsonIgnore]
	[GridIgnoreProperty]
	public bool HasError { get; set; }
}

/// <summary>
///
/// </summary>
public class PositionSkills
{
	/// <summary>
	///
	/// </summary>
	public string SkillCode { get; set; }
}

/// <summary>
///
/// </summary>
public class PositionAssets
{
	/// <summary>
	///
	/// </summary>
	public string AssetCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string AssetTypeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class PositionExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Position Code")]
	[JsonProperty(PropertyName = "PositionCode")]
	public string PositionCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Position Name")]
	[JsonProperty(PropertyName = "PositionName")]
	public string PositionName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Position Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Value for ReqAuthorization")]
	[Description("Requires Authorization")]
	[JsonProperty(PropertyName = "ReqAuthorization")]
	public string ReqAuthorization { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Cost per hour")]
	[JsonProperty(PropertyName = "CostPerHour")]
	public double CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	[Description("Profile Skills")]
	[JsonProperty(PropertyName = "Skills")]
	public List<ProfileSkillExternal> Skills { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Primary|Secondary", ErrorMessage = "Invalid scheduling level")]
	[MaxLength(100)]
	public string ScheduleLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid value")]
	[MaxLength(100)]
	public string Schedule { get; set; }
}

/// <summary>
///
/// </summary>
public class ProfileSkillExternal : SkillExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Profile Code")]
	[JsonProperty(PropertyName = "ProfileCode")]
	public string ProfileCode { get; set; }
}
