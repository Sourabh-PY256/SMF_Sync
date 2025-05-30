using System.Data;
using System.Globalization;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.EntityLogger;
using EWP.SF.Helper;
using MySqlConnector;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.ConnectionModule;
using System.Text;

using Newtonsoft.Json;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Range = EWP.SF.Common.Models.Range;

namespace EWP.SF.Item.DataAccess;

public class ProcedureRepo : IProcedureRepo
{
    private readonly string ConnectionString;
    private static readonly CompositeFormat MISSING_PARAM = CompositeFormat.Parse("Parameter \"{0}\" is required and was not provided.");
    private readonly string ConnectionStringReports;
    private readonly string ConnectionStringLogs;

    private readonly string Database;

    public ProcedureRepo(IApplicationSettings applicationSettings)
    {
        ConnectionString = applicationSettings.GetConnectionString();
        ConnectionStringReports = applicationSettings.GetReportsConnectionString();
        ConnectionStringLogs = applicationSettings.GetConnectionString("Logs");
        Database = applicationSettings.GetDatabaseFromConnectionString();
    }
    #region Procedure
  
	public ResponseData ProcessMasterInsByXML(Procedure procesInfo
	, string xmlSections
	, string xmlInstructions
	, string xmlChoice
	, string xmlRange
	, string xmlActionCheckBoxs
	//  , string xmlMultipleChoiceCheckBox
	// , string xmlActionOperators
	, User systemOperator
	, string xmlComponents
	, string xmlAtachments
	, bool IsValidation = false)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Procedure_Instruction_MRG", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.CommandTimeout = 30000;
				command.Parameters.AddCondition("_ProcedureId", procesInfo.ProcedureId, procesInfo is not null, string.Format(CultureInfo.InvariantCulture, MISSING_PARAM, "Process Id"));
				// command.Parameters.AddWithValue("_ActivityId", procesInfo.ActivityId);
				_ = command.Parameters.AddWithValue("_Operator", systemOperator.Id);
				_ = command.Parameters.AddWithValue("_XMLSections", xmlSections);
				_ = command.Parameters.AddWithValue("_XMLInstructions", xmlInstructions);
				_ = command.Parameters.AddWithValue("_XMLChoice", xmlChoice);
				_ = command.Parameters.AddWithValue("_XMLRange", xmlRange);
				_ = command.Parameters.AddWithValue("_XMLActionCheckBoxs", xmlActionCheckBoxs);
				_ = command.Parameters.AddWithValue("_XmlComponents", xmlComponents);
				_ = command.Parameters.AddWithValue("_XmlAtachments", xmlAtachments);
				_ = command.Parameters.AddWithValue("_IsValidation", IsValidation);

				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ResponseData
					{
						Id = rdr["Id"].ToStr(),
						Action = (ActionDB)rdr["Action"].ToInt32(),
						IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
						Code = rdr["Code"].ToStr(),
						Version = rdr["Version"].ToInt32(),
						Message = rdr["Message"].ToStr(),
					};
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				returnValue = null;
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return returnValue;
	}
	public Procedure GetProcedure(string ProcedureId, string ActivityId = null, string Instance = null)

	{
		Procedure returnValue = null;

		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_ProcedureById_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				List<ProcedureSection> listSection = [];
				List<ProcedureMasterInstruction> listInstruction = [];
				List<Choice> listTempChoice = [];
				List<Choice> listTempChoiceCheckBox = [];
				List<ActionOperator> listMultipleActionOperator = [];
				List<ComponentInstruction> listComponent = [];
				List<ActionChoiceDB> listTempActionsCheckBox = [];
				List<Range> listTempRange = [];

				ProcedureMasterInstruction elementInstruccion;
				_ = command.Parameters.AddWithValue("_ProcedureId", ProcedureId);
				_ = command.Parameters.AddWithValue("_ActivityId", ActivityId);
				_ = command.Parameters.AddWithValue("_Instnace", Instance);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new()
					{
						ProcedureId = rdr["Id"].ToStr(),
						ProcedureIdOrigin = rdr["ProcedureIdOrigin"].ToStr(),
						Code = rdr["Code"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Status = rdr["Status"].ToInt32(),
						StatusDescription = rdr["StatusDescription"].ToStr(),
						Version = rdr["Version"].ToInt32(),
						EarlierVersion = rdr["EarlierVersion"].ToInt32(),
						IdActivityClass = rdr["IdActivityClass"].ToInt32(),
						InterventionId = rdr["InterventionId"].ToStr(),
						SourceId = rdr["SourceId"].ToStr(),
						HasIntervention = rdr["HasIntervention"].ToInt32().ToBool(),
						HasSource = rdr["HasSource"].ToInt32().ToBool(),
						ActivityType = rdr["ActivityType"].ToStr(),
						ClassName = rdr["ClassName"].ToStr(),
						IdTypeClass = rdr["ActivityTypeCode"].ToStr(),
						Description = rdr["Description"].ToStr(),
						IsByProcedure = rdr["IsByProcedure"].ToInt32().ToBool(),
						IsManualActivity = rdr["IsManualActivity"].ToInt32().ToBool(),
						Layout = JsonSerializer.Deserialize<List<LayoutProcedure>>(rdr["Layout"].ToStr()),
					};
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ProcedureSection element = new()
					{
						SectionId = rdr["Id"].ToStr(),
						ProcedureId = rdr["ProcedureId"].ToStr(),
						TypeSection = rdr["TypeSection"].ToInt32(),
						SectionType = rdr["SectionType"].ToStr(),
						Section = rdr["Name"].ToStr(),
						Description = rdr["Description"].ToStr(),
						Observations = rdr["Observations"].ToStr(),
						Status = rdr["Status"].ToInt32(),
						OrderSection = rdr["OrderSection"].ToInt32(),
						AttachmentId = rdr["AttachmentId"].ToStr()
					};
					element.Attachment = GetAttachmentSection(element.AttachmentId, element.SectionId);
					listSection.Add(element);
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					elementInstruccion = new ProcedureMasterInstruction
					{
						InstructionId = rdr["Id"].ToStr(),
						ProcessId = rdr["ProcedureId"].ToStr(),
						CodeInstruction = rdr["CodeInstruction"].ToInt32(),
						SectionId = rdr["SectionId"].ToStr(),
						Instruction = rdr["Description"].ToStr(),
						InstructionDisplayTitle = rdr["InstructionDisplayTitle"].ToStr(),
						TypeInstrucction = rdr["TypeInstrucction"].ToStr(),
						ViewType = rdr["ViewType"].ToInt32(),
						MultiSelect = rdr["MultiSelect"].ToInt32().ToBool(),
						Mandatory = rdr["Mandatory"].ToBool(),
						IsGauge = rdr["IsGauge"].ToBool(),
						Long = rdr["Length"].ToInt32(),
						Type = rdr["Type"].ToInt32(),
						IsDecimal = rdr["IsDecimal"].ToInt32().ToBool(),
						TypeDataReading = rdr["TypeDataReading"].ToInt32(),
						TimeInSec = rdr["Time"].ToInt64(),
						DefaultValue = rdr["DefaultValue"].ToStr(),
						MinValue = rdr["MinValue"].ToDecimal(),
						MaxValue = rdr["MaxValue"].ToDecimal(),
						TargetValue = rdr["TargetValue"].ToDecimal(),
						CodeAutomatic = rdr["CodeAutomatic"].ToStr(),
						SignalCode = rdr["SignalCode"].ToString(),
						URLInstrucction = rdr["URLInstrucction"].ToString(),
						QueryUser = rdr["Query"].ToString(),
						Response = rdr["ResponseValue"].ToString()
					};
					listInstruction.Add(elementInstruccion);
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Range elementRange = new()
					{
						Id = rdr["Id"].ToStr(),
						InstructionId = rdr["InstructionId"].ToStr(),
						SectionId = rdr["SectionId"].ToStr(),
						Message = rdr["Message"].ToStr(),
						Min = rdr["Min"].ToDecimal(),
						Max = rdr["Max"].ToDecimal(),
						OrderChoice = rdr["SortId"].ToInt32(),
						IsNotify = rdr["IsNotify"].ToInt32().ToBool(),
						MessageNotify = rdr["MessageNotify"].ToStr(),
					};
					listTempRange.Add(elementRange);
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ActionChoiceDB elementChoice = new()
					{
						Id = rdr["Id"].ToString(),
						InstructionId = rdr["InstructionId"].ToString(),
						ValueChoice = rdr["ValueChoice"].ToStr(),
						SectionId = rdr["SectionId"].ToStr(),
						Message = rdr["Message"].ToStr(),
						MessageNotify = rdr["MessageNotify"].ToStr(),
						IsNotify = rdr["IsNotify"].ToInt32().ToBool(),
						OrderChoice = rdr["SortId"].ToInt32()
					};
					listTempActionsCheckBox.Add(elementChoice);
				}

				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					Choice elementChoice = new()
					{
						Id = rdr["Id"].ToString(),
						InstructionId = rdr["InstructionId"].ToString(),
						SectionId = rdr["SectionId"].ToStr(),
						ValueChoice = rdr["Description"].ToStr(),
						Message = rdr["Message"].ToStr(),
						MessageNotify = rdr["MessageNotify"].ToStr(),
						IsNotify = rdr["IsNotify"].ToInt32().ToBool(),
						OrderChoice = rdr["SortId"].ToInt32(),
						AttachmentId = rdr["AttachmentId"].ToStr()
					};
					elementChoice.Attachment = GetAttachmentSection(elementChoice.AttachmentId, elementChoice.Id);
					elementChoice.Selected = rdr["Selected"].ToInt32().ToBool();

					listTempChoice.Add(elementChoice);
				}
				_ = rdr.NextResultAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					ComponentInstruction element = new()
					{
						// Attachment = new ProcedureMasterSectionAttachment(),
						Id = rdr["Id"].ToString(),
						InstructionId = rdr["InstructionId"].ToString(),
						Code = rdr["Code"].ToStr(),
						CodeSignal = rdr["CodeSignal"].ToStr(),
						ComponentId = rdr["ComponentId"].ToStr(),
						QuantityIssue = rdr["QuantityIssue"].ToDecimal(),
						QuantityAvailable = rdr["QuantityAvailable"].ToDecimal(),
						MaterialImage = rdr["Image"].ToStr(),
						Name = rdr["Name"].ToStr(),
						Line = rdr["SortId"].ToStr(),
						Method = rdr["Method"].ToStr(),
						ProcedureId = returnValue.ProcedureId,// rdr["ProcedureId"].ToStr(),
						Quantity = rdr["Quantity"].ToDecimal(),
						Quantitytext = rdr["Quantitytext"].ToStr(),
						Tolerance = rdr["Tolerance"].ToDecimal(),
						UnitId = rdr["UnitId"].ToStr(),
						UnitType = rdr["UnitType"].ToStr(),
						Mandatory = rdr["Mandatory"].ToInt32().ToBool(),
						TypeComponent = rdr["TypeComponent"].ToString(),
						IsSubProduct = rdr["IsSubProduct"].ToInt32().ToBool(),
						IsRemainingTotal = rdr["IsRemainingTotal"].ToInt32().ToBool(),
						AttachmentId = rdr["IdAttachment"].ToString(),
					};
					listComponent.Add(element);
				}

				if (listSection is not null)
				{
					foreach (ProcedureSection section in listSection)
					{
						section.ListInstrucctions = [];
						section.ListInstrucctions = [.. listInstruction.Where(p => p.SectionId == section.SectionId)];

						foreach (ProcedureMasterInstruction instruction in section.ListInstrucctions)
						{
							if (listTempChoice is not null)
							{
								instruction.MultipleChoice = [];
								instruction.MultipleChoice = [.. listTempChoice.Where(p => p.InstructionId == instruction.InstructionId)];
							}
							if (listTempRange is not null)
							{
								instruction.Range = [];
								instruction.Range = [.. listTempRange.Where(p => p.InstructionId == instruction.InstructionId)];
							}
							if (listTempChoiceCheckBox is not null)
							{
								instruction.MultipleChoiceCheckBox = [];
								instruction.MultipleChoiceCheckBox = [.. listTempChoiceCheckBox.Where(p => p.InstructionId == instruction.InstructionId)];
							}

							if (listMultipleActionOperator is not null)
							{
								instruction.MultipleActionOperator = [];
								instruction.MultipleActionOperator = [.. listMultipleActionOperator.Where(p => p.InstructionId == instruction.InstructionId)];
							}

							if (listTempActionsCheckBox is not null)
							{
								instruction.ActionCheckBox = [];

								foreach (ActionChoiceDB acction in (ActionChoiceDB[])[.. listTempActionsCheckBox.Where(p => p.InstructionId == instruction.InstructionId)])
								{
									ActionChoice objAdd = new()
									{
										Id = acction.Id,
										InstructionId = acction.InstructionId,
										SectionId = acction.SectionId,
										Message = acction.Message,
										OrderChoice = acction.OrderChoice,
										ValueChoice = [.. acction.ValueChoice.Split(',')],
										IsNotify = acction.IsNotify,
										MessageNotify = acction.MessageNotify
									};
									instruction.ActionCheckBox.Add(objAdd);
								}
								// listTempActioneCheckBox.Where(p => p.InstructionId == instruction.InstructionId).ToArray();
							}

							if (listComponent is not null)
							{
								instruction.Components = [];
								instruction.Components = [.. listComponent.Where(p => p.InstructionId == instruction.InstructionId)];

								foreach (var component in instruction.Components)
								{
									//if (listattachmentcomponents is not null)
									//{
									//    component.Attachment = listattachmentcomponents.Where(p => p.SectionId == component.Id).FirstOrDefault();
									//}
								}
							}
						}
					}
					if (returnValue is not null)
					{
						returnValue.Sections = [];
						returnValue.Sections = listSection;
					}
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}

		return returnValue;
	}
	public ResponseData ProcessMasterIns(Procedure ProcessMaster, User User, bool IsValidation = false)
	{
		ResponseData returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				string store = "SP_SF_Procedure_Ins";
				using EWP_Command command = new(store, connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();

				_ = command.Parameters.AddWithValue("_procedureid", ProcessMaster.ProcedureId);
				_ = command.Parameters.AddWithValue("_code", ProcessMaster.Code);
				_ = command.Parameters.AddWithValue("_name", ProcessMaster.Name);
				_ = command.Parameters.AddWithValue("_status", ProcessMaster.Status);
				_ = command.Parameters.AddWithValue("_version", ProcessMaster.Version);
				_ = command.Parameters.AddWithValue("_earlierversion", ProcessMaster.EarlierVersion);
				_ = command.Parameters.AddWithValue("_ActivityTypeCode", ProcessMaster.ActivityType);
				_ = command.Parameters.AddWithValue("_idactivityclass", ProcessMaster.IdActivityClass);
				_ = command.Parameters.AddWithValue("_hasIntervention", ProcessMaster.HasIntervention);
				_ = command.Parameters.AddWithValue("_hasSource", ProcessMaster.HasSource);
				_ = command.Parameters.AddWithValue("_interventionId", ProcessMaster.InterventionId);
				_ = command.Parameters.AddWithValue("_sourceId", ProcessMaster.SourceId);
				_ = command.Parameters.AddWithValue("_description", ProcessMaster.Description);
				_ = command.Parameters.AddWithValue("_createdById", User.Id);
				_ = command.Parameters.AddWithValue("_createNewVersion", ProcessMaster.CreateNewVersion);
				_ = command.Parameters.AddWithValue("_procedureIdOrigin", ProcessMaster.ProcedureIdOrigin);
				_ = command.Parameters.AddWithValue("_IsValidation", IsValidation);
				_ = command.Parameters.AddWithValue("_UpdateEmployee", User.EmployeeId);
				_ = command.Parameters.AddWithValue("_IsByProcedure", ProcessMaster.IsByProcedure);
				_ = command.Parameters.AddWithValue("_IsManualActivity", ProcessMaster.IsManualActivity);
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ResponseData
					{
						Id = rdr["Id"].ToStr(),
						Action = (ActionDB)rdr["Action"].ToInt32(),
						IsSuccess = rdr["IsSuccess"].ToInt32().ToBool(),
						Code = rdr["Code"].ToStr(),
						Version = rdr["Version"].ToInt32(),
						Message = rdr["Message"].ToStr(),
					};
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				string errormsj = ex.Message;
				returnValue = null;
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return returnValue;
	}
	/// <summary>
	///
	/// </summary>
	public ProcedureMasterSectionAttachment GetAttachmentSection(string IdAttachment, string SectionId)
	{
		ProcedureMasterSectionAttachment returnValue = null;
		using (EWP_Connection connection = new(ConnectionString))
		{
			try
			{
				using EWP_Command command = new("SP_SF_Attachment_SEL", connection)
				{
					CommandType = CommandType.StoredProcedure
				};
				command.Parameters.Clear();
				command.Parameters.AddWithValue("_Id", IdAttachment);
				command.Parameters.AddWithValue("_AuxId", SectionId);
				command.Parameters.AddNull("_Entity");
				connection.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				MySqlDataReader rdr = command.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();

				while (rdr.ReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
				{
					returnValue = new ProcedureMasterSectionAttachment
					{
						Id = rdr["Id"].ToStr(),
						TypeCode = rdr["TypeCode"].ToString(),
						Name = rdr["Name"].ToStr(),
						URL = rdr["Name"].ToStr(),
						Extension = rdr["Extension"].ToStr(),
						Size = rdr["Size"].ToStr(),
						File = new FileAttachment()
					};
				}
			}
			catch (Exception ex)
			{
				//logger.Error(ex);
				throw;
			}
			finally
			{
				connection.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
		return returnValue;
	}
    #endregion Procedure
}