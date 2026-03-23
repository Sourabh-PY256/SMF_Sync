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
using System.Threading.Tasks;


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

	// ─── Shared normalization ─────────────────────────────────────────────────
	/// <summary>
	/// Applies the same normalization and defaulting rules used by
	/// <see cref="Component.FromProductExternal"/> so that both UI-sourced
	/// <see cref="Component"/> objects and ERP-converted ones are always in a
	/// consistent state before being persisted.
	/// </summary>
	private static void NormalizeComponent(Component component)
	{
		// 1. Code is mandatory
		if (string.IsNullOrWhiteSpace(component.Code))
			throw new ArgumentException("Product Code is required.");

		// 2. Default Name to Code when not supplied
		if (string.IsNullOrWhiteSpace(component.Name))
			component.Name = component.Code;

		// 3. Always treat this as a Product component
		component.ComponentType = ComponentType.Product;

		// 4. Default Status to Active
		if (component.Status == default)
			component.Status = Status.Active;

		// 5. Ensure ProcessEntry exists and its key fields are in sync
		component.ProcessEntry ??= new ProcessEntry();

		ProcessEntry pe = component.ProcessEntry;

		// Sync identifiers from parent Component
		if (string.IsNullOrEmpty(pe.Code))       pe.Code      = component.Code;
		if (string.IsNullOrEmpty(pe.Name))       pe.Name      = component.Name;
		if (string.IsNullOrEmpty(pe.Warehouse))  pe.Warehouse = component.WarehouseId;
		if (pe.Version == 0)                     pe.Version   = component.Version;

		// 6. Default ProcessEntry Status to Active
		if (pe.Status == default) pe.Status = Status.Active;

		// 7. Ensure child collections are never null (mirrors FromProductExternal)
		pe.Processes   ??= [];
		pe.Labor       ??= [];
		pe.Tools       ??= [];
		pe.Components  ??= [];

		// 8. Validate BOM/Route version/sequence are not negative
		if (pe.BomVersion  < 0) pe.BomVersion  = 0;
		if (pe.BomSequence < 0) pe.BomSequence = 0;
		if (pe.RouteVersion  < 0) pe.RouteVersion  = 0;
		if (pe.RouteSequence < 0) pe.RouteSequence = 0;

		// 9. Normalize Schedule - must be 0 or 1
		if (pe.Schedule is not (0 or 1)) pe.Schedule = 0;

		// 10. Validate all process entries child collections
		foreach (ProcessEntryProcess proc in pe.Processes)
		{
			proc.AvailableDevices ??= [];
			proc.Subproducts      ??= [];
			proc.Attributes       ??= [];
		}
	}

	// ─── ProcessProduct overloads ─────────────────────────────────────────────

	/// <summary>
	/// Entry point for UI callers that send a <see cref="Component"/> model.
	/// Applies shared normalization and delegates to the core merge logic.
	/// </summary>
	public Task<ResponseData> ProcessProduct(ActionDB mode, Component component, User systemOperator)
	{
		NormalizeComponent(component);
		return MergeProduct(mode, component, systemOperator, intSource: IntegrationSource.SF);
	}

	/// <summary>
	/// Entry point for ERP/DataSync callers that send a <see cref="ProductExternal"/> model.
	/// Converts to <see cref="Component"/>, applies shared normalization, and delegates
	/// to the core merge logic.
	/// </summary>
	public Task<ResponseData> ProcessProduct(ActionDB mode, ProductExternal externalProduct, User systemOperator)
	{
		Component component = Component.FromProductExternal(externalProduct);
		NormalizeComponent(component); // safety pass – same rules for ERP too
		return MergeProduct(mode, component, systemOperator, intSource: IntegrationSource.ERP);
	}

	/// <summary>
	/// Bulk sync entry point for ERP/DataSync lists.
	/// Accepts pre-converted <see cref="Component"/> lists.
	/// Automatically determines mode (Create/Update) based on existing components,
	/// then normalizes and merges via ProcessProduct.
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateProduct(List<Component> itemList, List<Component> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		if (itemList?.Count > 0)
		{
			foreach (Component item in itemList)
			{
				try
				{
					// For synchronization, determine the ActionDB mode automatically.
					Component existingComponent = (await GetComponents(item.Code, true).ConfigureAwait(false))?.FirstOrDefault(c => c.Status != Status.Failed);
					
					ActionDB mode = ActionDB.Create;
					if (existingComponent != null)
					{
						item.Id = existingComponent.Id;
						mode = ActionDB.Update;
					}

					// Use ProcessProduct -> this runs NormalizeComponent so ERP behaves like UI
					ResponseData resp = await ProcessProduct(mode, item, systemOperator).ConfigureAwait(false);
					returnValue.Add(resp);
				}
				catch (Exception ex)
				{
					returnValue.Add(new ResponseData
					{
						Code = item.Code,
						Entity = "Product",
						IsSuccess = false,
						Message = ex.Message
					});
				}
			}
		}
		return returnValue;
	}

	/// <summary>
	/// Core persistence logic. Merges a normalized <see cref="Component"/> into the system.
	/// Call <see cref="ProcessProduct"/> for new work — it ensures normalization runs first.
	/// </summary>
	public async Task<ResponseData> MergeProduct(ActionDB mode, Component componentInfo, User systemOperator, bool Validate = false, LevelMessage Level = LevelMessage.Success, bool NotifyOnce = true, bool isNewVersion = false, bool isExternalEndpoint = false, IntegrationSource intSource = IntegrationSource.SF)
	{
		ResponseData returnValue = null;


		if (componentInfo.ProcessEntry.MinQuantity > componentInfo.ProcessEntry.MaxQuantity && componentInfo.ProcessEntry.MaxQuantity > 0)
		{
			throw new Exception("Maximum quantity must be greater than Minimum Quantity");
		}

		// UNIFIED LOGIC: Enrich with ERP data if the source is ERP
		if (intSource == IntegrationSource.ERP && componentInfo.ComponentType == ComponentType.Product && componentInfo.ProcessEntry is not null)
		{
			ProcessEntry pe = componentInfo.ProcessEntry;
			ProductExternal item = new() { ProductCode = pe.Code, WarehouseCode = pe.Warehouse, Version = pe.Version, Sequence = pe.Sequence };
			
			// Fetch external details from ERP logic
			pe.Tasks = _dataImportOperation.GetDataImportTasks(item, systemOperator);
			pe.Components = await _dataImportOperation.GetDataImportItems(item, pe, systemOperator).ConfigureAwait(false);
			pe.Tools = await _dataImportOperation.GetDataImportTooling(item, pe, systemOperator).ConfigureAwait(false);
			pe.Labor = await _dataImportOperation.GetDataImportLabor(item, pe, systemOperator).ConfigureAwait(false);
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
							//isNewVersion = true;
							componentInfo.ProcessEntry.Id = Guid.CreateVersion7().ToStr();
						}
					}
					//Validar duplicados Opcenter
					//await ValidateOpcenterRules(entryInfo, systemOperator).ConfigureAwait(false);

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
										a.LineUID = Guid.CreateVersion7().ToStr();
									}
								});
							if (x.Subproducts is not null)
							{
								foreach (SubProduct z in x.Subproducts)
								{
									z.ProcessId = x.ProcessId;
									if (string.IsNullOrEmpty(z.LineUID))
									{
										z.LineUID = Guid.CreateVersion7().ToString();
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

						entryInfo.Components?.RemoveAll(x => x.ComponentType == 0);

						string jsonMaterials = JsonConvert.SerializeObject(entryInfo.Components);
						string jsonAlternativeMaterials = string.Empty;

						List<AlternativeComponent> AllAlternatives = [];
						entryInfo.Components.ForEach(x =>
						{
							if (string.IsNullOrEmpty(x.LineUID))
							{
								x.LineUID = Guid.CreateVersion7().ToStr();
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

						//await AttachmentBulkSync("Products", componentInfo.Image, componentInfo.ProcessEntryId, entryInfo.AttachmentIds, returnValue.Id, systemOperator).ConfigureAwait(false);

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
									_activityOperation.AssociateActivityProcessEntry(entryResult.Id, newActivity.ProcessId, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
								}
							}
							else
							{
								if (task.ActivityClassId > 0)
								{
									await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
								}
								_activityOperation.AssociateActivityProcessEntry(entryResult.Id, task.ProcessId, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
							}
						}
					}

					if (!string.IsNullOrEmpty(entryResult.Id) && entryResult.Labor is not null)
					{
						entryResult.Labor.ForEach(x =>
						{
							if (string.IsNullOrEmpty(x.LaborId)) { x.LaborId = x.Id; }
							if (string.IsNullOrEmpty(x.LineUID)) { x.LineUID = Guid.CreateVersion7().ToString(); }
						});
						_componentRepo.MergeProcessEntryLabor(entryResult.Id, JsonConvert.SerializeObject(entryResult.Labor), systemOperator);
					}

					if (!string.IsNullOrEmpty(entryResult.Id) && entryResult.Tools is not null)
					{
						entryResult.Tools.ForEach(x =>
						{
							if (string.IsNullOrEmpty(x.ToolId)) { x.ToolId = x.Id; }
							if (string.IsNullOrEmpty(x.LineUID)) { x.LineUID = Guid.CreateVersion7().ToString(); }
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
					//await AttachmentBulkSync("Products", componentInfo.Image, returnValue.Id, componentInfo.ProcessEntry.AttachmentIds, returnValue.Id, systemOperator).ConfigureAwait(false);

					componentInfo.Id = returnValue.Id;
					returnValue.Entity = componentInfo;
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
							componentInfo.ProcessEntry.Id = Guid.CreateVersion7().ToStr();
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
							//await ValidateOpcenterRules(entryInfo, systemOperator).ConfigureAwait(false);

							if (_componentRepo.UpdateProcessEntry(entryInfo, systemOperator))
							{
								List<SubProduct> AllSubProducts = [];
								foreach (ProcessEntryProcess x in entryInfo.Processes)
								{
									if (x.Subproducts is not null)
									{
										x.Subproducts.ForEach(z => z.ProcessId = x.ProcessId);
										AllSubProducts.AddRange(x.Subproducts);
									}
								}

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

								entryInfo.Components?.RemoveAll(x => x.ComponentType == 0);

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
									//await AttachmentBulkSync("Products", componentInfo.Image, componentInfo.ProcessEntryId, entryInfo.AttachmentIds, entryInfo.Id, systemOperator).ConfigureAwait(false);

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
												_activityOperation.AssociateActivityProcessEntry(entryInfo.Id, newActivity.ProcessId, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
											}
										}
										else if (task.ManualDelete)
										{
											bool tempResult = _activityOperation.RemoveActivityProcessEntryAssociation(entryInfo.Id, task.ProcessId, task.Id, systemOperator);
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
												_activityOperation.AssociateActivityProcessEntry(entryInfo.Id, task.ProcessId, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
											}
											else
											{
												_activityOperation.AssociateActivityProcessEntry(entryInfo.Id, task.ProcessId, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
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
								await componentInfo.ProcessEntry.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);
							}
						}
						else
						{
							// CREAR NUEVA VERSION
							entryInfo = componentInfo.ProcessEntry;
							//Validar duplicados Opcenter
							//await ValidateOpcenterRules(entryInfo, systemOperator).ConfigureAwait(false);

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
										x.ProcessId = Guid.CreateVersion7().ToString();
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
											task.Origin = nameof(OriginActivity.Product);
											Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
											if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
											{
												_activityOperation.AssociateActivityProcessEntry(entryInfo.Id, newActivity.ProcessId, newActivity.Id, newActivity.TriggerId, newActivity.SortId, newActivity.IsMandatory, newActivity.RawMaterials, systemOperator);
											}
										}
										else if (task.ManualDelete)
										{
											bool tempResult = _activityOperation.RemoveActivityProcessEntryAssociation(entryInfo.Id, task.ProcessId, task.Id, systemOperator);
										}
										else
										{
											if (task.ActivityClassId > 0)
											{
												await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
											}
											_activityOperation.AssociateActivityProcessEntry(entryInfo.Id, task.ProcessId, task.Id, task.TriggerId, task.SortId, task.IsMandatory, task.RawMaterials, systemOperator);
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
								await componentInfo.ProcessEntry.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);
							}
						}
					}
				}
				else
				{
					ResponseData mrgComponent = _componentRepo.MergeProduct(componentInfo, systemOperator, Validate, Level);
					//await AttachmentBulkSync("Products", componentInfo.Image, componentInfo.ProcessEntryId, componentInfo.ProcessEntry?.AttachmentIds, componentInfo.Code, systemOperator).ConfigureAwait(false);

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
	public async Task ValidateOpcenterRules(ProcessEntry entryInfo, User SystemOperator)
	{
		string OpcLicenseType = Config.Configuration["OPC-LicenseType"].ToStr();
		if (!string.Equals(OpcLicenseType, "ULTIMATE", StringComparison.OrdinalIgnoreCase))
		{
			entryInfo.Labor ??= [];
			entryInfo.Tools ??= [];

			int duplicados = entryInfo.Labor.Where(x => !string.IsNullOrEmpty(x.MachineId)).Select(x => new { x.ProcessId, x.MachineId }).Concat(entryInfo.Tools.Where(x => !string.IsNullOrEmpty(x.MachineId)).Select(x => new { x.ProcessId, x.MachineId })).GroupBy(x => new { x.MachineId, x.ProcessId }).Where(g => g.Count() > 1).Select(y => y.Key).Count();

			if (duplicados > 0)
			{
				throw new Exception("OPCenter license Type does not allow more than one Labor/Tool per Operation");
			}
		}

		Machine[] machines = await _deviceOperation.ListDevices(false, true, true).ConfigureAwait(false);
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

		// if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		// }

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

		// if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		// }

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
						z.ProcessId = x.ProcessId;
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
							z.ProcessId = x.ProcessId;
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
						z.ProcessId = x.ProcessId;
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

		// if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		// }

		#endregion Permission validation

		return await _componentRepo.ListProcessEntry(null, null, 0, 0, id).ConfigureAwait(false);
	}


}