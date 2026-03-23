
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IDataSyncServiceOperation
{

    bool UpdateDataSyncServiceExecution(string id, DateTime executionDate);
    bool UpdateDataSyncServiceStatus(string id, ServiceStatus status);
    Task<DataSyncService> GetBackgroundService(string backgroundService, string HttpMethod = "GET");
    Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo);
    Task<string> InsertDataSyncServiceLogDetail(DataSyncServiceLogDetail logDetail);
    bool InsertDataSyncServiceErpToken(DataSyncErpAuth tokenInfo);
    DataSyncErpAuth GetDataSyncServiceErpToken(string erpCode);
    Task<List<TimeZoneCatalog>> GetTimezones(bool currentValues = false);
    string GetDatasyncDynamicBody(string entityCode);

    Task<List<DataSyncErp>> ListDataSyncERP(string id = "", EnableType getInstances = EnableType.Yes);
    Task<User> GetUserWithoutValidations(User user);
    Task<double> GetTimezoneOffset(string offSetName = "");
}
