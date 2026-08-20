using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.Common.Models;



/// <summary>
///
/// </summary>
[GridBDEntityName("ItemGroup")]
public class InventoryItemGroup : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_itemgroup_log");

	/// <summary>
	///
	/// </summary>
	public string InventoryId { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("ItemGroupCode")]
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	/// <summary>
	///
	/// </summary>
	public DateTime ModifyDate { get; set; }

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
	public InventoryItemGroup()
	{
	}

	/// <summary>
	///
	/// </summary>
	public InventoryItemGroup(string id)
	{
		InventoryId = id;
	}
}

/// <summary>
///
/// </summary>
public class InventoryGroupDetail
{
	/// <summary>
	///
	/// </summary>
	public string InventoryId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string InventoryName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentId { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentName { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentCode { get; set; }

	/// <summary>
	///
	/// </summary>
	public string ComponentImage { get; set; }

	/// <summary>
	///
	/// </summary>
	public double InStock { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Commited { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Required { get; set; }

	/// <summary>
	///
	/// </summary>
	public double Available { get; set; }
}

/// <summary>
///
/// </summary>
public class InventoryExternal
{
	/// <summary>
	///
	/// </summary>
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Item Group Code")]
	[JsonProperty(PropertyName = "ItemGroupCode")]
	public string ItemGroupCode { get; set; }

	/// <summary>
	///
	/// </summary>
	[MaxLength(200)]
	[Description("Item Group Name")]
	[JsonProperty(PropertyName = "ItemGroupName")]
	public string ItemGroupName { get; set; }

	/// <summary>
	///
	/// </summary>
	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Item Group Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }
}
