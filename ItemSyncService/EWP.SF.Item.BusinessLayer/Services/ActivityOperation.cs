using EWP.SF.Common.Models;
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
public class ActivityOperation : IActivityOperation
{
    private readonly IActivityRepo _activityRepo;
    private readonly IApplicationSettings _applicationSettings;

    public ActivityOperation(IActivityRepo _activityRepo, IApplicationSettings applicationSettings)
    {
        _activityRepo = activityRepo;
        _applicationSettings = applicationSettings;
    }
    #region Activity
    
    public async Task<Activity> CreateActivity(Activity activityInfo, User systemOperator)
    {
        Activity returnValue = null;
        XmlSerializer xser = null;
        MessageBroker callback = null;
        MemoryStream ms = null;
        if (!string.IsNullOrEmpty(activityInfo.IncludedAssets))
        {
            List<AssetsTree> includedAssetsJson =
            [
                .. activityInfo.IncludedAssets.Split(',').Select(q => new AssetsTree
                    {
                        AssetCode = q.Substring(q.IndexOf('-') + 1, q.Length - q[..q.IndexOf('-')].Length - 1),
                        AssetTypeCode = q[..q[..q.IndexOf('-')].Length]
                    }),
                ];

            if (includedAssetsJson is not null)
            {
                activityInfo.IncludedAssets = JsonConvert.SerializeObject(includedAssetsJson);
            }
        }
        if (activityInfo.CurrentProcessMaster is not null
            && activityInfo.RequiresInstructions && activityInfo.CurrentProcessMaster.IsNewProcess && !string.IsNullOrEmpty(activityInfo.CurrentProcessMaster.ProcedureId))
        {
            activityInfo.CurrentProcessMaster.IsManualActivity = true;
            string jsonString = JsonConvert.SerializeObject(activityInfo.CurrentProcessMaster);
            Procedure ProcedureTmp = JsonConvert.DeserializeObject<Procedure>(jsonString);
            Common.ResponseModels.ResponseData ResultProcedure = await ProcessMasterInsByXML(ProcedureTmp, systemOperator).ConfigureAwait(false);
            //var ResultProcedure =ProcessMasterInsByXML(activityInfo.CurrentProcessMaster, systemOperator);
            //Fix Procedures222
            if (ResultProcedure.IsSuccess)
            {
                activityInfo.ParentId = ResultProcedure.Id;
            }
        }

        returnValue = _activityRepo.CreateActivity(activityInfo, systemOperator);
        if (!string.IsNullOrEmpty(activityInfo.Image))
        {
            _ = await SaveImageEntity("Activity", activityInfo.Image, activityInfo.Id, systemOperator).ConfigureAwait(false);
        }
        if (activityInfo.AttachmentIds is not null)
        {
            foreach (string attachmentId in activityInfo.AttachmentIds)
            {
                await AttachmentSync(attachmentId, returnValue.Id, systemOperator).ConfigureAwait(false);
            }
        }
        if (activityInfo.CurrentProcessMaster?.ProcedureId is not null
            && activityInfo.RequiresInstructions)
        {
            List<ComponentInstruction> listComponents = [];
            foreach (ProcedureSection section in activityInfo.CurrentProcessMaster.Sections)
            {
                foreach (ProcedureMasterInstruction instruction in section.ListInstrucctions)
                {
                    if (instruction.Components?.Count > 0)
                    {
                        foreach (ComponentInstruction component in instruction.Components)
                        {
                            component.InstructionId = instruction.InstructionId;
                            component.ActivityId = activityInfo.Id;
                            listComponents.Add(component);
                            if (component.AttachmentId is not null
                               && !string.IsNullOrEmpty(component.AttachmentId))
                            {
                                _ = await AttachmentSync(component.AttachmentId, component.Id, systemOperator).ConfigureAwait(false);
                            }
                        }
                        instruction.Components = null;
                    }
                }
            }
            string xmlComponents = string.Empty;

            if (listComponents.Count > 0)
            {
                ms = new MemoryStream();
                xser = new XmlSerializer(typeof(List<ComponentInstruction>));
                xser.Serialize(ms, listComponents);
                xmlComponents = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                bool resultItems = _activityRepo.ActivityItemInsByXML(systemOperator, xmlComponents);
            }
        }

        //await activityInfo.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(returnValue.Id))
        {
            ActivityInstanceCalculateRequest activityInfoSchedule = new()
            {
                ActivityId = activityInfo.Id,
                RecurrenceCode = activityInfo.Schedule.RecurrenceCode,
                OptionsEndCode = activityInfo.Schedule.OptionsEndCode,
                DailyCode = activityInfo.Schedule.DailyCode,
                StartDate = activityInfo.Schedule.StartDate,
                EndDate = activityInfo.Schedule.EndDate,
                DurationInSec = activityInfo.Schedule.DurationInSec,
                DailyDays = activityInfo.Schedule.DailyDays,
                NoWeeks = activityInfo.Schedule.NoWeeks,
                OptionsWeekly = activityInfo.Schedule.OptionsWeekly,
                Name = activityInfo.Name,
                OneTime = activityInfo.Schedule.OneTime,
                IsCreateActivity = true,
                MonthlyOptionsCode = activityInfo.Schedule.MonthlyOptionsCode,
                MonthlyEvery = activityInfo.Schedule.MonthlyEvery,
                MonthlyDay = activityInfo.Schedule.MonthlyDay,
                MonthlyDayCode = activityInfo.Schedule.MonthlyDayCode,
                MonthlyOrderDaysCode = activityInfo.Schedule.MonthlyOrderDaysCode,
                Occurrences = activityInfo.Schedule.Occurrences,
                MonthlyByYearly = activityInfo.Schedule.MonthlyByYearly,
                YearlyOptionsCode = activityInfo.Schedule.YearlyOptionsCode,
                EveryYear = activityInfo.Schedule.EveryYear,
                EveryHour = activityInfo.Schedule.EveryHour
            };

            if (!string.Equals(activityInfo.Origin, "PRODUCT", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(activityInfo.Origin, "ORDER", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(activityInfo.Origin, "OPERATIONTYPE", StringComparison.OrdinalIgnoreCase))
            {
                List<ActivityInstanceCalculateResponse> tmpListInstance = _activityRepo.ActivityInstanceCalculate(activityInfoSchedule, systemOperator);
                if (tmpListInstance is not null)
                {
                    returnValue.ListInstaceResponse = [];
                    returnValue.ListInstaceResponse = tmpListInstance;
                }
            }
            else
            {
                callback = _activityRepo.ActivityMergeSchedule(activityInfo, systemOperator);
            }
            //  callback = this._activityRepo.ActivityMergeSchedule(activityInfo, systemOperator);
            //  callback = this._activityRepo.ActivityMergeSchedule(activityInfo, systemOperator);
            activityInfo.Schedule.ActivityId = activityInfo.Id;
            //await activityInfo.Schedule.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);

            // if (callback is not null)
            // {
            //     Services.SyncInitializer.ForcePush(callback);
            // }discuss mario
        }
        return returnValue;
    }

    public async Task<Activity> UpdateActivity(Activity activityInfo, User systemOperator)
    {
        Activity returnValue = new();
        XmlSerializer xser = null;
        MemoryStream ms = null;
        if (!string.IsNullOrEmpty(activityInfo.IncludedAssets))
        {
            List<AssetsTree> includedAssetsJson =
            [
                .. activityInfo.IncludedAssets.Split(',').Select(q => new AssetsTree
                    {
                        AssetCode = q.Substring(q.IndexOf('-') + 1, q.Length - q[..q.IndexOf('-')].Length - 1),
                        AssetTypeCode = q[..q[..q.IndexOf('-')].Length]
                    }),
                ];

            if (includedAssetsJson is not null)
            {
                activityInfo.IncludedAssets = JsonConvert.SerializeObject(includedAssetsJson);
            }
        }
        if (activityInfo.CurrentProcessMaster is not null
                      && activityInfo.RequiresInstructions && (activityInfo.CurrentProcessMaster.IsNewProcess || activityInfo.CurrentProcessMaster.IsManualActivity)
                      && !string.IsNullOrEmpty(activityInfo.CurrentProcessMaster.ProcedureId))
        {
            //activityInfo.CurrentProcessMaster.IsManualActivity = true;
            //Procedure ProcedureTmp = (Procedure)activityInfo.CurrentProcessMaster.Clone();

            string jsonString = JsonConvert.SerializeObject(activityInfo.CurrentProcessMaster);
            Procedure ProcedureTmp = JsonConvert.DeserializeObject<Procedure>(jsonString);

            Common.ResponseModels.ResponseData ResultProcedure = await ProcessMasterInsByXML(ProcedureTmp, systemOperator).ConfigureAwait(false);
            if (ResultProcedure.IsSuccess)
            {
                activityInfo.ParentId = ResultProcedure.Id;
            }
        }
        if (activityInfo.CurrentProcessMaster is not null
            && !string.IsNullOrEmpty(activityInfo.CurrentProcessMaster.ProcedureId)
            && activityInfo.CurrentProcessMaster.ProcedureId is not null
            && activityInfo.RequiresInstructions)
        {
            List<ComponentInstruction> listComponents = [];
            foreach (ProcedureSection section in activityInfo.CurrentProcessMaster.Sections)
            {
                foreach (ProcedureMasterInstruction instruction in section.ListInstrucctions)
                {
                    if (instruction.Components?.Count > 0)
                    {
                        foreach (ComponentInstruction component in instruction.Components)
                        {
                            component.InstructionId = instruction.InstructionId;
                            component.ActivityId = activityInfo.Id;
                            listComponents.Add(component);
                            if (component.AttachmentId is not null
                               && !string.IsNullOrEmpty(component.AttachmentId))
                            {
                                _ = await AttachmentSync(component.AttachmentId, component.Id, systemOperator).ConfigureAwait(false);
                            }
                        }
                        instruction.Components = null;
                    }
                }
            }
            string xmlComponents = string.Empty;
            if (listComponents.Count > 0)
            {
                ms = new MemoryStream();
                xser = new XmlSerializer(typeof(List<ComponentInstruction>));
                xser.Serialize(ms, listComponents);
                xmlComponents = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");

                bool resultItems = _activityRepo.ActivityItemInsByXML(systemOperator, xmlComponents);
            }
        }
        if (activityInfo.EditSeries || activityInfo.Schedule.OneTime)
        {
            if (_activityRepo.UpdateActivity(activityInfo, systemOperator))
            {
                returnValue = activityInfo;
            }
        }
        if (!string.IsNullOrEmpty(activityInfo.Image))
        {
            _ = await SaveImageEntity("Activity", activityInfo.Image, activityInfo.Id, systemOperator).ConfigureAwait(false);
        }

        if (activityInfo.AttachmentIds is not null)
        {
            foreach (string attachmentId in activityInfo.AttachmentIds)
            {
                await AttachmentSync(attachmentId, activityInfo.Id, systemOperator).ConfigureAwait(false);
            }
        }
        //await activityInfo.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);
        //XmlSerializer xser = null;
        //MemoryStream ms = null;
        MessageBroker callback = null;
        if (activityInfo.Schedule is not null)
        {
            ActivityInstanceCalculateRequest activityInfoSchedule = new()
            {
                ActivityId = activityInfo.Id,
                RecurrenceCode = activityInfo.Schedule.RecurrenceCode,
                OptionsEndCode = activityInfo.Schedule.OptionsEndCode,
                DailyCode = activityInfo.Schedule.DailyCode,
                StartDate = activityInfo.Schedule.StartDate,
                EndDate = activityInfo.Schedule.EndDate,
                DurationInSec = activityInfo.Schedule.DurationInSec,
                DailyDays = activityInfo.Schedule.DailyDays,
                NoWeeks = activityInfo.Schedule.NoWeeks,
                OptionsWeekly = activityInfo.Schedule.OptionsWeekly,
                EditSeries = activityInfo.EditSeries,
                Name = activityInfo.Name,
                OneTime = activityInfo.Schedule.OneTime,
                InstanceId = activityInfo.Schedule.InstanceId,
                MonthlyOptionsCode = activityInfo.Schedule.MonthlyOptionsCode,
                MonthlyEvery = activityInfo.Schedule.MonthlyEvery,
                MonthlyDay = activityInfo.Schedule.MonthlyDay,
                MonthlyDayCode = activityInfo.Schedule.MonthlyDayCode,
                MonthlyOrderDaysCode = activityInfo.Schedule.MonthlyOrderDaysCode,
                Occurrences = activityInfo.Schedule.Occurrences,
                MonthlyByYearly = activityInfo.Schedule.MonthlyByYearly,
                YearlyOptionsCode = activityInfo.Schedule.YearlyOptionsCode,
                EveryYear = activityInfo.Schedule.EveryYear,
                EveryHour = activityInfo.Schedule.EveryHour
            };
            if (!string.Equals(activityInfo.Origin, "PRODUCT", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(activityInfo.Origin, "ORDER", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(activityInfo.Origin, "OPERATIONTYPE", StringComparison.OrdinalIgnoreCase))
            {
                // this._activityRepo.ActivityInstanceCalculate(activityInfoSchedule, systemOperator);
                List<ActivityInstanceCalculateResponse> tmpListInstance = _activityRepo.ActivityInstanceCalculate(activityInfoSchedule, systemOperator);
                if (tmpListInstance is not null)
                {
                    returnValue.ListInstaceResponse = [];
                    returnValue.ListInstaceResponse = tmpListInstance;
                }
            }
            else
            {
                callback = _activityRepo.ActivityMergeSchedule(activityInfo, systemOperator);
            }

            //callback = this._activityRepo.ActivityMergeSchedule(activityInfo, systemOperator);
            activityInfo.Schedule.ActivityId = activityInfo.Id;
            //await activityInfo.Schedule.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);
        }

        // if (callback is not null)
        // {
        //     Services.SyncInitializer.ForcePush(callback);
        // } discuss mario
        return returnValue;
    }

    public async Task<bool> DeleteActivity(Activity activityInfo, User systemOperator)
    {
        bool returnValue = _activityRepo.DeleteActivity(activityInfo, systemOperator);
        //await activityInfo.Log(EntityLogType.Delete, systemOperator).ConfigureAwait(false);
        return returnValue;
    }

    #endregion Activity
}
