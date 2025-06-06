
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Models;
using EWP.SF.Helper;	

namespace EWP.SF.Item.BusinessLayer;

public class DataSyncServiceOperation : IDataSyncServiceOperation
{
    private readonly IDataSyncRepository _dataSyncRepository;
    private readonly IApplicationSettings _applicationSettings;

    public DataSyncServiceOperation(IDataSyncRepository dataSyncRepository, IApplicationSettings applicationSettings)
    {
        _dataSyncRepository = dataSyncRepository;
        _applicationSettings = applicationSettings;
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

    public Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo)
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

    public  Task<List<TimeZoneCatalog>> GetTimezones(bool currentValues = false)
    {
        return _dataSyncRepository.GetTimezones(currentValues);
    }


    /// <summary>
	///
	/// </summary>
	public List<DataSyncErp> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes) => _dataSyncRepository.ListDataSyncERP(id, getInstances);


   
    
    #endregion DataSync
}
