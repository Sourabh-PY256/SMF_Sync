using System.Globalization;


using Microsoft.AspNetCore.Mvc;

using NLog;
using EWP.SF.Common.Models;

namespace EWP.SF.Item.API;

public class BaseController : ControllerBase
{
	private readonly CultureInfo enUS = new("en-US");

	public DateTime? DeltaDate
	{
		get
		{
			string dateHeader = GetHeader("DeltaDate");
			if (!string.IsNullOrEmpty(dateHeader))
			{
				if (dateHeader.Length > 19)
				{
					dateHeader = dateHeader[..19];
				}
				if (DateTime.TryParseExact(dateHeader, "yyyy-MM-ddTHH:mm:ss", enUS, DateTimeStyles.AdjustToUniversal, out DateTime tempValue))
				{
					return tempValue;
				}
			}
			return null;
		}
	}

	internal string Checksum
	{
		get
		{
			return HttpContext.Items["Checksum"] is not null ? HttpContext.Items["Checksum"].ToString() : string.Empty;
		}
	}

	internal RequestContext GetContext()
	{
		//Se asigna el valor del usuario que realiza la petición.
		if (HttpContext.Items["Context"] is RequestContext requestContext)
		{
			_ = ScopeContext.PushProperty("user_id", requestContext.User?.Id);
			_ = ScopeContext.PushProperty("employee_id", requestContext.EmployeeCode);
		}

		RequestContext returnValue = (RequestContext)HttpContext.Items["Context"];
		if (returnValue?.User is not null)
		{
			returnValue.User.TimeZoneOffset = returnValue.TimeZoneOffset;
		}

		return returnValue;
	}

	internal string GetHeader(string key)
	{
		return HttpContext.Request.Headers.TryGetValue(key, out Microsoft.Extensions.Primitives.StringValues value) ? value.ToString() : null;
	}

	internal Dictionary<string, string> GetHeaders()
	{
		return HttpContext.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString());
	}
}
