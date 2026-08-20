
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

public class ToolRepo : IToolRepo
{
	private readonly string ConnectionString;
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
	private readonly string ConnectionStringReports;
	private readonly string ConnectionStringLogs;

	private readonly string Database;

	public ToolRepo(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
		ConnectionStringReports = applicationSettings.GetReportsConnectionString();
		ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
		Database = applicationSettings.GetDatabaseFromConnectionString();
	}
	#region Tool
	public List<ToolType> ListToolType(string ToolTypeCode, DateTime? DeltaDate = null)
	{
		List<ToolType> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ToolingType_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_ToolingTypeCode", ToolTypeCode);
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ToolType element = new()
					{
						Id = rdr["Code"].ToStr(),
						ProcessTypeId = rdr["OperationTypeCode"].ToStr(),
						OperationType = rdr["OperationTypeName"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Icon = rdr["Icon"].ToStr(),
						Image = rdr["Image"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						Status = (Status)rdr["Status"].ToInt32(),
						Codes = [],
						LogDetailId = rdr["LogDetailId"].ToStr()
						//Scheduling = new ToolType_Scheduling(),
					};

					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						element.ModifyDate = rdr["UpdateDate"].ToDate();
						element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
					}

					(returnValue ??= []).Add(element);
				}

				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					string id = rdr["ToolingTypeCode"].ToStr();
					ToolType element = returnValue.Find(x => x.Id == id);
					element?.Codes.Add(rdr["ParameterCode"].ToStr());
				}

				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ToolType element = returnValue.Find(x => x.Id == rdr["ToolingTypeCode"].ToStr());
					if (element is not null)
					{
						element.Scheduling = new ToolType_Scheduling
						{
							Attribute1 = !string.IsNullOrEmpty(rdr["Attribute1"].ToStr()) ? rdr["Attribute1"].ToStr() : null,
							Attribute2 = !string.IsNullOrEmpty(rdr["Attribute2"].ToStr()) ? rdr["Attribute2"].ToDecimal() : null,
							Attribute3 = !string.IsNullOrEmpty(rdr["Attribute3"].ToStr()) ? rdr["Attribute3"].ToInt32() : null,
							ScheduleLevel = !string.IsNullOrEmpty(rdr["ScheduleLevel"].ToStr()) ? Convert.ToInt16(rdr["ScheduleLevel"]) : null,
							Schedule = rdr["Schedule"].ToBool()
						};
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
	/// <summary>
	///
	/// </summary>
	public ResponseData CreateToolType(ToolType toolTypeInfo, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ToolingType_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				toolTypeInfo.Scheduling ??= new ToolType_Scheduling();
				command.Parameters.AddWithValue("_Code", toolTypeInfo.Code);
				command.Parameters.AddWithValue("_Name", toolTypeInfo.Name);
				command.Parameters.AddWithValue("_Icon", toolTypeInfo.Icon);
				command.Parameters.AddWithValue("_Image", toolTypeInfo.Image);
				command.Parameters.AddWithValue("_Status", toolTypeInfo.Status.ToInt32());
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_Detail", () => string.Join(",", toolTypeInfo.Codes), toolTypeInfo.Codes is not null);
				command.Parameters.AddWithValue("_Attribute1", toolTypeInfo.Scheduling.Attribute1);
				command.Parameters.AddWithValue("_Attribute2", toolTypeInfo.Scheduling.Attribute2);
				command.Parameters.AddWithValue("_Attribute3", toolTypeInfo.Scheduling.Attribute3);
				command.Parameters.AddWithValue("_ScheduleLevel", toolTypeInfo.Scheduling.ScheduleLevel);
				command.Parameters.AddWithValue("_Schedule", toolTypeInfo.Scheduling.Schedule);
				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddWithValue("_OperationType", toolTypeInfo.ProcessTypeId);
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

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
	public List<Tool> ListTools(string ToolCode, DateTime? DeltaDate = null)
	{
		List<Tool> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Tooling_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_ToolingCode", ToolCode);
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Tool element = new()
					{
						Id = rdr["Code"].ToStr(),
						ToolingTypeCode = rdr["ToolingTypeCode"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Icon = rdr["Icon"].ToStr(),
						Image = rdr["Image"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						Status = (Status)rdr["Status"].ToInt32(),

						Details = []
					};

					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						element.ModifyDate = rdr["UpdateDate"].ToDate();
						element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
					}

					(returnValue ??= []).Add(element);
				}

				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					string ToolingCode = rdr["ToolingCode"].ToStr();
					Tool element = returnValue.Find(x => x.Code == ToolingCode);
					if (element is not null)
					{
						ToolDetail detail = new()
						{
							ProcessCode = rdr["ParameterCode"].ToStr(),
							Type = (ToolParamType)rdr["ParamType"].ToInt32(),
							Source = rdr["ParamSource"].ToStr(),
							Value = rdr["ParamValue"].ToStr(),
							StandardValue = rdr["StandardValue"].ToStr()
						};
						element.Details.Add(detail);
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

    #endregion Tool
}   