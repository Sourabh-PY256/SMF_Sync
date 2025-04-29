using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Common.Models;
using EWP.SF.Helper;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EWP.SF.ShopFloor.BusinessLayer;

[JsonConverter(typeof(StringEnumConverter))]
internal enum EntityLogType
{
	Create = 1,
	Update = 2,
	Delete = 3
}

internal static class EntityLogger
{
	private static DataAccess.Settings settings;

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
	public static async Task Log(this ILoggableEntity obj, EntityLogType type, User systemOperator)
	{
		if (obj is null)
		{
			return;
		}

		ILoggableEntity objToSend = obj;

		settings ??= new DataAccess.Settings();
		if (objToSend is not null)
		{
			string logTable = string.Empty;
			List<string> keyColumns = [];
			List<string> keyValues = [];
			string entity = JsonConvert.SerializeObject(objToSend, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			try
			{
				logTable = objToSend.EntityLogConfiguration.LogTable;
				if (string.IsNullOrEmpty(logTable))
				{
					logTable = objToSend.GetType().Name;
					throw new Exception("No LogTable is defined in Model");
				}

				Dictionary<string, string> columnMappings = obj.GetType().GetProperties()
				   .Where(x => x.GetCustomAttribute<EntityColumnAttribute>() is not null)
				   .ToDictionary(
					   x => x.Name,
					   x => x.GetCustomAttribute<EntityColumnAttribute>().ColumnName
				   );

				foreach (string key in columnMappings.Keys)
				{
					keyColumns.Add(columnMappings[key]);
					object value = obj.GetType().GetProperty(key)?.GetValue(objToSend);
					switch (value.GetType().Name)
					{
						case "DateTime":
							{
								keyValues.Add(string.Format("'{0}'", ((DateTime)value).ToString("yyyy/MM/dd HH:mm:ss.fff")));
								break;
							}
						case "Boolean":
							{
								keyValues.Add(string.Format("'{0}'", value.ToInt32().ToString()));
								break;
							}
						default:
							{
								if (value.GetType().BaseType.Name == "Enum")
								{
									keyValues.Add(string.Format("'{0}'", value.ToInt32().ToStr()));
								}
								else
								{
									keyValues.Add(string.Format("'{0}'", value.ToString()));
								}
								break;
							}
					}
				}
				await settings.SaveEntityLog(
					logTable,
					string.Join(',', [.. keyColumns]),
					string.Join(',', [.. keyValues]),
					type.ToString(),
					systemOperator,
					entity).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await settings.SaveEntityLog(
					logTable,
					string.Join(',', [.. keyColumns]),
					string.Join(',', [.. keyValues]),
					type.ToString(),
					systemOperator,
					entity,
					ex.Message).ConfigureAwait(false);
			}
		}
	}
}
