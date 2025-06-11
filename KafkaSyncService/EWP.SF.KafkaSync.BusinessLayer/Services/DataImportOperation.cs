using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.Constants;
using System.Text.RegularExpressions;



namespace EWP.SF.KafkaSync.BusinessLayer;

public  class DataImportOperation : IDataImportOperation
{

	private readonly IComponentOperation _componentOperation;

	private readonly IDeviceOperation _deviceOperation;
	private readonly IMeasureUnitOperation _measureUnitOperation;
	private readonly IProcessTypeOperation _processTypeOperation;
	private readonly ICatalogRepo _catalogRepo;
	private readonly IToolOperation _toolOperation;
	private readonly IActivityRepo _activityRepo;
	private readonly IMachineRepo _machineRepo;
	private readonly IDataSyncRepository _dataSyncRepository;
	private readonly IProcedureOperation _procedureOperation;
	private readonly IDataImportRepo _dataImportRepo;

	public DataImportOperation(
	IComponentOperation componentOperation,
	IMeasureUnitOperation measureUnitOperation,
	IProcessTypeOperation processTypeOperation,
	ICatalogRepo catalogRepo,
	IDeviceOperation deviceOperation,
	IToolOperation toolOperation,
	IActivityRepo activityRepo,
	IMachineRepo machineRepo,
	IDataSyncRepository dataSyncRepository,
	IProcedureOperation procedureOperation,
	IDataImportRepo dataImportRepo){
		_componentOperation = componentOperation;
		_measureUnitOperation = measureUnitOperation;
		_processTypeOperation = processTypeOperation;
		_catalogRepo = catalogRepo;
		_deviceOperation = deviceOperation;
		_toolOperation = toolOperation;
		_activityRepo = activityRepo;
		_machineRepo = machineRepo;
		_dataSyncRepository = dataSyncRepository;
		_procedureOperation = procedureOperation;
		_dataImportRepo = dataImportRepo;

	}
    // [GeneratedRegex(@"^[a-zA-Z0-9_. |]*$")]
	// private static partial Regex MyRegex4();
	// private static Regex MyRegexDataImport() => MyRegex4();
 private static Regex MyRegexDataImport()
{
    return new Regex(@"^[a-zA-Z0-9_. |]*$", RegexOptions.Compiled);
}
	/// <summary>
	///
	/// </summary>
	public async Task<List<Entity>> ListEntities()
	{
		List<Entity> lstEntities = await _dataImportRepo.ListEntities().ConfigureAwait(false);
		int order = 0;
		foreach (Entity entity in lstEntities)
		{
			order = 1;
			GetTreeDocsImport(null, entity.ListDocsImport, ref order);
		}
		return lstEntities;
	}

	/// <summary>
	///
	/// </summary>
	// public async Task<ResponseExportCsv> ExportToCsv(string Entity, string Parameter1 = "", string Parameter2 = "",
	// 	 string Parameter3 = "", string Parameter4 = "", string Parameter5 = "", bool WithAttachment = false)
	// {
	// 	ResponseExportCsv returnValue = new()
	// 	{
	// 		Attachment = []
	// 	};
	// 	ResponseAttachment objAdd;
	// 	Report rpt = new()
	// 	{
	// 		Parameters = []
	// 	};
	// 	Parameter obParam;
	// 	string ValueAuxId = "", ValueAttachmentId = "";
	// 	int AuxIdIndex = 0, AttachmentIdIndex = 0;
	// 	const string NameParam = "Entity"
	// 		, NameParam1 = "Parameter1"
	// 		, NameParam2 = "Parameter2", NameParam3 = "Parameter3", NameParam4 = "Parameter4"
	// 		, NameParam5 = "Parameter5"
	// 		, undefined = "undefined"
	// 		, AuxId = "AuxId"
	// 		, AttachmentId = "AttachmentId"
	// 		, Store = "SP_SF_Export_Csv_SEL";

	// 	rpt.Script = Store;
	// 	obParam = new Parameter
	// 	{
	// 		Name = NameParam,
	// 		Value = Entity,
	// 	};
	// 	rpt.Parameters.Add(obParam);

	// 	obParam = new Parameter
	// 	{
	// 		Name = NameParam1,
	// 		Value = Parameter1,
	// 	};
	// 	rpt.Parameters.Add(obParam);
	// 	obParam = new Parameter
	// 	{
	// 		Name = NameParam2,
	// 		Value = Parameter2,
	// 	};
	// 	rpt.Parameters.Add(obParam);
	// 	obParam = new Parameter
	// 	{
	// 		Name = NameParam3,
	// 		Value = Parameter3,
	// 	};
	// 	rpt.Parameters.Add(obParam);
	// 	obParam = new Parameter
	// 	{
	// 		Name = NameParam4,
	// 		Value = Parameter4,
	// 	};
	// 	rpt.Parameters.Add(obParam);
	// 	obParam = new Parameter
	// 	{
	// 		Name = NameParam5,
	// 		Value = Parameter5,
	// 	};
	// 	rpt.Parameters.Add(obParam);

