using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using EWP.SF.Common.Models;
using EWP.SF.Common.Constants;

namespace EWP.SF.Item.BusinessLayer;

public class ToolOperation : IToolOperation
{
    private readonly IToolRepo _toolRepo;

    private readonly ICatalogRepo _catalogRepo;
    private readonly IApplicationSettings _applicationSettings;

    private readonly IProcessTypeOperation _processTypeOperation;

    private readonly IAttachmentOperation _attachmentOperation;

    public ToolOperation(IToolRepo toolRepo, ICatalogRepo catalogRepo, IApplicationSettings applicationSettings
    , IProcessTypeOperation processTypeOperation, IAttachmentOperation attachmentOperation)
    {

        _toolRepo = toolRepo;
        _catalogRepo = catalogRepo;
        _applicationSettings = applicationSettings;
        _processTypeOperation = processTypeOperation;
        _attachmentOperation = attachmentOperation;
    }
    /// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateToolType(List<ToolTypeExternal> toolTypeList, List<ToolTypeExternal> toolTypeListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        List<ResponseData> returnValue = [];
        List<ProcedureExternal> proceduresExternal = [];
        List<CatProfile> listProfiles = _catalogRepo.GetCatalogProfile();
        ResponseData MessageError;
        bool NotifyOnce = true;
        string BaseId = string.Empty;
        List<ProcessType> ptypes = _processTypeOperation.GetProcessTypes(null, systemOperator, false);
        if (toolTypeList?.Count > 0)
        {
            NotifyOnce = toolTypeList.Count == 1;
            int Line = 0;
            foreach (ToolTypeExternal cycleToolType in toolTypeList)
            {
                ToolTypeExternal toolType = cycleToolType;
                Line++;
                try
                {
                    ToolType originalToolType = ListToolTypes(toolType.ToolingTypeCode)?.Find(x => x.Status != Status.Failed);
                    bool editMode = originalToolType is not null;

                    if (editMode && toolTypeListOriginal is not null)
                    {
                        toolType = toolTypeListOriginal.Find(x => x.ToolingTypeCode == cycleToolType.ToolingTypeCode);
                        toolType ??= cycleToolType;
                    }
                    BaseId = toolType.ToolingTypeCode;
                    List<ValidationResult> results = [];
                    ValidationContext context = new(toolType, null, null);
                    if (!Validator.TryValidateObject(toolType, context, results))
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
                    string name = toolType.ToolingTypeCode;

                    if (!editMode || !string.IsNullOrEmpty(toolType.ToolingTypeName))
                    {
                        name = toolType.ToolingTypeName;
                    }

                    if (!editMode || !string.IsNullOrEmpty(toolType.OperationType))
                    {
                        if (ptypes.Find(p => string.Equals(p.Id, toolType.OperationType, StringComparison.OrdinalIgnoreCase)) is null)
                        {
                            throw new Exception($"Operation type code is not valid: {toolType.OperationType}");
                        }
                    }
                    else if (!editMode && string.IsNullOrEmpty(toolType.OperationType))
                    {
                        throw new Exception($"Operation type is required");
                    }

                    switch (toolType?.Status.ToStr().ToUpperInvariant())
                    {
                        case "ACTIVE":
                            status = Status.Active;
                            break;

                        case "DISABLE":
                            status = Status.Disabled;
                            break;
                    }
                    if (!editMode && status == Status.Disabled)
                    {
                        throw new Exception("Cannot import a new disabled Tooling Type");
                    }
                    short level = 0;
                    if (!string.IsNullOrEmpty(toolType.ScheduleLevel))
                    {
                        switch (toolType?.ScheduleLevel.ToStr().ToUpperInvariant())
                        {
                            case "PRIMARY":
                                level = 1;
                                break;

                            case "SECONDARY":
                                level = 2;
                                break;
                        }
                    }
                    ToolType toolTypeInfo = null;
                    if (!editMode)
                    {
                        toolTypeInfo = new ToolType
                        {
                            Code = toolType.ToolingTypeCode,
                            Name = name,
                            Status = status,
                            ProcessTypeId = toolType.OperationType,
                            Scheduling = new ToolType_Scheduling
                            {
                                Schedule = toolType.Schedule.ToStr().Equals("yes", StringComparison.OrdinalIgnoreCase),
                                ScheduleLevel = level
                            }
                        };
                    }
                    else
                    {
                        toolTypeInfo = originalToolType;
                        if (!string.IsNullOrEmpty(toolType.ToolingTypeName))
                        {
                            toolTypeInfo.Name = name;
                        }
                        if (!string.IsNullOrEmpty(toolType.Status))
                        {
                            toolTypeInfo.Status = status;
                        }
                        if (string.IsNullOrEmpty(toolType.OperationType))
                        {
                            toolTypeInfo.ProcessTypeId = originalToolType.ProcessTypeId;
                        }
                        if (level > 0 && toolTypeInfo.Scheduling is not null)
                        {
                            toolTypeInfo.Scheduling.ScheduleLevel = level;
                        }
                        if (!string.IsNullOrEmpty(toolType.Schedule))
                        {
                            toolTypeInfo.Scheduling.Schedule = toolType.Schedule.Equals("yes", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    // ProcessTypeResponse resp = _toolRepo.MergeProcessType(operationTypeInfo, systemOperator, Validate, Level); // Broker
                    ResponseData resp = await CreateToolType(toolTypeInfo, systemOperator, Validate, true).ConfigureAwait(false);
                    returnValue.Add(resp);
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
	public async Task<ResponseData> CreateToolType(ToolType toolTypeInfo, User systemOperator
        , bool Validate = false, bool NotifyOnce = true)
    {
        ResponseData returnValue;
        ToolType tooltype = null;

        #region Permission validation

        if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_TOOLTYPE_MANAGE))
        {
        	throw new UnauthorizedAccessException(ErrorMessage.noPermission);
        }

        #endregion Permission validation

        returnValue = _toolRepo.CreateToolType(toolTypeInfo, systemOperator, Validate);
        if (returnValue.IsSuccess)
        {
            tooltype = ListToolTypes(returnValue.Code).Find(static x => x.Status != Status.Failed);
        }
        if (!Validate && tooltype is not null)
        {
            await tooltype.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
            if (NotifyOnce)
            {
                await _attachmentOperation.SaveImageEntity("ToolingType", toolTypeInfo.Image, toolTypeInfo.Code, systemOperator).ConfigureAwait(false);
                if (toolTypeInfo.AttachmentIds is not null)
                {
                    foreach (string attachment in toolTypeInfo.AttachmentIds)
                    {
                        await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
                    }
                }
                // Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged
                // 	, new { Catalog = Entities.ToolType, returnValue.Action, Data = tooltype }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
            }
        }
        return returnValue;
    }

    public List<ToolType> ListToolTypes(string ToolTypeCode, DateTime? DeltaDate = null) => _toolRepo.ListToolType(ToolTypeCode, DeltaDate);
    
    /// <summary>
	///
	/// </summary>
	public List<Tool> ListTools(string ToolCode = "", DateTime? DeltaDate = null) => _toolRepo.ListTools(ToolCode, DeltaDate);
    /// <summary>
    ///
    /// </summary>

}