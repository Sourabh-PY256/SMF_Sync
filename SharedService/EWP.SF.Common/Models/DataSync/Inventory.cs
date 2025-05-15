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



[GridBDEntityName("ItemGroup")]
public class Inventory : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_itemgroup_log");
	public string InventoryId { get; set; }

	[EntityColumn("ItemGroupCode")]
	[GridDrillDown]
	public string Code { get; set; }

	public string Name { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	public List<string> AttachmentIds { get; set; }

	public string LogDetailId { get; set; }

	public Inventory()
	{
	}

	public Inventory(string id)
	{
		InventoryId = id;
	}
}

public class InventoryGroupDetail
{
	public string InventoryId { get; set; }
	public string InventoryName { get; set; }
	public string ComponentId { get; set; }
	public string ComponentName { get; set; }
	public string ComponentCode { get; set; }
	public string ComponentImage { get; set; }
	public double InStock { get; set; }
	public double Commited { get; set; }
	public double Required { get; set; }
	public double Available { get; set; }
}

public class InventoryExternal
{
	[Key]
	[Required]
	[MaxLength(100)]
	[Description("Item Group Code")]
	[JsonProperty(PropertyName = "ItemGroupCode")]
	public string ItemGroupCode { get; set; }

	[MaxLength(200)]
	[Description("Item Group Name")]
	[JsonProperty(PropertyName = "ItemGroupName")]
	public string ItemGroupName { get; set; }

	[RegularExpression("Active|Disable", ErrorMessage = "Invalid Status")]
	[Description("Item Group Status")]
	[JsonProperty(PropertyName = "Status")]
	public string Status { get; set; }
}
