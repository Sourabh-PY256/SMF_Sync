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

public class ProcessTypeRepo : IProcessTypeRepo
{
	private readonly string ConnectionString;
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
	private readonly string ConnectionStringReports;
	private readonly string ConnectionStringLogs;

	private readonly string Database;

	public ProcessTypeRepo(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
		ConnectionStringReports = applicationSettings.GetReportsConnectionString();
		ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
		Database = applicationSettings.GetDatabaseFromConnectionString();
	}
	#region ProcessType

	public List<ProcessType> GetProcessType(string processTypeId, bool WithTool = false, DateTime? DeltaDate = null)
	{
		List<ProcessType> returnValue = [];
		ProcessType element;
		ProcessTypeDetail elem;
		Activity elemA;
		ProcessTypeAttribute elemPT;
		ProcessTypeAttribute attribute;
		ProcessTypeAttributeDetail elemAttr;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_OperationType_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddCondition("_OperationTypeCode", processTypeId, !string.IsNullOrEmpty(processTypeId));
				command.Parameters.AddWithValue("_WithTool", WithTool);
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					element = new ProcessType
					{
						Id = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Icon = rdr["Icon"].ToStr(),
						Image = rdr["Image"].ToStr(),
						UnitTypeId = rdr["UnitTypeId"].ToInt32(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
						CreationDate = !string.IsNullOrEmpty(rdr["CreateDate"].ToStr()) ? Convert.ToDateTime(rdr["CreateDate"], CultureInfo.InvariantCulture) : new DateTime(),
						Status = (Status)rdr["Status"].ToInt32(),
						RequiresTool = rdr["RequiresTool"].ToBool(),
						ProcessTimeType = rdr["ProcessTimeType"].IsNull() ? null : rdr["ProcessTimeType"].ToInt32(),
					};
					if (!string.IsNullOrEmpty(rdr["PositionCodes"].ToStr()))
					{
						element.Profiles.AddRange(rdr["PositionCodes"].ToStr().Split(','));
					}
					if (!string.IsNullOrEmpty(rdr["UpdateDate"].ToStr()))
					{
						element.ModifyDate = Convert.ToDateTime(rdr["UpdateDate"], CultureInfo.InvariantCulture);
						element.ModifiedBy = new User(rdr["UpdateUser"].ToInt32());
					}
					if (!string.IsNullOrEmpty(rdr["UserFields"].ToStr()))
					{
						element.UserFields = JsonConvert.DeserializeObject(rdr["UserFields"].ToStr());
					}

					returnValue.Add(element);
				}

				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					string Id = rdr["OperationTypeCode"].ToStr();
					element = returnValue.Find(x => x.Id == Id);
					if (!element.IsNull())
					{
						elem = new ProcessTypeDetail
						{
							Code = rdr["ParamCode"].ToStr(),
							Name = rdr["ParamName"].ToStr()
						};
						element.Details.Add(elem);
					}
				}

				rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					string Id = rdr["OperationTypeCode"].ToStr();
					element = returnValue.Find(x => x.Id == Id);
					if (!element.IsNull())
					{
						elemA = new Activity
						{
							Id = rdr["ActivityId"].ToStr(),
							Name = rdr["Name"].ToStr(),
							TriggerId = rdr["TriggerId"].ToInt32(),
							IsMandatory = rdr["IsMandatory"].ToBool(),
							SortId = rdr["SortId"].ToInt32(1),
							Origin = rdr["Origin"].ToStr(),
							Schedule = new ActivitySchedule
							{
								Duration = rdr["Duration"].ToDouble(),
								DurationUnit = rdr["DurationUnit"].ToInt32(),
								FrequencyMode = rdr["FreqMode"].ToStr(),
								FreqValue = rdr["FreqValue"].ToDouble()
							}
						};
						(element.Tasks ??= []).Add(elemA);
					}
				}

				if (rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						string Id = rdr["OperationTypeCode"].ToStr();
						element = returnValue.Find(x => x.Id == Id);
						if (!element.IsNull())
						{
							elemPT = new ProcessTypeAttribute
							{
								AttributeId = rdr["AttributeCode"].ToStr(),
								Name = rdr["Name"].ToStr(),
							};
							(element.Attributes ??= []).Add(elemPT);
						}
					}
				}

				if (rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						string Id = rdr["OperationTypeCode"].ToStr();
						string AttrType = rdr["AttributeCode"].ToStr();
						element = returnValue.Find(x => x.Id == Id);
						if (!element.IsNull())
						{
							attribute = element.Attributes.Find(x => x.AttributeId == AttrType);
							if (attribute is not null)
							{
								elemAttr = new ProcessTypeAttributeDetail
								{
									FromAttribute = rdr["FromAttributeValue"].ToStr(),
									ToAttribute = rdr["ToAttributeValue"].ToStr(),
									Time = rdr["Value"].ToDouble(),
								};
								(attribute.Details ??= []).Add(elemAttr);
							}
						}
					}
				}
				//SubTypes
				if (rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						string Id = rdr["OperationTypeCode"].ToStr();

						element = returnValue.Find(x => x.Id == Id);
						if (!element.IsNull())
						{
							ProcessTypeSubtype elemSubtype = new()
							{
								Code = rdr["Code"].ToStr(),
								Name = rdr["Name"].ToStr(),
								ProcessTypeId = Id
							};
							(element.SubTypes ??= []).Add(elemSubtype);
						}
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
	///
	/// </summary>
	public ResponseData SaveSubOperationTypes_Bulk(string paramsJSON, User systemOperator)
	{
		ResponseData returnValue = new();
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_operationsubtype_BLK", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_JSON", paramsJSON, !string.IsNullOrEmpty(paramsJSON), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Values"));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Operator"));
				command.Parameters.AddCondition("_OperatorEmployee", () => systemOperator.EmployeeId, systemOperator is not null);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				command.ExecuteNonQueryAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				returnValue = new ResponseData
				{
					IsSuccess = true
				};
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
	public List<ProcessTypeDetail> ListMachineProcessTypeDetails(string machineId)
	{
		List<ProcessTypeDetail> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_MACHINE_PROCESSDEFAULT_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_MachineCode", machineId);
				//Se requiere obtener la lista completa para no estar obteniendo registro por maquina ya que se realizan muchas peticiones a la bd
				// command.Parameters.AddCondition("_MachineId", machineId, !string.IsNullOrEmpty(machineId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Machine"));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ProcessTypeDetail element = new()
					{
						Code = rdr["ParameterCode"].ToStr(),
						ValueType = (ProcessTypeDetailSourceType)rdr["SourceType"].ToInt32(),
						ValueSourceId = rdr["SourceValue"].ToStr()
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

	
    #endregion ProcessType
}