	// 	try
	// 	{
	// 		returnValue.DataCsv = DAL.ExecuteReport(rpt);
	// 		if (returnValue.DataCsv?.Count > 0 && WithAttachment && !Parameter1.Contains(undefined))
	// 		{
	// 			for (int j = 0; j < returnValue.DataCsv.Count; j++)
	// 			{
	// 				string[] obj = returnValue.DataCsv[j];
	// 				if (j == 0)
	// 				{
	// 					AuxIdIndex = Array.IndexOf(obj, AuxId);
	// 					AttachmentIdIndex = Array.IndexOf(obj, AttachmentId);
	// 				}
	// 				else if (j > 0 && AuxIdIndex > 0 && AttachmentIdIndex > 0)
	// 				{
	// 					ValueAuxId = obj[AuxIdIndex];
	// 					ValueAttachmentId = obj[AttachmentIdIndex];
	// 					if (!string.IsNullOrEmpty(ValueAuxId) && !string.IsNullOrEmpty(ValueAttachmentId))
	// 					{
	// 						objAdd = await GetAttachmentAsync(ValueAttachmentId, ValueAuxId, false).ConfigureAwait(false);
	// 						if (objAdd is not null)
	// 						{
	// 							ResponseAttachment objCondition = returnValue.Attachment.Find(x => x.Name == objAdd.Name && x.Size == objAdd.Size);
	// 							if (objCondition is null)
	// 							{
	// 								returnValue.Attachment.Add(objAdd);
	// 							}
	// 						}
	// 					}
	// 					returnValue.DataCsv[j] = [.. obj.Where((_, index) => index != AuxIdIndex && index != AttachmentIdIndex)];
	// 				}
	// 			}
	// 		}
	// 	}
	// 	catch (Exception ex)
	// 	{
	// 		//logger.Error(ex);
	// 		throw;
	// 	}
	// 	return returnValue;
	// }

	/// <summary>
	///
	/// </summary>
	public object GetPropertiesEntity(string entity)
	{
		//Extract Parents from entity
		string[] parent = [];

		List<PropertiesEntityModel> response = (List<PropertiesEntityModel>)PropertiesEntity.GetProperties(entity, parent);
		ProcessElementList(response);
		return response;
	}

	/// <summary>
	///
	/// </summary>
	public void ProcessElementList(List<PropertiesEntityModel> response)
	{
		response.ForEach(el =>
		{
			if (!string.IsNullOrEmpty(el.DefaultMappingEntity))
			{
				el.DefaultMappingEntityList = _dataSyncRepository.ListDefaultMappingEntityObject(el.DefaultMappingEntity);
			}
			else
			{
				if (!string.IsNullOrEmpty(el.Expression))
				{
					// Agregar espacio al regex para que tome estatuses de ordenes de producción
					Regex rg = MyRegexDataImport();
					MatchCollection matches = rg.Matches(el.Expression);
					if (matches.Count > 0)
					{
						List<DefaultMappingEntityObject> entityVals = [];
						foreach (string value in el.Expression.Split('|'))
						{
							DefaultMappingEntityObject obj = new()
							{
								Id = value,
								Name = value
							};
							entityVals.Add(obj);
						}
						el.DefaultMappingEntityList = entityVals;
					}
				}
				if (el.Child is not null)
				{
					ProcessElementList(el.Child);
				}
			}
		});
	}

	/// <summary>
	///
	/// </summary>
	public async Task<ResponseAttachment> GetAttachmentAsync(string AttachmentId, string AuxId, bool FirstLoad)
	{
		ResponseAttachment result;
		try
		{
			// Step 1: Retrieve attachment details from the database
			result = _dataImportRepo.GetAttachment(AttachmentId, AuxId);
			if (result is not null && (result.Type == "File" || result.Type == "Image") && !FirstLoad)
			{
				// Step 2: Resolve relative path to absolute path
				string relativePath = Config.Configuration["PathAttachment"]; // e.g., "../../Attachments"
				string basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
				string pathTemp = Path.Combine(basePath, AttachmentId);

				// Step 3: Check if the file exists and load it
				if (File.Exists(pathTemp))
				{
					await using FileStream fStream = File.OpenRead(pathTemp);
					await using MemoryStream memoryStream = new();
					// Copy file contents to a MemoryStream
					await fStream.CopyToAsync(memoryStream).ConfigureAwait(false);

					// Convert to Base64
					byte[] bytes = memoryStream.ToArray();

					// Populate result object
					result.File.FileBase64 = Convert.ToBase64String(bytes);
					result.File.name = result.Name;
					result.File.size = result.Size.ToStr();
					result.File.type = result.Ext;
				}
			}
		}
		catch (Exception ex)
		{
			//logger.Error(ex);
			throw;
		}

		return result;
	}

