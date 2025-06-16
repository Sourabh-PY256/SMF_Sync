using EWP.SF.Common.Models;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using Newtonsoft.Json;
using EWP.SF.Helper;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Constants;


namespace EWP.SF.KafkaSync.BusinessLayer;


public class DemandOperation : IDemandOperation
{
	private readonly IDemandRepo _demandRepo;

	private readonly IMeasureUnitOperation _measureUnitOperation;

	public DemandOperation(IDemandRepo demandRepo, IMeasureUnitOperation measureUnitOperation)
	{
		_demandRepo = demandRepo;
		_measureUnitOperation = measureUnitOperation;
	}
	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<List<Demand>> ListDemand(User systemOperator, string OrderNumber = "")
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return await _demandRepo.ListDemand(OrderNumber).ConfigureAwait(false);
	}
	/// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateDemandBulk(List<DemandExternal> demandList, List<DemandExternal> demandListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		List<Demand> existingDemandList = await ListDemand(systemOperator, null).ConfigureAwait(false);
		List<MeasureUnit> unitsList = _measureUnitOperation.GetMeasureUnits();

		List<Demand> componentsToMerge = [];
		const bool NotifyOnce = false;
		if (demandList?.Count > 0)
		{
			int Line = 0;
			string BaseId = string.Empty;
			Demand itemInfo = null;
			demandList.ForEach(cycleItem =>
			{
				DemandExternal item = cycleItem;
				Line++;
				try
				{
					itemInfo = null;
					BaseId = item.DemandNo;
					Demand existingDemand = existingDemandList?.Find(d => d.Code == cycleItem.DemandNo);
					bool editMode = existingDemand is not null;
					if (editMode && demandListOriginal is not null)
					{
						item = demandListOriginal.Find(x => x.DemandNo == cycleItem.DemandNo && x.LineID == cycleItem.LineID && x.ItemCode == cycleItem.ItemCode);
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
					item.CustomerCode = item.CustomerCode.Replace("\"", "´´");
					item.CustomerName = item.CustomerName.Replace("\"", "´´");

					itemInfo = new Demand
					{
						ItemCode = item.ItemCode,
						CustomerName = item.CustomerName,
						CustomerCode = item.CustomerCode,
						Status = status,
						LineNo = item.LineID,
						Id = item.DemandNo,
						Code = item.DemandNo,
						Type = item.Type.ToStr().Contains("ORDER", StringComparison.OrdinalIgnoreCase) ? 1 : 2,
						Quantity = item.Quantity,
						ExpectedDate = item.DemandDate,
						WarehouseCode = item.WarehouseCode,
						Unit = item.InventoryUoM,
						Priority = 1
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
							itemInfo.Unit = unitInventory.Id;
						}
						else
						{
							throw new Exception("Item InventoryUoM is invalid");
						}
					}

					if (editMode)
					{
						if (string.IsNullOrEmpty(item.InventoryUoM))
						{
							itemInfo.Unit = existingDemand.Unit;
						}

						if (!string.IsNullOrEmpty(item.Status))
						{
							itemInfo.Status = string.Equals(item.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled;
						}
						else
						{
							itemInfo.Status = existingDemand.Status;
						}

						if (!string.IsNullOrEmpty(item.Type))
						{
							itemInfo.Type = item.Type.ToStr().Contains("ORDER", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
						}
						else
						{
							itemInfo.Type = existingDemand.Type;
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
			});

			string itemmsJson = JsonConvert.SerializeObject(componentsToMerge, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			ResponseData result = _demandRepo.MergeDemandBulk(itemmsJson, systemOperator, Validate);
		}
		if (!Validate)
		{
			// TODO: uncomment this after migrated main project as discuused mario
			// if (!NotifyOnce)
			// {
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Item, Action = ActionDB.IntegrateAll.ToStr() });
			// } 
			returnValue = Level switch
			{
				LevelMessage.Warning => [.. returnValue.Where(x => !string.IsNullOrEmpty(x.Message))],
				LevelMessage.Error => [.. returnValue.Where(x => !x.IsSuccess)],
				_ => returnValue
			};
		}

		return returnValue;
	}
}