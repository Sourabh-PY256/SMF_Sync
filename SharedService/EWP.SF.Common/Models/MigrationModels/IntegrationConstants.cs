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
public static class IntegrationConstants
{
	/// <summary>
	///
	/// </summary>
	public static string EVENT_CODE_PRINT_LABEL = "600";

	/// <summary>
	///
	/// </summary>
	public static string EVENT_CODE_ISSUE_MATERIAL = "601";

	/// <summary>
	///
	/// </summary>
	public static string EVENT_CODE_RECEIVE_PRODUCT = "602";
}