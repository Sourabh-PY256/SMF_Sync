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
using EWP.SF.Common.Models.Sensors;
using NLog;

namespace EWP.SF.KafkaSync.DataAccess;

public class MachineRepo : IMachineRepo
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
	
	private readonly string ConnectionString;
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
	private readonly string ConnectionStringReports;
	private readonly string ConnectionStringLogs;

	private readonly string Database;

	public MachineRepo(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
		ConnectionStringReports = applicationSettings.GetReportsConnectionString();
		ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
		Database = applicationSettings.GetDatabaseFromConnectionString();
	}
	/// <summary>
	///
	/// </summary>
	public List<Machine> ListMachines(string machineId = null, bool onlyActive = false, DateTime? DeltaDate = null)
	{
		List<Machine> returnValue = null;
		Machine element;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Machine_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId));
				command.Parameters.AddWithValue("_OnlyActive", onlyActive);
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					if (returnValue.IsNull())
					{
						returnValue = [];
					}

					element = new Machine
					{
						Id = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						ParentCode = rdr["ParentCode"].ToStr(),
						ProductionLineId = rdr["ParentCode"].ToStr(),
						Description = rdr["Name"].ToStr(),
						TypeId = rdr["OperationTypeCode"].ToStr(),
						Image = rdr["Image"].ToStr(),
						MaximumCapacity = rdr["MaximumCapacity"].ToInt32(),
						MinimumCapacity = rdr["MinimumCapacity"].ToInt32(),
						Location = rdr["Location"].ToStr(),
						CreationDate = !string.IsNullOrEmpty(rdr["CreateDate"].ToStr()) ? Convert.ToDateTime(rdr["CreateDate"], CultureInfo.InvariantCulture) : DateTime.UtcNow,
						LiveIconId = rdr["LiveIconId"].ToInt32(),
						IsAuxiliar = rdr["MachineType"].ToStr().Equals("auxiliar", StringComparison.OrdinalIgnoreCase),
						FacilityId = rdr["FacilityCode"].ToStr(),
						FloorId = rdr["FloorCode"].ToStr(),
						CtrlModel = rdr["CtrlModel"].ToStr(),
						CtrlSerial = rdr["CtrlSerial"].ToStr(),
						ManufactureDate = rdr["ManufactureDate"].ToStr(),
						RobotArmModel = rdr["RobotArmModel"].ToStr(),
						PwrSourceModel = rdr["PwrSourceModel"].ToStr(),
						PLCManufacturer = rdr["PLCManufacturer"].ToStr(),
						PLCSerial = rdr["PLCSerial"].ToStr(),
						HasTool = rdr["HasTool"].ToBool(),
						Status = (Status)rdr["Status"].ToInt32(),
						FacilityCode = rdr["FacilityCode"].ToStr(),
						LotCapacity = rdr["LotCapacity"].ToDouble(),
						LotCalculation = rdr["LotCalculation"].ToStr(),
						ProductionType = rdr["ProductionType"].ToStr(),
						Warehouse = rdr["Warehouse"].ToStr(),
						WarehouseCode = rdr["WarehouseCode"].ToStr(),
						BinLocations = rdr["BinLocationCode"].ToStr(),
						LogDetailId = rdr["LogDetailId"].ToStr()
					};

					returnValue.Add(element);
				}
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
	/// <summary>
	///
	/// </summary>
	public ResponseData CreateMachine(Machine machineInfo, User systemOperator, bool Validation, string Level)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Machine_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_Code", machineInfo?.Code);
				command.Parameters.AddWithValue("_Name", machineInfo?.Description);
				command.Parameters.AddWithValue("_OperationTypeCode", machineInfo?.TypeId);
				command.Parameters.AddWithValue("_Image", machineInfo?.Image);
				command.Parameters.AddWithValue("_Location", machineInfo?.Location);
				command.Parameters.AddWithValue("_LiveIconId", machineInfo?.LiveIconId.Val());
				command.Parameters.AddWithValue("_MaximumCapacity", machineInfo?.MaximumCapacity);
				command.Parameters.AddWithValue("_MinimumCapacity", machineInfo?.MinimumCapacity);
				command.Parameters.AddWithValue("_Status", machineInfo?.Status.ToInt32());
				command.Parameters.AddWithValue("_MachineType", machineInfo.IsAuxiliar ? "Auxiliar" : "Process");
				command.Parameters.AddWithValue("_CtrlSerial", machineInfo?.CtrlSerial);
				command.Parameters.AddWithValue("_CtrlModel", machineInfo?.CtrlModel);
				command.Parameters.AddWithValue("_ManufactureDate", machineInfo?.ManufactureDate);
				command.Parameters.AddWithValue("_RobotArmModel", machineInfo?.RobotArmModel);
				command.Parameters.AddWithValue("_PwrSourceModel", machineInfo?.PwrSourceModel);
				command.Parameters.AddWithValue("_PLCManufacturer", machineInfo?.PLCManufacturer);
				command.Parameters.AddWithValue("_PLCSerial", machineInfo?.PLCSerial);
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null);
				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddWithValue("_Level", Level);
				command.Parameters.AddWithValue("_ParentCode", machineInfo?.ParentCode);
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				command.Parameters.AddCondition("_OperationTypeDetails", () => JsonConvert.SerializeObject(machineInfo.ProcessTypeDetails), machineInfo.ProcessTypeDetails is not null);
				command.Parameters.AddCondition("_OEEConfiguration", () => JsonConvert.SerializeObject(machineInfo.OEEConfiguration), machineInfo.OEEConfiguration is not null);
				command.Parameters.AddCondition("_SchedulingData", () => JsonConvert.SerializeObject(machineInfo.Programming), machineInfo.Programming is not null);
				command.Parameters.AddCondition("_Planning", () => machineInfo.Programming.Planning, machineInfo.Programming is not null);
				command.Parameters.AddWithValue("_binLocations", machineInfo?.BinLocations);
				command.Parameters.AddWithValue("_WarehouseCode", machineInfo?.WarehouseCode);
				command.Parameters.AddWithValue("_LotCalculation", machineInfo?.LotCalculation);
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
	public List<Sensor> ListSensors(string SensorId = null, string machineId = null)
	{
		List<Sensor> returnValue = null;
		Sensor element;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Sensor_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_SensorCode", SensorId, !string.IsNullOrEmpty(SensorId));
				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId));
				command.Parameters.AddNull("_DeltaDate");
				// command.Parameters.AddNull("_Code");
				//   command.Parameters.AddNull("_MachineCode");

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					element = new Sensor
					{
						Id = rdr["Code"].ToStr(),
						Description = rdr["Name"].ToStr(),
						Code = rdr["Code"].ToStr(),
						MachineId = rdr["MachineCode"].ToStr(),
						MaximumValue = rdr["MaximumValue"].ToDouble(),
						MinimumValue = rdr["MinimumValue"].ToDouble(),
						Location = rdr["Location"].ToStr(),
						TypeId = rdr["TagTypeCode"].ToStr(),
						TagTypeName = rdr["TagTypeName"].ToStr(),
						OutOfRangeAlert = rdr["OutOfRangeAlert"].ToBool(),
						ApplicationAlert = rdr["ApplicationALert"].ToBool(),
						EmailAlert = rdr["EmailAlert"].ToBool(),
						IgnoreForHistory = rdr["IgnoreForHistory"].ToBool(),
						LiveScreen = rdr["LiveScreen"].ToBool(),
						Icon = rdr["Icon"].ToStr(),
						UnitId = rdr["Unit"].ToStr(),
						CodeUnit = rdr["Unit"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						TagClass = rdr["TagClass"].ToStr(),
						UrlStreaming = rdr["UrlStreaming"].ToStr()
					};

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
	public List<Sensor> GetSensors(string SensorCode, string MachineId, DateTime? DeltaDate = null)
	{
		List<Sensor> returnValue = [];
		Sensor element = null;
		SensorWhen detailSensorWhen = null;
		SensorThen detailSensorThen = null;
		SensorLiveViewer detailSensorLiveViewer = null;
		SensorRecipient detailRecipient = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Sensor_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_SensorCode", SensorCode);
				command.Parameters.AddWithValue("_MachineCode", MachineId);
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					element = new Sensor
					{
						Id = rdr["Code"].ToStr(),
						MachineId = rdr["MachineCode"].ToStr(),
						MachineValueId = rdr["MachineCode"].ToStr(),
						Description = rdr["Name"].ToStr(),
						Code = rdr["Code"].ToStr(),
						MachineDescription = rdr["MachineDescription"].ToStr(),
						TypeId = rdr["TagTypeCode"].ToStr(),
						TagTypeName = rdr["TagTypeName"].ToStr(),
						Picture = rdr["Picture"].ToStr(),
						UnitId = rdr["Unit"].ToStr(),
						UnitValueId = rdr["Unit"].ToStr(),
						IconId = rdr["Icon"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						LiveScreen = rdr["Livescreen"].ToBool(),
						IgnoreForHistory = rdr["IgnoreForHistory"].ToBool(),
						MaximumValue = rdr["MaximumValue"].ToInt32(),
						MinimumValue = rdr["MinimumValue"].ToInt32(),
						TagClass = rdr["TagClass"].ToStr(),
						UrlStreaming = rdr["UrlStreaming"].ToStr(),
						Flicker = rdr["Flicker"].ToBool(),
						Color = rdr["Color"].ToStr(),
					};

					returnValue.Add(element);
				}

				// Si la búsqueda es por turno se agrega el detalle
				if (!string.IsNullOrEmpty(SensorCode) && returnValue?.Count > 0)
				{
					rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						detailSensorWhen = new SensorWhen
						{
							SensorId = rdr["SensorCode"].ToStr(),
							Order = rdr["Order"].ToInt32(),
							MinimumValue = rdr["MinimumValue"].ToDecimal(),
							MaximumValue = rdr["MaximumValue"].ToDecimal(),
							Duration = rdr["Duration"].ToInt64()
						};
						returnValue[0].SensorsWhen.Add(detailSensorWhen);
					}
					rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						detailSensorThen = new SensorThen
						{
							SensorId = rdr["SensorCode"].ToStr(),
							When = rdr["When"].ToInt32(),
							Order = rdr["Order"].ToInt32(),
							Type = rdr["Type"].ToInt32(),
							Action = rdr["Action"].ToStr(),
							RequiresConfirm = rdr["RequiresConfirm"].ToInt32(),
							TemplateId = rdr["TemplateCode"].ToStr(),
							IdleTimeout = rdr["IdleTimeout"].ToInt64(),
							Duration = rdr["IdleTimeout"].ToInt64()
						};
						returnValue[0].SensorsWhen.First(x => x.Order == rdr["When"].ToInt32()).SensorsThen.Add(detailSensorThen);
					}

					rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						detailSensorLiveViewer = new SensorLiveViewer
						{
							SensorId = rdr["SensorCode"].ToStr(),
							MinimumValue = rdr["MinimumValue"].ToDecimal(),
							MaximumValue = rdr["MaximumValue"].ToDecimal(),
							Color = rdr["Color"].ToStr(),
							Duration = rdr["Duration"].ToInt64(),
							Flicker = rdr["Flicker"].ToBool(),
							Order = rdr["Order"].ToInt32()
						};
						returnValue[0].SensorLiveViewer.Add(detailSensorLiveViewer);
					}
					rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						detailRecipient = new SensorRecipient
						{
							SensorId = rdr["SensorCode"].ToStr(),
							When = rdr["When"].ToInt32(),
							Then = rdr["Then"].ToInt32(),
							Value = rdr["Value"].ToString(),
							//Description = rdr["Description"].ToString()
						};
						returnValue[0].SensorsWhen.First(x => x.Order == detailRecipient.When).
							SensorsThen.First(x => x.Order == detailRecipient.Then).SensorsRecipients.Add(detailRecipient);
					}
				}
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
	/// Get list Sensor details
	/// </summary>
	public Sensor GetSensorsDetails(string SensorId)
	{
		Sensor returnValue = new();
		SensorWhen detailSensorWhen = null;
		SensorThen detailSensorThen = null;
		SensorLiveViewer detailSensorLiveViewer = null;
		SensorRecipient detailRecipient = null;
		string SensorErrorId = string.Empty;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Sensor_details_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_SensorCode", SensorId);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					detailSensorWhen = new SensorWhen
					{
						SensorId = rdr["SensorCode"].ToStr(),
						Order = rdr["Order"].ToInt32(),
						MinimumValue = rdr["MinimumValue"].ToDecimal(),
						MaximumValue = rdr["MaximumValue"].ToDecimal(),
						Duration = rdr["Duration"].ToInt64()
					};
					SensorErrorId = rdr["SensorId"].ToStr();
					returnValue.SensorsWhen.Add(detailSensorWhen);
				}
				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					detailSensorThen = new SensorThen
					{
						SensorId = rdr["SensorCode"].ToStr(),
						When = rdr["When"].ToInt32(),
						Order = rdr["Order"].ToInt32(),
						Type = rdr["Type"].ToInt32(),
						Action = rdr["Action"].ToStr(),
						RequiresConfirm = rdr["RequiresConfirm"].ToInt32(),
						TemplateId = rdr["TemplateCode"].ToStr(),
						IdleTimeout = rdr["IdleTimeout"].ToInt64(),
						Duration = rdr["IdleTimeout"].ToInt64()
					};
					SensorErrorId = rdr["SensorCode"].ToStr();
					returnValue.SensorsWhen.First(x => x.SensorId == rdr["SensorCode"].ToStr() && x.Order == rdr["When"].ToInt32()).SensorsThen.Add(detailSensorThen);
				}

				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					detailSensorLiveViewer = new SensorLiveViewer
					{
						SensorId = rdr["SensorCode"].ToStr(),
						MinimumValue = rdr["MinimumValue"].ToDecimal(),
						MaximumValue = rdr["MaximumValue"].ToDecimal(),
						Color = rdr["Color"].ToStr(),
						Duration = rdr["Duration"].ToInt64(),
						Flicker = rdr["Flicker"].ToBool(),
						Order = rdr["Order"].ToInt32()
					};
					SensorErrorId = rdr["SensorCode"].ToStr();
					returnValue.SensorLiveViewer.Add(detailSensorLiveViewer);
				}
				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					detailRecipient = new SensorRecipient
					{
						SensorId = rdr["SensorCode"].ToStr(),
						When = rdr["When"].ToInt32(),
						Then = rdr["Then"].ToInt32(),
						Value = rdr["Value"].ToString(),
						//Description = rdr["Description"].ToString()
					};
					SensorErrorId = rdr["SensorCode"].ToStr();
					returnValue.SensorsWhen.First(x => x.SensorId == detailRecipient.SensorId && x.Order == detailRecipient.When)?.
						SensorsThen.First(x => x.SensorId == detailRecipient.SensorId && x.Order == detailRecipient.Then)?.SensorsRecipients.Add(detailRecipient);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
				throw new Exception("Error Detected SensorId = " + SensorErrorId);
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
	public List<MachineParam> ListMachineParams(string parameterId = null, string machineId = null)
	{
		List<MachineParam> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MachineParameter_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_ParameterCode", parameterId, !string.IsNullOrEmpty(parameterId));

				command.Parameters.AddCondition("_MachineCode", machineId, !string.IsNullOrEmpty(machineId));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					MachineParam element = new()
					{
						Id = rdr["Code"].ToStr(),
						Description = rdr["Name"].ToStr(),
						Code = rdr["Code"].ToStr(),
						MachineId = rdr["MachineCode"].ToStr(),
						Formula = rdr["Formula"].ToStr(),
						OutOfRangeAlert = rdr["OutOfRangeAlert"].ToBool(),
						ApplicationAlert = rdr["ApplicationAlert"].ToBool(),
						EmailAlert = rdr["EmailAlert"].ToBool(),
						StoreValue = rdr["StoreValue"].ToBool(),
						IgnoreForHistory = rdr["IgnoreForHistory"].ToBool(),
						CustomBehaviorId = rdr["CustomBehaviorId"].ToStr(),
						FallbackValue = rdr["FallbackValue"].ToStr(),
						LiveScreen = rdr["LiveScreen"].ToBool(),
						EditableScalar = rdr["EditableScalar"].ToBool(),
						DependsSensorId = rdr["DependsSensorCode"].ToStr(),
						Icon = rdr["Icon"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32()
					};

					(returnValue ??= []).Add(element);
				}

				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					if (returnValue is not null)
					{
						MachineParam param = returnValue.Find(x => x.Id == rdr["ParameterCode"].ToStr());
						if (param is not null)
						{
							BehaviorMatch element = new()
							{
								Code = rdr["Code"].ToStr(),
								SourceType = rdr["SourceType"].ToInt32(),
								SourceValue = rdr["SourceValue"].ToStr()
							};

							_ = (param.CustomBehaviorMatch ??= []).TryAdd(element.Code, element);
						}
					}
				}
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


}