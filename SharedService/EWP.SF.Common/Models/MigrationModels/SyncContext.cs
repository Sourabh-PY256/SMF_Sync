using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Attributes;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Enumerators;
using Newtonsoft.Json;

namespace EWP.SF.Common.Models.MigrationModels;

/// <summary>
///
/// </summary>
public enum MigrationType
{
	/// <summary>
	///
	/// </summary>
	Material = 1,
	/// <summary>
	///
	/// </summary>
	Product = 2,
	/// <summary>
	///
	/// </summary>
	Order = 3,
	/// <summary>
	///
	/// </summary>
	Stock = 4,
	/// <summary>
	///
	/// </summary>
	Quality = 5,
	/// <summary>
	///
	/// </summary>
	Catalog = 6,
	/// <summary>
	///
	/// </summary>
	Data = 7,
	/// <summary>
	///
	/// </summary>
	Demand = 8,
	/// <summary>
	///
	/// </summary>
	Supply = 9
}

/// <summary>
///
/// </summary>
public class SyncContext
{
	/// <summary>
	///
	/// </summary>
	public MigrationType Type { get; set; }

	/// <summary>
	///
	/// </summary>
	public string EntityId { get; set; }

	/// <summary>
	///
	/// </summary>
	public SyncContext()
	{
	}

	/// <summary>
	///
	/// </summary>
	public SyncContext(MigrationType type, string id)
	{
		Type = type;
		EntityId = id;
	}
}
