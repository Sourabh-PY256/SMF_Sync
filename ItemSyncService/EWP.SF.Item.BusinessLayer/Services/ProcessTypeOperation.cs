using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using Newtonsoft.Json;

namespace EWP.SF.Item.BusinessLayer;
public class ProcessTypeOperation : IProcessTypeOperation
{
    private readonly IProcessTypeRepo _processTypeRepo;

    public ProcessTypeOperation(IProcessTypeRepo processTypeRepo)
    {
        _processTypeRepo = processTypeRepo;
        ;
    }
	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public List<ProcessType> GetProcessTypes(string processType, User systemOperator, bool WithTool = false, DateTime? DeltaDate = null)
	{
		#region Permission validation

		// if (!systemOperator.Permissions.Any(static x => x.Code is Permissions.CP_MACHINE_EDIT or Permissions.CP_PROCESS_TYPE_MANAGE or Permissions.RPT_MACHINEDETAILS_VW))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		return _processTypeRepo.GetProcessType(processType, WithTool, DeltaDate);
	}
	
	
	
	/// <summary>
	///
	/// </summary>
	public List<ResponseData> ListUpdateSuboperationTypes_Bulk(List<SubProcessTypeExternal> clockList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		List<SubProcessTypeExternal> detailsToMerge = [];
		const bool NotifyOnce = false;
		if (clockList?.Count > 0)
		{
			int Line = 0;
			string BaseId = string.Empty;
			SubProcessTypeExternal itemInfo = null;
			clockList.ForEach(cycleDetail =>
			{
				Line++;
				try
				{
					BaseId = cycleDetail.OperationSubtypeCode;

					List<ValidationResult> results = [];
					ValidationContext context = new(cycleDetail, null, null);

					if (!Validator.TryValidateObject(cycleDetail, context, results) && results.Count > 0)
					{
						throw new Exception($"{results[0]}");
					}

					//Validate data;
					if (string.IsNullOrEmpty(cycleDetail.OperationSubtypeCode))
					{
						throw new Exception("Suboperation type code is required");
					}
					if (!string.IsNullOrEmpty(cycleDetail.OperationTypeCode))
					{
						ProcessType opType = (GetProcessTypes(cycleDetail.OperationTypeCode, systemOperator, false)?.FirstOrDefault()) ?? throw new Exception("Operation type code is invalid");
					}

					if (string.IsNullOrEmpty(cycleDetail.OperationSubtypeName))
					{
						cycleDetail.OperationSubtypeName = cycleDetail.OperationSubtypeName;
					}

					detailsToMerge.Add(cycleDetail);
					ResponseData response = new()
					{
						Code = cycleDetail.OperationSubtypeCode,
						Action = ActionDB.IntegrateAll,
						Entity = cycleDetail,
						EntityAlt = itemInfo,
						IsSuccess = true,
						Id = cycleDetail.OperationSubtypeCode
					};
					returnValue.Add(response);
				}
				catch (Exception ex)
				{
					MessageError = new ResponseData
					{
						Id = BaseId,
						Message = ex.Message
					};
					if (string.IsNullOrEmpty(cycleDetail.OperationSubtypeCode))
					{
						MessageError.Code = "Line:" + Line.ToStr();
					}
					else
					{
						MessageError.Code = cycleDetail.OperationSubtypeCode;
					}
					MessageError.Entity = cycleDetail;
					MessageError.EntityAlt = itemInfo;
					returnValue.Add(MessageError);
				}
			});

			string itemmsJson = JsonConvert.SerializeObject(detailsToMerge, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			ResponseData result = _processTypeRepo.SaveSubOperationTypes_Bulk(itemmsJson, systemOperator);
		}
		if (!Validate)
		{
			// if (!NotifyOnce)
			// {
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Item, Action = ActionDB.IntegrateAll.ToStr() });
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
/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public List<ProcessTypeDetail> ListMachineProcessTypeDetails(string machineId, User systemOperator)
	{
		#region Permission validation

		// if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.CP_MACHINE_EDIT))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		return _processTypeRepo.ListMachineProcessTypeDetails(machineId);
	}
	
}
