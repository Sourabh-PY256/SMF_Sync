using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IDataSyncRepository
{
    List<DataSyncCatalog> ListDataSyncErp(DataSyncCatalogFilter SearchFilter);
    List<DataSyncCatalog> ListDataSyncErpVersion(DataSyncCatalogFilter SearchFilter);
    List<DataSyncCatalog> ListDataSyncErpDatabase(DataSyncCatalogFilter SearchFilter);
    List<DataSyncCatalog> ListDataSyncErpDatabaseVersion(DataSyncCatalogFilter SearchFilter);
    List<DataSyncCatalog> ListDataSyncErpManufacturing(DataSyncCatalogFilter SearchFilter);
    List<DataSyncCatalog> ListDataSyncErpManufacturingVersion(DataSyncCatalogFilter SearchFilter);
    List<DataSyncCatalog> ListDataSyncInstanceCategory(DataSyncCatalogFilter SearchFilter);
    List<DataSyncErp> ListDataSyncERP(string Id = "", EnableType GetInstances = EnableType.Yes);
    Task SaveDatasyncLog(DataSyncErp DataSyncInfo, User SystemOperator, CancellationToken cancellationToken = default);
    Task DatasyncTempServiceLogAsync(string EntityCode, string mode, string Exception = "", CancellationToken cancellationToken = default);
    DataSyncErp MergeDataSyncERP(DataSyncErp DataSyncInfo, User SystemOperator);
    List<DataSyncService> ListDisabledServices();
    List<DataSyncService> ListDataSyncService(TriggerType Trigger, string Id = "");
    DataSyncService MergeDataSyncService(DataSyncService DataSyncInfo, User SystemOperator);
    bool UpdateDataSyncServiceStatus(string Id, ServiceStatus Status);
    Task<bool> InsertDataSyncServiceLog(DataSyncServiceLog logInfo, CancellationToken cancellationToken = default);
    Task<bool> InsertDataSyncServiceLogDetail(DataSyncServiceLogDetail logInfo, CancellationToken cancellationToken = default);
    bool InsertDataSyncServiceLogDetailBulk(string LogInfo);
    bool InsertDataSyncServiceErpToken(DataSyncErpAuth TokenInfo);
    DataSyncErpAuth GetDataSyncServiceErpToken(string ErpCode);
    List<DataSyncServiceLogDetail> GetDataSyncServiceFailRecords(string ErpId);
    List<DataSyncService> GetServiceInstanceFullData(string ServiceInstance);
    Task<DataSyncServiceLog> GetDataSyncServiceLogs(string LogId, int logType = 0, CancellationToken cancellationToken = default);
    string GetDatasyncDynamicBody(string EntityCode);
    List<TimeZoneCatalog> GetTimezones(bool currentValues);
    Task<List<DataSyncService>> GetBackgroundService(string backgroundService, string httpMethod, CancellationToken cancellationToken = default);
    bool UpdateDataSyncServiceExecution(string Id, DateTime ExecutionDate);
}
