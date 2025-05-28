using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

public class DataSyncRepository : IDataSyncRepository
{
	private readonly string ConnectionString;
	private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
	private readonly string ConnectionStringReports;
	private readonly string ConnectionStringLogs;

	private readonly string Database;

	public DataSyncRepository(IApplicationSettings applicationSettings)
	{
		ConnectionString = applicationSettings.GetConnectionString();
		ConnectionStringReports = applicationSettings.GetReportsConnectionString();
		ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
		Database = applicationSettings.GetDatabaseFromConnectionString();
	}



	public async Task<List<DataSyncService>> GetBackgroundService(string backgroundService, string httpMethod, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrEmpty(backgroundService, nameof(backgroundService));
		ArgumentException.ThrowIfNullOrEmpty(httpMethod, nameof(httpMethod));

		List<DataSyncService> returnValue = null;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_BackgroundService_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Service", backgroundService);
				command.Parameters.AddWithValue("_HttpMethod", httpMethod);

				try
				{
					// Execute reader asynchronously
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (rdr.HasRows)
						{
							// Get ordinals for column indices
							int idOrdinal = rdr.GetOrdinal("Id");
							int erpDataIdOrdinal = rdr.GetOrdinal("ErpDataId");
							int erpCodeOrdinal = rdr.GetOrdinal("ErpCode");
							int erpOrdinal = rdr.GetOrdinal("Erp");
							int erpVersionCodeOrdinal = rdr.GetOrdinal("ErpVersionCode");
							int erpVersionOrdinal = rdr.GetOrdinal("ErpVersion");
							int dbCodeOrdinal = rdr.GetOrdinal("DbCode");
							int dbOrdinal = rdr.GetOrdinal("Db");
							int dbVersionCodeOrdinal = rdr.GetOrdinal("DbVersionCode");
							int dbVersionOrdinal = rdr.GetOrdinal("DbVersion");
							int manufacturingCodeOrdinal = rdr.GetOrdinal("ManufacturingCode");
							int manufacturingOrdinal = rdr.GetOrdinal("Manufacturing");
							int manufacturingVersionCodeOrdinal = rdr.GetOrdinal("ManufacturingVersionCode");
							int manufacturingVersionOrdinal = rdr.GetOrdinal("ManufacturingVersion");
							int baseUrlOrdinal = rdr.GetOrdinal("BaseUrl");
							int tokenRequestJsonOrdinal = rdr.GetOrdinal("TokenRequestJson");
							int tokenRequestPathOrdinal = rdr.GetOrdinal("TokenRequestPath");
							int tokenRequestMapSchemaOrdinal = rdr.GetOrdinal("TokenRequestMapSchema");
							int tokenRequestResultPropOrdinal = rdr.GetOrdinal("TokenRequestResultProp");
							int requiresTokenRenewalOrdinal = rdr.GetOrdinal("RequiresTokenRenewal");
							int tokenRenewalMapSchemaOrdinal = rdr.GetOrdinal("TokenRenewalMapSchema");
							int requiredHeadersOrdinal = rdr.GetOrdinal("RequiredHeaders");
							int dateTimeFormatOrdinal = rdr.GetOrdinal("DateTimeFormat");
							int timeZoneOrdinal = rdr.GetOrdinal("TimeZone");
							int reprocessingTimeOrdinal = rdr.GetOrdinal("ReprocessingTime");
							int maxReprocessingTimeOrdinal = rdr.GetOrdinal("MaxReprocessingTime");
							int entityCodeOrdinal = rdr.GetOrdinal("EntityCode");
							int entityNameOrdinal = rdr.GetOrdinal("EntityName");
							int entityDescriptionOrdinal = rdr.GetOrdinal("EntityDescription");
							int entityClassExternalOrdinal = rdr.GetOrdinal("EntityClassExternal");
							int httpMethodOrdinal = rdr.GetOrdinal("HttpMethod");
							int responseMapSchemaOrdinal = rdr.GetOrdinal("ResponseMapSchema");
							int requestMapSchemaOrdinal = rdr.GetOrdinal("RequestMapSchema");
							int expectedResultOrdinal = rdr.GetOrdinal("ExpectedResult");
							int resultPropertyOrdinal = rdr.GetOrdinal("ResultProperty");
							int errorPropertyOrdinal = rdr.GetOrdinal("ErrorProperty");
							int tokenOrdinal = rdr.GetOrdinal("Token");
							int tokenTypeOrdinal = rdr.GetOrdinal("TokenType");
							int expirationTimeOrdinal = rdr.GetOrdinal("ExpirationTime");
							int expirationDateOrdinal = rdr.GetOrdinal("ExpirationDate");
							int pathOrdinal = rdr.GetOrdinal("Path");
							int urlParamsOrdinal = rdr.GetOrdinal("UrlParams");
							int singleRecordParamOrdinal = rdr.GetOrdinal("SingleRecordParam");
							int timeTriggerEnableOrdinal = rdr.GetOrdinal("TimeTriggerEnable");
							int frequencyMinOrdinal = rdr.GetOrdinal("FrequencyMin");
							int erpTriggerEnableOrdinal = rdr.GetOrdinal("ErpTriggerEnable");
							int sfTriggerEnableOrdinal = rdr.GetOrdinal("SfTriggerEnable");
							int manualSyncEnableOrdinal = rdr.GetOrdinal("ManualSyncEnable");
							int deltaSyncOrdinal = rdr.GetOrdinal("DeltaSync");
							int lastExecutionDateOrdinal = rdr.GetOrdinal("LastExecutionDate");
							int offsetMinOrdinal = rdr.GetOrdinal("OffsetMin");
							int requestTimeoutSecsOrdinal = rdr.GetOrdinal("RequestTimeoutSecs");
							int requestReprocMaxRetriesOrdinal = rdr.GetOrdinal("RequestReprocMaxRetries");
							int requestReprocFrequencySecsOrdinal = rdr.GetOrdinal("RequestReprocFrequencySecs");
							int revalidationEnableOrdinal = rdr.GetOrdinal("RevalidationEnable");
							int creationDateOrdinal = rdr.GetOrdinal("CreateDate");
							int createUserOrdinal = rdr.GetOrdinal("CreateUser");
							int statusOrdinal = rdr.GetOrdinal("Status");
							int enableDynamicBodyOrdinal = rdr.GetOrdinal("EnableDynamicBody");
							int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
							int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");

							while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								DataSyncService element = new()
								{
									Id = rdr[idOrdinal].ToStr(),
									ErpDataId = rdr[erpDataIdOrdinal].ToStr(),
									ErpData = new DataSyncErp
									{
										Id = rdr[erpDataIdOrdinal].ToStr(),
										ErpCode = rdr[erpCodeOrdinal].ToStr(),
										Erp = rdr[erpOrdinal].ToStr(),
										ErpVersionCode = rdr[erpVersionCodeOrdinal].ToStr(),
										ErpVersion = rdr[erpVersionOrdinal].ToStr(),
										DbCode = rdr[dbCodeOrdinal].ToStr(),
										Db = rdr[dbOrdinal].ToStr(),
										DbVersionCode = rdr[dbVersionCodeOrdinal].ToStr(),
										DbVersion = rdr[dbVersionOrdinal].ToStr(),
										ManufacturingCode = rdr[manufacturingCodeOrdinal].ToStr(),
										Manufacturing = rdr[manufacturingOrdinal].ToStr(),
										ManufacturingVersionCode = rdr[manufacturingVersionCodeOrdinal].ToStr(),
										ManufacturingVersion = rdr[manufacturingVersionOrdinal].ToStr(),
										BaseUrl = rdr[baseUrlOrdinal].ToStr(),
										TokenRequestJson = rdr[tokenRequestJsonOrdinal].ToStr(),
										TokenRequestPath = rdr[tokenRequestPathOrdinal].ToStr(),
										TokenRequestMapSchema = rdr[tokenRequestMapSchemaOrdinal].ToStr(),
										TokenRequestResultProp = rdr[tokenRequestResultPropOrdinal].ToStr(),
										RequiresTokenRenewal = (EnableType)rdr[requiresTokenRenewalOrdinal].ToInt32(),
										TokenRenewalMapSchema = rdr[tokenRenewalMapSchemaOrdinal].ToStr(),
										RequiredHeaders = rdr[requiredHeadersOrdinal].ToStr(),
										DateTimeFormat = (DateTimeFormatType)rdr[dateTimeFormatOrdinal].ToInt32(),
										TimeZone = rdr[timeZoneOrdinal].ToStr(),
										ReprocessingTime = rdr[reprocessingTimeOrdinal].ToInt32(),
										MaxReprocessingTime = rdr[maxReprocessingTimeOrdinal].ToInt32()
									},
									EntityId = rdr[entityCodeOrdinal].ToStr(),
									Entity = new Entity
									{
										Id = rdr[entityCodeOrdinal].ToStr(),
										Name = rdr[entityNameOrdinal].ToStr(),
										Description = rdr[entityDescriptionOrdinal].ToStr(),
										NameEntityExternal = rdr[entityClassExternalOrdinal].ToStr(),
									},
									EntityCategoryCode = rdr[entityCodeOrdinal].ToStr(),
									ErpMapping = new DataSyncErpMapping
									{
										ErpId = rdr[erpDataIdOrdinal].ToStr(),
										Id = rdr[entityCodeOrdinal].ToStr(),
										HttpMethod = rdr[httpMethodOrdinal].ToStr(),
										ResponseMapSchema = rdr[responseMapSchemaOrdinal].ToStr(),
										RequestMapSchema = rdr[requestMapSchemaOrdinal].ToStr(),
										ExpectedResult = rdr[expectedResultOrdinal].ToStr(),
										ResultProperty = rdr[resultPropertyOrdinal].ToStr(),
										ErrorProperty = rdr[errorPropertyOrdinal].ToStr()
									},
									TokenData = new DataSyncErpAuth
									{
										ErpId = rdr[erpDataIdOrdinal].ToStr(),
										Token = rdr[tokenOrdinal].ToStr(),
										TokenType = rdr[tokenTypeOrdinal].ToStr(),
										ExpirationTime = rdr[expirationTimeOrdinal].ToInt32(),
										ExpirationDate = rdr[expirationDateOrdinal].ToDate()
									},
									Path = rdr[pathOrdinal].ToStr(),
									UrlParams = rdr[urlParamsOrdinal].ToStr(),
									SingleRecordParam = rdr[singleRecordParamOrdinal].ToStr(),
									HttpMethod = rdr[httpMethodOrdinal].ToStr(),
									TimeTriggerEnable = (EnableType)rdr[timeTriggerEnableOrdinal].ToInt32(),
									FrequencyMin = rdr[frequencyMinOrdinal].ToInt32(),
									ErpTriggerEnable = (EnableType)rdr[erpTriggerEnableOrdinal].ToInt32(),
									SfTriggerEnable = (EnableType)rdr[sfTriggerEnableOrdinal].ToInt32(),
									ManualSyncEnable = (EnableType)rdr[manualSyncEnableOrdinal].ToInt32(),
									DeltaSync = (EnableType)rdr[deltaSyncOrdinal].ToInt32(),
									LastExecutionDate = rdr[lastExecutionDateOrdinal].ToDate(),
									OffsetMin = rdr[offsetMinOrdinal].ToInt32(),
									RequestTimeoutSecs = rdr[requestTimeoutSecsOrdinal].ToInt32(),
									RequestReprocMaxRetries = rdr[requestReprocMaxRetriesOrdinal].ToInt32(),
									RequestReprocFrequencySecs = rdr[requestReprocFrequencySecsOrdinal].ToInt32(),
									RevalidationEnable = (EnableType)rdr[revalidationEnableOrdinal].ToInt32(),
									CreationDate = rdr[creationDateOrdinal].ToDate(),
									CreatedBy = new User(rdr[createUserOrdinal].ToInt32()),
									Status = (ServiceStatus)rdr[statusOrdinal].ToInt32(),
									EnableDynamicBody = rdr[enableDynamicBodyOrdinal].ToInt32()
								};

								if (rdr[updateDateOrdinal].ToDate().Year > 1900)
								{
									element.ModifyDate = rdr[updateDateOrdinal].ToDate();
									element.ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32());
								}

								(returnValue ??= []).Add(element);
							}
						}
						else
						{
							returnValue = [];
						}
					}
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					// logger?.Error(ex);
					throw;
				}
				finally
				{
					await connection.CloseAsync().ConfigureAwait(false);
				}

				return returnValue;
			}
		}
	}



	public async Task<DataSyncServiceLog> GetDataSyncServiceLogs(string LogId, int logType = 0, CancellationToken cancellationToken = default)
	{
		DataSyncServiceLog returnValue = new();

		await using EWP_Connection connection = new(ConnectionStringLogs);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_DataSync_Log_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Database", Database);
				command.Parameters.AddWithValue("_LogId", LogId);

				if (logType > 0)
				{
					command.Parameters.AddWithValue("_LogType", logType);
				}
				else
				{
					command.Parameters.AddNull("_LogType");
				}

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						if (rdr.HasRows)
						{
							int idOrdinal = rdr.GetOrdinal("Id");
							int serviceInstanceIdOrdinal = rdr.GetOrdinal("ServiceInstanceId");
							int executionInitDateOrdinal = rdr.GetOrdinal("ExecutionInitDate");
							int sfProcessDateOrdinal = rdr.GetOrdinal("SfProcessDate");
							int executionFinishDateOrdinal = rdr.GetOrdinal("ExecutionFinishDate");
							int failedRecordsOrdinal = rdr.GetOrdinal("FailedRecords");
							int successRecordsOrdinal = rdr.GetOrdinal("SuccessRecords");
							int erpReceivedJsonOrdinal = rdr.GetOrdinal("ErpReceivedJson");
							int sfMappedJsonOrdinal = rdr.GetOrdinal("SfMappedJson");
							int sfResponseJsonOrdinal = rdr.GetOrdinal("SfResponseJson");
							int serviceExceptionOrdinal = rdr.GetOrdinal("ServiceException");
							int executionOriginOrdinal = rdr.GetOrdinal("ExecutionOrigin");

							while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								returnValue = new DataSyncServiceLog
								{
									Id = rdr[idOrdinal].ToStr(),
									ServiceInstanceId = rdr[serviceInstanceIdOrdinal].ToStr(),
									ExecutionInitDate = rdr[executionInitDateOrdinal].ToDate(),
									SfProcessDate = rdr[sfProcessDateOrdinal].ToDate(),
									ExecutionFinishDate = rdr[executionFinishDateOrdinal].ToDate(),
									FailedRecords = rdr[failedRecordsOrdinal].ToInt32(),
									SuccessRecords = rdr[successRecordsOrdinal].ToInt32(),
									ErpReceivedJson = rdr[erpReceivedJsonOrdinal].ToStr(),
									SfMappedJson = rdr[sfMappedJsonOrdinal].ToStr(),
									SfResponseJson = rdr[sfResponseJsonOrdinal].ToStr(),
									ServiceException = rdr[serviceExceptionOrdinal].ToStr(),
									ExecutionOrigin = (ServiceExecOrigin)rdr[executionOriginOrdinal].ToInt32(),
								};
							}

							if (await rdr.NextResultAsync(cancellationToken).ConfigureAwait(false) && returnValue is not null)
							{
								int idDetailOrdinal = rdr.GetOrdinal("Id");
								int logIdDetailOrdinal = rdr.GetOrdinal("LogId");
								int rowKeyDetailOrdinal = rdr.GetOrdinal("RowKey");
								int processDateDetailOrdinal = rdr.GetOrdinal("ProcessDate");
								int erpReceivedJsonDetailOrdinal = rdr.GetOrdinal("ErpReceivedJson");
								int sfMappedJsonDetailOrdinal = rdr.GetOrdinal("SfMappedJson");
								int responseJsonDetailOrdinal = rdr.GetOrdinal("ResponseJson");
								int messageExceptionDetailOrdinal = rdr.GetOrdinal("MessageException");
								int attemptNoDetailOrdinal = rdr.GetOrdinal("AttemptNo");
								int lastAttemptDateDetailOrdinal = rdr.GetOrdinal("LastAttemptDate");
								int logTypeDetailOrdinal = rdr.GetOrdinal("LogType");

								returnValue.Details = [];
								while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
								{
									DataSyncServiceLogDetail element = new()
									{
										Id = rdr[idDetailOrdinal].ToStr(),
										LogId = rdr[logIdDetailOrdinal].ToStr(),
										RowKey = rdr[rowKeyDetailOrdinal].ToStr(),
										ProcessDate = rdr[processDateDetailOrdinal].ToDate(),
										ErpReceivedJson = rdr[erpReceivedJsonDetailOrdinal].ToStr(),
										SfMappedJson = rdr[sfMappedJsonDetailOrdinal].ToStr(),
										ResponseJson = rdr[responseJsonDetailOrdinal].ToStr(),
										MessageException = rdr[messageExceptionDetailOrdinal].ToStr(),
										AttemptNo = rdr[attemptNoDetailOrdinal].ToInt32(),
										LastAttemptDate = rdr[lastAttemptDateDetailOrdinal].ToDate(),
										LogType = (DataSyncLogType)rdr[logTypeDetailOrdinal].ToInt32(),
									};
									returnValue.Details.Add(element);
								}
							}
						}
						else
						{
							returnValue = new DataSyncServiceLog();
						}
					}
				}
				catch (Exception ex)
				{
					// Log the exception if a logger is available
					// logger?.Error(ex);
					throw;
				}
			}
		}

		return returnValue;
	}




	public List<TimeZoneCatalog> GetTimezones(bool currentValues)
	{
		List<TimeZoneCatalog> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Erp_Timezone_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				_ = command.Parameters.AddWithValue("_Current", currentValues);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						TimeZoneCatalog element = new()
						{
							Key = rdr["Label"].ToStr(),
							Value = rdr["Value"].ToStr(),
							Offset = rdr["Offset"].ToDouble()
						};
						(returnValue ??= []).Add(element);
					}
				}
				else
				{
					returnValue = [];
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

	public string GetDatasyncDynamicBody(string EntityCode)
	{
		string returnValue = string.Empty;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Datasync_DynamicBody_GET", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_EntityCode", EntityCode);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				returnValue = command.ExecuteScalarAsync().ConfigureAwait(false).GetAwaiter().GetResult().ToStr();
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

	public bool UpdateDataSyncServiceExecution(string Id, DateTime ExecutionDate)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Entities_Instance_Exec_UPD", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Id", Id, !string.IsNullOrEmpty(Id), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Service Instance Id"));
				_ = command.Parameters.AddWithValue("_ExecutionDate", ExecutionDate);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public bool InsertDataSyncServiceErpToken(DataSyncErpAuth TokenInfo)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Erp_Token_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_ErpId", TokenInfo.ErpId);
				_ = command.Parameters.AddWithValue("_Token", TokenInfo.Token);
				_ = command.Parameters.AddWithValue("_TokenType", TokenInfo.TokenType);
				_ = command.Parameters.AddWithValue("_ExpirationTime", TokenInfo.ExpirationTime);
				_ = command.Parameters.AddWithValue("_ExpirationDate", TokenInfo.ExpirationDate);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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

	public DataSyncErpAuth GetDataSyncServiceErpToken(string ErpCode)
	{
		DataSyncErpAuth returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Erp_Token_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				_ = command.Parameters.AddWithValue("_ErpId", ErpCode);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new DataSyncErpAuth
					{
						ErpId = rdr["ErpId"].ToStr(),
						Token = rdr["Token"].ToStr(),
						TokenType = rdr["TokenType"].ToStr(),
						ExpirationTime = rdr["ExpirationTime"].ToInt32(),
						ExpirationDate = rdr["ExpirationDate"].ToDate(),
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

	public ResponseData MergeComponentBulk(string Json, User systemOperator, bool Validation)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Item_BLK", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_JSON", Json, !string.IsNullOrEmpty(Json));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

				// command.Parameters.AddWithValue("_Level", Level);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				int ActionOrdinal = rdr.GetOrdinal("Action");
				int IsSuccessOrdinal = rdr.GetOrdinal("IsSuccess");
				int CodeOrdinal = rdr.GetOrdinal("Code");
				int MessageOrdinal = rdr.GetOrdinal("Message");
				int IdOrdinal = rdr.GetOrdinal("Id");

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ResponseData
					{
						Action = (ActionDB)rdr[ActionOrdinal].ToInt32(),
						IsSuccess = rdr[IsSuccessOrdinal].ToInt32().ToBool(),
						Code = rdr[CodeOrdinal].ToStr(),
						Message = rdr[MessageOrdinal].ToStr(),
						Id = rdr[IdOrdinal].ToStr()
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
	public List<Inventory> ListInventory(string Code = "", DateTime? DeltaDate = null)
	{
		List<Inventory> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ItemGroup_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Code", Code, !string.IsNullOrEmpty(Code));
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Inventory element = new()
					{
						InventoryId = rdr["Code"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = (Status)rdr["Status"].ToInt32(),
						Image = rdr["Image"].ToStr(),
						CreationDate = rdr["CreateDate"].ToDate(),
						CreatedBy = new User(rdr["CreateUser"].ToInt32()),
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
	public async Task<List<Component>> ListComponents(string componentId, bool ignoreImages = false, string filter = "", DateTime? DeltaDate = null, CancellationToken cancellationToken = default)
	{
		List<Component> returnValue = [];

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			await using EWP_Command command = new("SP_SF_Item_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = 200
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.AddCondition("_Item", componentId, !string.IsNullOrEmpty(componentId));
				command.Parameters.AddCondition("_Filter", filter, !string.IsNullOrEmpty(filter));
				command.Parameters.AddNull("_Warehouse");
				command.Parameters.AddNull("_Version");
				command.Parameters.AddNull("_Sequence");
				command.Parameters.AddCondition("_DeltaDate", DeltaDate, DeltaDate.HasValue);

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

					await using (rdr.ConfigureAwait(false))
					{
						// Declare GetOrdinal variables for better performance
						int codeOrdinal = rdr.GetOrdinal("Code");
						int nameOrdinal = rdr.GetOrdinal("Name");
						int itemTypeOrdinal = rdr.GetOrdinal("ItemType");
						int unitTypeOrdinal = rdr.GetOrdinal("UnitType");
						int unitIdOrdinal = rdr.GetOrdinal("UnitId");
						int createDateOrdinal = rdr.GetOrdinal("CreateDate");
						int createUserOrdinal = rdr.GetOrdinal("CreateUser");
						int statusOrdinal = rdr.GetOrdinal("Status");
						int stockTypeOrdinal = rdr.GetOrdinal("StockType");
						int makerOrdinal = rdr.GetOrdinal("Maker");
						int itemGroupCodeOrdinal = rdr.GetOrdinal("ItemGroupCode");
						int managedByOrdinal = rdr.GetOrdinal("ManagedBy");
						int managedByNameOrdinal = rdr.GetOrdinal("ManagedByName");
						int inventoryUnitOrdinal = rdr.GetOrdinal("InventoryUnit");
						int productionUnitOrdinal = rdr.GetOrdinal("ProductionUnit");
						int supplyLeadTimeOrdinal = rdr.GetOrdinal("SupplyLeadTime");
						int safetyQtyOrdinal = rdr.GetOrdinal("SafetyQty");
						int enableScheduleOrdinal = rdr.GetOrdinal("EnableSchedule");
						int enableProductSyncOrdinal = rdr.GetOrdinal("EnableProductSync");
						int logDetailIdOrdinal = rdr.GetOrdinal("LogDetailId");
						int isStockOrdinal = rdr.GetOrdinal("IsStock");
						int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
						int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");
						int imageOrdinal = rdr.GetOrdinal("Image");
						while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
						{
							Component element = new()
							{
								Id = rdr[codeOrdinal].ToStr(),
								ExternalId = rdr[codeOrdinal].ToStr(),
								Code = rdr[codeOrdinal].ToStr(),
								Name = rdr[nameOrdinal].ToStr(),
								ComponentType = (ComponentType)rdr[itemTypeOrdinal].ToInt32(),
								UnitType = (UnitType)rdr[unitTypeOrdinal].ToInt32(),
								UnitId = rdr[unitIdOrdinal].ToStr(),
								CreationDate = !await rdr.IsDBNullAsync(createDateOrdinal, cancellationToken).ConfigureAwait(false) ? rdr[createDateOrdinal].ToDate() : default,
								CreatedBy = new User(rdr[createUserOrdinal].ToInt32()),
								Status = (Status)rdr[statusOrdinal].ToInt32(),
								Type = rdr[stockTypeOrdinal].ToInt32(),
								Maker = rdr[makerOrdinal].ToStr(),
								InventoryId = rdr[itemGroupCodeOrdinal].ToStr(),
								ManagedBy = rdr[managedByOrdinal].ToInt32(),
								ManagedByName = rdr[managedByNameOrdinal].ToStr(),
								UnitInventory = rdr[inventoryUnitOrdinal].ToStr(),
								UnitProduction = rdr[productionUnitOrdinal].ToStr(),
								SupplyLeadTime = rdr[supplyLeadTimeOrdinal].ToInt32(),
								SafetyQty = rdr[safetyQtyOrdinal].ToDouble(),
								Schedule = rdr[enableScheduleOrdinal].ToInt32(),
								ProductSync = rdr[enableProductSyncOrdinal].ToBool(),
								LogDetailId = rdr[logDetailIdOrdinal].ToStr(),
								IsStock = rdr[isStockOrdinal].ToBool()
							};

							if (!ignoreImages)
							{
								element.Image = rdr[imageOrdinal].ToStr();
							}

							if (rdr[updateDateOrdinal].ToDate().Year > 1900)
							{
								element.ModifyDate = !await rdr.IsDBNullAsync(updateDateOrdinal, cancellationToken).ConfigureAwait(false) ? rdr[updateDateOrdinal].ToDate() : default;
								element.ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32());
							}

							returnValue.Add(element);
						}
					}
				}
				catch (Exception ex)
				{
					//logger.Error(ex, "An error occurred while fetching components");
					throw;
				}

				return returnValue;
			}
		}
	}
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

	public async Task<User> GetUser(int userId, string userHash, User systemOperator, CancellationToken cancellationToken = default)
	{
		User returnValue = null;

		await using EWP_Connection connection = new(ConnectionString);
		await using (connection.ConfigureAwait(false))
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

			EWP_Command command = new("SP_SF_User_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};

			await using (command.ConfigureAwait(false))
			{
				command.Parameters.AddCondition("_Operator", systemOperator?.Id, !systemOperator.IsNull(), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User Id"));
				_ = command.Parameters.AddWithValue("_Id", userId != -99 ? userId : null);
				_ = command.Parameters.AddWithValue("_UserHash", !string.IsNullOrEmpty(userHash) ? userHash : null);
				command.Parameters.AddNull("_Code");
				command.Parameters.AddNull("_PermissionCode");
				command.Parameters.AddNull("_DeltaDate");

				try
				{
					MySqlDataReader rdr = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
					await using (rdr.ConfigureAwait(false))
					{
						// Declare ordinals for performance optimization
						int hashOrdinal = rdr.GetOrdinal("Hash");
						int idOrdinal = rdr.GetOrdinal("Id");
						int usernameOrdinal = rdr.GetOrdinal("Code");
						int displayNameOrdinal = rdr.GetOrdinal("Name");
						int lastLoginDateOrdinal = rdr.GetOrdinal("LastLoginDate");
						int emailOrdinal = rdr.GetOrdinal("email");
						int langCodeOrdinal = rdr.GetOrdinal("LangCode");
						int statusOrdinal = rdr.GetOrdinal("Status");
						int defaultLayoutOrdinal = rdr.GetOrdinal("DefaultLayoutId");
						int createUserOrdinal = rdr.GetOrdinal("CreateUser");
						int createUserNameOrdinal = rdr.GetOrdinal("CreateUserName");
						int createDateOrdinal = rdr.GetOrdinal("CreateDate");
						int updateDateOrdinal = rdr.GetOrdinal("UpdateDate");
						int updateUserOrdinal = rdr.GetOrdinal("UpdateUser");
						int updateUserNameOrdinal = rdr.GetOrdinal("UpdateUserName");
						int userTypeCodeOrdinal = rdr.GetOrdinal("UserTypeCode");
						int assetCodesOrdinal = rdr.GetOrdinal("AssetCodes");
						int printingStationOrdinal = rdr.GetOrdinal("PrintingStation");
						int printingMachineOrdinal = rdr.GetOrdinal("PrintingMachine");
						int employeeCodeOrdinal = rdr.GetOrdinal("EmployeeCode");
						int ipAddressOrdinal = rdr.GetOrdinal("IpAddress");
						int attendanceControlOrdinal = rdr.GetOrdinal("AttendanceControl");
						int executionControlOrdinal = rdr.GetOrdinal("ExecutionControl");
						int SecretAuthOrdinal = rdr.GetOrdinal("SecretAuth");
						int usedMultiFactorOrdinal = rdr.GetOrdinal("UsedMultiFactor");
						int temporalPasswordOrdinal = rdr.GetOrdinal("TemporalPassword");
						int defaultMenuIdOrdinal = rdr.GetOrdinal("DefaultMenuId");

						// First result set - User information
						if (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
						{
							returnValue = new User
							{
								Hash = rdr[hashOrdinal].ToStr(),
								Id = rdr[idOrdinal].ToInt32(),
								Username = rdr[usernameOrdinal].ToStr(),
								DisplayName = rdr[displayNameOrdinal].ToStr(),
								LastLoginDate = rdr[lastLoginDateOrdinal].ToDate(),
								Email = rdr[emailOrdinal].ToStr(),
								LanguageCode = rdr[langCodeOrdinal].ToStr(),
								Status = (Status)rdr[statusOrdinal].ToInt32(),
								DefaultLayout = rdr[defaultLayoutOrdinal].ToInt32(),
								CreatedBy = new User(rdr[createUserOrdinal].ToInt32())
								{
									DisplayName = rdr[createUserNameOrdinal].ToStr(),
								},
								CreationDate = rdr[createDateOrdinal].ToDate(),
								ModificationDate = rdr[updateDateOrdinal].ToDate(),
								ModifiedBy = new User(rdr[updateUserOrdinal].ToInt32())
								{
									DisplayName = rdr[updateUserNameOrdinal].ToStr()
								},
								UserTypeCode = rdr[userTypeCodeOrdinal].ToStr(),
								WorkCenterId = rdr[assetCodesOrdinal].ToStr(),
								PrintingStation = rdr[printingStationOrdinal].ToStr(),
								PrintingMachine = rdr[printingMachineOrdinal].ToStr(),
								EmployeeId = rdr[employeeCodeOrdinal].ToStr(),
								IP = rdr[ipAddressOrdinal].ToStr(),
								AttendanceControl = rdr[attendanceControlOrdinal].ToBool(),
								ExecutionControl = rdr[executionControlOrdinal].ToBool(),
								SecretKey = rdr[SecretAuthOrdinal].ToStr(),
								TemporalPassword = rdr[temporalPasswordOrdinal].ToBool(),
								DefaultMenuId = rdr[defaultMenuIdOrdinal] != DBNull.Value ? rdr[defaultMenuIdOrdinal].ToInt32() : null
							};
						}

						// Move to the next result set - Permissions
						if (await rdr.NextResultAsync(cancellationToken).ConfigureAwait(false))
						{
							int permissionCodeOrdinal = rdr.GetOrdinal("PermissionCode");
							int permissionNameOrdinal = rdr.GetOrdinal("Permission");
							int roleIdOrdinal = rdr.GetOrdinal("RoleId");
							int roleNameOrdinal = rdr.GetOrdinal("Role");

							while (await rdr.ReadAsync(cancellationToken).ConfigureAwait(false))
							{
								Permission permission = new()
								{
									Code = rdr[permissionCodeOrdinal].ToStr(),
									Name = rdr[permissionNameOrdinal].ToStr(),
									Id = rdr[permissionCodeOrdinal].ToStr(),
									Role = new Role(rdr[roleIdOrdinal].ToInt32())
									{
										Name = rdr[roleNameOrdinal].ToStr()
									}
								};

								(returnValue.Permissions ??= []).Add(permission);

								if (returnValue.Role is null || returnValue.Role.Id != permission.Role.Id)
								{
									returnValue.Role = (Role)permission.Role.Clone();
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					// logger.Error(ex, "An error occurred while fetching user data");
					// throw;
				}

				return returnValue;
			}
		}
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

	

	#region 
	public bool ActivityItemInsByXML(User systemOperator, string xmlComponents)
	{
		bool returnValue = false;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_activity_item_Ins", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.CommandTimeout = 30000;
				_ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
				_ = command.Parameters.AddWithValue("_XmlComponents", xmlComponents);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					//returnValue = new ResponseData()
					//{
					//    Id = rdr["Id"].ToStr(),
					//    Action = (ActionDB)rdr["Action"].ToInt32(),
					//    IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
					//    Code = rdr["Code"].ToStr(),
					//    Version = rdr["Version"].ToInt32(),
					//    Message = rdr["Message"].ToStr(),

					//};
				}
				returnValue = true;
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				returnValue = false;
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return returnValue;
	}
	public ResponseData ProcessMasterInsByXML(Procedure procesInfo
	, string xmlSections
	, string xmlInstructions
	, string xmlChoice
	, string xmlRange
	, string xmlActionCheckBoxs
	//  , string xmlMultipleChoiceCheckBox
	// , string xmlActionOperators
	, User systemOperator
	, string xmlComponents
	, string xmlAtachments
	, bool IsValidation = false)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Procedure_Instruction_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.CommandTimeout = 30000;
				command.Parameters.AddCondition("_ProcedureId", procesInfo.ProcedureId, procesInfo is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Process Id"));
				// command.Parameters.AddWithValue("_ActivityId", procesInfo.ActivityId);
				_ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
				_ = command.Parameters.AddWithValue("_XMLSections", xmlSections);
				_ = command.Parameters.AddWithValue("_XMLInstructions", xmlInstructions);
				_ = command.Parameters.AddWithValue("_XMLChoice", xmlChoice);
				_ = command.Parameters.AddWithValue("_XMLRange", xmlRange);
				_ = command.Parameters.AddWithValue("_XMLActionCheckBoxs", xmlActionCheckBoxs);
				_ = command.Parameters.AddWithValue("_XmlComponents", xmlComponents);
				_ = command.Parameters.AddWithValue("_XmlAtachments", xmlAtachments);
				_ = command.Parameters.AddWithValue("_IsValidation", IsValidation);

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
						Version = rdr["Version"].ToInt32(),
						Message = rdr["Message"].ToStr(),
					};
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				returnValue = null;
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return returnValue;
	}
	public Procedure GetProcedure(string ProcedureId, string ActivityId = null, string Instance = null)

	{
		Procedure returnValue = null;

		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ProcedureById_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				List<ProcedureSection> listSection = [];
				List<ProcedureMasterInstruction> listInstruction = [];
				List<Choice> listTempChoice = [];
				List<Choice> listTempChoiceCheckBox = [];
				List<ActionOperator> listMultipleActionOperator = [];
				List<ComponentInstruction> listComponent = [];
				List<ActionChoiceDB> listTempActionsCheckBox = [];
				List<Range> listTempRange = [];

				ProcedureMasterInstruction elementInstruccion;
				_ = command.Parameters.AddWithValue("_ProcedureId", ProcedureId);
				_ = command.Parameters.AddWithValue("_ActivityId", ActivityId);
				_ = command.Parameters.AddWithValue("_Instnace", Instance);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new()
					{
						ProcedureId = rdr["Id"].ToStr(),
						ProcedureIdOrigin = rdr["ProcedureIdOrigin"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = rdr["Status"].ToInt32(),
						StatusDescription = rdr["StatusDescription"].ToStr(),
						Version = rdr["Version"].ToInt32(),
						EarlierVersion = rdr["EarlierVersion"].ToInt32(),
						IdActivityClass = rdr["IdActivityClass"].ToInt32(),
						InterventionId = rdr["InterventionId"].ToStr(),
						SourceId = rdr["SourceId"].ToStr(),
						HasIntervention = rdr["HasIntervention"].ToInt32().ToBool(),
						HasSource = rdr["HasSource"].ToInt32().ToBool(),
						ActivityType = rdr["ActivityType"].ToStr(),
						ClassName = rdr["ClassName"].ToStr(),
						IdTypeClass = rdr["ActivityTypeCode"].ToStr(),
						Description = rdr["Description"].ToStr(),
						IsByProcedure = rdr["IsByProcedure"].ToInt32().ToBool(),
						IsManualActivity = rdr["IsManualActivity"].ToInt32().ToBool(),
						Layout = JsonSerializer.Deserialize<List<LayoutProcedure>>(rdr["Layout"].ToStr()),
					};
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ProcedureSection element = new()
					{
						SectionId = rdr["Id"].ToStr(),
						ProcedureId = rdr["ProcedureId"].ToStr(),
						TypeSection = rdr["TypeSection"].ToInt32(),
						SectionType = rdr["SectionType"].ToStr(),
						Section = rdr["Name"].ToStr(),
						Description = rdr["Description"].ToStr(),
						Observations = rdr["Observations"].ToStr(),
						Status = rdr["Status"].ToInt32(),
						OrderSection = rdr["OrderSection"].ToInt32(),
						AttachmentId = rdr["AttachmentId"].ToStr()
					};
					element.Attachment = GetAttachmentSection(element.AttachmentId, element.SectionId);
					listSection.Add(element);
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					elementInstruccion = new ProcedureMasterInstruction
					{
						InstructionId = rdr["Id"].ToStr(),
						ProcessId = rdr["ProcedureId"].ToStr(),
						CodeInstruction = rdr["CodeInstruction"].ToInt32(),
						SectionId = rdr["SectionId"].ToStr(),
						Instruction = rdr["Description"].ToStr(),
						InstructionDisplayTitle = rdr["InstructionDisplayTitle"].ToStr(),
						TypeInstrucction = rdr["TypeInstrucction"].ToStr(),
						ViewType = rdr["ViewType"].ToInt32(),
						MultiSelect = rdr["MultiSelect"].ToInt32().ToBool(),
						Mandatory = rdr["Mandatory"].ToBool(),
						IsGauge = rdr["IsGauge"].ToBool(),
						Long = rdr["Length"].ToInt32(),
						Type = rdr["Type"].ToInt32(),
						IsDecimal = rdr["IsDecimal"].ToInt32().ToBool(),
						TypeDataReading = rdr["TypeDataReading"].ToInt32(),
						TimeInSec = rdr["Time"].ToInt64(),
						DefaultValue = rdr["DefaultValue"].ToStr(),
						MinValue = rdr["MinValue"].ToDecimal(),
						MaxValue = rdr["MaxValue"].ToDecimal(),
						TargetValue = rdr["TargetValue"].ToDecimal(),
						CodeAutomatic = rdr["CodeAutomatic"].ToStr(),
						SignalCode = rdr["SignalCode"].ToString(),
						URLInstrucction = rdr["URLInstrucction"].ToString(),
						QueryUser = rdr["Query"].ToString(),
						Response = rdr["ResponseValue"].ToString()
					};
					listInstruction.Add(elementInstruccion);
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Range elementRange = new()
					{
						Id = rdr["Id"].ToStr(),
						InstructionId = rdr["InstructionId"].ToStr(),
						SectionId = rdr["SectionId"].ToStr(),
						Message = rdr["Message"].ToStr(),
						Min = rdr["Min"].ToDecimal(),
						Max = rdr["Max"].ToDecimal(),
						OrderChoice = rdr["SortId"].ToInt32(),
						IsNotify = rdr["IsNotify"].ToInt32().ToBool(),
						MessageNotify = rdr["MessageNotify"].ToStr(),
					};
					listTempRange.Add(elementRange);
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ActionChoiceDB elementChoice = new()
					{
						Id = rdr["Id"].ToString(),
						InstructionId = rdr["InstructionId"].ToString(),
						ValueChoice = rdr["ValueChoice"].ToStr(),
						SectionId = rdr["SectionId"].ToStr(),
						Message = rdr["Message"].ToStr(),
						MessageNotify = rdr["MessageNotify"].ToStr(),
						IsNotify = rdr["IsNotify"].ToInt32().ToBool(),
						OrderChoice = rdr["SortId"].ToInt32()
					};
					listTempActionsCheckBox.Add(elementChoice);
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Choice elementChoice = new()
					{
						Id = rdr["Id"].ToString(),
						InstructionId = rdr["InstructionId"].ToString(),
						SectionId = rdr["SectionId"].ToStr(),
						ValueChoice = rdr["Description"].ToStr(),
						Message = rdr["Message"].ToStr(),
						MessageNotify = rdr["MessageNotify"].ToStr(),
						IsNotify = rdr["IsNotify"].ToInt32().ToBool(),
						OrderChoice = rdr["SortId"].ToInt32(),
						AttachmentId = rdr["AttachmentId"].ToStr()
					};
					elementChoice.Attachment = GetAttachmentSection(elementChoice.AttachmentId, elementChoice.Id);
					elementChoice.Selected = rdr["Selected"].ToInt32().ToBool();

					listTempChoice.Add(elementChoice);
				}
				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ComponentInstruction element = new()
					{
						// Attachment = new ProcedureMasterSectionAttachment(),
						Id = rdr["Id"].ToString(),
						InstructionId = rdr["InstructionId"].ToString(),
						Code = rdr["Code"].ToStr(),
						CodeSignal = rdr["CodeSignal"].ToStr(),
						ComponentId = rdr["ComponentId"].ToStr(),
						QuantityIssue = rdr["QuantityIssue"].ToDecimal(),
						QuantityAvailable = rdr["QuantityAvailable"].ToDecimal(),
						MaterialImage = rdr["Image"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Line = rdr["SortId"].ToStr(),
						Method = rdr["Method"].ToStr(),
						ProcedureId = returnValue.ProcedureId,// rdr["ProcedureId"].ToStr(),
						Quantity = rdr["Quantity"].ToDecimal(),
						Quantitytext = rdr["Quantitytext"].ToStr(),
						Tolerance = rdr["Tolerance"].ToDecimal(),
						UnitId = rdr["UnitId"].ToStr(),
						UnitType = rdr["UnitType"].ToStr(),
						Mandatory = rdr["Mandatory"].ToInt32().ToBool(),
						TypeComponent = rdr["TypeComponent"].ToString(),
						IsSubProduct = rdr["IsSubProduct"].ToInt32().ToBool(),
						IsRemainingTotal = rdr["IsRemainingTotal"].ToInt32().ToBool(),
						AttachmentId = rdr["IdAttachment"].ToString(),
					};
					listComponent.Add(element);
				}

				if (listSection is not null)
				{
					foreach (ProcedureSection section in listSection)
					{
						section.ListInstrucctions = [];
						section.ListInstrucctions = [.. listInstruction.Where(p => p.SectionId == section.SectionId)];

						foreach (ProcedureMasterInstruction instruction in section.ListInstrucctions)
						{
							if (listTempChoice is not null)
							{
								instruction.MultipleChoice = [];
								instruction.MultipleChoice = [.. listTempChoice.Where(p => p.InstructionId == instruction.InstructionId)];
							}
							if (listTempRange is not null)
							{
								instruction.Range = [];
								instruction.Range = [.. listTempRange.Where(p => p.InstructionId == instruction.InstructionId)];
							}
							if (listTempChoiceCheckBox is not null)
							{
								instruction.MultipleChoiceCheckBox = [];
								instruction.MultipleChoiceCheckBox = [.. listTempChoiceCheckBox.Where(p => p.InstructionId == instruction.InstructionId)];
							}

							if (listMultipleActionOperator is not null)
							{
								instruction.MultipleActionOperator = [];
								instruction.MultipleActionOperator = [.. listMultipleActionOperator.Where(p => p.InstructionId == instruction.InstructionId)];
							}

							if (listTempActionsCheckBox is not null)
							{
								instruction.ActionCheckBox = [];

								foreach (ActionChoiceDB acction in (ActionChoiceDB[])[.. listTempActionsCheckBox.Where(p => p.InstructionId == instruction.InstructionId)])
								{
									ActionChoice objAdd = new()
									{
										Id = acction.Id,
										InstructionId = acction.InstructionId,
										SectionId = acction.SectionId,
										Message = acction.Message,
										OrderChoice = acction.OrderChoice,
										ValueChoice = [.. acction.ValueChoice.Split(',')],
										IsNotify = acction.IsNotify,
										MessageNotify = acction.MessageNotify
									};
									instruction.ActionCheckBox.Add(objAdd);
								}
								// listTempActioneCheckBox.Where(p => p.InstructionId == instruction.InstructionId).ToArray();
							}

							if (listComponent is not null)
							{
								instruction.Components = [];
								instruction.Components = [.. listComponent.Where(p => p.InstructionId == instruction.InstructionId)];

								foreach (var component in instruction.Components)
								{
									//if (listattachmentcomponents is not null)
									//{
									//    component.Attachment = listattachmentcomponents.Where(p => p.SectionId == component.Id).FirstOrDefault();
									//}
								}
							}
						}
					}
					if (returnValue is not null)
					{
						returnValue.Sections = [];
						returnValue.Sections = listSection;
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
	public ResponseData ProcessMasterIns(Procedure ProcessMaster, User User, bool IsValidation = false)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				string store = "SP_SF_Procedure_Ins";
				using EWP_Command command = new(store, connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_procedureid", ProcessMaster.ProcedureId);
				_ = command.Parameters.AddWithValue("_code", ProcessMaster.Code);
				_ = command.Parameters.AddWithValue("_name", ProcessMaster.Name);
				_ = command.Parameters.AddWithValue("_status", ProcessMaster.Status);
				_ = command.Parameters.AddWithValue("_version", ProcessMaster.Version);
				_ = command.Parameters.AddWithValue("_earlierversion", ProcessMaster.EarlierVersion);
				_ = command.Parameters.AddWithValue("_ActivityTypeCode", ProcessMaster.ActivityType);
				_ = command.Parameters.AddWithValue("_idactivityclass", ProcessMaster.IdActivityClass);
				_ = command.Parameters.AddWithValue("_hasIntervention", ProcessMaster.HasIntervention);
				_ = command.Parameters.AddWithValue("_hasSource", ProcessMaster.HasSource);
				_ = command.Parameters.AddWithValue("_interventionId", ProcessMaster.InterventionId);
				_ = command.Parameters.AddWithValue("_sourceId", ProcessMaster.SourceId);
				_ = command.Parameters.AddWithValue("_description", ProcessMaster.Description);
				_ = command.Parameters.AddWithValue("_createdById", User.Id);
				_ = command.Parameters.AddWithValue("_createNewVersion", ProcessMaster.CreateNewVersion);
				_ = command.Parameters.AddWithValue("_procedureIdOrigin", ProcessMaster.ProcedureIdOrigin);
				_ = command.Parameters.AddWithValue("_IsValidation", IsValidation);
				_ = command.Parameters.AddWithValue("_UpdateEmployee", User.EmployeeId);
				_ = command.Parameters.AddWithValue("_IsByProcedure", ProcessMaster.IsByProcedure);
				_ = command.Parameters.AddWithValue("_IsManualActivity", ProcessMaster.IsManualActivity);
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
						Version = rdr["Version"].ToInt32(),
						Message = rdr["Message"].ToStr(),
					};
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				string errormsj = ex.Message;
				returnValue = null;
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return returnValue;
	}
	public List<EmployeeSkills> EmployeeSkillsList(string id)
	{
		List<EmployeeSkills> returnValue = [];

		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_Employee_Skills_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddWithValue("_EmployeeId", id);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				EmployeeSkills element = new()
				{
					Employee_Skills_Id = rdr["Id"].ToStr(),
					EmployeeId = rdr["EmployeeCode"].ToStr(),
					SkillId = rdr["SkillCode"].ToStr(),
					NameSkill = rdr["NameSkill"].ToStr(),
					QualificationObtained = rdr["SkillLevelCode"].ToStr(),
					CertificationDate = rdr["CertificationDate"].ToDate(),
					NameDocument = rdr["NameDocument"].ToStr(),
					AttachmentId = rdr["AttachmentId"].ToStr()
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
		return returnValue;
	}
	public List<EmployeeContractsDetail> ContractsDetailList(string id)
	{
		List<EmployeeContractsDetail> returnValue = [];

		using EWP_Connection connection = new(ConnectionString);
		try
		{
			using EWP_Command command = new("SP_SF_Employee_Contracts_Detail_SEL", connection)
			{
				CommandType = CommandType.StoredProcedure
			};
			command.Parameters.Clear();

			command.Parameters.AddWithValue("_EmployeeId", id);

			connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

			while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
			{
				EmployeeContractsDetail element = new()
				{
					Employee_Contracts_Detail_Id = rdr["Id"].ToStr(),
					EmployeeId = rdr["EmployeeCode"].ToStr(),
					DateStart = rdr["StartDate"].ToDate(),
					//DateEnd = rdr["EndDate"].ToDate(),
					//Validar cual es el registro actual
					DateEnd = rdr["EndDate"] != DBNull.Value ? rdr["EndDate"].ToDate() : null,
					ProfileId = rdr["PositionCode"].ToStr(),
					Salary = rdr["Salary"].ToDecimal(),
					OvertimeCost = rdr["OvertimeCost"].ToDecimal(),
					VacationsDays = rdr["VacationsDays"].ToDecimal(),
					HolidaysIncluded = rdr["HolidaysIncluded"].ToDecimal()
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
		return returnValue;
	}

	#endregion

	public List<EmployeeSkills> CreateEmployeeSkills(string employeeId, string XML, User systemOperator)
	{
		List<EmployeeSkills> returnValue = [];
		EmployeeSkills employeeSkill = null;

		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Employee_Skillss_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_EmployeeId", employeeId, !string.IsNullOrEmpty(employeeId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "EmployeeId"));
				command.Parameters.AddCondition("_XML", XML, !string.IsNullOrEmpty(XML));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					employeeSkill = new EmployeeSkills
					{
						Employee_Skills_Id = rdr["EmployeeSkillId"].ToStr(),
						EmployeeId = rdr["EmployeeCode"].ToStr(),
						SkillId = rdr["SkillCode"].ToStr()
					};
					//employeeSkill.Name = rdr["Name"].ToStr();
					//downtimeType.ClassId = rdr["ClassId"].ToStr();
					//downtimeType.CreationDate = rdr["CreationDate"].ToDate();
					//downtimeType.CreatedById = rdr["CreatedById"].ToInt32();
					//downtimeType.Status = rdr["Status"].ToInt32();

					returnValue.Add(employeeSkill);
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
	public List<EmployeeContractsDetail> CreateEmployeeContractsDetail(string employeeId, string XML, User systemOperator)
	{
		List<EmployeeContractsDetail> returnValue = [];

		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Employee_Contracts_Detail_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_EmployeeId", employeeId, !string.IsNullOrEmpty(employeeId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "EmployeeId"));
				command.Parameters.AddCondition("_XML", XML, !string.IsNullOrEmpty(XML));
				command.Parameters.AddCondition("_Operator", () => systemOperator.Id, systemOperator is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "User"));
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					//DowntimeType downtimeType = new DowntimeType();

					//downtimeType.DowntimeTypeId = rdr["DowntimeTypeId"].ToStr();
					//downtimeType.Code = rdr["Code"].ToStr();
					//downtimeType.Name = rdr["Name"].ToStr();
					//downtimeType.ClassId = rdr["ClassId"].ToStr();
					//downtimeType.CreationDate = rdr["CreationDate"].ToDate();
					//downtimeType.CreatedById = rdr["CreatedById"].ToInt32();
					//downtimeType.Status = rdr["Status"].ToInt32();

					//returnValue.Add(downtimeType);
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
public ResponseData MRGEmployee(Employee employee, bool Validation, User systemOperator)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Employee_INS", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				command.Parameters.AddWithValue("_IsValidation", Validation);
				command.Parameters.AddWithValue("_EmployeeId", employee.Id);
				command.Parameters.AddCondition("_Code", employee.Code, !string.IsNullOrEmpty(employee.Code), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "_Code"));
				command.Parameters.AddCondition("_Name", employee.Name, !string.IsNullOrEmpty(employee.Name), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "_Name"));
				command.Parameters.AddWithValue("_LastName", employee.LastName);
				//command.Parameters.AddCondition("_ShiftId", employee.ShiftId, !string.IsNullOrEmpty(employee.ShiftId), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "_ShiftId"));
				//command.Parameters.AddWithValue("_ShiftId", null);
				command.Parameters.AddCondition("_OperatorEmployee", systemOperator.EmployeeId, !string.IsNullOrEmpty(systemOperator.EmployeeId));
				command.Parameters.AddWithValue("_Operator", employee.UserId);
				command.Parameters.AddWithValue("_Status", employee.Status);
				command.Parameters.AddWithValue("_Hash", employee.Password);
				command.Parameters.AddWithValue("_PasswordFlag", employee.FlagPassword);
				command.Parameters.AddWithValue("_SupervisorId", employee.SupervisorId);
				command.Parameters.AddWithValue("_AuthorizationRequired", employee.AuthorizationRequired);
				command.Parameters.AddWithValue("_DetallesAssignedAssetJSON", employee.AssetsId is not null ? employee.EmployeeAssetsToJSON() : null);

				command.Parameters.AddWithValue("_Email", employee.Email);
				command.Parameters.AddWithValue("_PhoneNumber", employee.PhoneNumber);
				command.Parameters.AddWithValue("_Street", employee.Street);
				command.Parameters.AddWithValue("_Number", employee.Number);
				command.Parameters.AddWithValue("_Region", employee.Region);
				command.Parameters.AddWithValue("_Country", employee.Country);
				command.Parameters.AddWithValue("_StateProvince", employee.StateProvince);
				command.Parameters.AddWithValue("_City", employee.City);
				command.Parameters.AddWithValue("_Other", employee.Other);
				command.Parameters.AddWithValue("_Role", employee.Role);
				command.Parameters.AddWithValue("_ZipCode", employee.ZipCode);
				command.Parameters.AddWithValue("_Genre", employee.Genre);
				command.Parameters.AddWithValue("_BirthDate", employee.DateBirth);
				command.Parameters.AddWithValue("_PlaceBirth", employee.PlaceBirth);
				command.Parameters.AddWithValue("_MaritalStatus", employee.MaritalStatus);
				command.Parameters.AddWithValue("_NumberChildren", employee.NumberChildren);
				command.Parameters.AddWithValue("_IDNumber", employee.IDNumber);
				command.Parameters.AddWithValue("_Nationality", employee.Nationality);
				command.Parameters.AddWithValue("_PassportNumber", employee.PassportNumber);
				command.Parameters.AddWithValue("_PassportIssueDate", employee.PassportIssueDate);
				command.Parameters.AddWithValue("_PassportExpDate", employee.PassportExpiry);
				command.Parameters.AddWithValue("_IssuingAuthority", employee.IssuingAuthority);
				command.Parameters.AddWithValue("_TimeTolerance", employee.TimeTolerance);
				command.Parameters.AddWithValue("_CostPerHour", employee.CostPerHour);
				command.Parameters.AddWithValue("_NotificationGroup", employee.NotificationGroup);
				command.Parameters.AddWithValue("_ExternalId", employee.ExternalId);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					if (rdr["Id"].ToStr() == "@CEOPositionHeldBy")
					{
						returnValue = new ResponseData
						{
							Id = rdr["Id"].ToStr(),
							IsSuccess = true,
							Message = "@CEOPositionHeldBy",
						};
					}
					else
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
	
}
