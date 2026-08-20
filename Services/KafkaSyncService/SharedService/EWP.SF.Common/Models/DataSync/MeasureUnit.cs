using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using EWP.SF.Common.Attributes;
using EWP.SF.Common.Constants;
using EWP.SF.Common.Enumerators;

using Newtonsoft.Json;
using EWP.SF.Common.EntityLogger;


namespace EWP.SF.Common.Models;

/// <summary>
///
/// </summary>
public class MeasureUnit : ILoggableEntity
{
	/// <summary>
	///
	/// </summary>
	public EntityLoggerConfig EntityLogConfiguration => new("sf_unit_log");

	/// <summary>
	///
	/// </summary>
	[GridDrillDown]
	public string Id { get; set; }

	/// <summary>
	///
	/// </summary>
	public UnitType Type { get; set; }

	/// <summary>
	///
	/// </summary>
	[EntityColumn("Unit")]
	[GridDrillDown]
	public string Code { get; set; }

	/// <summary>
	///
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///
	/// </summary>
	public decimal Factor { get; set; }

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
	public bool IsProductionResult { get; set; }

	/// <summary>
	///
	/// </summary>
	public string StatusMessage { get; set; }

	/// <summary>
	///
	/// </summary>
	public string TypeName { get; set; }

	/// <summary>
	///
	/// </summary>
	[GridCustomType(GridColumnType.IMAGE_ROUTE, "Image")]
	public string Image { get; set; }

	/// <summary>
	///
	/// </summary>
	public string LogDetailId { get; set; }

	/// <summary>
	///
	/// </summary>
	public List<string> AttachmentIds { get; set; }
}
