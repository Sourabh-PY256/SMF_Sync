
using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;

namespace EWP.SF.Item.BusinessLayer;

public interface IDataSyncServiceOperation
{

    bool UpdateDataSyncServiceExecution(string id, DateTime executionDate);
    // bool UpdateDataSyncServiceStatus(string id, ServiceStatus status);
    // List<DataSyncService> ListDataSyncServiceInternal(TriggerType trigger);
    Task<DataSyncService> GetBackgroundService(string backgroundService, string HttpMethod = "GET");
    Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo);
    bool InsertDataSyncServiceErpToken(DataSyncErpAuth tokenInfo);
    DataSyncErpAuth GetDataSyncServiceErpToken(string erpCode);
    Task<List<DataSyncServiceInstanceVisibility>> GetSyncServiceInstanceVisibility(User systemOperator, string services, TriggerType trigger);
    Task<List<TimeZoneCatalog>> GetTimezones(bool currentValues = false);
    string GetDatasyncDynamicBody(string entityCode);

    List<DataSyncErp> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes);
}
