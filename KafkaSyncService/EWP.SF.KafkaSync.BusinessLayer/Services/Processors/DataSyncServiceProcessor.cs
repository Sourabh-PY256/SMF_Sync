using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;

using EWP.SF.Helper;
using EWP.SF.Common;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models.Catalogs;
using Microsoft.Extensions.DependencyInjection;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class DataSyncServiceProcessor
{

	private readonly ILogger<DataSyncServiceProcessor> _logger;
	private readonly User _systemOperator;
	private readonly string _defaultSyncDate = "2000-01-01T00:00:00";

	IDataSyncServiceOperation _dataSyncServiceOperation;
	IServiceProvider _serviceProvider;






	public DataSyncServiceProcessor(ILogger<DataSyncServiceProcessor> logger,
	IDataSyncServiceOperation operations, IServiceProvider serviceProvider)
	{
		_logger = logger;
		_dataSyncServiceOperation = operations;
		_systemOperator = _dataSyncServiceOperation.GetUserWithoutValidations(new User(-1)).ConfigureAwait(false).GetAwaiter().GetResult();
		ContextCache.ERPOffset = null;
		_serviceProvider = serviceProvider;


	}
	private T GetOperation<T>() where T : class
	{
		return _serviceProvider.GetRequiredService<T>();
	}

	public async Task<DataSyncHttpResponse> SyncExecution(DataSyncService ServiceData, ServiceExecOrigin ExecOrigin = ServiceExecOrigin.Timer, TriggerType Trigger = TriggerType.SmartFactory, User User = null, string EntityCode = "", string BodyData = "", string loggerId = "")
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
		
		
		
		try
		{
			//ContextCache.SetRunningService(ServiceData.Id, true);
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
						_ = _dataSyncServiceOperation.UpdateDataSyncServiceExecution(ServiceData.Id, initDate);
					}
				}).ConfigureAwait(false);
			}
			else // Peticiones POST, PUT, PATCH, etc
			{
				response = await SendSfDataToErp(LogInfo, ServiceData, requestUser, ExecOrigin, BodyData, EntityCode, response, () =>
				{
					if (Trigger != TriggerType.DataSyncSettings && (string.IsNullOrEmpty(EntityCode) || !string.Equals(ServiceData.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)))
					{
						ServiceData.LastExecutionDate = initDate;
						_ = _dataSyncServiceOperation.UpdateDataSyncServiceExecution(ServiceData.Id, initDate);
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
			//_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
			_logger.LogInformation(serviceErrors);
			throw;
		}
		finally
		{
			//ServiceManager.Datasync_NotifyStop(ServiceData, EntityCode, initDate);
			ContextCache.SetRunningService(ServiceData.Id, false);
			_logger.LogInformation("DataSync {EntityName} finishing execution...", ServiceData.Entity.Name);
		}
		return response;
	}

	[RequiresUnreferencedCode("Calls System.String.IsNullOrEmpty(String)")]
	private async Task<DataSyncHttpResponse> SendErpDataToSf(DataSyncServiceLog LogInfo, DataSyncService ServiceData, User SystemOperator, ServiceExecOrigin ExecOrigin, string EntityCode, DataSyncHttpResponse HttpResponse, Action onResponse)
	{
		DataSyncResponse erpResult = await ErpGetRequest(ServiceData, EntityCode, LogInfo).ConfigureAwait(false);
		bool isError = false;
		//Status code es importante no quitarlo.
		HttpResponse.StatusCode = erpResult.StatusCode;
		LogInfo.ErpReceivedJson = erpResult.Response;
		// await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);

		if (erpResult.StatusCode == HttpStatusCode.NoContent)
		{

			try
			{
				object resp = JsonConvert.DeserializeObject(erpResult.Response);
				dynamic responseErp = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.ResponseMapSchema, erpResult.Response);
				string stsResponse = responseErp["Status"];
				isError = string.Equals(stsResponse, "error", StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception e)
			{
				if (erpResult.StatusCode == HttpStatusCode.NoContent)
				{
					string message = "ERP returned no content";
					HttpResponse.StatusCode = HttpStatusCode.NotFound;
					HttpResponse.Message = message;
					return HttpResponse;
				}
			}
			//Update Sync Time
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
					// Not Applicable for any ERP
					// case SyncERPEntity.ATTACHMENT_SERVICE:
					// 	List<AttachmentExternal> listAttachments = JsonConvert.DeserializeObject<List<AttachmentExternal>>(dataJson);
					// 	var attachmentOperation = GetOperation<IAttachmentOperation>();
					// 	LogInfo.SfMappedJson = JsonConvert.SerializeObject(listAttachments);
					// 	LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
					// 	//await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
					// 	if (listAttachments.Count > 0)
					// 	{
					// 		List<AttachmentExternalResponse> sfResponse = await attachmentOperation.AttachmentSyncSel(listAttachments, SystemOperator).ConfigureAwait(false);
					// 		successRecords = 0;
					// 		if (sfResponse.Count > 0)
					// 		{
					// 			foreach (AttachmentExternalResponse elem in sfResponse)
					// 			{
					// 				try
					// 				{
					// 					ServiceData.SingleRecordParam = "AttachmentId={0}";
					// 					DataSyncResponse erpAttachmentResult = await ErpGetRequest(ServiceData, elem.AttachmentIdExternal, LogInfo).ConfigureAwait(false);
					// 					if (erpAttachmentResult.StatusMessage == "OK" && !string.IsNullOrEmpty(erpAttachmentResult.Response))
					// 					{
					// 						AttachmentResponse response = await attachmentOperation.SaveAttachmentExternal(elem, erpAttachmentResult.Response, SystemOperator).ConfigureAwait(false);
					// 						successRecords++;
					// 						if (!string.IsNullOrEmpty(response.Id))
					// 						{
					// 							LogInfo.SfResponseJson = JsonConvert.SerializeObject(response);
					// 						}
					// 					}
					// 				}
					// 				catch
					// 				{
					// 					failedRecords++;
					// 				}
					// 			}
					// 		}
					// 	}
					// 	break;
// Not Applicable for any ERP
					case SyncERPEntity.ALLOCATION_SERVICE:
					case SyncERPEntity.FULL_ALLOCATION_SERVICE:
						StockAllocationExternal[] listAllocStock = JsonConvert.DeserializeObject<StockAllocationExternal[]>(dataJson);
						var stockAllocation = GetOperation<IStockAllocationOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listAllocStock);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						ResponseData sfAllocationResponse = null;
						try {
							if (listAllocStock.Length > 0)
							{
							    sfAllocationResponse = stockAllocation.ListUpdateAllocationBulk(listAllocStock, SystemOperator, false, LevelMessage.Success, true);
								successRecords = 0;
								if (sfAllocationResponse.IsSuccess)
								{
									successRecords = sfAllocationResponse.Code.ToInt32();
								}
								failedRecords = listAllocStock.Length - successRecords;
								LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfAllocationResponse);
							}
						}
						catch (Exception ex)
						{
							sfAllocationResponse = new ResponseData
							{
								IsSuccess = false,
								Message = ex.Message
							};
						}
						finally
						{
							failedRecords = listAllocStock.Length - successRecords;
							LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfAllocationResponse);
						}

						 break;
// Not Applicable for any ERP
					// case SyncERPEntity.IOT_DATA_SIMULATOR_SERVICE:
					// 	break;
// Not Applicable for any ERP
					// case SyncERPEntity.FACILITY_SERVICE:
					// case SyncERPEntity.FLOOR_SERVICE:
					// case SyncERPEntity.WORKCENTER_SERVICE:
					// case SyncERPEntity.PRODUCTION_LINE_SERVICE:
					// 	List<AssetExternal> listAssets = JsonConvert.DeserializeObject<List<AssetExternal>>(dataJson);
					// 	List<AssetExternal> listAssetsOrig = JsonConvert.DeserializeObject<List<AssetExternal>>(dataJsonOriginal);
					// 	var assetOperation = GetOperation<IAssetOperation>();
					// 	LogInfo.SfMappedJson = JsonConvert.SerializeObject(listAssets);
					// 	LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
					// 	_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
					// 	if (listAssets.Count > 0)
					// 	{
					// 		List<ResponseData> sfListResponse = [];
					// 		foreach (AssetExternal elem in listAssets)
					// 		{
					// 			List<AssetExternal> listElem = [];
					// 			DataSyncServiceLogDetail LogSingleInfo = new()
					// 			{
					// 				LogId = LogInfo.Id,
					// 				RowKey = elem.AssetCode,
					// 				ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
					// 				ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "assetCode", elem.AssetCode),
					// 				SfMappedJson = JsonConvert.SerializeObject(elem)
					// 			};
					// 			ResponseData sfResponse = null;
					// 			try
					// 			{
					// 				listElem.Add(elem);
					// 				sfResponse = (await assetOperation.CreateAssetsExternal(
					// 					listElem,
					// 					listAssetsOrig,
					// 					SystemOperator,
					// 					false,
					// 					"Success"
					// 				).ConfigureAwait(false)).FirstOrDefault();
					// 				LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
					// 			}
					// 			catch (Exception ex)
					// 			{
					// 				sfResponse = new ResponseData
					// 				{
					// 					IsSuccess = false,
					// 					Message = ex.Message
					// 				};
					// 			}
					// 			finally
					// 			{
					// 				(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);

					// 				sfListResponse.Add(sfResponse);
					// 			}
					// 		}
					// 		LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
					// 	}
					// 	break;

					case SyncERPEntity.BIN_LOCATION_SERVICE:
						List<BinLocationExternal> listBinLocations = JsonConvert.DeserializeObject<List<BinLocationExternal>>(dataJson);
						List<BinLocationExternal> listBinLocationsOriginal = JsonConvert.DeserializeObject<List<BinLocationExternal>>(dataJsonOriginal);
						var binLocationOperation = GetOperation<IBinLocationOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listBinLocations);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listBinLocations.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (BinLocationExternal elem in listBinLocations)
							{
								List<BinLocationExternal> listElem = [elem];

								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.LocationCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "locationCode", elem.LocationCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								ResponseData sfResponse = null;
								try
								{
									sfResponse = (await binLocationOperation.ListUpdateBinLocation(
									listElem,
									listBinLocationsOriginal,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();
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

					case SyncERPEntity.DEMAND_SERVICE:
						List<DemandExternal> listDemands = JsonConvert.DeserializeObject<List<DemandExternal>>(dataJson);
						List<DemandExternal> listDemandsOriginal = JsonConvert.DeserializeObject<List<DemandExternal>>(dataJsonOriginal);
						var demandOperation = GetOperation<IDemandOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listDemands);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						List<DataSyncServiceLogDetail> returnDetailList = [];
						if (listDemands.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							sfListResponse = await demandOperation.ListUpdateDemandBulk(listDemands, listDemandsOriginal, SystemOperator, false, LevelMessage.Success).ConfigureAwait(false);
							if (sfListResponse is not null)
							{
								foreach (ResponseData rsp in sfListResponse)
								{
									object entity = rsp.Entity;
									object entityAlt = rsp.EntityAlt;
									rsp.Entity = null;
									DataSyncServiceLogDetail LogSingleInfo = new()
									{
										LogId = LogInfo.Id,
										RowKey = rsp.Code,
										ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
										ErpReceivedJson = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }).Replace("\"", "\\\"", StringComparison.Ordinal),
										SfMappedJson = JsonConvert.SerializeObject(entityAlt).Replace("\"", "\\\"", StringComparison.Ordinal),
										ResponseJson = JsonConvert.SerializeObject(rsp).Replace("\"", "\\\"", StringComparison.Ordinal)
									};
									try
									{
										if (rsp.IsSuccess)
										{
											successRecords++;
											LogSingleInfo.LogType = DataSyncLogType.Success;
										}
										else
										{
											throw new Exception(rsp.Message);
										}
										returnDetailList.Add(LogSingleInfo);
									}
									catch (Exception ex)
									{
										failedRecords++;
										LogSingleInfo.LogType = DataSyncLogType.Error;
										LogSingleInfo.MessageException = ex.Message;
										returnDetailList.Add(LogSingleInfo);
									}
								}
								//_ = _dataSyncServiceOperation.InsertDataSyncServiceLogDetailBulk(returnDetailList);
							}
							//LogInfo.SfResponseJson = JsonConvert.SerializeObject(new { SuccessRecords = successRecords, FailedRecords = failedRecords, Data = sfListResponse.Select(x => new { x.Code, x.IsSuccess, x.Message }) });
						}
						break;
						case SyncERPEntity.DEMAND_FORECAST_SERVICE:
						List<DemandExternal> listDemandForecasts = JsonConvert.DeserializeObject<List<DemandExternal>>(dataJson);
						List<DemandExternal> listDemandForecastsOriginal = JsonConvert.DeserializeObject<List<DemandExternal>>(dataJsonOriginal);
						var demandForecastOperation = GetOperation<IDemandOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listDemandForecasts);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						List<DataSyncServiceLogDetail> returndetailList = [];
						if (listDemandForecasts.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							sfListResponse = await demandForecastOperation.ListUpdateDemandBulk(listDemandForecasts, listDemandForecastsOriginal, SystemOperator, false, LevelMessage.Success).ConfigureAwait(false);
							if (sfListResponse is not null)
							{
								foreach (ResponseData rsp in sfListResponse)
								{
									object entity = rsp.Entity;
									object entityAlt = rsp.EntityAlt;
									rsp.Entity = null;
									DataSyncServiceLogDetail LogSingleInfo = new()
									{
										LogId = LogInfo.Id,
										RowKey = rsp.Code,
										ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
										ErpReceivedJson = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }).Replace("\"", "\\\"", StringComparison.Ordinal),
										SfMappedJson = JsonConvert.SerializeObject(entityAlt).Replace("\"", "\\\"", StringComparison.Ordinal),
										ResponseJson = JsonConvert.SerializeObject(rsp).Replace("\"", "\\\"", StringComparison.Ordinal)
									};
									try
									{
										if (rsp.IsSuccess)
										{
											successRecords++;
											LogSingleInfo.LogType = DataSyncLogType.Success;
										}
										else
										{
											throw new Exception(rsp.Message);
										}
										returndetailList.Add(LogSingleInfo);
									}
									catch (Exception ex)
									{
										failedRecords++;
										LogSingleInfo.LogType = DataSyncLogType.Error;
										LogSingleInfo.MessageException = ex.Message;
										returndetailList.Add(LogSingleInfo);
									}
								}
								//_ = _dataSyncServiceOperation.InsertDataSyncServiceLogDetailBulk(returndetailList);
							}
							//LogInfo.SfResponseJson = JsonConvert.SerializeObject(new { SuccessRecords = successRecords, FailedRecords = failedRecords, Data = sfListResponse.Select(x => new { x.Code, x.IsSuccess, x.Message }) });
						}
						break;
					case SyncERPEntity.EMPLOYEE_SERVICE:
						List<EmployeeExternal> listEmployees = JsonConvert.DeserializeObject<List<EmployeeExternal>>(dataJson);
						List<EmployeeExternal> listEmployeesOriginal = JsonConvert.DeserializeObject<List<EmployeeExternal>>(dataJsonOriginal);
						var employeeOperation = GetOperation<IEmployeeOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listEmployees);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listEmployees.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (EmployeeExternal elem in listEmployees)
							{
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.EmployeeCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "employeeCode", elem.EmployeeCode),
									SfMappedJson = JsonConvert.SerializeObject(elem),
									ResponseJson = ""
								};
								ResponseData sfResponse = null;

								try
								{
									List<EmployeeExternal> listElem = [elem];
									sfResponse = (await employeeOperation.ImportEmployeesAsync(
										listElem,
										listEmployeesOriginal,
										SystemOperator,
										false,
										LevelMessage.Success,
										true,
										true
									).ConfigureAwait(false))[0];
									// Se escribe la respuesta del procesamiento
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
							// Asignar fecha de sincronización en configuración a 1900 para que corra consolidación global
							Config.Configuration.UpdateConfiguration("WFM-LastIntervalConsolidated", "1900-01-01 00:00:00");
							LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
						}

						break;

					case SyncERPEntity.INVENTORY_SERVICE:
						List<InventoryExternal> listInventories = JsonConvert.DeserializeObject<List<InventoryExternal>>(dataJson);
						List<InventoryExternal> listInventoryOriginal = JsonConvert.DeserializeObject<List<InventoryExternal>>(dataJsonOriginal);
						var inventoryOperation = GetOperation<IInventoryOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listInventories);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listInventories.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (InventoryExternal elem in listInventories)
							{
								List<InventoryExternal> listElem = [elem];
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.ItemGroupCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "itemGroupCode", elem.ItemGroupCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								ResponseData sfResponse = null;
								try
								{
									sfResponse = (await inventoryOperation.ListUpdateInventoryGroup(
									listElem,
									listInventoryOriginal,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();

									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new ResponseData
									{
										IsSuccess = false,
										Message = ex.Message
									};
									throw;
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

					case SyncERPEntity.INVENTORY_STATUS_SERVICE:
						List<InventoryStatusExternal> listInventoryStatus = JsonConvert.DeserializeObject<List<InventoryStatusExternal>>(dataJson);
						List<InventoryStatusExternal> listInventoryStatusOriginal = JsonConvert.DeserializeObject<List<InventoryStatusExternal>>(dataJsonOriginal);
						var inventoryStatusOperation = GetOperation<IInventoryStatusOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listInventoryStatus);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listInventoryStatus.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (InventoryStatusExternal elem in listInventoryStatus)
							{
								List<InventoryStatusExternal> listElem = [elem];
								ResponseData sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.InventoryStatusCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "inventoryStatusCode", elem.InventoryStatusCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = (await inventoryStatusOperation.ListUpdateInventoryStatus(
										listElem,
										listInventoryStatusOriginal,
										SystemOperator,
										false,
										LevelMessage.Success
									).ConfigureAwait(false)).FirstOrDefault();
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

					case SyncERPEntity.ITEM_SERVICE:
						List<ComponentExternal> listItems = JsonConvert.DeserializeObject<List<ComponentExternal>>(dataJson);
						List<ComponentExternal> listItemsOriginal = JsonConvert.DeserializeObject<List<ComponentExternal>>(dataJsonOriginal);
						var itemOperation = GetOperation<IItemOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listItems);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						List<DataSyncServiceLogDetail> returnDetailListItem = [];
						if (listItems.Count > 0)
						{
							List<ResponseData> sfListResponse = await itemOperation.ListUpdateComponentBulk(listItems, listItemsOriginal, SystemOperator, false, LevelMessage.Success).ConfigureAwait(false);
							if (sfListResponse is not null)
							{
								foreach (ResponseData rsp in sfListResponse)
								{
									object entity = rsp.Entity;
									object entityAlt = rsp.EntityAlt;
									rsp.Entity = null;
									DataSyncServiceLogDetail LogSingleInfo = new()
									{
										LogId = LogInfo.Id,
										RowKey = rsp.Code,
										ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
										ErpReceivedJson = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }).Replace("\"", "\\\"", StringComparison.Ordinal),
										SfMappedJson = JsonConvert.SerializeObject(entityAlt).Replace("\"", "\\\"", StringComparison.Ordinal),
										ResponseJson = JsonConvert.SerializeObject(rsp).Replace("\"", "\\\"", StringComparison.Ordinal)
									};
									try
									{
										if (rsp.IsSuccess)
										{
											successRecords++;
											LogSingleInfo.LogType = DataSyncLogType.Success;
										}
										else
										{
											throw new Exception(rsp.Message);
										}
										returnDetailListItem.Add(LogSingleInfo);
									}
									catch (Exception ex)
									{
										failedRecords++;
										LogSingleInfo.LogType = DataSyncLogType.Error;
										LogSingleInfo.MessageException = ex.Message;
										returnDetailListItem.Add(LogSingleInfo);
										throw;
									}
								}
								//		_ = _dataSyncServiceOperation.InsertDataSyncServiceLogDetailBulk(returnDetailListItem);
							}
							LogInfo.SfResponseJson = JsonConvert.SerializeObject(new { SuccessRecords = successRecords, FailedRecords = failedRecords, Data = sfListResponse.Select(x => new { x.Code, x.IsSuccess, x.Message }) });
						}
						break;
// Not Applicable for any ERP
					// case SyncERPEntity.CLOCKINOUT_SERVICE:
					// 	List<ClockInOutDetailsExternal> listClockInRecords = JsonConvert.DeserializeObject<List<ClockInOutDetailsExternal>>(dataJson);
					// 	List<ClockInOutDetailsExternal> listClockInRecordsOriginal = JsonConvert.DeserializeObject<List<ClockInOutDetailsExternal>>(dataJsonOriginal);
					// 	var workOrderOperation = GetOperation<IWorkOrderOperation>();
					// 	double clockInOffset = await workOrderOperation.GetTimezoneOffset("ERP").ConfigureAwait(false) * -1;
					// 	foreach (ClockInOutDetailsExternal itm in listClockInRecords)
					// 	{
					// 		if (itm.StartDate.HasValue && itm.StartDate.Value.Year <= 1900)
					// 		{
					// 			itm.StartDate = null;
					// 		}
					// 		else
					// 		{
					// 			itm.StartDate = itm.StartDate.Value.AddHours(clockInOffset);
					// 		}
					// 		if (itm.EndDate.HasValue && itm.EndDate.Value.Year <= 1900)
					// 		{
					// 			itm.EndDate = null;
					// 		}
					// 		else if (itm.EndDate.HasValue)
					// 		{
					// 			itm.EndDate = itm.EndDate.Value.AddHours(clockInOffset);
					// 		}
					// 	}
					// 	foreach (ClockInOutDetailsExternal itm in listClockInRecordsOriginal)
					// 	{
					// 		if (itm.StartDate.HasValue && itm.StartDate.Value.Year <= 1900)
					// 		{
					// 			itm.StartDate = null;
					// 		}
					// 		else
					// 		{
					// 			itm.StartDate = itm.StartDate.Value.AddHours(clockInOffset);
					// 		}
					// 		if (itm.EndDate.HasValue && itm.EndDate.Value.Year <= 1900)
					// 		{
					// 			itm.EndDate = null;
					// 		}
					// 		else if (itm.EndDate.HasValue)
					// 		{
					// 			itm.EndDate = itm.EndDate.Value.AddHours(clockInOffset);
					// 		}
					// 	}
					// 	LogInfo.SfMappedJson = JsonConvert.SerializeObject(listClockInRecords);
					// 	LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
					// 	await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
					// 	List<DataSyncServiceLogDetail> returnClockList = [];
					// 	if (listClockInRecords.Count > 0)
					// 	{
					// 		List<ResponseData> sfListResponse = workOrderOperation.ListUpdateCLockInOutBulk(listClockInRecords, listClockInRecordsOriginal, SystemOperator, false, LevelMessage.Success);
					// 		if (sfListResponse is not null)
					// 		{
					// 			sfListResponse.ForEach(rsp =>
					// 			{
					// 				object entity = rsp.Entity;
					// 				object entityAlt = rsp.EntityAlt;
					// 				rsp.Entity = null;
					// 				DataSyncServiceLogDetail LogSingleInfo = new()
					// 				{
					// 					LogId = LogInfo.Id,
					// 					RowKey = rsp.Code,
					// 					ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
					// 					ErpReceivedJson = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }).Replace("\"", "\\\""),
					// 					SfMappedJson = JsonConvert.SerializeObject(entityAlt).Replace("\"", "\\\""),
					// 					ResponseJson = JsonConvert.SerializeObject(rsp).Replace("\"", "\\\"")
					// 				};
					// 				try
					// 				{
					// 					if (rsp.IsSuccess)
					// 					{
					// 						successRecords++;
					// 						LogSingleInfo.LogType = DataSyncLogType.Success;
					// 					}
					// 					else
					// 					{
					// 						throw new Exception(rsp.Message);
					// 					}
					// 					returnClockList.Add(LogSingleInfo);
					// 				}
					// 				catch (Exception ex)
					// 				{
					// 					failedRecords++;
					// 					LogSingleInfo.LogType = DataSyncLogType.Error;
					// 					LogSingleInfo.MessageException = ex.Message;
					// 					returnClockList.Add(LogSingleInfo);
					// 				}
					// 			});
					// 			//_ = _dataSyncServiceOperation.InsertDataSyncServiceLogDetailBulk(returnClockList);
					// 		}
					// 		//LogInfo.SfResponseJson = JsonConvert.SerializeObject(new { SuccessRecords = successRecords, FailedRecords = failedRecords, Data = sfListResponse.Select(x => new { x.Code, x.IsSuccess, x.Message }) });
					// 	}
					// 	break;

					case SyncERPEntity.LOT_SERIAL_STATUS_SERVICE:
						List<LotSerialStatusExternal> listLotSerialStatus = JsonConvert.DeserializeObject<List<LotSerialStatusExternal>>(dataJson);
						List<LotSerialStatusExternal> listLotSerialStatusOriginal = JsonConvert.DeserializeObject<List<LotSerialStatusExternal>>(dataJsonOriginal);
						var lotSerialStatusOperation = GetOperation<ILotSerialStatusOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listLotSerialStatus);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listLotSerialStatus.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (LotSerialStatusExternal elem in listLotSerialStatus)
							{
								List<LotSerialStatusExternal> listElem = [elem];
								ResponseData sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.LotSerialStatusCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "lotSerialStatusCode", elem.LotSerialStatusCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = (await lotSerialStatusOperation.ListUpdateLotSerialStatus(
									listElem,
									listLotSerialStatusOriginal,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();
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

					case SyncERPEntity.MACHINE_SERVICE:
						List<MachineExternal> listMachines = JsonConvert.DeserializeObject<List<MachineExternal>>(dataJson);
						List<MachineExternal> listMachinesOriginal = JsonConvert.DeserializeObject<List<MachineExternal>>(dataJsonOriginal);
						var deviceOperation = GetOperation<IDeviceOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listMachines);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listMachines.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (MachineExternal elem in listMachines)
							{
								List<MachineExternal> listElem = [elem];
								ResponseData sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.MachineCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "machineCode", elem.MachineCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = (await deviceOperation.ListUpdateMachine(
										listElem,
										listMachinesOriginal,
										SystemOperator,
										false,
										"Success"
									).ConfigureAwait(false)).FirstOrDefault();

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
// Not implemented 
					// case SyncERPEntity.MACHINE_ISSUE_SERVICE:
					// 	List<MachineIssueExternal> listMachineIssues = JsonConvert.DeserializeObject<List<MachineIssueExternal>>(dataJson);
					// 	LogInfo.SfMappedJson = JsonConvert.SerializeObject(listMachineIssues);
					// 	LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);

					// 	await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
					// 	if (listMachineIssues.Count > 0)
					// 	{
					// 		List<ResponseData> sfListResponse = [];
					// 		foreach (MachineIssueExternal elem in listMachineIssues)
					// 		{
					// 			List<MachineIssueExternal> listElem = [elem];
					// 			ResponseData sfResponse = null;
					// 			DataSyncServiceLogDetail LogSingleInfo = new()
					// 			{
					// 				LogId = LogInfo.Id,
					// 				RowKey = elem.DocCode,
					// 				ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
					// 				ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "docCode", elem.DocCode),
					// 				SfMappedJson = JsonConvert.SerializeObject(elem)
					// 			};
					// 			try
					// 			{
					// 				//TODO: Implementar metodo de Machine Issue GET
					// 				/*
                    //                 sfResponse = (await _dataSyncServiceOperation.ListUpdateMaterialIssue(
                    //                 listElem,
                    //                 SystemOperator,
                    //                 false,
                    //                 LevelMessage.Success
                    //                 ).ConfigureAwait(false)).FirstOrDefault();
                    //                 */
					// 				LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
					// 			}
					// 			catch (Exception ex)
					// 			{
					// 				sfResponse = new ResponseData
					// 				{
					// 					IsSuccess = false,
					// 					Message = ex.Message
					// 				};
					// 			}
					// 			finally
					// 			{
					// 				(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);

					// 				sfListResponse.Add(sfResponse);
					// 			}
					// 		}
					// 		LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
					// 	}
					// 	break;

					case SyncERPEntity.MATERIAL_ISSUE_SERVICE:
						List<MaterialIssueExternal> listMaterialIssues = JsonConvert.DeserializeObject<List<MaterialIssueExternal>>(dataJson);
						var orderTransactionMaterialOperation = GetOperation<IOrderTransactionMaterialOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listMaterialIssues);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listMaterialIssues.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (MaterialIssueExternal elem in listMaterialIssues)
							{
								List<MaterialIssueExternal> listElem = [elem];
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
									sfResponse = (await orderTransactionMaterialOperation.ListUpdateMaterialIssue(
									listElem,
									SystemOperator,
									false,
									LevelMessage.Success
									).ConfigureAwait(false)).FirstOrDefault();

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

					case SyncERPEntity.MATERIAL_RETURN_SERVICE:
						List<MaterialReturnExternal> listMaterialReturns = JsonConvert.DeserializeObject<List<MaterialReturnExternal>>(dataJson);
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listMaterialReturns);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						var orderTransactionMaterialReturnOperation = GetOperation<IOrderTransactionMaterialOperation>();
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listMaterialReturns.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (MaterialReturnExternal elem in listMaterialReturns)
							{
								List<MaterialReturnExternal> listElem = [elem];
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.DocCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "docCode", elem.DocCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								ResponseData sfResponse = null;
								try
								{
									sfResponse = (await orderTransactionMaterialReturnOperation.ListUpdateMaterialReturn(
											listElem,
											SystemOperator,
											false,
											LevelMessage.Success
										).ConfigureAwait(false)).FirstOrDefault();
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
						// MaterialReturnExternal materialReturn = JsonConvert.DeserializeObject<MaterialReturnExternal>(dataJson);
						// var orderTransactionMaterialReturnOperation = GetOperation<IOrderTransactionMaterialOperation>();
						// if (materialReturn is not null)
						// {
						// 	List<MaterialReturnExternal> listElem = [materialReturn];
						// 	ResponseData sfResponse = (await orderTransactionMaterialReturnOperation.ListUpdateMaterialReturn(
						// 		listElem,
						// 		SystemOperator,
						// 		false,
						// 		LevelMessage.Success
						// 	).ConfigureAwait(false)).FirstOrDefault();

						// DataSyncServiceLogDetail LogSingleInfo = new()
						// {
						// 	Id = LogDetail.Id,
						// 	LogId = LogInfo.Id,
						// 	AttemptNo = LogDetail.AttemptNo + 1,
						// 	LastAttemptDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone)
						// };
						// (successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);
						//}
						break;

					case SyncERPEntity.POSITION_SERVICE:
						List<PositionExternal> listPositions = JsonConvert.DeserializeObject<List<PositionExternal>>(dataJson);
						List<PositionExternal> listPositionsOriginal = JsonConvert.DeserializeObject<List<PositionExternal>>(dataJsonOriginal);
						var profileOperation = GetOperation<IProfileOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listPositions);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listPositions.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (PositionExternal elem in listPositions)
							{
								List<PositionExternal> listElem = [elem];
								ResponseData sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.PositionCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "positionCode", elem.PositionCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = (await profileOperation.ListUpdateProfile(
									listElem,
									listPositionsOriginal,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();
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

					case SyncERPEntity.PRODUCT_SERVICE:
						List<ProductExternal> listProducts = JsonConvert.DeserializeObject<List<ProductExternal>>(dataJson);
						List<ProductExternal> listProductsOriginal = JsonConvert.DeserializeObject<List<ProductExternal>>(dataJsonOriginal);
						var productOperation = GetOperation<IComponentOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listProducts);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listProducts.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (ProductExternal elem in listProducts)
							{
								List<ProductExternal> listElem = [elem];
								ResponseData sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.ProductCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "productCode", elem.ProductCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = (await productOperation.ListUpdateProduct(
									listElem,
									listProductsOriginal,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();

									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new ResponseData
									{
										IsSuccess = false,
										Message = ex.Message
									};
									LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
									throw;
								}
								finally
								{
									//(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);

									sfListResponse.Add(sfResponse);
								}
							}
							LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
						}
						break;

					case SyncERPEntity.PRODUCTION_ORDER_CHANGE_STATUS_SERVICE:
						List<ProductionOrderChangeStatusExternal> listWorkOrderChangeStatus = JsonConvert.DeserializeObject<List<ProductionOrderChangeStatusExternal>>(dataJson);
						var orderchangeOperation = GetOperation<IWorkOrderOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listWorkOrderChangeStatus);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listWorkOrderChangeStatus.Count > 0)
						{
							List<WorkOrderResponse> sfListResponse = [];
							foreach (ProductionOrderChangeStatusExternal elem in listWorkOrderChangeStatus)
							{
								List<ProductionOrderChangeStatusExternal> listElem = [elem];
								WorkOrderResponse sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.OrderCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "orderCode", elem.OrderCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = orderchangeOperation.ListUpdateWorkOrderChangeStatus(
									listElem,
									SystemOperator,
									false,
									LevelMessage.Success
								).FirstOrDefault();
									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new WorkOrderResponse
									{
										IsSuccess = false,
										Message = ex.Message
									};
									LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
									throw;
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

					case SyncERPEntity.PRODUCTION_ORDER_SERVICE:
						List<WorkOrderExternal> listWorkOrders = JsonConvert.DeserializeObject<List<WorkOrderExternal>>(dataJson);
						var productOrderOperation = GetOperation<IWorkOrderOperation>();
						double offset = await productOrderOperation.GetTimezoneOffset("ERP").ConfigureAwait(false) * -1;
						listWorkOrders.ForEach(elem => productOrderOperation.AddWorkOrderDatesOffset(elem, offset));
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listWorkOrders);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listWorkOrders.Count > 0)
						{
							List<WorkOrderResponse> sfListResponse = [];
							foreach (WorkOrderExternal elem in listWorkOrders)
							{
								List<WorkOrderExternal> listElem = [elem];

								//Procesar Elemento Order para Fechas UTC

								//FIN Procesamiento
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.OrderCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "orderCode", elem.OrderCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								WorkOrderResponse sfResponse = null;
								try
								{
									sfResponse = (await productOrderOperation.ListUpdateWorkOrder(
									listElem,
									SystemOperator,
									false,
									LevelMessage.Success,
									true
								).ConfigureAwait(false)).FirstOrDefault();
									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new WorkOrderResponse
									{
										IsSuccess = false,
										Message = ex.Message
									};
									LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
									throw;
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

					case SyncERPEntity.PRODUCT_RECEIPT_SERVICE:
						List<ProductReceiptExternal> listProductReceipts = JsonConvert.DeserializeObject<List<ProductReceiptExternal>>(dataJson);
						var orderTransactionProductOperation = GetOperation<IOrderTransactionProductOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listProductReceipts);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listProductReceipts.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (ProductReceiptExternal elem in listProductReceipts)
							{
								List<ProductReceiptExternal> listElem = [elem];
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.DocCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "docCode", elem.DocCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								ResponseData sfResponse = null;
								try
								{
									sfResponse = (await orderTransactionProductOperation.ListUpdateProductReceipt(
									listElem,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();
									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new ResponseData
									{
										IsSuccess = false,
										Message = ex.Message
									};
									LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
									throw;
								}
								finally
								{
									(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);

									sfListResponse.Add(sfResponse);
								}
							}
							LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
							//string orderId = string.Join(",", listProductReceipts.Select(x => x.OrderCode).Distinct().ToArray());
							//ServiceManager.SendMessage(MessageBrokerType.WorkOrder, new { Type = "U", Id = orderId });
						}
						break;

					case SyncERPEntity.PRODUCT_RETURN_SERVICE:
						List<ProductReturnExternal> listProductReturns = JsonConvert.DeserializeObject<List<ProductReturnExternal>>(dataJson);
						var orderTransactionProductReturnOperation = GetOperation<IOrderTransactionProductOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listProductReturns);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listProductReturns.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (ProductReturnExternal elem in listProductReturns)
							{
								List<ProductReturnExternal> listElem = [elem];
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
									sfResponse = (await orderTransactionProductReturnOperation.ListUpdateProductReturn(
									listElem,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();
									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new ResponseData
									{
										IsSuccess = false,
										Message = ex.Message
									};
									LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
									throw;
								}
								finally
								{
									(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);

									sfListResponse.Add(sfResponse);
								}
							}
							LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
							//string orderId = string.Join(",", listProductReturns.Select(x => x.OrderCode).Distinct().ToArray());
							//ServiceManager.SendMessage(MessageBrokerType.WorkOrder, new { Type = "U", Id = orderId });
						}
						break;

					case SyncERPEntity.PRODUCT_TRANSFER_SERVICE:
						List<ProductTransferExternal> listProductTransfers = JsonConvert.DeserializeObject<List<ProductTransferExternal>>(dataJson);
						List<ProductTransferExternal> listProductsTransfersOriginal = JsonConvert.DeserializeObject<List<ProductTransferExternal>>(dataJsonOriginal);
						var productTransfer = GetOperation<IWorkOrderOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listProductTransfers);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listProductTransfers.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (ProductTransferExternal elem in listProductTransfers)
							{
								List<ProductTransferExternal> listElem = [elem];
								ResponseData sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.OrderCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "orderCode", elem.OrderCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = (await productTransfer.ListUpdateProductTransfer(
									listElem,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();

									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new ResponseData
									{
										IsSuccess = false,
										Message = ex.Message
									};
									LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
									throw;
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

					case SyncERPEntity.MATERIAL_SCRAP_SERVICE:
						List<MaterialIssueExternal> listMaterialScrapIssues = JsonConvert.DeserializeObject<List<MaterialIssueExternal>>(dataJson);
						var materialScrapOperation = GetOperation<IOrderTransactionMaterialOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listMaterialScrapIssues);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listMaterialScrapIssues.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (MaterialIssueExternal elem in listMaterialScrapIssues)
							{
								List<MaterialIssueExternal> listElem = [elem];
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
									sfResponse = (await materialScrapOperation.ListUpdateMaterialScrap(
									listElem,
									SystemOperator,
									false,
									LevelMessage.Success
									).ConfigureAwait(false)).FirstOrDefault();

									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new ResponseData
									{
										IsSuccess = false,
										Message = ex.Message
									};
									LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
									throw;
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

					// case SyncERPEntity.PROCEDURE_SERVICE:
					// 	List<ProcedureExternalSync> listProcedures = JsonConvert.DeserializeObject<List<ProcedureExternalSync>>(dataJson);
					// 	List<ProcedureExternalSync> listProceduresOriginal = JsonConvert.DeserializeObject<List<ProcedureExternalSync>>(dataJsonOriginal);
					// 	var procedureOperation = GetOperation<IProcedureOperation>();
					// 	LogInfo.SfMappedJson = JsonConvert.SerializeObject(listProcedures);
					// 	LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
					// 	await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
					// 	if (listProcedures.Count > 0)
					// 	{
					// 		List<ResponseData> sfListResponse = [];
					// 		foreach (ProcedureExternalSync elem in listProcedures)
					// 		{
					// 			List<ProcedureExternalSync> listElem = [elem];
					// 			ResponseData sfResponse = null;
					// 			DataSyncServiceLogDetail LogSingleInfo = new()
					// 			{
					// 				LogId = LogInfo.Id,
					// 				RowKey = elem.ProcedureCode,
					// 				ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
					// 				ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "Code", elem.ProcedureCode),
					// 				SfMappedJson = JsonConvert.SerializeObject(elem)
					// 			};
					// 			try
					// 			{
					// 				sfResponse = (await procedureOperation.ProcessMasterInsExternalSync(
					// 				listElem,
					// 				listProceduresOriginal,
					// 				SystemOperator,
					// 				false,
					// 				LevelMessage.Success
					// 				).ConfigureAwait(false)).FirstOrDefault();

					// 				LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
					// 			}
					// 			catch (Exception ex)
					// 			{
					// 				sfResponse = new ResponseData
					// 				{
					// 					IsSuccess = false,
					// 					Message = ex.Message
					// 				};
					// 			}
					// 			finally
					// 			{
					// 				(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);

					// 				sfListResponse.Add(sfResponse);
					// 			}
					// 		}
					// 		LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
					// 	}
					// 	break;

					case SyncERPEntity.STOCK_SERVICE:
					case SyncERPEntity.FULL_STOCK_SERVICE:
					List<StockExternal> stocks = JsonConvert.DeserializeObject<List<StockExternal>>(dataJson);
					var stockOperation = GetOperation<IStockOperation>();
						if (stocks != null && stocks.Any())
						//StockExternal stock = JsonConvert.DeserializeObject<StockExternal>(dataJson);
						{
							List<StockExternal> listElem = stocks;
							ResponseData sfResponse = null;
							// ResponseData sfResponse = stockOperation.ListUpdateStockBulk(
							// 	listElem,
							// 	SystemOperator,
							// 	false,
							// 	LevelMessage.Success
							// );
							try
							{
								sfResponse = (await stockOperation.ListUpdateStockBulk(
								listElem,
								SystemOperator,
								false,
								LevelMessage.Success
							).ConfigureAwait(false));

								// LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
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
								// (successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);
							}
						}

							// DataSyncServiceLogDetail LogSingleInfo = new()
							// {
							// 	Id = LogDetail.Id,
							// 	LogId = LogInfo.Id,
							// 	AttemptNo = LogDetail.AttemptNo + 1,
							// 	LastAttemptDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone)
							// };
							//(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);
						//}
						break;

					case SyncERPEntity.SUPPLY_SERVICE:
						List<SupplyExternal> supplies = JsonConvert.DeserializeObject<List<SupplyExternal>>(dataJson);

						var supplyOperation = GetOperation<ISupplyOperation>();

						if (supplies != null && supplies.Any())
						{
							// No need to wrap in a list anymore — it's already a list
							List<SupplyExternal> listElem = supplies;

							ResponseData sfResponse = null;

							// Optional: Uncomment and update log if needed
							// DataSyncServiceLogDetail LogSingleInfo = new()
							// {
							//     Id = LogDetail.Id,
							//     LogId = LogInfo.Id,
							//     AttemptNo = LogDetail.AttemptNo + 1,
							//     LastAttemptDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone)
							// };

							try
							{
								sfResponse = (await supplyOperation.ListUpdateSupply(
									listElem,
									null,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();

								// LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
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
								// (successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);
							}
						}
						break;
						case SyncERPEntity.SUPPLY_MRP_SERVICE:
						List<SupplyExternal> supplieMRPs = JsonConvert.DeserializeObject<List<SupplyExternal>>(dataJson);

						var supplyMRPOperation = GetOperation<ISupplyOperation>();

						if (supplieMRPs != null && supplieMRPs.Any())
						{
							// No need to wrap in a list anymore — it's already a list
							List<SupplyExternal> listElem = supplieMRPs;

							ResponseData sfResponse = null;

							// Optional: Uncomment and update log if needed
							// DataSyncServiceLogDetail LogSingleInfo = new()
							// {
							//     Id = LogDetail.Id,
							//     LogId = LogInfo.Id,
							//     AttemptNo = LogDetail.AttemptNo + 1,
							//     LastAttemptDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone)
							// };

							try
							{
								sfResponse = (await supplyMRPOperation.ListUpdateSupply(
									listElem,
									null,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();

								// LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
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
								// (successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);
							}
						}
						break;

					case SyncERPEntity.UNIT_MEASURE_SERVICE:
						List<MeasureUnitExternal> listMeasureUnits = JsonConvert.DeserializeObject<List<MeasureUnitExternal>>(dataJson);
						List<MeasureUnitExternal> listMeasureUnitsOriginal = JsonConvert.DeserializeObject<List<MeasureUnitExternal>>(dataJsonOriginal);
						var measureUnitOperation = GetOperation<IMeasureUnitOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listMeasureUnits);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listMeasureUnits.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (MeasureUnitExternal elem in listMeasureUnits)
							{
								List<MeasureUnitExternal> listElem = [elem];
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.UoMCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "uoMCode", elem.UoMCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								ResponseData sfResponse = null;
								try
								{
									sfResponse = (await measureUnitOperation.ListUpdateUnitMeasure(
									listElem,
									listMeasureUnitsOriginal,
									SystemOperator,
									false,
									LevelMessage.Success
								).ConfigureAwait(false)).FirstOrDefault();
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

					case SyncERPEntity.WAREHOUSE_SERVICE:
						List<WarehouseExternal> listWarehouses = JsonConvert.DeserializeObject<List<WarehouseExternal>>(dataJson);
						List<WarehouseExternal> listWarehousesOriginal = JsonConvert.DeserializeObject<List<WarehouseExternal>>(dataJsonOriginal);
						var warehouseOperation = GetOperation<IWarehouseOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listWarehouses);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						// _ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listWarehouses.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (WarehouseExternal elem in listWarehouses)
							{
								List<WarehouseExternal> listElem = [elem];
								ResponseData sfResponse = null;
								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.WarehouseCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "warehouseCode", elem.WarehouseCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = (await warehouseOperation.ListUpdateWarehouseGroup(
										listElem,
										listWarehousesOriginal,
										SystemOperator,
										false,
										LevelMessage.Success
									).ConfigureAwait(false)).FirstOrDefault();
									LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
								}
								catch (Exception ex)
								{
									sfResponse = new ResponseData
									{
										IsSuccess = false,
										Message = ex.Message
									};
									
									LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
									throw;
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

					case SyncERPEntity.TOOLING_TYPE_SERVICE:
						List<ToolTypeExternal> listToolType = JsonConvert.DeserializeObject<List<ToolTypeExternal>>(dataJson);
						List<ToolTypeExternal> listToolTypeOriginal = JsonConvert.DeserializeObject<List<ToolTypeExternal>>(dataJsonOriginal);
						var toolOperation = GetOperation<IToolOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listToolType);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listToolType.Count > 0)
						{
							List<ResponseData> sfListResponse = [];
							foreach (ToolTypeExternal elem in listToolType)
							{
								List<ToolTypeExternal> listElem = [elem];
								ResponseData sfResponse = null;

								DataSyncServiceLogDetail LogSingleInfo = new()
								{
									LogId = LogInfo.Id,
									RowKey = elem.ToolingTypeCode,
									ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
									ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "toolingTypeCode", elem.ToolingTypeCode),
									SfMappedJson = JsonConvert.SerializeObject(elem)
								};
								try
								{
									sfResponse = (await toolOperation.ListUpdateToolType(
											listElem,
											listToolTypeOriginal,
											SystemOperator,
											false,
											LevelMessage.Success
										).ConfigureAwait(false)).FirstOrDefault();
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

					case SyncERPEntity.PROCESS_TYPE_SERVICE:
						List<SubProcessTypeExternal> listProcessTypes = JsonConvert.DeserializeObject<List<SubProcessTypeExternal>>(dataJson);
						List<DataSyncServiceLogDetail> returnDetailListProcess = [];
						var processTypeOperation = GetOperation<IProcessTypeOperation>();
						LogInfo.SfMappedJson = JsonConvert.SerializeObject(listProcessTypes);
						LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
						await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
						if (listProcessTypes.Count > 0)
						{
							List<ResponseData> sfListResponse = processTypeOperation.ListUpdateSuboperationTypes_Bulk(listProcessTypes, SystemOperator, false, LevelMessage.Success);
							if (sfListResponse is not null)
							{
								foreach (ResponseData rsp in sfListResponse)
								{
									object entity = rsp.Entity;
									object entityAlt = rsp.EntityAlt;
									rsp.Entity = null;
									DataSyncServiceLogDetail LogSingleInfo = new()
									{
										LogId = LogInfo.Id,
										RowKey = rsp.Code,
										ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
										ErpReceivedJson = JsonConvert.SerializeObject(entity, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }).Replace("\"", "\\\""),
										SfMappedJson = JsonConvert.SerializeObject(entityAlt).Replace("\"", "\\\""),
										ResponseJson = JsonConvert.SerializeObject(rsp).Replace("\"", "\\\"")
									};
									try
									{
										if (rsp.IsSuccess)
										{
											successRecords++;
											LogSingleInfo.LogType = DataSyncLogType.Success;
										}
										else
										{
											throw new Exception(rsp.Message);
										}
										returnDetailListProcess.Add(LogSingleInfo);
									}
									catch (Exception ex)
									{
										failedRecords++;
										LogSingleInfo.LogType = DataSyncLogType.Error;
										LogSingleInfo.MessageException = ex.Message;
										returnDetailListProcess.Add(LogSingleInfo);
									}
								}
								//_ = _dataSyncServiceOperation.InsertDataSyncServiceLogDetailBulk(returnDetailListProcess);
							}
							LogInfo.SfResponseJson = JsonConvert.SerializeObject(new { SuccessRecords = successRecords, FailedRecords = failedRecords, Data = sfListResponse.Select(x => new { x.Code, x.IsSuccess, x.Message }) });
						}

						break;
// Not implemented 
					// case SyncERPEntity.LABOR_ISSUE_SERVICE:
					// 	List<LaborIssueExternal> listLaborIssues = JsonConvert.DeserializeObject<List<LaborIssueExternal>>(dataJson);
					// 	LogInfo.SfMappedJson = JsonConvert.SerializeObject(listLaborIssues);
					// 	LogInfo.SfProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
					// 	await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
					// 	if (listLaborIssues.Count > 0)
					// 	{
					// 		List<ResponseData> sfListResponse = [];
					// 		foreach (LaborIssueExternal elem in listLaborIssues)
					// 		{
					// 			List<LaborIssueExternal> listElem = [elem];
					// 			ResponseData sfResponse = null;
					// 			DataSyncServiceLogDetail LogSingleInfo = new()
					// 			{
					// 				LogId = LogInfo.Id,
					// 				RowKey = elem.DocCode,
					// 				ProcessDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone),
					// 				ErpReceivedJson = DataSyncServiceUtil.FindObjectByPropertyAndValue(ServiceData.ErpMapping, erpResult.Response, "docCode", elem.DocCode),
					// 				SfMappedJson = JsonConvert.SerializeObject(elem)
					// 			};
					// 			try
					// 			{
					// 				//TODO: Implementar el metodo de sincronización de LaborIssue GET
					// 				/*
                    //                 sfResponse = (await _operations.ListUpdateMaterialIssue(
                    //                 listElem,
                    //                 SystemOperator,
                    //                 false,
                    //                 LevelMessage.Success
                    //                 ).ConfigureAwait(false)).FirstOrDefault();
                    //                 */
					// 				LogSingleInfo.ResponseJson = JsonConvert.SerializeObject(sfResponse);
					// 			}
					// 			catch (Exception ex)
					// 			{
					// 				sfResponse = new ResponseData
					// 				{
					// 					IsSuccess = false,
					// 					Message = ex.Message
					// 				};
					// 			}
					// 			finally
					// 			{
					// 				(successRecords, failedRecords) = await ProcessResponse(sfResponse, successRecords, failedRecords, LogSingleInfo).ConfigureAwait(false);

					// 				sfListResponse.Add(sfResponse);
					// 			}
					// 		}
					// 		LogInfo.SfResponseJson = JsonConvert.SerializeObject(sfListResponse);
					// 	}
					// 	break;
					default:
						throw new Exception("No instance configured to receive data from ERP");
						
				}
				//update last execution time
				if (onResponse is not null && !isError)
						{
							onResponse();
						}
				// Log
				LogInfo.FailedRecords = failedRecords;
				LogInfo.SuccessRecords = successRecords;
				HttpResponse.FailRecords = failedRecords;
				HttpResponse.SuccessRecords = successRecords;
				LogInfo.ExecutionFinishDate = DataSyncServiceUtil.ConvertDate(ServiceData.ErpData.DateTimeFormat, DateTime.Now, ServiceData.ErpData.TimeZone);
                
				//Encender Servicio nuevamente
				 _ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
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
							throw;
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
					throw;

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

	private async Task<DataSyncHttpResponse> SendSfDataToErp(DataSyncServiceLog LogInfo, DataSyncService ServiceData, User SystemOperator, ServiceExecOrigin ExecOrigin, string RequestBody, string EntityCode, DataSyncHttpResponse HttpResponse, Action onResponse)
	{
		if (string.IsNullOrEmpty(RequestBody))
		{

		switch (ServiceData.Entity.Name)
		{
			// Not Applicable for any ERP
			case SyncERPEntity.MATERIAL_ISSUE_SERVICE:
				// Get WorkOrderOperation service to call GetMaterialTransactionRequestParams
				// This will retrieve all transactions where ExternalId is null or empty
				var workOrderOperation = GetOperation<IWorkOrderOperation>();
				object requestParams = await workOrderOperation.GetMaterialTransactionRequestParams(SystemOperator).ConfigureAwait(false);

				// Check if we need to process transactions individually
				dynamic requestParamsObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(requestParams));
				if (requestParamsObj.ProcessIndividually != null && (bool)requestParamsObj.ProcessIndividually)
				{
					// Process each transaction individually
					return await ProcessMaterialTransactionsIndividually(
						LogInfo, ServiceData, SystemOperator, ExecOrigin,
						requestParamsObj.Transactions, EntityCode, HttpResponse, onResponse
					).ConfigureAwait(false);
				}

				RequestBody = JsonConvert.SerializeObject(requestParams);
				break;

			case SyncERPEntity.ORDER_TRANSACTION_SERVICE:
				// Handle order transaction sync from ERP via SyncProducer endpoint
				// ERP sends TransactionId and ExternalId after processing the material issue
				if (!string.IsNullOrEmpty(RequestBody))
				{
					try
					{
						// Parse the request body from ERP
						dynamic messageData = JsonConvert.DeserializeObject(RequestBody);
						string transactionId = messageData?.TransactionId?.ToString();
						string externalId = messageData?.ExternalId?.ToString();

						if (string.IsNullOrEmpty(transactionId))
						{
							throw new Exception("TransactionId is required in ORDER_TRANSACTION_SERVICE request");
						}

						if (string.IsNullOrEmpty(externalId))
						{
							throw new Exception("ExternalId is required in ORDER_TRANSACTION_SERVICE request");
						}

						_logger.LogInformation("Processing ORDER_TRANSACTION_SERVICE for TransactionId: {TransactionId}, ExternalId: {ExternalId}",
							transactionId, externalId);

						// Update the ExternalId in the database
						var orderTransactionMaterialRepo = GetOperation<IOrderTransactionMaterialRepo>();
						bool updateSuccess = await orderTransactionMaterialRepo.UpdateOrderTransactionMaterialExternalId(
							transactionId,
							externalId,
							SystemOperator
						).ConfigureAwait(false);

						if (updateSuccess)
						{
							HttpResponse.StatusCode = HttpStatusCode.OK;
							HttpResponse.Message = $"Successfully updated ExternalId for transaction {transactionId}";
							_logger.LogInformation("Successfully updated ExternalId for transaction {TransactionId} with ExternalId {ExternalId}",
								transactionId, externalId);
						}
						else
						{
							HttpResponse.StatusCode = HttpStatusCode.InternalServerError;
							HttpResponse.Message = $"Failed to update ExternalId for transaction {transactionId}";
							_logger.LogError("Failed to update ExternalId for transaction {TransactionId}", transactionId);
						}

						return HttpResponse;
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error processing ORDER_TRANSACTION_SERVICE: {Message}", ex.Message);
						HttpResponse.StatusCode = HttpStatusCode.BadRequest;
						HttpResponse.Message = $"Error processing ORDER_TRANSACTION_SERVICE: {ex.Message}";
						return HttpResponse;
					}
				}
				else
				{
					// If no RequestBody, this might be from the internal Kafka notification
					// (published after material issue sync in ProcessMaterialTransactionsIndividually)
					_logger.LogInformation("ORDER_TRANSACTION_SERVICE called without body - transaction already updated");
					HttpResponse.StatusCode = HttpStatusCode.OK;
					HttpResponse.Message = "Order transaction already processed";
					return HttpResponse;
				}

		default:
		throw new Exception("No request body found");
		}
				}
		dynamic requestErpMapped = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.RequestMapSchema, RequestBody) ?? throw new Exception("No data to process");
		string requestErpJson = JsonConvert.SerializeObject(requestErpMapped);
		LogInfo.SfMappedJson = requestErpJson;
		LogInfo.EndpointUrl = ServiceData.ErpData.BaseUrl + ServiceData.Path;
		_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
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
				throw;
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
					//_ = await _dataSyncServiceOperation.InsertDataSyncServiceLogDetail(LogSingleInfo).ConfigureAwait(false);
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
			//_ = await _dataSyncServiceOperation.InsertDataSyncServiceLogDetail(LogSingleInfoFinish).ConfigureAwait(false);
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
			//_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);
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
			//_ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);

			// Handle post-success operations for MATERIAL_ISSUE_SERVICE
			if (ServiceData.Entity.Name == SyncERPEntity.MATERIAL_ISSUE_SERVICE)
			{
				await HandleMaterialIssueSuccess(responseErp, RequestBody, SystemOperator, ServiceData).ConfigureAwait(false);
			}
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
		//_ = await _dataSyncServiceOperation.InsertDataSyncServiceLogDetail(LogSingleInfoFinish).ConfigureAwait(false);

		return HttpResponse;
	}

	private async Task SetRequestHeaders(DataSyncService ServiceData, APIWebClient client)
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
		bool syncAllData = false;  //Config.Configuration["DataSyncService-SyncAll"].ToBool();
		using APIWebClient client = new()
		{
			TimeoutSeconds = ServiceData.RequestTimeoutSecs // Required Timeout
		};
		// Required Headers And Token
		await SetRequestHeaders(ServiceData, client).ConfigureAwait(false);

		string endpointUrl = ServiceData.ErpData.BaseUrl + ServiceData.Path;
		if (!string.IsNullOrEmpty(ServiceData.UrlParams))
		{
			var workOrderOperation = GetOperation<IWorkOrderOperation>();
			double erpOffset = await workOrderOperation.GetTimezoneOffset("ERP");
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
			// _ = await _dataSyncServiceOperation.InsertDataSyncServiceLog(LogInfo).ConfigureAwait(false);  // this method not imlemented
		}
		string dynamicBody = string.Empty;
		if (ServiceData.EnableDynamicBody == 1)
		{
			dynamicBody = _dataSyncServiceOperation.GetDatasyncDynamicBody(ServiceData.Entity.Id);
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

	private async Task<DataSyncResponse> ErpSendRequestAsync(DataSyncService ServiceData, ServiceExecOrigin ExecOrigin, string BodyData = "", bool triggerRevalidate = false, DataSyncServiceLog LogInfo = null)
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
				throw;
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

	private async Task<DataSyncErpAuth> GetErpToken(APIWebClient client, DataSyncService ServiceData)
	{
		DataSyncErpAuth response;
		if (!string.IsNullOrEmpty(ServiceData.TokenData.Token))
		{
			response = ServiceData.TokenData;
		}
		else
		{
			response = _dataSyncServiceOperation.GetDataSyncServiceErpToken(ServiceData.ErpData.ErpCode);
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
						_ = _dataSyncServiceOperation.InsertDataSyncServiceErpToken(response);
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

	private void ErpTokenRenewal(DataSyncService ServiceData, HttpResponseHeaders ResponseHeaders)
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
					_ = _dataSyncServiceOperation.InsertDataSyncServiceErpToken(objAuthRenewal);
				}
			}
		}
	}
	private static async Task<(int successRecords, int failedRecords)> ProcessResponse(ResponseData sfResponse, int successRecords, int failedRecords, DataSyncServiceLogDetail LogSingleInfo)
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
			throw;
		}
		finally
		{
			//_ = await _dataSyncServiceOperation.InsertDataSyncServiceLogDetail(LogSingleInfo).ConfigureAwait(false);
		}
		return (successRecords, failedRecords);
	}
	private static async Task<(int successRecords, int failedRecords)> ProcessResponse(WorkOrderResponse sfResponse, int successRecords, int failedRecords, DataSyncServiceLogDetail LogSingleInfo)
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
			//await _dataSyncServiceOperation.InsertDataSyncServiceLogDetail(LogSingleInfo).ConfigureAwait(false);
		}
		return (successRecords, failedRecords);
	}

	/// <summary>
	/// Processes multiple material transactions individually
	/// Sends separate POST request for each transaction and publishes to Kafka for each
	/// </summary>
	private async Task<DataSyncHttpResponse> ProcessMaterialTransactionsIndividually(
		DataSyncServiceLog LogInfo,
		DataSyncService ServiceData,
		User SystemOperator,
		ServiceExecOrigin ExecOrigin,
		dynamic transactions,
		string EntityCode,
		DataSyncHttpResponse HttpResponse,
		Action onResponse)
	{
		int successCount = 0;
		int failureCount = 0;
		List<string> errorMessages = [];

		// Get Kafka service for publishing messages
		var kafkaService = GetOperation<IKafkaService>();

		// Process each transaction
		foreach (var transaction in transactions)
		{
			try
			{
				string transactionId = transaction.TransactionId;
				_logger.LogInformation("Processing material transaction {TransactionId}", transactionId);

				// Serialize single transaction as request body
				string singleRequestBody = JsonConvert.SerializeObject(transaction);

				// Map to ERP format
				dynamic requestErpMapped = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.RequestMapSchema, singleRequestBody);
				string requestErpJson = JsonConvert.SerializeObject(requestErpMapped);

				// Send to ERP
				DataSyncResponse erpResult = await ErpSendRequestAsync(ServiceData, ExecOrigin, requestErpJson, false, LogInfo).ConfigureAwait(false);

				if (erpResult.StatusCode == HttpStatusCode.OK)
				{
					// Map response
					dynamic responseErp = DataSyncServiceUtil.MapEntity(ServiceData.ErpMapping.ResponseMapSchema, erpResult.Response);
					string stsResponse = responseErp["Status"];

					if (stsResponse?.Trim() == "Success")
					{
						// Extract ExternalId from response - parse docNum from mapped Message property
						string externalId = null;

						try
						{
							// The ResponseMapSchema maps "message" to "Message" property
							// Parse Message property to get data.docNum
							string messageJson = responseErp["Message"]?.ToString();

							if (!string.IsNullOrEmpty(messageJson))
							{
								JObject messageObject = JObject.Parse(messageJson);

								// Try to get data.docNum from the message object
								if (messageObject["data"] != null)
								{
									var docNum = messageObject["data"]["docNum"];
									if (docNum != null && !string.IsNullOrEmpty(docNum.ToString()))
									{
										externalId = docNum.ToString();
										_logger.LogInformation("Extracted docNum from Message.data: {ExternalId}", externalId);
									}
								}
							}
						}
						catch (Exception ex)
						{
							_logger.LogWarning(ex, "Failed to parse docNum from Message property, trying fallback fields");
						}

						// Fallback to standard fields if docNum parsing failed
						if (string.IsNullOrEmpty(externalId))
						{
							externalId = responseErp["ExternalId"] ?? responseErp["DocEntry"] ?? responseErp["Id"];
							if (!string.IsNullOrEmpty(externalId))
							{
								_logger.LogInformation("Using fallback ExternalId: {ExternalId}", externalId);
							}
						}

						if (!string.IsNullOrEmpty(externalId))
						{
							// Publish to Kafka - ORDER_TRANSACTION_SERVICE will handle the database update
							string topic = $"producer-sync-{SyncERPEntity.ORDER_TRANSACTION_SERVICE.ToLower()}";
							string key = $"order-transaction-{transactionId}-{Guid.NewGuid()}";

							// Create BodyData with TransactionId and ExternalId
							var bodyData = new
							{
								TransactionId = transactionId,
								ExternalId = externalId
							};

							var kafkaMessage = new SyncMessage
							{
								Service = SyncERPEntity.ORDER_TRANSACTION_SERVICE,
								Trigger = TriggerType.SmartFactory.ToString(),
								ExecutionType = (int)ServiceExecOrigin.Event,
								EntityCode = string.Empty,
								BodyData = JsonConvert.SerializeObject(bodyData)
								// ServiceData not needed - ProcessOrderTransactionService doesn't require it
							};

							await kafkaService.ProduceMessageAsync(topic, key, kafkaMessage).ConfigureAwait(false);

							successCount++;
							_logger.LogInformation("Successfully synced transaction {TransactionId} to ERP with ExternalId {ExternalId}, published to Kafka for database update",
								transactionId, externalId);
						}
						else
						{
							failureCount++;
							errorMessages.Add($"Transaction {transactionId}: No ExternalId in ERP response");
							_logger.LogWarning("No ExternalId in response for transaction {TransactionId}", transactionId);
						}
					}
					else
					{
						failureCount++;
						string errorMsg = responseErp["Message"] ?? "Unknown error";
						errorMessages.Add($"Transaction {transactionId}: {errorMsg}");
						_logger.LogError("ERP returned error for transaction {TransactionId}: {Error}", transactionId, errorMsg);
					}
				}
				else
				{
					failureCount++;
					errorMessages.Add($"Transaction {transactionId}: HTTP {erpResult.StatusCode} - {erpResult.StatusMessage}");
					_logger.LogError("HTTP error for transaction {TransactionId}: {StatusCode} - {Message}",
						transactionId, erpResult.StatusCode, erpResult.StatusMessage);
				}
			}
			catch (Exception ex)
			{
				failureCount++;
				string transactionId = transaction.TransactionId ?? "Unknown";
				errorMessages.Add($"Transaction {transactionId}: {ex.Message}");
				_logger.LogError(ex, "Exception processing transaction {TransactionId}", transactionId);
			}
		}

		// Build final response
		HttpResponse.StatusCode = failureCount == 0 ? HttpStatusCode.OK : HttpStatusCode.PartialContent;
		HttpResponse.Message = $"Processed {successCount + failureCount} transactions. Success: {successCount}, Failed: {failureCount}";

		if (errorMessages.Count > 0)
		{
			HttpResponse.Message += $"\nErrors:\n{string.Join("\n", errorMessages)}";
		}

		_logger.LogInformation("Batch processing complete. Success: {SuccessCount}, Failed: {FailureCount}",
			successCount, failureCount);

		return HttpResponse;
	}

	/// <summary>
	/// Public method to handle ORDER_TRANSACTION_SERVICE from Kafka consumer
	/// Updates ExternalId in database based on TransactionId and ExternalId in RequestBody
	/// </summary>
	public async Task<DataSyncHttpResponse> ProcessOrderTransactionService(string requestBody, User systemOperator)
	{
		DataSyncHttpResponse httpResponse = new();

		try
		{
			// Parse the request body from Kafka message
			dynamic messageData = JsonConvert.DeserializeObject(requestBody);
			string transactionId = messageData?.TransactionId?.ToString();
			string externalId = messageData?.ExternalId?.ToString();

			if (string.IsNullOrEmpty(transactionId))
			{
				throw new Exception("TransactionId is required in ORDER_TRANSACTION_SERVICE request");
			}

			if (string.IsNullOrEmpty(externalId))
			{
				throw new Exception("ExternalId is required in ORDER_TRANSACTION_SERVICE request");
			}

			_logger.LogInformation("Processing ORDER_TRANSACTION_SERVICE for TransactionId: {TransactionId}, ExternalId: {ExternalId}",
				transactionId, externalId);

			// Update the ExternalId in the database
			var orderTransactionMaterialRepo = GetOperation<IOrderTransactionMaterialRepo>();
			bool updateSuccess = await orderTransactionMaterialRepo.UpdateOrderTransactionMaterialExternalId(
				transactionId,
				externalId,
				systemOperator
			).ConfigureAwait(false);

			if (updateSuccess)
			{
				httpResponse.StatusCode = HttpStatusCode.OK;
				httpResponse.Message = $"Successfully updated ExternalId for transaction {transactionId}";
				_logger.LogInformation("Successfully updated ExternalId for transaction {TransactionId} with ExternalId {ExternalId}",
					transactionId, externalId);
			}
			else
			{
				httpResponse.StatusCode = HttpStatusCode.InternalServerError;
				httpResponse.Message = $"Failed to update ExternalId for transaction {transactionId}";
				_logger.LogError("Failed to update ExternalId for transaction {TransactionId}", transactionId);
			}

			return httpResponse;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing ORDER_TRANSACTION_SERVICE: {Message}", ex.Message);
			httpResponse.StatusCode = HttpStatusCode.BadRequest;
			httpResponse.Message = $"Error processing ORDER_TRANSACTION_SERVICE: {ex.Message}";
			return httpResponse;
		}
	}

	/// <summary>
	/// Handles post-success operations for Material Issue Service
	/// Publishes to Kafka for ORDER_TRANSACTION_SERVICE to handle database update
	/// Supports both single transaction and array of transactions
	/// </summary>
	private async Task HandleMaterialIssueSuccess(dynamic responseErp, string requestBody, User systemOperator, DataSyncService serviceData)
	{
		try
		{
			var kafkaService = GetOperation<IKafkaService>();
			string topic = $"producer-sync-{SyncERPEntity.ORDER_TRANSACTION_SERVICE.ToLower()}";

			// Parse request body - could be single object or array
			dynamic requestData = JsonConvert.DeserializeObject(requestBody);

			// Check if request is an array
			bool isArray = requestData is JArray;

			if (isArray)
			{
				// Handle array of transactions
				JArray requestArray = (JArray)requestData;
				JArray responseArray = responseErp is JArray ? (JArray)responseErp : null;

				if (responseArray == null || requestArray.Count != responseArray.Count)
				{
					throw new Exception($"Request/Response count mismatch. Request: {requestArray.Count}, Response: {responseArray?.Count ?? 0}");
				}

				// Process each transaction
				for (int i = 0; i < requestArray.Count; i++)
				{
					dynamic request = requestArray[i];
					dynamic response = responseArray[i];

					string transactionId = request["TransactionId"];
					string externalId = response["ExternalId"] ?? response["DocEntry"] ?? response["Id"];

					if (string.IsNullOrEmpty(transactionId))
					{
						_logger.LogWarning("TransactionId not found in request at index {Index}", i);
						continue;
					}

					if (string.IsNullOrEmpty(externalId))
					{
						_logger.LogWarning("ExternalId not found in response for transaction {TransactionId}", transactionId);
						continue;
					}

					// Publish to Kafka - ORDER_TRANSACTION_SERVICE will handle the database update
					string key = $"order-transaction-{transactionId}-{Guid.NewGuid()}";

					// Create BodyData with TransactionId and ExternalId
					var bodyData = new
					{
						TransactionId = transactionId,
						ExternalId = externalId
					};

					var kafkaMessage = new SyncMessage
					{
						Service = SyncERPEntity.ORDER_TRANSACTION_SERVICE,
						Trigger = TriggerType.SmartFactory.ToString(),
						ExecutionType = (int)ServiceExecOrigin.Event,
						EntityCode = string.Empty,
						BodyData = JsonConvert.SerializeObject(bodyData)
					};

					await kafkaService.ProduceMessageAsync(topic, key, kafkaMessage).ConfigureAwait(false);
					_logger.LogInformation("Published Kafka message for transaction {TransactionId} with ExternalId {ExternalId}", transactionId, externalId);
				}
			}
			else
			{
				// Handle single transaction (backward compatibility)
				string transactionId = requestData["TransactionId"];
				string externalId = responseErp["ExternalId"] ?? responseErp["DocEntry"] ?? responseErp["Id"];

				if (string.IsNullOrEmpty(transactionId))
				{
					throw new Exception("TransactionId not found in request body");
				}

				if (string.IsNullOrEmpty(externalId))
				{
					throw new Exception("ExternalId not found in ERP response");
				}

				// Publish to Kafka - ORDER_TRANSACTION_SERVICE will handle the database update
				string key = $"order-transaction-{transactionId}-{Guid.NewGuid()}";

				// Create BodyData with TransactionId and ExternalId
				var bodyData = new
				{
					TransactionId = transactionId,
					ExternalId = externalId
				};

				var kafkaMessage = new SyncMessage
				{
					Service = SyncERPEntity.ORDER_TRANSACTION_SERVICE,
					Trigger = TriggerType.SmartFactory.ToString(),
					ExecutionType = (int)ServiceExecOrigin.Event,
					EntityCode = string.Empty,
					BodyData = JsonConvert.SerializeObject(bodyData)
					// ServiceData not needed - ProcessOrderTransactionService doesn't require it
				};

				await kafkaService.ProduceMessageAsync(topic, key, kafkaMessage).ConfigureAwait(false);
				_logger.LogInformation("Published Kafka message for transaction {TransactionId} with ExternalId {ExternalId}", transactionId, externalId);
			}
		}
		catch (Exception ex)
		{
			// Log error but don't fail the main operation
			// The material issue was successful in ERP, this is just post-processing
			_logger.LogError(ex, "Post-success processing failed: {Message}", ex.Message);
			throw new Exception($"Post-success processing failed: {ex.Message}", ex);
		}
	}

}
