
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.Text;
using Range = EWP.SF.Common.Models.Range;
using SixLabors.ImageSharp;
using EWP.SF.Common.Models.Catalogs;

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

#if RELEASE
    private string FullAttachmentPath => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Config.Configuration["PathAttachment"]));
#else
    private string FullAttachmentPath => _applicationSettings.GetAppSetting("DebugAttachmentPath").ToStr();
#endif

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


    // public List<TimeZoneCatalog> GetTimezones(bool currentValues = false)
    // {
    // 	return _dataSyncRepository.GetTimezones(currentValues);
    // }

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

    public async Task<List<ResponseData>> ListUpdateComponentBulk(List<ComponentExternal> itemList, List<ComponentExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        List<ResponseData> returnValue = [];
        ResponseData MessageError;
        List<MeasureUnit> unitsList = GetMeasureUnits();
        List<Inventory> inventories = ListInventory(systemOperator, null);
        List<Component> componentsToMerge = [];
        bool NotifyOnce = false;
        if (itemList?.Count > 0)
        {
            int Line = 0;
            string BaseId = string.Empty;
            Component itemInfo = null;
            foreach (ComponentExternal cycleItem in itemList)
            {
                ComponentExternal item = cycleItem;
                Line++;
                try
                {
                    itemInfo = null;
                    BaseId = item.ItemCode;
                    Component existingComponent = (await GetComponents(cycleItem.ItemCode, false, null).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault();
                    bool editMode = existingComponent is not null;
                    if (editMode && itemListOriginal is not null)
                    {
                        item = itemListOriginal.Find(x => x.ItemCode == cycleItem.ItemCode);
                        item ??= cycleItem;
                    }
                    List<ValidationResult> results = [];
                    ValidationContext context = new(item, null, null);

                    if (!Validator.TryValidateObject(item, context, results))
                    {
                        if (editMode)
                        {
                            _ = results.RemoveAll(result => result.ErrorMessage.Contains("is required", StringComparison.OrdinalIgnoreCase));
                        }
                        if (results.Count > 0)
                        {
                            throw new Exception($"{results[0]}");
                        }
                    }
                    if (!editMode && string.IsNullOrEmpty(item.Status))
                    {
                        throw new Exception("Item InventoryUoM is invalid");
                    }
                    Status status = string.Equals(item.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled;
                    item.ItemName = item.ItemName.Replace("\"", "´´");
                    item.ItemCode = item.ItemCode.Replace("\"", "´´");
                    itemInfo = new Component
                    {
                        Code = item.ItemCode,
                        Name = !string.IsNullOrEmpty(item.ItemName) ? item.ItemName : item.ItemCode,
                        Status = status,
                        ComponentType = ComponentType.Material
                    };

                    if (!editMode || !string.IsNullOrEmpty(item.Status))
                    {
                        itemInfo.Status = status;
                    }
                    if (!editMode || !string.IsNullOrEmpty(item.InventoryUoM))
                    {
                        MeasureUnit unitInventory = unitsList.Find(unit => string.Equals(unit.Code.Trim(), item.InventoryUoM.Trim(), StringComparison.OrdinalIgnoreCase) && unit.Status == Status.Active && unit.IsProductionResult);
                        if (unitInventory is not null)
                        {
                            itemInfo.UnitInventory = unitInventory.Id;
                        }
                        else
                        {
                            throw new Exception("Item InventoryUoM is invalid");
                        }
                    }
                    if (!editMode || !string.IsNullOrEmpty(item.ProductionUoM))
                    {
                        MeasureUnit unitProduction = unitsList.Find(unit => string.Equals(unit.Code.Trim(), item.ProductionUoM.Trim(), StringComparison.OrdinalIgnoreCase) && unit.Status == Status.Active && unit.IsProductionResult);
                        if (unitProduction is not null)
                        {
                            itemInfo.UnitProduction = unitProduction.Id;
                        }
                        else
                        {
                            throw new Exception("Item ProductionUoM is invalid");
                        }
                    }
                    else
                    {
                        itemInfo.UnitProduction = itemInfo.UnitInventory;
                    }

                    if (!editMode || !string.IsNullOrEmpty(item.ManagedBy))
                    {
                        int managedById = 0;
                        switch (item.ManagedBy.Trim().ToUpperInvariant())
                        {
                            case "NONE":
                                managedById = 1; // NO MANAGEMENT
                                break;

                            case "BATCH":
                                managedById = 2; // BATCH
                                break;

                            case "SERIAL":
                                managedById = 3; // SERIE
                                break;
                        }
                        itemInfo.ManagedBy = managedById;
                    }
                    else if (editMode && string.IsNullOrEmpty(item.ManagedBy))
                    {
                        itemInfo.ManagedBy = existingComponent.ManagedBy;
                    }
                    if (!editMode || !string.IsNullOrEmpty(item.Type))
                    {
                        int typeId = 0;
                        switch (item.Type.Trim().ToUpperInvariant())
                        {
                            case "PURCHASE":
                                typeId = 1; // NO PURCHASE
                                break;

                            case "PRODUCTION":
                                typeId = 2; // PRODUCTION
                                break;
                        }
                        itemInfo.Type = typeId;
                    }
                    if (!editMode || !string.IsNullOrEmpty(item.ItemGroupCode))
                    {
                        Inventory inventoryInfo = inventories.Find(x => x.Code == item.ItemGroupCode && x.Status != Status.Failed);
                        if (inventoryInfo is not null && inventoryInfo.Code.Trim() == item.ItemGroupCode.Trim())
                        {
                            itemInfo.InventoryId = inventoryInfo.InventoryId;
                        }
                        else
                        {
                            throw new Exception("Invalid Item Group Code");
                        }
                    }
                    if (!editMode || item.SupplyLeadTime > 0)
                    {
                        itemInfo.SupplyLeadTime = item.SupplyLeadTime;
                    }
                    else
                    {
                        if (item.SupplyLeadTime == 0)
                        {
                            itemInfo.SupplyLeadTime = existingComponent.SupplyLeadTime;
                        }
                    }

                    if (!editMode || !string.IsNullOrEmpty(item.SyncProduction))
                    {
                        itemInfo.ProductSync = item.SyncProduction.Equals("YES", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.SyncProduction))
                        {
                            itemInfo.ProductSync = existingComponent.ProductSync;
                        }
                    }

                    if (!editMode || !string.IsNullOrEmpty(item.Schedule))
                    {
                        itemInfo.Schedule = item.Schedule.Equals("YES", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.SyncProduction))
                        {
                            itemInfo.Schedule = existingComponent.Schedule;
                        }
                    }

                    if (!editMode || item.SafetyQuantity.HasValue)
                    {
                        itemInfo.SafetyQty = item.SafetyQuantity.Value;
                    }
                    else
                    {
                        if (!item.SafetyQuantity.HasValue)
                        {
                            itemInfo.SafetyQty = existingComponent.SafetyQty;
                        }
                    }

                    if (!editMode || !string.IsNullOrEmpty(item.Stock))
                    {
                        itemInfo.IsStock = !item.Stock.Equals("NO", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.Stock))
                        {
                            itemInfo.IsStock = existingComponent.IsStock;
                        }
                    }
                    componentsToMerge.Add(itemInfo);
                    ResponseData response = new()
                    {
                        Code = itemInfo.Code,
                        Action = ActionDB.IntegrateAll,
                        Entity = item,
                        EntityAlt = itemInfo,
                        IsSuccess = true,
                        Id = itemInfo.Code
                    };
                    returnValue.Add(response);
                }
                catch (Exception ex)
                {
                    MessageError = new ResponseData
                    {
                        Id = BaseId,
                        Message = ex.Message
                    };
                    if (string.IsNullOrEmpty(item.ItemCode))
                    {
                        MessageError.Code = "Line:" + Line.ToStr();
                    }
                    else
                    {
                        MessageError.Code = item.ItemCode;
                    }
                    MessageError.Entity = item;
                    MessageError.EntityAlt = itemInfo;
                    returnValue.Add(MessageError);
                }
            }

            string itemsJson = JsonConvert.SerializeObject(componentsToMerge, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            ResponseData result = _dataSyncRepository.MergeComponentBulk(itemsJson, systemOperator, Validate);

            //if (componentsToMerge is not null && Services.ContextCache.Components is not null)
            //{
            //    componentsToMerge.ForEach(item =>
            //    {
            //        Component savedComp = GetComponents(item.Code, true, null)?.Find();
            //        if (savedComp is not null)
            //        {
            //            Services.ContextCache.Components.RemoveAll(comp => comp.Code == item.Code);
            //            Services.ContextCache.Components.Add(savedComp);
            //        }
            //    });

            //}
        }
        return returnValue;
    }


    public List<Inventory> ListInventory(User systemOperator, string InventoryCode = "", DateTime? DeltaDate = null)
    {
        #region Permission validation

        // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_INVENTORY_MANAGE))
        // {
        // 	throw new UnauthorizedAccessException(noPermission);
        // }

        #endregion Permission validation

        return _dataSyncRepository.ListInventory(InventoryCode, DeltaDate);
    }
    public List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null)
    {
        return _dataSyncRepository.GetMeasureUnits(unitType, unitId, DeltaDate);
    }
    public async Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "")
    {
        List<Component> returnValue;
        if (!string.IsNullOrEmpty(filter))
        {
            returnValue = await _dataSyncRepository.ListComponents(componentId, true, filter).ConfigureAwait(false);
        }
        else if (!string.IsNullOrEmpty(componentId))
        {
            returnValue = await _dataSyncRepository.ListComponents(componentId, false, string.Empty).ConfigureAwait(false);
        }
        else
        {
            returnValue = await _dataSyncRepository.ListComponents(componentId, true, filter).ConfigureAwait(false);
        }
        return returnValue?.ToArray();
    }

    #region Warehouse

    public Warehouse GetWarehouse(string Code)
    {
        return _dataSyncRepository.GetWarehouse(Code);
    }

    public async Task<ResponseData> MergeWarehouse(Warehouse WarehouseInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
    {
        ResponseData returnValue = null;

        #region Permission validation

        // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_WAREHOUSE_MANAGE))
        // {
        // 	throw new UnauthorizedAccessException(noPermission);
        // }discuss mario

        #endregion Permission validation

        // Warehouse returnValue = _dataSyncRepository.CreateWarehouse(WarehouseInfo, systemOperator);
        returnValue = _dataSyncRepository.MergeWarehouse(WarehouseInfo, systemOperator, Validate);
        // if (!Validate && returnValue is not null)
        // {
        // 	Warehouse ObjWarehouse = ListWarehouse(systemOperator, returnValue.Code).Find(x => x.Status != Status.Failed);
        // 	await ObjWarehouse.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
        // 	if (NotifyOnce)
        // 	{
        // 		_ = await SaveImageEntity("Warehouse", WarehouseInfo.Image, WarehouseInfo.Code, systemOperator).ConfigureAwait(false);
        // 		_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Warehouse, returnValue.Action, Data = ObjWarehouse }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
        // 		if (WarehouseInfo.AttachmentIds is not null)
        // 		{
        // 			foreach (string attachment in WarehouseInfo.AttachmentIds)
        // 			{
        // 				await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
        // 			}
        // 		}
        // 		returnValue.Entity = ObjWarehouse;
        // 	}
        // }discussmario
        return returnValue;
    }




    public async Task<List<ResponseData>> ListUpdateWarehouseGroup(List<WarehouseExternal> warehouseGroupList, List<WarehouseExternal> warehouseGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        List<ResponseData> returntValue = [];
        ResponseData MessageError;
        bool NotifyOnce = true;
        if (warehouseGroupList?.Count > 0)
        {
            NotifyOnce = warehouseGroupList.Count == 1;
            int Line = 0;
            string BaseId = string.Empty;
            foreach (WarehouseExternal cycleWarehouse in warehouseGroupList)
            {
                Line++;
                WarehouseExternal warehouse = cycleWarehouse;
                Warehouse originalWarehouse = GetWarehouse(cycleWarehouse.WarehouseCode);
                bool editMode = originalWarehouse is not null;
                if (editMode && warehouseGroupListOriginal is not null)
                {
                    warehouse = warehouseGroupListOriginal.Find(x => x.WarehouseCode == cycleWarehouse.WarehouseCode);
                    warehouse ??= cycleWarehouse;
                }
                try
                {
                    BaseId = warehouse.WarehouseName;
                    List<ValidationResult> results = [];
                    ValidationContext context = new(warehouse, null, null);
                    if (!Validator.TryValidateObject(warehouse, context, results))
                    {
                        if (editMode)
                        {
                            _ = results.RemoveAll(result => result.ErrorMessage.Contains("is required", StringComparison.OrdinalIgnoreCase));
                        }
                        if (results.Count > 0)
                        {
                            throw new Exception($"{results[0]}");
                        }
                    }
                    Status status = (Status)Status.Active.ToInt32();
                    if (!editMode || !string.IsNullOrEmpty(warehouse.Status))
                    {
                        if (string.Equals(warehouse.Status.Trim(), "DISABLE", StringComparison.OrdinalIgnoreCase))
                        {
                            status = (Status)Status.Disabled.ToInt32();
                        }
                    }
                    if (!editMode && status == Status.Disabled)
                    {
                        throw new Exception("Cannot import a new disabled warehouse");
                    }
                    Warehouse warehouseInfo;
                    if (!editMode)
                    {
                        warehouseInfo = new Warehouse
                        {
                            Code = warehouse.WarehouseCode,
                            Name = !string.IsNullOrEmpty(warehouse.WarehouseName) ? warehouse.WarehouseName : warehouse.WarehouseCode,
                            Status = status,
                            FacilityCode = warehouse.FacilityCode,
                            Schedule = warehouse.Schedule.ToStr().Equals("Yes", StringComparison.OrdinalIgnoreCase),
                        };
                    }
                    else
                    {
                        warehouseInfo = originalWarehouse;
                        if (!string.IsNullOrEmpty(warehouse.WarehouseName))
                        {
                            warehouseInfo.Name = warehouse.WarehouseName;
                        }
                        if (!string.IsNullOrEmpty(warehouse.FacilityCode))
                        {
                            warehouseInfo.FacilityCode = warehouse.FacilityCode;
                        }
                        warehouseInfo.Status = status;
                    }

                    if (!editMode || (warehouse.Locations?.Count > 0))
                    {
                        //List<ResponseData> binLocationsResponse = await ListUpdateBinLocation(warehouse.Locations, null, systemOperator, Validate, Level).ConfigureAwait(false);discuss mario
                        warehouseInfo.Details = null;
                    }
                    if (editMode && string.IsNullOrEmpty(warehouse.FacilityCode))
                    {
                        warehouseInfo.FacilityCode = originalWarehouse.FacilityCode;
                    }
                    ResponseData response = await MergeWarehouse(warehouseInfo, systemOperator, Validate).ConfigureAwait(false);
                    returntValue.Add(response);
                }
                catch (Exception ex)
                {
                    MessageError = new ResponseData
                    {
                        Id = BaseId,
                        Message = ex.Message,
                        Code = "Line:" + Line.ToStr()
                    };
                    returntValue.Add(MessageError);
                }
            }
        }
        return returntValue;
    }

    public Task<User> GetUserWithoutValidations(User user)
    {
        return _dataSyncRepository.GetUser(user.Id, null, new User(0));
    }

    #endregion Warehouse

    #region Asset
    public async Task<List<ResponseData>> CreateAssetsExternal(List<AssetExternal> AssetsList, List<AssetExternal> AssetListOriginal, User user, bool Validate, string Level)
    {
        List<ResponseData> returntValue = [];
        bool NotifyOnce = false;
        //Validación para saber si se notifica 1 registro desde la funcionalidad de creación
        AssetsList.ForEach(x =>
        {
            switch (x.AssetType.ToUpperInvariant())
            {
                case "FACILITY":
                    {
                        x.AssetType = Entities.Facility.ToStr();
                        break;
                    }
                case "FLOOR":
                    {
                        x.AssetType = Entities.Floor.ToStr();
                        break;
                    }
                case "WORKCENTER":
                    {
                        x.AssetType = Entities.Workcenter.ToStr();
                        break;
                    }
                case "PRODUCTIONLINE":
                    {
                        x.AssetType = Entities.ProductionLine.ToStr();
                        break;
                    }
            }
        });
        AssetsList = [.. AssetsList.OrderBy(x => Enum.Parse<Entities>(x.AssetType))];
        if (AssetsList.Count > 0 && AssetsList.Count == 1)
        {
            NotifyOnce = true;
        }
        foreach (AssetExternal asset in AssetsList)
        {
            AssetExternal currentAsset = asset;
            try
            {
                switch (asset.AssetType.ToUpperInvariant())
                {
                    case "FACILITY":
                        Facility existingFacility = GetFacility(asset.AssetCode);
                        if (existingFacility is not null && AssetListOriginal is not null)
                        {
                            currentAsset = AssetListOriginal.Find(a => a.AssetCode == asset.AssetCode);
                            currentAsset ??= asset;
                        }
                        if (existingFacility is null && !string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("Cannot import a Disabled Facility record");
                        }
                        ResponseData resultFacility = await CreateFacility(currentAsset, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
                        if (resultFacility is not null)
                        {
                            resultFacility.Entity = Entities.Facility.ToStr();
                            returntValue.Add(resultFacility);
                        }
                        break;

                    case "FLOOR":
                        Floor existingFloor = GetFloor(asset.AssetCode);
                        if (existingFloor is not null && AssetListOriginal is not null)
                        {
                            currentAsset = AssetListOriginal.Find(a => a.AssetCode == asset.AssetCode);
                            currentAsset ??= asset;
                        }
                        if (existingFloor is null && !string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception("Cannot import a Disabled Floor record");
                        }
                        ResponseData resultFloor = await CreateFloor(currentAsset, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
                        if (resultFloor is not null)
                        {
                            resultFloor.Entity = Entities.Floor.ToStr();
                            returntValue.Add(resultFloor);
                        }
                        break;

                    case "WORKCENTER":
                        WorkCenter existingWorkcenter = GetWorkCenter(asset.AssetCode);
                        if (existingWorkcenter is not null && AssetListOriginal is not null)
                        {
                            currentAsset = AssetListOriginal.Find(a => a.AssetCode == asset.AssetCode);
                            currentAsset ??= asset;
                        }
                        if (existingWorkcenter is null && !string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception("Cannot import a Disabled Floor record");
                        }
                        ResponseData resultWorkCenter = await CreateWorkCenter(currentAsset, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
                        if (resultWorkCenter is not null)
                        {
                            resultWorkCenter.Entity = Entities.Workcenter.ToStr();
                            returntValue.Add(resultWorkCenter);
                        }
                        break;

                    case "PRODUCTIONLINE":
                        ProductionLine existingProductionLine = GetProductionLine(asset.AssetCode, user);
                        if (existingProductionLine is not null && AssetListOriginal is not null)
                        {
                            currentAsset = AssetListOriginal.Find(a => a.AssetCode == asset.AssetCode);
                            currentAsset ??= asset;
                        }
                        if (existingProductionLine is null && !string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception("Cannot import a Disabled Floor record");
                        }
                        ResponseData resultProductLine = await CreateUpdateProductionLine(currentAsset, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
                        if (resultProductLine is not null)
                        {
                            resultProductLine.Entity = Entities.ProductionLine.ToStr();
                            returntValue.Add(resultProductLine);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                returntValue.Add(new ResponseData
                {
                    Entity = Entities.ProductionLine.ToStr(),
                    Action = ActionDB.Create,
                    IsSuccess = false,
                    Message = ex.Message,
                    Code = asset.AssetCode
                });
            }
        }

        return returntValue;
    }

    private async Task<ResponseData> CreateFacility(AssetExternal asset, User user, bool Validate, string Level, bool NotifyOnce)
    {
        Facility ObjFacility = new()
        {
            Code = asset.AssetCode,
            Name = asset.AssetName,
            AssetType = asset.AssetType,
            Status = string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled
        };
        return await CreateFacility(ObjFacility, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
    }

    private async Task<ResponseData> CreateFloor(AssetExternal asset, User user, bool Validate, string Level, bool NotifyOnce)
    {
        Floor ObjFloor = new()
        {
            Code = asset.AssetCode,
            Name = asset.AssetName,
            AssetType = asset.AssetType,
            ParentCode = asset.ParentCode,
            Status = string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled
        };
        //Floor existingRecord = GetFloor(asset.AssetCode);
        //if (existingRecord is not null)
        //{
        //    ObjFloor.ParentCode = existingRecord.ParentCode;
        //}
        return await CreateFloor(ObjFloor, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
    }

    private async Task<ResponseData> CreateWorkCenter(AssetExternal asset, User user, bool Validate, string Level, bool NotifyOnce)
    {
        WorkCenter ObjWorkCenter = new()
        {
            Code = asset.AssetCode,
            Name = asset.AssetName,
            AssetType = asset.AssetType,
            ParentCode = asset.ParentCode,
            ParentId = asset.ParentCode,
            Status = string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled
        };
        _ = GetWorkCenter(asset.AssetCode);
        //if (existingRecord is not null)
        //{
        //    ObjWorkCenter.ParentCode = existingRecord.ParentCode;
        //}
        return await CreateWorkCenter(ObjWorkCenter, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
    }

    private async Task<ResponseData> CreateUpdateProductionLine(AssetExternal asset, User user, bool Validate, string Level, bool NotifyOnce)
    {
        ProductionLine ObjProductionLine = new()
        {
            Code = asset.AssetCode,
            Description = asset.AssetName,
            AssetType = asset.AssetType,
            ParentCode = asset.ParentCode,
            Status = string.Equals(asset.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled
        };
        _ = GetProductionLine(asset.AssetCode, user);
        //if (existingRecord is not null)
        //{
        //    ObjProductionLine.ParentCode = existingRecord.ParentCode;
        //}
        return await CreateProductionLine(ObjProductionLine, user, Validate, Level, NotifyOnce).ConfigureAwait(false);
    }

    #endregion Asset

    

    #region Inventory
    public async Task<List<ResponseData>> ListUpdateInventoryGroup(List<InventoryExternal> inventoryGroupList, List<InventoryExternal> inventoryGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        List<ResponseData> returntValue = [];
        ResponseData MessageError;
        bool NotifyOnce = true;
        if (inventoryGroupList?.Count > 0)
        {
            NotifyOnce = inventoryGroupList.Count == 1;
            int Line = 0;
            string BaseId = string.Empty;
            foreach (InventoryExternal cycleInventoryGroup in inventoryGroupList)
            {
                InventoryExternal inventoryGroup = cycleInventoryGroup;
                Line++;
                try
                {
                    BaseId = inventoryGroup.ItemGroupCode;
                    Inventory existingInventory = GetInventory(cycleInventoryGroup.ItemGroupCode);
                    bool editMode = existingInventory is not null;
                    if (editMode && inventoryGroupListOriginal is not null)
                    {
                        inventoryGroup = inventoryGroupListOriginal.Find(x => x.ItemGroupCode == cycleInventoryGroup.ItemGroupCode);
                        inventoryGroup ??= cycleInventoryGroup;
                    }
                    List<ValidationResult> results = [];
                    ValidationContext context = new(inventoryGroup, null, null);
                    if (!Validator.TryValidateObject(inventoryGroup, context, results))
                    {
                        if (editMode)
                        {
                            _ = results.RemoveAll(result => result.ErrorMessage.Contains("is required", StringComparison.OrdinalIgnoreCase));
                        }
                        if (results.Count > 0)
                        {
                            throw new Exception($"{results[0]}");
                        }
                    }
                    Status status = (Status)Status.Active.ToInt32();
                    if (!string.IsNullOrEmpty(inventoryGroup.Status) && string.Equals(inventoryGroup.Status.Trim(), "INACTIVE", StringComparison.OrdinalIgnoreCase))
                    {
                        status = (Status)Status.Disabled.ToInt32();
                    }
                    if (status != Status.Active && !editMode)
                    {
                        throw new Exception("Cannot import a disabled Inventory Group record");
                    }
                    Inventory inventoryGroupInfo = new()
                    {
                        Code = inventoryGroup.ItemGroupCode,
                        Name = !string.IsNullOrEmpty(inventoryGroup.ItemGroupName) ? inventoryGroup.ItemGroupName : inventoryGroup.ItemGroupCode,
                        Status = status
                    };
                    // returntValue.Add(_dataSyncRepository.MergeInventory(inventoryGroupInfo, systemOperator, Validate, Level));
                    ResponseData response = await MergeInventory(inventoryGroupInfo, systemOperator, Validate).ConfigureAwait(false);
                    returntValue.Add(response);
                }
                catch (Exception ex)
                {
                    MessageError = new ResponseData
                    {
                        Id = BaseId,
                        Message = ex.Message,
                        Code = "Line:" + Line.ToStr()
                    };
                    returntValue.Add(MessageError);
                }
            }
        }

        return returntValue;
    }

    public Inventory GetInventory(string Code)
    {
        return _dataSyncRepository.GetInventory(Code);
    }

    public async Task<ResponseData> MergeInventory(Inventory InventoryInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
    {
        ResponseData returnValue = new();

        #region Permission validation

        // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_INVENTORY_MANAGE))
        // {
        // 	throw new UnauthorizedAccessException(noPermission);
        // }

        #endregion Permission validation

        returnValue = _dataSyncRepository.MergeInventory(InventoryInfo, systemOperator, Validate);
        if (!Validate && returnValue is not null)
        {
            Inventory ObjInventory = ListInventory(systemOperator, returnValue.Code).Find(x => x.Status != Status.Failed);
            //await ObjInventory.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            if (NotifyOnce)
            {
                _ = await SaveImageEntity("ItemGroup", InventoryInfo.Image, InventoryInfo.Code, systemOperator).ConfigureAwait(false);
                if (InventoryInfo.AttachmentIds is not null)
                {
                    foreach (string attachment in InventoryInfo.AttachmentIds)
                    {
                        await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                    }
                }
                // _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Inventory, returnValue.Action, Data = ObjInventory }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                returnValue.Entity = ObjInventory;
            }
        }
        return returnValue;
    }
    #endregion Inventory

    #region BinLocation
    public async Task<List<ResponseData>> ListUpdateBinLocation(List<BinLocationExternal> binLocationList, List<BinLocationExternal> binLocationListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        List<ResponseData> returnValue = [];
        ResponseData MessageError;
        bool NotifyOnce = true;
        if (binLocationList?.Count > 0)
        {
            NotifyOnce = binLocationList.Count == 1;
            int Line = 0;
            string BaseId = string.Empty;
            foreach (BinLocationExternal cycleLocation in binLocationList)
            {
                BinLocationExternal binLocation = cycleLocation;
                Line++;
                try
                {
                    BaseId = binLocation.LocationCode;
                    BinLocation originalBinLocation = ListBinLocation(systemOperator, cycleLocation.LocationCode)?.Find(x => x.Status != Status.Failed);
                    bool editMode = originalBinLocation is not null;
                    if (editMode && binLocationListOriginal is not null)
                    {
                        binLocation = binLocationListOriginal.Find(x => x.LocationCode == cycleLocation.LocationCode);
                        binLocation ??= cycleLocation;
                    }
                    List<ValidationResult> results = [];
                    ValidationContext context = new(binLocation, null, null);
                    if (!Validator.TryValidateObject(binLocation, context, results))
                    {
                        if (editMode)
                        {
                            _ = results.RemoveAll(x => x.ErrorMessage.Contains("is required", StringComparison.OrdinalIgnoreCase));
                        }
                        if (results.Count > 0)
                        {
                            throw new Exception($"{results[0]}");
                        }
                    }
                    string warehouseId = null;
                    if (!editMode || !string.IsNullOrEmpty(binLocation.WarehouseCode))
                    {
                        Warehouse warehouseData = GetWarehouse(binLocation.WarehouseCode);
                        if (warehouseData is not null)
                        {
                            warehouseId = warehouseData.WarehouseId;
                        }
                        if (string.IsNullOrEmpty(warehouseId))
                        {
                            throw new Exception("Warehouse Not Found");
                        }
                    }
                    Status status = Status.Disabled;

                    if ((!editMode || !string.IsNullOrEmpty(binLocation.Status)) && !string.IsNullOrWhiteSpace(binLocation.Status))
                    {
                        status = string.Equals(binLocation.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled;
                    }
                    BinLocation binLocationInfo = new()
                    {
                        LocationCode = binLocation.LocationCode,
                        Name = !string.IsNullOrEmpty(binLocation.LocationName) ? binLocation.LocationName : binLocation.LocationCode,
                        Aisle = binLocation.Aisle,
                        Rack = binLocation.Rack,
                        Level = binLocation.Level,
                        Column = binLocation.Column,
                        WarehouseId = warehouseId,
                        Status = status
                    };
                    if (editMode)
                    {
                        binLocationInfo.InventoryStatusCodes = originalBinLocation.InventoryStatusCodes;
                    }
                    if (editMode && string.IsNullOrEmpty(binLocation.Status))
                    {
                        binLocationInfo.Status = originalBinLocation.Status;
                    }
                    ResponseData response = await MergeBinLocation(binLocationInfo, systemOperator, Validate).ConfigureAwait(false);
                    returnValue.Add(response);
                }
                catch (Exception ex)
                {
                    MessageError = new ResponseData
                    {
                        Id = BaseId,
                        Message = ex.Message,
                        Code = "Line:" + Line.ToStr()
                    };
                    returnValue.Add(MessageError);
                }
            }
        }

        return returnValue;
    }
    public List<BinLocation> ListBinLocation(User systemOperator, string binLocationCode = "", DateTime? DeltaDate = null)
    {
        #region Permission validation

        // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_WAREHOUSE_MANAGE))
        // {
        // 	throw new UnauthorizedAccessException(noPermission);
        // }

        #endregion Permission validation

        return _dataSyncRepository.ListBinLocation(binLocationCode, DeltaDate);
    }

    public async Task<ResponseData> MergeBinLocation(BinLocation BinLocationInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
    {
        ResponseData returnValue = null;

        #region Permission validation

        // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_WAREHOUSE_MANAGE))
        // {
        // 	throw new UnauthorizedAccessException(noPermission);
        // }

        #endregion Permission validation

        returnValue = _dataSyncRepository.MergeBinLocation(BinLocationInfo, systemOperator, Validate);
        if (!Validate && returnValue is not null)
        {
            BinLocation ObjBinLocation = ListBinLocation(systemOperator, returnValue.Code).Find(x => x.Status != Status.Failed);
            //await ObjBinLocation.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            _ = await SaveImageEntity("BinLocation", BinLocationInfo.Image, ObjBinLocation.LocationCode, systemOperator).ConfigureAwait(false);
            if (BinLocationInfo.AttachmentIds is not null)
            {
                foreach (string attachment in BinLocationInfo.AttachmentIds)
                {
                    await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                }
            }
            // if (NotifyOnce)
            // {
            // 	_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.BinLocation, returnValue.Action, Data = ObjBinLocation }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
            // 	returnValue.Entity = ObjBinLocation;
            // }
        }
        return returnValue;
    }
    #endregion  BinLocation

    #region SchedulingShiftStatus


    /// <summary>
    /// Method put
    /// </summary>
    /// <param name="requestValue"></param>
    /// <param name="systemOperator"></param>
    /// <param name="Validate"></param>
    /// <param name="Level"></param>
    /// <param name="NotifyOnce"></param>
    /// <returns></returns>
    public async Task<List<ResponseData>> UpdateSchedulingShiftStatus(List<SchedulingShiftStatus> requestValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true)
    {
        List<ResponseData> returnValue = [];
        ResponseData responseMessage;
        ResponseData responseError;
        //List<User> users = new List<User>();
        SchedulingShiftStatus scheduleLog = null;
        int Line = 0;

        #region Permission validation

        // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_SCHEDULING_SHIFT_STATUS_MANAGE))
        // {
        //     throw new UnauthorizedAccessException(noPermission);
        // }

        #endregion Permission validation

        NotifyOnce = requestValue.Count == 1;
        foreach (SchedulingShiftStatus item in requestValue)
        {
            try
            {
                Line++;
                item.UserId = systemOperator.Id;
                responseMessage = _dataSyncRepository.PutSchedulingShiftStatus(item, systemOperator, Validate);
                returnValue.Add(responseMessage);
                if (!responseMessage.IsSuccess)
                {
                    continue;
                }

                if (!Validate)
                {
                    scheduleLog = _dataSyncRepository.GetSchedulingShiftStatus(item.Code, item.Type).FirstOrDefault();
                    //await scheduleLog.Log(responseMessage.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                }

                if (NotifyOnce && !Validate)
                {
                    if (item.AttachmentIds is not null)
                    {
                        foreach (string attachment in item.AttachmentIds)
                        {
                            await AttachmentSync(attachment, item.Code, systemOperator).ConfigureAwait(false);
                        }
                    }
                    // _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = scheduleLog.Type == "Asset" ? Entities.ShiftStatusAsset : Entities.ShiftStatusEmployee, Data = scheduleLog, responseMessage.Action }, responseMessage.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                }
            }
            catch (Exception ex)
            {
                responseError = new ResponseData
                {
                    Message = ex.Message,
                    Code = "Line:" + Line.ToStr()
                };
                returnValue.Add(responseError);
            }
        }
        // if (!NotifyOnce && !Validate)
        // {
        //     _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = scheduleLog.Type == "Asset" ? Entities.ShiftStatusAsset : Entities.ShiftStatusEmployee, Data = new object { }, Action = ActionDB.IntegrateAll });
        // }
        switch (Level)
        {
            case LevelMessage.Warning:
                returnValue = [.. returnValue.Where(p => !string.IsNullOrEmpty(p.Message))];
                break;

            case LevelMessage.Error:
                returnValue = [.. returnValue.Where(p => !p.IsSuccess)];
                break;
            case LevelMessage.Success:
                break;
        }
        return returnValue;
        //return _dataSyncRepository.PutSchedulingShiftStatus(requestValue);
    }


    #endregion SchedulingShiftStatus


    #region SchedulingCalendarShifts


    /// <summary>
    /// Method put
    /// </summary>
    /// <param name="requestValue"></param>
    /// <param name="systemOperator"></param>
    /// <returns></returns>
    public SchedulingCalendarShifts UpdateSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator)
    {
        #region Permission validation

        string errores = string.Empty;
        bool esNuevo = true;
        //if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_SCHEDULING_SHIFT_STATUS_MANAGE))
        //{
        //    throw new UnauthorizedAccessException(noPermission);
        //}

        #endregion Permission validation

        //New Calendar Shift
        ValidatePostSchedulingCalendarShift(requestValue);
        requestValue.UserId = systemOperator.Id;
        if (requestValue.IsParent)
        {
            SchedulingCalendarShifts UpdateCalendarShift = _dataSyncRepository.GetSchedulingCalendarShifts(string.IsNullOrEmpty(requestValue.Id) ? "0x0x0x0x0" : requestValue.Id, null, null, requestValue.AssetLevel, null).FirstOrDefault();
            if (UpdateCalendarShift is not null)
            {
                esNuevo = false;
                if (UpdateCalendarShift.IsParent)
                {
                    foreach (SchedulingCalendarShifts item in _dataSyncRepository.GetSchedulingCalendarShifts(null, null, UpdateCalendarShift.IdParent, 0, null))
                    {
                        //Se valida si deselecciono algun item para borrarlo.
                        if (item.AssetLevel > UpdateCalendarShift.AssetLevel && !requestValue.listChildren.Any(q => q.Id == item.Id))
                        {
                            ValidateDeleteSchedulingCalendarShift(item);
                            _ = _dataSyncRepository.DeleteSchedulingCalendarShifts(item);
                        }
                    }
                }
            }

            _ = _dataSyncRepository.PutSchedulingCalendarShifts(requestValue, systemOperator);
            //Se aplican cambios solo a los registros seleccionados.
            List<SchedulingCalendarShifts> childrenList;
            if (!requestValue.isEmployee && requestValue.Validation)//Para proxima funcionalidad
            {
                requestValue.listChildren.ForEach(q =>
                {
                    q.AssetLevelCode = q.IdAsset[..q.IdAsset[..q.IdAsset.IndexOf('-')].Length];
                    q.IdAsset = q.IdAsset.Substring(q.IdAsset.IndexOf('-') + 1, q.IdAsset.Length - q.IdAsset[..q.IdAsset.IndexOf('-')].Length - 1);
                });
            }
            //Falta validar el level para que sea unico el valor.
            childrenList = [.. requestValue.listChildren.Where(q => q.IdAsset != requestValue.IdAsset || q.AssetLevelCode != requestValue.AssetLevelCode)];

            foreach (SchedulingCalendarShifts children in childrenList)
            {
                if (!esNuevo)
                {
                    SchedulingCalendarShifts UpdateChildCalendarShift = _dataSyncRepository.GetSchedulingCalendarShifts(null, null, UpdateCalendarShift is not null ? UpdateCalendarShift.IdParent : requestValue.IdParent, 0, requestValue.AssetLevelCode)
                        .Find(q => q.IdAsset == children.IdAsset && q.AssetLevelCode == children.AssetLevelCode && q.FromDate == (UpdateCalendarShift is not null ? UpdateCalendarShift.FromDate : requestValue.FromDate));
                    children.Id = UpdateChildCalendarShift?.Id;
                }
                children.FromDate = requestValue.FromDate;
                children.IdParent = requestValue.IdParent;
                children.Origin = requestValue.Origin;
                //children.AssetLevel = requestValue.AssetLevel;
                children.CodeShift = requestValue.CodeShift;
                children.Status = requestValue.Status;
                children.Validation = requestValue.Validation;
                children.IsParent = false;
                try
                {
                    ValidatePostSchedulingCalendarShift(children);
                    if (!children.Validation)
                    {
                        _ = _dataSyncRepository.PutSchedulingCalendarShifts(children, systemOperator);
                    }
                }
                catch (Exception ex)
                {
                    errores += ex.Message + "|";
                }
            }
            return !string.IsNullOrEmpty(errores) ? throw new Exception(errores) : requestValue;
        }
        else
        {
            return _dataSyncRepository.PutSchedulingCalendarShifts(requestValue, systemOperator);
        }
    }

    /// <summary>
    /// Method delete
    /// </summary>
    /// <param name="requestValue"></param>
    /// <param name="systemOperator"></param>
    /// <returns></returns>
    public bool DeleteSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator)
    {
        #region Permission validation

        string errores = string.Empty;
        //if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_SCHEDULING_SHIFT_STATUS_MANAGE))
        //{
        //    throw new UnauthorizedAccessException(noPermission);
        //}

        #endregion Permission validation

        requestValue.UserId = systemOperator.Id;
        if (requestValue.IsParent)
        {
            ValidateDeleteSchedulingCalendarShift(requestValue);
            SchedulingCalendarShifts DeleteCalendarShift = _dataSyncRepository.GetSchedulingCalendarShifts(requestValue.Id, null, null, requestValue.AssetLevel, null).FirstOrDefault();
            //Se aplica borrado solo a los registros seleccionados.
            List<SchedulingCalendarShifts> childrenList;
            if (!requestValue.isEmployee)
            {
                requestValue.listChildren.ForEach(q =>
                {
                    q.AssetLevelCode = q.IdAsset[..q.IdAsset[..q.IdAsset.IndexOf('-')].Length];
                    q.IdAsset = q.IdAsset.Substring(q.IdAsset.IndexOf('-') + 1, q.IdAsset.Length - q.IdAsset[..q.IdAsset.IndexOf('-')].Length - 1);
                });
            }
            childrenList = [.. requestValue.listChildren.Where(q => q.IdAsset != DeleteCalendarShift.IdAsset || q.AssetLevelCode != DeleteCalendarShift.AssetLevelCode)];
            foreach (SchedulingCalendarShifts children in childrenList)
            {
                SchedulingCalendarShifts DeleteChildCalendarShift = _dataSyncRepository.GetSchedulingCalendarShifts(null, null, DeleteCalendarShift.IdParent, requestValue.AssetLevel, requestValue.AssetLevelCode)
                    .Find(q => q.IdAsset == children.IdAsset && q.AssetLevelCode == children.AssetLevelCode && q.FromDate == DeleteCalendarShift.FromDate);
                children.Id = DeleteChildCalendarShift?.Id;
                try
                {
                    ValidateDeleteSchedulingCalendarShift(children);
                    _ = _dataSyncRepository.DeleteSchedulingCalendarShifts(children);
                }
                catch (Exception ex)
                {
                    errores += ex.Message + ",";
                }
            }
            if (!string.IsNullOrEmpty(errores))
            {
                throw new Exception(errores);
            }
            _ = _dataSyncRepository.DeleteSchedulingCalendarShifts(requestValue);
            return true;
        }
        else
        {
            ValidateDeleteSchedulingCalendarShift(requestValue);
            return _dataSyncRepository.DeleteSchedulingCalendarShifts(requestValue);
        }
    }

    private void ValidatePostSchedulingCalendarShift(SchedulingCalendarShifts validRequest)
    {
        //New Calendar Shift
        if (string.IsNullOrEmpty(validRequest.Id))
        {
            List<SchedulingCalendarShifts> calendarsShift = _dataSyncRepository.GetSchedulingCalendarShifts(null, validRequest.IdAsset, null, validRequest.AssetLevel, validRequest.AssetLevelCode
                , validRequest.Origin);
            if (calendarsShift.Count > 0)
            {
                SchedulingCalendarShifts last = calendarsShift.FirstOrDefault();
                if (validRequest.FromDate <= last.FromDate)
                {
                    throw new Exception("@ExistShiftBindingBefore" + " | " + validRequest.AssetLevelCode + ": " + validRequest.IdAsset + "\r\n");
                }
                if (validRequest.CodeShift == last.CodeShift)
                {
                    throw new Exception("@CurrentShiftBinding" + " | " + validRequest.AssetLevelCode + ": " + validRequest.IdAsset + " - " + validRequest.Name + "\r\n");
                }
            }
        }
        else
        {
            List<SchedulingCalendarShifts> calendarsShift = _dataSyncRepository.GetSchedulingCalendarShifts(validRequest.Id, validRequest.IdAsset, null, validRequest.AssetLevel, null, validRequest.Origin);
            if (calendarsShift.Count > 0)
            {
                SchedulingCalendarShifts update = calendarsShift.FirstOrDefault();
                if (update.ToDate.HasValue)
                {
                    throw new Exception("@NotCurrentShiftBinding");
                }
                List<SchedulingCalendarShifts> beforeCalendarsShift = _dataSyncRepository.GetSchedulingCalendarShifts(null, validRequest.IdAsset, null, validRequest.AssetLevel, null, validRequest.Origin);
                SchedulingCalendarShifts beforeUpdate = beforeCalendarsShift.Find(q => q.Id != validRequest.Id);
                if (beforeUpdate is not null)
                {
                    if (beforeUpdate.CodeShift == validRequest.CodeShift)
                    {
                        throw new Exception("@SameShiftBinding");
                    }
                    if (beforeUpdate.FromDate >= validRequest.FromDate)
                    {
                        throw new Exception("@ExistShiftBindingBefore");
                    }
                }
            }
            else
            {
                throw new Exception("@NoRelatedRecords");
            }
        }
    }

    private void ValidateDeleteSchedulingCalendarShift(SchedulingCalendarShifts validRequest)
    {
        if (string.IsNullOrEmpty(validRequest.Id))
        {
            throw new Exception("@NoRelatedRecords");
        }
        List<SchedulingCalendarShifts> calendarsShift = _dataSyncRepository.GetSchedulingCalendarShifts(validRequest.Id, null, null, validRequest.AssetLevel, null);
        if (calendarsShift.Count > 0)
        {
            SchedulingCalendarShifts delete = calendarsShift.FirstOrDefault();
            if (delete.ToDate.HasValue)
            {
                throw new Exception("@NotCurrentShiftBinding");
            }
            if (delete.FromDate < DateTime.Now)
            {
                throw new Exception("@NotDeleteShiftBinding");
            }
        }
        else
        {
            throw new Exception("@NoRelatedRecords");
        }
    }

    #endregion SchedulingCalendarShifts

    #region Activity
    public async Task<Activity> CreateActivity(Activity activityInfo, User systemOperator)
    {
        Activity returnValue = null;
        XmlSerializer xser = null;
        MessageBroker callback = null;
        MemoryStream ms = null;
        if (!string.IsNullOrEmpty(activityInfo.IncludedAssets))
        {
            List<AssetsTree> includedAssetsJson =
            [
                .. activityInfo.IncludedAssets.Split(',').Select(q => new AssetsTree
                    {
                        AssetCode = q.Substring(q.IndexOf('-') + 1, q.Length - q[..q.IndexOf('-')].Length - 1),
                        AssetTypeCode = q[..q[..q.IndexOf('-')].Length]
                    }),
                ];

            if (includedAssetsJson is not null)
            {
                activityInfo.IncludedAssets = JsonConvert.SerializeObject(includedAssetsJson);
            }
        }
        if (activityInfo.CurrentProcessMaster is not null
            && activityInfo.RequiresInstructions && activityInfo.CurrentProcessMaster.IsNewProcess && !string.IsNullOrEmpty(activityInfo.CurrentProcessMaster.ProcedureId))
        {
            activityInfo.CurrentProcessMaster.IsManualActivity = true;
            string jsonString = JsonConvert.SerializeObject(activityInfo.CurrentProcessMaster);
            Procedure ProcedureTmp = JsonConvert.DeserializeObject<Procedure>(jsonString);
            Common.ResponseModels.ResponseData ResultProcedure = await ProcessMasterInsByXML(ProcedureTmp, systemOperator).ConfigureAwait(false);
            //var ResultProcedure =ProcessMasterInsByXML(activityInfo.CurrentProcessMaster, systemOperator);
            //Fix Procedures222
            if (ResultProcedure.IsSuccess)
            {
                activityInfo.ParentId = ResultProcedure.Id;
            }
        }

        returnValue = _dataSyncRepository.CreateActivity(activityInfo, systemOperator);
        if (!string.IsNullOrEmpty(activityInfo.Image))
        {
            _ = await SaveImageEntity("Activity", activityInfo.Image, activityInfo.Id, systemOperator).ConfigureAwait(false);
        }
        if (activityInfo.AttachmentIds is not null)
        {
            foreach (string attachmentId in activityInfo.AttachmentIds)
            {
                await AttachmentSync(attachmentId, returnValue.Id, systemOperator).ConfigureAwait(false);
            }
        }
        if (activityInfo.CurrentProcessMaster?.ProcedureId is not null
            && activityInfo.RequiresInstructions)
        {
            List<ComponentInstruction> listComponents = [];
            foreach (ProcedureSection section in activityInfo.CurrentProcessMaster.Sections)
            {
                foreach (ProcedureMasterInstruction instruction in section.ListInstrucctions)
                {
                    if (instruction.Components?.Count > 0)
                    {
                        foreach (ComponentInstruction component in instruction.Components)
                        {
                            component.InstructionId = instruction.InstructionId;
                            component.ActivityId = activityInfo.Id;
                            listComponents.Add(component);
                            if (component.AttachmentId is not null
                               && !string.IsNullOrEmpty(component.AttachmentId))
                            {
                                _ = await AttachmentSync(component.AttachmentId, component.Id, systemOperator).ConfigureAwait(false);
                            }
                        }
                        instruction.Components = null;
                    }
                }
            }
            string xmlComponents = string.Empty;

            if (listComponents.Count > 0)
            {
                ms = new MemoryStream();
                xser = new XmlSerializer(typeof(List<ComponentInstruction>));
                xser.Serialize(ms, listComponents);
                xmlComponents = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                bool resultItems = _dataSyncRepository.ActivityItemInsByXML(systemOperator, xmlComponents);
            }
        }

        //await activityInfo.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(returnValue.Id))
        {
            ActivityInstanceCalculateRequest activityInfoSchedule = new()
            {
                ActivityId = activityInfo.Id,
                RecurrenceCode = activityInfo.Schedule.RecurrenceCode,
                OptionsEndCode = activityInfo.Schedule.OptionsEndCode,
                DailyCode = activityInfo.Schedule.DailyCode,
                StartDate = activityInfo.Schedule.StartDate,
                EndDate = activityInfo.Schedule.EndDate,
                DurationInSec = activityInfo.Schedule.DurationInSec,
                DailyDays = activityInfo.Schedule.DailyDays,
                NoWeeks = activityInfo.Schedule.NoWeeks,
                OptionsWeekly = activityInfo.Schedule.OptionsWeekly,
                Name = activityInfo.Name,
                OneTime = activityInfo.Schedule.OneTime,
                IsCreateActivity = true,
                MonthlyOptionsCode = activityInfo.Schedule.MonthlyOptionsCode,
                MonthlyEvery = activityInfo.Schedule.MonthlyEvery,
                MonthlyDay = activityInfo.Schedule.MonthlyDay,
                MonthlyDayCode = activityInfo.Schedule.MonthlyDayCode,
                MonthlyOrderDaysCode = activityInfo.Schedule.MonthlyOrderDaysCode,
                Occurrences = activityInfo.Schedule.Occurrences,
                MonthlyByYearly = activityInfo.Schedule.MonthlyByYearly,
                YearlyOptionsCode = activityInfo.Schedule.YearlyOptionsCode,
                EveryYear = activityInfo.Schedule.EveryYear,
                EveryHour = activityInfo.Schedule.EveryHour
            };

            if (!string.Equals(activityInfo.Origin, "PRODUCT", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(activityInfo.Origin, "ORDER", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(activityInfo.Origin, "OPERATIONTYPE", StringComparison.OrdinalIgnoreCase))
            {
                List<ActivityInstanceCalculateResponse> tmpListInstance = _dataSyncRepository.ActivityInstanceCalculate(activityInfoSchedule, systemOperator);
                if (tmpListInstance is not null)
                {
                    returnValue.ListInstaceResponse = [];
                    returnValue.ListInstaceResponse = tmpListInstance;
                }
            }
            else
            {
                callback = _dataSyncRepository.ActivityMergeSchedule(activityInfo, systemOperator);
            }
            //  callback = this._dataSyncRepository.ActivityMergeSchedule(activityInfo, systemOperator);
            //  callback = this._dataSyncRepository.ActivityMergeSchedule(activityInfo, systemOperator);
            activityInfo.Schedule.ActivityId = activityInfo.Id;
            //await activityInfo.Schedule.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);

            // if (callback is not null)
            // {
            //     Services.SyncInitializer.ForcePush(callback);
            // }discuss mario
        }
        return returnValue;
    }

    public async Task<Activity> UpdateActivity(Activity activityInfo, User systemOperator)
    {
        Activity returnValue = new();
        XmlSerializer xser = null;
        MemoryStream ms = null;
        if (!string.IsNullOrEmpty(activityInfo.IncludedAssets))
        {
            List<AssetsTree> includedAssetsJson =
            [
                .. activityInfo.IncludedAssets.Split(',').Select(q => new AssetsTree
                    {
                        AssetCode = q.Substring(q.IndexOf('-') + 1, q.Length - q[..q.IndexOf('-')].Length - 1),
                        AssetTypeCode = q[..q[..q.IndexOf('-')].Length]
                    }),
                ];

            if (includedAssetsJson is not null)
            {
                activityInfo.IncludedAssets = JsonConvert.SerializeObject(includedAssetsJson);
            }
        }
        if (activityInfo.CurrentProcessMaster is not null
                      && activityInfo.RequiresInstructions && (activityInfo.CurrentProcessMaster.IsNewProcess || activityInfo.CurrentProcessMaster.IsManualActivity)
                      && !string.IsNullOrEmpty(activityInfo.CurrentProcessMaster.ProcedureId))
        {
            //activityInfo.CurrentProcessMaster.IsManualActivity = true;
            //Procedure ProcedureTmp = (Procedure)activityInfo.CurrentProcessMaster.Clone();

            string jsonString = JsonConvert.SerializeObject(activityInfo.CurrentProcessMaster);
            Procedure ProcedureTmp = JsonConvert.DeserializeObject<Procedure>(jsonString);

            Common.ResponseModels.ResponseData ResultProcedure = await ProcessMasterInsByXML(ProcedureTmp, systemOperator).ConfigureAwait(false);
            if (ResultProcedure.IsSuccess)
            {
                activityInfo.ParentId = ResultProcedure.Id;
            }
        }
        if (activityInfo.CurrentProcessMaster is not null
            && !string.IsNullOrEmpty(activityInfo.CurrentProcessMaster.ProcedureId)
            && activityInfo.CurrentProcessMaster.ProcedureId is not null
            && activityInfo.RequiresInstructions)
        {
            List<ComponentInstruction> listComponents = [];
            foreach (ProcedureSection section in activityInfo.CurrentProcessMaster.Sections)
            {
                foreach (ProcedureMasterInstruction instruction in section.ListInstrucctions)
                {
                    if (instruction.Components?.Count > 0)
                    {
                        foreach (ComponentInstruction component in instruction.Components)
                        {
                            component.InstructionId = instruction.InstructionId;
                            component.ActivityId = activityInfo.Id;
                            listComponents.Add(component);
                            if (component.AttachmentId is not null
                               && !string.IsNullOrEmpty(component.AttachmentId))
                            {
                                _ = await AttachmentSync(component.AttachmentId, component.Id, systemOperator).ConfigureAwait(false);
                            }
                        }
                        instruction.Components = null;
                    }
                }
            }
            string xmlComponents = string.Empty;
            if (listComponents.Count > 0)
            {
                ms = new MemoryStream();
                xser = new XmlSerializer(typeof(List<ComponentInstruction>));
                xser.Serialize(ms, listComponents);
                xmlComponents = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");

                bool resultItems = _dataSyncRepository.ActivityItemInsByXML(systemOperator, xmlComponents);
            }
        }
        if (activityInfo.EditSeries || activityInfo.Schedule.OneTime)
        {
            if (_dataSyncRepository.UpdateActivity(activityInfo, systemOperator))
            {
                returnValue = activityInfo;
            }
        }
        if (!string.IsNullOrEmpty(activityInfo.Image))
        {
            _ = await SaveImageEntity("Activity", activityInfo.Image, activityInfo.Id, systemOperator).ConfigureAwait(false);
        }

        if (activityInfo.AttachmentIds is not null)
        {
            foreach (string attachmentId in activityInfo.AttachmentIds)
            {
                await AttachmentSync(attachmentId, activityInfo.Id, systemOperator).ConfigureAwait(false);
            }
        }
        //await activityInfo.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);
        //XmlSerializer xser = null;
        //MemoryStream ms = null;
        MessageBroker callback = null;
        if (activityInfo.Schedule is not null)
        {
            ActivityInstanceCalculateRequest activityInfoSchedule = new()
            {
                ActivityId = activityInfo.Id,
                RecurrenceCode = activityInfo.Schedule.RecurrenceCode,
                OptionsEndCode = activityInfo.Schedule.OptionsEndCode,
                DailyCode = activityInfo.Schedule.DailyCode,
                StartDate = activityInfo.Schedule.StartDate,
                EndDate = activityInfo.Schedule.EndDate,
                DurationInSec = activityInfo.Schedule.DurationInSec,
                DailyDays = activityInfo.Schedule.DailyDays,
                NoWeeks = activityInfo.Schedule.NoWeeks,
                OptionsWeekly = activityInfo.Schedule.OptionsWeekly,
                EditSeries = activityInfo.EditSeries,
                Name = activityInfo.Name,
                OneTime = activityInfo.Schedule.OneTime,
                InstanceId = activityInfo.Schedule.InstanceId,
                MonthlyOptionsCode = activityInfo.Schedule.MonthlyOptionsCode,
                MonthlyEvery = activityInfo.Schedule.MonthlyEvery,
                MonthlyDay = activityInfo.Schedule.MonthlyDay,
                MonthlyDayCode = activityInfo.Schedule.MonthlyDayCode,
                MonthlyOrderDaysCode = activityInfo.Schedule.MonthlyOrderDaysCode,
                Occurrences = activityInfo.Schedule.Occurrences,
                MonthlyByYearly = activityInfo.Schedule.MonthlyByYearly,
                YearlyOptionsCode = activityInfo.Schedule.YearlyOptionsCode,
                EveryYear = activityInfo.Schedule.EveryYear,
                EveryHour = activityInfo.Schedule.EveryHour
            };
            if (!string.Equals(activityInfo.Origin, "PRODUCT", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(activityInfo.Origin, "ORDER", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(activityInfo.Origin, "OPERATIONTYPE", StringComparison.OrdinalIgnoreCase))
            {
                // this._dataSyncRepository.ActivityInstanceCalculate(activityInfoSchedule, systemOperator);
                List<ActivityInstanceCalculateResponse> tmpListInstance = _dataSyncRepository.ActivityInstanceCalculate(activityInfoSchedule, systemOperator);
                if (tmpListInstance is not null)
                {
                    returnValue.ListInstaceResponse = [];
                    returnValue.ListInstaceResponse = tmpListInstance;
                }
            }
            else
            {
                callback = _dataSyncRepository.ActivityMergeSchedule(activityInfo, systemOperator);
            }

            //callback = this._dataSyncRepository.ActivityMergeSchedule(activityInfo, systemOperator);
            activityInfo.Schedule.ActivityId = activityInfo.Id;
            //await activityInfo.Schedule.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);
        }

        // if (callback is not null)
        // {
        //     Services.SyncInitializer.ForcePush(callback);
        // } discuss mario
        return returnValue;
    }

    public async Task<bool> DeleteActivity(Activity activityInfo, User systemOperator)
    {
        bool returnValue = _dataSyncRepository.DeleteActivity(activityInfo, systemOperator);
        //await activityInfo.Log(EntityLogType.Delete, systemOperator).ConfigureAwait(false);
        return returnValue;
    }

    #endregion Activity

    #region Attachment
    public async Task<List<AttachmentResponse>> SaveAttachment(List<AttachmentLocal> listAttachmentRequest, User systemOperator)
    {
        List<AttachmentResponse> returnValue = [];

        try
        {
            foreach (AttachmentLocal attachmentRequest in listAttachmentRequest)
            {
                // Insert or update the attachment in the database
                AttachmentResponse tempAdd = await _dataSyncRepository.AttachmentPut(attachmentRequest, systemOperator).ConfigureAwait(false);

                if ((attachmentRequest.TypeCode == "File" || attachmentRequest.TypeCode == "Image") &&
                    !string.IsNullOrEmpty(attachmentRequest.FileBase64))
                {
                    // Determine the attachment path
                    string relativePath = attachmentRequest.IsTemp
                        ? Path.Combine(FullAttachmentPath, "Temp")
                        : FullAttachmentPath;

                    // Ensure the directory exists
                    if (!Directory.Exists(relativePath))
                    {
                        Directory.CreateDirectory(relativePath);
                    }

                    // Write the file
                    string filePath = Path.Combine(relativePath, tempAdd.Id);
                    byte[] imageBytes = Convert.FromBase64String(attachmentRequest.FileBase64);

                    await File.WriteAllBytesAsync(filePath, imageBytes).ConfigureAwait(false);
                }

                // Update response properties
                tempAdd.Name = attachmentRequest.Name;
                tempAdd.Size = attachmentRequest.Size;
                tempAdd.IsTemp = attachmentRequest.IsTemp;
                tempAdd.Extension = attachmentRequest.Extension;
                tempAdd.CreationDate = tempAdd.CreationDate;

                returnValue.Add(tempAdd);
            }
        }
        catch (Exception ex)
        {
            //logger.Error("An error occurred while saving attachments.", ex);check
            throw;
        }

        return returnValue;
    }

    public async Task<bool> AttachmentSync(string AttachmentId, string AuxId, User systemOperator)
    {
        string ResultAttachmentId = await _dataSyncRepository.AttachmentSync(AttachmentId, AuxId, systemOperator).ConfigureAwait(false);

        if (ResultAttachmentId is not null)
        {
            string pathFile = Path.Combine(FullAttachmentPath, AttachmentId);
            string pathTempFolder = Path.Combine(FullAttachmentPath, "Temp");
            string pathTempFile = Path.Combine(FullAttachmentPath, "Temp", AttachmentId);
            if (!Directory.Exists(FullAttachmentPath))
            {
                _ = Directory.CreateDirectory(FullAttachmentPath);
            }
            if (!Directory.Exists(pathTempFolder))
            {
                _ = Directory.CreateDirectory(pathTempFolder);
            }

            if (File.Exists(pathTempFile))
            {
                File.Move(pathTempFile, pathFile);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> SaveImageEntity(string Entity, string FileBase, string AuxId, User systemOperator)
    {
        if (string.IsNullOrEmpty(FileBase) || !FileBase.Contains("data:image"))
        {
            return false;
        }
        List<AttachmentLocal> listAttachmentRequest = [];
        try
        {
            byte[] imageBytes = Convert.FromBase64String(FileBase[(FileBase.LastIndexOf(',') + 1)..]);
            await using MemoryStream ms = new(imageBytes, 0, imageBytes.Length);
            using Image image = await Image.LoadAsync(ms).ConfigureAwait(false);
            AttachmentLocal objAdd = new()
            {
                TypeCode = "Image",
                Name = Entity + " " + AuxId,
                Extension = "jpg",
                FileBase64 = FileBase[(FileBase.LastIndexOf(',') + 1)..],
                AuxId = AuxId,
                Entity = Entity,
                IsTemp = false,
                Size = imageBytes.Length.ToString(),
                Status = 1,
                IsImageEntity = true,
            };

            listAttachmentRequest.Add(objAdd);
            _ = await SaveAttachment(listAttachmentRequest, systemOperator).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here if necessary
            return false;
        }
        return true;
    }
    #endregion Attachment

    #region ProcessMaster
    public async Task<ResponseData> ProcessMasterInsByXML(
        Procedure processMasterinfo,
        User systemOperator,
        bool Validate = false,
        bool NotifyOnce = true)
    {
        string returnValueProcess = "";
        ResponseData returnValue = null;
        //int saveVersionProcess = 0;
        if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate || processMasterinfo.IsProcessNewActivity || processMasterinfo.CreateVersionSync)
        {
            processMasterinfo.ProcedureId = Guid.NewGuid().ToStr();
            if (processMasterinfo.IsProcessNewActivity || processMasterinfo.IsDuplicate || processMasterinfo.CreateVersionSync)
            {
                processMasterinfo.ProcedureIdOrigin = processMasterinfo.ProcedureId;
            }
        }
        else if (processMasterinfo.Version == 1)
        {
            processMasterinfo.ProcedureIdOrigin = processMasterinfo.ProcedureId;
        }

        returnValueProcess = processMasterinfo.ProcedureId;
        returnValue = _dataSyncRepository.ProcessMasterIns(processMasterinfo, systemOperator);
        XmlSerializer xser = null;
        MemoryStream ms = null;
        try
        {
            if (!string.IsNullOrEmpty(processMasterinfo.ProcedureId) && returnValue.IsSuccess && processMasterinfo.Sections?.Count > 0)
            {
                ms = new MemoryStream();
                xser = new XmlSerializer(typeof(List<ProcedureSection>));
                List<ProcedureMasterInstruction> choices = [];
                List<ActionChoice> listActionCheckBox = [];
                List<Choice> listMultipleChoice = [];
                //List<ActionOperator> listActionOperator = new List<ActionOperator>();
                // List<Choice> listMultipleChoiceCheckBox = new List<Choice>();
                List<Range> listRange = [];
                List<ActionChoiceDB> listActionDB = [];
                List<Attachment> attchments = [];
                List<ComponentInstruction> listComponents = [];
                List<ProcessMasterAttachmentDetail> listAttachemtnDetail = [];

                string xmlChoices = string.Empty;
                string xmlRanges = string.Empty;
                string xmlComponents = string.Empty;
                string xmlSections = string.Empty;
                string xmlInstructions = string.Empty;
                string xmlActionCheckBoxs = string.Empty;
                string xmlMultipleChoiceCheckBox = string.Empty;
                string xmlActionOperators = string.Empty;
                string xmlAtachments = string.Empty;
                List<ProcedureMasterInstruction> listInstrucction = [];
                List<ProcedureSection> listTempSections = null;// new List<ProcedureSection>();

                string jsonString = "";

                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                //|| processMasterinfo.IsProcessNewActivity == true)
                {
                    jsonString = JsonConvert.SerializeObject(processMasterinfo.Sections);
                }

                processMasterinfo.Sections.ForEach(section =>
                {
                    if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                    // || processMasterinfo.IsProcessNewActivity == true)
                    {
                        section.SectionId = Guid.NewGuid().ToStr();
                    }
                });

                if ((processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                    //|| processMasterinfo.IsProcessNewActivity == true)
                    && !string.IsNullOrEmpty(jsonString))
                {
                    listTempSections = JsonConvert.DeserializeObject<List<ProcedureSection>>(jsonString);
                }

                foreach (ProcedureSection section in processMasterinfo.Sections)
                {
                    section.ProcedureId = processMasterinfo.ProcedureId;
                    if (section.AttachmentId is not null && !string.IsNullOrEmpty(section.AttachmentId))
                    {
                        await AttachmentSync(section.AttachmentId, section.SectionId, systemOperator).ConfigureAwait(false);
                    }

                    foreach (ProcedureMasterInstruction instruction in section.ListInstrucctions)
                    {
                        instruction.Status = 1;
                        instruction.SectionId = section.SectionId;
                        instruction.ViewType = (int?)instruction.ViewType ?? 0;
                        instruction.TypeDataReading = (int?)instruction.TypeDataReading ?? 0;
                        instruction.Long = (int?)instruction.Long ?? 0;
                        instruction.Type = (int?)instruction.Type ?? 0;
                        instruction.TypeInstrucction = instruction.TypeInstrucction;

                        if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                        {
                            instruction.InstructionId = Guid.NewGuid().ToStr();
                        }
                        listInstrucction.Add(instruction);
                        if (instruction.MultipleChoice?.Count > 0)
                        {
                            foreach (Choice choice in instruction.MultipleChoice)
                            {
                                choice.InstructionId = instruction.InstructionId;
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                // || processMasterinfo.IsProcessNewActivity == true)
                                {
                                    choice.OldId = choice.Id;
                                    choice.Id = Guid.NewGuid().ToStr();
                                }
                                if (processMasterinfo.IsDuplicate || processMasterinfo.CreateNewVersion)
                                //processMasterinfo.IsProcessNewActivity == true
                                {
                                    ProcedureSection SectionTemp = listTempSections.Find(p => p.SectionId == choice.SectionId);
                                    if (SectionTemp is not null)
                                    {
                                        choice.SectionId = processMasterinfo.Sections.Find(p => p.OrderSection == SectionTemp.OrderSection).SectionId;
                                    }
                                    else if (SectionTemp is null
                                     && choice.SectionId != "Finish"
                                     && choice.SectionId != "FinishError"
                                     && choice.SectionId != "Block")
                                    {
                                        choice.SectionId = "NoAction";
                                    }
                                }
                                listMultipleChoice.Add(choice);

                                if (choice.AttachmentId is not null && !string.IsNullOrEmpty(choice.AttachmentId))
                                {
                                    await AttachmentSync(choice.AttachmentId, choice.Id, systemOperator).ConfigureAwait(false);
                                }
                            }
                            instruction.MultipleChoice = null;
                        }
                        if (instruction.Range?.Count > 0)
                        {
                            instruction.Range.ForEach(range =>
                            {
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                {
                                    range.Id = Guid.NewGuid().ToStr();
                                }
                                range.InstructionId = instruction.InstructionId;
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                {
                                    ProcedureSection SectionTemp = listTempSections.Find(p => p.SectionId == range.SectionId);
                                    if (SectionTemp is not null)
                                    {
                                        range.SectionId = processMasterinfo.Sections.Find(p => p.OrderSection == SectionTemp.OrderSection).SectionId;
                                    }
                                    else if (SectionTemp is null
                                        && range.SectionId != "Finish"
                                        && range.SectionId != "FinishError"
                                        && range.SectionId != "Block")
                                    {
                                        range.SectionId = "NoAction";
                                    }
                                }
                                listRange.Add(range);
                            });
                            instruction.Range = null;
                        }
                        if (instruction.Components?.Count > 0)
                        {
                            foreach (ComponentInstruction component in instruction.Components)
                            {
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                {
                                    component.Id = Guid.NewGuid().ToStr();
                                }
                                component.InstructionId = instruction.InstructionId;
                                listComponents.Add(component);
                                if (component.AttachmentId is not null
                                   && !string.IsNullOrEmpty(component.AttachmentId))
                                {
                                    await AttachmentSync(component.AttachmentId, component.Id, systemOperator).ConfigureAwait(false);
                                }
                            }
                            instruction.Components = null;
                        }

                        if (instruction.ActionCheckBox?.Count > 0)
                        {
                            instruction.ActionCheckBox.ForEach(Action =>
                            {
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                {
                                    Action.Id = Guid.NewGuid().ToStr();
                                    ProcedureSection SectionTemp = listTempSections.Find(p => p.SectionId == Action.SectionId);
                                    if (SectionTemp is not null)
                                    {
                                        Action.SectionId = processMasterinfo.Sections.Find(p => p.OrderSection == SectionTemp.OrderSection).SectionId;
                                    }
                                    else if (SectionTemp is null
                                        && Action.SectionId != "Finish"
                                        && Action.SectionId != "FinishError"
                                        && Action.SectionId != "Block")
                                    {
                                        Action.SectionId = "NoAction";
                                    }
                                    List<string> ListNewValueAction = [];
                                    Action.ValueChoice.ForEach(value =>
                                    {
                                        Choice addObj = listMultipleChoice.Find(p => p.OldId == value);
                                        if (addObj is not null)
                                        {
                                            ListNewValueAction.Add(addObj.Id);
                                        }
                                    });
                                    Action.ValueChoice = ListNewValueAction;
                                }
                                ActionChoiceDB objAdd = new()
                                {
                                    Id = Action.Id,
                                    InstructionId = instruction.InstructionId,
                                    SectionId = Action.SectionId,
                                    Message = Action.Message,
                                    OrderChoice = Action.OrderChoice,
                                    IsNotify = Action.IsNotify,
                                    MessageNotify = Action.MessageNotify,
                                    ValueChoice = string.Join(',', Action.ValueChoice.Select(x => x.ToString()).ToArray())
                                };
                                listActionDB.Add(objAdd);
                            });
                            instruction.ActionCheckBox = null;
                        }
                    }
                    section.ListInstrucctions = null;
                }

                xser.Serialize(ms, processMasterinfo.Sections);
                xmlSections = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");

                if (listInstrucction.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<ProcedureMasterInstruction>));
                    xser.Serialize(ms, listInstrucction);
                    xmlInstructions = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listMultipleChoice.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<Choice>));
                    xser.Serialize(ms, listMultipleChoice);
                    xmlChoices = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listActionDB.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<ActionChoiceDB>));
                    xser.Serialize(ms, listActionDB);
                    xmlActionCheckBoxs = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listRange.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<Range>));
                    xser.Serialize(ms, listRange);
                    xmlRanges = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listComponents.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<ComponentInstruction>));
                    xser.Serialize(ms, listComponents);
                    xmlComponents = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listAttachemtnDetail.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<ProcessMasterAttachmentDetail>));
                    xser.Serialize(ms, listAttachemtnDetail);
                    xmlAtachments = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                returnValue = _dataSyncRepository.ProcessMasterInsByXML(processMasterinfo
                    , xmlSections
                    , xmlInstructions
                    , xmlChoices
                    , xmlRanges
                    , xmlActionCheckBoxs
                    , systemOperator
                    , xmlComponents
                    , xmlAtachments
                    , Validate);
                if (returnValue.IsSuccess)
                {
                    Procedure procedurelog = _dataSyncRepository.GetProcedure(processMasterinfo.ProcedureId, null);
                    // await procedurelog.Log(procedurelog.CreateNewVersion ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                    // _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged
                    //  , new
                    //  {
                    //      Catalog = Entities.Procedure,
                    //      Action = (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                    //  ? EntityLogType.Create : EntityLogType.Update
                    //  ,
                    //      Data = procedurelog
                    //  });
                }
            }
        }
        catch (Exception ex)
        {
            //logger.Error(ex);
            throw;
        }
        return returnValue;
    }
    #endregion ProcessMaster

    #region Employee
    public async Task<List<ResponseData>> ImportEmployeesAsync(List<EmployeeExternal> requestValue, List<EmployeeExternal> originalValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true, bool isDataSync = false)
    {
        List<ResponseData> returnValue = [];
        Employee employee = null;
        Employee employeeHistory = null;
        EmployeeContractsDetail historyContract = null;
        List<Employee> employeeList = [];
        List<CatProfile> lstProfiles = _dataSyncRepository.GetCatalogProfile();
        List<CatSkills> lstCatSkills = _dataSyncRepository.GetCatSkillsList();
        CatProfile catProfile = null;
        CatSkills catSkill = null;
        EmployeeSkills employeeSkill = null;
        EmployeeContractsDetail employeeContract = null;
        foreach (EmployeeExternal cycleItem in requestValue)
        {
            EmployeeExternal item = cycleItem;

            employeeHistory = GetEmployee(cycleItem.EmployeeCode);
            bool editMode = employeeHistory is not null;
            if (editMode && originalValue is not null)
            {
                item = originalValue.Find(x => x.EmployeeCode == cycleItem.EmployeeCode);
                item ??= cycleItem;
            }
            if (!editMode)
            {
                employee = new Employee
                {
                    Code = item.EmployeeCode,
                    Name = !string.IsNullOrEmpty(item.EmployeeName) ? item.EmployeeName : item.EmployeeCode,
                    LastName = item.EmployeeLastName,
                    Email = item.Email,
                    ExternalId = item.ExternalId
                };
            }
            else
            {
                employee = employeeHistory;
                if (!string.IsNullOrEmpty(item.EmployeeName))
                {
                    employee.Name = item.EmployeeName;
                }
                if (!string.IsNullOrEmpty(item.EmployeeLastName))
                {
                    employee.LastName = item.EmployeeLastName;
                }
                if (!string.IsNullOrEmpty(item.Email))
                {
                    employee.Email = item.Email;
                }
                if (!string.IsNullOrEmpty(item.ExternalId))
                {
                    employee.ExternalId = item.ExternalId;
                }
            }

            employee.Status = (Status)(string.IsNullOrEmpty(item.Status) ? 1 : item.Status == "Disable" ? 2 : 1);

            if (employee.Status != Status.Active && !editMode)
            {
                throw new Exception("Cannot import a disabled employee record");
            }
            employee.CostPerHour = item.CostPerHour;

            if (employeeHistory is null)
            {
                //Validar de que forma se cargará el pass cuando sea desde import y sea un registro nuevo
                employee.Password = Helper.Security.getHash(employee.Code.ToUpperInvariant() + ":" + Helper.Security.getHash(employee.Name));
                //Front end have this code to create a hash
                //$scope.employees.Current.Username.toUpperCase() + ":" + MD5($scope.employees.Current.Password).toUpperCase()
            }

            if (!editMode || !string.IsNullOrEmpty(item.ProfileCode))
            {
                catProfile = lstProfiles?.Where(q => string.Equals(q.Code, item.ProfileCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(x => x.Status != Status.Failed);
                //Valid value Profile to Catalog. If the value null, set default profile value.
                if (catProfile is not null)
                {
                    employeeContract = new EmployeeContractsDetail
                    {
                        ProfileId = catProfile.ProfileId,
                        DateStart = DateTime.Now.Date
                    };
                    if (employeeHistory is null || (employeeHistory.CurrentPositionId == employee.CurrentPositionId && (employee.EmployeeSkills is null || employee.EmployeeSkills.Count == 0)))
                    {
                        employee.EmployeeSkills = [];
                        foreach (string val in catProfile.Skills.Split(','))
                        {
                            if (val.Length > 0)
                            {
                                catSkill = lstCatSkills?.FirstOrDefault(q => q.SkillId == val);
                                employeeSkill = new EmployeeSkills
                                {
                                    SkillId = catSkill.SkillId
                                };
                                employee.EmployeeSkills.Add(employeeSkill);
                            }
                        }
                        employee.EmployeeContractsDetail = [employeeContract];
                    }
                    else
                    {
                        employeeHistory.EmployeeSkills = _dataSyncRepository.EmployeeSkillsList(employeeHistory.Id);
                        employeeHistory.EmployeeContractsDetail = _dataSyncRepository.ContractsDetailList(employeeHistory.Id);
                        historyContract = employeeHistory.EmployeeContractsDetail.OrderByDescending(qq => qq.DateEnd).FirstOrDefault();
                        if (historyContract is not null && historyContract.ProfileId != employeeContract.ProfileId)
                        {
                            employee.EmployeeSkills = [];
                            foreach (string val in catProfile.Skills.Split(','))
                            {
                                if (val.Length > 0)
                                {
                                    catSkill = lstCatSkills?.FirstOrDefault(q => q.SkillId == val);
                                    employeeSkill = new EmployeeSkills
                                    {
                                        SkillId = catSkill.SkillId
                                    };
                                    employee.EmployeeSkills.Add(employeeSkill);
                                }
                            }

                            employeeHistory.EmployeeContractsDetail = [.. employeeHistory.EmployeeContractsDetail.Where(qq => qq.ProfileId != historyContract.ProfileId && qq.DateEnd != historyContract.DateEnd)];
                            historyContract.DateEnd = DateTime.Now.Date.AddDays(-1);
                            employeeHistory.EmployeeContractsDetail.Add(historyContract);
                            employeeHistory.EmployeeContractsDetail.Add(employeeContract);
                            employee.EmployeeContractsDetail = employeeHistory.EmployeeContractsDetail;
                        }
                        else
                        {
                            employee.EmployeeSkills = employeeHistory.EmployeeSkills;
                            employee.EmployeeContractsDetail = employeeHistory.EmployeeContractsDetail;
                        }
                    }
                }
                else
                {
                    throw new Exception("Profile code not found '" + item.ProfileCode + "'");
                }
            }
            employeeList.Add(employee);
        }
        returnValue.AddRange(await MRGEmployee(employeeList, systemOperator, Validate, Level, NotifyOnce, isDataSync).ConfigureAwait(false));
        return returnValue;
    }
    public async Task<List<ResponseData>> MRGEmployee(List<Employee> requestValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true, bool isDataSync = false)
    {
        List<ResponseData> returnValue = [];
        ResponseData responseMessage;
        ResponseData responseError;
        //List<Employee> users = new List<User>();
        ResponseModel result;
        List<EmployeeSkills> responseEmployeeSkills = [];
        List<EmployeeContractsDetail> responseEmployeeContractsDetail = [];
        Employee employeeLog = null;
        int Line = 0;
        // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.HR_EMPLOYEE_MANAGE))
        // {
        // 	throw new UnauthorizedAccessException(noPermission);
        // }
        NotifyOnce = requestValue.Count == 1;
        foreach (Employee item in requestValue)
        {
            try
            {
                item.UserId = systemOperator.Id;
                //Coversión de datos para crear las listas
                if (!string.IsNullOrEmpty(item.AssetsId))
                {
                    item.EmployeeAssets =
                    [
                        .. item.AssetsId.Split(',').Select(q => new AssetsTree
                            {
                                AssetCode = q.Substring(q.IndexOf('-') + 1, q.Length - q[..q.IndexOf('-')].Length - 1),
                                AssetTypeCode = q[..q[..q.IndexOf('-')].Length]
                            }),
                        ];
                }

                responseMessage = _dataSyncRepository.MRGEmployee(item, Validate, systemOperator);
                returnValue.Add(responseMessage);
                if (!responseMessage.IsSuccess)
                {
                    continue;
                }
                else if (responseMessage.Id == "@CEOPositionHeldBy")
                {
                    responseMessage.Entity = new Employee { Id = "@CEOPositionHeldBy" };
                    continue;
                }
                _ = await SaveImageEntity("Employee", item.Image, item.Code, systemOperator).ConfigureAwait(false);
                if (item.AttachmentIds is not null)
                {
                    foreach (string attachment in item.AttachmentIds)
                    {
                        await AttachmentSync(attachment, item.Code, systemOperator).ConfigureAwait(false);
                    }
                }
                if (item.Activities?.Count > 0)
                {
                    foreach (Activity activity in item.Activities)
                    {
                        if (string.IsNullOrEmpty(activity.Id))
                        {
                            Activity newActivity = await CreateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ManualDelete)
                        {
                            bool tempResult = await DeleteActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else
                        {
                            if (activity.ActivityClassId > 0)
                            {
                                _ = await UpdateActivity(activity, systemOperator).ConfigureAwait(false);
                            }
                        }
                    }
                }
                if (item.Shift?.CodeShift is not null
                                  && !string.IsNullOrEmpty(item.Shift.CodeShift))
                {
                    item.Shift.Validation = false;
                    item.Shift.IdAsset = item.Code;
                    _ = UpdateSchedulingCalendarShifts(item.Shift, systemOperator);
                }
                if (item.ShiftDelete?.Id is not null
                && !string.IsNullOrEmpty(item.ShiftDelete.Id))
                {
                    item.ShiftDelete.Validation = false;
                    _ = DeleteSchedulingCalendarShifts(item.ShiftDelete, systemOperator);
                }
                item.Id = responseMessage.Id;
                if (returnValue is not null && !Validate)
                {
                    //Verificamos si se guardaran skills
                    if (item.EmployeeSkills?.Count > 0)
                    {
                        XmlSerializer employeeSkills = new(typeof(List<EmployeeSkills>));
                        MemoryStream ms = new();

                        employeeSkills.Serialize(ms, item.EmployeeSkills);
                        string xmlDowntimesTypes = System.Text.Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");

                        responseEmployeeSkills = _dataSyncRepository.CreateEmployeeSkills(item.Id, xmlDowntimesTypes, systemOperator);
                        responseEmployeeSkills.ForEach(x => x.AttachmentId = item.EmployeeSkills.First(q => q.SkillId == x.SkillId).AttachmentId);
                        //Validar el guardado de Attachment

                        foreach (EmployeeSkills itemSkill in responseEmployeeSkills)
                        {
                            if (!string.IsNullOrEmpty(itemSkill.AttachmentId))
                            {
                                await AttachmentSync(itemSkill.AttachmentId, itemSkill.Employee_Skills_Id, systemOperator).ConfigureAwait(false);
                            }
                        }
                    }

                    //Verificamos si se guardaran contratos
                    if (item.EmployeeContractsDetail?.Count > 0)
                    {
                        XmlSerializer employeesContractsDetail = new(typeof(List<EmployeeContractsDetail>));
                        MemoryStream ms = new();

                        employeesContractsDetail.Serialize(ms, item.EmployeeContractsDetail);
                        string xmlDowntimesTypes = System.Text.Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");

                        responseEmployeeContractsDetail = _dataSyncRepository.CreateEmployeeContractsDetail(item.Id, xmlDowntimesTypes, systemOperator);
                        if (!isDataSync)
                        {
                            GeneratePositionShiftExplosion(item.Code);
                        }
                    }
                }

                //Valid insert detalle tables
                //if ()
                //    continue;
                if (!Validate)
                {
                    //employeeLog = _dataSyncRepository.ListEmployees(null, item.Code).Find(x => x.Status != Status.Failed);
                    //await employeeLog.Log(responseMessage.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                }

                // if (!Validate)
                // {
                //     try
                //     {
                //         if (responseMessage.Action == ActionDB.Create)
                //         {
                //             result = await CheckUserTraining(employeeLog).ConfigureAwait(false);
                //             if (result.IsSuccess && result.Data is null)
                //             {
                //                 result = await CreateUserTraining(employeeLog).ConfigureAwait(false);
                //             }
                //         }
                //         else
                //         {
                //             result = await CheckUserTraining2(employeeLog.Email, employeeLog.Code).ConfigureAwait(false);
                //             if (result.IsSuccess && result.Data is null)
                //             {
                //                 result = await CreateUserTraining2(employeeLog.Email, employeeLog.Code, employeeLog.Name, employeeLog.LastName).ConfigureAwait(false);
                //             }
                //         }
                //     }
                //     catch (Exception ex)
                //     {
                //         responseError = new ResponseData
                //         {
                //             Message = ex.Message,
                //             Code = "Warning Line:" + Line.ToStr()
                //         };
                //         returnValue.Add(responseError);
                //     }
                // }
                // if (NotifyOnce && !Validate)
                // {
                // 	responseMessage.Entity = employeeLog;
                // 	_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Employees, Data = employeeLog, responseMessage.Action }, responseMessage.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                // }
            }
            catch (Exception ex)
            {
                responseError = new ResponseData
                {
                    Message = ex.Message,
                    Code = "Line:" + Line.ToStr()
                };
                returnValue.Add(responseError);
            }
        }
        // if (!NotifyOnce && !Validate)
        // {
        // 	_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Employees, Data = new object { }, Action = ActionDB.IntegrateAll });
        // }
        switch (Level)
        {
            case LevelMessage.Warning:
                returnValue = [.. returnValue.Where(p => !string.IsNullOrEmpty(p.Message))];
                break;

            case LevelMessage.Error:
                returnValue = [.. returnValue.Where(p => !p.IsSuccess)];
                break;
            case LevelMessage.Success:
                break;
        }

        return returnValue;
    }
    public void GeneratePositionShiftExplosion(string employeeCode)
	{
		_dataSyncRepository.GeneratePositionShiftExplosion(employeeCode);
	}
    #endregion Employee

    #endregion DataSync
}
