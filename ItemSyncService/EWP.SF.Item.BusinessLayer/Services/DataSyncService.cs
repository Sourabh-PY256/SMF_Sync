
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;

namespace EWP.SF.Item.BusinessLayer;

public  class DataSyncService : IDataSyncService    
{
    private readonly IDataSyncRepository _dataSyncRepository;

    public DataSyncService(IDataSyncRepository dataSyncRepository)
    {
        _dataSyncRepository = dataSyncRepository;
    }
	#region DataSync

	public async Task DatasyncTempServiceLogAsync(string EntityCode, string mode, string Exception = "")
	{
		if (string.Equals(mode, "START", StringComparison.OrdinalIgnoreCase) || string.Equals(mode, "END", StringComparison.OrdinalIgnoreCase) || ApplicationSettings.Instance.GetAppSetting("LogTimedServices").ToBool())
		{
			await _dataSyncRepository.DatasyncTempServiceLogAsync(EntityCode, mode, Exception).ConfigureAwait(false);
		}
	}

	public List<DataSyncCatalog> ListDataSyncErp(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return _dataSyncRepository.ListDataSyncErp(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return _dataSyncRepository.ListDataSyncErpVersion(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpDatabase(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return _dataSyncRepository.ListDataSyncErpDatabase(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpDatabaseVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return _dataSyncRepository.ListDataSyncErpDatabaseVersion(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpManufacturing(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return _dataSyncRepository.ListDataSyncErpManufacturing(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpManufacturingVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return _dataSyncRepository.ListDataSyncErpManufacturingVersion(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncInstanceCategory(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return _dataSyncRepository.ListDataSyncInstanceCategory(searchFilter);
	}

	public List<DataSyncErp> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes)
	{
		return _dataSyncRepository.ListDataSyncERP(id, getInstances);
	}

	public DataSyncErp MergeDataSyncERP(User systemOperator, DataSyncErp dataSyncInfo)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation
		return _dataSyncRepository.MergeDataSyncERP(dataSyncInfo, systemOperator);
	}

	public List<DataSyncService> ListDataSyncService(User systemOperator, string id, TriggerType trigger)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return _dataSyncRepository.ListDataSyncService(trigger, id);
	}

	public DataSyncService MergeDataSyncService(User systemOperator, DataSyncService dataSyncInfo)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return _dataSyncRepository.MergeDataSyncService(dataSyncInfo, systemOperator);
	}

	public bool UpdateDataSyncServiceExecution(string id, DateTime executionDate)
	{
		return _dataSyncRepository.UpdateDataSyncServiceExecution(id, executionDate);
	}

	public bool UpdateDataSyncServiceStatus(string id, ServiceStatus status)
	{
		return _dataSyncRepository.UpdateDataSyncServiceStatus(id, status);
	}

	public List<DataSyncService> ListDataSyncServiceInternal(TriggerType trigger)
	{
		return _dataSyncRepository.ListDataSyncService(trigger);
	}

	public async Task<DataSyncService> GetBackgroundService(string backgroundService, string HttpMethod = "GET")
	{
		return (await _dataSyncRepository.GetBackgroundService(backgroundService, HttpMethod.ToUpperInvariant()).ConfigureAwait(false)).FirstOrDefault();
	}

	public async Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo)
	{
		string returnValue = string.Empty;
		TransactionOptions transactionOptions = new()
		{
			IsolationLevel = IsolationLevel.ReadCommitted // o cualquier nivel de aislamiento que necesites
		};

		using (TransactionScope childScope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			returnValue = await _dataSyncRepository.InsertDataSyncServiceLog(logInfo).ConfigureAwait(false);
			childScope.Complete();
		}
		return returnValue;
	}

	public async Task<bool> InsertDataSyncServiceLogDetail(DataSyncServiceLogDetail logInfo)
	{
		bool returnValue = false;
		TransactionOptions transactionOptions = new()
		{
			IsolationLevel = IsolationLevel.ReadCommitted // o cualquier nivel de aislamiento que necesites
		};
		using (TransactionScope childScope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			returnValue = await _dataSyncRepository.InsertDataSyncServiceLogDetail(logInfo).ConfigureAwait(false);
			childScope.Complete();
		}
		return returnValue;
	}

	public bool InsertDataSyncServiceLogDetailBulk(List<DataSyncServiceLogDetail> logInfo)
	{
		bool returnValue = false;
		TransactionOptions transactionOptions = new()
		{
			IsolationLevel = IsolationLevel.ReadCommitted // o cualquier nivel de aislamiento que necesites
		};
		using (TransactionScope childScope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			string jsonBulk = JsonConvert.SerializeObject(logInfo, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			returnValue = _dataSyncRepository.InsertDataSyncServiceLogDetailBulk(jsonBulk);
			childScope.Complete();
		}
		return returnValue;
	}

	public bool InsertDataSyncServiceErpToken(DataSyncErpAuth tokenInfo)
	{
		return _dataSyncRepository.InsertDataSyncServiceErpToken(tokenInfo);
	}

	public DataSyncErpAuth GetDataSyncServiceErpToken(string erpCode)
	{
		return _dataSyncRepository.GetDataSyncServiceErpToken(erpCode);
	}

	public List<DataSyncServiceLogDetail> GetDataSyncServiceFailRecords(string erpId)
	{
		return _dataSyncRepository.GetDataSyncServiceFailRecords(erpId);
	}

	public DataSyncService GetServiceInstanceFullData(string serviceInstance)
	{
		return _dataSyncRepository.GetServiceInstanceFullData(serviceInstance).FirstOrDefault();
	}

	public List<DataSyncService> GetServiceInstancesFullData(User systemOperator, string serviceInstance)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return _dataSyncRepository.GetServiceInstanceFullData(serviceInstance);
	}

	public async Task<DataSyncServiceLog> GetDataSyncServiceLogs(string logId, int logType)
	{
		return await _dataSyncRepository.GetDataSyncServiceLogs(logId, logType).ConfigureAwait(false);
	}

	public async Task<List<DataSyncServiceInstanceVisibility>> GetSyncServiceInstanceVisibility(User systemOperator, string services, TriggerType trigger)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		List<DataSyncServiceInstanceVisibility> returnValue = _dataSyncRepository.GetSyncServiceInstanceVisibility(services, trigger);
		await Parallel.ForEachAsync(returnValue, (v, cancellationToken) => { v.Running = ContextCache.IsServiceRunning(v.ServiceInstanceId); return new ValueTask(); }).ConfigureAwait(false);
		return returnValue;
	}

	public List<DataSyncService> ListDisabledServices()
	{
		return _dataSyncRepository.ListDisabledServices();
	}

	public DataSyncErp MergeFullData(User systemOperator, DataSyncErp dataInfo)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		DataSyncErp returnValue = _dataSyncRepository.MergeDataSyncERP(dataInfo, systemOperator);
		if (returnValue is not null)
		{
			_ = _dataSyncRepository.SaveDatasyncLog(dataInfo, systemOperator).ConfigureAwait(false);
		}
		return returnValue;
	}

	public List<DataSyncServiceLog> GetDataSyncServiceHeaderLogs(User systemOperator, string serviceInstanceId = "")
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return _dataSyncRepository.GetDataSyncServiceHeaderLogs(serviceInstanceId);
	}

	public List<DataSyncServiceLog> GetDataSyncServiceHeaderErrorLogs(User systemOperator, string serviceInstanceId = "")
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return _dataSyncRepository.GetDataSyncServiceHeaderErrorLogs(serviceInstanceId);
	}

