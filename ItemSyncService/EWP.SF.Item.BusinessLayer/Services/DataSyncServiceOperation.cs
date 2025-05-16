
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EWP.SF.Item.BusinessLayer;

public class DataSyncServiceOperation : IDataSyncServiceOperation
{
	private readonly IDataSyncRepository _dataSyncRepository;

	public DataSyncServiceOperation(IDataSyncRepository dataSyncRepository)
	{
		_dataSyncRepository = dataSyncRepository;
	}
	#region DataSync

	// public async Task DatasyncTempServiceLogAsync(string EntityCode, string mode, string Exception = "")
	// {
	// 	if (string.Equals(mode, "START", StringComparison.OrdinalIgnoreCase) || string.Equals(mode, "END", StringComparison.OrdinalIgnoreCase) || ApplicationSettings.Instance.GetAppSetting("LogTimedServices").ToBool())
	// 	{
	// 		await _dataSyncRepository.DatasyncTempServiceLogAsync(EntityCode, mode, Exception, default).ConfigureAwait(false);
	// 	}
	// }

	// public List<DataSyncCatalog> ListDataSyncErp(User systemOperator, DataSyncCatalogFilter searchFilter)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	// searchFilter.LanguageCode = systemOperator.LanguageCode;
	// 	return _dataSyncRepository.ListDataSyncErp(searchFilter);
	// }

	// public List<DataSyncCatalog> ListDataSyncErpVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	// searchFilter.LanguageCode = systemOperator.LanguageCode;
	// 	return _dataSyncRepository.ListDataSyncErpVersion(searchFilter);
	// }

	// public List<DataSyncCatalog> ListDataSyncErpDatabase(User systemOperator, DataSyncCatalogFilter searchFilter)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	// searchFilter.LanguageCode = systemOperator.LanguageCode;
	// 	return _dataSyncRepository.ListDataSyncErpDatabase(searchFilter);
	// }

	// public List<DataSyncCatalog> ListDataSyncErpDatabaseVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	// searchFilter.LanguageCode = systemOperator.LanguageCode;
	// 	return _dataSyncRepository.ListDataSyncErpDatabaseVersion(searchFilter);
	// }

	// public List<DataSyncCatalog> ListDataSyncErpManufacturing(User systemOperator, DataSyncCatalogFilter searchFilter)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	// searchFilter.LanguageCode = systemOperator.LanguageCode;
	// 	return _dataSyncRepository.ListDataSyncErpManufacturing(searchFilter);
	// }

	// public List<DataSyncCatalog> ListDataSyncErpManufacturingVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	// searchFilter.LanguageCode = systemOperator.LanguageCode;
	// 	return _dataSyncRepository.ListDataSyncErpManufacturingVersion(searchFilter);
	// }

	// public List<DataSyncCatalog> ListDataSyncInstanceCategory(User systemOperator, DataSyncCatalogFilter searchFilter)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	return _dataSyncRepository.ListDataSyncInstanceCategory(searchFilter);
	// }

	// public List<DataSyncErp> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes)
	// {
	// 	return _dataSyncRepository.ListDataSyncERP(id, getInstances);
	// }

	// public DataSyncErp MergeDataSyncERP(User systemOperator, DataSyncErp dataSyncInfo)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation
	// 	return _dataSyncRepository.MergeDataSyncERP(dataSyncInfo, systemOperator);
	// }

	// public List<DataSyncService> ListDataSyncService(User systemOperator, string id, TriggerType trigger)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	return _dataSyncRepository.ListDataSyncService(trigger, id);
	// }

	// public DataSyncService MergeDataSyncService(User systemOperator, DataSyncService dataSyncInfo)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	return _dataSyncRepository.MergeDataSyncService(dataSyncInfo, systemOperator);
	// }

	public bool UpdateDataSyncServiceExecution(string id, DateTime executionDate)
	{
		return _dataSyncRepository.UpdateDataSyncServiceExecution(id, executionDate);
	}

	// public bool UpdateDataSyncServiceStatus(string id, ServiceStatus status)
	// {
	// 	return _dataSyncRepository.UpdateDataSyncServiceStatus(id, status);
	// }

	// public List<DataSyncService> ListDataSyncServiceInternal(TriggerType trigger)
	// {
	// 	return _dataSyncRepository.ListDataSyncService(trigger);
	// }

	public async Task<DataSyncService> GetBackgroundService(string backgroundService, string HttpMethod = "GET")
	{
		return (await _dataSyncRepository.GetBackgroundService(backgroundService, HttpMethod.ToUpperInvariant()).ConfigureAwait(false)).FirstOrDefault();
	}

