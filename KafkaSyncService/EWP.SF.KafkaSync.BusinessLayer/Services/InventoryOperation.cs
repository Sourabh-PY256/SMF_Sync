using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Constants;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class InventoryOperation : IInventoryOperation
{
    private readonly IInventoryRepo _inventoryRepo;  
    private readonly IAttachmentOperation _attachmentOperation;

    public InventoryOperation(IInventoryRepo inventoryRepo
    , IAttachmentOperation attachmentOperation)
    {
        _inventoryRepo = inventoryRepo;
        _attachmentOperation = attachmentOperation;
    }


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
                    InventoryItemGroup existingInventory = GetInventory(cycleInventoryGroup.ItemGroupCode);
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
                    InventoryItemGroup inventoryGroupInfo = new()
                    {
                        Code = inventoryGroup.ItemGroupCode,
                        Name = !string.IsNullOrEmpty(inventoryGroup.ItemGroupName) ? inventoryGroup.ItemGroupName : inventoryGroup.ItemGroupCode,
                        Status = status
                    };
                    // returntValue.Add(_inventoryRepo.MergeInventory(inventoryGroupInfo, systemOperator, Validate, Level));
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

    public InventoryItemGroup GetInventory(string Code)
    {
        return _inventoryRepo.GetInventory(Code);
    }

    public async Task<ResponseData> MergeInventory(InventoryItemGroup InventoryInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
    {
        ResponseData returnValue = new();

        #region Permission validation

        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_INVENTORY_MANAGE))
        {
        	throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        returnValue = _inventoryRepo.MergeInventory(InventoryInfo, systemOperator, Validate);
        if (!Validate && returnValue is not null)
        {
            InventoryItemGroup ObjInventory = ListInventory(systemOperator, returnValue.Code).Find(x => x.Status != Status.Failed);
            await ObjInventory.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            if (NotifyOnce)
            {
                _ = await _attachmentOperation.SaveImageEntity("ItemGroup", InventoryInfo.Image, InventoryInfo.Code, systemOperator).ConfigureAwait(false);
                if (InventoryInfo.AttachmentIds is not null)
                {
                    foreach (string attachment in InventoryInfo.AttachmentIds)
                    {
                        await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                    }
                }
                // _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Inventory, returnValue.Action, Data = ObjInventory }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                returnValue.Entity = ObjInventory;
            }
        }
        return returnValue;
    }
    public List<InventoryItemGroup> ListInventory(User systemOperator, string InventoryCode = "", DateTime? DeltaDate = null)
    {
        #region Permission validation

        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_INVENTORY_MANAGE))
        {
        	throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        return _inventoryRepo.ListInventory(InventoryCode, DeltaDate);
    }

    #endregion Inventorys
    #region SalesOrder

	/// <summary>
	/// Obtiene la lista de "demand"
	/// </summary>
	/// <param name="Id"></param>
	/// <param name="SalesOrder"></param>
	/// <param name="CustomerCode"></param>
	/// <param name="systemOperator">User</param>
	/// <param name="getAsMasterDetail"></param>
	/// <param name="DeltaDate"></param>
	/// <exception cref="Exception"></exception>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public SaleOrder[] ListSalesOrder(string Id, string SalesOrder, string CustomerCode, User systemOperator, bool getAsMasterDetail = false, DateTime? DeltaDate = null)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.INV_SALESORDER_LST))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		List<SaleOrder> data = _inventoryRepo.ListSalesOrder(Id, SalesOrder, CustomerCode, DeltaDate);
		// si es false, se mantienen solos los datos generales (el detalle tiene todos los datos)
		return !getAsMasterDetail
			? [.. data.SelectMany(static x => x.Detail)]
			: [.. data];
	}

	#endregion SalesOrder
}