
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IDataSyncServiceOperation
{

    bool UpdateDataSyncServiceExecution(string id, DateTime executionDate);
    Task<DataSyncService> GetBackgroundService(string backgroundService, string HttpMethod = "GET");
    Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo);
    bool InsertDataSyncServiceErpToken(DataSyncErpAuth tokenInfo);
    DataSyncErpAuth GetDataSyncServiceErpToken(string erpCode);
    Task<List<TimeZoneCatalog>> GetTimezones(bool currentValues = false);
    string GetDatasyncDynamicBody(string entityCode);

    List<DataSyncErp> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes);
    Task<User> GetUserWithoutValidations(User user);
}