	public async Task<string> GetDataSyncServiceHeaderDataLogs(string logId, string type, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return await _dataSyncRepository.GetDataSyncServiceHeaderDataLogs(logId, type).ConfigureAwait(false);
	}

	public async Task<List<DataSyncServiceLogDetail>> GetDataSyncServiceDetailLogs(User systemOperator, string logId)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return await _dataSyncRepository.GetDataSyncServiceDetailLogs(logId).ConfigureAwait(false);
	}

	public async Task<DataSyncServiceLogDetail> GetDataSyncServiceDetailLogsSingle(User systemOperator, string logId)
	{
		return await _dataSyncRepository.GetDataSyncServiceDetailLogsSingle(logId).ConfigureAwait(false);
	}

	public DataSyncErpMapping MergeDataSyncServiceInstanceMapping(User systemOperator, DataSyncErpMapping instanceMapping)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		DataSyncErpMapping returnValue = _dataSyncRepository.MergeDataSyncServiceInstanceMapping(instanceMapping, systemOperator);
		if (returnValue is not null)
		{
			_ = _dataSyncRepository.SaveDatasyncMappingLog(instanceMapping, systemOperator).ConfigureAwait(false);
		}
		return returnValue;
	}

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

	public List<TimeZoneCatalog> GetTimezones(bool currentValues = false)
	{
		return _dataSyncRepository.GetTimezones(currentValues);
	}

	public string GetDatasyncDynamicBody(string entityCode)
	{
		return _dataSyncRepository.GetDatasyncDynamicBody(entityCode);
	}
	public async Task<List<DataSyncIoTDataSimulator>> GetTagsSimulatorService(bool IsInitial)
	{
		return await _dataSyncRepository.GetTagsSimulatorService(IsInitial).ConfigureAwait(false);
	}
	#endregion DataSync
}
