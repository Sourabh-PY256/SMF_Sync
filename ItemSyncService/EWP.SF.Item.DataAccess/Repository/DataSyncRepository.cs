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

	public async Task<List<TimeZoneCatalog>> GetTimezones(bool currentValues)
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

	/// <summary>
	/// List the data synchronization ERP information.
	/// </summary>
	public List<DataSyncErp> ListDataSyncERP(string Id = "", EnableType GetInstances = EnableType.Yes)
	{
		List<DataSyncErp> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Erp_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Id", Id, !string.IsNullOrEmpty(Id));
				command.Parameters.AddWithValue("_GetInstances", GetInstances);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DataSyncErp element = new()
						{
							Id = rdr["Id"].ToStr(),
							ErpCode = rdr["ErpCode"].ToStr(),
							Erp = rdr["Erp"].ToStr(),
							ErpVersionCode = rdr["ErpVersionCode"].ToStr(),
							ErpVersion = rdr["ErpVersion"].ToStr(),
							DbCode = rdr["DbCode"].ToStr(),
							Db = rdr["Db"].ToStr(),
							DbVersionCode = rdr["DbVersionCode"].ToStr(),
							DbVersion = rdr["DbVersion"].ToStr(),
							ManufacturingCode = rdr["ManufacturingCode"].ToStr(),
							Manufacturing = rdr["Manufacturing"].ToStr(),
							ManufacturingVersionCode = rdr["ManufacturingVersionCode"].ToStr(),
							ManufacturingVersion = rdr["ManufacturingVersion"].ToStr(),
							BaseUrl = rdr["BaseUrl"].ToStr(),
							TokenRequestJson = rdr["TokenRequestJson"].ToStr(),
							TokenRequestPath = rdr["TokenRequestPath"].ToStr(),
							TokenRequestMapSchema = rdr["TokenRequestMapSchema"].ToStr(),
							TokenRequestResultProp = rdr["TokenRequestResultProp"].ToStr(),
							RequiresTokenRenewal = (EnableType)rdr["RequiresTokenRenewal"].ToInt32(),
							TokenRenewalMapSchema = rdr["TokenRenewalMapSchema"].ToStr(),
							RequiredHeaders = rdr["RequiredHeaders"].ToStr(),
							DateTimeFormat = (DateTimeFormatType)rdr["DateTimeFormat"].ToInt32(),
							TimeZone = rdr["TimeZone"].ToStr(),
							ReprocessingTime = rdr["ReprocessingTime"].ToInt32(),
							MaxReprocessingTime = rdr["MaxReprocessingTime"].ToInt32(),
							ReprocessingTimeOffset = rdr["ReprocessingOffsetSeconds"].ToInt32(),
							CreateDate = rdr["CreateDate"].ToDate(),
							CreateUser = new User(rdr["CreateUser"].ToInt32()),
							Status = (Status)rdr["Status"].ToInt32()
						};
						if (rdr["UpdateDate"].ToDate().Year > 1900)
						{
							element.UpdateDate = rdr["UpdateDate"].ToDate();
							element.UpdateUser = new User(rdr["UpdateUser"].ToInt32());
						}
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
	/// <summary>
	///
	/// </summary>
	public List<DefaultMappingEntityObject> ListDefaultMappingEntityObject(string Entity)
	{
		List<DefaultMappingEntityObject> returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_DataSync_Default_Mapping_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddCondition("_Entity", Entity, !string.IsNullOrEmpty(Entity), string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Entity"));
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (rdr.HasRows)
				{
					while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
					{
						DefaultMappingEntityObject element = new()
						{
							Id = rdr["Id"].ToStr(),
							Name = rdr["Name"].ToStr()
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


}