	/// <summary>
	///
	/// </summary>
	public List<Activity> GetDataImportTasks(ProcessTypeExternal operationType)
	{
		List<Activity> tasks = [];
		if (operationType.Tasks?.Count > 0)
		{
			List<ActivityClass> listActivityClasses = _activityRepo.ListActivityClasses();
			List<ActivityType> listActivityTypes = _activityRepo.ListActivityTypes();
			operationType.Tasks.ForEach(task =>
			{
				int activityClassId = 0;
				string activityTypeId = string.Empty;
				int triggerId = 0;
				listActivityClasses.ForEach(activityClass =>
				{
					if (string.Equals(activityClass.Name.Replace("@", "").Trim(), task.Class.Trim(), StringComparison.OrdinalIgnoreCase))
					{
						activityClassId = activityClass.Id;
					}
				});
				listActivityTypes.ForEach(activityType =>
				{
					if (string.Equals(activityType.Name.Trim(), task.Type.Trim(), StringComparison.OrdinalIgnoreCase))
					{
						activityTypeId = activityType.Id;
					}
				});
				switch (task.Stage.Trim().ToUpperInvariant())
				{
					case "START":
						triggerId = 1;
						break;

					case "DURING":
						triggerId = 2;
						break;

					case "END":
						triggerId = 3;
						break;
				}
				Activity activity = new()
				{
					Name = task.Name,
					SortId = task.Sort,
					Description = task.Description,
					IsMandatory = string.Equals(task.Mandatory.Trim(), "YES", StringComparison.OrdinalIgnoreCase),
					IsAvailable = string.Equals(task.Available.Trim(), "YES", StringComparison.OrdinalIgnoreCase),
					ActivityClassId = activityClassId,
					ActivityTypeId = activityTypeId,
					TriggerId = triggerId,
					Schedule = new ActivitySchedule
					{
						Duration = task.DurationInSec,
						DurationUnit = 1
					}
				};
				tasks.Add(activity);
			});
		}
		return tasks;
	}

	public List<DeviceSpeed> GetDataImportAvailableDevices(ProductOperation operationType, ProcessEntryProcess oldOperation = null)
	{
		List<DeviceSpeed> returnValue = [];
		List<Machine> machines = _machineRepo.ListMachines();
		if (operationType.OperationMachines?.Count > 0)
		{
			operationType.OperationMachines?.ForEach(machine =>
			{
				DeviceSpeed oldOpMachine = oldOperation?.AvailableDevices?.Find(x => x.Id == machine.MachineCode && x.LineId == machine.LineID);
				oldOpMachine ??= oldOperation?.AvailableDevices?.Find(x => x.LineId == machine.LineID);
				Machine origMachine = machines.Where(x => x.Code == machine.MachineCode)?.FirstOrDefault(x => x.Status != Status.Failed);
				if (origMachine is not null)
				{
					returnValue.Add(new DeviceSpeed
					{
						Id = origMachine.Id,
						ExecTime = machine.OperationTimeInSec,
						AutomaticSequencing = oldOpMachine is not null ? oldOpMachine.AutomaticSequencing : machine.AutomaticSequencing.ToStr().Equals("YES", StringComparison.OrdinalIgnoreCase),
						Schedule = oldOpMachine is not null ? oldOpMachine.Schedule : machine.Schedule.ToStr().Equals("YES", StringComparison.OrdinalIgnoreCase),
						Selected = string.IsNullOrEmpty(machine.Primary) ? (oldOpMachine?.Selected == true) : machine.Primary.ToStr().Equals("yes", StringComparison.OrdinalIgnoreCase),
						Quantity = operationType.Quantity,
						SetupTime = oldOpMachine is not null ? oldOpMachine.SetupTime : machine.SetupTimeInSec,
						LotCapacity = oldOpMachine is not null ? oldOpMachine.LotCapacity : origMachine.LotCapacity,
						TimeUnit = 1,
						WaitTime = oldOpMachine is not null ? oldOpMachine.WaitTime : machine.WaitingTimeInSec,
						LineId = machine.LineID,
						LineUID = oldOpMachine is not null ? oldOpMachine.LineUID : Guid.NewGuid().ToStr(),
						Comments = machine.Comments,
						IsBackflush = string.IsNullOrEmpty(machine.IssueMode) ? (oldOpMachine?.IsBackflush == true) : machine.IssueMode.ToLowerInvariant().Contains("backflush")
					});
				}
			});
		}
		else
		{
			returnValue.Add(new DeviceSpeed
			{
				Id = "00000000-0000-0000-0000-000000000000",
				ExecTime = 1,
				AutomaticSequencing = true,
				Schedule = false,
				Selected = true,
				Quantity = operationType.Quantity,
				SetupTime = 0,
				TimeUnit = 1,
				WaitTime = 0,
				LineUID = Guid.NewGuid().ToString(),
				LineId = "0"
			});
		}
		return returnValue;
	}

