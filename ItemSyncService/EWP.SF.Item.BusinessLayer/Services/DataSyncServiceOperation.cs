
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EWP.SF.Item.BusinessLayer;

public class DataSyncServiceOperation : IDataSyncServiceOperation
{
	private readonly IDataSyncRepository _dataSyncRepository;

	public DataSyncServiceOperation(IDataSyncRepository dataSyncRepository)
	{
		_dataSyncRepository = dataSyncRepository;
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
	public static async Task<DataSyncResponse> TestErpConnection(User systemOperator, DataSyncErp erpData)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.DATA_SYNC_MANAGER))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		using APIWebClient client = new()
		{
			TimeoutSeconds = 10
		};
		return await client.DataSyncDownload($"{erpData.BaseUrl}").ConfigureAwait(false);
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

	public bool InsertDataSyncServiceLogDetailBulk(string logInfo)
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
		if (!Validate)
		{
			if (!NotifyOnce)
			{
				//_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Item, Action = ActionDB.IntegrateAll.ToStr() });Need Discuss
			}
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
		// if (!Validate)
		// {
		// 	if (!NotifyOnce)
		// 	{
		// 		_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Warehouse, Action = ActionDB.IntegrateAll.ToStr() });
		// 	}
		// 	switch (Level)
		// 	{
		// 		case LevelMessage.Warning:
		// 			returntValue = [.. returntValue.Where(p => !string.IsNullOrEmpty(p.Message))];
		// 			break;

		// 		case LevelMessage.Error:
		// 			returntValue = [.. returntValue.Where(p => !p.IsSuccess)];
		// 			break;
		// 		case LevelMessage.Success:
		// 			break;
		// 	}
		// }  discuss mario
		return returntValue;
	}

	public Task<User> GetUserWithoutValidations(User user)
	{
		return _dataSyncRepository.GetUser(user.Id, null, new User(0));
	}

    public Task<List<ResponseData>> CreateAssetsExternal(List<AssetExternal> AssetsList, List<AssetExternal> AssetListOriginal, User user, bool Validate, string Level)
    {
        throw new NotImplementedException();
    }

    public Task<List<ResponseData>> CreateUpdateProductionLine(List<AssetExternal> productionLineList, List<AssetExternal> productionLineListOriginal, User user, bool Validate, string Level)
    {
        throw new NotImplementedException();
    }

    public Task<List<ResponseData>> CreateUpdateFloor(List<AssetExternal> floorList, List<AssetExternal> floorListOriginal, User user, bool Validate, string Level)
    {
        throw new NotImplementedException();
    }

    public Task<List<ResponseData>> CreateUpdateWorkCenter(List<AssetExternal> workCenterList, List<AssetExternal> workCenterListOriginal, User user, bool Validate, string Level)
    {
        throw new NotImplementedException();
    }

    public Task<List<ResponseData>> ListUpdateInventoryGroup(List<InventoryExternal> inventoryGroupList, List<InventoryExternal> inventoryGroupListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        throw new NotImplementedException();
    }

    public Task<List<ResponseData>> ListUpdateBinLocation(List<BinLocationExternal> binLocationList, List<BinLocationExternal> binLocationListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        throw new NotImplementedException();
    }

    #endregion Warehouse
    /*
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

        /// <summary>
        /// Obtiene la informacion de KPI de assets y permite filtrar por jerarquia y código permitiendo conocer la relación
        /// que existen entre los diferentes assets.
        /// </summary>
        /// <param name="base_assetType">Tipo de asset por el cual comenzará la busqueda (Company, Facility, Floor, Workcenter, ProductionLine, Machine)</param>
        /// <param name="base_assetCode">Código del asset a filtrar</param>
        /// <param name="target_assetType">Tipo de asset por el cual terminará la busqueda (Company, Facility, Floor, Workcenter, ProductionLine, Machine)</param>
        /// <returns>Lisa de KPI de los assets obtenidos con sus jerarquias</returns>
        public List<AssetKpi> GetAssetKpi(string base_assetType = null, string base_assetCode = null, string target_assetType = null)
        {
            return _dataSyncRepository.ListAssetKpi(base_assetType, base_assetCode, target_assetType);
        }

        /// <summary>
        /// Obtiene la informacion de Tags del asset y tags de machine relacionadas
        /// </summary>
        /// <param name="base_assetType">Tipo de asset por el cual comenzará la busqueda (Company, Facility, Floor, Workcenter, ProductionLine, Machine)</param>
        /// <param name="base_assetCode">Código del asset a filtrar</param>
        /// <returns>Lisa de tags del asset y tags de machines relacionadas</returns>
        public List<AssetKpi> GetTagsByAsset(string base_assetType = null, string base_assetCode = null)
        {
            return _dataSyncRepository.TagsByAsset(base_assetType, base_assetCode);
        }

        /// <summary>
        /// Obtiene layout del asset y tags de machine relacionadas
        /// </summary>
        /// <param name="base_assetType">Tipo de asset por el cual comenzará la busqueda (Company, Facility, Floor, Workcenter, ProductionLine, Machine)</param>
        /// <param name="base_assetCode">Código del asset a filtrar</param>
        /// <returns>Lisa de tags del asset y tags de machines relacionadas</returns>
        public List<ConfigurationAssetLiveDashBoard> GetAssetLayout(string base_assetType = null, string base_assetCode = null)
        {
            return _dataSyncRepository.AssetLayout(base_assetType, base_assetCode);
        }
        #endregion Asset

        #region Production Lines

        public ProductionLine[] ListProductionLines(bool deleted = false, DateTime? DeltaDate = null)
        {
            List<ProductionLine> lines = _dataSyncRepository.ListProductionLines(DeltaDate);

            return lines?.Where(d => (deleted && d.Status == Status.Deleted) || (!deleted && d.Status != Status.Deleted)).ToArray();
        }

        public ProductionLine GetProductionLine(string Code, User systemOperator)
        {
            ProductionLine returnValue = null;

            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_PRODUCTIONLINE_MANAGE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            return _dataSyncRepository.GetProductionLine(Code);
        }

        public async Task<ResponseData> CreateProductionLine(ProductionLine productionLineInfo, User systemOperator
          , bool Validate = false, string Level = "Success", bool NotifyOnce = true)
        {
            ResponseData returnValue = new();

            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_PRODUCTIONLINE_CREATE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            returnValue = _dataSyncRepository.CreateProductionLine(productionLineInfo, systemOperator, Validate, Level);
            if (!Validate)
            {
                ProductionLine ObjProductionLine = _dataSyncRepository.GetProductionLine(productionLineInfo.Code);
                await ObjProductionLine.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                //_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductionLine, returnValue.Action, Data = ObjProductionLine });
                if (NotifyOnce)
                {
                    _ = await SaveImageEntity("ProductionLine", productionLineInfo.Image, productionLineInfo.Code, systemOperator).ConfigureAwait(false);
                    if (productionLineInfo.AttachmentIds is not null)
                    {
                        foreach (string attachment in productionLineInfo.AttachmentIds)
                        {
                            await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                        }
                    }

                    if (productionLineInfo.Activities?.Count > 0)
                    {
                        foreach (Activity activity in productionLineInfo.Activities)
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
                    if (productionLineInfo.Shift?.CodeShift is not null
                    && !string.IsNullOrEmpty(productionLineInfo.Shift.CodeShift))
                    {
                        productionLineInfo.Shift.Validation = false;
                        productionLineInfo.Shift.IdAsset = productionLineInfo.Code;
                        _ = UpdateSchedulingCalendarShifts(productionLineInfo.Shift, systemOperator);
                    }
                    if (productionLineInfo.ShiftDelete?.Id is not null
                         && !string.IsNullOrEmpty(productionLineInfo.ShiftDelete.Id))
                    {
                        productionLineInfo.ShiftDelete.Validation = false;
                        _ = DeleteSchedulingCalendarShifts(productionLineInfo.ShiftDelete, systemOperator);
                    }

                    // Services.ServiceManager.sendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductionLine, Action = returnValue.Action, Data = ObjProductionLine }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                }
            }

            return returnValue;
        }

        public bool DeleteProductionLine(ProductionLine productionLineInfo, User systemOperator)
        {
            bool returnValue = false;

            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_PRODUCTIONLINE_DELETE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            return _dataSyncRepository.DeleteProductionLine(productionLineInfo, systemOperator);
        }

        #endregion Production Lines

        #region WorkCenter

        public List<WorkCenter> ListWorkCenter(DateTime? DeltaDate = null)
        {
            return _dataSyncRepository.ListWorkCenter(DeltaDate);
        }

        public WorkCenter GetWorkCenter(string WorkCenterCode)
        {
            return _dataSyncRepository.GetWorkCenter(WorkCenterCode);
        }

        public async Task<ResponseData> CreateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator, bool Validate = false, string Level = "Success", bool NotifyOnce = true)
        {
            ResponseData returnValue = new();
            WorkCenter ObjWorkCenter;

            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_WORKCENTER_MANAGE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            returnValue = _dataSyncRepository.CreateWorkCenter(WorkCenterInfo, systemOperator, Validate, Level);
            if (!Validate)
            {
                ObjWorkCenter = _dataSyncRepository.GetWorkCenter(WorkCenterInfo.Code);
                if (NotifyOnce)
                {
                    _ = await SaveImageEntity("Workcenter", WorkCenterInfo.Image, WorkCenterInfo.Code, systemOperator).ConfigureAwait(false);
                    if (WorkCenterInfo.AttachmentIds is not null)
                    {
                        foreach (string attachment in WorkCenterInfo.AttachmentIds)
                        {
                            await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                        }
                    }
                    if (WorkCenterInfo.Activities?.Count > 0)
                    {
                        foreach (Activity activity in WorkCenterInfo.Activities)
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
                    if (WorkCenterInfo.Shift?.CodeShift is not null
                                     && !string.IsNullOrEmpty(WorkCenterInfo.Shift.CodeShift))
                    {
                        WorkCenterInfo.Shift.Validation = false;
                        WorkCenterInfo.Shift.IdAsset = WorkCenterInfo.Code;
                        _ = UpdateSchedulingCalendarShifts(WorkCenterInfo.Shift, systemOperator);
                    }
                    if (WorkCenterInfo.ShiftDelete?.Id is not null
                                                             && !string.IsNullOrEmpty(WorkCenterInfo.ShiftDelete.Id))
                    {
                        WorkCenterInfo.ShiftDelete.Validation = false;
                        _ = DeleteSchedulingCalendarShifts(WorkCenterInfo.ShiftDelete, systemOperator);
                    }
                    //_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Workcenter, returnValue.Action, Data = ObjWorkCenter }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                }
                await ObjWorkCenter.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            }
            return returnValue;
        }

        public bool UpdateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator)
        {
            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_WORKCENTER_MANAGE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            return _dataSyncRepository.UpdateWorkCenter(WorkCenterInfo, systemOperator);
        }

        public bool DeleteWorkCenter(WorkCenter WorkCenterInfo, User systemOperator)
        {
            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_WORKCENTER_MANAGE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            return _dataSyncRepository.DeleteWorkCenter(WorkCenterInfo, systemOperator);
        }

        #endregion WorkCenter

        #region Floor

        public List<Floor> ListFloor(DateTime? DeltaDate = null)
        {
            return _dataSyncRepository.ListFloor(DeltaDate);
        }

        public Floor GetFloor(string FloorCode)
        {
            return _dataSyncRepository.GetFloor(FloorCode);
        }

        public async Task<ResponseData> CreateFloor(Floor FloorInfo, User systemOperator
            , bool Validate = false, string Level = "Success", bool NotifyOnce = true)
        {
            ResponseData returnValue = new();

            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_FLOOR_MANAGE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            returnValue = _dataSyncRepository.CreateFloor(FloorInfo, systemOperator, Validate, Level);
            if (!Validate)
            {
                Floor ObjFloor = _dataSyncRepository.GetFloor(FloorInfo.Code);
                await ObjFloor.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                if (NotifyOnce)
                {
                    _ = await SaveImageEntity("Floor", FloorInfo.Image, FloorInfo.Code, systemOperator).ConfigureAwait(false);
                    if (FloorInfo.AttachmentIds is not null)
                    {
                        foreach (string attachment in FloorInfo.AttachmentIds)
                        {
                            await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                        }
                    }
                    if (FloorInfo.Activities?.Count > 0)
                    {
                        foreach (Activity activity in FloorInfo.Activities)
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
                    if (FloorInfo.Shift?.CodeShift is not null
                                        && !string.IsNullOrEmpty(FloorInfo.Shift.CodeShift))
                    {
                        FloorInfo.Shift.Validation = false;
                        FloorInfo.Shift.IdAsset = FloorInfo.Code;
                        _ = UpdateSchedulingCalendarShifts(FloorInfo.Shift, systemOperator);
                    }
                    if (FloorInfo.ShiftDelete?.Id is not null
                      && !string.IsNullOrEmpty(FloorInfo.ShiftDelete.Id))
                    {
                        FloorInfo.ShiftDelete.Validation = false;
                        _ = DeleteSchedulingCalendarShifts(FloorInfo.ShiftDelete, systemOperator);
                    }

                    //_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Floor, returnValue.Action, Data = ObjFloor }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                }
            }
            return returnValue;
        }

        public bool DeleteFloor(Floor FloorInfo, User systemOperator)
        {
            bool returnValue = false;

            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_FLOOR_MANAGE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            returnValue = _dataSyncRepository.DeleteFloor(FloorInfo, systemOperator);
            // if (returnValue)
            // {
            // 	_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Floor, Action = ActionDB.Delete, Data = FloorInfo }, systemOperator.TimeZoneOffset);
            // }
            return returnValue;
        }

        #endregion Floor

        #region Facility

        public List<FacilityMapKpi> ListFacilityMapKpi()
        {
            return _dataSyncRepository.ListFacilityMapKpi();
        }

        public List<Facility> ListFacility(DateTime? deltaDate = null)
        {
            return _dataSyncRepository.ListFacility(deltaDate);
        }

        public Facility GetFacility(string Code)
        {
            return _dataSyncRepository.GetFacility(Code);
        }

        public async Task<ResponseData> CreateFacility(Facility FacilityInfo, User systemOperator
            , bool Validate = false
            , string Level = "Success"
            , bool NotifyOnce = true
            )
        {
            ResponseData returnValue = new();

            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_FACILITY_MANAGE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            returnValue = _dataSyncRepository.CreateFacility(FacilityInfo, systemOperator, Validate, Level);

            if (!Validate)
            {
                Facility ObjFacility = GetFacility(returnValue.Code);
                await ObjFacility.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                if (NotifyOnce)
                {
                    _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Facility, returnValue.Action, Data = ObjFacility }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                    _ = await SaveImageEntity("Facility", FacilityInfo.Image, FacilityInfo.Code, systemOperator).ConfigureAwait(false);
                    if (FacilityInfo.AttachmentIds is not null)
                    {
                        foreach (string attachment in FacilityInfo.AttachmentIds)
                        {
                            await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                        }
                    }

                    if (FacilityInfo.Activities?.Count > 0)
                    {
                        foreach (Activity activity in FacilityInfo.Activities)
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
                    if (FacilityInfo.Shift?.CodeShift is not null
                        && !string.IsNullOrEmpty(FacilityInfo.Shift.CodeShift))
                    {
                        FacilityInfo.Shift.Validation = false;
                        FacilityInfo.Shift.IdAsset = FacilityInfo.Code;
                        _ = UpdateSchedulingCalendarShifts(FacilityInfo.Shift, systemOperator);
                    }
                    if (FacilityInfo.ShiftDelete?.Id is not null
                      && !string.IsNullOrEmpty(FacilityInfo.ShiftDelete.Id))
                    {
                        FacilityInfo.ShiftDelete.Validation = false;
                        _ = DeleteSchedulingCalendarShifts(FacilityInfo.ShiftDelete, systemOperator);
                    }
                }
            }

            return returnValue;
        }

        public bool DeleteFacility(Facility FacilityInfo, User systemOperator)
        {
            bool returnValue = false;

            #region Permission validation

            // if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_FACILITY_MANAGE))
            // {
            // 	throw new UnauthorizedAccessException(noPermission);
            // }

            #endregion Permission validation

            returnValue = _dataSyncRepository.DeleteFacility(FacilityInfo, systemOperator);
            // if (returnValue)
            // {
            // 	_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Facility, Action = ActionDB.Delete, Data = FacilityInfo }, systemOperator.TimeZoneOffset);
            // }
            return returnValue;
        }

        public Facility GetFacilityByCode(string Code, string Entity)
        {
            return _dataSyncRepository.GetFacilityByCode(Code, Entity);
        }

        #endregion Facility

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
                await ObjInventory.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
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
                    _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Inventory, returnValue.Action, Data = ObjInventory }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
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
                await ObjBinLocation.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
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
        */
    #endregion DataSync
}
