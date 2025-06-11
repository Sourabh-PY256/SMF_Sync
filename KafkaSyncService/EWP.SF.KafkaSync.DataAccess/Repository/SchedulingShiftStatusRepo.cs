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

public class SchedulingShiftStatusRepo : ISchedulingShiftStatusRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public SchedulingShiftStatusRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
   #region SchedulingShiftStatus
	public List<SchedulingShiftStatus> GetSchedulingShiftStatus(string Code, string Type, DateTime? DeltaDate = null)
	{
		List<SchedulingShiftStatus> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SchedulingShiftStatus_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_Code", Code);
				_ = command.Parameters.AddWithValue("_Type", Type);
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					SchedulingShiftStatus element = new()
					{
						//Id = rdr["Id"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Efficiency = rdr["Efficiency"].ToDecimal(),
						Style = rdr["Style"].ToStr(),
						Color = rdr["Color"].ToStr(),
						Factor = rdr["Factor"].ToDecimal(),
						AllowSetup = rdr["AllowSetup"].ToBool(),
						Status = rdr["Status"].ToInt32(),
						Type = rdr["Type"].ToStr(),
						StatusName = rdr["StatusName"].ToStr(),
						CreationById = !string.IsNullOrEmpty(rdr["CreateUser"].ToStr()) ? rdr["CreateUser"].ToInt32() : null,
						CreationDate = !string.IsNullOrEmpty(rdr["CreateDate"].ToStr()) ? rdr["CreateDate"].ToDate() : null,
						ModifiedById = !string.IsNullOrEmpty(rdr["UpdateUser"].ToStr()) ? rdr["UpdateUser"].ToInt32() : null,
						ModifiedDate = !string.IsNullOrEmpty(rdr["UpdateDate"].ToStr()) ? rdr["UpdateDate"].ToDate() : null,
					};
					returnValue.Add(element);
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
	/// create / update catalog Scheduling Shift Status
	/// </summary>
	/// <param name="request">data catalog</param>
	/// <param name="systemOperator"></param>
	/// <param name="Validation"></param>
	/// <returns></returns>
	public ResponseData PutSchedulingShiftStatus(SchedulingShiftStatus request, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SchedulingShiftStatus_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_Code", request.Code);
				_ = command.Parameters.AddWithValue("_Name", request.Name);
				_ = command.Parameters.AddWithValue("_Efficiency", request.Efficiency.ToDecimal());
				_ = command.Parameters.AddWithValue("_Color", request.Color);
				_ = command.Parameters.AddWithValue("_Style", request.Style);
				_ = command.Parameters.AddWithValue("_Factor", request.Factor.ToDecimal());
				_ = command.Parameters.AddWithValue("_AllowSetup", request.AllowSetup.ToInt32());
				_ = command.Parameters.AddWithValue("_Status", request.Status.ToInt32());
				_ = command.Parameters.AddWithValue("_Type", request.Type);
				_ = command.Parameters.AddWithValue("_IsValidation", Validation);
				_ = command.Parameters.AddWithValue("_Operator", request.UserId.ToInt32());
				command.Parameters.AddCondition("_Employee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ResponseData
					{
						Id = rdr["Id"].ToStr(),
						Action = (ActionDB)rdr["Action"].ToInt32(),
						IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
						Code = rdr["Code"].ToStr(),
						Message = rdr["Message"].ToStr(),
					};
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
	#endregion SchedulingShiftStatus
}