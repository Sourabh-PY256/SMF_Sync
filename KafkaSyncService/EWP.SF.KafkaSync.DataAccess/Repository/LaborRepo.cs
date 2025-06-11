using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.DataAccess;

public class LaborRepo : ILaborRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public LaborRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    /// <summary>
	///
	/// </summary>
	public List<Labor> ListLabors()
    {
        List<Labor> returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_Labor_SEL", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    Labor element = new()
                    {
                        Id = rdr["PositionCode"].ToStr(),
                        Name = rdr["Name"].ToStr(),
                        LaborTime = rdr["LaborTimeInSec"].ToStr(),
                        Cost = rdr["Cost"].ToDouble(),
                        CreationDate = rdr["CreateDate"].ToDate(),
                        CreatedBy = new User(rdr["CreateUser"].ToInt32()),
                        Status = (Status)rdr["Status"].ToInt32()
                    };
                    if (returnValue.IsNull())
                    {
                        returnValue = [];
                    }
                    returnValue.Add(element);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        return returnValue;
    }
}