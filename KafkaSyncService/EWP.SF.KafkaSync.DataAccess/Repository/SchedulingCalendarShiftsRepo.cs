using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.DataAccess;

public class SchedulingCalendarShiftsRepo : ISchedulingCalendarShiftsRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public SchedulingCalendarShiftsRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region SchedulingCalendarShifts
	/// <summary>
	/// create / update catalog Scheduling Calendar Shifts
	/// </summary>
	/// <param name="request">data catalog</param>
	/// <param name="systemOperator"></param>
	/// <returns></returns>
	public SchedulingCalendarShifts PutSchedulingCalendarShifts(SchedulingCalendarShifts request, User systemOperator)
	{
		SchedulingCalendarShifts returnValue = request;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SchedulingCalendarShifts_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_Id", request.Id);
				_ = command.Parameters.AddWithValue("_CodeShift", request.CodeShift);
				_ = command.Parameters.AddWithValue("_IdAsset", request.IdAsset);
				_ = command.Parameters.AddWithValue("_AssetLevel", request.AssetLevel);
				_ = command.Parameters.AddWithValue("_AssetLevelCode", request.AssetLevelCode);
				_ = command.Parameters.AddWithValue("_FromDate", request.FromDate);
				_ = command.Parameters.AddWithValue("_IsParent", request.IsParent);
				_ = command.Parameters.AddWithValue("_IdParent", request.IdParent);
				_ = command.Parameters.AddWithValue("_Status", request.Status.ToInt32());
				_ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
				_ = command.Parameters.AddWithValue("_OperatorEmployee", systemOperator.EmployeeId);
				_ = command.Parameters.AddWithValue("_Origin", request.Origin);
				_ = command.Parameters.AddWithValue("_Validation", request.Validation);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue.Id = rdr["Id"].ToStr();
					returnValue.IdParent = rdr["IdParent"].ToStr();
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
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
	/// delete catalog Scheduling Calendar Shifts
	/// </summary>
	/// <param name="request">data catalog</param>
	/// <returns></returns>
	public bool DeleteSchedulingCalendarShifts(SchedulingCalendarShifts request)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SchedulingCalendarShifts_DEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				//command.Parameters.AddWithValue("_Id", request.Id);
				_ = command.Parameters.AddWithValue("_Id", request.Id);
				_ = command.Parameters.AddWithValue("_Operator", request.UserId.ToInt32());
				_ = command.Parameters.AddWithValue("_Origin", request.Origin);
				_ = command.Parameters.AddWithValue("_CodeOrigin", request.CodeOrigin);
				_ = command.Parameters.AddWithValue("_Validation", request.Validation);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				_ = command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = true;
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
	/// get list catalog Scheduling Calendar Shifts
	/// </summary>
	/// <param name="Id">key value to search</param>
	/// <param name="AssetCode"></param>
	/// <param name="IdParent"></param>
	/// <param name="AssetLevel"></param>
	/// <param name="AssetLevelCode"></param>
	/// <param name="Origin"></param>
	/// <returns></returns>
	public List<SchedulingCalendarShifts> GetSchedulingCalendarShifts(string Id, string AssetCode, string IdParent, int AssetLevel, string AssetLevelCode, string Origin = null)
	{
		List<SchedulingCalendarShifts> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SchedulingCalendarShifts_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_Id", Id);
				//command.Parameters.AddWithValue("_CodeShift", CodeShift);
				_ = command.Parameters.AddWithValue("_AssetCode", AssetCode);
				_ = command.Parameters.AddWithValue("_IdParent", IdParent);
				_ = command.Parameters.AddWithValue("_AssetLevel", AssetLevel);
				_ = command.Parameters.AddWithValue("_AssetLevelCode", AssetLevelCode);
				_ = command.Parameters.AddWithValue("_Origin", Origin);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					SchedulingCalendarShifts element = new()
					{
						Id = rdr["Id"].ToStr(),
						CodeShift = rdr["ShiftCode"].ToStr(),
						IdAsset = rdr["AssetCode"].ToStr(),
						AssetLevel = rdr["AssetLevel"].ToInt32(),
						AssetLevelCode = rdr["AssetLevelCode"].ToStr(),
						FromDate = rdr["FromDate"].ToDate(),
						ToDate = string.IsNullOrEmpty(rdr["ToDateParse"].ToStr()) ? null : rdr["ToDateParse"].ToDate(),
						Color = rdr["Color"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = rdr["Status"].ToInt32(),
						IdParent = rdr["ParentId"].ToStr(),
						IsParent = rdr["IsParent"].ToBool()
					};
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

	#endregion SchedulingCalendarShifts
}