using EWP.SF.Common.Models;
using EWP.SF.Item.DataAccess;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.ComponentModel.DataAnnotations;

namespace EWP.SF.Item.BusinessLayer;

public class InventoryStatusOperation : IInventoryStatusOperation
{
    private readonly IInventoryStatusRepo _inventoryStatusRepo;
    private readonly IApplicationSettings _applicationSettings;

    private readonly IAttachmentOperation _attachmentOperation;

    public InventoryStatusOperation(IInventoryStatusRepo inventoryStatusRepo, IApplicationSettings applicationSettings
    , IAttachmentOperation attachmentOperation)
    {
        _inventoryStatusRepo = inventoryStatusRepo;
        _applicationSettings = applicationSettings;
        _attachmentOperation = attachmentOperation;
    }

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public List<InventoryStatus> ListInventoryStatus(User systemOperator, string LotSerialStatusCode = "", DateTime? DeltaDate = null)
	{
		#region Permission validation

		// if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		return _inventoryStatusRepo.ListInventoryStatus(LotSerialStatusCode, DeltaDate);
	}

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<ResponseData> MergeInventoryStatus(InventoryStatus InventoryStatusInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		ResponseData returnValue = null;

		#region Permission validation

		// if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		// Warehouse returnValue = BrokerDAL.CreateWarehouse(WarehouseInfo, systemOperator);
		returnValue = _inventoryStatusRepo.MergeInventoryStatus(InventoryStatusInfo, systemOperator, Validate);
		if (!Validate && returnValue is not null)
		{
			InventoryStatus ObjInventoryStatus = ListInventoryStatus(systemOperator, returnValue.Code).Find(static x => x.Status != Status.Failed);
			// await ObjInventoryStatus.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
			if (NotifyOnce)
			{
				await _attachmentOperation.SaveImageEntity("inventorystatus", InventoryStatusInfo.Image, InventoryStatusInfo.Code, systemOperator).ConfigureAwait(false);
				if (InventoryStatusInfo.AttachmentIds is not null)
				{
					foreach (string attachment in InventoryStatusInfo.AttachmentIds)
					{
						await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
					}
				}
				//Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.InventoryStatus, returnValue.Action, Data = ObjInventoryStatus }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
				returnValue.Entity = ObjInventoryStatus;
			}
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateInventoryStatus(List<InventoryStatusExternal> inventoryStatusList, List<InventoryStatusExternal> inventoryStatusListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (inventoryStatusList?.Count > 0)
		{
			NotifyOnce = inventoryStatusList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (InventoryStatusExternal cycleInventoryStatus in inventoryStatusList)
			{
				InventoryStatusExternal inventoryStatus = cycleInventoryStatus;
				Line++;
				try
				{
					BaseId = inventoryStatus.InventoryStatusCode;
					InventoryStatus existingRecord = ListInventoryStatus(systemOperator, cycleInventoryStatus.InventoryStatusCode)?.Find(x => x.Status != Status.Failed);
					bool editMode = existingRecord is not null;
					if (editMode && inventoryStatusListOriginal is not null)
					{
						inventoryStatus = inventoryStatusListOriginal.Find(x => x.InventoryStatusCode == cycleInventoryStatus.InventoryStatusCode);
						inventoryStatus ??= cycleInventoryStatus;
					}
					List<ValidationResult> results = [];
					ValidationContext context = new(inventoryStatus, null, null);
					if (!Validator.TryValidateObject(inventoryStatus, context, results))
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
					if (!string.IsNullOrEmpty(inventoryStatus.Status) && string.Equals(inventoryStatus.Status.Trim(), "DISABLE", StringComparison.OrdinalIgnoreCase))
					{
						status = (Status)Status.Disabled.ToInt32();
					}
					if (status != Status.Active && !editMode)
					{
						throw new Exception("Cannot import a disabled Inventory Status record");
					}
					InventoryStatus inventoryStatusInfo = new()
					{
						Code = inventoryStatus.InventoryStatusCode,
						Name = !string.IsNullOrEmpty(inventoryStatus.InventoryStatusName) ? inventoryStatus.InventoryStatusName : inventoryStatus.InventoryStatusCode,
						Status = status,
						IsDelivery = string.Equals(inventoryStatus.IsDelivery.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsARInvoice = string.Equals(inventoryStatus.IsARInvoice.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsARCreditMemo = string.Equals(inventoryStatus.IsARCreditMemo.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsReturn = string.Equals(inventoryStatus.IsReturn.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsAPReturn = string.Equals(inventoryStatus.IsAPReturn.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsAPCreditMemo = string.Equals(inventoryStatus.IsAPCreditMemo.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsInventoryIssue = string.Equals(inventoryStatus.IsInventoryIssue.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsMaterialIssue = string.Equals(inventoryStatus.IsMaterialIssue.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsAllocation = string.Equals(inventoryStatus.IsAllocation.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsInventoryTransfer = string.Equals(inventoryStatus.IsInventoryTransfer.ToStr(), "YES", StringComparison.OrdinalIgnoreCase),
						IsPlanning = string.Equals(inventoryStatus.IsPlanning.ToStr(), "YES"
, StringComparison.OrdinalIgnoreCase)
					};
					if (editMode)
					{
						inventoryStatusInfo = existingRecord;
						if (!string.IsNullOrEmpty(inventoryStatus.InventoryStatusName))
						{
							inventoryStatusInfo.Name = inventoryStatus.InventoryStatusName;
						}
						if (!string.IsNullOrEmpty(inventoryStatus.Status))
						{
							inventoryStatusInfo.Status = status;
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsDelivery))
						{
							inventoryStatusInfo.IsDelivery = string.Equals(inventoryStatus.IsDelivery.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsARInvoice))
						{
							inventoryStatusInfo.IsARInvoice = string.Equals(inventoryStatus.IsARInvoice.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsARCreditMemo))
						{
							inventoryStatusInfo.IsARCreditMemo = string.Equals(inventoryStatus.IsARCreditMemo.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsReturn))
						{
							inventoryStatusInfo.IsReturn = string.Equals(inventoryStatus.IsReturn.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsAPReturn))
						{
							inventoryStatusInfo.IsAPReturn = string.Equals(inventoryStatus.IsAPReturn.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsAPCreditMemo))
						{
							inventoryStatusInfo.IsAPCreditMemo = string.Equals(inventoryStatus.IsAPCreditMemo.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsInventoryIssue))
						{
							inventoryStatusInfo.IsInventoryIssue = string.Equals(inventoryStatus.IsInventoryIssue.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsMaterialIssue))
						{
							inventoryStatusInfo.IsMaterialIssue = string.Equals(inventoryStatus.IsMaterialIssue.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsAllocation))
						{
							inventoryStatusInfo.IsAllocation = string.Equals(inventoryStatus.IsAllocation.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsInventoryTransfer))
						{
							inventoryStatusInfo.IsInventoryTransfer = string.Equals(inventoryStatus.IsInventoryTransfer.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsInventoryCounting))
						{
							inventoryStatusInfo.IsInventoryCounting = string.Equals(inventoryStatus.IsInventoryCounting.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
						if (!string.IsNullOrEmpty(inventoryStatus.IsPlanning))
						{
							inventoryStatusInfo.IsPlanning = string.Equals(inventoryStatus.IsPlanning.ToStr(), "YES", StringComparison.OrdinalIgnoreCase);
						}
					}
					ResponseData response = await MergeInventoryStatus(inventoryStatusInfo, systemOperator, Validate).ConfigureAwait(false);
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
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.InventoryStatus, Action = ActionDB.IntegrateAll.ToStr() });
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
