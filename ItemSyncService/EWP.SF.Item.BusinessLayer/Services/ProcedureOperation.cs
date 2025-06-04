using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Text;
using Range = EWP.SF.Common.Models.Range;
using SixLabors.ImageSharp;
using EWP.SF.Item.DataAccess;
using EWP.SF.Common.Enumerators;
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
    /// <summary>
	/// Create /Update List Procedures
	/// </summary>
	/// <param name="listProcedures">List Data</param>
	/// <param name="listProceduresOriginal">List Data</param>
	/// <param name="Validate">List Data</param>
	/// <param name="Level">List Data</param>///
	/// <param name="systemOperator">User Current</param>
	public async Task<List<ResponseData>> ProcessMasterInsExternalSync(List<ProcedureExternalSync> listProcedures
    , List<ProcedureExternalSync> listProceduresOriginal, User systemOperator, bool Validate, LevelMessage Level = 0)
    {
        List<ResponseData> returnValue = [];
        Procedure procedureInfo;
        ResponseData MessageError;
        List<ProcedureVersion> listProcedureVersions;
        Procedure procedureDB;
        bool NotifyOnce = true;
        bool CreateVersionSync = false;
        if (listProcedures?.Count > 0)
        {
            NotifyOnce = listProcedures.Count == 1;
            int Line = 0;
            foreach (ProcedureExternalSync procedure in listProcedures)
            {
                Line++;
                try
                {
                    procedureDB = new Procedure();
                    procedureInfo = new Procedure();
                    listProcedureVersions = ListProcedureVersionsByCode(procedure.ProcedureCode);
                    procedureDB = GetProcessByProcessCodeVersion(procedure.ProcedureCode, procedure.Version);
                    procedureInfo = await CompararYReemplazar(procedureDB, procedure, listProceduresOriginal, systemOperator).ConfigureAwait(false);

                    listProcedureVersions = ListProcedureVersionsByCode(procedure.ProcedureCode);
                    ProcedureVersion findProcedureDb = null;
                    if (listProcedureVersions is null || listProcedureVersions.Count == 0)
                    {
                        findProcedureDb = null;
                    }
                    else
                    {
                        findProcedureDb = listProcedureVersions.Find(x => x.Version == procedure.Version);
                    }

                    ProcedureVersion procedureWithMaxVersion = null;
                    if (listProcedureVersions is null || listProcedureVersions.Count == 0)
                    {
                        procedureWithMaxVersion = null;
                    }
                    else
                    {
                        procedureWithMaxVersion = listProcedureVersions.OrderByDescending(item => item.Version).FirstOrDefault();
                    }

                    if (findProcedureDb is null)
                    {
                        CreateVersionSync = true;
                    }
                    else if (findProcedureDb?.IsActivityUsed == true)
                    {
                        CreateVersionSync = true;
                        procedureInfo.Version = procedureWithMaxVersion.Version + 1;
                    }
                    else if (findProcedureDb?.IsActivityUsed == false)
                    {
                        CreateVersionSync = false;
                        procedureInfo.ProcedureId = findProcedureDb.ProcedureId;
                    }
                    else
                    {
                        CreateVersionSync = false;
                    }

                    procedureInfo.CreateVersionSync = CreateVersionSync;
                    if (procedureInfo.Sections?.Count > 0)
                    {
                        procedureInfo.Sections.ForEach(section =>
                        {
                            if (section.ListInstrucctions?.Count > 0)
                            {
                                section.ListInstrucctions.ForEach(instruction =>
                                {
                                    if (instruction.MultipleChoice?.Count > 0)
                                    {
                                        instruction.MultipleChoice.ForEach(choice =>
                                        {
                                            if (!string.IsNullOrEmpty(choice.SectionId))
                                            {
                                                ProcedureSection result = procedureInfo.Sections.Find(x => x.OrderSection == choice.SectionId.ToInt32());
                                                if (result is not null)
                                                {
                                                    choice.SectionId = result.SectionId;
                                                }
                                            }
                                            else
                                            {
                                                choice.SectionId = null;
                                            }
                                        });
                                    }
                                });
                            }
                        });
                    }

                    ResponseData result = await ProcessMasterInsByXML(procedureInfo, systemOperator, Validate, NotifyOnce).ConfigureAwait(false);
                    returnValue.Add(result);
                }
                catch (Exception ex)
                {
                    MessageError = new ResponseData
                    {
                        Message = ex.Message,
                        Code = "Line:" + Line.ToStr()
                    };
                    returnValue.Add(MessageError);
                }
            }
            if (!Validate)
            {
                // if (!NotifyOnce)
                // {
                // 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Procedure, Action = ActionDB.IntegrateAll.ToStr() });
                // }

                returnValue = Level switch
                {
                    LevelMessage.Warning => [.. returnValue.Where(x => !string.IsNullOrEmpty(x.Message))],
                    LevelMessage.Error => [.. returnValue.Where(x => !x.IsSuccess)],
                    _ => returnValue
                };
            }
        }
        return returnValue;
    }
    /// <summary>
	///
	/// </summary>
	public List<ProcedureVersion> ListProcedureVersionsByCode(string Code) => _procedureRepo.ListProcedureVersionsByCode(Code);
    /// <summary>
    ///
    /// </summary>
    public Procedure GetProcessByProcessCodeVersion(string Code, int Version)
    {
        Procedure Result = null;
        try
        {
            ProcessMasterVersionresult returnDB = _procedureRepo.GetProcessVersion(Code, Version);
            if (returnDB is not null)
            {
                Result = _procedureRepo.GetProcedure(returnDB.ProcedureId);
            }
        }
        catch (Exception ex)
        {
            //logger.Error(ex);
            throw;
        }
        return Result;
    }
    /// <summary>
	///
	/// </summary>
	public async Task<Procedure> CompararYReemplazar(Procedure procedureDB, ProcedureExternalSync procedureSync, List<ProcedureExternalSync> listProceduresOriginal, User systemOperator)
    {
        Procedure resultProcedure = new();
        procedureDB ??= new Procedure();
        int varStatus = 1;
        if (procedureSync.Status == "Development")
        {
            varStatus = 7;
        }
        else if (procedureSync.Status == "Active")
        {
            varStatus = 1;
        }
        else if (procedureSync.Status == "Inactive")
        {
            varStatus = 2;
        }
        else
        {
            varStatus = 1;
        }
        ProcedureExternalSync procedureOriginal = listProceduresOriginal.Find(x => x.ProcedureCode == procedureSync.ProcedureCode && x.Version == procedureSync.Version);
        resultProcedure.Name = ValidateString(procedureSync.ProcedureName, procedureDB.Name, procedureOriginal.ProcedureName);
        resultProcedure.ProcedureId = !string.IsNullOrEmpty(procedureDB.ProcedureId) ? procedureDB.ProcedureId : Guid.NewGuid().ToStr();
        resultProcedure.ProcedureIdOrigin = procedureDB.ProcedureIdOrigin;
        resultProcedure.Code = procedureSync.ProcedureCode;
        resultProcedure.Description = ValidateString(procedureSync.ProcedureName, procedureDB.Description, procedureOriginal.ProcedureName);
        resultProcedure.Version = procedureSync.Version;
        resultProcedure.Status = varStatus; // (Enum.Parse(typeof(Status), ValidateString(procedureSync.Status, ((Status)procedureDB.Status).ToString(), procedureOriginal.Status)).ToInt32());
        resultProcedure.EarlierVersion = procedureSync.EarlierVersion;
        resultProcedure.IdActivityClass = Enum.Parse<ClassTypeProcedure>(ValidateString(procedureSync.ClassCode, ((ClassTypeProcedure)procedureDB.IdActivityClass).ToString(), procedureOriginal.ClassCode)).ToInt32();
        resultProcedure.ActivityType = ValidateString(procedureSync.ActivityTypeCode, procedureDB.ActivityType, procedureOriginal.ActivityTypeCode);
        resultProcedure.InterventionId = ValidateString(procedureSync.InterventionCode, procedureDB.InterventionId, procedureOriginal.InterventionCode);
        resultProcedure.SourceId = ValidateString(procedureSync.SourceCode, procedureDB.SourceId, procedureOriginal.SourceCode);
        resultProcedure.Sections ??= [];
        foreach (SectionExternalSync section in procedureSync.Sections)
        {
            ProcedureSection sectionDb = new();
            if (procedureDB.Sections?.Count > 0)
            {
                sectionDb = procedureDB.Sections.Find(x => x.OrderSection == section.SectionOrder);
            }
            sectionDb ??= new();
            SectionExternalSync sectionOriginal = procedureOriginal.Sections.Find(x => x.SectionOrder == section.SectionOrder);
            resultProcedure.Sections.Add(await ValidateSection(section, sectionDb, sectionOriginal, resultProcedure.ProcedureId, systemOperator).ConfigureAwait(false));
        }

        return resultProcedure;
    }
    /// <summary>
    ///
    /// </summary>
    public static string ValidateString(string SyncValue, string DbValue, string OriginalValue)
    {
        DbValue ??= "";
        string retunrValue;
        if (string.IsNullOrEmpty(SyncValue))
        {
            retunrValue = DbValue;
        }
        else if (SyncValue != DbValue)
        {
            if (string.IsNullOrEmpty(OriginalValue) && !string.IsNullOrEmpty(DbValue))
            {
                retunrValue = DbValue;
            }
            else
            {
                retunrValue = SyncValue;
            }
        }
        else
        {
            retunrValue = DbValue;
        }
        return retunrValue;
    }
    /// <summary>
    ///
    /// </summary>
    public async Task<ProcedureSection> ValidateSection(SectionExternalSync SyncSection, ProcedureSection DbSection, SectionExternalSync OriginalSection, string ProcedureId, User systemOperator)
    {
        ProcedureSection sectionAdd = new()
        {
            SectionId = (!string.IsNullOrEmpty(DbSection.SectionId) && DbSection.SectionId is not null) ? DbSection.SectionId : Guid.NewGuid().ToStr(),
            Section = ValidateString(SyncSection.SectionName, DbSection.Section, OriginalSection.SectionName),
            Description = ValidateString(SyncSection.SectionDescription, DbSection.Description, OriginalSection.SectionDescription),
            Observations = ValidateString(SyncSection.Observations, DbSection.Observations, OriginalSection.Observations),
            ProcedureId = !string.IsNullOrEmpty(ProcedureId) ? ProcedureId : Guid.NewGuid().ToStr(),
            TypeSection = CastStringToEnumOrNullSync<SectionType>(SyncSection.SectionType, DbSection.SectionType, OriginalSection.SectionType),
            OrderSection = SyncSection.SectionOrder,
            Status = 1
        };
        if (SyncSection.TypeVisualHelp.Equals("ATTACH FILE", StringComparison.OrdinalIgnoreCase) || SyncSection.TypeVisualHelp.Equals("URL", StringComparison.OrdinalIgnoreCase))
        {
            List<AttachmentLocal> listAttachmentRequest = [];
            AttachmentLocal objAdd = new()
            {
                TypeCode = SyncSection.TypeVisualHelp.Equals("ATTACH FILE", StringComparison.OrdinalIgnoreCase) ? "Image" : SyncSection.TypeVisualHelp,
                Name = SyncSection.TypeVisualHelp.Equals("ATTACH FILE", StringComparison.OrdinalIgnoreCase) ? "jpg" : SyncSection.VisualHelp,
                Extension = SyncSection.TypeVisualHelp.Equals("ATTACH FILE", StringComparison.OrdinalIgnoreCase) ? "jpg" : SyncSection.TypeVisualHelp,
                FileBase64 = SyncSection.VisualHelp,
                AuxId = sectionAdd.SectionId,
                Entity = "Procedure.Section",
                IsTemp = false,
                Size = "0",
                Status = 1,
                IsImageEntity = true,
            };

            listAttachmentRequest.Add(objAdd);
            List<AttachmentResponse> result = await SaveAttachment(listAttachmentRequest, systemOperator).ConfigureAwait(false);
            if (result?.Count > 0)
            {
                sectionAdd.Attachment = new()
                {
                    Id = result.FirstOrDefault().Id
                };
            }
        }
        sectionAdd.ListInstrucctions ??= [];
        SyncSection.Instructions.ForEach(instrucction =>
        {
            ProcedureMasterInstruction instructionDB = new();
            if (DbSection.ListInstrucctions?.Count > 0)
            {
                instructionDB = DbSection.ListInstrucctions.Find(x => x.CodeInstruction == instrucction.InstructionOrder);
            }
            InstrucctionExternalSync instructionOriginal = OriginalSection.Instructions.Find(x => x.InstructionOrder == instrucction.InstructionOrder);
            instructionDB ??= new();
            sectionAdd.ListInstrucctions.Add(ValidateInstruction(instrucction, instructionDB, instructionOriginal, SyncSection, sectionAdd.SectionId));
        });
        return sectionAdd;
    }

    /// <summary>
    ///
    /// </summary>
    public static ProcedureMasterInstruction ValidateInstruction(InstrucctionExternalSync instrucctionSync, ProcedureMasterInstruction instructionDB
        , InstrucctionExternalSync instructionOriginal, SectionExternalSync SyncSection, string sectionId)
    {
        ProcedureMasterInstruction instructionAdd = new()
        {
            SectionId = sectionId,
            InstructionId = (!string.IsNullOrEmpty(instructionDB.InstructionId) && instructionDB.InstructionId is not null) ? instructionDB.InstructionId : Guid.NewGuid().ToStr(),
            DefaultValue = ValidateString(instrucctionSync.DefaultValue.ToStr(), instructionDB.DefaultValue, instructionOriginal.DefaultValue.ToStr()),
            // instructionAdd.ProcessId = procedureInfo.CreateNewVersion == false ? procedureDB.ProcedureId : "";
            TypeInstrucction = ValidateString(instrucctionSync.InstructionType, instructionDB.TypeInstrucction, instructionOriginal.InstructionType),
            CodeInstruction = instrucctionSync.InstructionOrder,
            //instructionAdd.Instruction = "&lt;h5 style=\"font-weight: bold; color: rgb(71, 95, 123); letter-spacing: 0.15px;\" & gt;" + instrucctionSync.InstructionDescription + "&lt;/h5&gt;";
            Instruction = "&lt;h5 style=\"font-weight: bold; color: rgb(71, 95, 123); letter-spacing: 0.15px;\"&gt;"
        + ValidateString(instrucctionSync.InstructionDescription, instructionDB.Instruction, instructionOriginal.InstructionDescription) + "&lt;/h5&gt;",
            // instructionAdd.Code = procedure.ProcedureCode;
            //  instructionAdd.Version = procedure.Version;
            SectionOrder = SyncSection.SectionOrder
        };
        int viewType = 0;
        if (instructionDB.ViewType is not null)
        {
            viewType = (int)instructionDB.ViewType;
        }

        instructionAdd.ViewType = CastIntToEnumOrNullSync<ViewTypes>(instrucctionSync.ViewType, viewType, instructionOriginal.ViewType);
        instructionAdd.MultiSelect = ValidateString(instrucctionSync.MultiSelect, instructionDB.MultiSelect.ToStr(), instructionOriginal.MultiSelect).ToBool();
        instructionAdd.Mandatory = ValidateString(instrucctionSync.InstructionMandatory.ToString(), instructionDB.Mandatory.ToString(), instructionOriginal.InstructionMandatory.ToString()).ToBool();// instrucctionSync.InstructionMandatory;
        instructionAdd.IsGauge = ValidateString(instrucctionSync.IsGauge.ToString(), instructionDB.IsGauge.ToString(), instructionOriginal.IsGauge.ToString()).ToBool();
        instructionAdd.Long = ValidateString(instrucctionSync.InstructionLongText.ToString(), instructionDB.Long.ToString(), instructionOriginal.InstructionLongText.ToString()).ToInt32();
        instructionAdd.QueryUser = ValidateString(instrucctionSync.InstructionQueryUser, instructionDB.QueryUser, instructionOriginal.InstructionQueryUser);
        instructionAdd.SignalCode = ValidateString(instrucctionSync.DataReadingSignalCode, instructionDB.SignalCode, instructionOriginal.DataReadingSignalCode);

        if (string.Equals(instrucctionSync.InstructionDataType, "NUMERIC", StringComparison.OrdinalIgnoreCase))
        {
            if (instrucctionSync.IsDecimal)
            {
                instructionAdd.Type = InputType.Decimal.ToInt32();
            }
            else
            {
                instructionAdd.Type = InputType.Integer.ToInt32();
            }
        }
        else
        {
            instructionAdd.Type = CastIntToEnumOrNullSync<InputType>(instrucctionSync.InstructionDataType, instructionDB.Type.ToInt32(), instructionOriginal.InstructionDataType);
        }
        instructionAdd.TypeDataReading = CastIntToEnumOrNullSync<DataReadingType>(instrucctionSync.TypeDataReading, instructionDB.TypeDataReading.ToInt32(), instructionOriginal.TypeDataReading);
        instructionAdd.TimeInSec = ValidateString(instrucctionSync.TimeInSec.ToStr(), instructionDB.TimeInSec.ToStr(), instructionOriginal.TimeInSec.ToStr()).ToInt64();
        instructionAdd.QueryUser = ValidateString(instrucctionSync.InstructionQueryUser, instructionDB.QueryUser, instructionOriginal.InstructionQueryUser);
        instructionAdd.Status = 1;
        instructionAdd.MinValue = ValidateString(instrucctionSync.GaugeMinValue.ToStr(), instructionDB.MinValue.ToStr(), instructionOriginal.GaugeMinValue.ToStr()).ToDecimal();
        instructionAdd.MaxValue = ValidateString(instrucctionSync.GaugeMaxValue.ToStr(), instructionDB.MaxValue.ToStr(), instructionOriginal.GaugeMaxValue.ToStr()).ToDecimal();
        instructionAdd.TargetValue = ValidateString(instrucctionSync.GaugeTargetValue.ToStr(), instructionDB.TargetValue.ToStr(), instructionOriginal.GaugeTargetValue.ToStr()).ToDecimal();
        instructionAdd.CodeAutomatic = ValidateString(instrucctionSync.CodeAutomatic, instructionDB.CodeAutomatic, instructionOriginal.CodeAutomatic);

        switch (instructionAdd.TypeInstrucction.ToUpperInvariant())
        {
            case "MULTIPLECHOICE":
                instructionAdd.MultipleChoice ??= [];
                if (instrucctionSync.Choices?.Count > 0)
                {
                    instrucctionSync.Choices.ForEach(choice =>
                    {
                        Choice choiceDb = new();
                        if (instructionDB.MultipleChoice?.Count > 0)
                        {
                            choiceDb = instructionDB.MultipleChoice.Find(x => x.OrderChoice == choice.OrderChoice);
                        }
                        choiceDb ??= new();
                        ChoiceExternalSync sectionOriginal = instructionOriginal.Choices.Find(x => x.OrderChoice == choice.OrderChoice);
                        instructionAdd.MultipleChoice.Add(ValidateChoice(choice, choiceDb, sectionOriginal, instructionAdd));
                    });
                }
                else
                {
                    throw new Exception(string.Format("Section Order: {0}   Instruction Order: {1}  Chocies not found. ", instructionAdd.CodeInstruction, SyncSection.SectionOrder));
                }

                break;
        }

        return instructionAdd;
    }

    /// <summary>
    ///
    /// </summary>
    public static Choice ValidateChoice(ChoiceExternalSync choiceSync, Choice choiceDB
, ChoiceExternalSync choiceOriginal, ProcedureMasterInstruction instructionAdd)
    {
        return new()
        {
            InstructionId = instructionAdd.InstructionId,
            OrderChoice = choiceSync.OrderChoice,
            ValueChoice = ValidateString(choiceSync.Value, choiceDB.ValueChoice, choiceOriginal.Value),
            SectionId = CastStrinToEnum<ActionTypeCode>(choiceSync.Action) == ActionTypeCode.Goto ? ValidateString(choiceSync.OrderSectionGoTo, choiceDB.SectionId, choiceOriginal.OrderSectionGoTo)
            : (CastStrinToEnum<ActionTypeCode>(choiceSync.Action) == ActionTypeCode.FinisWithWarning ? ActionTypeCode.finisherror.ToString() : choiceSync.Action),
            IsNotify = ValidateString(choiceSync.IsNotify.ToStr(), choiceDB.IsNotify.ToStr(), choiceOriginal.IsNotify.ToStr()).ToBool(),
            MessageNotify = ValidateString(choiceSync.MessageNotify, choiceDB.MessageNotify, choiceOriginal.MessageNotify),
            Message = ValidateString(choiceSync.MessageWarning, choiceDB.Message, choiceOriginal.MessageWarning),
            // choiceAdd.AttachmentId = instructionAdd.InstructionId;
            Id = (!string.IsNullOrEmpty(choiceDB.Id) && choiceDB.Id is not null) ? choiceDB.Id : Guid.NewGuid().ToStr()
        };
    }
    public static T? CastStrinToEnum<T>(string value) where T : struct, Enum
	{
		return Enum.TryParse(value, true, out T resultSync) ? resultSync : null;
	}
    /// <summary>
    ///
    /// </summary>
    public static int CastStringToEnumOrNullSync<T>(string valueSync, string valueDb, string valueOriginal) where T : struct, Enum
    {
        string sync = "", db = "", original = "";
        if (Enum.TryParse(valueSync, true, out T resultSync))
        {
            sync = resultSync.ToStr();
        }
        if (Enum.TryParse(valueDb, true, out T resultDb))
        {
            db = resultDb.ToStr();
        }
        if (Enum.TryParse(valueOriginal, true, out T resultOriginal))
        {
            original = resultOriginal.ToStr();
        }
        return Enum.TryParse(ValidateString(sync, db, original), true, out T resultFunction) ? resultFunction.ToInt32() : 0;
    }

	/// <summary>
	///
	/// </summary>
	public static int CastIntToEnumOrNullSync<T>(string valueSync, int valueDb, string valueOriginal) where T : struct, Enum
	{
		string sync = "", db = "", original = "";
		if (Enum.TryParse(valueSync, true, out T resultSync))
		{
			sync = resultSync.ToStr();
		}
		if (Enum.IsDefined(typeof(T), valueDb))
		{
			db = ((T)Enum.ToObject(typeof(T), valueDb)).ToString();
		}
		if (Enum.TryParse(valueOriginal, true, out T resultOriginal))
		{
			original = resultOriginal.ToStr();
		}
		return Enum.TryParse(ValidateString(sync, db, original), true, out T resultFunction) ? resultFunction.ToInt32() : 0;
	}
    #endregion ProcessMaster
}