	// public async Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo)
	// {
	// 	string returnValue = string.Empty;
	// 	TransactionOptions transactionOptions = new()
	// 	{
	// 		IsolationLevel = IsolationLevel.ReadCommitted // o cualquier nivel de aislamiento que necesites
	// 	};

	// 	using (TransactionScope childScope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
	// 	{
	// 		returnValue = await _dataSyncRepository.InsertDataSyncServiceLog(logInfo).ConfigureAwait(false);
	// 		childScope.Complete();
	// 	}
	// 	return returnValue;
	// }

	// public async Task<bool> InsertDataSyncServiceLogDetail(DataSyncServiceLogDetail logInfo)
	// {
	// 	bool returnValue = false;
	// 	TransactionOptions transactionOptions = new()
	// 	{
	// 		IsolationLevel = IsolationLevel.ReadCommitted // o cualquier nivel de aislamiento que necesites
	// 	};
	// 	using (TransactionScope childScope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
	// 	{
	// 		returnValue = await _dataSyncRepository.InsertDataSyncServiceLogDetail(logInfo).ConfigureAwait(false);
	// 		childScope.Complete();
	// 	}
	// 	return returnValue;
	// }

	// public bool InsertDataSyncServiceLogDetailBulk(List<DataSyncServiceLogDetail> logInfo)
	// {
	// 	bool returnValue = false;
	// 	TransactionOptions transactionOptions = new()
	// 	{
	// 		IsolationLevel = IsolationLevel.ReadCommitted // o cualquier nivel de aislamiento que necesites
	// 	};
	// 	using (TransactionScope childScope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
	// 	{
	// 		string jsonBulk = JsonConvert.SerializeObject(logInfo, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
	// 		returnValue = _dataSyncRepository.InsertDataSyncServiceLogDetailBulk(jsonBulk);
	// 		childScope.Complete();
	// 	}
	// 	return returnValue;
	// }

	public bool InsertDataSyncServiceErpToken(DataSyncErpAuth tokenInfo)
	{
		return _dataSyncRepository.InsertDataSyncServiceErpToken(tokenInfo);
	}

	public DataSyncErpAuth GetDataSyncServiceErpToken(string erpCode)
	{
		return _dataSyncRepository.GetDataSyncServiceErpToken(erpCode);
	}

	// public List<DataSyncServiceLogDetail> GetDataSyncServiceFailRecords(string erpId)
	// {
	// 	return _dataSyncRepository.GetDataSyncServiceFailRecords(erpId);
	// }

	// public DataSyncService GetServiceInstanceFullData(string serviceInstance)
	// {
	// 	return _dataSyncRepository.GetServiceInstanceFullData(serviceInstance).FirstOrDefault();
	// }

	// public List<DataSyncService> GetServiceInstancesFullData(User systemOperator, string serviceInstance)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	return _dataSyncRepository.GetServiceInstanceFullData(serviceInstance);
	// }

	// public async Task<DataSyncServiceLog> GetDataSyncServiceLogs(string logId, int logType)
	// {
	// 	return await _dataSyncRepository.GetDataSyncServiceLogs(logId, logType).ConfigureAwait(false);
	// }

	// public async Task<List<DataSyncServiceInstanceVisibility>> GetSyncServiceInstanceVisibility(User systemOperator, string services, TriggerType trigger)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	List<DataSyncServiceInstanceVisibility> returnValue = _dataSyncRepository.GetSyncServiceInstanceVisibility(services, trigger);
	// 	await Parallel.ForEachAsync(returnValue, (v, cancellationToken) => { v.Running = ContextCache.IsServiceRunning(v.ServiceInstanceId); return new ValueTask(); }).ConfigureAwait(false);
	// 	return returnValue;
	// }

	// public List<DataSyncService> ListDisabledServices()
	// {
	// 	return _dataSyncRepository.ListDisabledServices();
	// }

	// public DataSyncErp MergeFullData(User systemOperator, DataSyncErp dataInfo)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	DataSyncErp returnValue = _dataSyncRepository.MergeDataSyncERP(dataInfo, systemOperator);
	// 	if (returnValue is not null)
	// 	{
	// 		_ = _dataSyncRepository.SaveDatasyncLog(dataInfo, systemOperator).ConfigureAwait(false);
	// 	}
	// 	return returnValue;
	// }

	// public List<DataSyncServiceLog> GetDataSyncServiceHeaderLogs(User systemOperator, string serviceInstanceId = "")
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	return _dataSyncRepository.GetDataSyncServiceHeaderLogs(serviceInstanceId);
	// }

