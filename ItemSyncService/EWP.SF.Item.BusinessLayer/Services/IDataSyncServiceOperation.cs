
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Item.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EWP.SF.Item.BusinessLayer;

public interface IDataSyncServiceOperation
{

    bool UpdateDataSyncServiceExecution(string id, DateTime executionDate);
    // bool UpdateDataSyncServiceStatus(string id, ServiceStatus status);
    // List<DataSyncService> ListDataSyncServiceInternal(TriggerType trigger);
    Task<DataSyncService> GetBackgroundService(string backgroundService, string HttpMethod = "GET");
    Task<string> InsertDataSyncServiceLog(DataSyncServiceLog logInfo);
    // Task<bool> InsertDataSyncServiceLogDetail(DataSyncServiceLogDetail logInfo);
    // bool InsertDataSyncServiceLogDetailBulk(string logInfo);
    bool InsertDataSyncServiceErpToken(DataSyncErpAuth tokenInfo);
    DataSyncErpAuth GetDataSyncServiceErpToken(string erpCode);
    // List<DataSyncServiceLogDetail> GetDataSyncServiceFailRecords(string erpId);
    // DataSyncService GetServiceInstanceFullData(string serviceInstance);
    // List<DataSyncService> GetServiceInstancesFullData(User systemOperator, string serviceInstance);
    // Task<DataSyncServiceLog> GetDataSyncServiceLogs(string logId, int logType);
    Task<List<DataSyncServiceInstanceVisibility>> GetSyncServiceInstanceVisibility(User systemOperator, string services, TriggerType trigger);
    // List<DataSyncService> ListDisabledServices();
    // DataSyncErp MergeFullData(User systemOperator, DataSyncErp dataInfo);
    // DataSyncErpMapping MergeDataSyncServiceInstanceMapping(User systemOperator, DataSyncErpMapping instanceMapping);
    List<TimeZoneCatalog> GetTimezones(bool currentValues = false);
    string GetDatasyncDynamicBody(string entityCode);
    double GetTimezoneOffset(string offSetName = "");

    Task<List<ResponseData>> ListUpdateComponentBulk(List<ComponentExternal> itemList, List<ComponentExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level);

    Task<List<ResponseData>> ListUpdateWarehouseGroup(List<WarehouseExternal> warehouseGroupList, List<WarehouseExternal> warehouseGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level);
    Task<User> GetUserWithoutValidations(User user);

    Task<List<ResponseData>> CreateAssetsExternal(List<AssetExternal> AssetsList, List<AssetExternal> AssetListOriginal, User user, bool Validate, string Level);

    Task<List<ResponseData>> CreateUpdateProductionLine(List<AssetExternal> productionLineList, List<AssetExternal> productionLineListOriginal, User user, bool Validate, string Level);

    Task<List<ResponseData>> CreateUpdateFloor(List<AssetExternal> floorList, List<AssetExternal> floorListOriginal, User user, bool Validate, string Level);

    Task<List<ResponseData>> CreateUpdateWorkCenter(List<AssetExternal> workCenterList, List<AssetExternal> workCenterListOriginal, User user, bool Validate, string Level);

    Task<List<ResponseData>> ListUpdateInventoryGroup(List<InventoryExternal> inventoryGroupList, List<InventoryExternal> inventoryGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level);

    Task<List<ResponseData>> ListUpdateBinLocation(List<BinLocationExternal> binLocationList, List<BinLocationExternal> binLocationListOriginal, User systemOperator, bool Validate, LevelMessage Level);
}
