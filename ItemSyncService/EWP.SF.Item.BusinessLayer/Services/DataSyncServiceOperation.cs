
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

    /// <summary>
	///
	/// </summary>
	public List<DataSyncErp> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes) => _dataSyncRepository.ListDataSyncERP(id, getInstances);


   
    
    #endregion DataSync
}
