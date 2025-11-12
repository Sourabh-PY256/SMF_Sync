using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using EWP.SF.Common.Models.Catalogs;
using System.Transactions;
using EWP.SF.Common.Constants;


namespace EWP.SF.KafkaSync.BusinessLayer;

public class ComponentOperation : IComponentOperation
{
	private readonly IComponentRepo _componentRepo;

	private readonly IDeviceOperation _deviceOperation;
	private readonly IDataSyncServiceOperation _dataSyncServiceOperation;
	private readonly IWarehouseOperation _warehouseOperation;
	private readonly IMeasureUnitOperation _measureUnitOperation;
	private readonly IAttachmentOperation _attachmentOperation;
	private readonly IProcessTypeOperation _processTypeOperation;
	private readonly ICatalogRepo _catalogRepo;
	private readonly IActivityOperation _activityOperation;
	private readonly IToolOperation _toolOperation;
	private readonly IDataImportOperation _dataImportOperation;
	private readonly IInventoryOperation _inventoryOperation;

	public ComponentOperation(IComponentRepo componentRepo
	, IAttachmentOperation attachmentOperation, IWarehouseOperation warehouseOperation,
	 IDataSyncServiceOperation dataSyncServiceOperation
	 , IMeasureUnitOperation measureUnitOperation, IProcessTypeOperation processTypeOperation
	 , ICatalogRepo catalogRepo, IActivityOperation activityOperation, IToolOperation toolOperation
	 , IDataImportOperation dataImportOperation, IInventoryOperation inventoryOperation, IDeviceOperation deviceOperation)
	{
		_componentRepo = componentRepo;
		_attachmentOperation = attachmentOperation;
		_warehouseOperation = warehouseOperation;
		_dataSyncServiceOperation = dataSyncServiceOperation;
		_measureUnitOperation = measureUnitOperation;
		_processTypeOperation = processTypeOperation;
		_catalogRepo = catalogRepo;
		_activityOperation = activityOperation;
		_toolOperation = toolOperation;
		_dataImportOperation = dataImportOperation;
		_inventoryOperation = inventoryOperation;
		_deviceOperation = deviceOperation;
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
			Machine[] machines = _deviceOperation.ListDevices(false, true, true);
			List<Warehouse> warehouses = _warehouseOperation.ListWarehouse(systemOperator);
			List<MeasureUnit> units = _measureUnitOperation.GetMeasureUnits();
			MeasureUnit[] measures = [.. units.Where(x => x.IsProductionResult)];
			// List<Component> allComponents = GetComponents(string.Empty, true);
			List<ProcessType> processTypes = _processTypeOperation.GetProcessTypes(string.Empty, systemOperator);
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
					// if (editMode && itemListOriginal is not null)
					// {
					// 	item = itemListOriginal.Find(x => x.ProductCode == cycleItem.ProductCode && x.WarehouseCode == cycleItem.WarehouseCode && x.Version == cycleItem.Version);
					// 	item ??= cycleItem;
					// }
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

					string[] duplicatedOperations = [.. item.Operations.GroupBy(x => x.OperationNo).Where(g => g.Count() > 1).Select(y => y.Key)];
					if (duplicatedOperations?.Length > 0)
					{
						throw new Exception(string.Format("Product one or more OperationNo values are duplicated"));
					}

					// Validando Operaciones
					int opCount = 0;
					foreach (ProductOperation operation in item.Operations)
					{
						opCount++;

						//if (operation.OperationNo < 0)
						if (String.IsNullOrEmpty(operation.OperationNo ))
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
								CatProfile itemComp = (_catalogRepo.GetCatalogProfile(itm.ProfileCode)?.Find(x => x.Status != Status.Failed)) ?? throw new Exception(string.Format("Product Operation No.{0} - Labor {1} : ProfileCode not found. ", operation.OperationNo, itm.ProfileCode));
							});
						}
						if (operation.OperationTools is not null)
						{
							int countItems = 0;
							operation.OperationTools.ForEach(itm =>
							{
								countItems++;
								ToolType itemComp = (_toolOperation.ListToolTypes(itm.ToolingCode)?.Find(x => x.Status != Status.Failed)) ?? throw new Exception(string.Format("Product Operation No.{0} - ToolingType {1} : ToolingCode is invalid. ", operation.OperationNo, itm.ToolingCode));
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

								if (machine.MachineTools is not null)
								{
									int countItems = 0;
									machine.MachineTools.ForEach(itm =>
									{
										countItems++;
										Tool itemComp = _toolOperation.ListTools(itm.ToolingCode).FirstOrDefault() ?? throw new Exception(string.Format("Product Operation No.{0} - Machine {1} - ToolintType {2} : Tooling Code is invalid. ", operation.OperationNo, machine.MachineCode, itm.ToolingCode));
									});
								}
								if (machine.MachineLabor is not null)
								{
									int countItems = 0;
									machine.MachineLabor.ForEach(itm =>
									{
										countItems++;
										CatProfile itemComp = _catalogRepo.GetCatalogProfile(itm.ProfileCode).Find(x => x.Status != Status.Failed) ?? throw new Exception(string.Format("Product Operation No.{0} - Machine {1} - ProfileCode {2} :  Profile Code is invalid. ", operation.OperationNo, machine.MachineCode, itm.ProfileCode));
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
									oldOperation = oldProcesses?.Find(x => x.OperationNo.ToStr() == itmOperation.OperationNo.ToStr());
								}
								ProcessEntryProcess prc = new()
								{
									OperationNo = itmOperation.OperationNo.ToStr(),
									ProcessTypeId = CurrentOperationSubType.ProcessTypeId,
									ProcessSubTypeId = CurrentOperationSubType.Code,
									Name = itmOperation.OperationName ?? CurrentOperationSubType.Name,
									//Name = CurrentOperationSubType.Name,
									//Need to discuss with Mario
									//Step = Math.Floor(itmOperation.OperationNo).ToInt32(),
									//Sort = itmOperation.OperationNo == 0 ? 0 : (10 * (itmOperation.OperationNo.ToDecimal() % Math.Floor(itmOperation.OperationNo.ToDecimal()))).ToDouble().ToInt32(),
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
										prc.SlackTimeAfterPrevOp = Helper.Common.SecondsToTimeString(itmOperation.SlackTimeAftNextOp.ToInt32(), true);
									}
									else
									{
										prc.SlackTimeAfterPrevOp = Helper.Common.SecondsToTimeString(0, true);
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.SlackTimeAftNextOp.HasValue && !isForcedEdit)
									{
										prc.SlackTimeAfterPrevOp = Helper.Common.SecondsToTimeString(itmOperation.SlackTimeAftNextOp.ToInt32(), true);
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
										prc.SlackTimeBeforeNextOp = Helper.Common.SecondsToTimeString(itmOperation.SlackTimeBefNextOp.ToInt32(), true);
									}
									else
									{
										prc.SlackTimeBeforeNextOp = Helper.Common.SecondsToTimeString(0, true);
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.SlackTimeBefNextOp.HasValue && !isForcedEdit)
									{
										prc.SlackTimeBeforeNextOp = Helper.Common.SecondsToTimeString(itmOperation.SlackTimeBefNextOp.ToInt32(), true);
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
										prc.MaxTimeBeforeNextOp = Helper.Common.SecondsToTimeString(itmOperation.MaxTimeBefNextOp.ToInt32(), true);
									}
									else
									{
										prc.MaxTimeBeforeNextOp = Helper.Common.SecondsToTimeString(0, true);
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.MaxTimeBefNextOp.HasValue && !isForcedEdit)
									{
										prc.MaxTimeBeforeNextOp = Helper.Common.SecondsToTimeString(itmOperation.MaxTimeBefNextOp.ToInt32(), true);
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
										prc.MaxOpSpanIncrease = Helper.Common.SecondsToTimeString(itmOperation.MaxOpSpanIncrease.ToInt32(), true);
									}
									else
									{
										prc.MaxOpSpanIncrease = Helper.Common.SecondsToTimeString(0, true);
									}
								}
								else if (oldOperation is not null)
								{
									if (itmOperation.MaxOpSpanIncrease.HasValue && !isForcedEdit)
									{
										prc.MaxOpSpanIncrease = Helper.Common.SecondsToTimeString(itmOperation.MaxOpSpanIncrease.ToInt32(), true);
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
										prc.OperationNo = oldProcess.OperationNo;
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
									List<Activity> tasks =  _dataImportOperation.GetDataImportTasks(itmOperation, systemOperator);
									tasks ??= [];
									if (!editMode)
									{
										tasks.ForEach(tsk => tsk.OperationNo = prc.OperationNo);
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
											tsk.OperationNo = prc.OperationNo;
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
											OperationNo = prc.OperationNo,
											SortId = tsk.SortId,
											TriggerId = tsk.TriggerId,
											IsMandatory = tsk.IsMandatory,
											Origin = "OperationType"
										});
									});
								}
								// Machines
								prc.AvailableDevices = _dataImportOperation.GetDataImportAvailableDevices(itmOperation, oldOperation);
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
								prc.Subproducts = await _dataImportOperation.GetDataImportSubProducts(itmOperation).ConfigureAwait(false);
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
								prc.Attributes?.ForEach(x => x.OperationNo = prc.OperationNo);
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
					pe.Tools =  _dataImportOperation.GetDataImportTooling(item, pe, systemOperator);
					if (editMode && pe.Tools is not null && oldTools is not null)
					{
						pe.Tools.Where(tooling => string.IsNullOrEmpty(tooling.LineUID))?.ToList()?.ForEach(tlng =>
						{
							ProductOperationTool origTool = item.Operations.Find(x => x.OperationNo.ToStr() == tlng.OperationNo.ToStr())?.OperationTools.Find(ot => ot.ToolingCode == tlng.ToolId && ot.LineID == tlng.LineId.ToInt32());
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

					pe.Labor = _dataImportOperation.GetDataImportLabor(item, pe, systemOperator);
					if (editMode && pe.Labor is not null && oldLabors is not null)
					{
						pe.Labor.Where(elem => string.IsNullOrEmpty(elem.LineUID))?.ToList()?.ForEach(lbr =>
						{
							ProductOperationLabor origLbr = item.Operations.Find(x => x.OperationNo.ToStr() == lbr.OperationNo.ToStr())?.OperationLabor.Find(ot => ot.ProfileCode == lbr.LaborId && ot.LineID == lbr.LineId.ToInt32());
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
					pe.Components = await _dataImportOperation.GetDataImportItems(item, pe, systemOperator).ConfigureAwait(false);
					if (editMode && pe.Components is not null && oldComponents is not null)
					{
						pe.Components.Where(comp => string.IsNullOrEmpty(comp.LineUID))?.ToList()?.ForEach(cmp =>
						{
							ProductOperationItem origItm = item.Operations.Find(x => x.OperationNo.ToStr() == cmp.OperationNo.ToStr())?.OperationItems.Find(ot => ot.ItemCode == cmp.ComponentId && ot.LineID == cmp.LineId.ToInt32());
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
								Activity clonedActivity = await _activityOperation.CloneActivity(new Activity(tsk.Id), systemOperator, "PRODUCT").ConfigureAwait(false);
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

		if (!systemOperator.Permissions.Any(x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

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
					//ValidateOpcenterRules(entryInfo, systemOperator);

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
									z.OperationNo = x.OperationNo;
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
								x.Alternatives.ForEach(z => { z.OperationNo = x.OperationNo; z.ComponentId = x.ComponentId; });
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
								Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
								if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
								{
									task.Id = newActivity.Id;
									_ = _activityOperation.AssociateActivityProcessEntry(entryResult.Id, newActivity.OperationNo, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
								}
							}
							else
							{
								if (task.ActivityClassId > 0)
								{
									await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
								}
								_ = _activityOperation.AssociateActivityProcessEntry(entryResult.Id, task.OperationNo, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
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
							x.Attributes.ForEach(z => z.OperationNo = x.OperationNo);
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
				 await componentInfo.ProcessEntry.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);
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
							//ValidateOpcenterRules(entryInfo, systemOperator);

							if (_componentRepo.UpdateProcessEntry(entryInfo, systemOperator))
							{
								List<SubProduct> AllSubProducts = [];
								entryInfo.Processes.ForEach(x =>
								{
									if (x.Subproducts is not null)
									{
										x.Subproducts.ForEach(z => z.OperationNo = x.OperationNo);
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
										x.Alternatives.ForEach(z => { z.OperationNo = x.OperationNo; z.ComponentId = x.ComponentId; });
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
											Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
											if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
											{
												_ = _activityOperation.AssociateActivityProcessEntry(entryInfo.Id, newActivity.OperationNo, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
											}
										}
										else if (task.ManualDelete)
										{
											bool tempResult = _activityOperation.RemoveActivityProcessEntryAssociation(entryInfo.Id, task.OperationNo, task.Id, systemOperator);
										}
										else
										{
											if (task.ActivityClassId > 0)
											{
												await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
											}
											if (!string.IsNullOrEmpty(task.Id) && string.Equals(task.Origin.ToStr(), "OPERATIONTYPE", StringComparison.OrdinalIgnoreCase))
											{
												Activity clonedActivity = await _activityOperation.CloneActivity(new Activity(task.Id), systemOperator, "PRODUCT").ConfigureAwait(false);
												if (clonedActivity is not null)
												{
													task.Id = clonedActivity.Id;
												}
												_ = _activityOperation.AssociateActivityProcessEntry(entryInfo.Id, task.OperationNo, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
											}
											else
											{
												_ = _activityOperation.AssociateActivityProcessEntry(entryInfo.Id, task.OperationNo, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
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
										x.Attributes.ForEach(z => z.OperationNo = x.OperationNo);
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
								 await componentInfo.ProcessEntry.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);
							}
						}
						else
						{
							// CREAR NUEVA VERSION
							entryInfo = componentInfo.ProcessEntry;
							//Validar duplicados Opcenter
							//ValidateOpcenterRules(entryInfo, systemOperator);

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
										x.Subproducts.ForEach(z => z.OperationNo = x.OperationNo);
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
									if (string.IsNullOrEmpty(x.OperationNo))
									{
										x.OperationNo = Guid.NewGuid().ToString();
									}
								});

								string jsonMaterials = JsonConvert.SerializeObject(entryInfo.Components);
								string jsonAlternativeMaterials = string.Empty;

								List<AlternativeComponent> AllAlternatives = [];
								entryInfo.Components.ForEach(x =>
								{
									if (x.Alternatives is not null)
									{
										x.Alternatives.ForEach(z => { z.OperationNo = x.OperationNo; z.ComponentId = x.ComponentId; });
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
											Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
											if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
											{
												_ = _activityOperation.AssociateActivityProcessEntry(entryInfo.Id, newActivity.OperationNo, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
											}
										}
										else if (task.ManualDelete)
										{
											bool tempResult = _activityOperation.RemoveActivityProcessEntryAssociation(entryInfo.Id, task.OperationNo, task.Id, systemOperator);
										}
										else
										{
											if (task.ActivityClassId > 0)
											{
												await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
											}
											_ = _activityOperation.AssociateActivityProcessEntry(entryInfo.Id, task.OperationNo, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
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
										x.Attributes.ForEach(z => z.OperationNo = x.OperationNo);
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
								 await componentInfo.ProcessEntry.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);
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
	/// <summary>
	///
	/// </summary>
	public void ValidateOpcenterRules(ProcessEntry entryInfo, User SystemOperator)
	{
		string OpcLicenseType = Config.Configuration["OPC-LicenseType"].ToStr();
		if (!string.Equals(OpcLicenseType, "ULTIMATE", StringComparison.OrdinalIgnoreCase))
		{
			entryInfo.Labor ??= [];
			entryInfo.Tools ??= [];

			int duplicados = entryInfo.Labor.Where(x => !string.IsNullOrEmpty(x.MachineId)).Select(x => new { x.OperationNo, x.MachineId }).Concat(entryInfo.Tools.Where(x => !string.IsNullOrEmpty(x.MachineId)).Select(x => new { x.OperationNo, x.MachineId })).GroupBy(x => new { x.MachineId, x.OperationNo }).Where(g => g.Count() > 1).Select(y => y.Key).Count();

			if (duplicados > 0)
			{
				throw new Exception("OPCenter license Type does not allow more than one Labor/Tool per Operation");
			}
		}

		Machine[] machines = _deviceOperation.ListDevices(false, true, true);
		Warehouse warehouse = _warehouseOperation.ListWarehouse(SystemOperator).Where(w => w.WarehouseId == entryInfo.Warehouse).FirstOrDefault(x => x.Status != Status.Failed);
		if (warehouse is not null)
		{
			int wrongDevices = entryInfo.Processes
				.SelectMany(x => x.AvailableDevices)
				.Where(x => !string.IsNullOrEmpty(x.Id) && x.Id != "00000000-0000-0000-0000-000000000000")
				.Select(x => new
				{
					x.Id,
					Device = machines
					.FirstOrDefault(y => y.Id == x.Id && y.FacilityCode == warehouse.FacilityCode)
				})
				.Count(x => x.Device is null);
			if (wrongDevices > 0)
			{
				throw new Exception("One or more machines don't belong to Warehouse's facility");
			}
		}
	}
	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<List<ProcessEntry>> GetProcessEntry(string code, string warehouse, int? version, int? sequence, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return await _componentRepo.ListProcessEntry(code, warehouse, version, sequence).ConfigureAwait(false);
	}
	/// <summary>
	///
	/// </summary>
	public async Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "")
	{
		List<Component> returnValue;
		if (!string.IsNullOrEmpty(filter))
		{
			returnValue = await _componentRepo.ListComponents(componentId, true, filter).ConfigureAwait(false);
		}
		else if (!string.IsNullOrEmpty(componentId))
		{
			returnValue = await _componentRepo.ListComponents(componentId, false, string.Empty).ConfigureAwait(false);
		}
		else
		{
			returnValue = await _componentRepo.ListComponents(componentId, true, filter).ConfigureAwait(false);
		}
		return returnValue?.ToArray();
	}
	/// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateComponent(List<ComponentExternal> itemList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		List<MeasureUnit> unitsList = _measureUnitOperation.GetMeasureUnits();
		bool NotifyOnce = true;
		if (itemList?.Count > 0)
		{
			NotifyOnce = itemList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (ComponentExternal item in itemList)
			{
				Line++;
				try
				{
					Component OriginalComponent = (await GetComponents(item.ItemCode).ConfigureAwait(false))?.Where(c => c.Status != Status.Failed)?.FirstOrDefault();
					bool editMode = OriginalComponent is not null;
					BaseId = item.ItemCode;
					List<ValidationResult> results = [];
					ValidationContext context = new(item, null, null);

					if (!Validator.TryValidateObject(item, context, results))
					{
						throw new Exception($"{results[0]}");
					}

					Status status = string.Equals(item.Status.ToStr(), "ACTIVE", StringComparison.OrdinalIgnoreCase) ? Status.Active : Status.Disabled;

					if (!editMode && status == Status.Disabled)
					{
						throw new Exception("Cannot import a disabled Item");
					}
					Component itemInfo = new()
					{
						Code = item.ItemCode,
						Name = !string.IsNullOrEmpty(item.ItemName) ? item.ItemName : item.ItemCode,
						Status = status,
						ComponentType = ComponentType.Material
					};

					if (!string.IsNullOrEmpty(item.InventoryUoM))
					{
						MeasureUnit unitInventory = unitsList.Find(unit => string.Equals(unit.Code.Trim(), item.InventoryUoM.Trim(), StringComparison.OrdinalIgnoreCase) && unit.Status == Status.Active && unit.IsProductionResult);
						if (unitInventory is not null)
						{
							itemInfo.UnitInventory = unitInventory.Id;
						}
						else
						{
							throw new Exception("Item InventoryUoM is invalid");
						}
					}
					if (!string.IsNullOrEmpty(item.ProductionUoM))
					{
						MeasureUnit unitProduction = unitsList.Find(unit => string.Equals(unit.Code.Trim(), item.ProductionUoM.Trim(), StringComparison.OrdinalIgnoreCase) && unit.Status == Status.Active && unit.IsProductionResult);
						if (unitProduction is not null)
						{
							itemInfo.UnitProduction = unitProduction.Id;
						}
						else
						{
							throw new Exception("Item ProductionUoM is invalid");
						}
					}
					else
					{
						itemInfo.UnitProduction = itemInfo.UnitInventory;
					}

					if (!string.IsNullOrEmpty(item.ManagedBy))
					{
						int managedById = 0;
						switch (item.ManagedBy.Trim().ToUpperInvariant())
						{
							case "NONE":
								managedById = 1; // NO MANAGEMENT
								break;

							case "BATCH":
								managedById = 2; // BATCH
								break;

							case "SERIAL":
								managedById = 3; // SERIE
								break;
						}
						itemInfo.ManagedBy = managedById;
					}
					else
					{
						itemInfo.ManagedBy = 1; // NO MANAGEMENT
					}
					if (!string.IsNullOrEmpty(item.Type))
					{
						int typeId = 0;
						switch (item.Type.Trim().ToUpperInvariant())
						{
							case "PURCHASE":
								typeId = 1; // NO PURCHASE
								break;

							case "PRODUCTION":
								typeId = 2; // PRODUCTION
								break;
						}
						itemInfo.Type = typeId;
					}
					if (!string.IsNullOrEmpty(item.ItemGroupCode))
					{
						InventoryItemGroup inventoryInfo = _inventoryOperation.GetInventory(item.ItemGroupCode);
						if (inventoryInfo is not null && inventoryInfo.Code.Trim() == item.ItemGroupCode.Trim())
						{
							itemInfo.InventoryId = inventoryInfo.InventoryId;
						}
						else
						{
							throw new Exception("Invalid Item Group Code");
						}
					}

					if (editMode)
					{
						if (string.IsNullOrEmpty(item.ProductionUoM))
						{
							itemInfo.UnitProduction = OriginalComponent.UnitProduction;
						}
						if (string.IsNullOrEmpty(item.InventoryUoM))
						{
							itemInfo.UnitInventory = OriginalComponent.UnitInventory;
						}
						itemInfo.UnitTypes = OriginalComponent.UnitTypes;
					}

					// returnValue.Add(BrokerDAL.MergeComponent(itemInfo, systemOperator, Validate, Level));
					ResponseData response = await MergeComponent(itemInfo, systemOperator, Validate).ConfigureAwait(false);
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
	/// Merges an Item  into the system.
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<ResponseData> MergeComponent(Component componentInfo, User systemOperator, bool Validate = false, string Level = "Success", bool NotifyOnce = true)
	{
		ResponseData returnValue = null;

		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		if (componentInfo.ComponentType == ComponentType.Product && componentInfo.ProcessEntry is not null)
		{
			ProcessEntry entryInfo = componentInfo.ProcessEntry;
			//Validando Duplicados OPCenter
			//ValidateOpcenterRules(entryInfo, systemOperator);
			ProcessEntry entryResult = _componentRepo.CreateProcessEntry(entryInfo, systemOperator);
			if (!string.IsNullOrEmpty(entryResult.Id))
			{
				List<SubProduct> AllSubProducts = [];
				foreach (ProcessEntryProcess x in entryInfo.Processes.Where(static x => x.Subproducts is not null).ToList())
				{
					foreach (SubProduct z in x.Subproducts)
					{
						z.OperationNo = x.OperationNo;
					}
					AllSubProducts.AddRange(x.Subproducts);
				}

				string jsonOperations = JsonConvert.SerializeObject(entryInfo.Processes);
				string jsonSubProducts = string.Empty;

				if (AllSubProducts.Count > 0)
				{
					jsonSubProducts = JsonConvert.SerializeObject(AllSubProducts);
				}
				_ = (entryInfo.Components?.RemoveAll(static x => x.ComponentType == 0));

				string jsonMaterials = JsonConvert.SerializeObject(entryInfo.Components);
				string jsonAlternativeMaterials = string.Empty;
				List<AlternativeComponent> AllAlternatives = [];
				foreach (ProcessEntryComponent x in entryInfo.Components)
				{
					if (x.Alternatives is not null)
					{
						foreach (AlternativeComponent z in x.Alternatives)
						{
							z.OperationNo = x.OperationNo;
							z.ComponentId = x.ComponentId;
						}
						AllAlternatives.AddRange(x.Alternatives);
					}
				}
				if (AllAlternatives.Count > 0)
				{
					jsonAlternativeMaterials = JsonConvert.SerializeObject(AllAlternatives);
				}

				// Todo POner Tasks nueva version
				bool tempDetail = _componentRepo.SaveProductDetails(entryInfo, jsonOperations, jsonMaterials, jsonAlternativeMaterials, jsonSubProducts, systemOperator);

				componentInfo.ProcessEntryId = entryResult.Id;
				returnValue = _componentRepo.MergeComponent(componentInfo, systemOperator, Validate);
				if (string.IsNullOrEmpty(componentInfo.Id))
				{
					componentInfo.Id = returnValue.Id;
				}
			}

			if (!string.IsNullOrEmpty(entryResult.Id) && entryResult.Tasks is not null)
			{
				foreach (Activity task in entryResult.Tasks)
				{
					if (string.IsNullOrEmpty(task.Id))
					{
						task.Origin = OriginActivity.Product.ToStr();
						Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
						if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
						{
							task.Id = newActivity.Id;
							_activityOperation.AssociateActivityProcessEntry(entryResult.Id, newActivity.OperationNo, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
						}
					}
					else
					{
						if (task.ActivityClassId > 0)
						{
							await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
						}
						_activityOperation.AssociateActivityProcessEntry(entryResult.Id, task.OperationNo, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
					}
				}
			}

			if (!string.IsNullOrEmpty(entryResult.Id) && entryResult.Labor is not null)
			{
				foreach (ProcessEntryLabor x in entryResult.Labor)
				{
					if (string.IsNullOrEmpty(x.LaborId))
					{
						x.LaborId = x.Id;
					}
				}
				_componentRepo.MergeProcessEntryLabor(entryResult.Id, JsonConvert.SerializeObject(entryResult.Labor), systemOperator);
			}

			if (!string.IsNullOrEmpty(entryResult.Id) && entryResult.Tools is not null)
			{
				foreach (ProcessEntryTool x in entryResult.Tools)
				{
					if (string.IsNullOrEmpty(x.ToolId))
					{
						x.ToolId = x.Id;
					}
				}
				_componentRepo.MergeProcessEntryTools(entryResult.Id, JsonConvert.SerializeObject(entryResult.Tools), systemOperator);
			}

			foreach (ProcessEntryProcess x in entryResult.Processes)
			{
				if (x.Attributes is not null)
				{
					foreach (ProcessEntryAttribute z in x.Attributes)
					{
						z.OperationNo = x.OperationNo;
					}
				}
				else
				{
					x.Attributes = [];
				}
			}
			List<ProcessEntryAttribute> attrs = [.. entryResult.Processes.SelectMany(static x => x.Attributes)];
			if (!string.IsNullOrEmpty(entryResult.Id) && attrs is not null)
			{
				attrs.ForEach(static x => { if (string.IsNullOrEmpty(x.AttributeId)) { x.AttributeId = x.Id; } });
				_componentRepo.MergeProcessEntryAttributes(entryResult.Id, JsonConvert.SerializeObject(attrs), systemOperator);
			}
		}
		else
		{
			returnValue = _componentRepo.MergeComponent(componentInfo, systemOperator, Validate);
		}

		if (!Validate && returnValue?.IsSuccess == true)
		{
			Component ObjItem = _componentRepo.GetComponentByCode(returnValue.Code);
			returnValue.Id = ObjItem.Id;
			returnValue.Entity = ObjItem;
			if (NotifyOnce)
			{
				await _attachmentOperation.SaveImageEntity("Item", componentInfo.Image, componentInfo.Code, systemOperator).ConfigureAwait(false);
				if (componentInfo.AttachmentIds is not null)
				{
					foreach (string attachment in componentInfo.AttachmentIds)
					{
						await _attachmentOperation.AttachmentSync(attachment, returnValue.Code, systemOperator).ConfigureAwait(false);
					}
				}
				//Services.ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Item, returnValue.Action, Data = ObjItem }, returnValue.Action != ActionDB.IntegrateAll ? systemOperator.TimeZoneOffset : 0);
			}
			await ObjItem.Log(returnValue.Action == ActionDB.Create ? EntityLogType.Create : EntityLogType.Update, systemOperator).ConfigureAwait(false);
		}

		return returnValue;
	}
	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public async Task<List<ProcessEntry>> GetProcessEntryById(string id, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return await _componentRepo.ListProcessEntry(null, null, 0, 0, id).ConfigureAwait(false);
	}


}