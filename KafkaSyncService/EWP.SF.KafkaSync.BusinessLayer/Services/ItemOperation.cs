using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using EWP.SF.KafkaSync.DataAccess;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class ItemOperation : IItemOperation
{
    private readonly IItemRepo _itemRepo;
    private readonly IInventoryOperation _inventoryOperation;

    public ItemOperation(IItemRepo itemRepo, IInventoryOperation inventoryOperation)
    {
        _itemRepo = itemRepo;
        _inventoryOperation = inventoryOperation;
    }

public async Task<List<ResponseData>> ListUpdateComponentBulk(List<ComponentExternal> itemList, List<ComponentExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        List<ResponseData> returnValue = [];
        ResponseData MessageError;
        List<MeasureUnit> unitsList = GetMeasureUnits();
        List<Inventory> inventories = _inventoryOperation.ListInventory(systemOperator, null);
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
            ResponseData result = _itemRepo.MergeComponentBulk(itemsJson, systemOperator, Validate);

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
    public List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null)
    {
        return _itemRepo.GetMeasureUnits(unitType, unitId, DeltaDate);
    }
    public async Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "")
    {
        List<Component> returnValue;
        if (!string.IsNullOrEmpty(filter))
        {
            returnValue = await _itemRepo.ListComponents(componentId, true, filter).ConfigureAwait(false);
        }
        else if (!string.IsNullOrEmpty(componentId))
        {
            returnValue = await _itemRepo.ListComponents(componentId, false, string.Empty).ConfigureAwait(false);
        }
        else
        {
            returnValue = await _itemRepo.ListComponents(componentId, true, filter).ConfigureAwait(false);
        }
        return returnValue?.ToArray();
    }

}