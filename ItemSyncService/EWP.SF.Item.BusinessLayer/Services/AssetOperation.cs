
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

public class AssetOperation : IAssetOperation
{
    private readonly IAssetRepo _assetRepo;
    private readonly IApplicationSettings _applicationSettings;
    private readonly IAttachmentOperation _attachmentOperation;
    private readonly IActivityOperation _activityOperation;
    private readonly ISchedulingCalendarShiftsOperation _schedulingCalendarShiftsOperation;

    public AssetOperation(IAssetRepo activityRepo, IApplicationSettings applicationSettings, IAttachmentOperation attachmentOperation,
     IActivityOperation activityOperation, ISchedulingCalendarShiftsOperation  schedulingCalendarShiftsOperation )
    {
        _activityRepo = activityRepo;
        _applicationSettings = applicationSettings;
        _attachmentOperation = attachmentOperation;
        _activityOperation = activityOperation;
        _schedulingCalendarShiftsOperation =  schedulingCalendarShiftsOperation;
;
    }

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

    #region Production Lines

    public ProductionLine[] ListProductionLines(bool deleted = false, DateTime? DeltaDate = null)
    {
        List<ProductionLine> lines = _activityRepo.ListProductionLines(DeltaDate);

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

        return _activityRepo.GetProductionLine(Code);
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

        returnValue = _activityRepo.CreateProductionLine(productionLineInfo, systemOperator, Validate, Level);
        if (!Validate)
        {
            ProductionLine ObjProductionLine = _activityRepo.GetProductionLine(productionLineInfo.Code);
            // await ObjProductionLine.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);discussmario
            //_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductionLine, returnValue.Action, Data = ObjProductionLine });
            if (NotifyOnce)
            {
                _ = await _attachmentOperation.SaveImageEntity("ProductionLine", productionLineInfo.Image, productionLineInfo.Code, systemOperator).ConfigureAwait(false);
                if (productionLineInfo.AttachmentIds is not null)
                {
                    foreach (string attachment in productionLineInfo.AttachmentIds)
                    {
                        await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                    }
                }

                if (productionLineInfo.Activities?.Count > 0)
                {
                    foreach (Activity activity in productionLineInfo.Activities)
                    {
                        if (string.IsNullOrEmpty(activity.Id))
                        {
                            Activity newActivity = await _activityOperation.CreateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ManualDelete)
                        {
                            bool tempResult = await _activityOperation.DeleteActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else
                        {
                            if (activity.ActivityClassId > 0)
                            {
                                _ = await _activityOperation.UpdateActivity(activity, systemOperator).ConfigureAwait(false);
                            }
                        }
                    }
                }
                if (productionLineInfo.Shift?.CodeShift is not null
                && !string.IsNullOrEmpty(productionLineInfo.Shift.CodeShift))
                {
                    productionLineInfo.Shift.Validation = false;
                    productionLineInfo.Shift.IdAsset = productionLineInfo.Code;
                    _ = _schedulingCalendarShiftsOperation.UpdateSchedulingCalendarShifts(productionLineInfo.Shift, systemOperator);
                }
                if (productionLineInfo.ShiftDelete?.Id is not null
                     && !string.IsNullOrEmpty(productionLineInfo.ShiftDelete.Id))
                {
                    productionLineInfo.ShiftDelete.Validation = false;
                    _ = _schedulingCalendarShiftsOperation.DeleteSchedulingCalendarShifts(productionLineInfo.ShiftDelete, systemOperator);
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

        return _activityRepo.DeleteProductionLine(productionLineInfo, systemOperator);
    }

    #endregion Production Lines

    #region WorkCenter

    public WorkCenter GetWorkCenter(string WorkCenterCode)
    {
        return _activityRepo.GetWorkCenter(WorkCenterCode);
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

        returnValue = _activityRepo.CreateWorkCenter(WorkCenterInfo, systemOperator, Validate, Level);
        if (!Validate)
        {
            ObjWorkCenter = _activityRepo.GetWorkCenter(WorkCenterInfo.Code);
            if (NotifyOnce)
            {
                _ = await _attachmentOperation.SaveImageEntity("Workcenter", WorkCenterInfo.Image, WorkCenterInfo.Code, systemOperator).ConfigureAwait(false);
                if (WorkCenterInfo.AttachmentIds is not null)
                {
                    foreach (string attachment in WorkCenterInfo.AttachmentIds)
                    {
                        await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                    }
                }
                if (WorkCenterInfo.Activities?.Count > 0)
                {
                    foreach (Activity activity in WorkCenterInfo.Activities)
                    {
                        if (string.IsNullOrEmpty(activity.Id))
                        {
                            Activity newActivity = await _activityOperation.CreateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ManualDelete)
                        {
                            bool tempResult = await _activityOperation.DeleteActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else
                        {
                            if (activity.ActivityClassId > 0)
                            {
                                _ = await _activityOperation.UpdateActivity(activity, systemOperator).ConfigureAwait(false);
                            }
                        }
                    }
                }
                if (WorkCenterInfo.Shift?.CodeShift is not null
                                 && !string.IsNullOrEmpty(WorkCenterInfo.Shift.CodeShift))
                {
                    WorkCenterInfo.Shift.Validation = false;
                    WorkCenterInfo.Shift.IdAsset = WorkCenterInfo.Code;
                    _ = _schedulingCalendarShiftsOperation.UpdateSchedulingCalendarShifts(WorkCenterInfo.Shift, systemOperator);
                }
                if (WorkCenterInfo.ShiftDelete?.Id is not null
                                                         && !string.IsNullOrEmpty(WorkCenterInfo.ShiftDelete.Id))
                {
                    WorkCenterInfo.ShiftDelete.Validation = false;
                    _ = _schedulingCalendarShiftsOperation.DeleteSchedulingCalendarShifts(WorkCenterInfo.ShiftDelete, systemOperator);
                }
                //_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Workcenter, returnValue.Action, Data = ObjWorkCenter }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
            }
            //await ObjWorkCenter.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
        }
        return returnValue;
    }

    #endregion WorkCenter

    #region Floor

    public Floor GetFloor(string FloorCode)
    {
        return _activityRepo.GetFloor(FloorCode);
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

        returnValue = _activityRepo.CreateFloor(FloorInfo, systemOperator, Validate, Level);
        if (!Validate)
        {
            Floor ObjFloor = _activityRepo.GetFloor(FloorInfo.Code);
            //await ObjFloor.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            if (NotifyOnce)
            {
                _ = await _attachmentOperation.SaveImageEntity("Floor", FloorInfo.Image, FloorInfo.Code, systemOperator).ConfigureAwait(false);
                if (FloorInfo.AttachmentIds is not null)
                {
                    foreach (string attachment in FloorInfo.AttachmentIds)
                    {
                        await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                    }
                }
                if (FloorInfo.Activities?.Count > 0)
                {
                    foreach (Activity activity in FloorInfo.Activities)
                    {
                        if (string.IsNullOrEmpty(activity.Id))
                        {
                            Activity newActivity = await _activityOperation.CreateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ManualDelete)
                        {
                            bool tempResult = await _activityOperation.DeleteActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else
                        {
                            if (activity.ActivityClassId > 0)
                            {
                                _ = await _activityOperation.UpdateActivity(activity, systemOperator).ConfigureAwait(false);
                            }
                        }
                    }
                }
                if (FloorInfo.Shift?.CodeShift is not null
                                    && !string.IsNullOrEmpty(FloorInfo.Shift.CodeShift))
                {
                    FloorInfo.Shift.Validation = false;
                    FloorInfo.Shift.IdAsset = FloorInfo.Code;
                    _ = _schedulingCalendarShiftsOperation.UpdateSchedulingCalendarShifts(FloorInfo.Shift, systemOperator);
                }
                if (FloorInfo.ShiftDelete?.Id is not null
                  && !string.IsNullOrEmpty(FloorInfo.ShiftDelete.Id))
                {
                    FloorInfo.ShiftDelete.Validation = false;
                    _ = _schedulingCalendarShiftsOperation.DeleteSchedulingCalendarShifts(FloorInfo.ShiftDelete, systemOperator);
                }

                //_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Floor, returnValue.Action, Data = ObjFloor }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
            }
        }
        return returnValue;
    }
    #endregion Floor

    #region Facility

    public Facility GetFacility(string Code)
    {
        return _activityRepo.GetFacility(Code);
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

        returnValue = _activityRepo.CreateFacility(FacilityInfo, systemOperator, Validate, Level);

        if (!Validate)
        {
            Facility ObjFacility = GetFacility(returnValue.Code);
            //await ObjFacility.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            if (NotifyOnce)
            {
                // _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Facility, returnValue.Action, Data = ObjFacility }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                _ = await _attachmentOperation.SaveImageEntity("Facility", FacilityInfo.Image, FacilityInfo.Code, systemOperator).ConfigureAwait(false);
                if (FacilityInfo.AttachmentIds is not null)
                {
                    foreach (string attachment in FacilityInfo.AttachmentIds)
                    {
                        await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                    }
                }

                if (FacilityInfo.Activities?.Count > 0)
                {
                    foreach (Activity activity in FacilityInfo.Activities)
                    {
                        if (string.IsNullOrEmpty(activity.Id))
                        {
                            Activity newActivity = await _activityOperation.CreateActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else if (activity.ManualDelete)
                        {
                            bool tempResult = await _activityOperation.DeleteActivity(activity, systemOperator).ConfigureAwait(false);
                        }
                        else
                        {
                            if (activity.ActivityClassId > 0)
                            {
                                _ = await _activityOperation.UpdateActivity(activity, systemOperator).ConfigureAwait(false);
                            }
                        }
                    }
                }
                if (FacilityInfo.Shift?.CodeShift is not null
                    && !string.IsNullOrEmpty(FacilityInfo.Shift.CodeShift))
                {
                    FacilityInfo.Shift.Validation = false;
                    FacilityInfo.Shift.IdAsset = FacilityInfo.Code;
                    _ = _schedulingCalendarShiftsOperation.UpdateSchedulingCalendarShifts(FacilityInfo.Shift, systemOperator);
                }
                if (FacilityInfo.ShiftDelete?.Id is not null
                  && !string.IsNullOrEmpty(FacilityInfo.ShiftDelete.Id))
                {
                    FacilityInfo.ShiftDelete.Validation = false;
                    _ = _schedulingCalendarShiftsOperation.DeleteSchedulingCalendarShifts(FacilityInfo.ShiftDelete, systemOperator);
                }
            }
        }

        return returnValue;
    }

    #endregion Facility
}