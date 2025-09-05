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
using NLog;

namespace EWP.SF.KafkaSync.DataAccess;

public class OEERepo : IOEERepo
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public OEERepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    /// <summary>
	///
	/// </summary>
	public double SaveSensorAverage(string SensorId, string workOrderId, double value)
	{
		double returnValue = 0;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Sensor_Average_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_SensorCode", SensorId, !string.IsNullOrEmpty(SensorId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Sensor"));
				command.Parameters.AddWithValue("_OrderCode", workOrderId);
				command.Parameters.AddWithValue("_Value", value);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = command.ExecuteScalarAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToDouble();
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
	public MachineOEEConfiguration GetMachineOeeConfiguration(string machineId)
	{
		MachineOEEConfiguration returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MachineOEEConfiguration_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine"));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new MachineOEEConfiguration
					{
						AvailabilityMode = (OEEMode)rdr["AvailabilityMode"].ToInt32(),
						AvailabilitySourceId = rdr["AvailabilitySourceId"].ToStr(),
						AvailabilityOnValue = rdr["AvailabilityOnValue"].ToStr(),
						AvailabilityOffValue = rdr["AvailabilityOffValue"].ToStr(),
						IdleQty = rdr["IdleQty"].ToStr().ToDouble(),
						IdleSeconds = rdr["IdleTime"].ToDouble(),
						DowntimeModifierId = rdr["DowntimeModifierId"].ToStr(),

						PerformanceDefaultType = rdr["PerformanceType"].ToInt32(),
						PerformanceDefaultValue = rdr["PerformanceValue"].ToStr(),
						PerformanceDefaultUnit = rdr["PerformanceUnit"].ToStr(),
						PerformanceDefaultTimeQty = rdr["PerformanceTimeQty"].ToDouble(),
						PerformanceDefaultTimeUnit = "sec",
						PerformanceTimeFactor = 1,

						PerformanceMode = (OEEMode)rdr["PerformanceMode"].ToInt32(),
						PerformanceTriggerId = rdr["PerformanceTrigger"].ToStr(),
						PerformanceSourceId = rdr["PerformanceSourceId"].ToStr(),
						PerformanceTimeSourceId = rdr["PerformanceTimeSourceId"].ToStr(),

						StartTime = rdr["StartTime"].ToDate(),
						EndTime = rdr["EndTime"].ToDate(),
						AdjustTime = rdr["AdjustTime"].ToBool(),
						ProductionType = rdr["ProductionType"].ToStr(),

						QualityMode = (OEEMode)rdr["QualityMode"].ToInt32(),
						QualitySourceId = rdr["QualitySourceId"].ToStr(),

						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						CreationDate = rdr["CreateDate"].ToDate()
					};
					if (rdr["UpdateDate"].ToDate().Year > 1900)
					{
						returnValue.ModifyDate = rdr["UpdateDate"].ToDate();
						returnValue.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
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
	public List<MachineOEEConfiguration> GetMachineOeeConfiguration()
	{
		List<MachineOEEConfiguration> returnValue = null;
		MachineOEEConfiguration element;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MachineOEEConfiguration_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddNull("_MachineCode");

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue ??= [];
					element = new MachineOEEConfiguration
					{
						MachineId = rdr["MachineCode"].ToStr(),
						AvailabilityMode = (OEEMode)rdr["AvailabilityMode"].ToInt32(),
						AvailabilitySourceId = rdr["AvailabilitySourceId"].ToStr(),
						AvailabilityOnValue = rdr["AvailabilityOnValue"].ToStr(),
						AvailabilityOffValue = rdr["AvailabilityOffValue"].ToStr(),
						IdleQty = !string.IsNullOrEmpty(rdr["IdleQty"].ToStr()) ? rdr["IdleQty"].ToDouble() : 0,
						IdleSeconds = !string.IsNullOrEmpty(rdr["IdleTime"].ToStr()) ? rdr["IdleTime"].ToDouble() : 0,
						DowntimeModifierId = rdr["DowntimeModifierId"].ToStr(),

						PerformanceDefaultType = rdr["PerformanceType"].ToInt32(),
						PerformanceDefaultValue = rdr["PerformanceValue"].ToStr(),
						PerformanceDefaultUnit = rdr["PerformanceUnit"].ToStr(),
						PerformanceDefaultTimeQty = rdr["PerformanceTimeQty"].ToDouble(),
						PerformanceDefaultTimeUnit = "sec",
						PerformanceTimeFactor = 1,

						PerformanceMode = (OEEMode)rdr["PerformanceMode"].ToInt32(),
						PerformanceTriggerId = rdr["PerformanceTrigger"].ToStr(),
						PerformanceSourceId = rdr["PerformanceSourceId"].ToStr(),
						PerformanceTimeSourceId = rdr["PerformanceTimeSourceId"].ToStr(),
						ProductionType = rdr["ProductionType"].ToStr(),

						QualityMode = (OEEMode)rdr["QualityMode"].ToInt32(),
						QualitySourceId = rdr["QualitySourceId"].ToStr(),

						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						CreationDate = !string.IsNullOrEmpty(rdr["CreateDate"].ToStr()) ? Convert.ToDateTime(rdr["CreateDate"], CultureInfo.InvariantCulture) : new DateTime()
					};

					if (!string.IsNullOrEmpty(rdr["UpdateDate"].ToStr()))
					{
						element.ModifyDate = Convert.ToDateTime(rdr["UpdateDate"], CultureInfo.InvariantCulture);
						element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
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

	/// <summary>
	///
	/// </summary>
	public bool SaveMachineOeeConfiguration(string machineId, MachineOEEConfiguration configuration, User systemOperator)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MachineOEEConfiguration_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine"));
				command.Parameters.AddWithValue("_AvailabilityMode", configuration.AvailabilityMode.ToInt32());
				command.Parameters.AddWithValue("_AvailabilitySourceID", configuration.AvailabilitySourceId);

				command.Parameters.AddWithValue("_AvailabilityOnValue", configuration.AvailabilityOnValue);
				command.Parameters.AddWithValue("_AvailabilityOffValue", configuration.AvailabilityOffValue);

				command.Parameters.AddWithValue("_IdleQty", configuration.IdleQty);
				command.Parameters.AddWithValue("_DowntimeModifierId", configuration.DowntimeModifierId);

				command.Parameters.AddWithValue("_PerformanceMode", configuration.PerformanceMode);
				command.Parameters.AddWithValue("_PerformanceTrigger", configuration.PerformanceTriggerId);

				command.Parameters.AddWithValue("_PerformanceSourceID", configuration.PerformanceSourceId);

				command.Parameters.AddWithValue("_PerformanceTimeSourceId", configuration.PerformanceTimeSourceId);

				command.Parameters.AddWithValue("_PerformanceType", configuration.PerformanceDefaultType);
				command.Parameters.AddWithValue("_PerformanceValue", configuration.PerformanceDefaultValue);
				command.Parameters.AddWithValue("_PerformanceUnit", configuration.PerformanceDefaultUnit);
				command.Parameters.AddWithValue("_PerformanceTimeQty", configuration.PerformanceDefaultTimeQty);
				command.Parameters.AddWithValue("_QualityMode", configuration.QualityMode.ToInt32());
				command.Parameters.AddWithValue("_QualitySourceId", configuration.QualitySourceId);
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, !systemOperator.IsNull(), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Operator"));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				command.Parameters.AddWithValue("_StartTime", configuration.StartTime);
				command.Parameters.AddWithValue("_EndTime", configuration.EndTime);
				command.Parameters.AddWithValue("_AdjustTime", configuration.AdjustTime);
				command.Parameters.AddWithValue("_ProductionType", configuration.ProductionType);
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

	/// <summary>
	///
	/// </summary>
	public bool SaveMachineProgramming(string machineId, MachineProgramming configuration, User systemOperator)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MachineProgramming_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine"));
				command.Parameters.AddWithValue("_CapacityModeCode", configuration.CapacityMode);
				command.Parameters.AddWithValue("_ChangeGroupCode", configuration.GroupChange);
				command.Parameters.AddWithValue("_GanttPosition", configuration.GanttPosition);
				command.Parameters.AddWithValue("_TheoricEfficiency", configuration.TheoricEfficiency);
				command.Parameters.AddWithValue("_CostPerHour", configuration.CostPerHour);
				command.Parameters.AddWithValue("_Attribute2", configuration.Attribute2);
				command.Parameters.AddWithValue("_Attribute3", configuration.Attribute3);
				command.Parameters.AddCondition("_InfiniteModeBehavior", configuration.InfinityModeBehavior, !string.IsNullOrEmpty(configuration.InfinityModeBehavior));
				command.Parameters.AddWithValue("_ScheduleLevel", configuration.ScheduleLevel ?? (int?)null);
				command.Parameters.AddWithValue("_ConcurrentSetupTime", configuration.ConcurrentSetupTime);
				command.Parameters.AddWithValue("_Schedule", configuration.Schedule);
				command.Parameters.AddWithValue("_DetailJSON", configuration.GetJsonDetails());
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

	/// <summary>
	///
	/// </summary>
	public MachineProgramming GetMachineProgramming(string machineId)
	{
		MachineProgramming returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MachineProgramming_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_MachineCode", machineId);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new MachineProgramming
					{
						CapacityMode = rdr["CapacityModeCode"].ToStr(),
						GroupChange = rdr["ChangeGroupCode"].ToStr(),
						GanttPosition = rdr["GanttPosition"].ToStr(),
						TheoricEfficiency = rdr["TheoricEfficiency"].ToStr(),
						CostPerHour = rdr["CostPerHour"].ToDouble(),
						Attribute2 = !string.IsNullOrEmpty(rdr["Attribute2"].ToStr()) ? rdr["Attribute2"].ToInt32() : null,
						Attribute3 = !string.IsNullOrEmpty(rdr["Attribute3"].ToStr()) ? rdr["Attribute3"].ToInt32() : null,
						InfinityModeBehavior = rdr["InfiniteModeBehavior"].ToStr(),
						ScheduleLevel = !string.IsNullOrEmpty(rdr["ScheduleLevel"].ToStr()) ? Convert.ToInt16(rdr["ScheduleLevel"]) : null,
						ConcurrentSetupTime = rdr["ConcurrentSetupTime"].ToBool(),
						Schedule = rdr["Schedule"].ToBool(),
						Planning = rdr["Planning"].ToBool(),

						Details = []
					};
				}
				if (rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						MachineProgrammingDetail element = new()
						{
							CriteriaType = rdr["CriteriaCode"].ToStr(),
							SortType = rdr["SortCode"].ToStr(),
							Sort = rdr["SortId"].ToInt32()
						};
						returnValue.Details.Add(element);
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
	public bool SaveMachineAvailability(string machineId, bool online, User systemOperator, OEEMode mode, string downtimeId = "")
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MACHINE_AVAILABILITY_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine"));
				command.Parameters.AddWithValue("_IO", online);
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, !systemOperator.IsNull(), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Operator"));
				command.Parameters.AddWithValue("_TypeId", mode.ToInt32());
				command.Parameters.AddWithValue("_DowntimeId", downtimeId);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
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
	public async Task<OEEModel> GetLiveOee(string machineId, DateTime? startDate, CancellationToken cancel = default)
	{
		OEEModel returnValue = new();

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancel).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_Machine_OEE_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = 600
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Device"));
				command.Parameters.AddCondition("_StartDate", startDate, startDate.HasValue);

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancel).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// Declare GetOrdinal variables outside the loop for better performance
						int availabilityOrdinal = rdr.GetOrdinal("Availability");
						int performanceOrdinal = rdr.GetOrdinal("Performance");
						int qualityOrdinal = rdr.GetOrdinal("Quality");
						int oeeOrdinal = rdr.GetOrdinal("OEE");
						int onTimeOrdinal = rdr.GetOrdinal("OnTime");
						int offTimeOrdinal = rdr.GetOrdinal("OffTime");

						if (await rdr.ReadAsync(cancel).ConfigureAwait(false))
						{
							returnValue.Availability = rdr[availabilityOrdinal].ToDouble();
							returnValue.Performance = rdr[performanceOrdinal].ToDouble();
							returnValue.Quality = rdr[qualityOrdinal].ToDouble();
							returnValue.OEE = rdr[oeeOrdinal].ToDouble();
							returnValue.OnTime = rdr[onTimeOrdinal].ToStr();
							returnValue.OffTime = rdr[offTimeOrdinal].ToStr();
						}
					}
				}
				catch (Exception ex)
				{
					logger.Error(ex, "An error occurred while fetching live OEE data");
					throw; // Re-throw the exception to propagate it up the call stack
				}

				return returnValue;
			}
		}
	}

	/// <summary>
	///
	/// </summary>
	public bool SaveMachinePerformance(string machineId, string processId, bool isOutput, string workOrderId, double value, double factor, double deviceFactor, double processFactor, double orderFactor)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MACHINE_PERFORMANCE_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine"));
				command.Parameters.AddCondition("_OperationNo", processId, !string.IsNullOrEmpty(processId));
				command.Parameters.AddWithValue("_IsOutput", isOutput);
				command.Parameters.AddCondition("_OrderCode", workOrderId, !string.IsNullOrEmpty(workOrderId));
				command.Parameters.AddWithValue("_Value", value);
				command.Parameters.AddWithValue("_Factor", factor);
				command.Parameters.AddWithValue("_DeviceFactor", deviceFactor);
				command.Parameters.AddWithValue("_ProcessFactor", processFactor);
				command.Parameters.AddWithValue("_OrderFactor", orderFactor);
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

	/// <summary>
	///
	/// </summary>
	public double SaveMachineQuality(string machineId, bool isOutput, string workOrderId, string processId, string shiftId, QualityType type, QualityMode mode, string testId, string sampleId, double sample, double rejected, User systemOperator, string employeeId = "", Action<string> callback = null)
	{
		double returnValue = 0;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Machine_Quality_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine"));
				command.Parameters.AddWithValue("_IsOutput", isOutput);
				command.Parameters.AddCondition("_OrderCode", workOrderId, !string.IsNullOrEmpty(workOrderId));
				command.Parameters.AddCondition("_OperationNo", processId, !string.IsNullOrEmpty(processId));
				command.Parameters.AddCondition("_ShiftId", shiftId, !string.IsNullOrEmpty(shiftId));
				command.Parameters.AddWithValue("_TypeId", type.ToInt32());
				command.Parameters.AddWithValue("_Mode", mode.ToInt32());
				command.Parameters.AddWithValue("_Sample", sample);
				command.Parameters.AddWithValue("_Rejected", rejected);
				command.Parameters.AddCondition("_EmployeeId", employeeId, !string.IsNullOrEmpty(employeeId));
				command.Parameters.AddCondition("_TestId", employeeId, !string.IsNullOrEmpty(testId));
				command.Parameters.AddCondition("_SampleId", employeeId, !string.IsNullOrEmpty(sampleId));
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = rdr["Rejected"].ToDouble();
					if (!string.IsNullOrEmpty(employeeId) && callback is not null)
					{
						string logId = rdr["LogId"].ToStr();
						if (!string.IsNullOrEmpty(logId))
						{
							callback(logId);
						}
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
	public bool SetDeviceDown(string deviceId)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_SetDeviceDown", connection)
				{
					CommandType = CommandType.StoredProcedure,
					CommandTimeout = 600
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_MachineCode", deviceId);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return returnValue;
	}
}