	// public List<DataSyncServiceLog> GetDataSyncServiceHeaderErrorLogs(User systemOperator, string serviceInstanceId = "")
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	return _dataSyncRepository.GetDataSyncServiceHeaderErrorLogs(serviceInstanceId);
	// }

	// public async Task<string> GetDataSyncServiceHeaderDataLogs(string logId, string type, User systemOperator)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	return await _dataSyncRepository.GetDataSyncServiceHeaderDataLogs(logId, type).ConfigureAwait(false);
	// }

	// public async Task<List<DataSyncServiceLogDetail>> GetDataSyncServiceDetailLogs(User systemOperator, string logId)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	return await _dataSyncRepository.GetDataSyncServiceDetailLogs(logId).ConfigureAwait(false);
	// }

	// public async Task<DataSyncServiceLogDetail> GetDataSyncServiceDetailLogsSingle(User systemOperator, string logId)
	// {
	// 	return await _dataSyncRepository.GetDataSyncServiceDetailLogsSingle(logId).ConfigureAwait(false);
	// }

	// public DataSyncErpMapping MergeDataSyncServiceInstanceMapping(User systemOperator, DataSyncErpMapping instanceMapping)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
	// 	{
	// 		throw new UnauthorizedAccessException(NotificationSettings.noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	DataSyncErpMapping returnValue = _dataSyncRepository.MergeDataSyncServiceInstanceMapping(instanceMapping, systemOperator);
	// 	if (returnValue is not null)
	// 	{
	// 		_ = _dataSyncRepository.SaveDatasyncMappingLog(instanceMapping, systemOperator).ConfigureAwait(false);
	// 	}
	// 	return returnValue;
	// }

