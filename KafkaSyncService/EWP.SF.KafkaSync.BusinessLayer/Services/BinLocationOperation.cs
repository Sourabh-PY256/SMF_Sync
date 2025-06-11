using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Constants;


namespace EWP.SF.KafkaSync.BusinessLayer;

public class BinLocationOperation : IBinLocationOperation
{
    private readonly IBinLocationRepo _binLocationRepo;

    private readonly IWarehouseOperation _warehouseOperation;

    private readonly IAttachmentOperation _attachmentOperation;

    public BinLocationOperation(IBinLocationRepo binLocationRepo
    , IAttachmentOperation attachmentOperation, IWarehouseOperation warehouseOperation)
    {
        _binLocationRepo = binLocationRepo;
        _attachmentOperation = attachmentOperation;
        _warehouseOperation = warehouseOperation;
    }


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
                        Warehouse warehouseData = _warehouseOperation.GetWarehouse(binLocation.WarehouseCode);
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

        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_WAREHOUSE_MANAGE))
        {
        	throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        return _binLocationRepo.ListBinLocation(binLocationCode, DeltaDate);
    }

    public async Task<ResponseData> MergeBinLocation(BinLocation BinLocationInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
    {
        ResponseData returnValue = null;

        #region Permission validation

        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.INV_WAREHOUSE_MANAGE))
        {
        	throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        returnValue = _binLocationRepo.MergeBinLocation(BinLocationInfo, systemOperator, Validate);
        if (!Validate && returnValue is not null)
        {
            BinLocation ObjBinLocation = ListBinLocation(systemOperator, returnValue.Code).Find(x => x.Status != Status.Failed);
            await ObjBinLocation.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            _ = await _attachmentOperation.SaveImageEntity("BinLocation", BinLocationInfo.Image, ObjBinLocation.LocationCode, systemOperator).ConfigureAwait(false);
            if (BinLocationInfo.AttachmentIds is not null)
            {
                foreach (string attachment in BinLocationInfo.AttachmentIds)
                {
                    await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
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
}