using System.Data;
using System.Globalization;
using EWP.SF.Helper;
using EWP.SF.ConnectionModule;

using EWP.SF.Common.Models;
using NLog;

namespace EWP.SF.KafkaSync.DataAccess;

public class ClockInOutRepo : IClockInOutRepo
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private readonly string ConnectionString;

    public ClockInOutRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
    }

    /// <summary>
    /// Bulk-merges clock-in/clock-out records via SP_SF_CheckInOut_BLK, matching the 2503 monolith's Broker.WorkOrder.MergeClockInOutBulk.
    /// </summary>
    public void MergeClockInOutBulk(string Json, User systemOperator, bool Validate)
    {
        using EWP_Connection connection = new(ConnectionString);
        try
        {
            using EWP_Command command = new("SP_SF_CheckInOut_BLK", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Clear();
            command.Parameters.AddCondition("_JSON", Json, !string.IsNullOrEmpty(Json));
            command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, "Parameter \"{0}\" is required and was not provided.", "User"));
            command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

            connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error executing SP_SF_CheckInOut_BLK");
            throw;
        }
    }
}
