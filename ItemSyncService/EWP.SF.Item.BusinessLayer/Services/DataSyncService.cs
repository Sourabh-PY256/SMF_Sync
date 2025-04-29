
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

	#region DataSync

	public async Task DatasyncTempServiceLogAsync(string EntityCode, string mode, string Exception = "")
	{
		if (string.Equals(mode, "START", StringComparison.OrdinalIgnoreCase) || string.Equals(mode, "END", StringComparison.OrdinalIgnoreCase) || ApplicationSettings.Instance.GetAppSetting("LogTimedServices").ToBool())
		{
			await BrokerDAL.DatasyncTempServiceLogAsync(EntityCode, mode, Exception).ConfigureAwait(false);
		}
	}

	public List<DataSyncCatalog> ListDataSyncErp(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return BrokerDAL.ListDataSyncErp(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return BrokerDAL.ListDataSyncErpVersion(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpDatabase(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return BrokerDAL.ListDataSyncErpDatabase(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpDatabaseVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return BrokerDAL.ListDataSyncErpDatabaseVersion(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpManufacturing(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return BrokerDAL.ListDataSyncErpManufacturing(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncErpManufacturingVersion(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		// searchFilter.LanguageCode = systemOperator.LanguageCode;
		return BrokerDAL.ListDataSyncErpManufacturingVersion(searchFilter);
	}

	public List<DataSyncCatalog> ListDataSyncInstanceCategory(User systemOperator, DataSyncCatalogFilter searchFilter)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		return BrokerDAL.ListDataSyncInstanceCategory(searchFilter);
	}

	public List<DataSyncErp> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes)
	{
		return BrokerDAL.ListDataSyncERP(id, getInstances);
	}

	public DataSyncErp MergeDataSyncERP(User systemOperator, DataSyncErp dataSyncInfo)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation
		return BrokerDAL.MergeDataSyncERP(dataSyncInfo, systemOperator);
	}

	public List<DataSyncService> ListDataSyncService(User systemOperator, string id, TriggerType trigger)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		return BrokerDAL.ListDataSyncService(trigger, id);
	}

	public DataSyncService MergeDataSyncService(User systemOperator, DataSyncService dataSyncInfo)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		return BrokerDAL.MergeDataSyncService(dataSyncInfo, systemOperator);
	}

	public bool UpdateDataSyncServiceExecution(string id, DateTime executionDate)
	{
		return BrokerDAL.UpdateDataSyncServiceExecution(id, executionDate);
	}

	public bool UpdateDataSyncServiceStatus(string id, ServiceStatus status)
	{
		return BrokerDAL.UpdateDataSyncServiceStatus(id, status);
	}

	public List<DataSyncService> ListDataSyncServiceInternal(TriggerType trigger)
	{
		return BrokerDAL.ListDataSyncService(trigger);
	}

	public async Task<DataSyncService> GetBackgroundService(string backgroundService, string HttpMethod = "GET")
	{
		return (await BrokerDAL.GetBackgroundService(backgroundService, HttpMethod.ToUpperInvariant()).ConfigureAwait(false)).FirstOrDefault();
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
			returnValue = await BrokerDAL.InsertDataSyncServiceLog(logInfo).ConfigureAwait(false);
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
			returnValue = await BrokerDAL.InsertDataSyncServiceLogDetail(logInfo).ConfigureAwait(false);
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
			returnValue = BrokerDAL.InsertDataSyncServiceLogDetailBulk(jsonBulk);
			childScope.Complete();
		}
		return returnValue;
	}

	public bool InsertDataSyncServiceErpToken(DataSyncErpAuth tokenInfo)
	{
		return BrokerDAL.InsertDataSyncServiceErpToken(tokenInfo);
	}

	public DataSyncErpAuth GetDataSyncServiceErpToken(string erpCode)
	{
		return BrokerDAL.GetDataSyncServiceErpToken(erpCode);
	}

	public List<DataSyncServiceLogDetail> GetDataSyncServiceFailRecords(string erpId)
	{
		return BrokerDAL.GetDataSyncServiceFailRecords(erpId);
	}

	public DataSyncService GetServiceInstanceFullData(string serviceInstance)
	{
		return BrokerDAL.GetServiceInstanceFullData(serviceInstance).FirstOrDefault();
	}

	public List<DataSyncService> GetServiceInstancesFullData(User systemOperator, string serviceInstance)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		return BrokerDAL.GetServiceInstanceFullData(serviceInstance);
	}

	public async Task<DataSyncServiceLog> GetDataSyncServiceLogs(string logId, int logType)
	{
		return await BrokerDAL.GetDataSyncServiceLogs(logId, logType).ConfigureAwait(false);
	}

	public async Task<List<DataSyncServiceInstanceVisibility>> GetSyncServiceInstanceVisibility(User systemOperator, string services, TriggerType trigger)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		List<DataSyncServiceInstanceVisibility> returnValue = BrokerDAL.GetSyncServiceInstanceVisibility(services, trigger);
		await Parallel.ForEachAsync(returnValue, (v, cancellationToken) => { v.Running = ContextCache.IsServiceRunning(v.ServiceInstanceId); return new ValueTask(); }).ConfigureAwait(false);
		return returnValue;
	}

	public List<DataSyncService> ListDisabledServices()
	{
		return BrokerDAL.ListDisabledServices();
	}

	public DataSyncErp MergeFullData(User systemOperator, DataSyncErp dataInfo)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		DataSyncErp returnValue = BrokerDAL.MergeDataSyncERP(dataInfo, systemOperator);
		if (returnValue is not null)
		{
			_ = BrokerDAL.SaveDatasyncLog(dataInfo, systemOperator).ConfigureAwait(false);
		}
		return returnValue;
	}

	public List<DataSyncServiceLog> GetDataSyncServiceHeaderLogs(User systemOperator, string serviceInstanceId = "")
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		return BrokerDAL.GetDataSyncServiceHeaderLogs(serviceInstanceId);
	}

	public List<DataSyncServiceLog> GetDataSyncServiceHeaderErrorLogs(User systemOperator, string serviceInstanceId = "")
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		return BrokerDAL.GetDataSyncServiceHeaderErrorLogs(serviceInstanceId);
	}

	public async Task<string> GetDataSyncServiceHeaderDataLogs(string logId, string type, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		return await BrokerDAL.GetDataSyncServiceHeaderDataLogs(logId, type).ConfigureAwait(false);
	}

	public async Task<List<DataSyncServiceLogDetail>> GetDataSyncServiceDetailLogs(User systemOperator, string logId)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		return await BrokerDAL.GetDataSyncServiceDetailLogs(logId).ConfigureAwait(false);
	}

	public async Task<DataSyncServiceLogDetail> GetDataSyncServiceDetailLogsSingle(User systemOperator, string logId)
	{
		return await BrokerDAL.GetDataSyncServiceDetailLogsSingle(logId).ConfigureAwait(false);
	}

	public DataSyncErpMapping MergeDataSyncServiceInstanceMapping(User systemOperator, DataSyncErpMapping instanceMapping)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
		}

		#endregion Permission validation

		DataSyncErpMapping returnValue = BrokerDAL.MergeDataSyncServiceInstanceMapping(instanceMapping, systemOperator);
		if (returnValue is not null)
		{
			_ = BrokerDAL.SaveDatasyncMappingLog(instanceMapping, systemOperator).ConfigureAwait(false);
		}
		return returnValue;
	}

	public static async Task<DataSyncResponse> TestErpConnection(User systemOperator, DataSyncErp erpData)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(noPermission);
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
		return BrokerDAL.GetTimezones(currentValues);
	}

	public string GetDatasyncDynamicBody(string entityCode)
	{
		return BrokerDAL.GetDatasyncDynamicBody(entityCode);
	}
	public async Task<List<DataSyncIoTDataSimulator>> GetTagsSimulatorService(bool IsInitial)
	{
		return await BrokerDAL.GetTagsSimulatorService(IsInitial).ConfigureAwait(false);
	}
	#endregion DataSync
}
