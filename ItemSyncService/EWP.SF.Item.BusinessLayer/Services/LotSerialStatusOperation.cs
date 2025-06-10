using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using EWP.SF.Common.Models;
using EWP.SF.Common.Constants;

namespace EWP.SF.Item.BusinessLayer;

public class LotSerialStatusOperation : ILotSerialStatusOperation
{
    private readonly ILotSerialStatusRepo _lotSerialStatusRepo;

    private readonly IAttachmentOperation _attachmentOperation;

    public LotSerialStatusOperation(ILotSerialStatusRepo lotSerialStatusRepo, IApplicationSettings applicationSettings
    ,IAttachmentOperation attachmentOperation)
    {
        _lotSerialStatusRepo = lotSerialStatusRepo;
        _attachmentOperation = attachmentOperation;
    }

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public List<LotSerialStatus> ListLotSerialStatus(User systemOperator, string LotSerialStatusCode = "", DateTime? DeltaDate = null)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return _lotSerialStatusRepo.ListLotSerialStatus(LotSerialStatusCode, DeltaDate);
	}

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<ResponseData> MergeLotSerialStatus(LotSerialStatus LotSerialStatusInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		ResponseData returnValue = null;

		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		// Warehouse returnValue = BrokerDAL.CreateWarehouse(WarehouseInfo, systemOperator);
		returnValue = _lotSerialStatusRepo.MergeLotSerialStatus(LotSerialStatusInfo, systemOperator, Validate);
		if (!Validate && returnValue is not null)
		{
			LotSerialStatus ObjLotSerialStatus = ListLotSerialStatus(systemOperator, returnValue.Code).Find(static x => x.Status != Status.Failed);
			await ObjLotSerialStatus.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
			await _attachmentOperation.SaveImageEntity("LotSerialStatus", LotSerialStatusInfo.Image, LotSerialStatusInfo.Code, systemOperator).ConfigureAwait(false);
			if (LotSerialStatusInfo.AttachmentIds is not null)
			{
				foreach (string attachment in LotSerialStatusInfo.AttachmentIds)
				{
					await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
				}
			}
			// if (NotifyOnce)
			// {
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.LotSerialStatus, returnValue.Action, Data = ObjLotSerialStatus }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
			// 	returnValue.Entity = ObjLotSerialStatus;
			// }
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateLotSerialStatus(List<LotSerialStatusExternal> lotSerialStatusList, List<LotSerialStatusExternal> lotSerialStatusListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (lotSerialStatusList?.Count > 0)
		{
			NotifyOnce = lotSerialStatusList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (LotSerialStatusExternal cycleLotSerialStatus in lotSerialStatusList)
			{
				LotSerialStatusExternal lotSerialStatus = cycleLotSerialStatus;
				Line++;
				try
				{
					BaseId = lotSerialStatus.LotSerialStatusCode;

					LotSerialStatus originalSerialStatus = ListLotSerialStatus(systemOperator, cycleLotSerialStatus.LotSerialStatusCode)?.Find(x => x.Status != Status.Failed);
					bool editMode = originalSerialStatus is not null;
					if (editMode && lotSerialStatusListOriginal is not null)
					{
						lotSerialStatus = lotSerialStatusListOriginal?.Find(x => x.LotSerialStatusCode == cycleLotSerialStatus.LotSerialStatusCode);
						lotSerialStatus ??= cycleLotSerialStatus;
					}
					List<ValidationResult> results = [];
					ValidationContext context = new(lotSerialStatus, null, null);
					if (!Validator.TryValidateObject(lotSerialStatus, context, results))
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
					if (!editMode || !string.IsNullOrEmpty(lotSerialStatus.Status))
					{
						if (string.Equals(lotSerialStatus.Status.Trim(), "DISABLE", StringComparison.OrdinalIgnoreCase))
						{
							status = (Status)Status.Disabled.ToInt32();
						}
					}
					bool allowIssue = true;
					if (!editMode || !string.IsNullOrEmpty(lotSerialStatus.AllowIssue))
					{
						if (!string.Equals(lotSerialStatus.AllowIssue, "YES", StringComparison.OrdinalIgnoreCase))
						{
							allowIssue = false;
						}
					}
					LotSerialStatus lotSerialStatusInfo = new()
					{
						Code = lotSerialStatus.LotSerialStatusCode,
						Name = !string.IsNullOrEmpty(lotSerialStatus.LotSerialStatusName) ? lotSerialStatus.LotSerialStatusName : lotSerialStatus.LotSerialStatusCode,
						Status = status,
						AllowIssue = allowIssue
					};
					ResponseData response = await MergeLotSerialStatus(lotSerialStatusInfo, systemOperator, Validate).ConfigureAwait(false);
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
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.LotSerialStatus, Action = ActionDB.IntegrateAll.ToStr() });
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
