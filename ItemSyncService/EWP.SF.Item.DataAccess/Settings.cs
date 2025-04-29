using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using EWP.SF.Common.Models;
using EWP.SF.Common.Enumerators;
using EWP.SF.ConnectionModule;
using EWP.SF.Helper;

using MySqlConnector;

using Newtonsoft.Json;

using NLog;

namespace EWP.SF.ShopFloor.DataAccess;

public partial class Settings
{
  private static readonly Logger logger = LogManager.GetCurrentClassLogger();
  private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");

  public string ConnectionString { get; set; }
  public string ConnectionStringLogs { get; set; }
  public string Database { get; set; }

  public Settings()
  {
    if (ApplicationSettings.Instance.GetConnectionString() is not null)
    {
      ConnectionString = ApplicationSettings.Instance.GetConnectionString();
      ConnectionStringLogs = ApplicationSettings.Instance.GetConnectionString("Logs");
      Database = ApplicationSettings.Instance.GetDatabaseFromConnectionString();
    }
  }

  public async Task<bool> SaveEntityLog(
    string logTable,
    string keyColumns,
    string keyValues,
    string logType,
    User user,
    string entity,
    string exception = "",
    CancellationToken cancellationToken = default)
  {
    await using EWP_Connection connection = new(ConnectionStringLogs);
    await using (connection.ConfigureAwait(false))
    {
      await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

      await using EWP_Command command = new("SP_SYS_EntityLog_INS", connection)
      {
        CommandType = CommandType.StoredProcedure
      };

      await using (command.ConfigureAwait(false))
      {
        command.Parameters.AddWithValue("_Database", Database);
        command.Parameters.AddWithValue("_LogTable", logTable);
        command.Parameters.AddWithValue("_KeyColumns", keyColumns);
        command.Parameters.AddWithValue("_KeyValues", keyValues);
        command.Parameters.AddWithValue("_LogType", logType);
        command.Parameters.AddWithValue("_LogUser", user?.Id);
        command.Parameters.AddWithValue("_LogEmployee", user?.EmployeeId);
        command.Parameters.AddWithValue("_Entity", entity);
        command.Parameters.AddWithValue("_Exception", exception);

        bool result;
        try
        {
          // Execute the stored procedure
          await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
          result = true;
        }
        catch (Exception ex)
        {
          // Log the exception if a logger is available
          logger?.Error(ex, "Error occurred while saving entity log.");
          throw;
        }

        return result;
      }
    }
  }
}
