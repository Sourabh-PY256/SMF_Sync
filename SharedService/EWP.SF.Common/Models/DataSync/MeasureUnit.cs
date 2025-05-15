using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.Constants;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json;
using EWP.SF.Common.EntityLogger;


namespace EWP.SF.Common.Models;

public class MeasureUnit : ILoggableEntity
{
	public EntityLoggerConfig EntityLogConfiguration => new("sf_unit_log");

	[GridDrillDown]
	public string Id { get; set; }

	public UnitType Type { get; set; }

	[EntityColumn("Unit")]
	[GridDrillDown]
	public string Code { get; set; }

	public string Name { get; set; }
	public decimal Factor { get; set; }
	public DateTime CreationDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User CreatedBy { get; set; }

	public DateTime ModifyDate { get; set; }

	[GridLookUpEntity("Id", "User", "Id", "DisplayName")]
	public User ModifiedBy { get; set; }

	[GridLookUpEntity(null, "Status", "Id", "Name")]
	[GridRequireTranslate]
	public Status Status { get; set; }

	public bool IsProductionResult { get; set; }
	public string StatusMessage { get; set; }
	public string TypeName { get; set; }

	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	public string LogDetailId { get; set; }
	public List<string> AttachmentIds { get; set; }
}
