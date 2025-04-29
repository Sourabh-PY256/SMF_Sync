
using EWP.SF.ShopFloor.DataAccess;
using EWP.SF.ShopFloor.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
namespace EWP.SF.ShopFloor.BusinessLayer;

public  class WorkCenterService : IWorkCenterService    
{
	#region WorkCenter
 private readonly IWorkCenterRepository _workCenterRepository;

    public WorkCenterService(IWorkCenterRepository workCenterRepository)
    {
        _workCenterRepository = workCenterRepository;
    }
    public async Task<List<WorkCenter>>ListWorkCenter(DateTime? DeltaDate = null) => 
	await _workCenterRepository.ListWorkCenter(DeltaDate);

    public async Task<WorkCenter> GetWorkCenter(string WorkCenterCode)
	{
		return await _workCenterRepository.GetWorkCenter(WorkCenterCode);
	}

	// public async Task<ResponseData> CreateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator, bool Validate = false, string Level = "Success", bool NotifyOnce = true)
	// {
	// 	ResponseData returnValue = new();
	// 	WorkCenter ObjWorkCenter;

	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_WORKCENTER_MANAGE))
	// 	{
	// 		throw new UnauthorizedAccessException(noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	returnValue = await _workCenterRepository.CreateWorkCenter(WorkCenterInfo, systemOperator, Validate, Level);
	// 	if (!Validate)
	// 	{
	// 		ObjWorkCenter = await _workCenterRepository.GetWorkCenter(WorkCenterInfo.Code);
	// 		if (NotifyOnce)
	// 		{
	// 			_ = await SaveImageEntity("Workcenter", WorkCenterInfo.Image, WorkCenterInfo.Code, systemOperator).ConfigureAwait(false);
	// 			if (WorkCenterInfo.AttachmentIds is not null)
	// 			{
	// 				foreach (string attachment in WorkCenterInfo.AttachmentIds)
	// 				{
	// 					await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
	// 				}
	// 			}
	// 			if (WorkCenterInfo.Activities?.Count > 0)
	// 			{
	// 				foreach (Activity activity in WorkCenterInfo.Activities)
	// 				{
	// 					if (string.IsNullOrEmpty(activity.Id))
	// 					{
	// 						Activity newActivity = await CreateActivity(activity, systemOperator).ConfigureAwait(false);
	// 					}
	// 					else if (activity.ManualDelete)
	// 					{
	// 						bool tempResult = await DeleteActivity(activity, systemOperator).ConfigureAwait(false);
	// 					}
	// 					else
	// 					{
	// 						if (activity.ActivityClassId > 0)
	// 						{
	// 							_ = await UpdateActivity(activity, systemOperator).ConfigureAwait(false);
	// 						}
	// 					}
	// 				}
	// 			}
	// 			if (WorkCenterInfo.Shift?.CodeShift is not null
	// 							 && !string.IsNullOrEmpty(WorkCenterInfo.Shift.CodeShift))
	// 			{
	// 				WorkCenterInfo.Shift.Validation = false;
	// 				WorkCenterInfo.Shift.IdAsset = WorkCenterInfo.Code;
	// 				_ = UpdateSchedulingCalendarShifts(WorkCenterInfo.Shift, systemOperator);
	// 			}
	// 			if (WorkCenterInfo.ShiftDelete?.Id is not null
	// 													 && !string.IsNullOrEmpty(WorkCenterInfo.ShiftDelete.Id))
	// 			{
	// 				WorkCenterInfo.ShiftDelete.Validation = false;
	// 				_ = DeleteSchedulingCalendarShifts(WorkCenterInfo.ShiftDelete, systemOperator);
	// 			}
	// 			_ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Workcenter, returnValue.Action, Data = ObjWorkCenter }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
	// 		}
	// 		await ObjWorkCenter.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
	// 	}
	// 	return returnValue;
	// }

	public async Task<bool> UpdateWorkCenter(WorkCenter WorkCenterInfo, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_WORKCENTER_MANAGE))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return await _workCenterRepository.UpdateWorkCenter(WorkCenterInfo, systemOperator);
	}

	public async Task<bool> DeleteWorkCenter(WorkCenter WorkCenterInfo, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.ASS_WORKCENTER_MANAGE))
		{
			throw new UnauthorizedAccessException(NotificationSettings.noPermission);
		}

		#endregion Permission validation

		return await _workCenterRepository.DeleteWorkCenter(WorkCenterInfo, systemOperator);
	}

	#endregion WorkCenter
}