	public  async Task<List<SubProduct>> GetDataImportSubProducts(ProductOperation operationType)
	{
		List<SubProduct> returnValue = [];
		List<MeasureUnit> units = _measureUnitOperation.GetMeasureUnits()?.Where(x => x.IsProductionResult && x.Status != Status.Failed).ToList();
		foreach (ProductOperationByProduct material in operationType.OperationByProducts)
		{
			Component origComponent = (await _componentOperation.GetComponents(material.ItemCode).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault();
			if (origComponent is not null)
			{
				MeasureUnit origUnit = units.Find(zx => zx.Code == material.InventoryUoM) ?? throw new ResponseDataException("InventoryUOM '" + material.InventoryUoM + "' does not exist");
				returnValue.Add(new SubProduct
				{
					ComponentId = origComponent.Id,
					Name = origComponent.Name,
					Code = material.ItemCode,
					Factor = material.Quantity,
					WarehouseCode = material.WarehouseCode,
					UnitId = origUnit.Code,
					LineId = material.LineID.ToStr(),
					Comments = material.Comments
				});
			}
			else
			{
				throw new ResponseDataException("ItemCode '" + material.ItemCode + "' does not exist");
			}
		}
		return returnValue;
	}

	public  List<ProcessEntryAttribute> GetDataImportAttributes(ProductOperation operationType)
	{
		List<ProcessEntryAttribute> returnValue = [];

		operationType.Attributes?.ForEach(attr =>
			{
				returnValue.Add(new ProcessEntryAttribute
				{
					AttributeId = "",
					Selected = true,
				});
			});
		return returnValue;
	}

	// private List<ProcessEntryTool> GetDataImportOperationTools(ProductOperation operationType)
	// {
	// 	List<ProcessEntryTool> returnValue = [];
	// 	List<ToolType> toolTypes = _toolOperation.ListToolTypes("");
	// 	operationType.OperationTools?.ForEach(optool =>
	// 		{
	// 			ToolType origTool = toolTypes.Where(x => x.Code == optool.ToolingCode).FirstOrDefault(x => x.Status != Status.Failed) ?? throw new Exception("ToolingCode '" + optool.ToolingCode + "' does not exist");
	// 			ProcessEntryTool newtool = new()
	// 			{
	// 				ToolId = origTool.Id,
	// 				Cost = 0,
	// 				LineId = optool.LineID.ToStr(),
	// 				Quantity = optool.Quantity,
	// 				Comments = optool.Comments
	// 			};
	// 			switch (optool.Usage.ToUpperInvariant())
	// 			{
	// 				case "INCREMENTSTART":
	// 					newtool.Usage = "1";
	// 					break;

	// 				case "INCREMENTEND":
	// 					newtool.Usage = "2";
	// 					break;

	// 				case "DECREMENTSTART":
	// 					newtool.Usage = "3";
	// 					break;

	// 				case "DECREMENTEND":
	// 					newtool.Usage = "4";
	// 					break;

	// 				case "INCREMENTFORPROCESSTIMEONLY":
	// 					newtool.Usage = "5";
	// 					break;

	// 				case "DECREMENTFORPROCESSTIMEONLY":
	// 					newtool.Usage = "6";
	// 					break;

	// 				case "NOCHANGE":
	// 					newtool.Usage = "7";
	// 					break;

	// 				case "INCREMENTTOEND":
	// 					newtool.Usage = "8";
	// 					break;

	// 				case "DECREMENTTOEND":
	// 					newtool.Usage = "9";
	// 					break;

	// 				case "INCREMENTSETUPTIMEONLY":
	// 					newtool.Usage = "10";
	// 					break;

	// 				case "DECREMENTSETUPTIMEONLY":
	// 					newtool.Usage = "11";
	// 					break;

	// 				case "INCREMENTFROMSTARTOFSETUP":
	// 					newtool.Usage = "12";
	// 					break;

	// 				case "DECREMENTFROMSTARTOFSETUP":
	// 					newtool.Usage = "13";
	// 					break;

	// 				case "INCREMENTFORENTIREJOB":
	// 					newtool.Usage = "14";
	// 					break;

	// 				case "DECREMENTFORENTIREJOB":
	// 					newtool.Usage = "15";
	// 					break;

	// 				default:
	// 					newtool.Usage = "7";
	// 					break;
	// 			}
	// 			returnValue.Add(newtool);
	// 		});
	// 	return returnValue;
	// }

	/// <summary>
	///
	/// </summary>
	public List<Activity> GetDataImportTasks(ProductOperation operationType, User systemOperator)
	{
		List<Activity> tasks = [];
		if (operationType.Tasks?.Count > 0)
		{
			List<ActivityClass> listActivityClasses = _activityRepo.ListActivityClasses();
			List<ActivityType> listActivityTypes = _activityRepo.ListActivityTypes();
			List<ActivitySource> listActivitySource = _activityRepo.ListActivitySources();
			List<Intervention> listActivityIntervention = _activityRepo.ListActivityInterventions();
			operationType.Tasks.ForEach(task =>
			{
				int activityClassId = 0;
				string activityTypeId = string.Empty;
				string activitySource = string.Empty;
				string activityIntervention = string.Empty;
				int triggerId = 0;

				listActivityClasses.ForEach(activityClass =>
				{
					if (task.Class is null || string.IsNullOrEmpty(task.Class))
					{
						throw new Exception(string.Format("task Order No.{0} : Class not found. ", task.Sort));
					}
					else if (string.Equals(activityClass.Name.Replace("@", "").Trim(), task.Class.Trim(), StringComparison.OrdinalIgnoreCase))
					{
						activityClassId = activityClass.Id;
					}
				});
				if (!string.IsNullOrEmpty(task.Type) && task.Type is not null)
				{
					listActivityTypes.ForEach(activityType =>
					{
						if (string.Equals(activityType.Name.Trim(), task.Type.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							activityTypeId = activityType.Id;
						}
					});
				}

				if (!string.IsNullOrEmpty(task.SourceCode) && task.SourceCode is not null)
				{
					listActivitySource.ForEach(source =>
					{
						if (string.Equals(source.Name.Trim(), task.SourceCode.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							activitySource = source.Name;
						}
					});
				}
				if (!string.IsNullOrEmpty(task.InterventionCode) && task.InterventionCode is not null)
				{
					listActivityIntervention.ForEach(type =>
					{
						if (string.Equals(type.Name.Trim(), task.InterventionCode.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							activityIntervention = type.Id;
						}
					});
				}
				if (string.IsNullOrEmpty(task.Stage) && task.Stage is null)
				{
					throw new Exception(string.Format("task Order No.{0} : Stage not found. ", task.Sort));
				}

				switch (task.Stage.Trim().ToUpperInvariant())
				{
					case "START":
						triggerId = 1;
						break;

					case "DURING":
						triggerId = 2;
						break;

					case "END":
						triggerId = 3;
						break;
				}
				Activity activity = new()
				{
					Name = task.Name,
					SortId = task.Sort,
					Description = task.Description,
					IsMandatory = string.Equals(task.Mandatory.Trim(), "YES", StringComparison.OrdinalIgnoreCase),
					IsAvailable = string.Equals(task.Available.Trim(), "YES", StringComparison.OrdinalIgnoreCase),
					IsShift = false,
					Origin = OriginActivity.Product.ToStr(),
					ActivityClassId = activityClassId,
					ActivityTypeId = activityTypeId,
					TriggerId = triggerId,
					Status = Status.Active,
					InterventionId = activityIntervention,
					SourceId = activitySource,
					Schedule = new ActivitySchedule
					{
						Duration = task.DurationInSec,
						DurationUnit = 1,
						FrequencyMode = task.FrequencyMode,
						FreqValue = task.FreqValue
					}
				};

				if (task.ProcedureCode is not null && !string.IsNullOrEmpty(task.ProcedureCode) && task.Version == 0)
				{
					throw new Exception(string.Format("Tasrk Sort {0} : Version Procedure not found. ", task.Sort));
				}
				Procedure itemProcedure = _procedureOperation.GetProcessByProcessCodeVersion(task.ProcedureCode, task.Version);
				if (itemProcedure is not null)
				{
					activity.CurrentProcessMaster = itemProcedure;
					activity.ParentId = itemProcedure.ProcedureId;
					activity.Name = itemProcedure.Name;
					activity.RequiresInstructions = true;

					if (task.TasksMaterials?.Count > 0
					&& activity.CurrentProcessMaster.Sections is not null
					&& operationType.OperationItems?.Count > 0)
					{
						task.TasksMaterials.ForEach(material =>
						{
							ProductOperationItem item = operationType.OperationItems.Find(x => x.ItemCode == material.ItemCode) ?? throw new Exception(string.Format("{0} : Material not found. ", material.ItemCode));
							ProcedureSection section = activity.CurrentProcessMaster.Sections.Find(x => x.OrderSection == material.SectionOrder) ?? throw new Exception(string.Format("Section Order No.{0} : Section not found. ", material.SectionOrder));
							if (item is not null && section is not null)
							{
								ProcedureMasterInstruction instruction = section.ListInstrucctions[0];
								instruction.Components ??= [];
								ComponentInstruction itemAdd = new()
								{
									Code = item.ItemCode,
									Id = Guid.NewGuid().ToStr(),
									UnitId = item.InventoryUoM,
									InstructionId = instruction.InstructionId,
									ProcedureId = itemProcedure.ProcedureId,
									ComponentId = item.ItemCode,
									TypeComponent = item.Type,

									Line = (instruction.Components.Count + 1).ToStr(),
									Name = item.ItemCode
								};
								itemAdd.Line = material.Sort.ToStr();
								itemAdd.Quantity = material.QuantityPercentage;
								itemAdd.Tolerance = material.Tolerance;
								itemAdd.Mandatory = string.Equals(material.Mandatory.Trim(), "YES", StringComparison.OrdinalIgnoreCase);
								itemAdd.IsRemainingTotal = string.Equals(material.IsRemainingTotal.Trim(), "YES", StringComparison.OrdinalIgnoreCase);

								instruction.Components.Add(itemAdd);
							}
						});
					}
				}
				else
				{
					activity.CurrentProcessMaster = new();
				}

				tasks.Add(activity);
			});
		}
		return tasks;
	}

	/// <summary>
	///
	/// </summary>
	public List<Activity> GetDataImportOerderTasks(Common.Models.WorkOrderOperation operationType, OrderProcess currentProcess)
	{
		List<Activity> tasks = [];
		if (operationType.Tasks?.Count > 0)
		{
			List<ActivityClass> listActivityClasses = _activityRepo.ListActivityClasses();
			List<ActivityType> listActivityTypes = _activityRepo.ListActivityTypes();
			List<ActivitySource> listActivitySource = _activityRepo.ListActivitySources();
			List<Intervention> listActivityIntervention = _activityRepo.ListActivityInterventions();
			operationType.Tasks.ForEach(task =>
			{
				int activityClassId = 0;
				string activityTypeId = string.Empty;
				string activitySource = string.Empty;
				string activityIntervention = string.Empty;
				int triggerId = 0;
				//task.ProcessId = 0;

				listActivityClasses.ForEach(activityClass =>
				{
					if (task.Class is null || string.IsNullOrEmpty(task.Class))
					{
						throw new Exception(string.Format("task Order No.{0} : Class not found. ", task.Sort));
					}
					else if (string.Equals(activityClass.Name.Replace("@", "").Trim(), task.Class.Trim(), StringComparison.OrdinalIgnoreCase))
					{
						activityClassId = activityClass.Id;
					}
				});
				if (!string.IsNullOrEmpty(task.Type) && task.Type is not null)
				{
					listActivityTypes.ForEach(activityType =>
					{
						if (string.Equals(activityType.Name.Trim(), task.Type.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							activityTypeId = activityType.Id;
						}
					});
				}

				if (!string.IsNullOrEmpty(task.SourceCode) && task.SourceCode is not null)
				{
					listActivitySource.ForEach(source =>
					{
						if (string.Equals(source.Name.Trim(), task.SourceCode.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							activitySource = source.Name;
						}
					});
				}
				if (!string.IsNullOrEmpty(task.InterventionCode) && task.InterventionCode is not null)
				{
					listActivityIntervention.ForEach(type =>
					{
						if (string.Equals(type.Name.Trim(), task.InterventionCode.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							activityIntervention = type.Id;
						}
					});
				}
				if (string.IsNullOrEmpty(task.Stage) && task.Stage is null)
				{
					throw new Exception(string.Format("task Order No.{0} : Stage not found. ", task.Sort));
				}

				switch (task.Stage.Trim().ToUpperInvariant())
				{
					case "START":
						triggerId = 1;
						break;

					case "DURING":
						triggerId = 2;
						break;

					case "END":
						triggerId = 3;
						break;
				}
				Activity activity = new()
				{
					Name = task.Name,
					SortId = task.Sort,
					Description = task.Description,
					IsMandatory = string.Equals(task.Mandatory.Trim(), "YES", StringComparison.OrdinalIgnoreCase),
					IsAvailable = string.Equals(task.Available.Trim(), "YES", StringComparison.OrdinalIgnoreCase),
					IsShift = false,
					Origin = OriginActivity.Product.ToStr(),
					ProcessId = currentProcess.ProcessId,
					ActivityClassId = activityClassId,
					ActivityTypeId = activityTypeId,
					TriggerId = triggerId,
					Status = Status.Active,
					InterventionId = activityIntervention,
					SourceId = activitySource,
					Schedule = new ActivitySchedule
					{
						Duration = task.DurationInSec,
						DurationUnit = 1,
						FrequencyMode = task.FrequencyMode,
						FreqValue = task.FreqValue
					}
				};

				if (task.ProcedureCode is not null && !string.IsNullOrEmpty(task.ProcedureCode) && task.Version == 0)
				{
					throw new Exception(string.Format("Tasrk Sort {0} : Version Procedure not found. ", task.Sort));
				}
				Procedure itemProcedure = _procedureOperation.GetProcessByProcessCodeVersion(task.ProcedureCode, task.Version);
				if (itemProcedure is not null)
				{
					activity.CurrentProcessMaster = itemProcedure;
					activity.ParentId = itemProcedure.ProcedureId;
					activity.Name = itemProcedure.Name;
					activity.RequiresInstructions = true;

					if (task.TasksMaterials?.Count > 0
					&& activity.CurrentProcessMaster.Sections is not null
					&& operationType.Items?.Count > 0)
					{
						task.TasksMaterials.ForEach(material =>
						{
							WorkOrderItem item = operationType.Items.Find(x => x.ItemCode == material.ItemCode) ?? throw new Exception(string.Format("{0} : Material not found. ", material.ItemCode));
							ProcedureSection section = activity.CurrentProcessMaster.Sections.Find(x => x.OrderSection == material.SectionOrder) ?? throw new Exception(string.Format("Section Order No.{0} : Section not found. ", material.SectionOrder));
							if (item is not null && section is not null)
							{
								ProcedureMasterInstruction instruction = section.ListInstrucctions[0];
								instruction.Components ??= [];
								ComponentInstruction itemAdd = new()
								{
									Id = Guid.NewGuid().ToStr(),
									UnitId = item.InventoryUoM,
									Code = item.ItemCode,
									InstructionId = instruction.InstructionId,
									ProcedureId = itemProcedure.ProcedureId,
									ComponentId = item.ItemCode,
									TypeComponent = item.Type,
									Line = (instruction.Components.Count + 1).ToStr(),
									Name = item.ItemCode,
									Quantity = material.QuantityPercentage,
									Tolerance = material.Tolerance
								};
								itemAdd.Line = material.Sort.ToStr();
								itemAdd.Mandatory = string.Equals(material.Mandatory.Trim(), "YES", StringComparison.OrdinalIgnoreCase);
								itemAdd.IsRemainingTotal = string.Equals(material.IsRemainingTotal.Trim(), "YES", StringComparison.OrdinalIgnoreCase);

								instruction.Components.Add(itemAdd);
							}
						});
					}
				}
				else
				{
					activity.CurrentProcessMaster = new();
				}

				tasks.Add(activity);
			});
		}
		return tasks;
	}

	private static void GetTreeDocsImport(string Parent, List<EntityDocImport> docsImports, ref int order, int level = 1, string path = "")
	{
		foreach (EntityDocImport doc in docsImports.Where(x => x.ParentEntityExternal == Parent))
		{
			doc.Order = order;
			order++;
			doc.Path = path + level.ToString() + ".";
			GetTreeDocsImport(doc.NameEntityExternal, docsImports, ref order, 1, path + level.ToString() + ".");
			level++;
		}
	}

	/// <summary>
	///
	/// </summary>
	public List<ProcessEntryTool> GetDataImportTooling(ProductExternal item, ProcessEntry pe, User systemOperator)
	{
		List<ProcessEntryTool> returnValue = [];
		List<ProcessType> processTypes = _processTypeOperation.GetProcessTypes(string.Empty, systemOperator);
		Machine[] machines = _deviceOperation.ListDevices(false, true, true);

		item.Operations.ForEach(op =>
		{
			ProcessType processType = processTypes.Find(pt => string.Equals(pt.Code, op.OperationType, StringComparison.OrdinalIgnoreCase));
			if (processType is not null)
			{
				ProcessEntryProcess process = pe.Processes.Find(pr => pr.ProcessTypeId == processType.Id);
				op.OperationTools ??= [];
				op.OperationTools.ForEach(tool =>
				{
					ToolType currentTool = _toolOperation.ListToolTypes(tool.ToolingCode).Find(x => x.Status != Status.Failed);
					int UsageId = Array.IndexOf(RegularExpression.UsageRegex.Split('|'), tool.Usage) + 1;
					returnValue.Add(
					 new ProcessEntryTool { ProcessId = process.ProcessId, LineId = tool.LineID.ToStr(), ToolId = currentTool.Id, MachineId = "", Quantity = tool.Quantity, Schedule = tool.Schedule.ToStr().Equals("YES", StringComparison.OrdinalIgnoreCase), Usage = UsageId.ToStr(), Cost = 1, Comments = tool.Comments, IsBackflush = tool.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase), Source = tool.Source }
					 );
				});

				op.OperationMachines?.ForEach(machine =>
					{
						machine.MachineTools?.ForEach(tool =>
							{
								Machine currentMachine = machines.FirstOrDefault(m => string.Equals(m.Code, machine.MachineCode, StringComparison.OrdinalIgnoreCase));
								ToolType currentTool = _toolOperation.ListToolTypes(tool.ToolingCode).Find(x => x.Status != Status.Failed);
								int UsageId = Array.IndexOf(RegularExpression.UsageRegex.Split('|'), tool.Usage) + 1;
								returnValue.Add(
								 new ProcessEntryTool { ProcessId = process.ProcessId, LineId = tool.LineID.ToStr(), ToolId = currentTool.Id, MachineId = currentMachine?.Id, Quantity = tool.Quantity, Schedule = tool.Schedule.ToStr().Equals("YES", StringComparison.OrdinalIgnoreCase), Usage = UsageId.ToStr(), Cost = 1, Comments = tool.Comments, IsBackflush = tool.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase), Source = tool.Source }
								 );
							});
					});
			}
			else
			{
				throw new Exception("Invalid Operation Type");
			}
		});

		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public List<ProcessEntryLabor> GetDataImportLabor(ProductExternal item, ProcessEntry pe, User systemOperator)
	{
		List<ProcessEntryLabor> returnValue = [];
		List<ProcessType> processTypes = _processTypeOperation.GetProcessTypes(string.Empty, systemOperator);
		Machine[] machines = _deviceOperation.ListDevices(false, true, true);

		item.Operations.ForEach(op =>
		{
			ProcessType processType = processTypes.Find(pt => string.Equals(pt.Code, op.OperationType, StringComparison.OrdinalIgnoreCase));
			if (processType is not null)
			{
				ProcessEntryProcess process = pe.Processes.Find(pr => pr.ProcessId.ToDouble() == op.OperationNo.ToDouble());
				op.OperationLabor ??= [];
				op.OperationLabor.ForEach(prof =>
				{
					CatProfile currentProfile = _catalogRepo.GetCatalogProfile(prof.ProfileCode).Find(x => x.Status != Status.Failed);
					int UsageId = Array.IndexOf(RegularExpression.UsageRegex.Split('|'), prof.Usage) + 1;
					returnValue.Add(
					 new ProcessEntryLabor { ProcessId = process.ProcessId, LineId = prof.LineID.ToStr(), LaborId = currentProfile.ProfileId, MachineId = "", Quantity = prof.Quantity, Schedule = prof.Schedule.ToStr().Equals("YES", StringComparison.OrdinalIgnoreCase), Usage = UsageId.ToStr(), Cost = 1, Comments = prof.Comments, IsBackflush = prof.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase), Source = prof.Source }
					 );
				});

				op.OperationMachines?.ForEach(machine =>
					{
						machine.MachineLabor?.ForEach(prof =>
							{
								Machine currentMachine = machines.FirstOrDefault(m => string.Equals(m.Code, machine.MachineCode, StringComparison.OrdinalIgnoreCase));
								CatProfile currentProfile = _catalogRepo.GetCatalogProfile(prof.ProfileCode).Find(x => x.Status != Status.Failed);
								int UsageId = Array.IndexOf(RegularExpression.UsageRegex.Split('|'), prof.Usage) + 1;
								returnValue.Add(
								 new ProcessEntryLabor { ProcessId = process.ProcessId, LineId = prof.LineID.ToStr(), LaborId = currentProfile.ProfileId, MachineId = currentMachine.Id, Quantity = prof.Quantity, Schedule = prof.Schedule.ToStr().Equals("YES", StringComparison.OrdinalIgnoreCase), Usage = UsageId.ToStr(), Cost = 1, Comments = prof.Comments, IsBackflush = prof.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase), Source = prof.Source }
								 );
							});
					});
			}
			else
			{
				throw new Exception("Invalid Operation Type");
			}
		});
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	public async Task<List<ProcessEntryComponent>> GetDataImportItems(ProductExternal item, ProcessEntry pe, User systemOperator)
	{
		List<ProcessEntryComponent> returnValue = [];
		List<ProcessType> processTypes = _processTypeOperation.GetProcessTypes(string.Empty, systemOperator);
		MeasureUnit[] units = _measureUnitOperation.GetMeasureUnits()?.Where(x => x.IsProductionResult).ToArray();
		foreach (ProductOperation op in item.Operations)
		{
			ProcessType processType = processTypes.Find(pt => string.Equals(pt.Code, op.OperationType, StringComparison.OrdinalIgnoreCase));
			if (processType is not null)
			{
				ProcessEntryProcess process = pe.Processes.Find(pr => pr.ProcessId.ToDouble() == op.OperationNo.ToDouble());

				if (op.OperationItems is not null)
				{
					foreach (ProductOperationItem itm in op.OperationItems)
					{
						Component opComp = (await _componentOperation.GetComponents(itm.ItemCode, true).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault();
						if (opComp is not null)
						{
							MeasureUnit itmUnit = units.FirstOrDefault(x => string.Equals(x.Code, itm.InventoryUoM, StringComparison.OrdinalIgnoreCase));
							if (string.IsNullOrEmpty(itm.Schedule))
							{
								itm.Schedule = "YES";
							}
							returnValue.Add(
							 new ProcessEntryComponent { ProcessId = process.ProcessId, Code = itm.ItemCode, ComponentId = opComp.Id, ComponentType = ComponentType.Material, Quantity = itm.Quantity, Step = process.Step, ProcessTypeId = process.ProcessTypeId, Name = opComp.Name, UnitId = itmUnit.Code, WarehouseCode = itm.WarehouseCode, IsBackflush = itm.IssueMethod.Contains("backflush", StringComparison.OrdinalIgnoreCase), LineId = itm.LineID.ToStr(), Source = itm.Source, Class = string.Equals(itm.Type.ToStr(), "MATERIAL", StringComparison.OrdinalIgnoreCase) ? 1 : 2, Comments = itm.Comments, IsSchedule = itm.Schedule.ToStr().Equals("YES", StringComparison.Ordinal) }
							 );
						}
						else
						{
							throw new Exception(string.Format("Operation {0} Item code {1} is invalid", op.OperationType, itm.ItemCode));
						}
					}
				}
			}
			else
			{
				throw new Exception("Invalid Operation Type");
			}
		}
		return returnValue;
	}


}
