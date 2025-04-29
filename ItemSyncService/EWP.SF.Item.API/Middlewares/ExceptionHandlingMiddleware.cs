

using Newtonsoft.Json;

using NLog;

using EWP.SF.Common.Models;

namespace EWP.SF.Item.API;

internal class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            string exCode = Guid.NewGuid().ToString();

            if (context.Items["Context"] is RequestContext requestContext)
            {
                _ = ScopeContext.PushProperty("exception_id", exCode);
                _ = ScopeContext.PushProperty("user_id", requestContext.User?.Id);
                _ = ScopeContext.PushProperty("employee_id", requestContext.EmployeeCode);
            }

            logger.LogError(exception.Message, exception);

            ResponseModel problemDetails = new()
            {
                IsSuccess = false,
                Message = exception.Message,
                ErrorCode = exCode
            };

            context.Response.StatusCode = StatusCodes.Status200OK;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails)).ConfigureAwait(false);
        }
    }
}
