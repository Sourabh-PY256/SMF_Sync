using EWP.SF.Common.Models;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Item.DataAccess;
using EWP.SF.Common.Constants;


namespace EWP.SF.Item.BusinessLayer;


public class SupplyOperation : ISupplyOperation
{
	private readonly ISupplyRepo _supplyRepo;
    private readonly IWarehouseOperation _warehouseOperation;
	private readonly IComponentOperation _componentOperation;

	public SupplyOperation(ISupplyRepo supplyRepo
	, IWarehouseOperation warehouseOperation
	, IComponentOperation componentOperation)
	{
		_supplyRepo = supplyRepo;
		_warehouseOperation = warehouseOperation;
		_componentOperation = componentOperation;
	}

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<List<Supply>> ListSupply(User systemOperator, string OrderNumber = "")
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return await _supplyRepo.ListSupply(OrderNumber).ConfigureAwait(false);
	}

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<ResponseData> MergeSupply(Supply supplyInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		ResponseData returnValue = null;

		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		returnValue = _supplyRepo.MergeSupply(supplyInfo, systemOperator, Validate);
		if (!Validate && returnValue is not null)
		{
			Supply ObjSupply = (await ListSupply(systemOperator, returnValue.Code).ConfigureAwait(false)).FirstOrDefault();
			//ObjSupply.Log((returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update), systemOperator);
			// if (NotifyOnce)
			// {
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Supply, returnValue.Action, Data = ObjSupply }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
			// 	returnValue.Entity = ObjSupply;
			// }
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	/// <exception cref="ValidationException"></exception>
	public async Task<List<ResponseData>> ListUpdateSupply(List<SupplyExternal> SupplyList, List<SupplyExternal> SupplyListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (SupplyList?.Count > 0)
		{
			NotifyOnce = SupplyList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (SupplyExternal cycleSupply in SupplyList)
			{
				SupplyExternal supply = cycleSupply;
				Line++;
				try
				{
					BaseId = supply.SupplyNo;
					Supply existingSupply = (await ListSupply(systemOperator, cycleSupply.SupplyNo).ConfigureAwait(false))?.FirstOrDefault();
					bool editMode = existingSupply is not null;

					if (editMode && SupplyListOriginal is not null)
					{
						supply = SupplyListOriginal.Find(x => x.SupplyNo == cycleSupply.SupplyNo && x.LineID == cycleSupply.LineID);
						supply ??= cycleSupply;
					}
					List<ValidationResult> results = [];
					ValidationContext context = new(supply, null, null);
					if (!Validator.TryValidateObject(supply, context, results))
					{
						if (editMode)
						{
							_ = results.RemoveAll(result => result.ErrorMessage.Contains("is required", StringComparison.OrdinalIgnoreCase));
						}
						if (results.Count > 0)
						{
							throw new ValidationException($"{results[0]}");
						}
					}
					// Item
					string itemCode = null;
					string itemName = null;
					if (!editMode)
					{
						Component ObjItem = _componentOperation.GetComponentByCode(supply.ItemCode);
						if (ObjItem is not null)
						{
							if (ObjItem.Status == Status.Failed)
							{
								throw new Exception($"Item Not Found (Code: {supply.ItemCode})");
							}
							itemCode = ObjItem.Code;
							itemName = ObjItem.Name;
						}
						else
						{
							throw new Exception($"Item Not Found (Code: {supply.ItemCode})");
						}
					}
					// Warehouse
					string warehouseCode = null;
					if (!editMode || !string.IsNullOrEmpty(supply.WarehouseCode))
					{
						Warehouse warehouseData = _warehouseOperation.GetWarehouse(supply.WarehouseCode);
						if (warehouseData is not null)
						{
							warehouseCode = warehouseData.Code;
						}
						if (string.IsNullOrEmpty(warehouseCode))
						{
							throw new Exception($"Warehouse Not Found (Code: {supply.WarehouseCode})");
						}
					}
					Supply supplyInfo = null;
					if (!editMode)
					{
						supplyInfo = new Supply
						{
							Code = supply.SupplyNo,
							VendorCode = supply.VendorCode,
							VendorName = supply.VendorName,
							LineNo = supply.LineID,
							ItemCode = itemCode,
							Quantity = supply.Quantity,
							WarehouseCode = warehouseCode,
							Unit = supply.InventoryUoM,
							ExpectedDate = supply.SupplyDate,
							Type = string.Equals(supply.Type, "purchase order", StringComparison.OrdinalIgnoreCase) ? 1 : 2,
							Status = supply?.Status?.ToUpperInvariant() == "ACTIVE" ? Status.Active : Status.Disabled
						};
					}
					else
					{
						supplyInfo = existingSupply;
						supplyInfo.Quantity = supply.Quantity;
						supplyInfo.ExpectedDate = supply.SupplyDate;
						if (!string.IsNullOrEmpty(supply.Type))
						{
							supplyInfo.Type = string.Equals(supply.Type, "purchase order", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
						}
						if (!string.IsNullOrEmpty(supply.InventoryUoM))
						{
							supplyInfo.Unit = supply.InventoryUoM;
						}
						if (!string.IsNullOrEmpty(supply.WarehouseCode))
						{
							supplyInfo.WarehouseCode = warehouseCode;
						}
						if (!string.IsNullOrEmpty(supply.VendorName))
						{
							supplyInfo.VendorName = supply.VendorName;
						}
						if (!string.IsNullOrEmpty(supply.VendorCode))
						{
							supplyInfo.VendorCode = supply.VendorCode;
						}
						supplyInfo.LineNo = supply.LineID;
						if (!string.IsNullOrEmpty(supply.ItemCode))
						{
							supplyInfo.ItemCode = supply.ItemCode;
						}
						if (!string.IsNullOrEmpty(supply.Status))
						{
							supplyInfo.Status = string.Equals(supply.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled;
						}
					}
					ResponseData response = await MergeSupply(supplyInfo, systemOperator, Validate).ConfigureAwait(false);
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
		if (!Validate)
		{
			// if (!NotifyOnce)
			// {
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Supply, Action = ActionDB.IntegrateAll.ToStr() });
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
