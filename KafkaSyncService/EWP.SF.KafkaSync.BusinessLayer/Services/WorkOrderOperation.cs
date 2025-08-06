using EWP.SF.Common.Models;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using Newtonsoft.Json;
using EWP.SF.Common.Models.MigrationModels;
using System.Transactions;
using System.Xml.Serialization;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using EWP.SF.Common.Constants;
using EWP.SF.KafkaSync.BusinessEntities;
using Confluent.Kafka;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class WorkOrderOperation : IWorkOrderOperation
{
	private readonly IWorkOrderRepo _workOrderRepo;
	private readonly IMeasureUnitOperation _measureUnitOperation;
	private readonly IWarehouseOperation _warehouseOperation;
	private readonly IEmployeeOperation _employeeOperation;
	private readonly  IOrderTransactionProductRepo _orderTransactionProductRepo;
	private readonly IDataSyncServiceOperation _dataSyncServiceOperation;
	private readonly IProcessTypeOperation _processTypeOperation;
	private readonly IComponentOperation _componentOperation;
	private readonly IActivityOperation _activityOperation;
	private readonly IDataImportOperation _dataImportOperation;
	private readonly IInventoryOperation _inventoryOperation;
	private readonly IMachineRepo _machineRepo;
	private readonly IToolOperation _toolOperation;
	private readonly IDeviceOperation _deviceOperation;
	private readonly ILaborRepo _laborRepo;


	public WorkOrderOperation(IWorkOrderRepo workOrderRepo, ICatalogRepo catalogRepo, IApplicationSettings applicationSettings
	, IMeasureUnitOperation measureUnitOperation, IEmployeeOperation employeeOperation
	, IWarehouseOperation warehouseOperation, IDataSyncServiceOperation dataSyncServiceOperation
	, IOrderTransactionProductRepo orderTransactionProductRepo, IProcessTypeOperation processTypeOperation
	, IComponentOperation componentOperation, IActivityOperation activityOperation
	, IDataImportOperation dataImportOperation, IInventoryOperation inventoryOperation
	, IMachineRepo machineRepo, IToolOperation toolOperation, IDeviceOperation deviceOperation, ILaborRepo laborRepo)
	{
		_workOrderRepo = workOrderRepo;
		_measureUnitOperation = measureUnitOperation;
		_warehouseOperation = warehouseOperation;
		_employeeOperation = employeeOperation;
		_dataSyncServiceOperation = dataSyncServiceOperation;
		_orderTransactionProductRepo = orderTransactionProductRepo;
		_processTypeOperation = processTypeOperation;
		_componentOperation = componentOperation;
		_activityOperation = activityOperation;
		_dataImportOperation = dataImportOperation;
		_inventoryOperation = inventoryOperation;
		_machineRepo = machineRepo;
		_toolOperation = toolOperation;
		_deviceOperation = deviceOperation;
		_laborRepo = laborRepo;
	}
	private static string RemoveXMLHeader(string xml) => xml.Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
	/// <summary>
	///
	/// </summary>
	public WorkOrder GetWorkOrderByCode(string workOrderCode) => _workOrderRepo.GetWorkOrderByCode(workOrderCode);
	/// <summary>
	///
	/// </summary>
	public List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(List<ProductionOrderChangeStatusExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<WorkOrderResponse> returnValue = [];
		List<ProcedureExternal> proceduresExternal = [];
		List<KeyValuePair<string, int>> processedValues = [];
		WorkOrderResponse MessageError;
		bool NotifyOnce = true;
		if (workOrderList?.Count > 0)
		{
			NotifyOnce = workOrderList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			workOrderList.ForEach(workOrder =>
			{
				Line++;
				try
				{
					BaseId = workOrder.OrderCode;
					List<ValidationResult> results = [];
					ValidationContext context = new(workOrder, null, null);
					if (!Validator.TryValidateObject(workOrder, context, results))
					{
						throw new Exception($"{results[0]}");
					}
					// order
					string orderId = string.Empty;
					WorkOrder wo = GetWorkOrderByCode(workOrder.OrderCode);
					if (wo is not null)
					{
						orderId = wo.Id;
					}
					if (string.IsNullOrEmpty(orderId))
					{
						throw new Exception("Order Doesn't Exists");
					}
					// status
					BMMOrderStatus status = workOrder.Status switch
					{
						"New" => BMMOrderStatus.New,
						"In Progress" => BMMOrderStatus.Running,
						"Released" => BMMOrderStatus.Approved,
						"Queued" => BMMOrderStatus.Queued,
						"Cancelled" => BMMOrderStatus.Cancelled,
						"On Hold" => BMMOrderStatus.Hold,
						"Finished" => BMMOrderStatus.Finished,
						_ => BMMOrderStatus.Error,
					};
					if (status == BMMOrderStatus.Error)
					{
						throw new Exception("Invalid Status (Expected Values: New|In Progress|Released|Queued|Cancelled|On Hold|Finished)");
					}

					if (status == BMMOrderStatus.Approved && wo.Status == Status.Active)
					{
						status = BMMOrderStatus.Running;
					}
					if (status.ToInt32() != wo.Status.ToInt32())
					{
						// reglas status
						List<int> lstStatusValidation = [];
						// Si estatus 3,6,7 No permite hacer nada
						lstStatusValidation = [(int)BMMOrderStatus.Deleted, (int)BMMOrderStatus.Finished, (int)BMMOrderStatus.Cancelled];
						if (lstStatusValidation.Contains((int)wo.Status))
						{
							throw new Exception("Orders With Status Deleted, Finished Or Cancelled Cannot Be changed");
						}
						// Si es 1 : Permitidos son : 1,6,8
						if ((BMMOrderStatus)wo.Status == BMMOrderStatus.Running)
						{
							lstStatusValidation = [(int)BMMOrderStatus.Running, (int)BMMOrderStatus.Finished, (int)BMMOrderStatus.Hold];
							if (!lstStatusValidation.Contains((int)status))
							{
								throw new Exception("Orders With Status In Progress Only Can Be Changed To: In Progress, Finished Or On Hold");
							}
						}
						// Si estatus 2: Permitidos son: 4,5,7,8
						if ((BMMOrderStatus)wo.Status == BMMOrderStatus.New)
						{
							lstStatusValidation = [(int)BMMOrderStatus.Approved, (int)BMMOrderStatus.Queued, (int)BMMOrderStatus.Cancelled, (int)BMMOrderStatus.Hold, (int)BMMOrderStatus.Finished];
							if (!lstStatusValidation.Contains((int)status))
							{
								throw new Exception("Orders With Status New Only Can Be Changed To: Released, Queued, Cancelled, Finished Or On Hold");
							}
						}
						// Si estatus : 4 : Permitidos 4,5,7,8
						if ((BMMOrderStatus)wo.Status == BMMOrderStatus.Approved)
						{
							lstStatusValidation = [(int)BMMOrderStatus.Approved, (int)BMMOrderStatus.Queued, (int)BMMOrderStatus.Cancelled, (int)BMMOrderStatus.Hold, (int)BMMOrderStatus.Finished, (int)BMMOrderStatus.Running];
							if (!lstStatusValidation.Contains((int)status))
							{
								throw new Exception("Orders With Status Released Only Can Be Changed To: In Progress, Released, Queued, Finished, Cancelled Or On Hold");
							}
						}
						// Si estatus : 5 : Permitidos 4,5,7,8
						if ((BMMOrderStatus)wo.Status == BMMOrderStatus.Queued)
						{
							lstStatusValidation = [(int)BMMOrderStatus.Approved, (int)BMMOrderStatus.Queued, (int)BMMOrderStatus.Cancelled, (int)BMMOrderStatus.Hold, (int)BMMOrderStatus.Finished];
							if (!lstStatusValidation.Contains((int)status))
							{
								throw new Exception("Orders With Status Queued Only Can Be Changed To: Released, Queued, Finished, Cancelled Or On Hold");
							}
						}
						// Si estatus 8 : Permitidos 1, 6, 8
						if ((BMMOrderStatus)wo.Status == BMMOrderStatus.Hold)
						{
							lstStatusValidation = [(int)BMMOrderStatus.New, (int)BMMOrderStatus.Running, (int)BMMOrderStatus.Finished, (int)BMMOrderStatus.Hold];
							if (!lstStatusValidation.Contains((int)status))
							{
								throw new Exception("Orders With Status On Hold Only Can Be Changed To: New, Running, Finished Or On Hold");
							}
						}
						// workOrder info
						WorkOrderChangeStatus workOrderInfo = new()
						{
							OrderId = orderId,
							OrderCode = workOrder.OrderCode,
							Status = status
						};

						returnValue.Add(_workOrderRepo.MergeWorkOrderChangeStatus(workOrderInfo, systemOperator, Validate, Level));
						if (wo.Status.ToInt32() != status.ToInt32())
						{
							processedValues.Add(new KeyValuePair<string, int>(orderId, workOrderInfo.Status.ToInt32()));
						}
					}
					else
					{
						returnValue.Add(new WorkOrderResponse
						{
							Action = ActionDB.Update,
							IsSuccess = true,
							Code = workOrder.OrderCode,
						});
					}
				}
				catch (Exception ex)
				{
					MessageError = new WorkOrderResponse
					{
						Id = BaseId,
						Message = ex.Message,
						Code = "Line:" + Line.ToStr()
					};
					returnValue.Add(MessageError);
				}
			});
		}
		if (!Validate)
		{
			// if (!NotifyOnce)
			// {
			// 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductionOrderChangeStatus, Action = ActionDB.IntegrateAll.ToStr() });
			// }
			if (processedValues.Count > 0)
			{
				SyncInitializer.ForcePush(new MessageBroker
				{
					Type = MessageBrokerType.WorkOrder,
					ElementId = JsonConvert.SerializeObject(processedValues),
					ElementValue = systemOperator.Id.ToStr(),
					Aux = "S"
				});
			}
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
	public Task<List<WorkOrder>> GetWorkOrder(string workOrderId) => _workOrderRepo.GetWorkOrder(workOrderId);
	public List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		List<MeasureUnit> unitsList = _measureUnitOperation.GetMeasureUnits();
		List<Employee> employeeList = _employeeOperation.GetEmployees(string.Empty, string.Empty, systemOperator);
		List<ClockInOutDetailsExternal> detailsToMerge = [];
		const bool NotifyOnce = false;

		if (clockList?.Count > 0)
		{
			int Line = 0;
			string BaseId = string.Empty;
			ClockInOutDetailsExternal itemInfo = null;
			clockList.ForEach(cycleDetail =>
			{
				itemInfo = cycleDetail;
				Line++;
				try
				{
					BaseId = cycleDetail.ClockInOutId;

					List<ValidationResult> results = [];
					ValidationContext context = new(cycleDetail, null, null);

					if (!Validator.TryValidateObject(cycleDetail, context, results) && results.Count > 0)
					{
						throw new Exception($"{results[0]}");
					}

					if (string.IsNullOrEmpty(cycleDetail.EmployeeCode))
					{
						throw new Exception("Employee code is required");
					}

					Employee employee = employeeList.Find(emp => emp.ExternalId.Equals(cycleDetail.EmployeeCode, StringComparison.OrdinalIgnoreCase));

					if (employee is null)
					{
						employee = employeeList.Find(emp => emp.Code.Equals(cycleDetail.EmployeeCode, StringComparison.OrdinalIgnoreCase));
					}
					else
					{
						cycleDetail.EmployeeCode = employee.Code;
					}
					if (employee is null)
					{
						throw new Exception(string.Format("Employee code \"{0}\" does not exist", cycleDetail.EmployeeCode));
					}
					if (!cycleDetail.StartDate.HasValue)
					{
						throw new Exception("Start Date is required");
					}

					detailsToMerge.Add(cycleDetail);
					ResponseData response = new()
					{
						Code = cycleDetail.ClockInOutId,
						Action = ActionDB.IntegrateAll,
						Entity = cycleDetail,
						EntityAlt = itemInfo,
						IsSuccess = true,
						Id = cycleDetail.ClockInOutId
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
					if (string.IsNullOrEmpty(cycleDetail.ClockInOutId))
					{
						MessageError.Code = "Line:" + Line.ToStr();
					}
					else
					{
						MessageError.Code = cycleDetail.ClockInOutId;
					}
					MessageError.Entity = cycleDetail;
					MessageError.EntityAlt = itemInfo;
					returnValue.Add(MessageError);
				}
			});

			string itemsJson = JsonConvert.SerializeObject(detailsToMerge, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			ResponseData result = _workOrderRepo.MergeClockInOutBulk(itemsJson, systemOperator, Validate);
		}
		if (!Validate)
		{
			// if (!NotifyOnce)
			// {
			// 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.Item, Action = ActionDB.IntegrateAll.ToStr() });
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
	public async Task<double> GetTimezoneOffset(string offSetName = "")
	{
		double offset = 0;
		if (offSetName == "ERP")
		{
			if (!ContextCache.ERPOffset.HasValue)
			{
				try
				{
					List<TimeZoneCatalog> tz = await _dataSyncServiceOperation.GetTimezones(true).ConfigureAwait(false);
					TimeZoneCatalog erpOffset = tz.Find(x => x.Key == "ERP");
					offset = erpOffset.Offset;
					ContextCache.ERPOffset = offset;
				}
				catch { }
			}
			else
			{
				offset = ContextCache.ERPOffset.Value;
			}
		}
		else
		{
			List<TimeZoneCatalog> tz = await _dataSyncServiceOperation.GetTimezones(true).ConfigureAwait(false);
			if (string.IsNullOrEmpty(offSetName))
			{
				TimeZoneCatalog SfOffset = tz.Find(x => x.Key == "SmartFactory");
				TimeZoneCatalog erpOffset = tz.Find(x => x.Key == "ERP");
				double baseOffset = 0;
				double integrationOffset = 0;
				if (SfOffset is not null)
				{
					baseOffset = SfOffset.Offset;
				}
				if (erpOffset is not null)
				{
					integrationOffset = erpOffset.Offset;
				}
				offset = baseOffset - integrationOffset;
			}
			else
			{
				TimeZoneCatalog namedOffset = tz.Find(x => x.Key == offSetName);
				if (namedOffset is not null)
				{
					offset = namedOffset.Offset;
				}
			}
		}
		return offset;
	}
	/// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateProductTransfer(List<ProductTransferExternal> transferList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		List<Warehouse> warehouses = _warehouseOperation.ListWarehouse(systemOperator);
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (transferList?.Count > 0)
		{
			NotifyOnce = transferList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (ProductTransferExternal transaction in transferList)
			{
				Line++;
				try
				{
					BaseId = transaction.OrderCode;
					List<ValidationResult> results = [];
					ValidationContext context = new(transaction, null, null);
					if (!Validator.TryValidateObject(transaction, context, results))
					{
						throw new Exception($"{results[0]}");
					}
					// order
					string orderId = string.Empty;
					WorkOrder wo = (await GetWorkOrder(transaction.OrderCode).ConfigureAwait(false)).FirstOrDefault();
					if (wo is not null)
					{
						orderId = wo.Id;
					}
					if (string.IsNullOrEmpty(orderId))
					{
						throw new Exception("Order doesn't exist");
					}
					// status

					OrderProcess process = wo.Processes.Find(wop => wop.IsOutput) ?? throw new Exception("Error finding last operation");
					transaction.OperationNo = process.ProcessId.ToDouble();
					List<ReturnMaterialContext> orderContext = GetProductReturnContext(transaction.OrderCode, systemOperator);

					transaction.Items.ForEach(itm =>
					{
						if (itm.Lots is null || itm.Lots.Count == 0)
						{
							throw new Exception("Order " + transaction.OrderCode + ": item " + itm.ItemCode + " details are required");
						}

						if (!warehouses.Any(w => string.Equals(w.Code, itm.FromWarehouseCode, StringComparison.OrdinalIgnoreCase)))
						{
							throw new Exception("Order " + transaction.OrderCode + ": item " + itm.ItemCode + " FromWarehouse does not exist");
						}
						Warehouse selectedWhs = warehouses.Find(w => string.Equals(w.Code, itm.ToWarehouseCode, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception("Order " + transaction.OrderCode + ": item " + itm.ItemCode + " ToWarehouse does not exist");
						if (selectedWhs.Details?.Count > 0 && string.IsNullOrEmpty(itm.ToBinLocationCode))
						{
							throw new Exception("Order " + transaction.OrderCode + ": item " + itm.ItemCode + " ToBinLocationCode is required for warehouse " + selectedWhs.Code);
						}

						if (!string.IsNullOrEmpty(itm.ToBinLocationCode) && !string.IsNullOrEmpty(itm.ToInventoryStatusCode))
						{
							BinLocation currentBin = (selectedWhs?.Details?.Find(bl => string.Equals(bl.LocationCode, itm.ToBinLocationCode, StringComparison.OrdinalIgnoreCase))) ?? throw new Exception("Order " + transaction.OrderCode + ":  item " + itm.ItemCode + " ToBinlocationCode not found in warehouse " + selectedWhs.Code);
							if (!currentBin.InventoryStatusCodes.Contains(itm.ToInventoryStatusCode))
							{
								throw new Exception("Order " + transaction.OrderCode + ":  item " + itm.ItemCode + " ToBinlocationCode doesnt match ToInventoryStatusCode");
							}
						}
						else if (!string.IsNullOrEmpty(itm.ToBinLocationCode))
						{
							BinLocation currentBin = (selectedWhs?.Details?.Find(bl => string.Equals(bl.LocationCode, itm.ToBinLocationCode, StringComparison.OrdinalIgnoreCase))) ?? throw new Exception("Order " + transaction.OrderCode + ":  item " + itm.ItemCode + " ToBinlocationCode not found on warehouse " + selectedWhs.Code);
							if (currentBin?.InventoryStatusCodes.Count > 0)
							{
								itm.ToInventoryStatusCode = currentBin.InventoryStatusCodes.FirstOrDefault();
							}
							else
							{
								throw new Exception("Order " + transaction.OrderCode + ":  item " + itm.ItemCode + " ToBinlocationCode doesn't have associated an Inventory Status");
							}
						}
						else if (!string.IsNullOrEmpty(itm.ToInventoryStatusCode))
						{
							BinLocation currentBin = (selectedWhs?.Details?.Find(bl => bl.InventoryStatusCodes.Contains(itm.ToInventoryStatusCode))) ?? throw new Exception("Order " + transaction.OrderCode + ":  item " + itm.ItemCode + " BinlocationCode not found for ToInventoryStatusCode");
							itm.ToBinLocationCode = currentBin.LocationCode;
						}
						else
						{
							itm.ToBinLocationCode = "";
							itm.ToInventoryStatusCode = "";
						}

						itm.Lots.ForEach(lot =>
						{
							if (lot.Quantity <= 0)
							{
								throw new ArgumentException("Order " + transaction.OrderCode + ": quantity must be greater than zero for item " + itm.ItemCode);
							}

							ReturnMaterialContext foundItm = orderContext.Find(ctx => string.Equals(ctx.ComponentId, itm.ItemCode, StringComparison.OrdinalIgnoreCase) && ctx.Quantity >= lot.Quantity) ?? throw new Exception("Order " + transaction.OrderCode + " doesnt have enough received stock for item " + itm.ItemCode);
						});
					});

					using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
					transaction.Items.ForEach(itm =>
					{
						itm.Lots.ForEach(lot =>
						{
							OrderTransactionProductStatus currentValue = new()
							{
								TransactionId = transaction.TransactionId,
								WarehouseCode = itm.FromWarehouseCode,
								OrderCode = transaction.OrderCode,
								OperationNo = transaction.OperationNo,
								LineId = itm.LineNo,
								BinLocationCode = itm.FromBinLocationCode,
								NewBinLocationCode = itm.ToBinLocationCode,
								ItemId = itm.ItemCode,
								Quantity = lot.Quantity,
								LotNo = lot.LotNo,
								Pallet = lot.Pallet,
								NewInventoryStatusCode = itm.ToInventoryStatusCode,
								NewWarehouseCode = itm.ToWarehouseCode
							};
							ResponseData result = MergeOrderTransactionProductStatus(currentValue, systemOperator, false, false);
						});
					});

					scope.Complete();
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
	/// Gets the product return context.
	/// </summary>
	public List<ReturnMaterialContext> GetProductReturnContext(string workorderId, User systemOperator)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_ORDERPROGRESS_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return _workOrderRepo.GetProductReturnContext(workorderId);
	}
	/// <summary>
	/// Merges the order transaction product status.
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public ResponseData MergeOrderTransactionProductStatus(OrderTransactionProductStatus orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return _orderTransactionProductRepo.MergeOrderTransactionProductStatus(orderTransactionInfo, systemOperator, Validate);
	}
	/// <summary>
	///
	/// </summary>
	public async Task<List<WorkOrderResponse>> ListUpdateWorkOrder(List<WorkOrderExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level, bool isDataSynced = false)
	{
		List<WorkOrderResponse> returnValue = [];
		WorkOrderResponse MessageError;
		MeasureUnit[] units = null;
		List<ProcessType> processTypesList = null;

		if (workOrderList?.Count > 0)
		{
			units = _measureUnitOperation.GetMeasureUnits()?.Where(x => x.IsProductionResult).ToArray();
			processTypesList = _processTypeOperation.GetProcessTypes(string.Empty, systemOperator);
			ProcessTypeSubtype[] subProcessTypes = [.. processTypesList.SelectMany(c => c.SubTypes)];
			int Line = 0;
			string BaseId = string.Empty;
			foreach (WorkOrderExternal workOrder in workOrderList)
			{
				Line++;
				try
				{
					BaseId = workOrder.OrderCode;
					List<ValidationResult> results = [];
					ValidationContext context = new(workOrder, null, null);
					if (!Validator.TryValidateObject(workOrder, context, results))
					{
						throw new Exception($"{results[0]}");
					}
					WorkOrder originalWorkOrder = (await GetWorkOrder(workOrder.OrderCode).ConfigureAwait(false))?.FirstOrDefault();
					bool editMode = originalWorkOrder is not null && !string.IsNullOrEmpty(originalWorkOrder.Id);
					// product
					ProcessEntry currentProduct = null;
					string processEntryId = string.Empty;
					if (
						!string.IsNullOrEmpty(workOrder.ProductCode) &&
						!string.IsNullOrEmpty(workOrder.WarehouseCode) &&
						!string.IsNullOrEmpty(workOrder.Version.ToString()) &&
						!string.IsNullOrEmpty(workOrder.Sequence.ToString())
					)
					{
						Warehouse ObjWarehouse = _warehouseOperation.GetWarehouse(workOrder.WarehouseCode);
						if (!string.IsNullOrEmpty(ObjWarehouse.WarehouseId))
						{
							List<ProcessEntry> ptl = null;
							if (originalWorkOrder is not null)
							{
								//Obtiene producto por Id de producto sin importar almacen o version
								ptl = await _componentOperation.GetProcessEntryById(originalWorkOrder.ProcessEntryId, systemOperator).ConfigureAwait(false);
							}
							else
							{
								// Obtiene producto por Codigo almacen
								ptl = await _componentOperation.GetProcessEntry(workOrder.ProductCode, ObjWarehouse.WarehouseId, workOrder.Version, workOrder.Sequence, systemOperator).ConfigureAwait(false);
							}
							if (ptl is not null)
							{
								currentProduct = ptl.Find(x => x.Status != Status.Failed);
								if (!string.IsNullOrEmpty(currentProduct.Id))
								{
									processEntryId = currentProduct.Id;
								}
							}
						}
					}
					if (string.IsNullOrEmpty(processEntryId))
					{
						throw new Exception(string.Format("Product code [{0}] on warehouse: {1} not found", workOrder.ProductCode, workOrder.WarehouseCode));
					}

					Status orderStatus = Status.Disabled;
					if (!string.IsNullOrEmpty(workOrder.Status))
					{
						orderStatus = workOrder.Status.ToUpperInvariant() switch
						{
							"NEW" => Status.Disabled,
							"IN PROGRESS" => Status.Active,
							"RELEASED" => Status.Pending,
							"QUEUED" => Status.Queued,
							"CANCELLED" => Status.Cancelled,
							"ON HOLD" => Status.Hold,
							"FINISHED" => Status.Finished,
							_ => throw new InvalidOperationException($"Unknown work order status: {workOrder.Status}")
						};
					}
					string salesOrder = null;
					if (!string.IsNullOrEmpty(workOrder.SalesOrder))
					{
						SaleOrder[] SaleOrder = _inventoryOperation.ListSalesOrder(workOrder.SalesOrder, null, null, systemOperator);
						if (SaleOrder?.Length > 0)
						{
							salesOrder = workOrder.SalesOrder;
						}
						else
						{
							throw new Exception(string.Format("Sales order no. {0} does not exist", workOrder.SalesOrder));
						}
					}
					if (editMode && currentProduct is not null && !string.Equals(currentProduct.Code, workOrder.ProductCode, StringComparison.OrdinalIgnoreCase))
					{
						throw new Exception("Changing the product in a Production order is not allowed");
					}
					// workOrder info
					WorkOrder workOrderInfo = new()
					{
						OrderCode = workOrder.OrderCode,
						ProcessEntryId = processEntryId, // Depende de ProductCode, WarehouseCode, Version, Sequence
						PlannedQty = workOrder.Quantity,
						Formula = workOrder.FormulaCode,
						OrderType = workOrder.OrderType,
						LotNo = workOrder.LotNo,
						OrderGroup = workOrder.OrderGroup,
						SalesOrder = salesOrder,
						Comments = workOrder.Comments,
						PlannedStart = workOrder.PlannedStartDate,
						PlannedEnd = workOrder.PlannedEndDate,
						Status = orderStatus,
						Priority = workOrder.OrderPriority.ToStr(),
						DueDate = workOrder.DueDate,
						Processes = []
					};
					if (editMode)
					{
						workOrderInfo = originalWorkOrder;

						workOrderInfo.PlannedQty = workOrder.Quantity;
						workOrderInfo.Formula = workOrder.FormulaCode;
						workOrderInfo.OrderType = workOrder.OrderType;
						workOrderInfo.LotNo = workOrder.LotNo;
						workOrderInfo.OrderGroup = workOrder.OrderGroup;
						workOrderInfo.SalesOrder = workOrder.SalesOrder;
						workOrderInfo.Comments = workOrder.Comments;
						if (!string.IsNullOrEmpty(workOrder.OrderPriority))
						{
							workOrderInfo.Priority = workOrder.OrderPriority;
						}

						if (workOrder.DueDate.Year > 1900 && !originalWorkOrder.APS)
						{
							workOrderInfo.DueDate = workOrder.DueDate;
						}
						if (workOrder.PlannedStartDate.Year > 1900 && !originalWorkOrder.APS)
						{
							workOrderInfo.PlannedStart = workOrder.PlannedStartDate;
						}
						if (workOrder.PlannedEndDate.Year > 1900 && !originalWorkOrder.APS)
						{
							workOrderInfo.PlannedEnd = workOrder.PlannedEndDate;
						}
					}
					if (workOrder.Operations is null || workOrder.Operations.Count == 0)
					{
						throw new Exception("Order requires at least one operation");
					}
					else
					{
						foreach (Common.Models.WorkOrderOperation op in workOrder.Operations)
						{
							if (op.Machines is null || op.Machines.Count == 0)
							{
								op.Machines =
								[
									new WorkOrderMachine
										{
											MachineCode = "00000000-0000-0000-0000-000000000000",
											OperationTimeInSec = 1,
											Primary = "Yes",
											Eficiency = 100
										},
									];
							}

							ProcessTypeSubtype CurrentOperationSubType = subProcessTypes.FirstOrDefault(pt =>
								string.Equals(pt.Code, op.OperationSubtype, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception(string.Format(
									"Operation No. {0} {1} Suboperation Type not found",
									op.Step,
									op.OperationSubtype
								)
							);
							ProcessType CurrentOperationType = processTypesList.Find(pt => string.Equals(pt.Code, CurrentOperationSubType.ProcessTypeId, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception(string.Format("Operation No. {0} {1} Suboperation Type parent not found", op.Step, op.OperationSubtype));
							op.OperationType = CurrentOperationSubType.ProcessTypeId;

							foreach (WorkOrderMachine machine in op.Machines)
							{
								if (machine.MachineCode != "00000000-0000-0000-0000-000000000000" && _machineRepo.ListMachines(machine.MachineCode)?.FirstOrDefault() is null)
								{
									throw new Exception(string.Format("Operation No. {0} Machine: {1} not found", op.Step, machine.MachineCode));
								}

								OrderProcess curProcess = new()
								{
									Step = op.Step.ToInt32(),
									ProcessId = op.Step.ToStr(),
									ProcessTypeId = CurrentOperationSubType.ProcessTypeId,
									OperationName = CurrentOperationSubType.Name,
									ProcessSubTypeId = op.OperationSubtype,
									Total = op.Quantity,
									MachineId = machine.MachineCode,
									LineId = machine.LineNo.ToStr(),
									PlannedEnd = op.PlannedEndDate,
									PlannedStart = op.PlannedStartDate,
									Comments = machine.Comments,
									SetupTime = machine.SetupTimeInSec,
									ExecTime = machine.OperationTimeInSec,
									WaitTime = machine.WaitingTimeInSec,
									IsBackflush = machine.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase)
								};

								if (op.PlannedStartDate.Year <= 1900 && !(editMode && originalWorkOrder.APS))
								{
									throw new Exception(string.Format("Operation No.{0} PlannedStartDate is required", op.Step));
								}
								if (op.PlannedEndDate.Year <= 1900 && !(editMode && originalWorkOrder.APS))
								{
									throw new Exception(string.Format("Operation No.{0} PlannedEndDate is required", op.Step));
								}
								if (op.PlannedEndDate < op.PlannedStartDate && !(editMode && originalWorkOrder.APS))
								{
									throw new Exception(string.Format("Operation No.{0} PlannedEndDate must be greater than PlannedStartDate", op.Step));
								}
								bool addNewProcess = true;
								if (editMode)
								{
									OrderProcess foundProcess = workOrderInfo.Processes.Find(p =>
										p.ProcessId.ToDouble() == curProcess.ProcessId.ToDouble() &&
										p.LineId.ToInt32() == machine.LineNo
									);
									if (foundProcess?.Received == 0)
									{
										curProcess = foundProcess;
										addNewProcess = false;
										curProcess.Total = op.Quantity;
										curProcess.MachineId = machine.MachineCode;
										curProcess.OriginalMachineId = curProcess.MachineId;
										curProcess.PlannedSetupStart = foundProcess.PlannedSetupStart;
										curProcess.PlannedSetupEnd = foundProcess.PlannedSetupEnd;
										curProcess.WaitTime = machine.WaitingTimeInSec;
										curProcess.ExecTime = machine.OperationTimeInSec;
										curProcess.SetupTime = machine.SetupTimeInSec;
										curProcess.IsBackflush = foundProcess.IsBackflush;
										curProcess.IssuedTime = foundProcess.IssuedTime;

										if (!string.IsNullOrEmpty(machine.IssueMode))
										{
											curProcess.IsBackflush = machine.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase);
										}

										if (!string.IsNullOrEmpty(machine.Comments))
										{
											curProcess.Comments = machine.Comments;
										}

										if (!originalWorkOrder.APS)
										{
											curProcess.PlannedEnd = op.PlannedEndDate;
											curProcess.PlannedStart = op.PlannedStartDate;
										}
										else
										{
											curProcess.PlannedEnd = foundProcess.PlannedEnd;
											curProcess.PlannedStart = foundProcess.PlannedStart;
										}
									}
									else if (foundProcess is not null)
									{
										foundProcess = workOrderInfo.Processes.Find(x => x.ProcessId.ToDouble() == curProcess.ProcessId.ToDouble() && x.MachineId == curProcess.MachineId);
										if (foundProcess is not null)
										{
											curProcess = foundProcess;
											addNewProcess = false;
											curProcess.Total = op.Quantity;
											curProcess.PlannedSetupStart = foundProcess.PlannedSetupStart;
											curProcess.PlannedSetupEnd = foundProcess.PlannedSetupEnd;
											curProcess.WaitTime = machine.WaitingTimeInSec;
											curProcess.ExecTime = machine.OperationTimeInSec;
											curProcess.SetupTime = machine.SetupTimeInSec;
											curProcess.IsBackflush = foundProcess.IsBackflush;
											curProcess.IssuedTime = foundProcess.IssuedTime;

											if (!string.IsNullOrEmpty(machine.IssueMode))
											{
												curProcess.IsBackflush = machine.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase);
											}

											if (!originalWorkOrder.APS)
											{
												curProcess.PlannedEnd = op.PlannedEndDate;
												curProcess.PlannedStart = op.PlannedStartDate;
											}
											else
											{
												curProcess.PlannedEnd = foundProcess.PlannedEnd;
												curProcess.PlannedStart = foundProcess.PlannedStart;
											}
										}
									}
								}

								ProcessType processType = null;
								if (string.IsNullOrEmpty(op.OperationType))
								{
									ProcessEntryProcess actualProcess = currentProduct.Processes.Where(prc => prc.ProcessId.ToDouble() == curProcess.ProcessId.ToDouble())?.FirstOrDefault();
									if (processType is not null)
									{
										curProcess.ProcessTypeId = actualProcess.ProcessTypeId;
										_ = processTypesList.Where(pt => pt.Id == op.OperationType)?.FirstOrDefault();
									}
									else
									{
										throw new Exception(string.Format("Order operation No.{0} not found", op.Step));
									}
								}
								else
								{
									processType = processTypesList.Where(pt => pt.Id == op.OperationType)?.FirstOrDefault();
									if (processType is not null)
									{
										curProcess.ProcessTypeId = processType.Id;
									}
									else
									{
										throw new Exception(string.Format("Order operation Type {0} not found", op.OperationType));
									}
								}
								if (op.ByProducts is not null)
								{
									workOrderInfo.Subproducts ??= [];
									foreach (WorkOrderByProduct bp in op.ByProducts)
									{
										if (string.IsNullOrEmpty(bp.WarehouseCode))
										{
											throw new Exception(string.Format("Byproduct {1} in Operation No. {0} Warehouse code is required", op.Step, bp.ItemCode));
										}
										Component opComp = (await _componentOperation.GetComponents(bp.ItemCode, true).ConfigureAwait(false)).Where(c => c.Status != Status.Failed)?.FirstOrDefault();
										Warehouse whs = _warehouseOperation.GetWarehouse(bp.WarehouseCode);
										if (whs is null)
										{
											throw new Exception(string.Format("Byproduct in Operation No. {0} Warehouse code {1} is invalid", op.Step, bp.WarehouseCode));
										}
										if (opComp is not null)
										{
											SubProduct newComp = new()
											{
												ComponentId = bp.ItemCode,
												ProcessId = curProcess.ProcessId,
												Factor = bp.Quantity,
												LineId = bp.LineId.ToStr(),
												LineUID = string.IsNullOrEmpty(bp.LineUID) ? Guid.NewGuid().ToString() : bp.LineUID,
												WarehouseCode = bp.WarehouseCode,
												Comments = bp.Comments
											};
											if (editMode)
											{
												SubProduct foundComponent = workOrderInfo.Subproducts.Find(x => x.ProcessId.ToDouble() == curProcess.ProcessId.ToDouble() && x.ComponentId == newComp.ComponentId && x.LineId == newComp.LineId);
												if (foundComponent is not null)
												{
													newComp = foundComponent;

													newComp.Factor = bp.Quantity;

													newComp.WarehouseCode = bp.WarehouseCode;

													if (!string.IsNullOrEmpty(bp.Comments))
													{
														newComp.Comments = bp.Comments;
													}
												}
											}
											if (!workOrderInfo.Subproducts.Any(x => x.ComponentId == newComp.ComponentId && x.ProcessId == newComp.ProcessId && x.LineId == newComp.LineId))
											{
												workOrderInfo.Subproducts.Add(newComp);
											}
										}
										else
										{
											throw new Exception(string.Format("Operation No. {0} Item code {1} is invalid", op.Step, bp.ItemCode));
										}
									}
								}
								if (op.Items is not null)
								{
									workOrderInfo.Components ??= [];

									foreach (WorkOrderItem itm in op.Items)
									{
										// ProcessEntryComponent productItem = currentProduct.Components.FirstOrDefault(x =>
										// 	x.ProcessId.ToDouble() == curProcess.ProcessId.ToDouble() &&
										// 	x.ComponentId == itm.ItemCode
										// );
										Component opComp = (await _componentOperation.GetComponents(itm.ItemCode, true).ConfigureAwait(false)).Where(x => x.Status != Status.Failed)?.FirstOrDefault();
										if (opComp is not null)
										{
											string UnitCode = currentProduct.UnitId;
											if (!string.IsNullOrEmpty(itm.InventoryUoM))
											{
												MeasureUnit itmUnit = units.FirstOrDefault(x => string.Equals(x.Code, itm.InventoryUoM, StringComparison.OrdinalIgnoreCase));
												if (itmUnit is not null)
												{
													UnitCode = itmUnit.Id;
												}
												else
												{
													throw new Exception(string.Format("Operation Type {0} on Item {1} Inventory UoM code {2} is invalid", op.OperationType, itm.ItemCode, itm.InventoryUoM));
												}
											}
											OrderComponent newComp = new()
											{
												ProcessId = curProcess.ProcessId,
												MaterialType = 1,
												SourceId = itm.ItemCode,
												TargetQty = itm.Quantity,
												InputUnitId = UnitCode,
												TargetUnitId = UnitCode,
												WarehouseCode = itm.WarehouseCode,
												LineId = itm.LineId.ToStr(),
												IsBackflush = itm.IssueMethod.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase),
												Source = itm.Source,
												Comments = itm.Comments
											};

											// if (!editMode && productItem is not null && string.IsNullOrEmpty(itm.Source))
											// {
											// 	newComp.Source = productItem.Source;
											// }

											if (editMode)
											{
												OrderComponent foundComponent = workOrderInfo.Components.Find(x =>
													x.ProcessId.ToDouble() == curProcess.ProcessId.ToDouble() &&
													x.SourceId == newComp.SourceId &&
													x.LineId == newComp.LineId
												);
												if (foundComponent is not null)
												{
													newComp = foundComponent;
													newComp.MaterialType = 1;
													newComp.TargetQty = itm.Quantity;
													newComp.InputUnitId = UnitCode;
													newComp.TargetUnitId = UnitCode;
													newComp.WarehouseCode = itm.WarehouseCode;
													newComp.IsBackflush = itm.IssueMethod.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase);

													if (!string.IsNullOrEmpty(itm.Comments))
													{
														newComp.Comments = itm.Comments;
													}
												}
											}
											if (!workOrderInfo.Components.Any(x => x.SourceId == newComp.SourceId && x.ProcessId == newComp.ProcessId && x.LineId == newComp.LineId))
											{
												workOrderInfo.Components.Add(newComp);
											}
										}
										else
										{
											throw new Exception(string.Format("Operation No. {0} Item code {1} is invalid", op.Step, itm.ItemCode));
										}
									}
								}
								if (op.Tooling is not null)
								{
									workOrderInfo.Tools ??= [];
									foreach (WorkOrderOperationTool tool in op.Tooling)
									{
										WorkOrderTool newTool = new();
										ToolType currentToolType = _toolOperation.ListToolTypes(tool.ToolingCode)?.Find(x => x.Status != Status.Failed);
										ProcessEntryTool productTool = currentProduct.Tools.FirstOrDefault(x =>
											x.ProcessId.ToDouble() == curProcess.ProcessId.ToDouble() &&
											x.ToolId == tool.ToolingCode
										);
										if (currentToolType is not null)
										{
											newTool.ToolId = tool.ToolingCode;
											newTool.ProcessId = curProcess.ProcessId;
											newTool.LineId = tool.LineId.ToStr();
											newTool.Quantity = tool.Quantity;
											newTool.PlannedQty = tool.Quantity;
											newTool.Source = tool.Source;
											newTool.Comments = tool.Comments;
											newTool.Usage = tool.Usage;
											newTool.IsBackflush = tool.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase);

											if (!editMode && productTool is not null)
											{
												if (string.IsNullOrEmpty(tool.Source))
												{
													newTool.Source = productTool.Source;
												}
												if (string.IsNullOrEmpty(tool.Usage))
												{
													newTool.Usage = productTool.Usage;
												}
											}

											WorkOrderTool existingTool = workOrderInfo.Tools.Find(x => x.ProcessId.ToDouble() == newTool.ProcessId.ToDouble() && x.LineId == newTool.LineId);
											if (existingTool is null)
											{
												workOrderInfo.Tools.Add(newTool);
											}
											else
											{
												existingTool.ToolId = tool.ToolingCode;
												existingTool.Quantity = tool.Quantity;
												existingTool.PlannedQty = tool.Quantity;

												if (!string.IsNullOrEmpty(tool.IssueMode))
												{
													existingTool.IsBackflush = tool.IssueMode.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase);
												}

												if (!string.IsNullOrEmpty(tool.Usage) && tool.Usage != "0")
												{
													existingTool.Usage = tool.Usage;
												}
												if (!string.IsNullOrEmpty(tool.Comments))
												{
													existingTool.Comments = tool.Comments;
												}
											}
										}
										else
										{
											throw new Exception(string.Format("Tooling Type: {0} not found", tool.ToolingCode));
										}
									}
								}

								if (op.Labor is not null)
								{
									workOrderInfo.Labor ??= [];
									foreach (WorkOrderOperationLabor labor in op.Labor)
									{
										WorkOrderLabor newLabor = new();
										ProcessEntryLabor productLabor = currentProduct.Labor.FirstOrDefault(x =>
											x.ProcessId.ToDouble() == curProcess.ProcessId.ToDouble() &&
											x.LaborId == labor.ProfileCode
										);
										Labor currentLabor = _laborRepo.ListLabors()?.Find(x => string.Equals(x.Id, labor.ProfileCode, StringComparison.OrdinalIgnoreCase));
										if (currentLabor is not null)
										{
											newLabor.LaborId = labor.ProfileCode;
											newLabor.ProcessId = curProcess.ProcessId;
											newLabor.LineId = labor.LineId.ToStr();
											newLabor.Quantity = labor.Quantity;
											newLabor.PlannedQty = labor.Quantity;
											newLabor.Source = labor.Source;
											newLabor.Comments = labor.Comments;
											newLabor.Usage = labor.Usage;
											newLabor.IsBackflush = labor.IssueMode.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase);

											if (!editMode && productLabor is not null)
											{
												if (string.IsNullOrEmpty(labor.Source))
												{
													newLabor.Source = productLabor.Source;
												}
												if (string.IsNullOrEmpty(labor.Usage))
												{
													newLabor.Usage = productLabor.Usage;
												}
											}

											WorkOrderLabor existingLabor = workOrderInfo.Labor.Find(x =>
												x.ProcessId.ToDouble() == newLabor.ProcessId.ToDouble() &&
												x.LineId == newLabor.LineId);
											if (existingLabor is null)
											{
												workOrderInfo.Labor.Add(newLabor);
											}
											else
											{
												existingLabor.LaborId = newLabor.LaborId;
												existingLabor.Quantity = newLabor.Quantity;
												existingLabor.PlannedQty = newLabor.Quantity;

												if (!string.IsNullOrEmpty(labor.IssueMode))
												{
													existingLabor.IsBackflush = labor.IssueMode.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase);
												}
												if (!string.IsNullOrEmpty(labor.Usage) && labor.Usage != "0")
												{
													existingLabor.Usage = labor.Usage;
												}
												if (!string.IsNullOrEmpty(labor.Comments))
												{
													existingLabor.Comments = labor.Comments;
												}
											}
										}
										else
										{
											throw new Exception(string.Format("Labor profile code: {0} not found", labor.ProfileCode));
										}
									}
								}
								if (op.Tasks is not null)
								{
									List<Activity> tasks = _dataImportOperation.GetDataImportOrderTasks(op, curProcess);
									if (!editMode)
									{
										workOrderInfo.Tasks = tasks;
									}
									else
									{
										foreach (Activity tsk in tasks)
										{
											if (workOrderInfo.Tasks?.Count > 0)
											{
												workOrderInfo.Tasks.Where(x =>
													x.SortId == tsk.SortId &&
													x.TriggerId == tsk.TriggerId)?.ToList()?.ForEach(x =>
														{
															x.ManualDelete = true;
															tsk.ManualDelete = true;
														});
											}
											tsk.ProcessId = curProcess.ProcessId;
											if (!tsk.ManualDelete)
											{
												workOrderInfo.Tasks ??= [];
												workOrderInfo.Tasks.Add(tsk);
											}
										}
									}
								}

								if (addNewProcess)
								{
									workOrderInfo.Processes.Add(curProcess);
								}
								foreach (OrderProcess previousProcess in workOrderInfo.Processes.Where(x => x.ProcessId == curProcess.ProcessId))
								{
									previousProcess.Total = curProcess.Total;
									previousProcess.OriginalMachineId = null;
								}
							}
						}
					}

					if (!editMode || (editMode && !originalWorkOrder.APS))
					{
						if (workOrder.PlannedStartDate.Year > 1900)
						{
							workOrderInfo.PlannedStart = workOrder.PlannedStartDate;
						}
						else
						{
							OrderProcess firstProcess = workOrderInfo.Processes.OrderBy(x => x.PlannedStart).FirstOrDefault();
							if (firstProcess is null)
							{
								workOrderInfo.PlannedStart = workOrder.PlannedStartDate;
							}
							else
							{
								workOrderInfo.PlannedStart = firstProcess.PlannedStart;
							}
						}
						if (workOrder.PlannedEndDate.Year > 1900)
						{
							workOrderInfo.PlannedEnd = workOrder.PlannedEndDate;
						}
						else
						{
							OrderProcess lastProcess = workOrderInfo.Processes.OrderByDescending(x => x.PlannedEnd).FirstOrDefault();
							if (lastProcess is null)
							{
								workOrderInfo.PlannedEnd = workOrder.PlannedEndDate;
							}
							else
							{
								workOrderInfo.PlannedEnd = lastProcess.PlannedEnd;
							}
						}

						if (Math.Abs((workOrderInfo.PlannedStart - workOrderInfo.PlannedEnd).TotalSeconds) < 1e-6)
						{
							workOrderInfo.PlannedStart = new DateTime(
								workOrderInfo.PlannedStart.Year,
								workOrderInfo.PlannedStart.Month,
								workOrderInfo.PlannedStart.Day,
								0,
								0,
								0,
								DateTimeKind.Utc
							);
							workOrderInfo.PlannedEnd = new DateTime(
								workOrderInfo.PlannedStart.Year,
								workOrderInfo.PlannedStart.Month,
								workOrderInfo.PlannedStart.Day,
								23,
								59,
								59,
								DateTimeKind.Utc
							);
						}
					}

					// tasks
					if (currentProduct?.Tasks is not null && !editMode)
					{
						workOrderInfo.Tasks ??= [];
						foreach (Activity t in currentProduct.Tasks)
						{
							OrderProcess existingProcess = workOrderInfo.Processes.Find(x => x.ProcessId.ToDouble() == t.ProcessId.ToDouble());
							if (existingProcess is not null)
							{
								workOrderInfo.Tasks.Add(t);
							}
						}
					}

					returnValue.Add(await MergeWorkOrder(editMode ? ActionDB.Update : ActionDB.Create, workOrderInfo, systemOperator, Validate, Level.ToStr(), true, isDataSynced, IntegrationSource.ERP).ConfigureAwait(false));
				}
				catch (Exception ex)
				{
					MessageError = new WorkOrderResponse
					{
						Id = BaseId,
						Message = ex.Message,
						Code = "Line:" + Line.ToStr()
					};
					returnValue.Add(MessageError);
				}
			}
		}
		// if (!Validate)
		// {
		// 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.WorkOrder, Action = ActionDB.IntegrateAll.ToStr() });
		// 	returnValue = Level switch
		// 	{
		// 		LevelMessage.Warning => [.. returnValue.Where(x => !string.IsNullOrEmpty(x.Message))],
		// 		LevelMessage.Error => [.. returnValue.Where(x => !x.IsSuccess)],
		// 		_ => returnValue
		// 	};
		// }
		return returnValue;
	}

	/// <summary>
	///
	/// </summary>
	/// <summary>
	/// Merges the specified work order with the current work order.
	/// </summary>
	public async Task<WorkOrderResponse> MergeWorkOrder(
		ActionDB mode,
		WorkOrder workOrderInfo,
		User systemOperator,
		bool Validate = false,
		string Level = "Success",
		bool NotifyOnce = true,
		bool IsDataSynced = false,
		IntegrationSource intSrc = IntegrationSource.SF
	)
	{
		WorkOrderResponse returnValue = null;

		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_WORKORDER_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		await ValidateRules(workOrderInfo, systemOperator).ConfigureAwait(false);

		using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
		{
			if (IsDataSynced || await SendOrderDataToERP(workOrderInfo, systemOperator).ConfigureAwait(false))
			{
				if (string.IsNullOrEmpty(workOrderInfo.OrderCode))
				{
					throw new Exception("Invalid Order Code");
				}

				if (mode == ActionDB.Create)
				{
					if (workOrderInfo.Processes is null)
					{
						throw new Exception(ErrorMessage.BadParams);
					}
					LevelMessage objLevel = Enum.Parse<LevelMessage>(Level);
					returnValue = _workOrderRepo.MergeWorkOrder(workOrderInfo, systemOperator, Validate, objLevel, mode, intSrc);
					if (!string.IsNullOrEmpty(returnValue.WorkOrder.Id) && workOrderInfo.Processes?.Count > 0)
					{
						MemoryStream ms;
						returnValue.WorkOrder.Processes.ForEach(static x => { if (string.IsNullOrEmpty(x.LineUID)) { x.LineUID = Guid.NewGuid().ToString(); } });
						string processDetailJSON = JsonConvert.SerializeObject(returnValue.WorkOrder.Processes);
						_workOrderRepo.MergeWorkOrderProcesses(returnValue.WorkOrder, processDetailJSON, systemOperator);

						if (workOrderInfo.Components?.Count > 0)
						{
							returnValue.WorkOrder.Components.ForEach(static x => { if (string.IsNullOrEmpty(x.LineUID)) { x.LineUID = Guid.NewGuid().ToString(); } });
							string componentDetailsJSON = JsonConvert.SerializeObject(workOrderInfo.Components);
							_workOrderRepo.MergeWorkOrderComponents(returnValue.WorkOrder, componentDetailsJSON, systemOperator);

							if (workOrderInfo.ToolValues?.Count > 0)
							{
								XmlSerializer toolValueSerializer = new(typeof(List<ToolValue>));
								ms = new MemoryStream();
								toolValueSerializer.Serialize(ms, workOrderInfo.ToolValues);
								string toolDetailsXML = RemoveXMLHeader(Encoding.UTF8.GetString(ms.ToArray()));
								_workOrderRepo.MergeWorkOrderToolValues(returnValue.WorkOrder, toolDetailsXML, systemOperator);
							}
						}

						if (workOrderInfo.Subproducts?.Count > 0 && !string.IsNullOrEmpty(returnValue.WorkOrder.Id))
						{
							returnValue.WorkOrder.Subproducts.ForEach(static x =>
							{
								if (string.IsNullOrEmpty(x.LineUID))
								{
									x.LineUID = Guid.NewGuid().ToString();
								}
							});
							string byProductJson = JsonConvert.SerializeObject(workOrderInfo.Subproducts);
							_workOrderRepo.MergeWorkOrderByProducts(returnValue.WorkOrder, byProductJson, systemOperator);
						}
						string ToolingJson = string.Empty;
						if (workOrderInfo.Tools?.Count > 0 && !string.IsNullOrEmpty(returnValue.WorkOrder.Id))
						{
							returnValue.WorkOrder.Tools.ForEach(static x => { if (string.IsNullOrEmpty(x.LineUID)) { x.LineUID = Guid.NewGuid().ToString(); } });
							ToolingJson = JsonConvert.SerializeObject(workOrderInfo.Tools);
						}
						_workOrderRepo.MergeWorkOrderTooling(returnValue.WorkOrder, ToolingJson, systemOperator);
						string LaborJson = string.Empty;
						if (workOrderInfo.Labor?.Count > 0 && !string.IsNullOrEmpty(returnValue.WorkOrder.Id))
						{
							returnValue.WorkOrder.Labor.ForEach(static x => { if (string.IsNullOrEmpty(x.LineUID)) { x.LineUID = Guid.NewGuid().ToString(); } });
							LaborJson = JsonConvert.SerializeObject(workOrderInfo.Labor);
						}
						_workOrderRepo.MergeWorkOrderLabor(returnValue.WorkOrder, LaborJson, systemOperator);
						if (!string.IsNullOrEmpty(returnValue.WorkOrder.Id) && returnValue.WorkOrder.Tasks is not null)
						{
							//Order Creation:
							Dictionary<string, string> newTaskIds = [];
							// Get Ids from existing tasks when order is being created (Inherited tasks)

							// For each inheritedTasks clone the activity and fill the newTaskIds dictionary
							foreach (string id in (string[])[.. returnValue.WorkOrder.Tasks.Where(static task => !string.IsNullOrEmpty(task.Id)).Select(static task => task.Id)])
							{
								if (!newTaskIds.ContainsKey(id))
								{
									Activity clonedActivity = await _activityOperation.CloneActivity(new Activity(id), systemOperator, "ORDER").ConfigureAwait(false);
									if (clonedActivity is not null)
									{
										newTaskIds.Add(id, clonedActivity.Id);
									}
								}
							}

							// Save tasks for order
							foreach (Activity task in returnValue.WorkOrder.Tasks)
							{
								// if the task has no id means its new
								if (string.IsNullOrEmpty(task.Id))
								{
									task.Origin = OriginActivity.Order.ToStr();
									Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
									if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
									{
										task.Id = newActivity.Id;
										_ = _activityOperation.AssociateActivityWorkOrder(
											returnValue.WorkOrder.Id,
											newActivity.ProcessId,
											newActivity.AssetId,
											newActivity.Id,
											newActivity.TriggerId,
											newActivity.SortId,
											newActivity.IsMandatory,
											newActivity.RawMaterials,
											systemOperator
										);
									}
								}
								else
								{
									// if the task has Id check vs the dictionary and replace its id
									if (newTaskIds.TryGetValue(task.Id, out string value))
									{
										task.Id = value;
									}
									if (task.ActivityClassId > 0)
									{
										await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
									}
									_ = _activityOperation.AssociateActivityWorkOrder(
										returnValue.WorkOrder.Id,
										task.ProcessId,
										task.AssetId,
										task.Id,
										task.TriggerId,
										task.SortId,
										task.IsMandatory,
										task.RawMaterials,
										systemOperator
									);
								}
							}
						}
					}
					await returnValue.WorkOrder.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);
				}
				else
				{
					List<Activity> rv;
					LevelMessage objLevel = Enum.Parse<LevelMessage>(Level);
					returnValue = _workOrderRepo.MergeWorkOrder(workOrderInfo, systemOperator, Validate, objLevel, mode);
					const bool result = true;
					if (result && !string.IsNullOrEmpty(workOrderInfo.Id) && workOrderInfo.Processes?.Count > 0)
					{
						string processDetailJSON = JsonConvert.SerializeObject(workOrderInfo.Processes);
						_workOrderRepo.MergeWorkOrderProcesses(workOrderInfo, processDetailJSON, systemOperator);

						if (workOrderInfo.Components is not null)
						{
							string componentDetailsJSON = JsonConvert.SerializeObject(workOrderInfo.Components);
							_workOrderRepo.MergeWorkOrderComponents(workOrderInfo, componentDetailsJSON, systemOperator);
						}

						string byProductJson = string.Empty;
						if (workOrderInfo.Subproducts?.Count > 0 && !string.IsNullOrEmpty(workOrderInfo.Id))
						{
							byProductJson = JsonConvert.SerializeObject(workOrderInfo.Subproducts);
						}
						_workOrderRepo.MergeWorkOrderByProducts(workOrderInfo, byProductJson, systemOperator);

						string ToolingJson = string.Empty;
						if (workOrderInfo.Tools?.Count > 0 && !string.IsNullOrEmpty(workOrderInfo.Id))
						{
							ToolingJson = JsonConvert.SerializeObject(workOrderInfo.Tools);
						}
						_workOrderRepo.MergeWorkOrderTooling(workOrderInfo, ToolingJson, systemOperator);

						string LaborJson = string.Empty;
						if (workOrderInfo.Labor?.Count > 0 && !string.IsNullOrEmpty(workOrderInfo.Id))
						{
							LaborJson = JsonConvert.SerializeObject(workOrderInfo.Labor);
						}
						_workOrderRepo.MergeWorkOrderLabor(workOrderInfo, LaborJson, systemOperator);

						if (result && workOrderInfo.Tasks is not null)
						{
							foreach (Activity task in workOrderInfo.Tasks)
							{
								if (string.IsNullOrEmpty(task.Id))
								{
									task.Origin = OriginActivity.Order.ToStr();
									Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
									if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
									{
										_activityOperation.AssociateActivityWorkOrder(
											workOrderInfo.Id,
											newActivity.ProcessId,
											newActivity.AssetId,
											newActivity.Id,
											newActivity.TriggerId,
											newActivity.SortId,
											newActivity.IsMandatory,
											newActivity.RawMaterials,
											systemOperator
										);
									}
								}
								else if (task.ManualDelete)
								{
									bool tempResult = _activityOperation.RemoveActivityWorkOrderAssociation(workOrderInfo.Id, task.ProcessId, task.AssetId, task.Id, systemOperator);
								}
								else
								{
									if (task.ActivityClassId > 0)
									{
										await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
									}
									_activityOperation.AssociateActivityWorkOrder(
										workOrderInfo.Id,
										task.ProcessId,
										task.AssetId,
										task.Id,
										task.TriggerId,
										task.SortId,
										task.IsMandatory,
										task.RawMaterials,
										systemOperator
									);
								}
							}
						}
						WorkOrder wo = (await GetWorkOrder(workOrderInfo.Id).ConfigureAwait(false)).FirstOrDefault();
						if (wo is not null)
						{
							rv = wo.Tasks;
							await wo.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);
						}
						else
						{
							rv = [];
						}
						returnValue.WorkOrder = wo;
						returnValue.WorkOrderActivityList = rv;
					}
				}
			}
			scope.Complete();
		}
		// SyncInitializer.ForcePush(new MessageBroker
		// {
		// 	Type = MessageBrokerType.WorkOrder,
		// 	Aux = mode == ActionDB.Create ? "N" : "U",
		// 	ElementValue = returnValue.WorkOrder.Id,
		// });
		// ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.WorkOrder, Action = ActionDB.IntegrateAll.ToStr() });
		return returnValue;
	}
/// <summary>
	///
	/// </summary>
	public async Task ValidateRules(WorkOrder orderInfo, User SystemOperator)
	{
		string OpcLicenseType = Config.Configuration["OPC-LicenseType"].ToStr();
		if (!string.Equals(OpcLicenseType, "ULTIMATE", StringComparison.OrdinalIgnoreCase))
		{
			orderInfo.Labor ??= [];
			orderInfo.Tools ??= [];

			int duplicated = orderInfo.Labor
				.Where(x => !string.IsNullOrEmpty(x.MachineId))
				.Select(x => new { x.ProcessId, x.MachineId })
				.Concat(orderInfo.Tools
					.Where(x => !string.IsNullOrEmpty(x.MachineId))
					.Select(x => new { x.ProcessId, x.MachineId })
				)
				.GroupBy(x => new { x.MachineId, x.ProcessId })
				.Where(g => g.Count() > 1)
				.Select(y => y.Key)
				.Count();

			if (duplicated > 0)
			{
				throw new Exception("OPCenter license Type does not allow more than one Labor/Tool per Operation");
			}
		}

		Machine[] machines = _deviceOperation.ListDevices(false, true, true);
		ProcessEntry orderEntry = (await _componentOperation.GetProcessEntryById(orderInfo.ProcessEntryId, SystemOperator).ConfigureAwait(false)).Find(x => x.Status != Status.Failed);
		if (orderEntry is not null)
		{
			Warehouse warehouse = _warehouseOperation.ListWarehouse(SystemOperator).Where(w => w.WarehouseId == orderEntry.Warehouse).FirstOrDefault(x => x.Status != Status.Failed);
			if (warehouse is not null)
			{
				int wrongDevices = orderInfo.Processes
					.Where(x => !string.IsNullOrEmpty(x.MachineId) && x.MachineId != "00000000-0000-0000-0000-000000000000")
					.Select(x => new
					{
						x.MachineId,
						Device = machines
						.FirstOrDefault(y => y.Id == x.MachineId && y.FacilityCode == warehouse.FacilityCode)
					})
					.Count(x => x.Device is null);
				if (wrongDevices > 0)
				{
					throw new Exception("One or more machines don't belong to Warehouse's facility");
				}
			}
		}
	}
	private async Task<bool> SendOrderDataToERP(WorkOrder order, User systemOperator)
	{
		bool returnValue = false;
		if (order is not null)
		{
			ProcessEntry product = (await _componentOperation.GetProcessEntryById(order.ProcessEntryId, systemOperator).ConfigureAwait(false)).Find(x => x.Status != Status.Failed);
			string statusName = "New";
			switch (order.Status)
			{
				case Status.Active:
					statusName = "In Progress";
					break;
				case Status.Disabled:
					statusName = "New";
					break;
				case Status.Pending:
					statusName = "Released";
					break;
				case Status.Queued:
					statusName = "Queued";
					break;
				case Status.Cancelled:
					statusName = "Cancelled";
					break;
				case Status.Hold:
					statusName = "On Hold";
					break;
				case Status.Finished:
					statusName = "Finished";
					break;
				case Status.Empty:
				case Status.Deleted:
				case Status.Failed:
				case Status.Execute:
					break;
			}

			WorkOrderExternal extOrder = new()
			{
				OrderCode = order.Id,
				Comments = order.Comments,
				DueDate = order.DueDate,
				FormulaCode = order.Formula,
				InventoryUoM = product?.UnitId,
				OrderGroup = order.OrderGroup,
				OrderType = order.OrderType,
				SalesOrder = order.SalesOrder,
				PlannedEndDate = order.PlannedEnd,
				PlannedStartDate = order.PlannedStart,
				ProductCode = product?.Code,
				Quantity = order.PlannedQty,
				WarehouseCode = product.Warehouse,
				Version = product.Version,
				Sequence = product.Sequence,
				Status = statusName,
				Operations = [],
				LotNo = order.LotNo,
				OrderPriority = order.Priority.ToInt32().ToStr()
			};
			foreach (var itm in order.Processes.GroupBy(g => g.ProcessId, (key, g) => new { OperationNo = key, Process = g.ToArray() }).ToArray())
			{
				Common.Models.WorkOrderOperation op = new();
				OrderProcess prc = itm.Process.First();
				op.OperationName = prc.OperationName;
				op.OperationSubtype = prc.ProcessSubTypeId;
				op.Step = prc.ProcessId.ToDouble();
				op.Quantity = prc.Total;
				op.PlannedStartDate = prc.PlannedStart;
				op.PlannedEndDate = prc.PlannedEnd;
				op.OutputUoM = extOrder.InventoryUoM;
				op.Machines = [];
				op.Labor = [];
				op.Tooling = [];
				op.Items = [];
				op.ByProducts = [];

				order.Labor ??= [];
				order.Components ??= [];
				order.Tools ??= [];
				order.Subproducts ??= [];

				foreach (OrderProcess machine in itm.Process.Where(x => x.MachineId != "00000000-0000-0000-0000-000000000000"))
				{
					ProcessEntryProcess prodProcess = product.Processes.Find(x => x.ProcessId == machine.ProcessId);
					DeviceSpeed ds = null;
					double orderTimes = 1;
					double productMachineTime = -1;
					if (prodProcess is not null)
					{
						try
						{
							ds = prodProcess.AvailableDevices?.Find(x => x.Id == machine.MachineId);
							orderTimes = order.PlannedQty / product.Quantity;
							productMachineTime = ds.ExecTime * orderTimes;
						}
						catch { }
					}
					WorkOrderMachine opMachine = new()
					{
						MachineCode = machine.MachineId,
						Primary = machine.MachineStatus == Status.Active ? "Yes" : "No",
						LineNo = machine.LineId.ToInt32(),
						LineUID = machine.LineUID,
						Labor = [],
						Tooling = [],
						Quantity = (machine.MachineStatus == Status.Active).ToInt32(),
						Comments = machine.Comments
					};
					if (productMachineTime >= 0)
					{
						opMachine.OperationTimeInSec = productMachineTime;
					}
					else
					{
						opMachine.OperationTimeInSec = (machine.PlannedEnd - machine.PlannedStart).TotalSeconds;
					}

					foreach (WorkOrderLabor ml in (WorkOrderLabor[])[.. order.Labor.Where(x => x.MachineId == machine.MachineId && x.ProcessId == machine.ProcessId)])
					{
						ProcessEntryLabor pt = product.Labor?.Find(x => x.ProcessId == ml.ProcessId && x.MachineId == ml.MachineId && x.LaborId == ml.LaborId);
						WorkOrderMachineLabor woMl = new()
						{
							Quantity = ml.PlannedQty,
							LineId = ml.LineId.ToInt32(),
							LineUID = ml.LineUID,
							ProfileCode = ml.LaborId,
							Comments = ml.Comments,
							Usage = ml.Usage
						};

						opMachine.Labor.Add(woMl);
					}
					foreach (WorkOrderTool mt in (WorkOrderTool[])[.. order.Tools.Where(x => x.MachineId == machine.MachineId && x.ProcessId == machine.ProcessId)])
					{
						ProcessEntryTool pt = product.Tools?.Find(x => x.ProcessId == mt.ProcessId && x.MachineId == mt.MachineId && x.ToolId == mt.ToolId);
						WorkOrderMachineTool woMt = new()
						{
							Quantity = mt.PlannedQty,
							LineId = mt.LineId.ToInt32(),
							LineUID = mt.LineUID,
							ToolingCode = mt.ToolId,
							Comments = mt.Comments,
							Usage = mt.Usage
						};

						opMachine.Tooling.Add(woMt);
					}
					op.Machines.Add(opMachine);
				}

				foreach (WorkOrderLabor ml in (WorkOrderLabor[])[.. order.Labor.Where(x => string.IsNullOrEmpty(x.MachineId) && x.ProcessId == prc.ProcessId)])
				{
					ProcessEntryLabor pt = product.Labor?.Find(x => x.ProcessId == ml.ProcessId && x.MachineId == ml.MachineId && x.LaborId == ml.LaborId);
					WorkOrderOperationLabor woOl = new()
					{
						Quantity = ml.PlannedQty,
						LineId = ml.LineId.ToInt32(),
						LineUID = ml.LineUID,
						ProfileCode = ml.LaborId,
						Comments = ml.Comments,
						Usage = ml.Usage
					};

					op.Labor.Add(woOl);
				}
				foreach (WorkOrderTool mt in (WorkOrderTool[])[.. order.Tools.Where(x => string.IsNullOrEmpty(x.MachineId) && x.ProcessId == prc.ProcessId)])
				{
					ProcessEntryTool pt = product.Tools?.Find(x => x.ProcessId == mt.ProcessId && x.MachineId == mt.MachineId && x.ToolId == mt.ToolId);
					WorkOrderOperationTool woOt = new()
					{
						Quantity = mt.PlannedQty,
						LineId = mt.LineId.ToInt32(),
						LineUID = mt.LineUID,
						ToolingCode = mt.ToolId,
						Comments = mt.Comments,
						Usage = mt.Usage
					};

					op.Tooling.Add(woOt);
				}

				foreach (OrderComponent mt in (OrderComponent[])[.. order.Components.Where(x => x.ProcessId == prc.ProcessId)])
				{
					WorkOrderItem itmOrd = new()
					{
						ItemCode = mt.SourceId,
						LineId = mt.LineId.ToInt32(),
						LineUID = mt.LineUID,
						WarehouseCode = mt.WarehouseCode,
						IssueMethod = mt.IsBackflush ? "Backflush" : "Manual",
						InventoryUoM = mt.TargetUnitId,
						Quantity = mt.TargetQty,
						Comments = mt.Comments
					};
					op.Items.Add(itmOrd);
				}

				foreach (SubProduct mt in order.Subproducts.Where(x => x.ProcessId == prc.ProcessId).ToArray())
				{
					WorkOrderByProduct byp = new()
					{
						ItemCode = mt.ComponentId,
						LineId = mt.LineId.ToInt32(),
						LineUID = mt.LineUID,
						WarehouseCode = mt.WarehouseCode,
						Quantity = mt.Factor,
						InventoryUoM = (await _componentOperation.GetComponents(mt.ComponentId).ConfigureAwait(false)).Where(c => c.Status != Status.Failed)?.FirstOrDefault()?.UnitProduction,
						Comments = mt.Comments
					};
					op.ByProducts.Add(byp);
				}
				extOrder.Operations.Add(op);
			}

			double offset = await GetTimezoneOffset("ERP").ConfigureAwait(false);
			AddWorkOrderDatesOffset(extOrder, offset);
			DataSyncHttpResponse resp = null;
			//Need to discuss
			// ProductionOrderService scopedService = (ProductionOrderService)StaticServiceProvider.Provider.GetService(typeof(ProductionOrderService));
			// DataSyncService serviceInfo = await GetBackgroundService(BackgroundServices.PRODUCTION_ORDER_SERVICE, "POST").ConfigureAwait(false);
			// DataSyncHttpResponse resp = await scopedService.ManualExecution(
			// 	serviceInfo,
			// 	TriggerType.SmartFactory,
			// 	ServiceExecOrigin.Event,
			// 	systemOperator,
			// 	string.Empty,
			// 	JsonConvert.SerializeObject(extOrder)
			// ).ConfigureAwait(false);
			//returnValue = resp.StatusCode == HttpStatusCode.OK;
			if (!returnValue)
			{
				//throw new Exception("ERP|" + resp.Message);need to discuss
				throw new Exception("ERP|" );
			}
			else
			{
				try
				{
					if (!string.IsNullOrEmpty(resp.Message))
					{
						JObject o = JObject.Parse(resp.Message);
						JObject msg = JObject.Parse(o["Message"].ToString());
						if (msg.ContainsKey("docEntry"))
						{
							string entry = msg["docEntry"].ToStr();
							if (order.Id != entry && !string.IsNullOrEmpty(entry))
							{
								order.Id = entry;
								order.OrderCode = entry;
							}
						}
						foreach (JObject obj in msg["operations"].Cast<JObject>())
						{
							double OperationNo = 0;
							OrderProcess currentProcess = null;
							if (obj.ContainsKey("operationNo"))
							{
								OperationNo = obj["operationNo"].ToDouble();
								currentProcess = order.Processes.Find(pp => pp.ProcessId.ToDouble() == OperationNo);
							}
							if (currentProcess is not null)
							{
								if (obj.ContainsKey("items"))
								{
									foreach (JObject jItm in obj["items"].Cast<JObject>())
									{
										string itmUid = jItm["lineUID"].ToStr();
										string itmLid = jItm["lineID"].ToStr();
										OrderComponent cmp = order.Components?.Find(i => i.LineUID == itmUid);
										if (cmp is not null)
										{
											cmp.LineId = itmLid.ToInt32().ToStr();
										}
									}
								}

								if (obj.ContainsKey("machines"))
								{
									foreach (JObject jItm in obj["machines"].Cast<JObject>())
									{
										string curUID = jItm["lineUID"].ToStr();
										string curID = jItm["lineID"].ToStr();
										OrderProcess spd = order.Processes.Find(dev => dev.LineUID == curUID);
										if (spd is not null)
										{
											spd.LineId = curID.ToInt32().ToStr();
										}
									}
								}
								if (obj.ContainsKey("byProducts"))
								{
									foreach (JObject jItm in obj["byProducts"].Cast<JObject>())
									{
										string curUID = jItm["lineUID"].ToStr();
										string curID = jItm["lineID"].ToStr();
										SubProduct spd = order.Subproducts?.Find(dev => dev.LineUID == curUID);
										if (spd is not null)
										{
											spd.LineId = curID.ToInt32().ToStr();
										}
									}
								}

								if (obj.ContainsKey("labors"))
								{
									foreach (JObject jItm in obj["labors"].Cast<JObject>())
									{
										string curUID = jItm["lineUID"].ToStr();
										string curID = jItm["lineID"].ToStr();
										WorkOrderLabor woL = order.Labor?.Find(dev =>
											dev.ProcessId.ToDouble() == OperationNo &&
											dev.LineUID == curUID
										);
										if (woL is not null)
										{
											woL.LineId = curID.ToInt32().ToStr();
										}
									}
								}

								if (obj.ContainsKey("tooling"))
								{
									foreach (JObject jItm in obj["tooling"].Cast<JObject>())
									{
										string curUID = jItm["lineUID"].ToStr();
										string curID = jItm["lineID"].ToStr();
										WorkOrderTool elmt = order.Tools?.Find(dev =>
											dev.ProcessId.ToDouble() == OperationNo &&
											dev.LineUID == curUID
										);
										if (elmt is not null)
										{
											elmt.LineId = curID.ToInt32().ToStr();
										}
									}
								}
							}
						}
					}
				}
				catch
				{
				}
			}
		}
		return returnValue;
	}
	/// <summary>
	///
	/// </summary>
	public 
	 void AddWorkOrderDatesOffset(WorkOrderExternal order, double offset)
	{
		if (order is not null)
		{
			if (order.PlannedStartDate.Year > 1900)
			{
				order.PlannedStartDate = order.PlannedStartDate.AddHours(offset);
			}
			if (order.PlannedEndDate.Year > 1900)
			{
				order.PlannedEndDate = order.PlannedEndDate.AddHours(offset);
			}
			if (order.DueDate.Year > 1900)
			{
				order.DueDate = order.DueDate.AddHours(offset);
			}

			order.Operations?.ForEach(op =>
				{
					if (op.PlannedStartDate.Year > 1900)
					{
						op.PlannedStartDate = op.PlannedStartDate.AddHours(offset);
					}
					if (op.PlannedEndDate.Year > 1900)
					{
						op.PlannedEndDate = op.PlannedEndDate.AddHours(offset);
					}
				});
		}
	}

}
