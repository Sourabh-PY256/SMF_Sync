using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using EWP.SF.Common.Models;
using EWP.SF.Common.Constants;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class MeasureUnitOperation : IMeasureUnitOperation
{
    private readonly IMeasureUnitRepo _measureUnitRepo;

    private readonly IAttachmentOperation _attachmentOperation;

    public MeasureUnitOperation(IMeasureUnitRepo measureUnitRepo,IAttachmentOperation attachmentOperation)
    {
        _measureUnitRepo = measureUnitRepo;
        _attachmentOperation = attachmentOperation;
    }

	/// <summary>
	///
	/// </summary>
	public List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null) => _measureUnitRepo.GetMeasureUnits(unitType, unitId, DeltaDate);

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<ResponseData> MergeUnitMeasure(MeasureUnit measureUnitInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		ResponseData returnValue = null;

		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_MEASUREUNIT_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		returnValue = _measureUnitRepo.MergeUnitMeasure(measureUnitInfo, systemOperator, Validate);
		if (!Validate && returnValue is not null)
		{
			MeasureUnit ObjMeasureUnit = GetMeasureUnits(null, returnValue.Code).Find(static x => x.Status != Status.Failed);
			await ObjMeasureUnit.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
			if (NotifyOnce)
			{
				await _attachmentOperation.SaveImageEntity("UnitMeasure", measureUnitInfo.Image, measureUnitInfo.Code, systemOperator).ConfigureAwait(false);
				if (measureUnitInfo.AttachmentIds is not null)
				{
					foreach (string attachment in measureUnitInfo.AttachmentIds)
					{
						await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
					}
				}
				// Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.MeasureUnit, returnValue.Action, Data = ObjMeasureUnit }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
				// returnValue.Entity = ObjMeasureUnit;
			}
		}
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateUnitMeasure(List<MeasureUnitExternal> measureUnitInfoList, List<MeasureUnitExternal> measureUnitInfoListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (measureUnitInfoList?.Count > 0)
		{
			NotifyOnce = measureUnitInfoList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (MeasureUnitExternal cycleMeasureUnit in measureUnitInfoList)
			{
				MeasureUnitExternal measureUnit = cycleMeasureUnit;
				Line++;
				try
				{
					BaseId = measureUnit.UoMCode;
					MeasureUnit originalUnit = GetMeasureUnits(null, cycleMeasureUnit.UoMCode)?.Find(x => x.Status != Status.Failed);
					bool editMode = originalUnit is not null;
					if (editMode && measureUnitInfoListOriginal is not null)
					{
						measureUnit = measureUnitInfoListOriginal.Find(x => x.UoMCode == cycleMeasureUnit.UoMCode);
						measureUnit ??= cycleMeasureUnit;
					}
					List<ValidationResult> results = [];
					ValidationContext context = new(measureUnit, null, null);
					if (!Validator.TryValidateObject(measureUnit, context, results))
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
					// Status
					Status status = (Status)Status.Active.ToInt32();
					if (!editMode || !string.IsNullOrEmpty(measureUnit.Status))
					{
						if (string.Equals(measureUnit.Status.Trim(), "DISABLE", StringComparison.OrdinalIgnoreCase))
						{
							status = (Status)Status.Disabled.ToInt32();
						}
					}
					if (!editMode && status == Status.Disabled)
					{
						throw new Exception("Cannot import a new disabled measure unit");
					}
					bool isProduction = false;
					if (!editMode || !string.IsNullOrEmpty(measureUnit.IsInventory))
					{
						isProduction = string.Equals(measureUnit.IsInventory, "YES", StringComparison.OrdinalIgnoreCase);
					}
					// UnitType
					UnitType unitTypeParsed = 0;
					if (!editMode || !string.IsNullOrEmpty(measureUnit.UnitType))
					{
						bool v = Enum.TryParse(measureUnit.UnitType, out unitTypeParsed);
					}
					// Data Import
					MeasureUnit measureUnitInfo = new()
					{
						Code = measureUnit.UoMCode,
						Name = !string.IsNullOrEmpty(measureUnit.UoMName) ? measureUnit.UoMName : measureUnit.UoMCode,
						Status = status,
						IsProductionResult = isProduction,
						Type = unitTypeParsed
					};

					if (!editMode || measureUnit.Factor.HasValue)
					{
						measureUnitInfo.Factor = (decimal)measureUnit.Factor.Value;
					}
					if (editMode)
					{
						if (string.IsNullOrEmpty(measureUnit.UnitType))
						{
							measureUnitInfo.Type = originalUnit.Type;
						}

						measureUnitInfo.IsProductionResult = originalUnit.IsProductionResult;

						if (!measureUnit.Factor.HasValue)
						{
							measureUnitInfo.Factor = originalUnit.Factor;
						}
					}
					ResponseData response = await MergeUnitMeasure(measureUnitInfo, systemOperator, Validate).ConfigureAwait(false);
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
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.MeasureUnit, Action = ActionDB.IntegrateAll.ToStr() });
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
