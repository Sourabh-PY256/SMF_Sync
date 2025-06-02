using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.CustomBehavior;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models.Sensors;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
[GridBDEntityName("ToolingType")]
public class ToolType : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_toolingtype_log");

	/// <summary>
	///
	/// </summary>
	public ToolType()
	{
	}

	/// <summary>
	///
	/// </summary>
	public ToolType(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridDisabledHiding]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridDrillDown("OperationType")]
	public string ProcessTypeId { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("OperationTypeName")]
	public string OperationType { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ToolingTypeCode")]
	[GridRequireDecode]
	[GridDrillDown]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDisabledHiding]
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
	public List<string> Codes { get; set; }

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
	public DateTime ModifyDate { get; set; }

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
	public ToolType_Scheduling Scheduling { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string SchedulingToJSON()
	{
		string returnValue = string.Empty;
		if (Scheduling is not null)
		{
			returnValue = JsonConvert.SerializeObject(Scheduling);
		}
		return "[" + returnValue + "]";
	}
}

/// <summary>
///
/// </summary>
public class ToolType_Scheduling
{
	/// <summary>
	///
	/// </summary>
	public string ToolTypeId { get; set; }

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
	public int? Attribute3 { get; set; }

	/// <summary>
	///
	/// </summary>
	public short? ScheduleLevel { get; set; }

	/// <summary>
	///
	/// </summary>
	public bool Schedule { get; set; }
}

/// <summary>
///
/// </summary>
[GridBDEntityName("Tooling")]
public class Tool : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_tooling_log");

	/// <summary>
	///
	/// </summary>
	public Tool()
	{
	}

	/// <summary>
	///
	/// </summary>
	public Tool(string id)
	{
		Id = id;
	}

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomPropertyName("ToolTypeCode")]
	[GridDrillDown("ToolingType", "Code")]
	[GridRequireDecode]
	public string ToolingTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ToolingCode")]
	[GridDrillDown]
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridRequireDecode]
	[GridDisabledHiding]
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.ICON_CLASS, "Icon")]
	public string Icon { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<ToolDetail> Details { get; set; }

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
	public DateTime ModifyDate { get; set; }

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
	public List<ToolMachineAllowed> MachinesAllowed { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }

	/// <summary>
	///
	/// </summary>
	public string machinesAllowedToJSON()
	{
		string returnValue = string.Empty;
		if (MachinesAllowed is not null)
		{
			returnValue = JsonConvert.SerializeObject(MachinesAllowed);
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public decimal CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public string CompatibleMachines { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToolTypestring { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<Activity> Activities { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts Shift { get; set; }

	/// <summary>
	///
	/// </summary>
	public SchedulingCalendarShifts ShiftDelete { get; set; }
}

/// <summary>
///
/// </summary>
public class ToolDetail
{
	/// <summary>
	///
	/// </summary>
	public string ToolingCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public ToolParamType Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Source { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StandardValue { get; set; }
}

/// <summary>
///
/// </summary>
public class ToolMachineAllowed
{
	/// <summary>
	///
	/// </summary>
	public string ToolId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string MachineId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ProcessTypeId { get; set; }
}

/// <summary>
///
/// </summary>
public class ToolExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public string ToolingCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToolingName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ToolType { get; set; }

	// public decimal CostPerHour { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<CompatibleMachine> CompatibleMachines { get; set; }
}

/// <summary>
///
/// </summary>
public class CompatibleMachine
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	public string ToolingCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[Required]
	public string MachineCode { get; set; }
}

/// <summary>
///
/// </summary>
public class ToolTypeExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	public string ToolingTypeCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(500)]
	public string ToolingTypeName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	public string Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(100)]
	public string OperationType { get; set; }

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
