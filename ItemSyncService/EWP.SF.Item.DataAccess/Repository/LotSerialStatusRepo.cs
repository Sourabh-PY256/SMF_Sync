
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

namespace EWP.SF.Item.DataAccess;

public class LotSerialStatusRepo : ILotSerialStatusRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public LotSerialStatusRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    /// <summary>
	///
	/// </summary>
	public List<LotSerialStatus> ListLotSerialStatus(string Code = "", DateTime? DeltaDate = null)
	{
		List<LotSerialStatus> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_LotSerial_Status_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				if (string.IsNullOrEmpty(Code))
				{
					command.Parameters.AddNull("_Code");
				}
				else
				{
					command.Parameters.AddWithValue("_Code", Code);
				}
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					LotSerialStatus element = new()
					{
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						CreateDate = rdr["CreateDate"].ToDate(),
						CreateUser = new User(rdr["CreateUser"].ToInt32()),
						Status = (Status)rdr["Status"].ToInt32(),
						Image = rdr["Image"].ToStr(),
						LogDetailId = rdr["LogDetailId"].ToStr()
					};

					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						element.UpdateDate = rdr["UpdateDate"].ToDate();
						element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
					}

					(returnValue ??= []).Add(element);
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
    /// <summary>
    ///
    /// </summary>
    public ResponseData MergeLotSerialStatus(LotSerialStatus LotSerialStatusInfo, User systemOperator, bool Validation)
    {
        ResponseData returnValue = null;
        using (EWP_Connection connection = new(ConnectionString))
        {
            try
            {
                using EWP_Command command = new("SP_SF_LotSerial_Status_INS", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Clear();

                command.Parameters.AddWithValue("_IsValidation", Validation);
                command.Parameters.AddWithValue("_Code", LotSerialStatusInfo.Code);
                command.Parameters.AddWithValue("_Name", LotSerialStatusInfo.Name);
                command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
                command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
                command.Parameters.AddWithValue("_Status", LotSerialStatusInfo.Status.ToInt32());
                command.Parameters.AddWithValue("_AllowIssue", LotSerialStatusInfo.AllowIssue);
                // command.Parameters.AddWithValue("_Level", Level);

                connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    returnValue = new ResponseData
                    {
                        Action = (ActionDB)rdr["Action"].ToInt32(),
                        IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
                        Code = rdr["Code"].ToStr(),
                        Message = rdr["Message"].ToStr(),
                    };
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