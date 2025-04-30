using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

using EWP.SF.Item.BusinessEntities;


using EWP.SF.Helper;
using EWP.SF.Common;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public class DataSyncServiceProcessor
{
	private  IDataSyncServiceOperation _operations;
	private readonly ILogger<DataSyncServiceProcessor> _logger;
	private readonly User _systemOperator;
	private readonly string _defaultSyncDate = "2000-01-01T00:00:00";

	public DataSyncServiceProcessor(ILogger<DataSyncServiceProcessor> logger,IDataSyncServiceOperation operations)
	{
		_logger = logger;
		_operations = operations;
		//_systemOperator = _operations.GetUserWithoutValidations(new User(-1)).ConfigureAwait(false).GetAwaiter().GetResult();
		ContextCache.ERPOffset = null;
	}

	private  async Task<(int successRecords, int failedRecords)> ProcessResponse(ResponseData sfResponse, int successRecords, int failedRecords, DataSyncServiceLogDetail LogSingleInfo)
	{
		try
		{
			if (sfResponse.IsSuccess)
			{
				successRecords++;
				LogSingleInfo.LogType = DataSyncLogType.Success;
			}
			else
			{
				failedRecords++;
				LogSingleInfo.LogType = DataSyncLogType.Error;
				LogSingleInfo.MessageException = sfResponse.Message;
			}
		}
		catch (Exception ex)
		{
			failedRecords++;
			LogSingleInfo.LogType = DataSyncLogType.Error;
			LogSingleInfo.MessageException = ex.Message;
		}
		finally
		{
			_ = await _operations.InsertDataSyncServiceLogDetail(LogSingleInfo).ConfigureAwait(false);
		}
		return (successRecords, failedRecords);
	}
/*
	private  async Task<(int successRecords, int failedRecords)> ProcessResponse(WorkOrderResponse sfResponse, int successRecords, int failedRecords, DataSyncServiceLogDetail LogSingleInfo)
	{
		try
		{
			if (sfResponse.IsSuccess)
			{
				successRecords++;
				LogSingleInfo.LogType = DataSyncLogType.Success;
			}
			else
			{
				failedRecords++;
				LogSingleInfo.LogType = DataSyncLogType.Error;
				LogSingleInfo.MessageException = sfResponse.Message;
			}
		}
		catch (Exception ex)
		{
			failedRecords++;
			LogSingleInfo.LogType = DataSyncLogType.Error;
			LogSingleInfo.MessageException = ex.Message;
		}
		finally
		{
			_ = await _operations.InsertDataSyncServiceLogDetail(LogSingleInfo).ConfigureAwait(false);
		}
		return (successRecords, failedRecords);
	}
*/
	public async Task<DataSyncHttpResponse> ExecuteService(DataSyncService ServiceData, ServiceExecOrigin ExecOrigin = ServiceExecOrigin.Timer, TriggerType Trigger = TriggerType.SmartFactory, User User = null, string EntityCode = "", string BodyData = "", string loggerId = "")
	{
		DataSyncHttpResponse response = new();
		await Task.Yield();
		_logger.LogInformation("DataSync {EntityName} initializing execution...", ServiceData.Entity.Name);
		User requestUser = User ?? _systemOperator;
		DateTime initDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);

		DataSyncServiceLog LogInfo = new()
		{
			ServiceInstanceId = ServiceData.Id,
			ExecutionInitDate = initDate,
			ExecutionOrigin = ExecOrigin,
			LogUser = requestUser.Id,
			LogEmployee = requestUser.EmployeeId
		};
		if (!string.IsNullOrEmpty(loggerId))
		{
			LogInfo.Id = loggerId;
		}
		string logId = await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
		LogInfo.Id = logId;
		response.LogId = logId;
		try
		{
			ContextCache.SetRunningService(ServiceData.Id, true);
			//ServiceManager.Datasync_NotifyStart(ServiceData, EntityCode);
			// Actualizar fecha de ultima ejecución solo cuando no sea GET o no exista un EntityCode definido

			string httpMethod = string.Empty;
			if (!string.IsNullOrEmpty(ServiceData.HttpMethod))
			{
				httpMethod = ServiceData.HttpMethod.ToUpperInvariant();
			}
			if (httpMethod == "GET") // Peticiones GET
			{
				response = await SendErpDataToSf(LogInfo, ServiceData, requestUser, ExecOrigin, EntityCode, response, () =>
				{
					if (Trigger != TriggerType.DataSyncSettings && (string.IsNullOrEmpty(EntityCode) || !string.Equals(ServiceData.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)))
					{
						ServiceData.LastExecutionDate = initDate;
						_ = _operations.UpdateDataSyncServiceExecution(ServiceData.Id, initDate);
					}
				}).ConfigureAwait(false);
			}
			else // Peticiones POST, PUT, PATCH, etc
			{
				response = await SendSfDataToErp(LogInfo, ServiceData, requestUser, ExecOrigin, BodyData, response, () =>
				{
					if (Trigger != TriggerType.DataSyncSettings && (string.IsNullOrEmpty(EntityCode) || !string.Equals(ServiceData.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)))
					{
						ServiceData.LastExecutionDate = initDate;
						_ = _operations.UpdateDataSyncServiceExecution(ServiceData.Id, initDate);
					}
				}).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			if (response.StatusCode == HttpStatusCode.NoContent)
			{
				response.Message = "ERP returned no content";
			}
			else if (response.StatusCode == HttpStatusCode.BadRequest)
			{
				response.Message = "Invalid request to ERP";
			}
			else if (ex is ResponseDataException)
			{
				response.Message = ex.Message;
			}
			else if (string.IsNullOrEmpty(response.Message) || response.Message.Length > 1000)
			{
				response.Message = "ERP Connection unavailable";
			}

			string serviceErrors = $"DataSync {ServiceData.Entity.Name} Error: {ex.Message}";
			if (!string.IsNullOrEmpty(LogInfo.ErpReceivedJson))
			{
				string serviceError = DataSyncServiceUtil.GetErpErrors(LogInfo.ErpReceivedJson, ServiceData.ErpMapping.ErrorProperty);
				serviceErrors += $" | Service Error: {serviceError}";
			}
			LogInfo.ServiceException = serviceErrors;
			LogInfo.ExecutionFinishDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
			_ = await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
			_logger.LogInformation(serviceErrors);
		}
		finally
		{
			//ServiceManager.Datasync_NotifyStop(ServiceData, EntityCode, initDate);
			ContextCache.SetRunningService(ServiceData.Id, false);
			_logger.LogInformation("DataSync {EntityName} finishing execution...", ServiceData.Entity.Name);
		}
		return response;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="ErpData"></param>
	/// <param name="ListFailedRecords"></param>
	/// <param name="User"></param>
	/// <returns></returns>
	public async Task ProcessErrors(DataSyncErp ErpData, List<DataSyncServiceLogDetail> ListFailedRecords, User User = null)
	{
		_logger.LogInformation($"DataSync Error Processor initializing execution...");
		User requestUser = User ?? _systemOperator;
		foreach (DataSyncServiceLogDetail log in ListFailedRecords)
		{
			_logger.LogInformation("Log Error {LogId} initializing...", log.Id);
			DataSyncService serviceData = _operations.GetServiceInstanceFullData(log.ServiceInstanceId);
			if (serviceData.RevalidationEnable == EnableType.Yes)
			{
				DateTime limitDateToProcessErrors = (DateTime)(log.ProcessDate?.AddSeconds(ErpData.MaxReprocessingTime));
				DateTime currentDate = DataSyncServiceUtil.ConvertDate(serviceData.ErpData.DateTimeFormat, DateTime.Now, serviceData.ErpData.TimeZone);
				dynamic dataMapped = DataSyncServiceUtil.MapEntity(serviceData.ErpMapping.ResponseMapSchema, log.ErpReceivedJson, serviceData.ErpMapping.ResultProperty);
				DataSyncServiceLog logInfo = new()
				{
					Id = log.LogId,
					ServiceInstanceId = serviceData.Id
				};
				try
				{
					if (dataMapped is not null)
					{
						string dataMappedJson = JsonConvert.SerializeObject(dataMapped);
						await ReprocessError(log.ErpReceivedJson, dataMappedJson, serviceData, logInfo, log, requestUser).ConfigureAwait(false);
					}
					else
					{
						throw new Exception("No data to process");
					}
				}
				catch (Exception ex)
				{
					string serviceErrors = $"DataSync Error Processor exception: {ex.Message}";
					if (!string.IsNullOrEmpty(logInfo.ErpReceivedJson))
					{
						string serviceError = DataSyncServiceUtil.GetErpErrors(logInfo.ErpReceivedJson, serviceData.ErpMapping.ErrorProperty);
						serviceErrors += $" | Exception: {serviceError}";
					}

					_ = await _operations.InsertDataSyncServiceLog(logInfo).ConfigureAwait(false);
					_logger.LogInformation(serviceErrors);
				}
				finally
				{
					_logger.LogInformation("Log Error {LogId} finishing...", log.Id);
				}
			}
		}
		_logger.LogInformation($"DataSync Error Processor finishing execution...");
	}

	[RequiresUnreferencedCode("Calls System.String.IsNullOrEmpty(String)")]
	private async Task<DataSyncHttpResponse> SendErpDataToSf(DataSyncServiceLog LogInfo, DataSyncService ServiceData, User SystemOperator, ServiceExecOrigin ExecOrigin, string EntityCode, DataSyncHttpResponse HttpResponse, Action onResponse)
	{
		DataSyncResponse erpResult = await ErpGetRequest(ServiceData, EntityCode, LogInfo).ConfigureAwait(false);

		//Status code es importante no quitarlo.
		HttpResponse.StatusCode = erpResult.StatusCode;
		LogInfo.ErpReceivedJson = erpResult.Response;
		await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);

		if (erpResult.StatusCode == HttpStatusCode.NoContent || erpResult.StatusCode == HttpStatusCode.OK)
		{
			bool isError = false;
			try
			{
				object resp = JsonConvert.DeserializeObject(erpResult.Response);
				dynamic responseErp = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.ResponseMapSchema, erpResult.Response);
				string stsResponse = responseErp["Status"];
				isError = string.Equals(stsResponse, "error", StringComparison.OrdinalIgnoreCase);
			}
			catch { }
			if (onResponse is not null && !isError)
			{
				onResponse();
			}
		}
		if (erpResult.StatusCode == HttpStatusCode.OK)
		{
			dynamic dataMapped = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.ResponseMapSchema, erpResult.Response, ServiceData.ErpMapping.ResultProperty);
			dynamic dataMappedOriginal = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.ResponseMapSchema, erpResult.Response, ServiceData.ErpMapping.ResultProperty, true);
			if (dataMapped is not null)
			{
				string dataJson = JsonConvert.SerializeObject(dataMapped);
				string dataJsonOriginal = JsonConvert.SerializeObject(dataMappedOriginal);
				int failedRecords = 0;
				int successRecords = 0;
				switch (ServiceData.Entity.Name)
				{
					

					case BackgroundServices.ITEM_SERVICE:
						

					
								
						
						List<LaborIssueExternal> listLaborIssues = JsonConvert.DeserializeObject<List<LaborIssueExternal>>(dataJson);
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listLaborIssues);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						_ = await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listLaborIssues.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (LaborIssueExternal elem in listLaborIssues)
							{
								List<LaborIssueExternal> listElem = [elem];
								ResponseData sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.DocCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "docCode", elem.DocCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									//TODO: Implementar el metodo de sincronización de LaborIssue GET
									/*
                                    sfResponse = (await _operations.ListUpdateMaterialIssue(
                                    listElem,
                                    SystemOperator,
                                    false,
                                    LevelMessage.Success
                                    ).ConfigureAwait(false)).FirstOrDefault();
                                    */
									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new ResponseData
									{
										IsSuccess = false,
										Message = ex.Message
									};
								}
								finally
								{
									(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);

									sfListResponse.Add(sfResponse);
								}
							}
							LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
						}
						break;
					default: throw new Exception("No instance configured to receive data from ERP");
				}
				// Log
				LogInfo.FailedRecords = failedRecords;
				LogInfo.SuccessRecords = successRecords;
				HttpResponse.FailRecords = failedRecords;
				HttpResponse.SuccessRecords = successRecords;
				LogInfo.ExecutionFinishDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);

				//Encender Servicio nuevamente
				_ = await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
			}
			else
			{
				try
				{
					dynamic obj = JsonConvert.DeserializeObject(erpResult.Response);
					string message = obj[ServiceData.ErpMapping.ResultProperty];
					if (message.Contains('{') && message.Contains('}'))
					{
						try
						{
							dynamic subobj = JsonConvert.DeserializeObject(message);
							if (subobj is not null && !string.IsNullOrEmpty(subobj[ServiceData.ErpMapping.ResultProperty]))
							{
								message = "Level 2:" + subobj[ServiceData.ErpMapping.ResultProperty];
							}
						}
						catch
						{
						}
					}
					HttpResponse.StatusCode = HttpStatusCode.InternalServerError;
					HttpResponse.Message = message;
				}
				catch
				{
					string message = "Error fetching data from ERP";
					HttpResponse.StatusCode = HttpStatusCode.NotFound;
					HttpResponse.Message = message;
				}

				throw new Exception(HttpResponse.Message);
			}
		}
		else
		{
			throw new Exception($"Request Status Code {erpResult.StatusCode.ToInt32()}: {erpResult.StatusMessage}");
		}

		return HttpResponse;
	}

	private  async Task<DataSyncHttpResponse> SendSfDataToErp(DataSyncServiceLog LogInfo, DataSyncService ServiceData, User SystemOperator, ServiceExecOrigin ExecOrigin, string RequestBody, DataSyncHttpResponse HttpResponse, Action onResponse)
	{
		if (string.IsNullOrEmpty(RequestBody))
		{
			throw new Exception("No request body found");
		}
		dynamic requestErpMapped = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.RequestMapSchema, RequestBody) ?? throw new Exception("No data to process");
		string requestErpJson = JsonConvert.SerializeObject(requestErpMapped);
		LogInfo.SfMappedJson = requestErpJson;
		LogInfo.EndpointUrl = ServiceData.ErpData.BaseUrl + ServiceData.Path;
		//_ = await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
		int tries = 0;
		string detailId = Guid.NewGuid().ToString();
		DataSyncResponse erpResult;
		do
		{
			tries++;
			erpResult = await ErpSendRequestAsync(ServiceData, ExecOrigin, requestErpJson, tries > 1, LogInfo).ConfigureAwait(false);
			string temporaryResponse = string.Empty;
			try
			{
				dynamic tempResponseErp = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.ResponseMapSchema, erpResult.Response);
				temporaryResponse = tempResponseErp["Status"];
				onResponse?.Invoke();
			}
			catch
			{
			}
			if (erpResult.StatusCode == HttpStatusCode.RequestTimeout || string.Equals(temporaryResponse, "PENDING", StringComparison.OrdinalIgnoreCase))
			{
				erpResult.StatusCode = HttpStatusCode.RequestTimeout;
				if (tries < (ServiceData.RequestReprocMaxRetries + 1) && ServiceData.RequestReprocMaxRetries > 0)
				{
					DataSyncServiceLogDetail LogSingleInfo = new()
					{
						Id = detailId,
						LogId = LogInfo.Id,
						RowKey = ServiceData.EntityId,
						AttemptNo = tries,
						ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
						SfMappedJson = JsonConvert.SerializeObject(requestErpJson),
						ResponseJson = string.IsNullOrEmpty(erpResult.Response) ? erpResult.StatusMessage : erpResult.Response,
						LastAttemptDate = DateTime.UtcNow,
						LogType = DataSyncLogType.Error
					};
					//_ = await _operations.InsertDataSyncServiceLogDetail(LogSingleInfo).ConfigureAwait(false);
					await Task.Delay(1000 * ServiceData.RequestReprocFrequencySecs).ConfigureAwait(false);
				}
			}
		} while (erpResult.StatusCode == HttpStatusCode.RequestTimeout && ServiceData.RevalidationEnable == EnableType.Yes && tries < (ServiceData.RequestReprocMaxRetries + 1) && ServiceData.RequestReprocMaxRetries > 0);
		DataSyncServiceLogDetail LogSingleInfoFinish;

		if (erpResult.StatusCode != HttpStatusCode.OK)
		{
			LogSingleInfoFinish = new DataSyncServiceLogDetail
			{
				Id = detailId,
				LogId = LogInfo.Id,
				RowKey = ServiceData.EntityId,
				AttemptNo = tries,
				ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
				SfMappedJson = JsonConvert.SerializeObject(requestErpJson),
				ResponseJson = string.IsNullOrEmpty(erpResult.Response) ? erpResult.StatusMessage : erpResult.Response,
				LastAttemptDate = DateTime.UtcNow,
				LogType = DataSyncLogType.Error
			};
			//_ = await _operations.InsertDataSyncServiceLogDetail(LogSingleInfoFinish).ConfigureAwait(false);
			throw new Exception($"Request Status Code {erpResult.StatusCode.ToInt32()}: {erpResult.StatusMessage}");
		}
		string expectedResult = ServiceData.ErpMapping.ExpectedResult;
		if (string.IsNullOrEmpty(expectedResult))
		{
			_ = new DataSyncServiceLogDetail
			{
				Id = detailId,
				LogId = LogInfo.Id,
				RowKey = ServiceData.EntityId,
				AttemptNo = tries,
				ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
				SfMappedJson = JsonConvert.SerializeObject(requestErpJson),
				ResponseJson = string.IsNullOrEmpty(erpResult.Response) ? erpResult.StatusMessage : erpResult.Response,
				LastAttemptDate = DateTime.UtcNow,
				LogType = DataSyncLogType.Error
			};
			throw new Exception($"Instance is not correctly configured (Expected Result setting is needed)");
		}
		dynamic responseErp = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.ResponseMapSchema, erpResult.Response);
		LogInfo.ErpReceivedJson = JsonConvert.SerializeObject(responseErp);
		dynamic expectedErpResult = JsonConvert.DeserializeObject<JObject>(expectedResult);
		string stsResponse = responseErp["Status"];
		string stsExpected = expectedErpResult["Status"];
		int failedRecords = 0;
		int successRecords = 0;
		if (string.IsNullOrEmpty(stsResponse) || string.IsNullOrEmpty(stsExpected))
		{
			_ = new DataSyncServiceLogDetail
			{
				Id = detailId,
				LogId = LogInfo.Id,
				RowKey = ServiceData.EntityId,
				AttemptNo = tries,
				ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
				SfMappedJson = JsonConvert.SerializeObject(requestErpJson),
				ResponseJson = string.IsNullOrEmpty(erpResult.Response) ? erpResult.StatusMessage : erpResult.Response,
				LastAttemptDate = DateTime.UtcNow,
				LogType = DataSyncLogType.Error
			};
			throw new Exception($"Instance is not correctly configured (Expected Result mapping error)");
		}
		// Status Comparison
		if (stsResponse.Trim() == "Error")
		{
			failedRecords += 1;
			string stsMessage = "An error has occurred";
			if (responseErp["Message"] is not null)
			{
				stsMessage = responseErp["Message"];
			}
			HttpResponse.Message = stsMessage;
			LogInfo.FailedRecords = failedRecords;
			LogInfo.SuccessRecords = successRecords;
			LogInfo.ServiceException = responseErp.ToString();
			LogInfo.ExecutionFinishDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
			//_ = await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
		}
		else if (stsResponse.Trim() == stsExpected.Trim())
		{
			successRecords += 1;
			// Log
			LogInfo.FailedRecords = failedRecords;
			LogInfo.SuccessRecords = successRecords;
			HttpResponse.StatusCode = HttpStatusCode.OK;
			HttpResponse.Message = LogInfo.ErpReceivedJson;
			LogInfo.ExecutionFinishDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
			//_ = await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
		}

		LogSingleInfoFinish = new DataSyncServiceLogDetail
		{
			Id = detailId,
			LogId = LogInfo.Id,
			RowKey = ServiceData.EntityId,
			AttemptNo = tries,
			ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
			SfMappedJson = JsonConvert.SerializeObject(requestErpJson),
			ResponseJson = string.IsNullOrEmpty(erpResult.Response) ? erpResult.StatusMessage : erpResult.Response,
			LastAttemptDate = DateTime.UtcNow,
			LogType = successRecords > 0 ? DataSyncLogType.Success : DataSyncLogType.Error
		};
		//_ = await _operations.InsertDataSyncServiceLogDetail(LogSingleInfoFinish).ConfigureAwait(false);

		return HttpResponse;
	}

	private static async Task ReprocessError(string ErpJson, string DataJson, DataSyncService ServiceData, DataSyncServiceLog LogInfo, DataSyncServiceLogDetail LogDetail, User SystemOperator)
	{
		int failedRecords = 0;
		int successRecords = 0;
		LogInfo.ExecutionOrigin = ServiceExecOrigin.Timer;
		switch (ServiceData.Entity.Name)
		{
			case BackgroundServices.ITEM_SERVICE:
				break;
		}
		// Log
		LogInfo.FailedRecords += failedRecords;
		LogInfo.SuccessRecords += successRecords;
		// LogInfo.ExecutionFinishDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
		// await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
	}

	private  async Task SetRequestHeaders(DataSyncService ServiceData, APIWebClient client)
	{
		// Required Headers
		client.DefaultRequestHeaders.Clear();
		if (!string.IsNullOrEmpty(ServiceData.ErpData.RequiredHeaders))
		{
			List<KeyValuePair<string, string>> requiredHeaderList = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(ServiceData.ErpData.RequiredHeaders);
			requiredHeaderList.ForEach(header => client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value));
		}
		// Token
		if (!string.IsNullOrEmpty(ServiceData.ErpData.TokenRequestPath))
		{
			DataSyncErpAuth auth = await GetErpToken(client, ServiceData).ConfigureAwait(false);
			ServiceData.TokenData = auth;
			if (!string.IsNullOrEmpty(auth.Token))
			{
				switch (auth.TokenType)
				{
					case "Bearer":
						client.DefaultRequestHeaders.Add("Authorization", $"Bearer {auth.Token}");
						break;
				}
			}
		}
	}

	private async Task<DataSyncResponse> ErpGetRequest(DataSyncService ServiceData, string EntityCode = "", DataSyncServiceLog LogInfo = null)
	{
		string httpMethod = string.Empty;
		if (!string.IsNullOrEmpty(ServiceData.HttpMethod))
		{
			httpMethod = ServiceData.HttpMethod.ToUpperInvariant();
		}
		if (httpMethod != "GET")
		{
			throw new Exception("No GET request");
		}

		_ = new DataSyncResponse();
		bool syncAllData = Config.Configuration["DataSyncService-SyncAll"].ToBool();
		using APIWebClient client = new()
		{
			TimeoutSeconds = ServiceData.RequestTimeoutSecs // Required Timeout
		};
		// Required Headers And Token
		await SetRequestHeaders(ServiceData, client).ConfigureAwait(false);

		string endpointUrl = ServiceData.ErpData.BaseUrl + ServiceData.Path;
		if (!string.IsNullOrEmpty(ServiceData.UrlParams))
		{
			double erpOffset = _operations.GetTimezoneOffset("ERP");
			DateTime offsetExecDate = ServiceData.LastExecutionDate.AddMinutes(-ServiceData.OffsetMin).AddHours(erpOffset);
			string syncDate = offsetExecDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
			if (syncAllData || ServiceData.DeltaSync == EnableType.No)
			{
				syncDate = _defaultSyncDate;
			}
			endpointUrl += $"?{ServiceData.UrlParams.Replace("{0}", syncDate)}";
		}

		if (!string.IsNullOrEmpty(EntityCode) && !string.IsNullOrEmpty(ServiceData.SingleRecordParam))
		{
			// Normal;
			if (!EntityCode.Contains('='))
			{
				endpointUrl = ServiceData.ErpData.BaseUrl + ServiceData.Path + "?" + ServiceData.SingleRecordParam.Replace("{0}", EntityCode);
			}

			if (EntityCode.Contains('='))
			{
				endpointUrl = ServiceData.ErpData.BaseUrl + ServiceData.Path + "?" + EntityCode;
			}
			else if (EntityCode.Length >= 19)
			{
				if (DateTime.TryParseExact(EntityCode[..19], "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deltaDate))
				{
					string syncDate = deltaDate.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
					endpointUrl = ServiceData.ErpData.BaseUrl + ServiceData.Path;
					endpointUrl += $"?{ServiceData.UrlParams.Replace("{0}", syncDate)}";
				}
			}
		}
		if (ServiceData.RequestTimeoutSecs > 5)
		{
			endpointUrl += $"&Timeout={ServiceData.RequestTimeoutSecs - 5}";
		}
		if (LogInfo is not null)
		{
			LogInfo.EndpointUrl = endpointUrl;
			_ = await _operations.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
		}
		string dynamicBody = string.Empty;
		if (ServiceData.EnableDynamicBody == 1)
		{
			dynamicBody = _operations.GetDatasyncDynamicBody(ServiceData.Entity.Id);
		}
		DataSyncResponse erpResponse = await client.DataSyncDownload(endpointUrl, dynamicBody).ConfigureAwait(false);

		if (ServiceData.ErpData.RequiresTokenRenewal == EnableType.Yes)
		{
			// Autorenovación de token
			if (!string.IsNullOrEmpty(ServiceData.ErpData.TokenRenewalMapSchema))
			{
				ErpTokenRenewal(ServiceData, erpResponse.ReturnHeaders);
			}
		}
		return erpResponse;
	}

	private  async Task<DataSyncResponse> ErpSendRequestAsync(DataSyncService ServiceData, ServiceExecOrigin ExecOrigin, string BodyData = "", bool triggerRevalidate = false, DataSyncServiceLog LogInfo = null)
	{
		string httpMethod = string.Empty;
		if (!string.IsNullOrEmpty(ServiceData.HttpMethod))
		{
			httpMethod = ServiceData.HttpMethod.ToUpperInvariant();
		}
		if (httpMethod == "GET")
		{
			throw new InvalidOperationException("Invalid request");
		}

		_ = new DataSyncResponse();
		using APIWebClient client = new()
		{
			// Required Timeout
			TimeoutSeconds = ServiceData.RequestTimeoutSecs
		};
		// Required Headers And Token
		await SetRequestHeaders(ServiceData, client).ConfigureAwait(false);

		string endpointUrl = ServiceData.ErpData.BaseUrl + ServiceData.Path;

		// Si es SAP B1 se enviará petición a endpoint validate
		if (triggerRevalidate && ServiceData.RevalidationEnable == EnableType.Yes)
		{
			endpointUrl += "/Validate";
		}

		DataSyncResponse erpResponse;
		try
		{
			erpResponse = await client.DataSyncUpload(endpointUrl, httpMethod, BodyData).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			bool isTimeout = EvaluateTimeoutException(ex);
			if (isTimeout && ServiceData.RevalidationEnable == EnableType.Yes)
			{
				erpResponse = new DataSyncResponse { StatusCode = HttpStatusCode.RequestTimeout, Response = JsonConvert.SerializeObject(new { status = "Pending", message = "a timeout has occurred" }), StatusMessage = "Request Timeout" };
			}
			else if (isTimeout)
			{
				throw new ResponseDataException("The request has timed out");
			}
			else
			{
				throw;
			}
		}

		if (ServiceData.ErpData.RequiresTokenRenewal == EnableType.Yes)
		{
			// Autorenovación de token
			if (!string.IsNullOrEmpty(ServiceData.ErpData.TokenRenewalMapSchema) && erpResponse.ReturnHeaders is not null)
			{
				ErpTokenRenewal(ServiceData, erpResponse.ReturnHeaders);
			}
		}
		return erpResponse;
	}

	private static bool EvaluateTimeoutException(Exception ex)
	{
		if (ex is WebException wx && wx.Status == WebExceptionStatus.Timeout)
		{
			return true;
		}
		return ex is TaskCanceledException || (ex.InnerException is not null && ex.InnerException is TaskCanceledException);
	}

	private  async Task<DataSyncErpAuth> GetErpToken(APIWebClient client, DataSyncService ServiceData)
	{
		DataSyncErpAuth response;
		if (!string.IsNullOrEmpty(ServiceData.TokenData.Token))
		{
			response = ServiceData.TokenData;
		}
		else
		{
			response = _operations.GetDataSyncServiceErpToken(ServiceData.ErpData.ErpCode);
		}
		int isTokenExpired = 0;
		if (response is not null)
		{
			DateTime nowDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
			isTokenExpired = DateTime.Compare(nowDate, response.ExpirationDate);
		}
		if (response is null || isTokenExpired > 0)
		{
			// Token
			int tries = 0;
			while (tries < 3)
			{
				try
				{
					tries++;
					string erpResult = await client.UploadString(ServiceData.ErpData.BaseUrl + ServiceData.ErpData.TokenRequestPath, "post", ServiceData.ErpData.TokenRequestJson).ConfigureAwait(false);
					JObject jsResult = JObject.Parse(erpResult);
					_ = jsResult.TryGetValue(ServiceData.ErpData.TokenRequestResultProp, out JToken objTokenSearch);
					if (objTokenSearch is null)
					{
						_ = jsResult.TryGetValue("status", out objTokenSearch);
						if (objTokenSearch is not null)
						{
							throw new Exception(objTokenSearch.ToStr());
						}
					}
					List<DataSyncMapSchema> listTokenMapSchema = JsonConvert.DeserializeObject<List<DataSyncMapSchema>>(ServiceData.ErpData.TokenRequestMapSchema);
					dynamic tokenMapped = DataSyncServiceUtil.MapProperties(objTokenSearch, listTokenMapSchema);
					string tokenMappedJson = JsonConvert.SerializeObject(tokenMapped);
					DataSyncErpAuth tokenData = JsonConvert.DeserializeObject<DataSyncErpAuth>(tokenMappedJson);

					tokenData.ErpId = ServiceData.ErpData.Id;
					if (tokenData.ExpirationTime > 0)
					{
						tokenData.ExpirationDate = DateTime.Now.AddSeconds(tokenData.ExpirationTime.ToInt32());
					}
					response = tokenData;
					if (!string.IsNullOrEmpty(response.Token))
					{
						_ = _operations.InsertDataSyncServiceErpToken(response);
					}
					tries = 4;
				}
				catch (Exception ex)
				{
					if (!string.IsNullOrEmpty(ex.Message) && ex.Message.Length == 3)
					{
						if (ex.Message.Equals("401", StringComparison.Ordinal))
						{
							throw new ResponseDataException("Unauthorized to ERP Connector", "500");
						}
						else
						{
							throw new ResponseDataException("Could not authenticate with ERP");
						}
					}
					else if (ex.InnerException is not null)
					{
						if (ex.InnerException.Message.Contains("connection attempt failed"))
						{
							if (tries >= 3)
							{
								throw new ResponseDataException("Could not connect to ERP Connector", "500");
							}
						}
						else if (ex.InnerException.Message.Contains("target machine actively refused"))
						{
							throw new ResponseDataException("The ERP Connector rejected the connection", "500");
						}
						else
						{
							if (tries >= 3)
							{
								throw new ResponseDataException("Could not authenticate with ERP");
							}
						}
					}
					else
					{
						if (tries >= 3)
						{
							throw new ResponseDataException("Could not authenticate with ERP");
						}
					}
				}
			}
		}
		return response;
	}

	private  void ErpTokenRenewal(DataSyncService ServiceData, HttpResponseHeaders ResponseHeaders)
	{
		DataSyncTokenRenewSchema tokenRenewalMapSchema = JsonConvert.DeserializeObject<DataSyncTokenRenewSchema>(ServiceData.ErpData.TokenRenewalMapSchema);
		if (tokenRenewalMapSchema.Origin == TokenRenewOrigin.Header)
		{
			dynamic renewalToken = new JObject();
			tokenRenewalMapSchema.MapSchema.ForEach(schema =>
			{
				if (ResponseHeaders.Contains(schema.OriginProperty))
				{
					renewalToken[schema.MapProperty] = ResponseHeaders.GetValues(schema.OriginProperty).First();
				}
			});
			string tokenRenewalMapSchemaJson = JsonConvert.SerializeObject(renewalToken);
			DataSyncErpAuth objAuthRenewal = JsonConvert.DeserializeObject<DataSyncErpAuth>(tokenRenewalMapSchemaJson);
			if (ServiceData.TokenData.Token != objAuthRenewal.Token)
			{
				objAuthRenewal.ErpId = ServiceData.ErpData.Id;
				if (!string.IsNullOrEmpty(objAuthRenewal.Token))
				{
					_ = _operations.InsertDataSyncServiceErpToken(objAuthRenewal);
				}
			}
		}
	}
}
