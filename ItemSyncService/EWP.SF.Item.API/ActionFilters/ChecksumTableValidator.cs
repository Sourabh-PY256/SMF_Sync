using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Item.BusinessLayer;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EWP.SF.ShopFloor.API;

internal sealed class ChecksumTableAttribute : ActionFilterAttribute
{
	private readonly string _tableName;
	private ISystemSettingsService? _systemSettingsService;

	public string TableName => _tableName;

	public ChecksumTableAttribute(string tableName)
	{
		_tableName = tableName;
	}

	// Runs before the action method is invoked
	public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		// Resolve the service from the request services
		_systemSettingsService = context.HttpContext.RequestServices.GetRequiredService<ISystemSettingsService>();

		// Perform checksum validation and potentially exit early
		string checksum = await _systemSettingsService.GetTableChecksum(TableName).ConfigureAwait(false);
		DateTime requestDate = await _systemSettingsService.GetServerTimeUTC().ConfigureAwait(false);
		context.HttpContext.Items["Checksum"] = checksum;
		context.HttpContext.Items["RequestedDate"] = requestDate;

		// Check if an incoming checksum is provided
		string? incomingChecksum = context.HttpContext.Request.Headers["Checksum"];

		if (!string.IsNullOrEmpty(incomingChecksum) && incomingChecksum.Equals(checksum, StringComparison.OrdinalIgnoreCase))
		{
			// Checksum is valid, exit early without calling the action method
			context.Result = new OkObjectResult(new ResponseModel
			{
				Checksum = checksum,
				RequestDate = requestDate,
				Message = "No changes detected, exiting early."
			});
			return; // Exit early, the action will not be executed
		}

		// Continue to the action method if checksum is not valid
		_ = await next().ConfigureAwait(false);
	}

	// Runs after the action method is invoked but before the result is sent to the client
	public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
	{
		// Retrieve the checksum and request date from the context
		string? checksum = context.HttpContext.Items["Checksum"]?.ToString();
		DateTime? requestedDate = context.HttpContext.Items["RequestedDate"] as DateTime?;

		if (checksum is not null && requestedDate is not null && context.Result is ObjectResult result)
		{
			// If it's a Task (asynchronous), await it and access the result
			// Handle both synchronous and asynchronous responses
			if (result.Value is Task task)
			{
				await task.ConfigureAwait(false);

				Type responseType = task.GetType().GetGenericArguments()[0];
				if (responseType == typeof(ResponseModel))
				{
					ResponseModel response = await ((Task<ResponseModel>)task).ConfigureAwait(false);
					SetChecksumAndDate(response, checksum, (DateTime)requestedDate);
				}
			}
			else if (result.Value is ResponseModel response) // If it's a synchronous ResponseModel, process it directly

			{
				SetChecksumAndDate(response, checksum, (DateTime)requestedDate);
			}
		}

		// Proceed with the result execution
		_ = await next().ConfigureAwait(false);
	}

	private static void SetChecksumAndDate(ResponseModel response, string checksum, DateTime requestedDate)
	{
		response.Checksum = checksum;
		response.RequestDate = requestedDate;
	}
}
