using EWP.SF.Common.Models;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Item.DataAccess;


namespace EWP.SF.Item.BusinessLayer;


public class WarehouseOperation : IWarehouseOperation
{
    private readonly IWarehouseRepo _warehouseRepo;
    private readonly IApplicationSettings _applicationSettings;

    private readonly IAttachmentOperation _attachmentOperation;

    public WarehouseOperation(IWarehouseRepo warehouseRepo, IApplicationSettings applicationSettings
    , IAttachmentOperation attachmentOperation)
    {
        _warehouseRepo = warehouseRepo;
        _applicationSettings = applicationSettings;
        _attachmentOperation = attachmentOperation;
    }
    #region Warehouse

    public Warehouse GetWarehouse(string Code)
    {
        return _warehouseRepo.GetWarehouse(Code);
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
        returnValue = _warehouseRepo.MergeWarehouse(WarehouseInfo, systemOperator, Validate);
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

    #endregion Warehouse
}