using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

using Newtonsoft.Json;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
[GridBDEntityName("OperationSubtype")]
public class ProcessType : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("SF_OperationType_Log");

	/// <summary>
	///
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("OperationTypeCode")]
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	public string Icon { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("UnitType")]
	[GridLookUpEntity(null, "CustomUnitType", "Id", "Name")]
	public int UnitTypeId { get; set; }

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
	public DateTime? ModifyDate { get; set; }

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
	[GridCustomPropertyName("RequiresTools")]
	public bool RequiresTool { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeDetail> Details { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeSubtype> SubTypes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeAttribute> Attributes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Tasks { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> Profiles { get; set; }

	/// <summary>
	///
	/// </summary>
	public int? ProcessTimeType { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public object UserFields { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProcessType()
	{
		Profiles = [];
		Details = [];
		SubTypes = [];
		Attributes = [];
		Tasks = [];
		AttachmentIds = [];
	}

	/// <summary>
	///
	/// </summary>
	public string AttributesToJSON()
	{
		string returnValue = string.Empty;
		if (Attributes is not null)
		{
			returnValue = JsonConvert.SerializeObject(Attributes);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public ProcessType(string id)
	{
		Id = id;
		Details = [];
	}
}

/// <summary>
///
/// </summary>
public class ProcessTypeDetail : ICloneable
{
	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public ProcessTypeDetailSourceType ValueType { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ValueSourceId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	public object Clone() => MemberwiseClone();
}

/// <summary>
///
/// </summary>
public class ProcessTypeSubtype
{
	/// <summary>
	///
	/// </summary>
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Selected { get; set; }
}

/// <summary>
///
/// </summary>
[Serializable]
public enum ProcessTypeDetailSourceType
{
	/// <summary>
	///
	/// </summary>
	[XmlEnum("")]
	Nothing = 0,

	/// <summary>
	///
	/// </summary>
	[XmlEnum("1")]
	Sensor = 1,

	/// <summary>
	///
	/// </summary>
	[XmlEnum("2")]
	Parameter = 2,

	/// <summary>
	///
	/// </summary>
	[XmlEnum("3")]
	Scalar = 3
}

/// <summary>
///
/// </summary>
public class ProcessTypeAttribute
{
	/// <summary>
	///
	/// </summary>
	public string AttributeId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TimeUnit { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Selected { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeAttributeDetail> Details { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessTypeAttributeDetail
{
	/// <summary>
	///
	/// </summary>
	public string FromAttribute { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToAttribute { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Time { get; set; }

	/// <summary>
	///
	/// </summary>
	public int TimeUnit { get; set; }
}

/// <summary>
///
/// </summary>
public class SubProcessTypeExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[MaxLength(100)]
	[Description("Operation Type Code")]
	[JsonProperty(PropertyName = "OperationTypeCode")]
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	[Key]
	[Required]
	[Description("Operation Type Name")]
	[JsonProperty(PropertyName = "OperationSubtypeCode")]
	public string OperationSubtypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Operation Type Name")]
	[JsonProperty(PropertyName = "OperationSubtypeName")]
	public string OperationSubtypeName { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessTypeExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Operation Type Code")]
	[JsonProperty(PropertyName = "OperationTypeCode")]
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Operation Type Name")]
	[JsonProperty(PropertyName = "OperationTypeName")]
	public string OperationTypeName { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Description("Operation Type Unit of Measure")]
	[JsonProperty(PropertyName = "UoM")]
	public string UoM { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Operation Type Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Status")]
	[Description("Operation Type Require Tools")]
	[JsonProperty(PropertyName = "ReqTools")]
	public string ReqTools { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeLabor> Labor { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeAttr> Attributes { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeTask> Tasks { get; set; }

	// public List<ProcedureExternal> Procedures { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessTypeLabor
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string ProfileCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessTypeAttr
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string AttributeTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string AttributeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessTypeProcessExternal : ProcedureExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OperationTypeCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessTypeTask
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public int Sort { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	public string Description { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Mandatory")]
	public string Mandatory { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Yes|No", ErrorMessage = "Invalid Available")]
	public string Available { get; set; }

	/// <summary>
	///
	/// </summary>
	public double DurationInSec { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Production|Maintenance|Quality|General", ErrorMessage = "Invalid Class")]
	public string Class { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Start|During|End", ErrorMessage = "Invalid Stage")]
	public string Stage { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string ProcedureCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public int Version { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ProcessTypeProcessExternal> Procedures { get; set; }

	// public ProcessTypeProcessExternal Procedure { get; set; }
}

/// <summary>
///
/// </summary>
public class ProcessTypeNotification
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string OperationTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	[MaxLength(100)]
	public string NotificationCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string NotificationType { get; set; }
}
