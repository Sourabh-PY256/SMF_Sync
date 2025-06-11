
using EWP.SF.Common.Constants;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

namespace EWP.SF.KafkaSync.BusinessLayer;
public class ProductionLinesOperation : IProductionLinesOperation
{
    private readonly IProductionLinesRepo _productionLinesRepo;

	public ProductionLinesOperation(IProductionLinesRepo productionLinesRepo)
	{ _productionLinesRepo = productionLinesRepo;
        
    }
	#region Production Lines

		/// <summary>
		///
		/// </summary>
	public ProductionLine[] ListProductionLines(bool deleted = false, DateTime? DeltaDate = null)
	{
		List<ProductionLine> lines = _productionLinesRepo.ListProductionLines(DeltaDate);

		return lines?.Where(d => (deleted && d.Status == Status.Deleted) || (!deleted && d.Status != Status.Deleted)).ToArray();
	}

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public ProductionLine GetProductionLine(string Code, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.CP_PRODUCTIONLINE_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return _productionLinesRepo.GetProductionLine(Code);
	}

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	// public async Task<ResponseData> CreateProductionLine(ProductionLine productionLineInfo, User systemOperator
	//   , bool Validate = false, string Level = "Success", bool NotifyOnce = true)
	// {
	// 	ResponseData returnValue = new();

	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.CP_PRODUCTIONLINE_CREATE))
	// 	{
	// 		throw new UnauthorizedAccessException(noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	returnValue = BrokerDAL.CreateProductionLine(productionLineInfo, systemOperator, Validate, Level);
	// 	if (!Validate)
	// 	{
	// 		ProductionLine ObjProductionLine = BrokerDAL.GetProductionLine(productionLineInfo.Code);
	// 		await ObjProductionLine.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
	// 		Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductionLine, returnValue.Action, Data = ObjProductionLine });
	// 		if (NotifyOnce)
	// 		{
	// 			await SaveImageEntity("ProductionLine", productionLineInfo.Image, productionLineInfo.Code, systemOperator).ConfigureAwait(false);
	// 			if (productionLineInfo.AttachmentIds is not null)
	// 			{
	// 				foreach (string attachment in productionLineInfo.AttachmentIds)
	// 				{
	// 					await AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
	// 				}
	// 			}

	// 			if (productionLineInfo.Activities?.Count > 0)
	// 			{
	// 				foreach (Activity activity in productionLineInfo.Activities)
	// 				{
	// 					if (string.IsNullOrEmpty(activity.Id))
	// 					{
	// 						Activity newActivity = await CreateActivity(activity, systemOperator).ConfigureAwait(false);
	// 					}
	// 					else if (activity.ManualDelete)
	// 					{
	// 						bool tempResult = await DeleteActivity(activity, systemOperator).ConfigureAwait(false);
	// 					}
	// 					else if (activity.ActivityClassId > 0)
	// 					{
	// 						await UpdateActivity(activity, systemOperator).ConfigureAwait(false);
	// 					}
	// 				}
	// 			}
	// 			if (productionLineInfo.Shift?.CodeShift is not null
	// 			&& !string.IsNullOrEmpty(productionLineInfo.Shift.CodeShift))
	// 			{
	// 				productionLineInfo.Shift.Validation = false;
	// 				productionLineInfo.Shift.IdAsset = productionLineInfo.Code;
	// 				_ = UpdateSchedulingCalendarShifts(productionLineInfo.Shift, systemOperator);
	// 			}
	// 			if (productionLineInfo.ShiftDelete?.Id is not null
	// 				 && !string.IsNullOrEmpty(productionLineInfo.ShiftDelete.Id))
	// 			{
	// 				productionLineInfo.ShiftDelete.Validation = false;
	// 				_ = DeleteSchedulingCalendarShifts(productionLineInfo.ShiftDelete, systemOperator);
	// 			}

	// 			// Services.ServiceManager.sendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductionLine, Action = returnValue.Action, Data = ObjProductionLine }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
	// 		}
	// 	}

	// 	return returnValue;
	// }

	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public bool DeleteProductionLine(ProductionLine productionLineInfo, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.CP_PRODUCTIONLINE_DELETE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return _productionLinesRepo.DeleteProductionLine(productionLineInfo, systemOperator);
	}

	#endregion Production Lines
}
