
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

    public Task<List<TimeZoneCatalog>> GetTimezones(bool currentValues = false)
    {
        return _dataSyncRepository.GetTimezones(currentValues);
    }


    /// <summary>
	///
	/// </summary>
	public List<DataSyncErp> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes) => _dataSyncRepository.ListDataSyncERP(id, getInstances);


    /// <summary>
	///
	/// </summary>
	public Task<User> GetUserWithoutValidations(User user) => _dataSyncRepository.GetUser(user.Id, null, new User(0));


    #endregion DataSync
}
