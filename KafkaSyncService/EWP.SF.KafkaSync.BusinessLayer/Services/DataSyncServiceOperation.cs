
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.Common.Models;
using EWP.SF.Helper;
using System.Transactions;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class DataSyncServiceOperation : IDataSyncServiceOperation
{
    private readonly IDataSyncRepository _dataSyncRepository;


    public DataSyncServiceOperation(IDataSyncRepository dataSyncRepository)
    {
        _dataSyncRepository = dataSyncRepository;
    }

    #region DataSync

    public bool UpdateDataSyncServiceExecution(string id, DateTime executionDate)
    {
        return _dataSyncRepository.UpdateDataSyncServiceExecution(id, executionDate);
    }

    public bool UpdateDataSyncServiceStatus(string id, ServiceStatus status)
    {
        return _dataSyncRepository.UpdateDataSyncServiceStatus(id, status);
    }

    public async Task<DataSyncService> GetBackgroundService(string backgroundService, string HttpMethod = "GET")
    {
        return (await _dataSyncRepository.GetBackgroundService(backgroundService, HttpMethod.ToUpperInvariant()).ConfigureAwait(false)).FirstOrDefault();
    }


    public bool InsertDataSyncServiceErpToken(DataSyncErpAuth tokenInfo)
    {
        return _dataSyncRepository.InsertDataSyncServiceErpToken(tokenInfo);
    }

    public DataSyncErpAuth GetDataSyncServiceErpToken(string erpCode)
    {
        return _dataSyncRepository.GetDataSyncServiceErpToken(erpCode);
    }

    public string GetDatasyncDynamicBody(string entityCode)
    {
        return _dataSyncRepository.GetDatasyncDynamicBody(entityCode);
    }

    /// <summary>
	/// Insert a new log entry for the data synchronization service.
	/// </summary>
	public async Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo)
	{
		string returnValue = string.Empty;
		TransactionOptions transactionOptions = new()
		{
			IsolationLevel = IsolationLevel.ReadCommitted
		};

		using (TransactionScope childScope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			returnValue = await _dataSyncRepository.InsertDataSyncServiceLog(logInfo).ConfigureAwait(false);
			childScope.Complete();
		}
		return returnValue;
	}

	public async Task<string> InsertDataSyncServiceLogDetail(DataSyncServiceLogDetail logDetail)
	{
		string returnValue = string.Empty;
		TransactionOptions transactionOptions = new()
		{
			IsolationLevel = IsolationLevel.ReadCommitted
		};

		using (TransactionScope childScope = new(TransactionScopeOption.RequiresNew, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			returnValue = await _dataSyncRepository.InsertDataSyncServiceLogDetail(logDetail).ConfigureAwait(false);
			childScope.Complete();
		}
		return returnValue;
	}

    public Task<List<TimeZoneCatalog>> GetTimezones(bool currentValues = false)
    {
        return _dataSyncRepository.GetTimezones(currentValues);
    }


    /// <summary>
	///
	/// </summary>
	public Task<List<DataSyncErp>> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes) => _dataSyncRepository.ListDataSyncERP(id, getInstances);


    /// <summary>
	///
	/// </summary>
	public Task<User> GetUserWithoutValidations(User user) => _dataSyncRepository.GetUser(user.Id, null, new User(0));

public async Task<double> GetTimezoneOffset(string offSetName = "")
	{
		double offset = 0;
		if (offSetName == "ERP")
		{
			if (!ContextCache.ERPOffset.HasValue)
			{
				try
				{
					List<TimeZoneCatalog> tz = await GetTimezones(true).ConfigureAwait(false);
					TimeZoneCatalog erpOffset = tz.Find(x => x.Key == "ERP");
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
			List<TimeZoneCatalog> tz = await GetTimezones(true).ConfigureAwait(false);
			if (string.IsNullOrEmpty(offSetName))
			{
				TimeZoneCatalog SfOffset = tz.Find(x => x.Key == "SmartFactory");
				TimeZoneCatalog erpOffset = tz.Find(x => x.Key == "ERP");
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
				TimeZoneCatalog namedOffset = tz.Find(x => x.Key == offSetName);
				if (namedOffset is not null)
				{
					offset = namedOffset.Offset;
				}
			}
		}
		return offset;
	}
	/// 
    #endregion DataSync
}
