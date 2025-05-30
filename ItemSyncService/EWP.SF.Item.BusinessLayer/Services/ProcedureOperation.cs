using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Text;
using Range = EWP.SF.Common.Models.Range;
using SixLabors.ImageSharp;
namespace EWP.SF.Item.BusinessLayer;

public class ProcedureOperation : IProcedureOperation
{
    private readonly IProcedureRepo _procedureRepo;
    private readonly IApplicationSettings _applicationSettings;

    private readonly IAttachmentOperation _attachmentOperation;

    public ProcedureOperation(IProcedureRepo procedureRepo, IApplicationSettings applicationSettings
    , IAttachmentOperation attachmentOperation)
    {
        _procedureRepo = procedureRepo;
        _applicationSettings = applicationSettings;
        _attachmentOperation = attachmentOperation;
    }


    #region ProcessMaster
    public async Task<ResponseData> ProcessMasterInsByXML(
        Procedure processMasterinfo,
        User systemOperator,
        bool Validate = false,
        bool NotifyOnce = true)
    {
        string returnValueProcess = "";
        ResponseData returnValue = null;
        //int saveVersionProcess = 0;
        if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate || processMasterinfo.IsProcessNewActivity || processMasterinfo.CreateVersionSync)
        {
            processMasterinfo.ProcedureId = Guid.NewGuid().ToStr();
            if (processMasterinfo.IsProcessNewActivity || processMasterinfo.IsDuplicate || processMasterinfo.CreateVersionSync)
            {
                processMasterinfo.ProcedureIdOrigin = processMasterinfo.ProcedureId;
            }
        }
        else if (processMasterinfo.Version == 1)
        {
            processMasterinfo.ProcedureIdOrigin = processMasterinfo.ProcedureId;
        }

