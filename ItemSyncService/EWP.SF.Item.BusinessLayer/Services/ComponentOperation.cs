using EWP.SF.Common.Models;
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using EWP.SF.Common.Models.Catalogs;


namespace EWP.SF.Item.BusinessLayer;

public class ComponentOperation : IComponentOperation
{
	private readonly IComponentRepo _componentRepo;

	private readonly IDataSyncServiceOperation _dataSyncServiceOperation;
	private readonly IApplicationSettings _applicationSettings;

	private readonly IWarehouseOperation _warehouseOperation;

	private readonly IAttachmentOperation _attachmentOperation;

	public ComponentOperation(IComponentRepo componentRepo, IApplicationSettings applicationSettings
	, IAttachmentOperation attachmentOperation, IWarehouseOperation warehouseOperation, IDataSyncServiceOperation dataSyncServiceOperation)
	{
		_componentRepo = componentRepo;
		_applicationSettings = applicationSettings;
		_attachmentOperation = attachmentOperation;
		_warehouseOperation = warehouseOperation;
		_dataSyncServiceOperation = dataSyncServiceOperation;
	}
	public Component GetComponentByCode(string Code)
	{
		return _componentRepo.GetComponentByCode(Code);
}
	public async Task<List<ResponseData>> ListUpdateProduct(List<ProductExternal> itemList, List<ProductExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		List<ProcedureExternal> proceduresExternal = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		DataSyncErp currentERP = _dataSyncServiceOperation.ListDataSyncERP("[Active]", EnableType.No).FirstOrDefault();

		if (itemList?.Count > 0)
		{
			// Catálogos Necesarios para validar productos
			Machine[] machines = ListDevices(false, true, true);
			List<Warehouse> warehouses = ListWarehouse(systemOperator);
			List<MeasureUnit> units = GetMeasureUnits();
			MeasureUnit[] measures = [.. units.Where(x => x.IsProductionResult)];
			// List<Component> allComponents = GetComponents(string.Empty, true);
			List<ProcessType> processTypes = GetProcessTypes(string.Empty, systemOperator);
			ProcessTypeSubtype[] subProcessTypes = [.. processTypes.SelectMany(c => c.SubTypes)];
			bool MultiVersionEnabled = Config.Configuration["Product-Versioning"].ToBool();
			bool MultiWarehouseEnabled = Config.Configuration["Product-MultiWarehouse"].ToBool();
			bool isForcedEdit = false;
			NotifyOnce = itemList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (ProductExternal cycleItem in itemList)
			{
				ProductExternal item = cycleItem;
				Line++;
				try
				{
					BaseId = item.ProductCode;
					string warehouseId = string.Empty;
					Warehouse warehouse = warehouses.Where(x => string.Equals(x.Code, item.WarehouseCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(x => x.Status != Status.Failed);
					if (warehouse is null)
					{
						throw new Exception("Invalid Warehouse code");
					}
					else
					{
						warehouseId = warehouse.WarehouseId;
					}
					//Version
					if (MultiVersionEnabled && item.Version == 0)
					{
						throw new Exception("Product version is required");
					}
					//Secuencia
					if (item.Sequence == 0)
					{
						item.Sequence = 1;
					}
					ProcessEntry existingEntry = (await GetProcessEntry(item.ProductCode, warehouseId, item.Version, item.Sequence, systemOperator).ConfigureAwait(false))?.Find(x => x.Status != Status.Failed);
					bool editMode = existingEntry is not null;
					if (editMode && itemListOriginal is not null)
					{
						item = itemListOriginal.Find(x => x.ProductCode == cycleItem.ProductCode && x.WarehouseCode == cycleItem.WarehouseCode && x.Version == cycleItem.Version);
						item ??= cycleItem;
					}
					if (!editMode && !string.Equals(item.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase))
					{
						throw new Exception("Cannot import a new disabled product");
					}
					List<ValidationResult> results = [];
					ValidationContext context = new(item, null, null);
					if (!Validator.TryValidateObject(item, context, results))
					{
						throw new Exception($"{results[0]}");
					}
					//Validaciones Iniciales

					string measureUnitId = string.Empty;
					UnitType measureUnitType = 0;
					//Se valida que tenga Llave el producto

					Component existingComponent = (await GetComponents(item.ProductCode, true).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault();
					if (existingComponent is null)
					{
						List<ComponentExternal> compList =
						[
							new ComponentExternal
							{
								InventoryUoM = item.InventoryUoM,
								ItemCode = item.ProductCode,
								ItemName = string.IsNullOrEmpty(item.ProductName) ? item.ProductCode : item.ProductName,
								ProductionUoM = item.InventoryUoM,
								Type = "Production",
								ManagedBy = "None"
							},
						];
						await ListUpdateComponent(compList, systemOperator, Validate, Level).ConfigureAwait(false);
						existingComponent = (await GetComponents(item.ProductCode, true).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault();
					}
					// Se repiten las validaciones por si cambio
					warehouse = warehouses.Find(x => string.Equals(x.Code, item.WarehouseCode, StringComparison.OrdinalIgnoreCase));
					if (warehouse is null)
					{
						throw new Exception("Invalid Warehouse code");
					}
					else
					{
						warehouseId = warehouse.WarehouseId;
					}
					//Version
					if (MultiVersionEnabled && item.Version == 0)
					{
						throw new Exception("Product version is required");
					}
					//Secuencia
					if (item.Sequence == 0)
					{
						item.Sequence = 1;
					}

					if (existingComponent is not null)
					{
						existingComponent.ComponentType = ComponentType.Product;
					}

					// Nombre de producto
					if (string.IsNullOrEmpty(item.ProductName))
					{
						if (existingComponent is not null && !string.IsNullOrEmpty(existingComponent.Name))
						{
							item.ProductName = existingComponent.Name;
						}
						else
						{
							item.ProductName = item.ProductCode;
						}
					}

					if (!editMode && string.IsNullOrEmpty(item.InventoryUoM))
					{
						throw new Exception("Inventory UoM is required");
					}
					else if (!editMode || !string.IsNullOrEmpty(item.InventoryUoM))
					{
						MeasureUnit inventoryUnit = measures.FirstOrDefault(x => string.Equals(x.Code, item.InventoryUoM, StringComparison.OrdinalIgnoreCase));
						if (inventoryUnit is null)
						{
							throw new Exception("Invalid Inventory UoM code");
						}
						else
						{
							measureUnitId = inventoryUnit.Code;
							measureUnitType = inventoryUnit.Type;
						}
					}

					double[] duplicatedOperations = [.. item.Operations.GroupBy(x => x.OperationNo).Where(g => g.Count() > 1).Select(y => y.Key)];
					if (duplicatedOperations?.Length > 0)
					{
						throw new Exception(string.Format("Product one or more OperationNo values are duplicated"));
					}

					// Validando Operaciones
					int opCount = 0;
					foreach (ProductOperation operation in item.Operations)
					{
						opCount++;

						if (operation.OperationNo < 0)
						{
							throw new Exception(string.Format("Product Operation at position [{0}] : OperationNo is required. ", opCount));
						}

						// Para validar: Operation Type va a venir en el endpoint?
						ProcessTypeSubtype CurrentOperationSubType = subProcessTypes.FirstOrDefault(pt => string.Equals(pt.Code, operation.OperationSubtype, StringComparison.OrdinalIgnoreCase));
						ProcessType CurrentOperationType = null;
						if (CurrentOperationSubType is null)
						{
							throw new Exception(string.Format("Product Operation No.{0} : SubOperationType not found. ", operation.OperationSubtype));
						}
						else
						{
							CurrentOperationType = processTypes.Find(op => op.Id == CurrentOperationSubType.ProcessTypeId);
							operation.OperationType = CurrentOperationType.Id;
						}

						if (CurrentOperationType is null)
						{
							throw new Exception(string.Format("Product Operation No.{0} : SubOperationType parent not found. ", operation.OperationSubtype));
						}

						//Validar OutputUoM
						if (string.IsNullOrEmpty(operation.OutputUoM))
						{
							throw new Exception(string.Format("Product Operation No.{0} : OutputUoM is required. ", operation.OperationNo));
						}
						else
						{   // Validar Tipo de operación
							MeasureUnit operationUnit = measures.FirstOrDefault(x => string.Equals(x.Code, operation.OutputUoM, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception(string.Format("Product Operation No.{0} : OutputUoM is invalid ", operation.OperationNo));
							operationUnit = measures.FirstOrDefault(x => string.Equals(x.Code, operation.OutputUoM, StringComparison.OrdinalIgnoreCase) && x.Type.ToInt32() == CurrentOperationType.UnitTypeId);
							if (operationUnit is null)
							{
								throw new Exception(string.Format("Product Operation No.{0} : OutputUoM is invalid for OperationType {1} ", operation.OperationNo, operation.OperationType));
							}
						}

						if (operation.OperationItems is not null)
						{
							int countItems = 0;
							foreach (ProductOperationItem itm in operation.OperationItems)
							{
								countItems++;
								Component itemComp = ((await GetComponents(itm.ItemCode, true).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault()) ?? throw new Exception(string.Format("Product Operation No.{0} : ItemCode at position [{1}] is invalid. ", operation.OperationNo, countItems));
								MeasureUnit ItemUOM = (measures.Where(x => x.Code.Equals(itm.InventoryUoM?.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()) ?? throw new Exception(string.Format("Product Operation No.{0} - Item {1} : InventoryUoM not found or disabled. ", operation.OperationNo, itm.ItemCode));
								Warehouse ItemWhs = (warehouses.Where(x => x.Code.Equals(itm.WarehouseCode?.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))?.FirstOrDefault()) ?? throw new Exception(string.Format("Product Operation No.{0} - Item {1} : WarehouseCode not found. ", operation.OperationNo, itm.ItemCode));
							}
						}
						if (operation.OperationLabor is not null)
						{
							int countItems = 0;
							operation.OperationLabor.ForEach(itm =>
							{
								countItems++;
								CatProfile itemComp = (_componentRepo.GetCatalogProfile(itm.ProfileCode)?.Find(x => x.Status != Status.Failed)) ?? throw new Exception(string.Format("Product Operation No.{0} - Labor {1} : ProfileCode not found. ", operation.OperationNo, itm.ProfileCode));
							});
						}
						if (operation.OperationTools is not null)
						{
							int countItems = 0;
							operation.OperationTools.ForEach(itm =>
							{
								countItems++;
								ToolType itemComp = (_componentRepo.ListToolType(itm.ToolingCode)?.Find(x => x.Status != Status.Failed)) ?? throw new Exception(string.Format("Product Operation No.{0} - ToolingType {1} : ToolingCode is invalid. ", operation.OperationNo, itm.ToolingCode));
							});
						}
						if (operation.OperationMachines is not null)
						{
							int machineCount = 0;
							operation.OperationMachines.ForEach(machine =>
							{
								machineCount++;
								if (string.IsNullOrEmpty(machine.MachineCode))
								{
									throw new Exception(string.Format("Product Operation No.{0} - Machine at pos. [{1}] - MachineCode is required. ", operation.OperationNo, machineCount));
								}
								else
								{
									Machine currentMachine = machines.FirstOrDefault(m => string.Equals(m.Code, machine.MachineCode, StringComparison.OrdinalIgnoreCase) && m.Status == Status.Active && m.OEEConfiguration is not null) ?? throw new Exception(string.Format("Product Operation No.{0} - Machine[{1}] : MachineCode is invalid. ", operation.OperationNo, machine.MachineCode));
									//if (currentMachine is not null && currentMachine.TypeId != CurrentOperationType.Id)
									//{
									//    throw new Exception(string.Format("Product Operation No.{0} - Machine {1} : OperationType not assigned to this machine. ", operation.OperationNo, machine.MachineCode));
									//}
								}

								if (machine.MachineLabor is not null)
								{
									int countItems = 0;
									machine.MachineTools.ForEach(itm =>
									{
										countItems++;
										Tool itemComp = _componentRepo.ListTools(itm.ToolingCode).FirstOrDefault() ?? throw new Exception(string.Format("Product Operation No.{0} - Machine {1} - ToolintType {2} : Tooling Code is invalid. ", operation.OperationNo, machine.MachineCode, itm.ToolingCode));
									});
								}
								if (machine.MachineTools is not null)
								{
									int countItems = 0;
									machine.MachineLabor.ForEach(itm =>
									{
										countItems++;
										CatProfile itemComp = _componentRepo.GetCatalogProfile(itm.ProfileCode).Find(x => x.Status != Status.Failed) ?? throw new Exception(string.Format("Product Operation No.{0} - Machine {1} - ProfileCode {2} :  Profile Code is invalid. ", operation.OperationNo, machine.MachineCode, itm.ProfileCode));
									});
								}
							});
						}
						if (operation.OperationByProducts is not null)
						{
							int countByProd = 0;
							foreach (ProductOperationByProduct bypr in operation.OperationByProducts)
							{
								countByProd++;
								Component itemComp = ((await GetComponents(bypr.ItemCode, true).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault()) ?? throw new Exception(string.Format("Product Operation No.{0} - ByProduct {1} : ItemCode is invalid. ", operation.OperationNo, bypr.ItemCode));
								MeasureUnit ItemUOM = measures.FirstOrDefault(m => string.Equals(m.Code, bypr.InventoryUoM, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception(string.Format("Product Operation No.{0} - ByProduct {1} : InventoryUoM is invalid. ", operation.OperationNo, bypr.ItemCode));
								Warehouse ItemWhs = warehouses.Find(m => string.Equals(m.Code, bypr.WarehouseCode, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception(string.Format("Product Operation No.{0} - ByProduct {1} : WarehouseCode is invalid. ", operation.OperationNo, bypr.ItemCode));
							}
						}
					}

					// Comienza rellenado de objetos internos

					ProcessEntry pe = new()
					{
						Code = item.ProductCode,
						Name = !string.IsNullOrEmpty(item.ProductName) ? item.ProductName : item.ProductCode,
						Quantity = item.Quantity,
						Processes = [],
						Components = [],
						Tasks = [],
						Tools = [],
						Labor = [],
						Version = item.Version,
						Sequence = item.Sequence,
						Warehouse = warehouseId,
						Comments = item.Comments,
						Formula = item.FormulaCode,
						Schedule = string.Equals(item.Schedule.ToStr(), "YES", StringComparison.OrdinalIgnoreCase).ToInt32(),
					};

					Component itemInfo = new()
					{
						Code = item.ProductCode,
						Name = !string.IsNullOrEmpty(item.ProductName) ? item.ProductName : item.ProductCode,
						ComponentType = ComponentType.Product,
						// Version = item.Version,
						Status = Status.Active
					};
					if (existingComponent is not null)
					{
						itemInfo = existingComponent;
					}
					if (!MultiWarehouseEnabled)
					{
						ProcessEntry activeEntry = (await GetProcessEntry(item.ProductCode, null, null, null, systemOperator).ConfigureAwait(false))?.Find(x => x.Status == Status.Active);
						if (activeEntry is not null)
						{
							existingEntry = (await GetProcessEntry(item.ProductCode, activeEntry.Warehouse, activeEntry.Version, activeEntry.Sequence, systemOperator).ConfigureAwait(false))?.Find(x => x.Status != Status.Failed);
							if (existingEntry is not null && activeEntry.Warehouse != warehouseId)
							{
								existingEntry.Warehouse = warehouseId;
								existingEntry.Version = item.Version;
								existingEntry.Sequence = item.Sequence;
								isForcedEdit = true;
							}
						}
					}
					else
					{
						existingEntry = (await GetProcessEntry(item.ProductCode, warehouseId, item.Version, item.Sequence, systemOperator).ConfigureAwait(false))?.Find(x => x.Status != Status.Failed);
					}
					if (existingEntry is not null)
					{
						pe = existingEntry;
						pe.Quantity = item.Quantity;
						pe.Name = item.ProductName;
						if (!string.IsNullOrEmpty(item.Comments))
						{
							pe.Comments = item.Comments;
						}
						if (!string.IsNullOrEmpty(item.Schedule))
						{
							pe.Schedule = string.Equals(item.Schedule.ToStr(), "YES", StringComparison.OrdinalIgnoreCase).ToInt32();
						}
					}

					pe.UnitId = measureUnitId;
					pe.UnitType = measureUnitType;
					if (!string.IsNullOrEmpty(item.Status))
					{
						switch (item.Status.Trim().ToUpperInvariant())
						{
							case "ACTIVE":
								pe.Status = Status.Active;
								break;
							case "DRAFT":
								pe.Status = Status.Disabled;
								break;
						}
					}
					else
					{
						pe.Status = Status.Active;
					}
					// New Optional fields
					if (!string.IsNullOrEmpty(item.BomVersion))
					{
						pe.BomVersion = item.BomVersion.ToInt32();
					}
					// New Optional fields
					if (!string.IsNullOrEmpty(item.BomSequence))
					{
						pe.BomSequence = item.BomSequence.ToInt32();
					}
					// New Optional fields
					if (!string.IsNullOrEmpty(item.RouteVersion))
					{
						pe.RouteVersion = item.RouteVersion.ToInt32();
					}
					// New Optional fields
					if (!string.IsNullOrEmpty(item.RouteSequence))
					{
						pe.RouteSequence = item.RouteSequence.ToInt32();
					}

					List<ProcessEntryProcess> oldProcesses = pe.Processes;
					List<ProcessEntryTool> oldTools = pe.Tools;
					List<ProcessEntryLabor> oldLabors = pe.Labor;
					List<ProcessEntryComponent> oldComponents = pe.Components;
					pe.Components = [];

					//Operations
					if (item.Operations?.Count > 0)
					{
						pe.Processes = [];

						// tasks
						foreach (ProductOperation itmOperation in item.Operations)
						{
							ProcessTypeSubtype CurrentOperationSubType = subProcessTypes.FirstOrDefault(pt => string.Equals(pt.Code, itmOperation.OperationSubtype, StringComparison.OrdinalIgnoreCase));
							ProcessType CurrentOperationType = processTypes.Find(pt => string.Equals(pt.Code, CurrentOperationSubType.ProcessTypeId, StringComparison.OrdinalIgnoreCase));
							try
							{
								MeasureUnit operationUnit = measures.FirstOrDefault(x => string.Equals(x.Code, itmOperation.OutputUoM, StringComparison.OrdinalIgnoreCase));
								ProcessEntryProcess oldOperation = null;
								if (editMode || !string.IsNullOrEmpty(pe.Id))
								{
									oldOperation = oldProcesses?.Find(x => x.ProcessId.ToDouble() == itmOperation.OperationNo.ToDouble());
								}
								ProcessEntryProcess prc = new()
								{
									ProcessId = itmOperation.OperationNo.ToStr(),
									ProcessTypeId = CurrentOperationSubType.ProcessTypeId,
									ProcessSubTypeId = CurrentOperationSubType.Code,
									Name = CurrentOperationSubType.Name,
									Step = Math.Floor(itmOperation.OperationNo).ToInt32(),
									Sort = itmOperation.OperationNo == 0 ? 0 : (10 * (itmOperation.OperationNo.ToDecimal() % Math.Floor(itmOperation.OperationNo.ToDecimal()))).ToDouble().ToInt32(),
									TransferType = null,
									TransferQty = null,
									SlackTimeAfterPrevOp = null,
									SlackTimeBeforeNextOp = null,
									MaxTimeBeforeNextOp = null,
									MaxOpSpanIncrease = null,
									Quantity = itmOperation.Quantity,
									OperationClassId = 1, // Production
									Unit = operationUnit.Code
								};

								if (!editMode || !string.IsNullOrEmpty(itmOperation.TransferType))
								{
									prc.TransferType = itmOperation.TransferType?.ToUpperInvariant() == "AFTERTRANSFER" ? "AfterTransfer" : "AfterComplete";
								}

								if (!editMode && oldOperation is null)
								{
									if (itmOperation.TransferQuantity.HasValue)
									{
										prc.TransferQty = itmOperation.TransferQuantity.Value;
									}
									else
									{
										prc.TransferQty = 0;
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.TransferQuantity.HasValue && !isForcedEdit)
									{
										prc.TransferQty = itmOperation.TransferQuantity.Value;
									}
									else
									{
										prc.TransferQty = oldOperation.TransferQty;
									}
								}

								if (!editMode && oldOperation is null)
								{
									if (itmOperation.SlackTimeAftNextOp.HasValue)
									{
										prc.SlackTimeAfterPrevOp = Helpers.Common.SecondsToTimeString(itmOperation.SlackTimeAftNextOp.ToInt32(), true);
									}
									else
									{
										prc.SlackTimeAfterPrevOp = Common.SecondsToTimeString(0, true);
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.SlackTimeAftNextOp.HasValue && !isForcedEdit)
									{
										prc.SlackTimeAfterPrevOp = Common.SecondsToTimeString(itmOperation.SlackTimeAftNextOp.ToInt32(), true);
									}
									else
									{
										prc.SlackTimeAfterPrevOp = oldOperation.SlackTimeAfterPrevOp;
									}
								}

								if (!editMode && oldOperation is null)
								{
									if (itmOperation.SlackTimeBefNextOp.HasValue)
									{
										prc.SlackTimeBeforeNextOp = Common.SecondsToTimeString(itmOperation.SlackTimeBefNextOp.ToInt32(), true);
									}
									else
									{
										prc.SlackTimeBeforeNextOp = Common.SecondsToTimeString(0, true);
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.SlackTimeBefNextOp.HasValue && !isForcedEdit)
									{
										prc.SlackTimeBeforeNextOp = Common.SecondsToTimeString(itmOperation.SlackTimeBefNextOp.ToInt32(), true);
									}
									else
									{
										prc.SlackTimeBeforeNextOp = oldOperation.SlackTimeBeforeNextOp;
									}
								}

								if (!editMode && oldOperation is null)
								{
									if (itmOperation.MaxTimeBefNextOp.HasValue)
									{
										prc.MaxTimeBeforeNextOp = Common.SecondsToTimeString(itmOperation.MaxTimeBefNextOp.ToInt32(), true);
									}
									else
									{
										prc.MaxTimeBeforeNextOp = Common.SecondsToTimeString(0, true);
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.MaxTimeBefNextOp.HasValue && !isForcedEdit)
									{
										prc.MaxTimeBeforeNextOp = Common.SecondsToTimeString(itmOperation.MaxTimeBefNextOp.ToInt32(), true);
									}
									else
									{
										prc.MaxTimeBeforeNextOp = oldOperation.MaxTimeBeforeNextOp;
									}
								}

								if (!editMode && oldOperation is null)
								{
									if (itmOperation.MaxOpSpanIncrease.HasValue)
									{
										prc.MaxOpSpanIncrease = SecondsToTimeString(itmOperation.MaxOpSpanIncrease.ToInt32(), true);
									}
									else
									{
										prc.MaxOpSpanIncrease = SecondsToTimeString(0, true);
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.MaxOpSpanIncrease.HasValue && !isForcedEdit)
									{
										prc.MaxOpSpanIncrease = SecondsToTimeString(itmOperation.MaxOpSpanIncrease.ToInt32(), true);
									}
									else
									{
										prc.MaxOpSpanIncrease = oldOperation.MaxOpSpanIncrease;
									}
								}

								// Campos no mapeados
								if ((editMode || isForcedEdit) && oldOperation is not null)
								{
									prc.SpareStringField1 = oldOperation.SpareStringField1;
									prc.SpareStringField2 = oldOperation.SpareStringField2;
									prc.SpareNumberField = oldOperation.SpareNumberField;
								}

								//Attributes
								if (!editMode && !isForcedEdit)
								{
									if (itmOperation.Attributes?.Count > 0)
									{
										prc.Attributes = [];
										itmOperation.Attributes.ForEach(prdAttr =>
										{
											prc.Attributes.Add(new ProcessEntryAttribute
											{
												AttributeId = prdAttr.AttributeTypeCode,
												Selected = true,
												Value = prdAttr.AttributeCode
											});
										});
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.Attributes?.Count > 0 && !isForcedEdit)
									{
										prc.Attributes = [];
										itmOperation.Attributes.ForEach(prdAttr =>
										{
											prc.Attributes.Add(new ProcessEntryAttribute
											{
												AttributeId = prdAttr.AttributeTypeCode,
												Selected = true,
												Value = prdAttr.AttributeCode
											});
										});
									}
									else
									{
										prc.Attributes = oldOperation.Attributes;
									}
								}
								int OldOperationTimeType = 0;
								if (oldProcesses is not null)
								{
									ProcessEntryProcess oldProcess = oldProcesses.Find(x => x.Name == prc.Name && x.ProcessTypeId == prc.ProcessTypeId && x.Step == prc.Step && x.Sort == prc.Sort);
									if (oldProcess is not null)
									{
										prc.ProcessId = oldProcess.ProcessId;
										OldOperationTimeType = oldProcess.ProcessTimeType.ToInt32();
									}
								}
								switch (itmOperation.OperationTimeType)
								{
									case "SpecificOpTime":
										prc.ProcessTimeType = 4;
										break;

									case "SpecificRatePerHour":
										prc.ProcessTimeType = 5;
										break;

									case "SpecificBatchTime":
										prc.ProcessTimeType = 6;
										break;

									default:
										prc.ProcessTimeType = 4;
										break;
								}
								if ((string.IsNullOrEmpty(itmOperation.OperationTimeType) || isForcedEdit) && oldProcesses is not null)
								{
									prc.ProcessTimeType = OldOperationTimeType;
								}

								// Tasks
								if (itmOperation.Tasks?.Count > 0)
								{
									List<Activity> tasks = GetDataImportTasks(itmOperation, systemOperator);
									tasks ??= [];
									if (!editMode)
									{
										tasks.ForEach(tsk => tsk.ProcessId = prc.ProcessId);
										if (pe.Tasks is null)
										{
											pe.Tasks = tasks;
										}
										else
										{
											pe.Tasks.AddRange(tasks);
										}
									}
									else
									{
										tasks.ForEach(tsk =>
										{
											pe.Tasks.Where(x => x.SortId == tsk.SortId && x.TriggerId == tsk.TriggerId)?.ToList()?.ForEach(x =>
											{
												x.ManualDelete = true;
											});
											tsk.ProcessId = prc.ProcessId;
											pe.Tasks.Add(tsk);
										});
									}
								}
								else if (CurrentOperationType is not null && !editMode && CurrentOperationType.Tasks is not null)
								{
									pe.Tasks ??= [];
									CurrentOperationType.Tasks.ForEach(tsk =>
									{
										pe.Tasks.Add(new Activity
										{
											Id = tsk.Id,
											ProcessId = prc.ProcessId,
											SortId = tsk.SortId,
											TriggerId = tsk.TriggerId,
											IsMandatory = tsk.IsMandatory,
											Origin = "OperationType"
										});
									});
								}
								// Machines
								prc.AvailableDevices = GetDataImportAvailableDevices(itmOperation, oldOperation);
								if (prc.AvailableDevices?.Any(d => d.Selected) == false)
								{
									DeviceSpeed firstDevice = prc.AvailableDevices?.OrderBy(o => o.LineId)?.FirstOrDefault(x => x.Id != "00000000-0000-0000-0000-000000000000");
									firstDevice ??= prc.AvailableDevices?.OrderBy(o => o.LineId)?.FirstOrDefault();
									firstDevice.Selected = true;
								}
								// Adicionar maquinas previas /Alternativas cuando es SAP B1
								if (currentERP is not null && (currentERP.ErpCode == "SAP_B1" || currentERP.ErpCode == "SAP_B1_OPT") && oldOperation?.AvailableDevices is not null)
								{
									prc.AvailableDevices ??= [];
									oldOperation.AvailableDevices.ForEach(dev =>
									{
										//if (!dev.Selected)
										{
											DeviceSpeed oldval = new()
											{
												LineUID = dev.LineUID,
												LineId = string.IsNullOrEmpty(dev.LineId) ? "0" : dev.LineId,
												Id = dev.Id,
												Quantity = dev.Quantity,
												AutomaticSequencing = dev.AutomaticSequencing,
												ExecTime = dev.ExecTime,
												LotCapacity = dev.LotCapacity,
												Schedule = dev.Schedule,
												Selected = false,
												SetupTime = dev.SetupTime,
												TimeUnit = dev.TimeUnit,
												Unit = dev.Unit,
												WaitTime = dev.WaitTime,
												IsBackflush = dev.IsBackflush
											};
											if (!prc.AvailableDevices.Any(x => x.Id == dev.Id))
											{
												prc.AvailableDevices.Add(oldval);
											}
										}
									});
								}
								if (prc.AvailableDevices?.Any(d => d.Selected) == false)
								{
									DeviceSpeed firstDevice = prc.AvailableDevices?.OrderBy(o => o.LineId)?.FirstOrDefault(x => x.Id != "00000000-0000-0000-0000-000000000000");
									firstDevice ??= prc.AvailableDevices?.OrderBy(o => o.LineId)?.FirstOrDefault();
									firstDevice.Selected = true;
								}
								if (editMode && oldOperation is not null && prc.AvailableDevices is not null)
								{
									prc.AvailableDevices.Where(m => string.IsNullOrEmpty(m.LineUID))?.ToList()?.ForEach(m =>
									{
										ProductMachine curm = itmOperation.OperationMachines.Find(om => om.MachineCode == m.Id && om.LineID == m.LineId);
										DeviceSpeed speed = oldOperation.AvailableDevices?.Find(om => om.LineId == m.LineId);
										if (speed is not null)
										{
											m.LineUID = speed.LineUID;
											m.LotCapacity = speed.LotCapacity;
											m.WaitTime = speed.WaitTime;
											if (curm is not null)
											{
												if (string.IsNullOrEmpty(curm.AutomaticSequencing))
												{
													m.AutomaticSequencing = speed.AutomaticSequencing;
												}
												if (string.IsNullOrEmpty(curm.Schedule))
												{
													m.Schedule = speed.Schedule;
												}
												if (string.IsNullOrEmpty(curm.IssueMode))
												{
													m.IsBackflush = speed.IsBackflush;
												}
											}
											else
											{
												m.Schedule = speed.Schedule;
												m.AutomaticSequencing = speed.AutomaticSequencing;
												m.IsBackflush = speed.IsBackflush;
											}
										}
										else
										{
											m.LineUID = Guid.NewGuid().ToString();
										}
									});
								}
								// Sub products
								prc.Subproducts = await GetDataImportSubProducts(itmOperation).ConfigureAwait(false);
								if (editMode && oldOperation is not null && prc.Subproducts is not null)
								{
									prc.Subproducts.Where(m => string.IsNullOrEmpty(m.LineUID))?.ToList()?.ForEach(m =>
									{
										SubProduct sprd = oldOperation.Subproducts?.Find(om => om.LineId == m.LineId);
										if (sprd is not null)
										{
											m.LineUID = sprd.LineUID;
										}
										else
										{
											m.LineUID = Guid.NewGuid().ToString();
										}
									});
								}
								//Attributes
								prc.Attributes?.ForEach(x => x.ProcessId = prc.ProcessId);
								pe.Processes.Add(prc);
							}
							catch (ResponseDataException ex)
							{
								returnValue.Add(new ResponseData
								{
									Action = ex.Action,
									Code = item.ProductCode,
									Entity = "Product",
									IsSuccess = false,
									Message = ex.Message
								});
							}
						}
						int lastStep = pe.Processes.OrderBy(o => o.Step).Select(o => o.Step).LastOrDefault();
						if (lastStep > 0)
						{
							if (pe.Processes.Count(x => x.Step == lastStep) > 1)
							{
								throw new Exception("Parallel Operations are not allowed at the end.");
							}

							foreach (ProcessEntryProcess process in pe.Processes.Where(x => x.Step == lastStep))
							{
								process.IsOutput = true;
							}
						}
					}

					//TODO REVISAR LINE ID, AGREGAR VALIDACIONES MANDATORIO Y LLENAR LINEUID
					pe.Tools = GetDataImportTooling(item, pe, systemOperator);
					if (editMode && pe.Tools is not null && oldTools is not null)
					{
						pe.Tools.Where(tooling => string.IsNullOrEmpty(tooling.LineUID))?.ToList()?.ForEach(tlng =>
						{
							ProductOperationTool origTool = item.Operations.Find(x => x.OperationNo.ToDouble() == tlng.ProcessId.ToDouble())?.OperationTools.Find(ot => ot.ToolingCode == tlng.ToolId && ot.LineID == tlng.LineId.ToInt32());
							ProcessEntryTool oldTooling = oldTools?.Find(x => x.LineId.ToInt32() == tlng.LineId.ToInt32());
							if (oldTooling is not null)
							{
								if (string.IsNullOrEmpty(tlng.Usage) || tlng.Usage == "0")
								{
									tlng.Usage = oldTooling.Usage;
								}
								tlng.Source = oldTooling.Source;
								tlng.Cost = oldTooling.Cost;
								if (origTool is not null && string.IsNullOrEmpty(origTool.Schedule))
								{
									tlng.Schedule = oldTooling.Schedule;
								}
							}
							if (oldTooling is not null && !string.IsNullOrEmpty(oldTooling.LineUID))
							{
								tlng.LineUID = oldTooling.LineUID;
							}
							else
							{
								tlng.LineUID = Guid.NewGuid().ToStr();
							}
						});
					}

					pe.Labor = GetDataImportLabor(item, pe, systemOperator);
					if (editMode && pe.Labor is not null && oldLabors is not null)
					{
						pe.Labor.Where(elem => string.IsNullOrEmpty(elem.LineUID))?.ToList()?.ForEach(lbr =>
						{
							ProductOperationLabor origLbr = item.Operations.Find(x => x.OperationNo.ToDouble() == lbr.ProcessId.ToDouble())?.OperationLabor.Find(ot => ot.ProfileCode == lbr.LaborId && ot.LineID == lbr.LineId.ToInt32());
							ProcessEntryLabor OldLabor = oldLabors?.Find(x => x.LineId.ToInt32() == lbr.LineId.ToInt32());
							if (OldLabor is not null)
							{
								if (string.IsNullOrEmpty(lbr.Usage) || lbr.Usage == "0")
								{
									lbr.Usage = OldLabor.Usage;
								}
								lbr.Source = OldLabor.Source;
								lbr.Cost = OldLabor.Cost;
								if (origLbr is not null && string.IsNullOrEmpty(origLbr.Schedule))
								{
									lbr.Schedule = OldLabor.Schedule;
								}
							}
							if (OldLabor is not null && !string.IsNullOrEmpty(OldLabor.LineUID))
							{
								lbr.LineUID = OldLabor.LineUID;
							}
							else
							{
								lbr.LineUID = Guid.NewGuid().ToStr();
							}
						});
					}
					pe.Components = await GetDataImportItems(item, pe, systemOperator).ConfigureAwait(false);
					if (editMode && pe.Components is not null && oldComponents is not null)
					{
						pe.Components.Where(comp => string.IsNullOrEmpty(comp.LineUID))?.ToList()?.ForEach(cmp =>
						{
							ProductOperationItem origItm = item.Operations.Find(x => x.OperationNo.ToDouble() == cmp.ProcessId.ToDouble())?.OperationItems.Find(ot => ot.ItemCode == cmp.ComponentId && ot.LineID == cmp.LineId.ToInt32());
							ProcessEntryComponent oldComp = oldComponents?.Find(x => x.LineId.ToInt32() == cmp.LineId.ToInt32());
							if (oldComp is not null)
							{
								cmp.Source = oldComp.Source;
								cmp.Class = oldComp.Class;
								if (origItm is not null && string.IsNullOrEmpty(origItm.Schedule))
								{
									cmp.IsSchedule = oldComp.IsSchedule;
								}
								if (origItm is not null && string.IsNullOrEmpty(origItm.Type))
								{
									cmp.Class = oldComp.Class;
								}
								if (origItm is not null && string.IsNullOrEmpty(origItm.IssueMethod))
								{
									cmp.IsBackflush = oldComp.IsBackflush;
								}
								if (origItm is not null && string.IsNullOrEmpty(origItm.Source))
								{
									cmp.Source = oldComp.Source;
								}
							}
							if (oldComp is not null && !string.IsNullOrEmpty(oldComp.LineUID))
							{
								cmp.LineUID = oldComp.LineUID;
							}
							else
							{
								cmp.LineUID = Guid.NewGuid().ToStr();
							}
						});
					}

					if (!editMode && pe.Tasks is not null)
					{
						foreach (Activity tsk in pe.Tasks)
						{
							if (!string.IsNullOrEmpty(tsk.Id) && string.Equals(tsk.Origin, "OPERATIONTYPE", StringComparison.OrdinalIgnoreCase))
							{
								Activity clonedActivity = await CloneActivity(new Activity(tsk.Id), systemOperator, "PRODUCT").ConfigureAwait(false);
								if (clonedActivity is not null)
								{
									tsk.Id = clonedActivity.Id;
								}
							}
						}
					}

					itemInfo.ProcessEntry = pe;
					ResponseData resp = new();
					if (Validate)
					{
						resp = _componentRepo.MergeProduct(itemInfo, systemOperator, Validate, Level);
						await _attachmentOperation.SaveImageEntity("Item", itemInfo.Image, itemInfo.Code, systemOperator).ConfigureAwait(false);
						if (itemInfo.AttachmentIds is not null)
						{
							foreach (string attachment in itemInfo.AttachmentIds)
							{
								await _attachmentOperation.AttachmentSync(attachment, itemInfo.Code, systemOperator).ConfigureAwait(false);
							}
						}
					}
					else
					{
						if (string.IsNullOrEmpty(itemInfo.ProcessEntry.Id))
						{
							itemInfo.ProcessEntry.Id = string.Empty;
						}
						ResponseData itemFound = _componentRepo.MergeProduct(itemInfo, systemOperator, true, Level);
						await _attachmentOperation.SaveImageEntity("Item", itemInfo.Image, itemInfo.Code, systemOperator).ConfigureAwait(false);
						if (itemInfo.AttachmentIds is not null)
						{
							foreach (string attachment in itemInfo.AttachmentIds)
							{
								await _attachmentOperation.AttachmentSync(attachment, itemInfo.Code, systemOperator).ConfigureAwait(false);
							}
						}
						if (itemFound.Action == ActionDB.Update)
						{
							itemInfo.Id = itemFound.Id;
							if (!string.IsNullOrEmpty(itemInfo.ProcessEntry.Id))
							{
								itemInfo.ProcessEntryId = itemInfo.ProcessEntry.Id;
							}
						}
						resp = await MergeProduct(itemFound.Action, itemInfo, systemOperator, Validate, Level, NotifyOnce, false, true, IntegrationSource.ERP).ConfigureAwait(false);
						if (resp.IsSuccess)
						{
							itemInfo.ProcessEntryId = resp.Id;
							_componentRepo.MergeComponent(itemInfo, systemOperator, Validate);
						}
					}
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
			// if (!NotifyOnce)
			// {
			// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Product, Action = ActionDB.IntegrateAll.ToStr() });
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
	public async Task<ResponseData> MergeProduct(ActionDB mode, Component componentInfo, User systemOperator, bool Validate = false, LevelMessage Level = LevelMessage.Success, bool NotifyOnce = true, bool isNewVersion = false, bool isExternalEndpoint = false, IntegrationSource intSource = IntegrationSource.SF)
	{
		ResponseData returnValue = null;

		// response.Component = componentInfo;

		#region Permission validation

		// if (!systemOperator.Permissions.Any(x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		if (componentInfo.ProcessEntry.MinQuantity > componentInfo.ProcessEntry.MaxQuantity && componentInfo.ProcessEntry.MaxQuantity > 0)
		{
			throw new Exception("Maximum quantity must be greater than Minimum Quantity");
		}
		TransactionOptions tso = new()
		{
			IsolationLevel = IsolationLevel.ReadCommitted
		};
		//using (TransactionScope scope = new(TransactionScopeOption.Required, tso,TransactionScopeAsyncFlowOption.Enabled))
		{
			if (mode == ActionDB.Create)
			{
				if (componentInfo.ComponentType == ComponentType.Product && componentInfo.ProcessEntry is not null)
				{
					if (componentInfo.ProcessEntry.Quantity == 0)
					{
						throw new Exception("Invalid Product Quantity");
					}

					//CREANDO PRODUCTO
					ProcessEntry entryInfo = componentInfo.ProcessEntry;
					//Validate Create NewVersion
					if (entryInfo.isNewVersion)
					{
						int newVersion = _componentRepo.GetNextProductVersion(componentInfo.ProcessEntry);
						if (newVersion > 0)
						{
							componentInfo.ProcessEntry.Version = newVersion;
							componentInfo.ProcessEntry.Sequence = 1;
							isNewVersion = true;
							componentInfo.ProcessEntry.Id = Guid.NewGuid().ToStr();
						}
					}
					//Validar duplicados Opcenter
					ValidateOpcenterRules(entryInfo, systemOperator);

					if (entryInfo.Version == 0)
					{
						entryInfo.Version = 1;
					}

					if (entryInfo.Sequence == 0)
					{
						entryInfo.Sequence = 1;
					}

					ProcessEntry entryResult = _componentRepo.CreateProcessEntry(entryInfo, systemOperator, intSource);
					if (!string.IsNullOrEmpty(entryResult.Id))
					{
						List<SubProduct> AllSubProducts = [];
						entryInfo.Processes.ForEach(x =>
						{
							x.AvailableDevices?.ForEach(a =>
								{
									if (string.IsNullOrEmpty(a.LineUID))
									{
										a.LineUID = Guid.NewGuid().ToStr();
									}
								});
							if (x.Subproducts is not null)
							{
								foreach (SubProduct z in x.Subproducts)
								{
									z.ProcessId = x.ProcessId;
									if (string.IsNullOrEmpty(z.LineUID))
									{
										z.LineUID = Guid.NewGuid().ToString();
									}
								}
								AllSubProducts.AddRange(x.Subproducts);
							}
						});

						string jsonOperations = JsonConvert.SerializeObject(entryInfo.Processes);
						string jsonSubProducts = string.Empty;
						if (AllSubProducts.Count > 0)
						{
							jsonSubProducts = JsonConvert.SerializeObject(AllSubProducts);
						}

						_ = (entryInfo.Components?.RemoveAll(x => x.ComponentType == 0));

						string jsonMaterials = JsonConvert.SerializeObject(entryInfo.Components);
						string jsonAlternativeMaterials = string.Empty;

						List<AlternativeComponent> AllAlternatives = [];
						entryInfo.Components.ForEach(x =>
						{
							if (string.IsNullOrEmpty(x.LineUID))
							{
								x.LineUID = Guid.NewGuid().ToStr();
							}
							if (x.Alternatives is not null)
							{
								x.Alternatives.ForEach(z => { z.ProcessId = x.ProcessId; z.ComponentId = x.ComponentId; });
								AllAlternatives.AddRange(x.Alternatives);
							}
						});
						if (AllAlternatives.Count > 0)
						{
							jsonAlternativeMaterials = JsonConvert.SerializeObject(AllAlternatives);
						}
						// Todo POner Tasks nueva version
						_componentRepo.SaveProductDetails(entryInfo, jsonOperations, jsonMaterials, jsonAlternativeMaterials, jsonSubProducts, systemOperator);

						componentInfo.ProcessEntryId = entryResult.Id;
						returnValue = _componentRepo.MergeProduct(componentInfo, systemOperator, Validate, Level);
						componentInfo.Id = returnValue.Id;
						await _attachmentOperation.SaveImageEntity("Item", componentInfo.Image, componentInfo.Code, systemOperator).ConfigureAwait(false);
						if (componentInfo.AttachmentIds is not null)
						{
							foreach (string attachment in componentInfo.AttachmentIds)
							{
								await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
							}
						}
						returnValue.Entity = componentInfo;
					}

					if (!string.IsNullOrEmpty(entryResult.Id) && entryResult.Tasks is not null)
					{
						foreach (Activity task in entryResult.Tasks)
						{
							if (string.IsNullOrEmpty(task.Id))
							{
								task.Origin = OriginActivity.Product.ToStr();
								Activity newActivity = await CreateActivity(task, systemOperator).ConfigureAwait(false);
								if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
								{
									task.Id = newActivity.Id;
									_ = AssociateActivityProcessEntry(entryResult.Id, newActivity.ProcessId, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
								}
							}
							else
							{
								if (task.ActivityClassId > 0)
								{
									await UpdateActivity(task, systemOperator).ConfigureAwait(false);
								}
								_ = AssociateActivityProcessEntry(entryResult.Id, task.ProcessId, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
							}
						}
					}

					if (!string.IsNullOrEmpty(entryResult.Id) && entryResult.Labor is not null)
					{
						entryResult.Labor.ForEach(x =>
						{
							if (string.IsNullOrEmpty(x.LaborId)) { x.LaborId = x.Id; }
							if (string.IsNullOrEmpty(x.LineUID)) { x.LineUID = Guid.NewGuid().ToString(); }
						});
						_componentRepo.MergeProcessEntryLabor(entryResult.Id, JsonConvert.SerializeObject(entryResult.Labor), systemOperator);
					}

					if (!string.IsNullOrEmpty(entryResult.Id) && entryResult.Tools is not null)
					{
						entryResult.Tools.ForEach(x =>
						{
							if (string.IsNullOrEmpty(x.ToolId)) { x.ToolId = x.Id; }
							if (string.IsNullOrEmpty(x.LineUID)) { x.LineUID = Guid.NewGuid().ToString(); }
						});
						_componentRepo.MergeProcessEntryTools(entryResult.Id, JsonConvert.SerializeObject(entryResult.Tools), systemOperator);
					}

					entryResult.Processes.ForEach(x =>
					{
						if (x.Attributes is not null)
						{
							x.Attributes.ForEach(z => z.ProcessId = x.ProcessId);
						}
						else
						{
							x.Attributes = [];
						}
					});
					List<ProcessEntryAttribute> attrs = [.. entryResult.Processes.SelectMany(x => x.Attributes)];
					if (!string.IsNullOrEmpty(entryResult.Id) && attrs is not null)
					{
						attrs.ForEach(x => { if (string.IsNullOrEmpty(x.AttributeId)) { x.AttributeId = x.Id; } });
						_componentRepo.MergeProcessEntryAttributes(entryResult.Id, JsonConvert.SerializeObject(attrs), systemOperator);
					}
				}
				else
				{
					returnValue = _componentRepo.MergeProduct(componentInfo, systemOperator, Validate, Level);
					await _attachmentOperation.SaveImageEntity("Item", componentInfo.Image, componentInfo.Code, systemOperator).ConfigureAwait(false);
					if (componentInfo.AttachmentIds is not null)
					{
						foreach (string attachment in componentInfo.AttachmentIds)
						{
							await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
						}
					}
					componentInfo.Id = returnValue.Id;
					returnValue.Entity = componentInfo;

					//if (returnValue is not null && !string.IsNullOrEmpty(returnValue.Id) && Services.ContextCache.Components is not null)
					//{
					//    Services.ContextCache.Components.RemoveAll(comp => comp.Id == componentInfo.Id);
					//    Services.ContextCache.Components.Add(componentInfo);
					//}
				}
				// if (!Validate)
				// {
				// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Product, Action = ActionDB.IntegrateAll.ToStr() });
				// }
				// await componentInfo.ProcessEntry.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);
			}
			else
			{
				// ES ACTUALIZAR
				ProcessEntry entryInfo;
				if (componentInfo.ComponentType == ComponentType.Product && componentInfo.ProcessEntry is not null)
				{
					bool MultiVersionEnabled = Config.Configuration["Product-Versioning"].ToBool();
					bool MultiWarehouse = Config.Configuration["Product-MultiWarehouse"].ToBool();
					if (!MultiVersionEnabled)
					{
						int newVersion = _componentRepo.VerifyProductVersion(componentInfo.ProcessEntry);
						if (newVersion > 0)
						{
							// Solo cuando es multi almacenes, cuando no es por que ya se hizo validación de tareas previamente.
							if (MultiWarehouse)
							{
								ProcessEntry ptTemp = (await GetProcessEntry(componentInfo.ProcessEntry.Code
									, componentInfo.ProcessEntry.Warehouse, componentInfo.ProcessEntry.Version
									, componentInfo.ProcessEntry.Sequence, systemOperator).ConfigureAwait(false)).Find(x => x.Status != Status.Failed);
								if (ptTemp.Tasks?.Count > 0)
								{
									componentInfo.ProcessEntry.Tasks.AddRange(ptTemp.Tasks);
								}
							}
							componentInfo.ProcessEntry.Version = newVersion;
							componentInfo.ProcessEntry.Sequence = 1;
							isNewVersion = true;
							componentInfo.ProcessEntry.Id = Guid.NewGuid().ToStr();
						}
					}
					Component originalComponent = (await _componentRepo.ListComponents(componentInfo.Id).ConfigureAwait(false)).FirstOrDefault();
					if (originalComponent is not null)
					{
						if (!string.IsNullOrEmpty(componentInfo.ProcessEntry?.Id) && !isNewVersion)
						{
							// ACTUALIZAR VERSION ACTUAL
							entryInfo = componentInfo.ProcessEntry;
							//Validar duplicados Opcenter
							ValidateOpcenterRules(entryInfo, systemOperator);

							if (_componentRepo.UpdateProcessEntry(entryInfo, systemOperator))
							{
								List<SubProduct> AllSubProducts = [];
								entryInfo.Processes.ForEach(x =>
								{
									if (x.Subproducts is not null)
									{
										x.Subproducts.ForEach(z => z.ProcessId = x.ProcessId);
										AllSubProducts.AddRange(x.Subproducts);
									}
								});

								string jsonOperations = JsonConvert.SerializeObject(entryInfo.Processes);
								string jsonSubProducts = string.Empty;

								if (AllSubProducts.Count > 0)
								{
									jsonSubProducts = JsonConvert.SerializeObject(AllSubProducts);
								}
								else
								{
									jsonSubProducts = "[]";
								}

								_ = (entryInfo.Components?.RemoveAll(x => x.ComponentType == 0));

								string jsonMaterials = JsonConvert.SerializeObject(entryInfo.Components);
								string jsonAlternativeMaterials = string.Empty;

								List<AlternativeComponent> AllAlternatives = [];
								entryInfo.Components?.ForEach(x =>
								{
									if (x.Alternatives is not null)
									{
										x.Alternatives.ForEach(z => { z.ProcessId = x.ProcessId; z.ComponentId = x.ComponentId; });
										AllAlternatives.AddRange(x.Alternatives);
									}
								});

								if (AllAlternatives.Count > 0)
								{
									jsonAlternativeMaterials = JsonConvert.SerializeObject(AllAlternatives);
								}

								bool tempDetail = _componentRepo.SaveProductDetails(entryInfo, jsonOperations, jsonMaterials, jsonAlternativeMaterials, jsonSubProducts, systemOperator);

								if (tempDetail)
								{
									returnValue = _componentRepo.MergeProduct(componentInfo, systemOperator, Validate, Level);
									await _attachmentOperation.SaveImageEntity("Item", componentInfo.Image, componentInfo.Code, systemOperator).ConfigureAwait(false);
									if (componentInfo.AttachmentIds is not null)
									{
										foreach (string attachment in componentInfo.AttachmentIds)
										{
											await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
										}
									}
									tempDetail = returnValue.IsSuccess;
								}

								if (tempDetail && entryInfo.Tasks is not null)
								{
									foreach (Activity task in entryInfo.Tasks)
									{
										if (string.IsNullOrEmpty(task.Id))
										{
											task.Origin = OriginActivity.Product.ToStr();
											Activity newActivity = await CreateActivity(task, systemOperator).ConfigureAwait(false);
											if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
											{
												_ = AssociateActivityProcessEntry(entryInfo.Id, newActivity.ProcessId, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
											}
										}
										else if (task.ManualDelete)
										{
											bool tempResult = RemoveActivityProcessEntryAssociation(entryInfo.Id, task.ProcessId, task.Id, systemOperator);
										}
										else
										{
											if (task.ActivityClassId > 0)
											{
												await UpdateActivity(task, systemOperator).ConfigureAwait(false);
											}
											if (!string.IsNullOrEmpty(task.Id) && string.Equals(task.Origin.ToStr(), "OPERATIONTYPE", StringComparison.OrdinalIgnoreCase))
											{
												Activity clonedActivity = await CloneActivity(new Activity(task.Id), systemOperator, "PRODUCT").ConfigureAwait(false);
												if (clonedActivity is not null)
												{
													task.Id = clonedActivity.Id;
												}
												_ = AssociateActivityProcessEntry(entryInfo.Id, task.ProcessId, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
											}
											else
											{
												_ = AssociateActivityProcessEntry(entryInfo.Id, task.ProcessId, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
											}
										}
									}
								}

								if (tempDetail && entryInfo.Labor?.Count > 0)
								{
									entryInfo.Labor.ForEach(x => { if (string.IsNullOrEmpty(x.LaborId)) { x.LaborId = x.Id; } });
								}
								else
								{
									entryInfo.Labor = [];
								}
								_componentRepo.MergeProcessEntryLabor(entryInfo.Id, JsonConvert.SerializeObject(entryInfo.Labor), systemOperator);

								if (tempDetail && entryInfo.Tools?.Count > 0)
								{
									entryInfo.Tools.ForEach(x => { if (string.IsNullOrEmpty(x.ToolId)) { x.ToolId = x.Id; } });
								}
								else
								{
									entryInfo.Tools = [];
								}
								_componentRepo.MergeProcessEntryTools(entryInfo.Id, JsonConvert.SerializeObject(entryInfo.Tools), systemOperator);

								entryInfo.Processes.ForEach(x =>
								{
									if (x.Attributes is not null)
									{
										x.Attributes.ForEach(z => z.ProcessId = x.ProcessId);
									}
									else
									{
										x.Attributes = [];
									}
								});
								List<ProcessEntryAttribute> attrs = [.. entryInfo.Processes.SelectMany(x => x.Attributes)];
								if (tempDetail && attrs is not null)
								{
									attrs.ForEach(x => { if (string.IsNullOrEmpty(x.AttributeId)) { x.AttributeId = x.Id; } });
									_componentRepo.MergeProcessEntryAttributes(entryInfo.Id, JsonConvert.SerializeObject(attrs), systemOperator);
								}

								ProcessEntry pt = (await GetProcessEntry(entryInfo.Code, entryInfo.Warehouse, entryInfo.Version, entryInfo.Sequence, systemOperator).ConfigureAwait(false)).Find(x => x.Status != Status.Failed);
								if (pt is not null)
								{
									componentInfo.ProcessEntry = pt;
									returnValue.Entity = componentInfo;
								}
								else
								{
									returnValue.Entity = componentInfo;
								}
								// if (!Validate)
								// {
								// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Product, Action = ActionDB.IntegrateAll.ToStr() });
								// }
								// await componentInfo.ProcessEntry.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);
							}
						}
						else
						{
							// CREAR NUEVA VERSION
							entryInfo = componentInfo.ProcessEntry;
							//Validar duplicados Opcenter
							ValidateOpcenterRules(entryInfo, systemOperator);

							ProcessEntry entryResult = null;
							if (string.IsNullOrEmpty(entryInfo.Id) || isNewVersion)
							{
								entryResult = _componentRepo.CreateProcessEntry(entryInfo, systemOperator, intSource);
							}
							else
							{
								entryResult = entryInfo;
							}
							if (!string.IsNullOrEmpty(entryResult.Id))
							{
								List<SubProduct> AllSubProducts = [];
								entryInfo.Processes.ForEach(x =>
								{
									if (x.Subproducts is not null)
									{
										x.Subproducts.ForEach(z => z.ProcessId = x.ProcessId);
										AllSubProducts.AddRange(x.Subproducts);
									}
								});
								string jsonOperations = JsonConvert.SerializeObject(entryInfo.Processes);
								string jsonSubProducts = string.Empty;

								if (AllSubProducts.Count > 0)
								{
									jsonSubProducts = JsonConvert.SerializeObject(AllSubProducts);
								}

								entryInfo.Components.ForEach(x =>
								{
									if (string.IsNullOrEmpty(x.ProcessId))
									{
										x.ProcessId = Guid.NewGuid().ToString();
									}
								});

								string jsonMaterials = JsonConvert.SerializeObject(entryInfo.Components);
								string jsonAlternativeMaterials = string.Empty;

								List<AlternativeComponent> AllAlternatives = [];
								entryInfo.Components.ForEach(x =>
								{
									if (x.Alternatives is not null)
									{
										x.Alternatives.ForEach(z => { z.ProcessId = x.ProcessId; z.ComponentId = x.ComponentId; });
										AllAlternatives.AddRange(x.Alternatives);
									}
								});
								if (AllAlternatives.Count > 0)
								{
									jsonAlternativeMaterials = JsonConvert.SerializeObject(AllAlternatives);
								}

								bool tempDetail = _componentRepo.SaveProductDetails(entryInfo, jsonOperations, jsonMaterials, jsonAlternativeMaterials, jsonSubProducts, systemOperator);

								if (tempDetail && entryInfo.Tasks is not null)
								{
									foreach (Activity task in entryInfo.Tasks)
									{
										if (string.IsNullOrEmpty(task.Id))
										{
											task.Origin = OriginActivity.Product.ToString();
											Activity newActivity = await CreateActivity(task, systemOperator).ConfigureAwait(false);
											if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
											{
												_ = AssociateActivityProcessEntry(entryInfo.Id, newActivity.ProcessId, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
											}
										}
										else if (task.ManualDelete)
										{
											bool tempResult = RemoveActivityProcessEntryAssociation(entryInfo.Id, task.ProcessId, task.Id, systemOperator);
										}
										else
										{
											if (task.ActivityClassId > 0)
											{
												await UpdateActivity(task, systemOperator).ConfigureAwait(false);
											}
											_ = AssociateActivityProcessEntry(entryInfo.Id, task.ProcessId, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
										}
									}
								}

								if (tempDetail && entryInfo.Labor?.Count > 0)
								{
									entryInfo.Labor.ForEach(x => { if (string.IsNullOrEmpty(x.LaborId)) { x.LaborId = x.Id; } });
									_componentRepo.MergeProcessEntryLabor(entryInfo.Id, JsonConvert.SerializeObject(entryInfo.Labor), systemOperator);
								}

								if (tempDetail && entryInfo.Tools?.Count > 0)
								{
									entryInfo.Tools.ForEach(x => { if (string.IsNullOrEmpty(x.ToolId)) { x.ToolId = x.Id; } });
									_componentRepo.MergeProcessEntryTools(entryInfo.Id, JsonConvert.SerializeObject(entryInfo.Tools), systemOperator);
								}

								entryInfo.Processes.ForEach(x =>
								{
									if (x.Attributes is not null)
									{
										x.Attributes.ForEach(z => z.ProcessId = x.ProcessId);
									}
									else
									{
										x.Attributes = [];
									}
								});
								List<ProcessEntryAttribute> attrs = [.. entryInfo.Processes.SelectMany(x => x.Attributes)];
								if (tempDetail && attrs is not null)
								{
									attrs.ForEach(x => { if (string.IsNullOrEmpty(x.AttributeId)) { x.AttributeId = x.Id; } });
									_componentRepo.MergeProcessEntryAttributes(entryInfo.Id, JsonConvert.SerializeObject(attrs), systemOperator);
								}

								ProcessEntry pt = (await GetProcessEntry(entryInfo.Code, entryInfo.Warehouse, entryInfo.Version, entryInfo.Sequence, systemOperator).ConfigureAwait(false)).Find(x => x.Status != Status.Failed);

								if (returnValue is null)
								{
									returnValue = new ResponseData
									{
										IsSuccess = pt is not null
									};
									if (!string.IsNullOrEmpty(pt.Id))
									{
										returnValue.Id = pt.Id;
									}
								}
								if (pt is not null)
								{
									componentInfo.ProcessEntry = pt;
									returnValue.Entity = componentInfo;
								}
								else
								{
									returnValue.Entity = componentInfo;
								}
								// if (!Validate)
								// {
								// 	Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Product, Action = ActionDB.IntegrateAll.ToStr() });
								// }
								// await componentInfo.ProcessEntry.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);
							}
						}
					}
				}
				else
				{
					ResponseData mrgComponent = _componentRepo.MergeProduct(componentInfo, systemOperator, Validate, Level);
					await _attachmentOperation.SaveImageEntity("Item", componentInfo.Image, componentInfo.Code, systemOperator).ConfigureAwait(false);
					if (componentInfo.AttachmentIds is not null)
					{
						foreach (string attachment in componentInfo.AttachmentIds)
						{
							await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
						}
					}
					bool result = mrgComponent.IsSuccess;
					returnValue = mrgComponent;
				}
			}
			//scope.Complete();
		}
		return returnValue;
	}


}