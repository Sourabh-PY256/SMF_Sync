
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

	#endregion Warehouse
	
    #endregion DataSync
}
