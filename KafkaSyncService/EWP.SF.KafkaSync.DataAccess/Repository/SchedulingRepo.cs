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
using EWP.SF.Common.Models.IntegrationStaging;

namespace EWP.SF.KafkaSync.DataAccess;
public class SchedulingRepo : ISchedulingRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public SchedulingRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
	#region SchedulingShiftStatus

	/// <summary>
	/// get list catalog Scheduling Shift Status
	/// </summary>
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

				command.Parameters.AddWithValue("_Code", Code);
				command.Parameters.AddWithValue("_Type", Type);
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

				command.Parameters.AddWithValue("_Code", request.Code);
				command.Parameters.AddWithValue("_Name", request.Name);
				command.Parameters.AddWithValue("_Efficiency", request.Efficiency.ToDecimal());
				command.Parameters.AddWithValue("_Color", request.Color);
				command.Parameters.AddWithValue("_Style", request.Style);
				command.Parameters.AddWithValue("_Factor", request.Factor.ToDecimal());
				command.Parameters.AddWithValue("_AllowSetup", request.AllowSetup.ToInt32());
				command.Parameters.AddWithValue("_Status", request.Status.ToInt32());
				command.Parameters.AddWithValue("_Type", request.Type);
				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddWithValue("_Operator", request.UserId.ToInt32());
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

	#region SchedulingShift

	/// <summary>
	/// get list Scheduling Shift
	/// </summary>
	public List<SchedulingShift> GetSchedulingShift(string Code, string Type, DateTime? DeltaDate = null)
	{
		List<SchedulingShift> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SchedulingShift_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_Code", Code);
				command.Parameters.AddWithValue("_Type", Type);
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					SchedulingShift element = new()
					{
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Duration = rdr["DurationInSec"].ToInt64(),
						//Color = rdr["Color"].ToStr(),
						ReferenceDate = string.IsNullOrEmpty(rdr["ReferenceDateParse"].ToStr()) ? null : rdr["ReferenceDateParse"].ToDate(),
						Status = rdr["Status"].ToInt32(),
						IsCheckInOut = rdr["IsCheckInOut"].ToBool(),
						Type = rdr["Type"].ToStr(),
						StatusName = rdr["StatusName"].ToStr(),
						CreationById = !string.IsNullOrEmpty(rdr["CreateUser"].ToStr()) ? rdr["CreateUser"].ToInt32() : null,
						CreationDate = !string.IsNullOrEmpty(rdr["CreateDate"].ToStr()) ? rdr["CreateDate"].ToDate() : null,
						ModifiedById = !string.IsNullOrEmpty(rdr["UpdateUser"].ToStr()) ? rdr["UpdateUser"].ToInt32() : null,
						ModifiedDate = !string.IsNullOrEmpty(rdr["UpdateDate"].ToStr()) ? rdr["UpdateDate"].ToDate() : null,
					};

					returnValue.Add(element);
				}
				// Si la busqueda es por turno se agrega el detalle
				if (!string.IsNullOrEmpty(Code) && returnValue is not null)
				{
					rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						SchedulingShiftDetail detail = new()
						{
							CodeShift = rdr["ShiftCode"].ToStr(),
							CodeShiftStatus = rdr["ShiftStatusCode"].ToStr(),
							Name = rdr["Name"].ToStr(),
							Efficiency = rdr["Efficiency"].ToDecimal(),
							Factor = rdr["Factor"].ToDecimal(),
							Style = rdr["Style"].ToStr(),
							Color = rdr["Color"].ToStr(),
							Duration = rdr["DurationInSec"].ToInt64(),
							Order = rdr["SortId"].ToInt32(),
							Status = rdr["Status"].ToInt32(),
							ShiftCheck = string.IsNullOrEmpty(rdr["ShiftCheck"].ToStr()) ? null : rdr["ShiftCheck"].ToStr(),
						};

						returnValue[0].ShiftDetails.Add(detail);
					}
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
	public ResponseData PutSchedulingShift(SchedulingShift request, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SchedulingShift_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_Code", request.Code);
				command.Parameters.AddWithValue("_Name", request.Name);
				command.Parameters.AddWithValue("_Duration", request.Duration.ToInt64());
				//command.Parameters.AddWithValue("_Color", request.Color);
				command.Parameters.AddWithValue("_ReferenceDate", request.ReferenceDate);
				command.Parameters.AddWithValue("_AllowSetup", 0);
				command.Parameters.AddWithValue("_Operator", request.UserId.ToInt32());
				command.Parameters.AddCondition("_Employee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				command.Parameters.AddWithValue("_Status", request.Status.ToInt32());
				command.Parameters.AddWithValue("_Type", request.Type);
				command.Parameters.AddWithValue("_DetallesJSON", request.ShiftDetailsToJSON());
				command.Parameters.AddWithValue("_IsValidation", Validation);
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
	#endregion SchedulingShift

	#region SchedulingCalendarShifts

	/// <summary>
	/// get list catalog Scheduling Calendar Shifts
	/// </summary>
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

				command.Parameters.AddWithValue("_Id", Id);
				//command.Parameters.AddWithValue("_CodeShift", CodeShift);
				command.Parameters.AddWithValue("_AssetCode", AssetCode);
				command.Parameters.AddWithValue("_IdParent", IdParent);
				command.Parameters.AddWithValue("_AssetLevel", AssetLevel);
				command.Parameters.AddWithValue("_AssetLevelCode", AssetLevelCode);
				command.Parameters.AddWithValue("_Origin", Origin);
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

	/// <summary>
	/// create / update catalog Scheduling Calendar Shifts
	/// </summary>
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

				command.Parameters.AddWithValue("_Id", request.Id);
				command.Parameters.AddWithValue("_CodeShift", request.CodeShift);
				command.Parameters.AddWithValue("_IdAsset", request.IdAsset);
				command.Parameters.AddWithValue("_AssetLevel", request.AssetLevel);
				command.Parameters.AddWithValue("_AssetLevelCode", request.AssetLevelCode);
				command.Parameters.AddWithValue("_FromDate", request.FromDate);
				command.Parameters.AddWithValue("_IsParent", request.IsParent);
				command.Parameters.AddWithValue("_IdParent", request.IdParent);
				command.Parameters.AddWithValue("_Status", request.Status.ToInt32());
				command.Parameters.AddWithValue("_Operator", systemOperator.Id);
				command.Parameters.AddWithValue("_OperatorEmployee", systemOperator.EmployeeId);
				command.Parameters.AddWithValue("_Origin", request.Origin);
				command.Parameters.AddWithValue("_Validation", request.Validation);
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
				command.Parameters.AddWithValue("_Id", request.Id);
				command.Parameters.AddWithValue("_Operator", request.UserId.ToInt32());
				command.Parameters.AddWithValue("_Origin", request.Origin);
				command.Parameters.AddWithValue("_CodeOrigin", request.CodeOrigin);
				command.Parameters.AddWithValue("_Validation", request.Validation);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	#endregion SchedulingCalendarShifts

	#region Scheduling Viewer

	/// <summary>
	///
	/// </summary>
	public async Task<ResultGantt> GetSchedulingViewer(CancellationToken cancel = default)
	{
		ResultGantt resultValue = new();
		List<GanttRow> listGanttRows = [];
		List<CalendarTemplate> listTemplate = [];
		List<CalendarTemplateByResource> listTemplateByResource = [];

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancel).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_Gantt_Rows_SEL_2", connection)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = 600
			};

			await using (command.ConfigureAwait(false))
			{
				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// Declare GetOrdinal variables outside the loop for better performance
						int idOrdinal = rdr.GetOrdinal("Id");
						int nameOrdinal = rdr.GetOrdinal("name");
						int heightOrdinal = rdr.GetOrdinal("height");
						int classesOrdinal = rdr.GetOrdinal("classes");
						int idOrderNoOrdinal = rdr.GetOrdinal("IdOrderNo");
						int nameOrderOrdinal = rdr.GetOrdinal("nameOrder");
						int orderNoOrdinal = rdr.GetOrdinal("orderNo");
						int colorOrdinal = rdr.GetOrdinal("color");
						int fromOrdinal = rdr.GetOrdinal("from");
						int toOrdinal = rdr.GetOrdinal("to");
						int priorityOrdinal = rdr.GetOrdinal("Priority");
						int contentOrdinal = rdr.GetOrdinal("content");
						int classestaskOrdinal = rdr.GetOrdinal("classestask");
						int toDependencyOrdinal = rdr.GetOrdinal("toDependecy");
						int jsPlumbDefaultsOrdinal = rdr.GetOrdinal("jsPlumbDefaults");
						int colorDependencyOrdinal = rdr.GetOrdinal("colorDependecy");
						int productOrdinal = rdr.GetOrdinal("Product");
						int descProductOrdinal = rdr.GetOrdinal("DescProduct");
						int quantityOrdinal = rdr.GetOrdinal("Quantity");
						int positionOrdinal = rdr.GetOrdinal("Position");
						int dueDateOrdinal = rdr.GetOrdinal("DueDate");
						int totalSetupTimeOrdinal = rdr.GetOrdinal("TotalSetupTime");
						int totalProcessTimeOrdinal = rdr.GetOrdinal("TotalProcessTime");
						int opNoOrdinal = rdr.GetOrdinal("OpNo");
						int setupStartOrdinal = rdr.GetOrdinal("SetupStart");
						int efficiencyOrdinal = rdr.GetOrdinal("Efficiency");

						// Loop to read GanttRow data
						while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
						{
							GanttRow objAdd = new()
							{
								id = rdr[idOrdinal].ToString(),
								name = rdr[nameOrdinal].ToString(),
								height = rdr[heightOrdinal].ToString(),
								classes = rdr[classesOrdinal].ToString(),
								IdOrderNo = rdr[idOrderNoOrdinal].ToString(),
								nameOrder = rdr[nameOrderOrdinal].ToString(),
								orderNo = rdr[orderNoOrdinal].ToString(),
								color = rdr[colorOrdinal].ToString(),
								from = rdr[fromOrdinal].ToDate(),
								to = rdr[toOrdinal].ToDate(),
								Priority = rdr[priorityOrdinal].ToInt32(),
								content = rdr[contentOrdinal].ToString(),
								classestask = rdr[classestaskOrdinal].ToString(),
								toDependency = rdr[toDependencyOrdinal].ToString(),
								jsPlumbDefaults = rdr[jsPlumbDefaultsOrdinal].ToString(),
								colorDependency = rdr[colorDependencyOrdinal].ToString(),
								Product = rdr[productOrdinal].ToString(),
								DescProduct = rdr[descProductOrdinal].ToString(),
								Quantity = rdr[quantityOrdinal].ToString(),
								Position = rdr[positionOrdinal].ToInt32(),
								DueDate = await rdr.IsDBNullAsync(dueDateOrdinal, cancel).ConfigureAwait(false) ? null : rdr[dueDateOrdinal].ToDate(),
								TotalSetupTime = await rdr.IsDBNullAsync(totalSetupTimeOrdinal, cancel).ConfigureAwait(false) ? 0 : rdr[totalSetupTimeOrdinal].ToInt32(),
								TotalProcessTime = rdr[totalProcessTimeOrdinal].ToInt32(),
								OpNo = rdr[opNoOrdinal].ToString(),
								SetupStart = await rdr.IsDBNullAsync(setupStartOrdinal, cancel).ConfigureAwait(false) ? null : rdr[setupStartOrdinal].ToDate(),
								Efficiency = rdr[efficiencyOrdinal].ToString()
							};
							listGanttRows.Add(objAdd);
						}

						// Move to the next result for CalendarTemplate
						await rdr.NextResultAsync(cancel).ConfigureAwait(false);

						// Declare GetOrdinal variables for CalendarTemplate
						int resourcesIdOrdinal = rdr.GetOrdinal("ResourcesId");
						int resourceOrdinal = rdr.GetOrdinal("Resource");
						int templateIdOrdinal = rdr.GetOrdinal("TemplateId");
						int templateNameOrdinal = rdr.GetOrdinal("TemplateName");
						int startWeekDayOrdinal = rdr.GetOrdinal("StartWeekDay");
						int startTimeOrdinal = rdr.GetOrdinal("StartTime");
						int endWeekDayOrdinal = rdr.GetOrdinal("EndWeekDay");
						int endTimeOrdinal = rdr.GetOrdinal("EndTime");
						int stateIdOrdinal = rdr.GetOrdinal("StateId");
						int shiftCheckOrdinal = rdr.GetOrdinal("ShiftCheck");
						int stateOrdinal = rdr.GetOrdinal("State");
						int templateEfficiencyOrdinal = rdr.GetOrdinal("Efficiency");
						int colorOrdinalTemplate = rdr.GetOrdinal("Color");
						int orderByTemplateOrdinal = rdr.GetOrdinal("OrderByTemplate");
						int durationOrdinal = rdr.GetOrdinal("Duration");

						while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
						{
							CalendarTemplate objAdd = new()
							{
								ResourcesId = rdr[resourcesIdOrdinal].ToString(),
								Resource = rdr[resourceOrdinal].ToString(),
								TemplateId = rdr[templateIdOrdinal].ToString(),
								TemplateName = rdr[templateNameOrdinal].ToString(),
								StartWeekDay = rdr[startWeekDayOrdinal].ToInt32(),
								StartTime = rdr[startTimeOrdinal].ToString(),
								EndWeekDay = rdr[endWeekDayOrdinal].ToInt32(),
								EndTime = rdr[endTimeOrdinal].ToString(),
								StateId = rdr[stateIdOrdinal].ToString(),
								ShiftCheck = rdr[shiftCheckOrdinal].ToString(),
								State = rdr[stateOrdinal].ToString(),
								Efficiency = rdr[templateEfficiencyOrdinal].ToDecimal(),
								Color = rdr[colorOrdinalTemplate].ToString(),
								OrderByTemplate = rdr[orderByTemplateOrdinal].ToInt32(),
								Longitud = rdr[durationOrdinal].ToInt32()
							};
							listTemplate.Add(objAdd);
						}

						// Move to the next result for CalendarTemplateByResource
						await rdr.NextResultAsync(cancel).ConfigureAwait(false);

						// Declare GetOrdinal variables for CalendarTemplateByResource
						int fromDateOrdinal = rdr.GetOrdinal("FromDate");
						int toDateOrdinal = rdr.GetOrdinal("ToDate");
						int referenceDateOrdinal = rdr.GetOrdinal("ReferenceDate");
						int orderByTemplateResourceOrdinal = rdr.GetOrdinal("OrderByTemplateResource");

						while (await rdr.ReadAsync(cancel).ConfigureAwait(false))
						{
							CalendarTemplateByResource objAdd = new()
							{
								ResourcesId = rdr[resourcesIdOrdinal].ToString(),
								Resource = rdr[resourceOrdinal].ToString(),
								TemplateId = rdr[templateIdOrdinal].ToString(),
								FromDate = await rdr.IsDBNullAsync(fromDateOrdinal, cancel).ConfigureAwait(false) ? null : rdr[fromDateOrdinal].ToDate(),
								ToDate = await rdr.IsDBNullAsync(toDateOrdinal, cancel).ConfigureAwait(false) ? null : rdr[toDateOrdinal].ToDate(),
								ReferenceDate = await rdr.IsDBNullAsync(referenceDateOrdinal, cancel).ConfigureAwait(false) ? null : rdr[referenceDateOrdinal].ToDate(),
								OrderByTemplateResource = rdr[orderByTemplateResourceOrdinal].ToInt32()
							};
							listTemplateByResource.Add(objAdd);
						}

						resultValue.listGanttRows = listGanttRows;
						resultValue.listCalendarTemplate = listTemplate;
						resultValue.listCalendarTemplateByResource = listTemplateByResource;
					}
				}
				catch (Exception ex)
				{
					//logger.Error(ex);
					throw;
				}

				return resultValue;
			}
		}
	}
	#endregion Scheduling Viewer

	#region Position Shift Explosion
	/// <summary>
	///
	/// </summary>
	public void GeneratePositionShiftExplosion(string EmployeeCode)
	{
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_PositionShiftExplosion_INS", connection)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = 30000
			};
			command.Parameters.Clear();

			command.Parameters.AddWithValue("_EmployeeCode", EmployeeCode);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				ResponseData returnValue = new()
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

	/// <summary>
	///
	/// </summary>
	public void GeneratePositionLoopShiftExplosion(string ShiftCode, string ShiftStatusCode)
	{
		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_PositionLoopShiftExplosion", connection)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = 30000
			};
			command.Parameters.Clear();

			command.Parameters.AddWithValue("_ShiftCode", ShiftCode);
			command.Parameters.AddWithValue("_ShiftStatusCode", ShiftStatusCode);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				ResponseData returnValue = new()
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

	/// <summary>
	///
	/// </summary>
	public List<PositionExplosionDetail> ListPositionShiftExplosionDetail(string Position, DateTime StartDate, DateTime EndDate)
	{
		List<PositionExplosionDetail> returnValue = [];
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_PositionShiftExplosionDetail", connection)
				{
					CommandType = CommandType.StoredProcedure,
					CommandTimeout = 30000
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_Position", Position);
				command.Parameters.AddWithValue("_StartDate", StartDate);
				command.Parameters.AddWithValue("_EndDate", EndDate);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					PositionExplosionDetail element = new()
					{
						EmployeeCode = rdr["EmployeeCode"].ToStr(),
						EmployeeName = rdr["EmployeeName"].ToStr(),
						PositionCode = rdr["PositionCode"].ToStr(),
						PositionName = rdr["PositionName"].ToStr(),
						IntervalStart = rdr["IntervalStart"].ToDate(),
						IntervalEnd = rdr["IntervalEnd"].ToDate(),
						Efficiency = rdr["Efficiency"].ToDouble(),
						IsActivity = rdr["IsActivity"].ToInt32().ToBool(),
						RowIsEmpty = rdr["RowIsEmpty"].ToInt32().ToBool(),
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

	#endregion Position Shift Explosion
}
