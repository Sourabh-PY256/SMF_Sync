using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.DataAccess;

public class WarehouseRepo : IWarehouseRepo
{
	private readonly string ConnectionString;
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
	private readonly string ConnectionStringReports;
	private readonly string ConnectionStringLogs;

	private readonly string Database;

	public WarehouseRepo(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
		ConnectionStringReports = applicationSettings.GetReportsConnectionString();
		ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
		Database = applicationSettings.GetDatabaseFromConnectionString();
	}

	#region WarehouseRepo
	public Warehouse GetWarehouse(string Code)
	{
		Warehouse returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Warehouse_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Code"));
				command.Parameters.AddNull("_DeltaDate");
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new Warehouse
					{
						WarehouseId = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						Image = rdr["Image"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						Schedule = rdr["EnableSchedule"].ToBool(),
						FacilityCode = rdr["FacilityCode"].ToStr(),
						BinLocationCode = rdr["BinLocationCode"].ToStr(),
						IsProduction = rdr["IsProduction"].ToBool(),
						LogDetailId = rdr["LogDetailId"].ToStr()
					};

					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						returnValue.ModifyDate = rdr["UpdateDate"].ToDate();
						returnValue.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
					}
				}

				if (rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult() && returnValue is not null)
				{
					returnValue.Details = [];
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						BinLocation element = new()
						{
							WarehouseId = rdr["WarehouseCode"].ToStr(),
							LocationCode = rdr["LocationCode"].ToStr(),
							InventoryStatusCodes = [],
							Name = rdr["Name"].ToStr(),
							Status = (Status)rdr["Status"].ToInt32()
						};
						returnValue.Details.Add(element);
					}
				}

				if (rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult() && returnValue is not null)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						BinLocation location = returnValue.Details.FirstOrDefault(x => x.LocationCode == rdr["BinLocationCode"].ToStr());
						location?.InventoryStatusCodes.Add(rdr["InventoryStatusCode"].ToStr());
					}
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
	public ResponseData MergeWarehouse(Warehouse WarehouseInfo, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Warehouse_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_Code", WarehouseInfo.Code);
				_ = command.Parameters.AddWithValue("_Name", WarehouseInfo.Name);
				_ = command.Parameters.AddWithValue("_Status", WarehouseInfo.Status.ToInt32());
				command.Parameters.AddCondition("_User", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				// command.Parameters.AddCondition("_DataJSON", () => { return WarehouseInfo.detailToJSON(); }, WarehouseInfo.Details is not null && WarehouseInfo.Details.Count > 0);
				_ = command.Parameters.AddWithValue("_IsValidation", Validation);
				_ = command.Parameters.AddWithValue("_Schedule", WarehouseInfo.Schedule.ToInt32());
				_ = command.Parameters.AddWithValue("_FacilityCode", WarehouseInfo.FacilityCode);
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				_ = command.Parameters.AddWithValue("_BinLocationCode", WarehouseInfo.BinLocationCode);
				_ = command.Parameters.AddWithValue("_IsProduction", WarehouseInfo.IsProduction);
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
	/// <summary>
	///
	/// </summary>
	public List<Warehouse> ListWarehouse(string Code = "", DateTime? DeltaDate = null)
	{
		List<Warehouse> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Warehouse_SEL", connection)
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
					Warehouse element = new()
					{
						Id = rdr["Code"].ToStr(),
						WarehouseId = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						Image = rdr["Image"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						Schedule = rdr["EnableSchedule"].ToBool(),
						FacilityCode = rdr["FacilityCode"].ToStr(),
						BinLocationCode = rdr["BinLocationCode"].ToStr(),
						IsProduction = rdr["IsProduction"].ToBool(),
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

    #endregion WarehouseRepo
}