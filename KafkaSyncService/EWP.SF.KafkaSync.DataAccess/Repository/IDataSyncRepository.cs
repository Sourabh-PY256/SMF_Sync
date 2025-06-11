using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IDataSyncRepository
{

    Task<DataSyncServiceLog> GetDataSyncServiceLogs(string LogId, int logType = 0, CancellationToken cancellationToken = default);
    string GetDatasyncDynamicBody(string EntityCode);
    Task<List<TimeZoneCatalog>> GetTimezones(bool currentValues);
    List<DataSyncErp> ListDataSyncERP(string Id = "", EnableType GetInstances = EnableType.Yes);
    Task<List<DataSyncService>> GetBackgroundService(string backgroundService, string httpMethod, CancellationToken cancellationToken = default);
    bool UpdateDataSyncServiceExecution(string Id, DateTime ExecutionDate);
    List<DefaultMappingEntityObject> ListDefaultMappingEntityObject(string Entity);
    DataSyncErpAuth GetDataSyncServiceErpToken(string ErpCode);

    bool InsertDataSyncServiceErpToken(DataSyncErpAuth TokenInfo);


}
