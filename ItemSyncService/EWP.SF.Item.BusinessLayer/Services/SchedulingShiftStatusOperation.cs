using EWP.SF.Common.Models;
using EWP.SF.Item.DataAccess;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using EWP.SF.Common.Constants;
namespace EWP.SF.Item.BusinessLayer;

public class SchedulingShiftStatusOperation : ISchedulingShiftStatusOperation
{
    private readonly ISchedulingShiftStatusRepo _schedulingShiftStatusRepo;
    private readonly IAttachmentOperation _attachmentOperation;

    public SchedulingShiftStatusOperation(ISchedulingShiftStatusRepo schedulingShiftStatusRepo,
     IAttachmentOperation attachmentOperation)
    {
        _schedulingShiftStatusRepo = schedulingShiftStatusRepo;
        _attachmentOperation = attachmentOperation;
    }

   #region SchedulingShiftStatus


    /// <summary>
    /// Method put
    /// </summary>
    /// <param name="requestValue"></param>
    /// <param name="systemOperator"></param>
    /// <param name="Validate"></param>
    /// <param name="Level"></param>
    /// <param name="NotifyOnce"></param>
    /// <returns></returns>
    public async Task<List<ResponseData>> UpdateSchedulingShiftStatus(List<SchedulingShiftStatus> requestValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true)
    {
        List<ResponseData> returnValue = [];
        ResponseData responseMessage;
        ResponseData responseError;
        //List<User> users = new List<User>();
        SchedulingShiftStatus scheduleLog = null;
        int Line = 0;

        #region Permission validation

        if (!systemOperator.Permissions.Any(x => x.Code == Permissions.CP_SCHEDULING_SHIFT_STATUS_MANAGE))
        {
            throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        NotifyOnce = requestValue.Count == 1;
        foreach (SchedulingShiftStatus item in requestValue)
        {
            try
            {
                Line++;
                item.UserId = systemOperator.Id;
                responseMessage = _schedulingShiftStatusRepo.PutSchedulingShiftStatus(item, systemOperator, Validate);
                returnValue.Add(responseMessage);
                if (!responseMessage.IsSuccess)
                {
                    continue;
                }

                if (!Validate)
                {
                    scheduleLog = _schedulingShiftStatusRepo.GetSchedulingShiftStatus(item.Code, item.Type).FirstOrDefault();
                    await scheduleLog.Log(responseMessage.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                }

                if (NotifyOnce && !Validate)
                {
                    if (item.AttachmentIds is not null)
                    {
                        foreach (string attachment in item.AttachmentIds)
                        {
                            await _attachmentOperation.AttachmentSync(attachment, item.Code, systemOperator).ConfigureAwait(false);
                        }
                    }
                    // _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = scheduleLog.Type == "Asset" ? Entities.ShiftStatusAsset : Entities.ShiftStatusEmployee, Data = scheduleLog, responseMessage.Action }, responseMessage.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
                }
            }
            catch (Exception ex)
            {
                responseError = new ResponseData
                {
                    Message = ex.Message,
                    Code = "Line:" + Line.ToStr()
                };
                returnValue.Add(responseError);
            }
        }
        // if (!NotifyOnce && !Validate)
        // {
        //     _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = scheduleLog.Type == "Asset" ? Entities.ShiftStatusAsset : Entities.ShiftStatusEmployee, Data = new object { }, Action = ActionDB.IntegrateAll });
        // }
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
        return returnValue;
        //return _schedulingShiftStatusRepo.PutSchedulingShiftStatus(requestValue);
    }


    #endregion SchedulingShiftStatus
}