        returnValueProcess = processMasterinfo.ProcedureId;
        returnValue = _procedureRepo.ProcessMasterIns(processMasterinfo, systemOperator);
        XmlSerializer xser = null;
        MemoryStream ms = null;
        try
        {
            if (!string.IsNullOrEmpty(processMasterinfo.ProcedureId) && returnValue.IsSuccess && processMasterinfo.Sections?.Count > 0)
            {
                ms = new MemoryStream();
                xser = new XmlSerializer(typeof(List<ProcedureSection>));
                List<ProcedureMasterInstruction> choices = [];
                List<ActionChoice> listActionCheckBox = [];
                List<Choice> listMultipleChoice = [];
                //List<ActionOperator> listActionOperator = new List<ActionOperator>();
                // List<Choice> listMultipleChoiceCheckBox = new List<Choice>();
                List<Range> listRange = [];
                List<ActionChoiceDB> listActionDB = [];
                List<Attachment> attchments = [];
                List<ComponentInstruction> listComponents = [];
                List<ProcessMasterAttachmentDetail> listAttachemtnDetail = [];

                string xmlChoices = string.Empty;
                string xmlRanges = string.Empty;
                string xmlComponents = string.Empty;
                string xmlSections = string.Empty;
                string xmlInstructions = string.Empty;
                string xmlActionCheckBoxs = string.Empty;
                string xmlMultipleChoiceCheckBox = string.Empty;
                string xmlActionOperators = string.Empty;
                string xmlAtachments = string.Empty;
                List<ProcedureMasterInstruction> listInstrucction = [];
                List<ProcedureSection> listTempSections = null;// new List<ProcedureSection>();

                string jsonString = "";

                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                //|| processMasterinfo.IsProcessNewActivity == true)
                {
                    jsonString = JsonConvert.SerializeObject(processMasterinfo.Sections);
                }

                processMasterinfo.Sections.ForEach(section =>
                {
                    if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                    // || processMasterinfo.IsProcessNewActivity == true)
                    {
                        section.SectionId = Guid.NewGuid().ToStr();
                    }
                });

                if ((processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                    //|| processMasterinfo.IsProcessNewActivity == true)
                    && !string.IsNullOrEmpty(jsonString))
                {
                    listTempSections = JsonConvert.DeserializeObject<List<ProcedureSection>>(jsonString);
                }

                foreach (ProcedureSection section in processMasterinfo.Sections)
                {
                    section.ProcedureId = processMasterinfo.ProcedureId;
                    if (section.AttachmentId is not null && !string.IsNullOrEmpty(section.AttachmentId))
                    {
                        await _attachmentOperation.AttachmentSync(section.AttachmentId, section.SectionId, systemOperator).ConfigureAwait(false);
                    }

                    foreach (ProcedureMasterInstruction instruction in section.ListInstrucctions)
                    {
                        instruction.Status = 1;
                        instruction.SectionId = section.SectionId;
                        instruction.ViewType = (int?)instruction.ViewType ?? 0;
                        instruction.TypeDataReading = (int?)instruction.TypeDataReading ?? 0;
                        instruction.Long = (int?)instruction.Long ?? 0;
                        instruction.Type = (int?)instruction.Type ?? 0;
                        instruction.TypeInstrucction = instruction.TypeInstrucction;

                        if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                        {
                            instruction.InstructionId = Guid.NewGuid().ToStr();
                        }
                        listInstrucction.Add(instruction);
                        if (instruction.MultipleChoice?.Count > 0)
                        {
                            foreach (Choice choice in instruction.MultipleChoice)
                            {
                                choice.InstructionId = instruction.InstructionId;
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                // || processMasterinfo.IsProcessNewActivity == true)
                                {
                                    choice.OldId = choice.Id;
                                    choice.Id = Guid.NewGuid().ToStr();
                                }
                                if (processMasterinfo.IsDuplicate || processMasterinfo.CreateNewVersion)
                                //processMasterinfo.IsProcessNewActivity == true
                                {
                                    ProcedureSection SectionTemp = listTempSections.Find(p => p.SectionId == choice.SectionId);
                                    if (SectionTemp is not null)
                                    {
                                        choice.SectionId = processMasterinfo.Sections.Find(p => p.OrderSection == SectionTemp.OrderSection).SectionId;
                                    }
                                    else if (SectionTemp is null
                                     && choice.SectionId != "Finish"
                                     && choice.SectionId != "FinishError"
                                     && choice.SectionId != "Block")
                                    {
                                        choice.SectionId = "NoAction";
                                    }
                                }
                                listMultipleChoice.Add(choice);

                                if (choice.AttachmentId is not null && !string.IsNullOrEmpty(choice.AttachmentId))
                                {
                                    await _attachmentOperation.AttachmentSync(choice.AttachmentId, choice.Id, systemOperator).ConfigureAwait(false);
                                }
                            }
                            instruction.MultipleChoice = null;
                        }
                        if (instruction.Range?.Count > 0)
                        {
                            instruction.Range.ForEach(range =>
                            {
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                {
                                    range.Id = Guid.NewGuid().ToStr();
                                }
                                range.InstructionId = instruction.InstructionId;
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                {
                                    ProcedureSection SectionTemp = listTempSections.Find(p => p.SectionId == range.SectionId);
                                    if (SectionTemp is not null)
                                    {
                                        range.SectionId = processMasterinfo.Sections.Find(p => p.OrderSection == SectionTemp.OrderSection).SectionId;
                                    }
                                    else if (SectionTemp is null
                                        && range.SectionId != "Finish"
                                        && range.SectionId != "FinishError"
                                        && range.SectionId != "Block")
                                    {
                                        range.SectionId = "NoAction";
                                    }
                                }
                                listRange.Add(range);
                            });
                            instruction.Range = null;
                        }
                        if (instruction.Components?.Count > 0)
                        {
                            foreach (ComponentInstruction component in instruction.Components)
                            {
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                {
                                    component.Id = Guid.NewGuid().ToStr();
                                }
                                component.InstructionId = instruction.InstructionId;
                                listComponents.Add(component);
                                if (component.AttachmentId is not null
                                   && !string.IsNullOrEmpty(component.AttachmentId))
                                {
                                    await _attachmentOperation.AttachmentSync(component.AttachmentId, component.Id, systemOperator).ConfigureAwait(false);
                                }
                            }
                            instruction.Components = null;
                        }

                        if (instruction.ActionCheckBox?.Count > 0)
                        {
                            instruction.ActionCheckBox.ForEach(Action =>
                            {
                                if (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                                {
                                    Action.Id = Guid.NewGuid().ToStr();
                                    ProcedureSection SectionTemp = listTempSections.Find(p => p.SectionId == Action.SectionId);
                                    if (SectionTemp is not null)
                                    {
                                        Action.SectionId = processMasterinfo.Sections.Find(p => p.OrderSection == SectionTemp.OrderSection).SectionId;
                                    }
                                    else if (SectionTemp is null
                                        && Action.SectionId != "Finish"
                                        && Action.SectionId != "FinishError"
                                        && Action.SectionId != "Block")
                                    {
                                        Action.SectionId = "NoAction";
                                    }
                                    List<string> ListNewValueAction = [];
                                    Action.ValueChoice.ForEach(value =>
                                    {
                                        Choice addObj = listMultipleChoice.Find(p => p.OldId == value);
                                        if (addObj is not null)
                                        {
                                            ListNewValueAction.Add(addObj.Id);
                                        }
                                    });
                                    Action.ValueChoice = ListNewValueAction;
                                }
                                ActionChoiceDB objAdd = new()
                                {
                                    Id = Action.Id,
                                    InstructionId = instruction.InstructionId,
                                    SectionId = Action.SectionId,
                                    Message = Action.Message,
                                    OrderChoice = Action.OrderChoice,
                                    IsNotify = Action.IsNotify,
                                    MessageNotify = Action.MessageNotify,
                                    ValueChoice = string.Join(',', Action.ValueChoice.Select(x => x.ToString()).ToArray())
                                };
                                listActionDB.Add(objAdd);
                            });
                            instruction.ActionCheckBox = null;
                        }
                    }
                    section.ListInstrucctions = null;
                }

                xser.Serialize(ms, processMasterinfo.Sections);
                xmlSections = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");

                if (listInstrucction.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<ProcedureMasterInstruction>));
                    xser.Serialize(ms, listInstrucction);
                    xmlInstructions = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listMultipleChoice.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<Choice>));
                    xser.Serialize(ms, listMultipleChoice);
                    xmlChoices = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listActionDB.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<ActionChoiceDB>));
                    xser.Serialize(ms, listActionDB);
                    xmlActionCheckBoxs = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listRange.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<Range>));
                    xser.Serialize(ms, listRange);
                    xmlRanges = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listComponents.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<ComponentInstruction>));
                    xser.Serialize(ms, listComponents);
                    xmlComponents = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                if (listAttachemtnDetail.Count > 0)
                {
                    ms = new MemoryStream();
                    xser = new XmlSerializer(typeof(List<ProcessMasterAttachmentDetail>));
                    xser.Serialize(ms, listAttachemtnDetail);
                    xmlAtachments = Encoding.UTF8.GetString(ms.ToArray()).Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
                }
                returnValue = _procedureRepo.ProcessMasterInsByXML(processMasterinfo
                    , xmlSections
                    , xmlInstructions
                    , xmlChoices
                    , xmlRanges
                    , xmlActionCheckBoxs
                    , systemOperator
                    , xmlComponents
                    , xmlAtachments
                    , Validate);
                if (returnValue.IsSuccess)
                {
                    Procedure procedurelog = _procedureRepo.GetProcedure(processMasterinfo.ProcedureId, null);
                    // await procedurelog.Log(procedurelog.CreateNewVersion ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
                    // _ = Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged
                    //  , new
                    //  {
                    //      Catalog = Entities.Procedure,
                    //      Action = (processMasterinfo.CreateNewVersion || processMasterinfo.IsDuplicate)
                    //  ? EntityLogType.Create : EntityLogType.Update
                    //  ,
                    //      Data = procedurelog
                    //  });
                }
            }
        }
        catch (Exception ex)
        {
            //logger.Error(ex);
            throw;
        }
        return returnValue;
    }
    #endregion ProcessMaster
}