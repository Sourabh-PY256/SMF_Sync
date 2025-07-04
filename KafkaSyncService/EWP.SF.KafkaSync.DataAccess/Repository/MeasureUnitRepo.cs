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

public class MeasureUnitRepo : IMeasureUnitRepo
{
	private readonly string ConnectionString;
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
	private readonly string ConnectionStringReports;
	private readonly string ConnectionStringLogs;

	private readonly string Database;

	public MeasureUnitRepo(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
		ConnectionStringReports = applicationSettings.GetReportsConnectionString();
		ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
		Database = applicationSettings.GetDatabaseFromConnectionString();
	}
	#region MeasureUnit
	/// <summary>
	///
	/// </summary>
	public ResponseData MergeUnitMeasure(MeasureUnit measureUnitInfo, User systemOperator, bool Validation)
	{
		ArgumentNullException.ThrowIfNull(measureUnitInfo);
		ArgumentNullException.ThrowIfNull(systemOperator);

		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Unit_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddCondition("_UnitTypeId", (int)measureUnitInfo.Type, (int)measureUnitInfo.Type > 0);
				command.Parameters.AddWithValue("_Code", measureUnitInfo.Code);
				command.Parameters.AddWithValue("_Name", measureUnitInfo.Name);
				command.Parameters.AddWithValue("_Factor", measureUnitInfo.Factor);
				command.Parameters.AddWithValue("_Status", measureUnitInfo.Status.ToInt32());
				command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddWithValue("_IsProductionResult", measureUnitInfo.IsProductionResult.ToInt32());
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ResponseData
					{
						Action = (ActionDB)rdr["Action"].ToInt32(),
						Id = rdr["Id"].ToStr(),
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
	/// <summary>
	///
	/// </summary>
	public List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null)
	{
		List<MeasureUnit> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Unit_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_UnitType", () => unitType.Value.ToInt32(), unitType.HasValue);
				command.Parameters.AddCondition("_UnitId", unitId, !string.IsNullOrEmpty(unitId));
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					MeasureUnit element = new()
					{
						Id = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Type = (UnitType)rdr["UnitTypeId"].ToInt32(),
						TypeName = rdr["UnitType"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Factor = rdr["Factor"].ToDecimal(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						Status = (Status)rdr["Status"].ToInt32(),
						IsProductionResult = rdr["IsProductionResult"].ToBool(),
						StatusMessage = rdr["StatusMessage"].ToStr(),
						Image = rdr["Image"].ToStr(),
						LogDetailId = rdr["LogDetailId"].ToStr()
					};
					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						element.ModifyDate = rdr["UpdateDate"].ToDate();
						element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
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

    #endregion MeasureUnit
}