	public static async Task<DataSyncResponse> TestErpConnection(User systemOperator, DataSyncErp erpData)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		using APIWebClient client = new()
		{
			TimeoutSeconds = 10
		};
		return await client.DataSyncDownload($"{erpData.BaseUrl}").ConfigureAwait(false);
	}

	// public List<TimeZoneCatalog> GetTimezones(bool currentValues = false)
	// {
	// 	return _dataSyncRepository.GetTimezones(currentValues);
	// }

	public string GetDatasyncDynamicBody(string entityCode)
	{
		return _dataSyncRepository.GetDatasyncDynamicBody(entityCode);
	}

	public Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo)
	{
		throw new NotImplementedException();
	}

	public bool InsertDataSyncServiceLogDetailBulk(string logInfo)
	{
		throw new NotImplementedException();
	}

	public Task<List<DataSyncServiceInstanceVisibility>> GetSyncServiceInstanceVisibility(User systemOperator, string services, TriggerType trigger)
	{
		throw new NotImplementedException();
	}

	public DataSyncErpMapping MergeDataSyncServiceInstanceMapping(User systemOperator, DataSyncErpMapping instanceMapping)
	{
		throw new NotImplementedException();
	}

	public List<TimeZoneCatalog> GetTimezones(bool currentValues = false)
	{
		throw new NotImplementedException();
	}
	// public async Task<List<DataSyncIoTDataSimulator>> GetTagsSimulatorService(bool IsInitial)
	// {
	// 	return await _dataSyncRepository.GetTagsSimulatorService(IsInitial).ConfigureAwait(false);
	// }

	public double GetTimezoneOffset(string offSetName = "")
	{
		double offset = 0;
		if (offSetName == "ERP")
		{
			if (!ContextCache.ERPOffset.HasValue)
			{
				try
				{
					List<TimeZoneCatalog> tz = _dataSyncRepository.GetTimezones(true);
					TimeZoneCatalog erpOffset = tz.Find(t => t.Key == "ERP");
					offset = erpOffset.Offset;
					ContextCache.ERPOffset = offset;
				}
				catch { }
			}
			else
			{
				offset = ContextCache.ERPOffset.Value;
			}
		}
		else
		{
			List<TimeZoneCatalog> tz = _dataSyncRepository.GetTimezones(true);
			if (string.IsNullOrEmpty(offSetName))
			{
				TimeZoneCatalog SfOffset = tz.Find(t => t.Key == "SmartFactory");
				TimeZoneCatalog erpOffset = tz.Find(t => t.Key == "ERP");
				double baseOffset = 0;
				double integrationOffset = 0;
				if (SfOffset is not null)
				{
					baseOffset = SfOffset.Offset;
				}
				if (erpOffset is not null)
				{
					integrationOffset = erpOffset.Offset;
				}
				offset = baseOffset - integrationOffset;
			}
			else
			{
				TimeZoneCatalog namedOffset = tz.Find(t => t.Key == offSetName);
				if (namedOffset is not null)
				{
					offset = namedOffset.Offset;
				}
			}
		}
		return offset;
	}

	public async Task<List<ResponseData>> ListUpdateComponentBulk(List<ComponentExternal> itemList, List<ComponentExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		List<MeasureUnit> unitsList = GetMeasureUnits();
		List<Inventory> inventories = ListInventory(systemOperator, null);
		List<Component> componentsToMerge = [];
		bool NotifyOnce = false;
		if (itemList?.Count > 0)
		{
			int Line = 0;
			string BaseId = string.Empty;
			Component itemInfo = null;
			foreach (ComponentExternal cycleItem in itemList)
			{
				ComponentExternal item = cycleItem;
				Line++;
				try
				{
					itemInfo = null;
					BaseId = item.ItemCode;
					Component existingComponent = (await GetComponents(cycleItem.ItemCode, false, null).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault();
					bool editMode = existingComponent is not null;
					if (editMode && itemListOriginal is not null)
					{
						item = itemListOriginal.Find(x => x.ItemCode == cycleItem.ItemCode);
						item ??= cycleItem;
					}
					List<ValidationResult> results = [];
					ValidationContext context = new(item, null, null);

					if (!Validator.TryValidateObject(item, context, results))
					{
						if (editMode)
						{
							_ = results.RemoveAll(result => result.ErrorMessage.Contains("is required", StringComparison.OrdinalIgnoreCase));
						}
						if (results.Count > 0)
						{
							throw new Exception($"{results[0]}");
						}
					}
					if (!editMode && string.IsNullOrEmpty(item.Status))
					{
						throw new Exception("Item InventoryUoM is invalid");
					}
					Status status = string.Equals(item.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled;
					item.ItemName = item.ItemName.Replace("\"", "´´");
					item.ItemCode = item.ItemCode.Replace("\"", "´´");
					itemInfo = new Component
					{
						Code = item.ItemCode,
						Name = !string.IsNullOrEmpty(item.ItemName) ? item.ItemName : item.ItemCode,
						Status = status,
						ComponentType = ComponentType.Material
					};

					if (!editMode || !string.IsNullOrEmpty(item.Status))
					{
						itemInfo.Status = status;
					}
					if (!editMode || !string.IsNullOrEmpty(item.InventoryUoM))
					{
						MeasureUnit unitInventory = unitsList.Find(unit => string.Equals(unit.Code.Trim(), item.InventoryUoM.Trim(), StringComparison.OrdinalIgnoreCase) && unit.Status == Status.Active && unit.IsProductionResult);
						if (unitInventory is not null)
						{
							itemInfo.UnitInventory = unitInventory.Id;
						}
						else
						{
							throw new Exception("Item InventoryUoM is invalid");
						}
					}
					if (!editMode || !string.IsNullOrEmpty(item.ProductionUoM))
					{
						MeasureUnit unitProduction = unitsList.Find(unit => string.Equals(unit.Code.Trim(), item.ProductionUoM.Trim(), StringComparison.OrdinalIgnoreCase) && unit.Status == Status.Active && unit.IsProductionResult);
						if (unitProduction is not null)
						{
							itemInfo.UnitProduction = unitProduction.Id;
						}
						else
						{
							throw new Exception("Item ProductionUoM is invalid");
						}
					}
					else
					{
						itemInfo.UnitProduction = itemInfo.UnitInventory;
					}

					if (!editMode || !string.IsNullOrEmpty(item.ManagedBy))
					{
						int managedById = 0;
						switch (item.ManagedBy.Trim().ToUpperInvariant())
						{
							case "NONE":
								managedById = 1; // NO MANAGEMENT
								break;

							case "BATCH":
								managedById = 2; // BATCH
								break;

							case "SERIAL":
								managedById = 3; // SERIE
								break;
						}
						itemInfo.ManagedBy = managedById;
					}
					else if (editMode && string.IsNullOrEmpty(item.ManagedBy))
					{
						itemInfo.ManagedBy = existingComponent.ManagedBy;
					}
					if (!editMode || !string.IsNullOrEmpty(item.Type))
					{
						int typeId = 0;
						switch (item.Type.Trim().ToUpperInvariant())
						{
							case "PURCHASE":
								typeId = 1; // NO PURCHASE
								break;

							case "PRODUCTION":
								typeId = 2; // PRODUCTION
								break;
						}
						itemInfo.Type = typeId;
					}
					if (!editMode || !string.IsNullOrEmpty(item.ItemGroupCode))
					{
						Inventory inventoryInfo = inventories.Find(x => x.Code == item.ItemGroupCode && x.Status != Status.Failed);
						if (inventoryInfo is not null && inventoryInfo.Code.Trim() == item.ItemGroupCode.Trim())
						{
							itemInfo.InventoryId = inventoryInfo.InventoryId;
						}
						else
						{
							throw new Exception("Invalid Item Group Code");
						}
					}
					if (!editMode || item.SupplyLeadTime > 0)
					{
						itemInfo.SupplyLeadTime = item.SupplyLeadTime;
					}
					else
					{
						if (item.SupplyLeadTime == 0)
						{
							itemInfo.SupplyLeadTime = existingComponent.SupplyLeadTime;
						}
					}

					if (!editMode || !string.IsNullOrEmpty(item.SyncProduction))
					{
						itemInfo.ProductSync = item.SyncProduction.Equals("YES", StringComparison.OrdinalIgnoreCase);
					}
					else
					{
						if (string.IsNullOrEmpty(item.SyncProduction))
						{
							itemInfo.ProductSync = existingComponent.ProductSync;
						}
					}

					if (!editMode || !string.IsNullOrEmpty(item.Schedule))
					{
						itemInfo.Schedule = item.Schedule.Equals("YES", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
					}
					else
					{
						if (string.IsNullOrEmpty(item.SyncProduction))
						{
							itemInfo.Schedule = existingComponent.Schedule;
						}
					}

					if (!editMode || item.SafetyQuantity.HasValue)
					{
						itemInfo.SafetyQty = item.SafetyQuantity.Value;
					}
					else
					{
						if (!item.SafetyQuantity.HasValue)
						{
							itemInfo.SafetyQty = existingComponent.SafetyQty;
						}
					}

					if (!editMode || !string.IsNullOrEmpty(item.Stock))
					{
						itemInfo.IsStock = !item.Stock.Equals("NO", StringComparison.OrdinalIgnoreCase);
					}
					else
					{
						if (string.IsNullOrEmpty(item.Stock))
						{
							itemInfo.IsStock = existingComponent.IsStock;
						}
					}
					componentsToMerge.Add(itemInfo);
					ResponseData response = new()
					{
						Code = itemInfo.Code,
						Action = ActionDB.IntegrateAll,
						Entity = item,
						EntityAlt = itemInfo,
						IsSuccess = true,
						Id = itemInfo.Code
					};
					returnValue.Add(response);
				}
				catch (Exception ex)
				{
					MessageError = new ResponseData
					{
						Id = BaseId,
						Message = ex.Message
					};
					if (string.IsNullOrEmpty(item.ItemCode))
					{
						MessageError.Code = "Line:" + Line.ToStr();
					}
					else
					{
						MessageError.Code = item.ItemCode;
					}
					MessageError.Entity = item;
					MessageError.EntityAlt = itemInfo;
					returnValue.Add(MessageError);
				}
			}

			string itemsJson = JsonConvert.SerializeObject(componentsToMerge, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			ResponseData result = _dataSyncRepository.MergeComponentBulk(itemsJson, systemOperator, Validate);

			//if (componentsToMerge is not null && Services.ContextCache.Components is not null)
			//{
			//    componentsToMerge.ForEach(item =>
			//    {
			//        Component savedComp = GetComponents(item.Code, true, null)?.Find();
			//        if (savedComp is not null)
			//        {
			//            Services.ContextCache.Components.RemoveAll(comp => comp.Code == item.Code);
			//            Services.ContextCache.Components.Add(savedComp);
			//        }
			//    });

			//}
		}
		if (!Validate)
		{
			if (!NotifyOnce)
			{
				//_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Item, Action = ActionDB.IntegrateAll.ToStr() });Need Discuss
			}
			switch (Level)
			{
				case LevelMessage.Warning:
					returnValue = [.. returnValue.Where(p => !string.IsNullOrEmpty(p.Message))];
					break;

				case LevelMessage.Error:
					returnValue = [.. returnValue.Where(p => !p.IsSuccess)];
					break;
				case LevelMessage.Success:
					break;
			}
		}

		return returnValue;
	}


	public List<Inventory> ListInventory(User systemOperator, string InventoryCode = "", DateTime? DeltaDate = null)
	{
		#region Permission validation

		// if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_INVENTORY_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		return _dataSyncRepository.ListInventory(InventoryCode, DeltaDate);
	}
	public List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null)
	{
		return _dataSyncRepository.GetMeasureUnits(unitType, unitId, DeltaDate);
	}
	public async Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "")
	{
		List<Component> returnValue;
		if (!string.IsNullOrEmpty(filter))
		{
			returnValue = await _dataSyncRepository.ListComponents(componentId, true, filter).ConfigureAwait(false);
		}
		else if (!string.IsNullOrEmpty(componentId))
		{
			returnValue = await _dataSyncRepository.ListComponents(componentId, false, string.Empty).ConfigureAwait(false);
		}
		else
		{
			returnValue = await _dataSyncRepository.ListComponents(componentId, true, filter).ConfigureAwait(false);
		}
		return returnValue?.ToArray();
	}
	// public async Task<List<ResponseData>> CreateAssetsExternal(List<AssetExternal> AssetsList, List<AssetExternal> AssetListOriginal, User user, bool Validate, string Level)
	// {
	// 	List<ResponseData> returntValue = [];
	// 	bool NotifyOnce = false;
	// 	//Validación para saber si se notifica 1 registro desde la funcionalidad de creación
	// 	AssetsList.ForEach(x =>
	// 	{
	// 		switch (x.AssetType.ToUpperInvariant())
	// 		{
	// 			case "FACILITY":
	// 				{
	// 					x.AssetType = Entities.Facility.ToStr();
	// 					break;
	// 				}
	// 			case "FLOOR":
	// 				{
	// 					x.AssetType = Entities.Floor.ToStr();
	// 					break;
	// 				}
	// 			case "WORKCENTER":
	// 				{
	// 					x.AssetType = Entities.Workcenter.ToStr();
	// 					break;
	// 				}
	// 			case "PRODUCTIONLINE":
	// 				{
	// 					x.AssetType = Entities.ProductionLine.ToStr();
	// 					break;
	// 				}
	// 		}
	// 	});
	// 	AssetsList = [.. AssetsList.OrderBy(x => Enum.Parse<Entities>(x.AssetType))];
	// 	if (AssetsList.Count > 0 && AssetsList.Count == 1)
	// 	{
	// 		NotifyOnce = true;
	// 	}
	// 	foreach (AssetExternal asset in AssetsList)
	// 	{
	// 		AssetExternal currentAsset = asset;
	// 		try
	// 		{
	// 			switch (asset.AssetType.ToUpperInvariant())
	// 			{
	// 				case "FACILITY":
	// 					Facility existingFacility = GetFacility(asset.AssetCode);
	// 					if (existingFacility is not null && AssetListOriginal is not null)
	// 					{
	// 						currentAsset = AssetListOriginal.Find(a => a.AssetCode == asset.AssetCode);
	// 						currentAsset ??= asset;
	// 					}
	// 					if (existingFacility is null && !string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
	// 					{
	// 						throw new InvalidOperationException("Cannot import a Disabled Facility record");
	// 					}
	// 					ResponseData resultFacility = await CreateFacility(currentAsset, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
	// 					if (resultFacility is not null)
	// 					{
	// 						resultFacility.Entity = Entities.Facility.ToStr();
	// 						returntValue.Add(resultFacility);
	// 					}
	// 					break;

	// 				case "FLOOR":
	// 					Floor existingFloor = GetFloor(asset.AssetCode);
	// 					if (existingFloor is not null && AssetListOriginal is not null)
	// 					{
	// 						currentAsset = AssetListOriginal.Find(a => a.AssetCode == asset.AssetCode);
	// 						currentAsset ??= asset;
	// 					}
	// 					if (existingFloor is null && !string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
	// 					{
	// 						throw new Exception("Cannot import a Disabled Floor record");
	// 					}
	// 					ResponseData resultFloor = await CreateFloor(currentAsset, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
	// 					if (resultFloor is not null)
	// 					{
	// 						resultFloor.Entity = Entities.Floor.ToStr();
	// 						returntValue.Add(resultFloor);
	// 					}
	// 					break;

	// 				case "WORKCENTER":
	// 					WorkCenter existingWorkcenter = GetWorkCenter(asset.AssetCode);
	// 					if (existingWorkcenter is not null && AssetListOriginal is not null)
	// 					{
	// 						currentAsset = AssetListOriginal.Find(a => a.AssetCode == asset.AssetCode);
	// 						currentAsset ??= asset;
	// 					}
	// 					if (existingWorkcenter is null && !string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
	// 					{
	// 						throw new Exception("Cannot import a Disabled Floor record");
	// 					}
	// 					ResponseData resultWorkCenter = await CreateWorkCenter(currentAsset, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
	// 					if (resultWorkCenter is not null)
	// 					{
	// 						resultWorkCenter.Entity = Entities.Workcenter.ToStr();
	// 						returntValue.Add(resultWorkCenter);
	// 					}
	// 					break;

	// 				case "PRODUCTIONLINE":
	// 					ProductionLine existingProductionLine = GetProductionLine(asset.AssetCode, user);
	// 					if (existingProductionLine is not null && AssetListOriginal is not null)
	// 					{
	// 						currentAsset = AssetListOriginal.Find(a => a.AssetCode == asset.AssetCode);
	// 						currentAsset ??= asset;
	// 					}
	// 					if (existingProductionLine is null && !string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
	// 					{
	// 						throw new Exception("Cannot import a Disabled Floor record");
	// 					}
	// 					ResponseData resultProductLine = await CreateUpdateProductionLine(currentAsset, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
	// 					if (resultProductLine is not null)
	// 					{
	// 						resultProductLine.Entity = Entities.ProductionLine.ToStr();
	// 						returntValue.Add(resultProductLine);
	// 					}
	// 					break;
	// 			}
	// 		}
	// 		catch (Exception ex)
	// 		{
	// 			returntValue.Add(new ResponseData
	// 			{
	// 				Entity = Entities.ProductionLine.ToStr(),
	// 				Action = ActionDB.Create,
	// 				IsSuccess = false,
	// 				Message = ex.Message,
	// 				Code = asset.AssetCode
	// 			});
	// 		}
	// 	}
	// 	if (!NotifyOnce && !Validate)
	// 	{
	// 		// Se crea notificación de toda la entidad Facility
	// 		ResponseData[] tmpListFacility = [.. returntValue.Where(p => p.Entity.ToStr() == Entities.Facility.ToStr())];
	// 		if (tmpListFacility.Length > 1)
	// 		{
	// 			_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Facility, Action = ActionDB.IntegrateAll.ToStr() });
	// 		}
	// 		ResponseData[] tmpListFloor = [.. returntValue.Where(p => p.Entity.ToStr() == Entities.Floor.ToStr())];
	// 		if (tmpListFloor.Length > 1)
	// 		{
	// 			_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Floor, Action = ActionDB.IntegrateAll.ToStr() });
	// 		}
	// 		ResponseData[] tmpListWorkcenter = [.. returntValue.Where(p => p.Entity.ToStr() == Entities.Workcenter.ToStr())];
	// 		if (tmpListWorkcenter.Length > 1)
	// 		{
	// 			_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Workcenter, Action = ActionDB.IntegrateAll.ToStr() });
	// 		}
	// 		ResponseData[] tmpListProductionLine = [.. returntValue.Where(p => p.Entity.ToStr() == Entities.ProductionLine.ToStr())];
	// 		if (tmpListProductionLine.Length > 1)
	// 		{
	// 			_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductionLine, Action = ActionDB.IntegrateAll.ToStr() });
	// 		}
	// 	}
	// 	return returntValue;
	// }
	#region Warehouse

	// public List<Warehouse> ListWarehouse(User systemOperator, string WarehouseCode = "", DateTime? DeltaDate = null)
	// {
	// 	#region Permission validation

	// 	// if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_WAREHOUSE_MANAGE))
	// 	// {
	// 	// 	throw new UnauthorizedAccessException(noPermission);
	// 	// }discuss mario

	// 	#endregion Permission validation

	// 	return _dataSyncRepository.ListWarehouse(WarehouseCode, DeltaDate);
	// }

	// public facilityBin GetFacilityBinLocations(string Code)
	// {
	// 	return _dataSyncRepository.GetFacilityBinLocations(Code);
	// }

	public Warehouse GetWarehouse(string Code)
	{
		return _dataSyncRepository.GetWarehouse(Code);
	}

	public async Task<ResponseData> MergeWarehouse(Warehouse WarehouseInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		ResponseData returnValue = null;

		#region Permission validation

		// if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_WAREHOUSE_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }discuss mario

		#endregion Permission validation

		// Warehouse returnValue = _dataSyncRepository.CreateWarehouse(WarehouseInfo, systemOperator);
		returnValue = _dataSyncRepository.MergeWarehouse(WarehouseInfo, systemOperator, Validate);
		// if (!Validate && returnValue is not null)
		// {
		// 	Warehouse ObjWarehouse = ListWarehouse(systemOperator, returnValue.Code).Find(x => x.Status != Status.Failed);
		// 	await ObjWarehouse.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
		// 	if (NotifyOnce)
		// 	{
		// 		_ = await SaveImageEntity("Warehouse", WarehouseInfo.Image, WarehouseInfo.Code, systemOperator).ConfigureAwait(false);
		// 		_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Warehouse, returnValue.Action, Data = ObjWarehouse }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
		// 		if (WarehouseInfo.AttachmentIds is not null)
		// 		{
		// 			foreach (string attachment in WarehouseInfo.AttachmentIds)
		// 			{
		// 				await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
		// 			}
		// 		}
		// 		returnValue.Entity = ObjWarehouse;
		// 	}
		// }discussmario
		return returnValue;
	}




	public async Task<List<ResponseData>> ListUpdateWarehouseGroup(List<WarehouseExternal> warehouseGroupList, List<WarehouseExternal> warehouseGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returntValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (warehouseGroupList?.Count > 0)
		{
			NotifyOnce = warehouseGroupList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (WarehouseExternal cycleWarehouse in warehouseGroupList)
			{
				Line++;
				WarehouseExternal warehouse = cycleWarehouse;
				Warehouse originalWarehouse = GetWarehouse(cycleWarehouse.WarehouseCode);
				bool editMode = originalWarehouse is not null;
				if (editMode && warehouseGroupListOriginal is not null)
				{
					warehouse = warehouseGroupListOriginal.Find(x => x.WarehouseCode == cycleWarehouse.WarehouseCode);
					warehouse ??= cycleWarehouse;
				}
				try
				{
					BaseId = warehouse.WarehouseName;
					List<ValidationResult> results = [];
					ValidationContext context = new(warehouse, null, null);
					if (!Validator.TryValidateObject(warehouse, context, results))
					{
						if (editMode)
						{
							_ = results.RemoveAll(result => result.ErrorMessage.Contains("is required", StringComparison.OrdinalIgnoreCase));
						}
						if (results.Count > 0)
						{
							throw new Exception($"{results[0]}");
						}
					}
					Status status = (Status)Status.Active.ToInt32();
					if (!editMode || !string.IsNullOrEmpty(warehouse.Status))
					{
						if (string.Equals(warehouse.Status.Trim(), "DISABLE", StringComparison.OrdinalIgnoreCase))
						{
							status = (Status)Status.Disabled.ToInt32();
						}
					}
					if (!editMode && status == Status.Disabled)
					{
						throw new Exception("Cannot import a new disabled warehouse");
					}
					Warehouse warehouseInfo;
					if (!editMode)
					{
						warehouseInfo = new Warehouse
						{
							Code = warehouse.WarehouseCode,
							Name = !string.IsNullOrEmpty(warehouse.WarehouseName) ? warehouse.WarehouseName : warehouse.WarehouseCode,
							Status = status,
							FacilityCode = warehouse.FacilityCode,
							Schedule = warehouse.Schedule.ToStr().Equals("Yes", StringComparison.OrdinalIgnoreCase),
						};
					}
					else
					{
						warehouseInfo = originalWarehouse;
						if (!string.IsNullOrEmpty(warehouse.WarehouseName))
						{
							warehouseInfo.Name = warehouse.WarehouseName;
						}
						if (!string.IsNullOrEmpty(warehouse.FacilityCode))
						{
							warehouseInfo.FacilityCode = warehouse.FacilityCode;
						}
						warehouseInfo.Status = status;
					}

					if (!editMode || (warehouse.Locations?.Count > 0))
					{
						//List<ResponseData> binLocationsResponse = await ListUpdateBinLocation(warehouse.Locations, null, systemOperator, Validate, Level).ConfigureAwait(false);discuss mario
						warehouseInfo.Details = null;
					}
					if (editMode && string.IsNullOrEmpty(warehouse.FacilityCode))
					{
						warehouseInfo.FacilityCode = originalWarehouse.FacilityCode;
					}
					ResponseData response = await MergeWarehouse(warehouseInfo, systemOperator, Validate).ConfigureAwait(false);
					returntValue.Add(response);
				}
				catch (Exception ex)
				{
					MessageError = new ResponseData
					{
						Id = BaseId,
						Message = ex.Message,
						Code = "Line:" + Line.ToStr()
					};
					returntValue.Add(MessageError);
				}
			}
		}
		// if (!Validate)
		// {
		// 	if (!NotifyOnce)
		// 	{
		// 		_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Warehouse, Action = ActionDB.IntegrateAll.ToStr() });
		// 	}
		// 	switch (Level)
		// 	{
		// 		case LevelMessage.Warning:
		// 			returntValue = [.. returntValue.Where(p => !string.IsNullOrEmpty(p.Message))];
		// 			break;

		// 		case LevelMessage.Error:
		// 			returntValue = [.. returntValue.Where(p => !p.IsSuccess)];
		// 			break;
		// 		case LevelMessage.Success:
		// 			break;
		// 	}
		// }  discuss mario
		return returntValue;
	}
	
	public Task<User> GetUserWithoutValidations(User user)
	{
		return _dataSyncRepository.GetUser(user.Id, null, new User(0));
	}

	#endregion Warehouse

	#endregion DataSync
}
