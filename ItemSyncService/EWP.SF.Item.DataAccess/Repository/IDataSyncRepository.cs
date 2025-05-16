using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IDataSyncRepository
{

    Task<DataSyncServiceLog> GetDataSyncServiceLogs(string LogId, int logType = 0, CancellationToken cancellationToken = default);
    string GetDatasyncDynamicBody(string EntityCode);
    List<TimeZoneCatalog> GetTimezones(bool currentValues);
    Task<List<DataSyncService>> GetBackgroundService(string backgroundService, string httpMethod, CancellationToken cancellationToken = default);
    bool UpdateDataSyncServiceExecution(string Id, DateTime ExecutionDate);
    DataSyncErpAuth GetDataSyncServiceErpToken(string ErpCode);

    bool InsertDataSyncServiceErpToken(DataSyncErpAuth TokenInfo);

    ResponseData MergeComponentBulk(string Json, User systemOperator, bool Validation);

    List<Inventory> ListInventory(string Code = "", DateTime? DeltaDate = null);

    List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null);
    Task<List<Component>> ListComponents(string componentId, bool ignoreImages = false, string filter = "", DateTime? DeltaDate = null, CancellationToken cancellationToken = default);
    Warehouse GetWarehouse(string Code);
    ResponseData MergeWarehouse(Warehouse WarehouseInfo, User systemOperator, bool Validation);
}
