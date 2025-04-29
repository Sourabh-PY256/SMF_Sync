using EWP.SF.Common.Models;

namespace EWP.SF.Item.API;

internal class DecodeRouteMiddleware(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context)
	{
		// Decode route values
		foreach (string routeValue in context.Request.RouteValues.Keys.ToList())
		{
			if (context.Request.RouteValues[routeValue] is string encodedValue)
			{
				// Decode the value and update the route
				context.Request.RouteValues[routeValue] = Uri.UnescapeDataString(encodedValue);
			}
		}

		// Call the next middleware in the pipeline
		await next(context).ConfigureAwait(false);
	}
}
