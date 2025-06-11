using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.Helper;
using EWP.SF.Common.Constants;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class SchedulingCalendarShiftsOperation : ISchedulingCalendarShiftsOperation
{
    private readonly ISchedulingCalendarShiftsRepo _schedulingclandarShiftsRepo;

    public SchedulingCalendarShiftsOperation(ISchedulingCalendarShiftsRepo schedulingclandarShiftsRepo)
    {
        _schedulingclandarShiftsRepo = schedulingclandarShiftsRepo;
    }

    #region SchedulingCalendarShifts
    /// <summary>
    /// Method put
    /// </summary>
    /// <param name="requestValue"></param>
    /// <param name="systemOperator"></param>
    /// <returns></returns>
    public SchedulingCalendarShifts UpdateSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator)
    {
        #region Permission validation

        string errores = string.Empty;
        bool esNuevo = true;
        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_SCHEDULING_SHIFT_STATUS_MANAGE))
        {
           throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        //New Calendar Shift
        ValidatePostSchedulingCalendarShift(requestValue);
        requestValue.UserId = systemOperator.Id;
        if (requestValue.IsParent)
        {
            SchedulingCalendarShifts UpdateCalendarShift = _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(string.IsNullOrEmpty(requestValue.Id) ? "0x0x0x0x0" : requestValue.Id, null, null, requestValue.AssetLevel, null).FirstOrDefault();
            if (UpdateCalendarShift is not null)
            {
                esNuevo = false;
                if (UpdateCalendarShift.IsParent)
                {
                    foreach (SchedulingCalendarShifts item in _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(null, null, UpdateCalendarShift.IdParent, 0, null))
                    {
                        //Se valida si deselecciono algun item para borrarlo.
                        if (item.AssetLevel > UpdateCalendarShift.AssetLevel && !requestValue.listChildren.Any(q => q.Id == item.Id))
                        {
                            ValidateDeleteSchedulingCalendarShift(item);
                            _ = _schedulingclandarShiftsRepo.DeleteSchedulingCalendarShifts(item);
                        }
                    }
                }
            }

            _ = _schedulingclandarShiftsRepo.PutSchedulingCalendarShifts(requestValue, systemOperator);
            //Se aplican cambios solo a los registros seleccionados.
            List<SchedulingCalendarShifts> childrenList;
            if (!requestValue.isEmployee && requestValue.Validation)//Para proxima funcionalidad
            {
                requestValue.listChildren.ForEach(q =>
                {
                    q.AssetLevelCode = q.IdAsset[..q.IdAsset[..q.IdAsset.IndexOf('-')].Length];
                    q.IdAsset = q.IdAsset.Substring(q.IdAsset.IndexOf('-') + 1, q.IdAsset.Length - q.IdAsset[..q.IdAsset.IndexOf('-')].Length - 1);
                });
            }
            //Falta validar el level para que sea unico el valor.
            childrenList = [.. requestValue.listChildren.Where(q => q.IdAsset != requestValue.IdAsset || q.AssetLevelCode != requestValue.AssetLevelCode)];

            foreach (SchedulingCalendarShifts children in childrenList)
            {
                if (!esNuevo)
                {
                    SchedulingCalendarShifts UpdateChildCalendarShift = _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(null, null, UpdateCalendarShift is not null ? UpdateCalendarShift.IdParent : requestValue.IdParent, 0, requestValue.AssetLevelCode)
                        .Find(q => q.IdAsset == children.IdAsset && q.AssetLevelCode == children.AssetLevelCode && q.FromDate == (UpdateCalendarShift is not null ? UpdateCalendarShift.FromDate : requestValue.FromDate));
                    children.Id = UpdateChildCalendarShift?.Id;
                }
                children.FromDate = requestValue.FromDate;
                children.IdParent = requestValue.IdParent;
                children.Origin = requestValue.Origin;
                //children.AssetLevel = requestValue.AssetLevel;
                children.CodeShift = requestValue.CodeShift;
                children.Status = requestValue.Status;
                children.Validation = requestValue.Validation;
                children.IsParent = false;
                try
                {
                    ValidatePostSchedulingCalendarShift(children);
                    if (!children.Validation)
                    {
                        _ = _schedulingclandarShiftsRepo.PutSchedulingCalendarShifts(children, systemOperator);
                    }
                }
                catch (Exception ex)
                {
                    errores += ex.Message + "|";
                }
            }
            return !string.IsNullOrEmpty(errores) ? throw new Exception(errores) : requestValue;
        }
        else
        {
            return _schedulingclandarShiftsRepo.PutSchedulingCalendarShifts(requestValue, systemOperator);
        }
    }

    /// <summary>
    /// Method delete
    /// </summary>
    /// <param name="requestValue"></param>
    /// <param name="systemOperator"></param>
    /// <returns></returns>
    public bool DeleteSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator)
    {
        #region Permission validation

        string errores = string.Empty;
        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_SCHEDULING_SHIFT_STATUS_MANAGE))
        {
           throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        requestValue.UserId = systemOperator.Id;
        if (requestValue.IsParent)
        {
            ValidateDeleteSchedulingCalendarShift(requestValue);
            SchedulingCalendarShifts DeleteCalendarShift = _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(requestValue.Id, null, null, requestValue.AssetLevel, null).FirstOrDefault();
            //Se aplica borrado solo a los registros seleccionados.
            List<SchedulingCalendarShifts> childrenList;
            if (!requestValue.isEmployee)
            {
                requestValue.listChildren.ForEach(q =>
                {
                    q.AssetLevelCode = q.IdAsset[..q.IdAsset[..q.IdAsset.IndexOf('-')].Length];
                    q.IdAsset = q.IdAsset.Substring(q.IdAsset.IndexOf('-') + 1, q.IdAsset.Length - q.IdAsset[..q.IdAsset.IndexOf('-')].Length - 1);
                });
            }
            childrenList = [.. requestValue.listChildren.Where(q => q.IdAsset != DeleteCalendarShift.IdAsset || q.AssetLevelCode != DeleteCalendarShift.AssetLevelCode)];
            foreach (SchedulingCalendarShifts children in childrenList)
            {
                SchedulingCalendarShifts DeleteChildCalendarShift = _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(null, null, DeleteCalendarShift.IdParent, requestValue.AssetLevel, requestValue.AssetLevelCode)
                    .Find(q => q.IdAsset == children.IdAsset && q.AssetLevelCode == children.AssetLevelCode && q.FromDate == DeleteCalendarShift.FromDate);
                children.Id = DeleteChildCalendarShift?.Id;
                try
                {
                    ValidateDeleteSchedulingCalendarShift(children);
                    _ = _schedulingclandarShiftsRepo.DeleteSchedulingCalendarShifts(children);
                }
                catch (Exception ex)
                {
                    errores += ex.Message + ",";
                }
            }
            if (!string.IsNullOrEmpty(errores))
            {
                throw new Exception(errores);
            }
            _ = _schedulingclandarShiftsRepo.DeleteSchedulingCalendarShifts(requestValue);
            return true;
        }
        else
        {
            ValidateDeleteSchedulingCalendarShift(requestValue);
            return _schedulingclandarShiftsRepo.DeleteSchedulingCalendarShifts(requestValue);
        }
    }

    private void ValidatePostSchedulingCalendarShift(SchedulingCalendarShifts validRequest)
    {
        //New Calendar Shift
        if (string.IsNullOrEmpty(validRequest.Id))
        {
            List<SchedulingCalendarShifts> calendarsShift = _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(null, validRequest.IdAsset, null, validRequest.AssetLevel, validRequest.AssetLevelCode
                , validRequest.Origin);
            if (calendarsShift.Count > 0)
            {
                SchedulingCalendarShifts last = calendarsShift.FirstOrDefault();
                if (validRequest.FromDate <= last.FromDate)
                {
                    throw new Exception("@ExistShiftBindingBefore" + " | " + validRequest.AssetLevelCode + ": " + validRequest.IdAsset + "\r\n");
                }
                if (validRequest.CodeShift == last.CodeShift)
                {
                    throw new Exception("@CurrentShiftBinding" + " | " + validRequest.AssetLevelCode + ": " + validRequest.IdAsset + " - " + validRequest.Name + "\r\n");
                }
            }
        }
        else
        {
            List<SchedulingCalendarShifts> calendarsShift = _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(validRequest.Id, validRequest.IdAsset, null, validRequest.AssetLevel, null, validRequest.Origin);
            if (calendarsShift.Count > 0)
            {
                SchedulingCalendarShifts update = calendarsShift.FirstOrDefault();
                if (update.ToDate.HasValue)
                {
                    throw new Exception("@NotCurrentShiftBinding");
                }
                List<SchedulingCalendarShifts> beforeCalendarsShift = _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(null, validRequest.IdAsset, null, validRequest.AssetLevel, null, validRequest.Origin);
                SchedulingCalendarShifts beforeUpdate = beforeCalendarsShift.Find(q => q.Id != validRequest.Id);
                if (beforeUpdate is not null)
                {
                    if (beforeUpdate.CodeShift == validRequest.CodeShift)
                    {
                        throw new Exception("@SameShiftBinding");
                    }
                    if (beforeUpdate.FromDate >= validRequest.FromDate)
                    {
                        throw new Exception("@ExistShiftBindingBefore");
                    }
                }
            }
            else
            {
                throw new Exception("@NoRelatedRecords");
            }
        }
    }

    private void ValidateDeleteSchedulingCalendarShift(SchedulingCalendarShifts validRequest)
    {
        if (string.IsNullOrEmpty(validRequest.Id))
        {
            throw new Exception("@NoRelatedRecords");
        }
        List<SchedulingCalendarShifts> calendarsShift = _schedulingclandarShiftsRepo.GetSchedulingCalendarShifts(validRequest.Id, null, null, validRequest.AssetLevel, null);
        if (calendarsShift.Count > 0)
        {
            SchedulingCalendarShifts delete = calendarsShift.FirstOrDefault();
            if (delete.ToDate.HasValue)
            {
                throw new Exception("@NotCurrentShiftBinding");
            }
            if (delete.FromDate < DateTime.Now)
            {
                throw new Exception("@NotDeleteShiftBinding");
            }
        }
        else
        {
            throw new Exception("@NoRelatedRecords");
        }
    }

    #endregion SchedulingCalendarShifts
}