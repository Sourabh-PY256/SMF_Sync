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
using EWP.SF.KafkaSync.DataAccess;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class WorkOrderOperation : IWorkOrderOperation
{
	private readonly IWorkOrderRepo _workOrderRepo;
	private readonly IMeasureUnitOperation _measureUnitOperation;
	private readonly IWarehouseOperation _warehouseOperation;
	private readonly IEmployeeOperation _employeeOperation;
	private readonly IOrderTransactionProductRepo _orderTransactionProductRepo;
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

	private readonly IBinLocationRepo _binLocationRepo;
	private readonly DataSyncServiceManager _dataSyncServiceManager;
	private readonly IOrderTransactionMaterialRepo _orderTransactionMaterialRepo;



	public WorkOrderOperation(IWorkOrderRepo workOrderRepo
	, IMeasureUnitOperation measureUnitOperation, IEmployeeOperation employeeOperation
	, IWarehouseOperation warehouseOperation, IDataSyncServiceOperation dataSyncServiceOperation
	, IOrderTransactionProductRepo orderTransactionProductRepo, IProcessTypeOperation processTypeOperation
	, IComponentOperation componentOperation, IActivityOperation activityOperation
	, IDataImportOperation dataImportOperation, IInventoryOperation inventoryOperation
	, IMachineRepo machineRepo, IToolOperation toolOperation, IDeviceOperation deviceOperation,
	 IBinLocationRepo binLocationRepo, ILaborRepo laborRepo, DataSyncServiceManager dataSyncServiceManager,
	 IOrderTransactionMaterialRepo orderTransactionMaterialRepo)
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
		_binLocationRepo = binLocationRepo;
		_dataSyncServiceManager = dataSyncServiceManager;
		_orderTransactionMaterialRepo = orderTransactionMaterialRepo;
	}
	private static string RemoveXMLHeader(string xml) => xml.Replace("'", "´").Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
	/// <summary>
	///
	/// </summary>
	public WorkOrder GetWorkOrderByCode(string workOrderCode) => _workOrderRepo.GetWorkOrderByCode(workOrderCode);
	/// <summary>
	///
	/// </summary>
	public List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(List<ProductionOrderChangeStatusExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
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
						"Completed" => BMMOrderStatus.Finished,
						"Planned" => BMMOrderStatus.Running,
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
					transaction.OperationNo = process.ProcessId.ToStr();
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
	public async Task<List<WorkOrderResponse>> ListUpdateProductionOrder(
		List<WorkOrderExternal> workOrderList,
		User systemOperator,
		bool Validate,
		LevelMessage Level,
		bool isDataSynced = false,
		string logId = null
	)
	{
		List<WorkOrderResponse> returnValue = [];
		WorkOrderResponse MessageError;
		MeasureUnit[] units = null;
		List<ProcessType> processTypesList = null;
		Dictionary<int, string> ResourceInventory = null;
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
					ProductionOrder originalWorkOrder = (await GetProductionOrder(workOrder.OrderCode).ConfigureAwait(false));
					bool editMode = originalWorkOrder is not null && !string.IsNullOrEmpty(originalWorkOrder.Code);
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
								ptl = await _componentOperation.GetProcessEntryById(originalWorkOrder.ProductId, systemOperator).ConfigureAwait(false);
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
					ProductionOrder workOrderInfo = new()
					{
						Code = workOrder.OrderCode,
						OrderCode = workOrder.OrderCode,
						ProductId = processEntryId, // Depende de ProductCode, WarehouseCode, Version, Sequence
						Quantity = workOrder.Quantity,
						Formula = workOrder.FormulaCode,
						OrderType = workOrder.OrderType,
						LotNo = workOrder.LotNo,
						OrderGroup = workOrder.OrderGroup,
						SalesOrder = salesOrder,
						Comments = workOrder.Comments,
						PlannedStartDate = workOrder.PlannedStartDate,
						PlannedEndDate = workOrder.PlannedEndDate,
						Status = orderStatus,
						Priority = workOrder.OrderPriority.ToStr(),
						DueDate = workOrder.DueDate,
						Operations = []
					};
					if (editMode)
					{
						workOrderInfo = originalWorkOrder;
						workOrderInfo.Quantity = workOrder.Quantity;
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
							workOrderInfo.PlannedStartDate = workOrder.PlannedStartDate;
						}
						if (workOrder.PlannedEndDate.Year > 1900 && !originalWorkOrder.APS)
						{
							workOrderInfo.PlannedEndDate = workOrder.PlannedEndDate;
						}
					}
					if (workOrder.Operations is null || workOrder.Operations.Count == 0)
					{
						throw new Exception("Order requires at least one operation");
					}
					else
					{
						//Validate OperationNo Sequence and Groups
						ValidateOperationSequenceGroups(workOrder.Operations);
						foreach (Common.Models.WorkOrderOperation op in workOrder.Operations)
						{
							if (op.Step > 0)
							{
								ProcessTypeSubtype CurrentOperationSubType = subProcessTypes.FirstOrDefault(pt =>
									string.Equals(pt.Code, op.OperationSubtype, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception(string.Format(
										"Operation No. {0} {1} Suboperation Type not found",
										op.Step,
										op.OperationSubtype
									)
								);
								ProcessType CurrentOperationType = processTypesList.Find(pt => string.Equals(pt.Code, CurrentOperationSubType.ProcessTypeId, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception(string.Format("Operation No. {0} {1} Suboperation Type parent not found", op.Step, op.OperationSubtype));
								op.OperationType = CurrentOperationSubType.ProcessTypeId;
								op.OperationName = CurrentOperationSubType.Name;
							}
							else
							{
								op.OperationType = "Unassigned";
								op.OperationSubtype = "Unassigned";
								op.OperationName = "Unassigned";
							}
							ProductionOrderOperation curProcess = new()
							{
								OperationId = !String.IsNullOrEmpty(op.LineUID) ? op.LineUID : Guid.CreateVersion7().ToString(),
								OperationNo = op.Step.ToInt32(),
								OperationCode = !String.IsNullOrEmpty(op.OperationCode) ? op.OperationCode : op.Step.ToStr(),
								OperationGroup = op.OperationGroup,
								OperationTypeCode = op.OperationType,
								Name = op.OperationName,
								OperationSubTypeCode = op.OperationSubtype,
								Quantity = op.Quantity,
								LineId = op.LineId,
								PlannedEndDate = op.PlannedEndDate,
								PlannedStartDate = op.PlannedStartDate,
								/*
								Comments = machine.Comments,
								SetupTime = machine.SetupTimeInSec,
								ExecTime = machine.OperationTimeInSec,
								WaitTime = machine.WaitingTimeInSec,
								IsBackflush = machine.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase)
								*/
							};

							bool addOperation = true;

							// Find current operation or create new
							ProductionOrderOperation foundOp = workOrderInfo.Operations.FirstOrDefault(p =>
								(!String.IsNullOrEmpty(op.LineUID) && p.OperationId == op.LineUID)
								|| (p.LineId.ToInt32() == op.LineId.ToInt32())
							);

							if (foundOp is not null)
							{
								addOperation = false;
								curProcess = foundOp;
								curProcess.OperationNo = op.Step.ToInt32();
								curProcess.OperationCode = !String.IsNullOrEmpty(op.OperationCode) ? op.OperationCode : op.Step.ToStr();
								curProcess.OperationGroup = op.OperationGroup;
								if (!string.IsNullOrEmpty(op.LineUID))
								{
									curProcess.LineId = op.LineId;
								}
							}

							// Si no tiene bandera APS se puede modificar la fecha de Planeacion
							if (originalWorkOrder?.APS == false)
							{
								curProcess.PlannedEndDate = op.PlannedEndDate;
								curProcess.PlannedStartDate = op.PlannedStartDate;
							}

							// Si Maquinas viene vacio se inicializa con un default
							if (op.Machines is null || op.Machines.Count == 0)
							{
								op.Machines =
								[
									new WorkOrderMachine
										{
											MachineCode = "00000000-0000-0000-0000-000000000000",
											OperationTimeInSec = 1,
											LineNo = -1,
											LineUID = Guid.CreateVersion7().ToString(),
											Primary = "Yes",
											Eficiency = 100
										},
									];
							}

							ProcessType processType = null;
							if (string.IsNullOrEmpty(op.OperationType))
							{
								ProcessEntryProcess actualProcess = currentProduct.Processes.FirstOrDefault(prc => prc.ProcessId.ToDouble() == curProcess.OperationNo.ToDouble());
								if (processType is not null)
								{
									curProcess.OperationTypeCode = actualProcess.ProcessTypeId;
									_ = processTypesList.Where(pt => pt.Id == op.OperationType)?.FirstOrDefault();
								}
								else
								{
									throw new Exception(string.Format("Order operation No.{0} not found", op.Step));
								}
							}
							else
							{
								processType = processTypesList.FirstOrDefault(pt => pt.Id == op.OperationType);
								if (processType is not null)
								{
									curProcess.OperationTypeCode = processType.Id;
								}
								else
								{
									throw new Exception(string.Format("Order operation Type {0} not found", op.OperationType));
								}
							}
							if (op.ByProducts is not null)
							{
								curProcess.Byproducts ??= [];
								foreach (WorkOrderByProduct bp in op.ByProducts)
								{
									if (string.IsNullOrEmpty(bp.WarehouseCode))
									{
										throw new Exception(string.Format("Byproduct {1} in Operation No. {0} Warehouse code is required", op.Step, bp.ItemCode));
									}
									Component opComp = (await _componentOperation.GetComponents(bp.ItemCode, true).ConfigureAwait(false)).Where(c => c.Status != Status.Failed)?.FirstOrDefault();
									Warehouse whs = _warehouseOperation.GetWarehouse(bp.WarehouseCode) ?? throw new Exception(string.Format("Byproduct in Operation No. {0} Warehouse code {1} is invalid", op.Step, bp.WarehouseCode));
									if (opComp is not null)
									{
										bool addByProduct = true;
										ProductionOrderByProduct newComp = new()
										{
											ItemCode = bp.ItemCode,
											Quantity = bp.Quantity,
											LineId = bp.LineId.ToStr(),
											LineUID = string.IsNullOrEmpty(bp.LineUID) ? Guid.CreateVersion7().ToString() : bp.LineUID,
											WarehouseCode = bp.WarehouseCode,
											Comments = bp.Comments
										};
										if (editMode)
										{
											ProductionOrderByProduct foundByp = curProcess.Byproducts.Find(x => (!String.IsNullOrEmpty(bp.LineUID) && x.LineUID == bp.LineUID) || (String.IsNullOrEmpty(bp.LineUID) && x.LineId == bp.LineId.ToStr()));
											if (foundByp is not null)
											{
												addByProduct = false;
												newComp = foundByp;
												newComp.ItemCode = bp.ItemCode;
												newComp.Quantity = bp.Quantity;
												newComp.WarehouseCode = bp.WarehouseCode;
												if (!String.IsNullOrEmpty(bp.LineUID))
												{
													newComp.LineId = bp.LineId.ToStr();
												}
											}
											if (!string.IsNullOrEmpty(bp.Comments))
											{
												newComp.Comments = bp.Comments;
											}
										}
										if (addByProduct)
										{
											if (!String.IsNullOrEmpty(bp.LineUID))
											{
												newComp.LineId = bp.LineId.ToStr();
											}
											else
											{
												// Revisar inventariado para eliminar LineNo de otra seccion si existiera
												RemoveProductionOrderResourceByLineId(workOrderInfo, bp.LineId.ToStr());
											}
											curProcess.Byproducts.Add(newComp);
										}
									}
									else
									{
										throw new Exception(string.Format("Operation No. {0} Item code {1} is invalid", op.Step, bp.ItemCode));
									}
								}
							}
							foreach (WorkOrderMachine machine in op.Machines)
							{
								if (machine.MachineCode != "00000000-0000-0000-0000-000000000000" && _machineRepo.ListMachines(machine.MachineCode)?.FirstOrDefault() is null)
								{
									throw new Exception(string.Format("Operation No. {0} Machine: {1} not found", op.Step, machine.MachineCode));
								}
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

								ProductionOrderMachine currentMachine = new()
								{
									MachineCode = machine.MachineCode,
									LineId = machine.LineNo.ToStr(),
									LineUID = !String.IsNullOrEmpty(machine.LineUID) ? machine.LineUID : Guid.CreateVersion7().ToString(),
									Received = 0,
									Rejected = 0,
									Consumption = machine.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase) ? 1 : 0,
									ActualExecTime = 0,
									WaitTime = machine.WaitingTimeInSec,
									PlannedSetupTime = machine.SetupTimeInSec,
								};

								if (editMode)
								{
									bool addMachine = true;
									ProductionOrderMachine foundMachine = curProcess.Machines.FirstOrDefault(m =>
										(!String.IsNullOrEmpty(machine.LineUID) && m.LineUID == machine.LineUID)
										|| (String.IsNullOrEmpty(machine.LineUID) && m.LineId.ToInt32() == machine.LineNo.ToInt32())
									);
									if (foundMachine is not null)
									{
										currentMachine = foundMachine;
										currentMachine.MachineCode = machine.MachineCode;
										addMachine = false;

										if (!String.IsNullOrEmpty(machine.LineUID))
										{
											currentMachine.LineId = machine.LineNo.ToStr();
										}
									}
									if (!string.IsNullOrEmpty(machine.IssueMode))
									{
										currentMachine.Consumption = machine.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
									}

									if (!string.IsNullOrEmpty(machine.Comments))
									{
										curProcess.Comments = machine.Comments;
									}

									if (addMachine)
									{
										if (!String.IsNullOrEmpty(machine.LineUID))
										{
											currentMachine.LineId = machine.LineNo.ToStr();
										}
										else
										{
											// Revisar inventariado para eliminar LineNo de otra seccion si existiera
											RemoveProductionOrderResourceByLineId(workOrderInfo, machine.LineNo.ToStr());
										}
										curProcess.Machines.Add(currentMachine);
									}
								}
							}
							if (op.Items is not null)
							{
								curProcess.Items ??= [];

								foreach (WorkOrderItem itm in op.Items)
								{
									ProcessEntryComponent productItem = currentProduct.Components?.FirstOrDefault(x =>
										x.ProcessId.ToDouble() == curProcess.OperationNo.ToDouble() &&
										x.ComponentId == itm.ItemCode
									);
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
										bool AddItem = true;
										ProductionOrderItem newComp = new()
										{
											Class = 1,
											ItemCode = itm.ItemCode,
											Quantity = itm.Quantity,
											IssuedQty = 0,
											UnitCode = UnitCode,
											WarehouseCode = itm.WarehouseCode,
											LineId = itm.LineId.ToStr(),
											LineUID = !String.IsNullOrEmpty(itm.LineUID) ? itm.LineUID : Guid.CreateVersion7().ToString(),
											Consumption = itm.IssueMethod.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase) ? 1 : 0,
											Source = itm.Source,
											Comments = itm.Comments
										};

										if (!editMode && productItem is not null && string.IsNullOrEmpty(itm.Source))
										{
											newComp.ItemCode = productItem.Source;
										}

										if (editMode)
										{
											ProductionOrderItem foundComponent = curProcess.Items.Find(x =>
												(!String.IsNullOrEmpty(itm.LineUID) && x.LineUID == itm.LineUID) ||
												(String.IsNullOrEmpty(itm.LineUID) && x.LineId == itm.LineId.ToStr())
											);
											if (foundComponent is not null)
											{
												AddItem = false;
												newComp = foundComponent;

												if (!String.IsNullOrEmpty(itm.ItemCode))
												{
													newComp.ItemCode = itm.ItemCode;
												}
												if (!String.IsNullOrEmpty(itm.LineUID))
												{
													newComp.LineId = itm.LineId.ToStr();
												}
												newComp.Quantity = itm.Quantity;
												newComp.UnitCode = UnitCode;

												newComp.WarehouseCode = itm.WarehouseCode;
												newComp.Consumption = itm.IssueMethod.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
												if (!string.IsNullOrEmpty(itm.Comments))
												{
													newComp.Comments = itm.Comments;
												}
											}
										}
										if (AddItem)
										{
											if (!String.IsNullOrEmpty(itm.LineUID))
											{
												newComp.LineId = itm.LineId.ToStr();
											}
											else
											{
												// Revisar inventariado para eliminar LineNo de otra seccion si existiera
												RemoveProductionOrderResourceByLineId(workOrderInfo, itm.LineId.ToStr());
											}
											curProcess.Items.Add(newComp);
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
								curProcess.ToolingType ??= [];
								foreach (WorkOrderOperationTool tool in op.Tooling)
								{
									bool addToolingType = true;
									ProductionOrderResource newTool = new();
									ToolType currentToolType = _toolOperation.ListToolTypes(tool.ToolingCode)?.Find(x => x.Status != Status.Failed);
									ProcessEntryTool productTool = currentProduct.Tools?.FirstOrDefault(x =>
										x.ProcessId.ToDouble() == curProcess.OperationNo.ToDouble() &&
										x.ToolId == tool.ToolingCode
									);
									if (currentToolType is not null)
									{
										newTool.Code = tool.ToolingCode;
										newTool.LineId = tool.LineId.ToStr();
										newTool.LineUID = string.IsNullOrEmpty(tool.LineUID) ? Guid.CreateVersion7().ToString() : tool.LineUID;
										newTool.Quantity = tool.Quantity;
										newTool.PlannedQty = tool.Quantity;
										newTool.Source = tool.Source;
										newTool.Comments = tool.Comments;
										newTool.Usage = tool.Usage;
										newTool.Consumption = tool.IssueMode.ToStr().Equals("BACKFLUSH", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

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

										ProductionOrderResource existingTool = curProcess.ToolingType.Find(x =>
												(!String.IsNullOrEmpty(tool.LineUID) && x.LineUID == tool.LineUID) ||
												(String.IsNullOrEmpty(tool.LineUID) && x.LineId == tool.LineId.ToStr()));
										if (existingTool is not null)
										{
											addToolingType = false;
											newTool = existingTool;
											newTool.Quantity = tool.Quantity;
											newTool.PlannedQty = tool.Quantity;
											if (!String.IsNullOrEmpty(tool.ToolingCode))
											{
												newTool.Code = tool.ToolingCode;
											}
											if (!string.IsNullOrEmpty(tool.IssueMode))
											{
												newTool.Consumption = tool.IssueMode.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
											}

											if (!string.IsNullOrEmpty(tool.Usage) && tool.Usage != "0")
											{
												newTool.Usage = tool.Usage;
											}
											if (!string.IsNullOrEmpty(tool.Comments))
											{
												newTool.Comments = tool.Comments;
											}
											if (!String.IsNullOrEmpty(tool.LineUID))
											{
												newTool.LineId = tool.LineId.ToStr();
											}
										}
										if (addToolingType)
										{
											if (!String.IsNullOrEmpty(tool.LineUID))
											{
												newTool.LineId = tool.LineId.ToStr();
											}
											else
											{
												// Revisar inventariado para eliminar LineNo de otra seccion si existiera
												RemoveProductionOrderResourceByLineId(workOrderInfo, tool.LineId.ToStr());
											}
											curProcess.ToolingType.Add(newTool);
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
								curProcess.Labor ??= [];
								foreach (WorkOrderOperationLabor labor in op.Labor)
								{
									bool AddLabor = true;
									ProductionOrderResource newLabor = new();
									ProcessEntryLabor productLabor = currentProduct.Labor?.FirstOrDefault(x =>
										x.ProcessId.ToDouble() == curProcess.OperationNo.ToDouble() &&
										x.LaborId == labor.ProfileCode
									);
									Labor currentLabor = _laborRepo.ListLabors()?.Find(x => string.Equals(x.Id, labor.ProfileCode, StringComparison.OrdinalIgnoreCase));
									if (currentLabor is not null)
									{
										newLabor.Code = labor.ProfileCode;
										newLabor.LineId = labor.LineId.ToStr();
										newLabor.LineUID = string.IsNullOrEmpty(labor.LineUID) ? Guid.CreateVersion7().ToString() : labor.LineUID;
										newLabor.Quantity = labor.Quantity;
										newLabor.PlannedQty = labor.Quantity;
										newLabor.Source = labor.Source;
										newLabor.Comments = labor.Comments;
										newLabor.Usage = labor.Usage;
										newLabor.Consumption = labor.IssueMode.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

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

										ProductionOrderResource existingLabor = curProcess.Labor.Find(x =>
										(!String.IsNullOrEmpty(labor.LineUID) && x.LineUID == labor.LineUID) ||
										(String.IsNullOrEmpty(labor.LineUID) && x.LineId == labor.LineId.ToStr()));
										if (existingLabor is not null)
										{
											AddLabor = false;
											newLabor = existingLabor;
											newLabor.Quantity = newLabor.Quantity;
											newLabor.PlannedQty = newLabor.Quantity;
											if (!String.IsNullOrEmpty(labor.ProfileCode))
											{
												newLabor.Code = labor.ProfileCode;
											}
											if (!string.IsNullOrEmpty(labor.IssueMode))
											{
												newLabor.Consumption = labor.IssueMode.ToStr().ToLowerInvariant().Contains("backflush", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
											}
											if (!string.IsNullOrEmpty(labor.Usage) && labor.Usage != "0")
											{
												newLabor.Usage = labor.Usage;
											}
											if (!string.IsNullOrEmpty(labor.Comments))
											{
												newLabor.Comments = labor.Comments;
											}
											if (!String.IsNullOrEmpty(labor.LineUID))
											{
												newLabor.LineId = labor.LineId.ToStr();
											}
										}

										if (AddLabor)
										{
											if (!String.IsNullOrEmpty(labor.LineUID))
											{
												newLabor.LineId = labor.LineId.ToStr();
											}
											else
											{
												// Revisar inventariado para eliminar LineNo de otra seccion si existiera
												RemoveProductionOrderResourceByLineId(workOrderInfo, labor.LineId.ToStr());
											}
											curProcess.Labor.Add(newLabor);
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
								List<Activity> tasks = _dataImportOperation.GetDataImportProductionOrderTasks(op, curProcess);
								if (!editMode)
								{
									curProcess.Tasks = tasks;
								}
								else
								{
									foreach (Activity tsk in tasks)
									{
										if (curProcess.Tasks?.Count > 0)
										{
											curProcess.Tasks.Where(x =>
												x.SortId == tsk.SortId &&
												x.TriggerId == tsk.TriggerId)?.ToList()?.ForEach(x =>
													{
														x.ManualDelete = true;
														tsk.ManualDelete = true;
													});
										}
										tsk.ProcessId = curProcess.OperationNo.ToStr();
										if (!tsk.ManualDelete)
										{
											curProcess.Tasks ??= [];
											curProcess.Tasks.Add(tsk);
										}
									}
								}
							}
							if (addOperation)
							{
								workOrderInfo.Operations.Add(curProcess);
							}
						}
					}

					if (!editMode || (editMode && !originalWorkOrder.APS))
					{
						if (workOrder.PlannedStartDate.Year > 1900)
						{
							workOrderInfo.PlannedStartDate = workOrder.PlannedStartDate;
						}
						else
						{
							ProductionOrderOperation firstProcess = workOrderInfo.Operations.OrderBy(x => x.PlannedStartDate).FirstOrDefault();
							if (firstProcess is null)
							{
								workOrderInfo.PlannedStartDate = workOrder.PlannedStartDate;
							}
							else
							{
								workOrderInfo.PlannedStartDate = firstProcess.PlannedStartDate;
							}
						}
						if (workOrder.PlannedEndDate.Year > 1900)
						{
							workOrderInfo.PlannedEndDate = workOrder.PlannedEndDate;
						}
						else
						{
							ProductionOrderOperation lastProcess = workOrderInfo.Operations.OrderByDescending(x => x.PlannedEndDate).FirstOrDefault();
							if (lastProcess is null)
							{
								workOrderInfo.PlannedEndDate = workOrder.PlannedEndDate;
							}
							else
							{
								workOrderInfo.PlannedEndDate = lastProcess.PlannedEndDate;
							}
						}

						if (Math.Abs((workOrderInfo.PlannedStartDate - workOrderInfo.PlannedEndDate).TotalSeconds) < 1e-6)
						{
							workOrderInfo.PlannedStartDate = new DateTime(
								workOrderInfo.PlannedStartDate.Year,
								workOrderInfo.PlannedStartDate.Month,
								workOrderInfo.PlannedStartDate.Day,
								0,
								0,
								0,
								DateTimeKind.Utc
							);
							workOrderInfo.PlannedEndDate = new DateTime(
								workOrderInfo.PlannedStartDate.Year,
								workOrderInfo.PlannedStartDate.Month,
								workOrderInfo.PlannedStartDate.Day,
								23,
								59,
								59,
								DateTimeKind.Utc
							);
						}
					}

					// Missing tasks from product
					if (currentProduct?.Tasks is not null && !editMode)
					{
						foreach (Activity t in currentProduct.Tasks)
						{
							ProductionOrderOperation existingProcess = workOrderInfo.Operations.Find(x => x.OperationNo.ToDouble() == t.ProcessId.ToDouble());

							if (existingProcess is not null)
							{
								existingProcess.Tasks ??= [];
								existingProcess.Tasks.Add(t);
							}
						}
					}

					returnValue.Add(await MergeProductionOrder(editMode ? ActionDB.Update : ActionDB.Create, workOrderInfo, systemOperator, Validate, Level.ToStr(), true, isDataSynced, IntegrationSource.ERP).ConfigureAwait(false));
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
	// <summary>
	/// Merges the specified work order with the current work order.
	/// </summary>
	public async Task<WorkOrderResponse> MergeProductionOrder(
		ActionDB mode,
		ProductionOrder workOrderInfo,
		User systemOperator,
		bool Validate = false,
		string Level = "Success",
		bool NotifyOnce = true,
		bool IsDataSynced = false,
		IntegrationSource intSrc = IntegrationSource.SF
	)
	{
		WorkOrderResponse returnValue = null;
		workOrderInfo.OrderCode = workOrderInfo.Code;
		await ValidateRules(workOrderInfo, systemOperator).ConfigureAwait(false);

		using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
		{
			if (IsDataSynced || await SendOrderDataToERP(workOrderInfo, systemOperator).ConfigureAwait(false))
			{
				if (string.IsNullOrEmpty(workOrderInfo.Code))
				{
					throw new Exception("Invalid Order Code");
				}

				if (mode == ActionDB.Create)
				{
					if (workOrderInfo.Operations is null)
					{
						throw new Exception(ErrorMessage.BadParams);
					}
					LevelMessage objLevel = Enum.Parse<LevelMessage>(Level);
					returnValue = _workOrderRepo.MergeProductionOrder(workOrderInfo, systemOperator, Validate, objLevel, mode, intSrc);
					if (!string.IsNullOrEmpty(returnValue.ProductionOrder.Code) && workOrderInfo.Operations?.Count > 0)
					{
						returnValue.ProductionOrder.Operations.ForEach(static op =>
						{
							op.Machines?.ForEach(machine =>
							{
								if (string.IsNullOrEmpty(machine.LineUID))
								{
									machine.LineUID = Guid.CreateVersion7().ToString();
								}
							});
						});
						string processDetailJSON = JsonConvert.SerializeObject(returnValue.ProductionOrder.Operations);
						_workOrderRepo.MergeProductionOrderOperations(returnValue.ProductionOrder, processDetailJSON, systemOperator);

						if (workOrderInfo.Operations?.Any(op => op.Items?.Count > 0) == true)
						{
							returnValue.ProductionOrder.Operations?
							.ForEach(op =>
							{
								op.Items?.ForEach(static x =>
								{
									if (string.IsNullOrEmpty(x.LineUID))
									{
										x.LineUID = Guid.CreateVersion7().ToString();
									}
								});
							});
							string componentDetailsJSON = JsonConvert.SerializeObject(
							returnValue.ProductionOrder.Operations?
								.SelectMany(op => (op.Items ?? Enumerable.Empty<ProductionOrderItem>())
									.Select(item => new
									{
										op.OperationId,
										op.OperationNo, // o op.ProcessId si es lo que identifica la operación
										Item = item
									})
								)
						);

							_workOrderRepo.MergeProductionOrderComponents(returnValue.ProductionOrder, componentDetailsJSON, systemOperator);
						}
						if (workOrderInfo.Operations?.Any(op => op.Byproducts?.Count > 0) == true)
						{
							returnValue.ProductionOrder.Operations?
							.ForEach(op =>
							{
								op.Byproducts?.ForEach(static x =>
								{
									if (string.IsNullOrEmpty(x.LineUID))
									{
										x.LineUID = Guid.CreateVersion7().ToString();
									}
								});
							});

							string byProductJson = JsonConvert.SerializeObject(
							returnValue.ProductionOrder.Operations?
								.SelectMany(op => (op.Byproducts ?? Enumerable.Empty<ProductionOrderByProduct>())
									.Select(bp => new
									{
										op.OperationId,
										op.OperationNo, // o ProcessId si ese es el identificador
										ByProduct = bp
									})
								)
							);

							_workOrderRepo.MergeProductionOrderByProducts(returnValue.ProductionOrder, byProductJson, systemOperator);
						}
						string ToolingJson = string.Empty;
						if (workOrderInfo.Operations?.Any(op =>
							(op.ToolingType?.Count > 0) ||
							(op.Machines?.Any(m => m.ToolingType?.Count > 0) == true)
						) == true && !string.IsNullOrEmpty(returnValue.ProductionOrder.Code))
						{
							returnValue.ProductionOrder.Operations?
							.ForEach(op =>
							{
								op.ToolingType?.ForEach(static x =>
								{
									if (string.IsNullOrEmpty(x.LineUID))
									{
										x.LineUID = Guid.CreateVersion7().ToString();
									}
								});
								op.Machines?.ForEach(m =>
								{
									m.ToolingType?.ForEach(static x =>
									{
										if (string.IsNullOrEmpty(x.LineUID))
										{
											x.LineUID = Guid.CreateVersion7().ToString();
										}
									});
								});
							});

							IEnumerable<object> allToolingTypesWithOp = workOrderInfo.Operations?
								.SelectMany(op =>
									// ToolingType a nivel operación
									(op.ToolingType ?? Enumerable.Empty<ProductionOrderResource>())
										.Select(t => new
										{
											op.OperationId,
											op.OperationNo,
											MachineCode = string.Empty,
											Tooling = t
										}
										)
										// ToolingType dentro de cada máquina (preservando m)
										.Concat(
											op.Machines?
												.SelectMany(m => (m.ToolingType ?? Enumerable.Empty<ProductionOrderResource>())
													.Select(t => new
													{
														op.OperationNo,
														MachineCode = m.MachineCode ?? string.Empty,
														MachineId = m.LineUID ?? string.Empty,
														Tooling = t
													})
												) ?? Enumerable.Empty<object>()
										)
								);

							ToolingJson = JsonConvert.SerializeObject(allToolingTypesWithOp);
						}
						_workOrderRepo.MergeProductionOrderToolingType(returnValue.ProductionOrder, ToolingJson, systemOperator);

						string LaborJson = string.Empty;
						if (workOrderInfo.Operations?.Any(op =>
							(op.Labor?.Count > 0) ||
							(op.Machines?.Any(m => m.Labor?.Count > 0) == true)
						) == true && !string.IsNullOrEmpty(returnValue.ProductionOrder.Code))
						{
							returnValue.ProductionOrder.Operations?
							.ForEach(op =>
							{
								op.Labor?.ForEach(static x =>
								{
									if (string.IsNullOrEmpty(x.LineUID))
									{
										x.LineUID = Guid.CreateVersion7().ToString();
									}
								});
								op.Machines?.ForEach(m =>
								{
									m.Labor?.ForEach(static x =>
									{
										if (string.IsNullOrEmpty(x.LineUID))
										{
											x.LineUID = Guid.CreateVersion7().ToString();
										}
									});
								});
							});

							IEnumerable<object> allLabor = workOrderInfo.Operations?
							.SelectMany(op =>
								// ToolingType a nivel operación
								(op.Labor ?? Enumerable.Empty<ProductionOrderResource>())
									.Select(t => new
									{
										op.OperationId,
										op.OperationNo,
										MachineCode = string.Empty,
										Labor = t
									})
								// ToolingType dentro de cada máquina (preservando m)
								.Concat(
									op.Machines?
										.SelectMany(m => (m.Labor ?? Enumerable.Empty<ProductionOrderResource>())
											.Select(t => new
											{
												op.OperationId,
												op.OperationNo,
												MachineCode = m.MachineCode ?? string.Empty,
												MachineId = m.LineUID ?? string.Empty,
												Labor = t
											})
										) ?? Enumerable.Empty<object>()
								)
							);

							LaborJson = JsonConvert.SerializeObject(allLabor);
						}
						_workOrderRepo.MergeProductionOrderLabor(returnValue.ProductionOrder, LaborJson, systemOperator);

						//Task Section
						if (!string.IsNullOrEmpty(returnValue.ProductionOrder.Code) && workOrderInfo.Operations?.Any(op => op.Tasks?.Count > 0) == true)
						{
							//Order Creation:
							Dictionary<string, string> newTaskIds = [];
							// Get Ids from existing tasks when order is being created (Inherited tasks)
							// For each inheritedTasks clone the activity and fill the newTaskIds dictionary
							foreach (ProductionOrderOperation op in returnValue.ProductionOrder.Operations)
							{
								foreach (Activity task in op.Tasks)
								{
									if (!string.IsNullOrEmpty(task.Id) && !newTaskIds.ContainsKey(task.Id))
									{
										Activity clonedActivity = await _activityOperation.CloneActivity(new Activity(task.Id), systemOperator, "ORDER").ConfigureAwait(false);
										newTaskIds.Add(task.Id, clonedActivity.Id);
										task.Id = clonedActivity.Id;
										if (task.ActivityClassId > 0)
										{
											await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
										}
										_activityOperation.AssociateActivityWorkOrder(
											returnValue.ProductionOrder.Code,
											op.OperationNo.ToStr(),
											task.AssetId,
											task.Id,
											task.TriggerId,
											task.SortId,
											task.IsMandatory,
											task.RawMaterials,
											systemOperator
										);
									}
									else
									{
										task.Origin = OriginActivity.Order.ToStr();
										Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
										if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
										{
											task.Id = newActivity.Id;
											_activityOperation.AssociateActivityWorkOrder(
												returnValue.ProductionOrder.Code,
												op.OperationNo.ToStr(),
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
								}
								;
							}
							;
						}
					}
					await returnValue.ProductionOrder.Log(EntityLogType.Create, systemOperator).ConfigureAwait(false);
				}
				else
				{
					List<Activity> rv;
					LevelMessage objLevel = Enum.Parse<LevelMessage>(Level);
					returnValue = _workOrderRepo.MergeProductionOrder(workOrderInfo, systemOperator, Validate, objLevel, mode);
					const bool result = true;
					if (result && !string.IsNullOrEmpty(workOrderInfo.Code) && workOrderInfo.Operations?.Count > 0)
					{
						string processDetailJSON = JsonConvert.SerializeObject(workOrderInfo.Operations);
						_workOrderRepo.MergeProductionOrderOperations(workOrderInfo, processDetailJSON, systemOperator);

						if (workOrderInfo.Operations?.Any(op => op.Items?.Count > 0) == true)
						{
							workOrderInfo.Operations?
							.ForEach(op =>
							{
								op.Items?.ForEach(static x =>
								{
									if (string.IsNullOrEmpty(x.LineUID))
									{
										x.LineUID = Guid.CreateVersion7().ToString();
									}
								});
							});
							string componentDetailsJSON = JsonConvert.SerializeObject(
							workOrderInfo.Operations?
								.SelectMany(op => (op.Items ?? Enumerable.Empty<ProductionOrderItem>())
									.Select(item => new
									{
										op.OperationId,
										op.OperationNo, // o op.ProcessId si es lo que identifica la operación
										Item = item
									})
								)
						);

							_workOrderRepo.MergeProductionOrderComponents(workOrderInfo, componentDetailsJSON, systemOperator);
						}

						if (workOrderInfo.Operations?.Any(op => op.Byproducts?.Count > 0) == true && !string.IsNullOrEmpty(workOrderInfo.Code))
						{
							workOrderInfo.Operations?
							.ForEach(op =>
							{
								op.Byproducts?.ForEach(static x =>
								{
									if (string.IsNullOrEmpty(x.LineUID))
									{
										x.LineUID = Guid.CreateVersion7().ToString();
									}
								});
							});

							string byProductJson = JsonConvert.SerializeObject(
							workOrderInfo.Operations?
								.SelectMany(op => (op.Byproducts ?? Enumerable.Empty<ProductionOrderByProduct>())
									.Select(bp => new
									{
										op.OperationId,
										op.OperationNo, // o ProcessId si ese es el identificador
										ByProduct = bp
									})
								)
							);

							_workOrderRepo.MergeProductionOrderByProducts(workOrderInfo, byProductJson, systemOperator);
						}

						string ToolingJson = string.Empty;
						if (workOrderInfo.Operations?.Any(op =>
							(op.ToolingType?.Count > 0) ||
							(op.Machines?.Any(m => m.ToolingType?.Count > 0) == true)
						) == true && !string.IsNullOrEmpty(workOrderInfo.Code))
						{
							workOrderInfo.Operations?
							.ForEach(op =>
							{
								op.ToolingType?.ForEach(static x =>
								{
									if (string.IsNullOrEmpty(x.LineUID))
									{
										x.LineUID = Guid.CreateVersion7().ToString();
									}
								});
								op.Machines?.ForEach(m =>
								{
									m.ToolingType?.ForEach(static x =>
									{
										if (string.IsNullOrEmpty(x.LineUID))
										{
											x.LineUID = Guid.CreateVersion7().ToString();
										}
									});
								});
							});

							IEnumerable<object> allToolingTypesWithOp = workOrderInfo.Operations?
							.SelectMany(op =>
								// ToolingType a nivel operación
								(op.ToolingType ?? Enumerable.Empty<ProductionOrderResource>())
									.Select(t => new
									{
										op.OperationId,
										op.OperationNo,
										MachineCode = string.Empty,
										Tooling = t
									})
								// ToolingType dentro de cada máquina (preservando m)
								.Concat(
									op.Machines?
										.SelectMany(m => (m.ToolingType ?? Enumerable.Empty<ProductionOrderResource>())
											.Select(t => new
											{
												op.OperationId,
												op.OperationNo,
												MachineCode = m.MachineCode ?? string.Empty,
												MachineId = m.LineUID ?? string.Empty,
												Tooling = t
											})
										) ?? Enumerable.Empty<object>()
								)
							);

							ToolingJson = JsonConvert.SerializeObject(allToolingTypesWithOp);
						}
						_workOrderRepo.MergeProductionOrderToolingType(workOrderInfo, ToolingJson, systemOperator);

						string LaborJson = string.Empty;
						if (workOrderInfo.Operations?.Any(op =>
							(op.Labor?.Count > 0) ||
							(op.Machines?.Any(m => m.Labor?.Count > 0) == true)
						) == true && !string.IsNullOrEmpty(workOrderInfo.Code))
						{
							workOrderInfo.Operations?
							.ForEach(op =>
							{
								op.Labor?.ForEach(static x =>
								{
									if (string.IsNullOrEmpty(x.LineUID))
									{
										x.LineUID = Guid.CreateVersion7().ToString();
									}
								});
								op.Machines?.ForEach(m =>
								{
									m.Labor?.ForEach(static x =>
									{
										if (string.IsNullOrEmpty(x.LineUID))
										{
											x.LineUID = Guid.CreateVersion7().ToString();
										}
									});
								});
							});

							IEnumerable<object> allLabor = workOrderInfo.Operations?
							.SelectMany(op =>
								// ToolingType a nivel operación
								(op.Labor ?? Enumerable.Empty<ProductionOrderResource>())
									.Select(t => new
									{
										op.OperationId,
										op.OperationNo,
										MachineCode = string.Empty,
										Labor = t
									})
								// ToolingType dentro de cada máquina (preservando m)
								.Concat(
									op.Machines?
										.SelectMany(m => (m.Labor ?? Enumerable.Empty<ProductionOrderResource>())
											.Select(t => new
											{
												op.OperationId,
												op.OperationNo,
												MachineCode = m.MachineCode ?? string.Empty,
												MachineId = m.LineUID ?? string.Empty,
												Labor = t
											})
										) ?? Enumerable.Empty<object>()
								)
							);

							LaborJson = JsonConvert.SerializeObject(allLabor);
						}
						_workOrderRepo.MergeProductionOrderLabor(workOrderInfo, LaborJson, systemOperator);

						//Task Section
						if (!string.IsNullOrEmpty(workOrderInfo.Code) && workOrderInfo.Operations?.Any(op => op.Tasks?.Count > 0) == true)
						{
							//Order Creation:
							Dictionary<string, string> newTaskIds = [];
							// Get Ids from existing tasks when order is being created (Inherited tasks)
							// For each inheritedTasks clone the activity and fill the newTaskIds dictionary
							foreach (ProductionOrderOperation op in workOrderInfo.Operations)
							{
								foreach (Activity task in op.Tasks)
								{
									if (string.IsNullOrEmpty(task.Id))
									{
										task.Origin = OriginActivity.Order.ToStr();
										Activity newActivity = await _activityOperation.CreateActivity(task, systemOperator).ConfigureAwait(false);
										if (newActivity is not null && !string.IsNullOrEmpty(newActivity.Id))
										{
											_activityOperation.AssociateActivityWorkOrder(
												workOrderInfo.Code,
												op.OperationNo.ToStr(),
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
										bool tempResult = _activityOperation.RemoveActivityWorkOrderAssociation(workOrderInfo.Code, op.OperationNo.ToStr(), task.AssetId, task.Id, systemOperator);
									}
									else
									{
										if (task.ActivityClassId > 0)
										{
											await _activityOperation.UpdateActivity(task, systemOperator).ConfigureAwait(false);
										}
										_activityOperation.AssociateActivityWorkOrder(
											workOrderInfo.Code,
											op.OperationNo.ToStr(),
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
						ProductionOrder prodOrd = (await GetProductionOrder(workOrderInfo.Code).ConfigureAwait(false));
						await prodOrd.Log(EntityLogType.Update, systemOperator).ConfigureAwait(false);

						returnValue.ProductionOrder = prodOrd;
					}
				}
			}
			scope.Complete();
		}
		// SyncInitializer.ForcePush(new MessageBroker
		// {
		// 	Type = MessageBrokerType.WorkOrder,
		// 	Aux = mode == ActionDB.Create ? "N" : "U",
		// 	ElementValue = returnValue.ProductionOrder.Code,
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

		Machine[] machines = await _deviceOperation.ListDevices(false, true, true).ConfigureAwait(false);
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
	/// <summary>
	/// Validates the rules for a production order.
	/// </summary>
	public async Task ValidateRules(ProductionOrder orderInfo, User systemOperator)
	{
		string opcLicenseType = Config.Configuration["OPC-LicenseType"].ToStr();

		if (!string.Equals(opcLicenseType, "ULTIMATE", StringComparison.OrdinalIgnoreCase))
		{
			// Buscar duplicados en Labor y Tooling
			var allLaborAndTools = orderInfo.Operations
				.SelectMany(x => (x.Machines ?? Enumerable.Empty<ProductionOrderMachine>())
					.SelectMany(machine =>
						(machine.Labor ?? Enumerable.Empty<ProductionOrderResource>())
							.Select(_ => new { x.OperationNo, machine.MachineCode })
							.Concat((machine.ToolingType ?? Enumerable.Empty<ProductionOrderResource>())
								.Select(_ => new { x.OperationNo, machine.MachineCode })
							)
					)
				)
				.Where(x => !string.IsNullOrEmpty(x.MachineCode));

			int duplicated = allLaborAndTools
				.GroupBy(x => new { x.MachineCode, x.OperationNo })
				.Count(x => x.Count() > 1);

			if (duplicated > 0)
			{
				throw new Exception("OPCenter license Type does not allow more than one Labor/Tool per Operation");
			}
		}

		// Validar que las máquinas pertenezcan a la instalación del Warehouse
		Machine[] machines = await  _deviceOperation.ListDevices(false, true, true).ConfigureAwait(false);

		ProcessEntry orderEntry = (await _componentOperation.GetProcessEntryById(orderInfo.ProductId, systemOperator)
			.ConfigureAwait(false))
			.Find(x => x.Status != Status.Failed);

		if (orderEntry is not null)
		{
			Warehouse warehouse = _warehouseOperation.ListWarehouse(systemOperator)
				.Where(x => x.WarehouseId == orderEntry.Warehouse)
				.FirstOrDefault(x => x.Status != Status.Failed);

			if (warehouse is not null)
			{
				int wrongDevices = orderInfo.Operations
					.SelectMany(x => x.Machines ?? Enumerable.Empty<ProductionOrderMachine>())
					.Where(x => !string.IsNullOrEmpty(x.MachineCode) &&
								x.MachineCode != "00000000-0000-0000-0000-000000000000")
					.Select(x => new
					{
						x.MachineCode,
						Device = machines.FirstOrDefault(dev =>
							dev.Id == x.MachineCode &&   // Si MachineCode es GUID, coincide con dev.Id
							dev.FacilityCode == warehouse.FacilityCode)
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
				OrderProcess prc = itm.Process[0];
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
				throw new Exception("ERP|");
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
							string OperationNo = "0";
							OrderProcess currentProcess = null;
							if (obj.ContainsKey("operationNo"))
							{
								OperationNo = obj["operationNo"].ToStr();
								currentProcess = order.Processes.Find(pp => pp.ProcessId.ToStr() == OperationNo);
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
											//spd.LineId = curID.ToInt32();
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
											dev.ProcessId.ToStr() == OperationNo &&
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
											dev.ProcessId.ToStr() == OperationNo &&
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
	private async Task<bool> SendOrderDataToERP(ProductionOrder order, User systemOperator)
	{
		bool returnValue = false;
		if (order is not null)
		{
			ProcessEntry product = (await _componentOperation.GetProcessEntryById(order.ProductId, systemOperator).ConfigureAwait(false)).Find(x => x.Status != Status.Failed);
			string statusName = order.Status switch
			{
				Status.Active => "In Progress",
				Status.Disabled => "New",
				Status.Pending => "Released",
				Status.Queued => "Queued",
				Status.Cancelled => "Cancelled",
				Status.Hold => "On Hold",
				Status.Finished => "Finished",
				Status.Empty or Status.Deleted or Status.Failed or Status.Execute => "New", // or "" if preferred
				_ => "New"
			};

			WorkOrderExternal extOrder = new()
			{
				OrderCode = order.Code,
				Comments = order.Comments,
				DueDate = order.DueDate,
				FormulaCode = order.Formula,
				InventoryUoM = product?.UnitId,
				OrderGroup = order.OrderGroup,
				OrderType = order.OrderType,
				SalesOrder = order.SalesOrder,
				PlannedEndDate = order.PlannedEndDate,
				PlannedStartDate = order.PlannedStartDate,
				ProductCode = product?.Code,
				Quantity = order.Quantity,
				WarehouseCode = product.Warehouse,
				Version = product.Version,
				Sequence = product.Sequence,
				Status = statusName,
				Operations = [],
				LotNo = order.LotNo,
				OrderPriority = order.Priority.ToInt32().ToStr()
			};
			foreach (var itm in order.Operations.GroupBy(x => x.OperationNo, (key, x) => new { OperationNo = key, Process = x.ToArray() }).ToArray())
			{
				ProductionOrderOperation prc = itm.Process[0];
				EWP.SF.Common.Models.WorkOrderOperation op = new()
				{
					OperationName = prc.Name,
					LineId = prc.LineId,
					LineUID = prc.OperationId,
					OperationSubtype = prc.OperationSubTypeCode,
					Step = prc.OperationNo.ToDouble(),
					Quantity = prc.Quantity,
					PlannedStartDate = prc.PlannedStartDate,
					PlannedEndDate = prc.PlannedEndDate,
					OutputUoM = extOrder.InventoryUoM,
					Machines = [],
					Labor = [],
					Tooling = [],
					Items = [],
					ByProducts = []
				};
				op.Labor ??= [];
				op.Items ??= [];
				op.Tooling ??= [];
				op.ByProducts ??= [];

				foreach (ProductionOrderMachine machine in prc.Machines.Where(x => x.MachineCode != "00000000-0000-0000-0000-000000000000"))
				{
					double orderTimes = 1;
					double productMachineTime = -1;
					try
					{
						orderTimes = order.Quantity / product.Quantity;
						productMachineTime = machine.PlannedExecTime * orderTimes;
					}
					catch { }

					WorkOrderMachine opMachine = new()
					{
						MachineCode = machine.MachineCode,
						Primary = machine.Status == Status.Active ? "Yes" : "No",
						LineNo = machine.LineId.ToInt32(),
						LineUID = machine.LineUID,
						Labor = [],
						Tooling = [],
						Quantity = (machine.Status == Status.Active).ToInt32(),
						Comments = ""
					};
					if (productMachineTime >= 0)
					{
						opMachine.OperationTimeInSec = productMachineTime;
					}
					else
					{
						opMachine.OperationTimeInSec = machine.PlannedExecTime;
					}

					foreach (ProductionOrderResource ml in machine.Labor ?? [])
					{
						WorkOrderMachineLabor woMl = new()
						{
							Quantity = ml.PlannedQty,
							LineId = ml.LineId.ToInt32(),
							LineUID = ml.LineUID,
							ProfileCode = ml.Code,
							Comments = ml.Comments,
							Usage = ml.Usage
						};

						opMachine.Labor.Add(woMl);
					}
					foreach (ProductionOrderResource mt in machine.ToolingType ?? [])
					{
						WorkOrderMachineTool woMt = new()
						{
							Quantity = mt.PlannedQty,
							LineId = mt.LineId.ToInt32(),
							LineUID = mt.LineUID,
							ToolingCode = mt.Code,
							Comments = mt.Comments,
							Usage = mt.Usage
						};

						opMachine.Tooling.Add(woMt);
					}
					op.Machines.Add(opMachine);
				}

				foreach (ProductionOrderResource ml in prc.Labor ?? [])
				{
					WorkOrderOperationLabor woOl = new()
					{
						Quantity = ml.PlannedQty,
						LineId = ml.LineId.ToInt32(),
						LineUID = ml.LineUID,
						ProfileCode = ml.Code,
						Comments = ml.Comments,
						Usage = ml.Usage
					};

					op.Labor.Add(woOl);
				}

				foreach (ProductionOrderResource mt in prc.ToolingType ?? [])
				{
					WorkOrderOperationTool woOt = new()
					{
						Quantity = mt.PlannedQty,
						LineId = mt.LineId.ToInt32(),
						LineUID = mt.LineUID,
						ToolingCode = mt.Code,
						Comments = mt.Comments,
						Usage = mt.Usage
					};

					op.Tooling.Add(woOt);
				}

				foreach (ProductionOrderItem mt in prc.Items ?? [])
				{
					WorkOrderItem itmOrd = new()
					{
						ItemCode = mt.ItemCode,
						LineId = mt.LineId.ToInt32(),
						LineUID = mt.LineUID,
						WarehouseCode = mt.WarehouseCode,
						IssueMethod = mt.Consumption == 1 ? "Backflush" : "Manual",
						InventoryUoM = mt.UnitCode,
						Quantity = mt.Quantity,
						Comments = mt.Comments
					};
					op.Items.Add(itmOrd);
				}

				foreach (ProductionOrderByProduct mt in prc.Byproducts ?? [])
				{
					WorkOrderByProduct byp = new()
					{
						ItemCode = mt.ItemCode,
						LineId = mt.LineId.ToInt32(),
						LineUID = mt.LineUID,
						WarehouseCode = mt.WarehouseCode,
						Quantity = mt.Quantity,
						InventoryUoM = mt.UnitCode,
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
				throw new Exception("ERP|");
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
						if (order.Code != entry && !string.IsNullOrEmpty(entry))
						{
							order.Code = entry;
							order.OrderCode = entry;
						}
					}
					foreach (JObject obj in msg["operations"].Cast<JObject>())
					{
						double OperationNo = 0;
						ProductionOrderOperation currentProcess = null;
						if (obj.ContainsKey("operationNo"))
						{
							OperationNo = obj["operationNo"].ToDouble();
							currentProcess = order.Operations.Find(pp => pp.OperationNo == OperationNo);
						}
						if (currentProcess is not null)
						{
							if (obj.ContainsKey("items"))
							{
								foreach (JObject jItm in obj["items"].Cast<JObject>())
								{
									string itmUid = jItm["lineUID"].ToStr();
									string itmLid = jItm["lineID"].ToStr();
									ProductionOrderItem cmp = currentProcess.Items?.Find(i => i.LineUID == itmUid);
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
									ProductionOrderMachine spd = currentProcess.Machines.Find(dev => dev.LineUID == curUID);
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
									ProductionOrderByProduct spd = currentProcess.Byproducts?.Find(dev => dev.LineUID == curUID);
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
									ProductionOrderResource woL = currentProcess.Labor?.Find(dev =>
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
									ProductionOrderResource tool = currentProcess.ToolingType?.Find(dev =>
										dev.LineUID == curUID
									);
									if (tool is not null)
									{
										tool.LineId = curID.ToInt32().ToStr();
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
	/// <summary>
	/// Transfers products from one work order to another.
	/// </summary>
	public async Task<object> UpdateWorkOrderComponent(TransactionMaterialSyncRequest request, User systemOperator)
    {
        var material = request.OrderTransactionsMaterial?.FirstOrDefault();
        string workOrderId = material?.OrderCode;
        string employeeId = material?.EmployeeId;
        List<OrderComponent> componentValues = request.OrderTransactionsMaterialDetail?.Select(d => new OrderComponent {
            SourceId = d.ItemCode,
            InputQty = d.Quantity,
            LineId = d.LineNo,
            ProcessId = material?.OperationNo,
            Batches = new List<ComponentBatch> {
                new ComponentBatch {
                    Batch = d.LotNumber,
                    Quantity = d.Quantity,
                    Location = d.BinLocationCode,
                    WarehouseCode = d.WarehouseCode,
                    BatchDate = d.ExpDate ?? DateTime.MinValue,
                    InventoryStatus = d.InventoryStatusCode,
                    Pallet = d.Pallet,
                    Type = d.Type
                }
            }
        }).ToList() ?? new List<OrderComponent>();

		object returnValue = null;
		string objectToInsert = "";
		WorkOrder tempOrder = (await GetWorkOrder(workOrderId).ConfigureAwait(false)).FirstOrDefault();
		List<OrderComponent> valuesToInsert = [];
		bool CanProceed = true;
		bool isReturn = false;
		string movType = "Issue";
		string movEvent = SyncERPEntity.MATERIAL_ISSUE_SERVICE;

		if (tempOrder?.Components is not null)
		{
			foreach (OrderComponent order in tempOrder.Components)
			{
				OrderComponent[] tempValue = [.. componentValues.Where(x => x.ProcessId.ToStr() == order.ProcessId.ToStr() && x.ComponentType == order.ComponentType && x.SourceId == order.SourceId && x.LineId == order.LineId)];
				if (tempValue.Length > 0)
				{
					try
					{
						double oldValue = order.InputQty;
						double QtyChanged = tempValue
							.Where(x => x.Batches is not null)
							.SelectMany(x => x.Batches)
							.Sum(x => x.Quantity);
						order.InputQty = oldValue + QtyChanged;

						if (QtyChanged != 0)
						{
							foreach (OrderComponent y in tempValue)
							{
								y.InputQty = y.Batches.Sum(x => x.Quantity);
								if (string.IsNullOrEmpty(y.LineId))
								{
									y.LineId = "0";
								}
							}

							valuesToInsert.AddRange(tempValue);
						}
						if (QtyChanged < 0)
						{
							isReturn = true;
						}
					}
					catch { }
				}
			}

			string transactionId = Guid.CreateVersion7().ToStr();
			List<KeyValuePair<string, string>> ErrorList = [];

			#region object 2 ins

			object ob2Ins = new
			{
				sf_order_transactions_material = valuesToInsert.Select(x =>
					new
					{
						OperationNo = x.ProcessId,
						OrderCode = workOrderId,
						LineNo = x.LineId,
						Quantity = x.InputQty,
						EmployeeId = employeeId,
						UserId = systemOperator.Id,
						TransactionId = transactionId,
						Comments = string.Empty,
						ExternalId = string.Empty,
						ExternalDate = string.Empty
					}
				).ToArray(),
				c = valuesToInsert.SelectMany(x => x.Batches, (parent, detail) =>
				new
				{
					TransactionId = transactionId,
					ItemCode = detail.ComponentId,
					detail.Quantity,
					LotNumber = detail.Batch,
					detail.Pallet,
					BinLocationCode = detail.Location,
					InventoryStatusCode = detail.InventoryStatus,
					ExpDate = detail.BatchDate,
					detail.WarehouseCode,
					detail.Type,
					MachineCode = parent.MachineId,
					OriginalItem = parent.OriginalSourceId,
					LineNo = string.IsNullOrEmpty(detail.LineId) ? parent.LineId : detail.LineId,
					OrderLot = detail.OrderId
				}).ToArray()
			};

			#endregion object 2 ins

			objectToInsert = JsonConvert.SerializeObject(ob2Ins);
			string externalId = string.Empty;

			//Validar evento onIssueMaterial
			if (!string.IsNullOrEmpty(tempOrder.ExternalId))
			{
				if (isReturn)
				{
					movType = "Return";
					movEvent = SyncERPEntity.MATERIAL_RETURN_SERVICE;
				}

				foreach (OrderComponent order in valuesToInsert)
				{
					order.Batches.RemoveAll(x => x.Quantity == 0);
					order.ExternalId = order.SourceId;
				}
				List<OrderComponent> comps = [.. valuesToInsert.Where(x => !string.IsNullOrEmpty(x.ExternalId))];
				comps = JsonConvert.DeserializeObject<List<OrderComponent>>(JsonConvert.SerializeObject(comps));
				comps.ForEach(c => c.Batches.ForEach(b => b.Quantity = Math.Abs(b.Quantity)));
				comps.RemoveAll(x => x.Batches.Count == 0);

				if (comps.Count > 0)
				{
					foreach (OrderComponent cp in comps)
					{
						cp.InputQty = Math.Abs(cp.InputQty);
						Component item = (await _componentOperation.GetComponents(cp.SourceId).ConfigureAwait(false)).Where(x => x.Status != Status.Failed)?.FirstOrDefault();
					}
					if (!string.IsNullOrEmpty(employeeId))
					{
						systemOperator.EmployeeId = employeeId;
					}
					object requestParams = new
					{
						TransactionId = transactionId,
						//tempOrder.ExternalId,
						tempOrder.OrderType,
						tempOrder.OrderCode,
						Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
						Type = movType,
						comps[0].ProcessId,
						Components = comps,
						Comments = "",
						Employee = systemOperator.EmployeeId
					};
					// 		var endpointResponse = await _dataSyncServiceManager.ExecuteServiceEndpoint(
					// 	movEvent,
					// 	string.Empty,
					// 	JsonConvert.SerializeObject(requestParams),
					// 	"POST",
					// 	systemOperator
					// ).ConfigureAwait(false);
					//Not saved in Temp table 
					//BrokerDAL.TrySaveTempWorkOrderTransaction(movEvent, objectToInsert);
					// This logic replace with kafka
					// DataSyncHttpResponse resp = await ServiceManager.ExecuteService(
					// 	movEvent,
					// 	TriggerType.SmartFactory,
					// 	ServiceExecOrigin.Event,
					// 	systemOperator,
					// 	"POST",
					// 	string.Empty,
					// 	JsonConvert.SerializeObject(requestParams)
					// ).ConfigureAwait(false);
					// 		CanProceed = resp.StatusCode == HttpStatusCode.OK;
					// 		if (CanProceed)
					// 		{
					// 			try
					// 			{
					// 				if (!string.IsNullOrEmpty(resp.Message))
					// 				{
					// 					JObject o = JObject.Parse(resp.Message);
					// 					JObject m = JObject.Parse(o["Message"].ToString());
					// 					JObject d = JObject.Parse(m["data"].ToString());
					// 					externalId = d["docNum"].ToString();
					// 				}
					// 			}
					// 			catch
					// 			{
					// 			}
					// 		}
					// 		else
					// 		{
					// 			ErrorList.Add(new KeyValuePair<string, string>("500", "ERP|" + resp.Message));
					// 		}
				}
				//}

				if (CanProceed)
				{
					try
					{
						List<MessageBroker> messagesToPush = [];
						using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
						valuesToInsert.ForEach(tempValue =>
						{
							string result = _workOrderRepo.UpdateMaterialManual(transactionId, tempValue, employeeId, workOrderId, externalId, systemOperator);
							if (!string.IsNullOrEmpty(result))
							{
								messagesToPush.Add(new MessageBroker
								{
									Type = MessageBrokerType.ManualMaterialIssue,
									ElementId = workOrderId,
									ElementValue = tempValue.ProcessId,
									MachineId = tempValue.MachineId,
									Aux = string.Format("{0}|{1}", tempValue.SourceId, tempValue.InputQty.ToStr())
								});

								messagesToPush.Add(new MessageBroker
								{
									Type = MessageBrokerType.ExternalMaterialIssue,
									ElementId = result
								});
							}
						});

						tempOrder.Components = [.. tempOrder.Components.Where(x => x.SourceId == x.OriginalSourceId)];
						if (tempOrder.Components?.Count > 0)
						{
							string componentDetailsJSON = JsonConvert.SerializeObject(tempOrder.Components);
							//bool success = _workOrderRepo.MergeWorkOrderComponents(tempOrder, componentDetailsJSON, systemOperator);
						}

						scope.Complete();
						returnValue = ob2Ins;

						//messagesToPush.ForEach(SyncInitializer.ForcePush);
					}
					catch (Exception ex)
					{
						//await BrokerDAL.SaveTransactionErrorLog(transactionId, movEvent, ex.Message + "|" + ex.StackTrace, objectToInsert).ConfigureAwait(false);
						throw;
					}
				}

				if (!CanProceed && ErrorList.Count > 0)
				{
					throw new Exception(ErrorList.FirstOrDefault().Value);
				}
			}
		}
		return returnValue;
	}
	// public async Task<bool> CreateOrderProgressEntry(ManualOrderProgressRequest request, User systemOperator, DataSyncServiceManager ServiceManager)
	// {
	// 	#region Permission validation

	// 	if (!systemOperator.Permissions.Any(x => x.Code == Permissions.PRD_ORDERPROGRESS_MANAGE))
	// 	{
	// 		throw new UnauthorizedAccessException(noPermission);
	// 	}

	// 	#endregion Permission validation

	// 	string objectToInsert = "";
	// 	Machine m = GetDevice(request.MachineId);
	// 	double performanceValue = -1;
	// 	bool CanProceed = true;
	// 	OrderProcess lastProcess = null;
	// 	if (m?.OEEConfiguration is not null)
	// 	{
	// 		if (m.OEEConfiguration.PerformanceDefaultType == 1)
	// 		{
	// 			Sensor s = m.Sensors.Find(x => x.Id == m.OEEConfiguration.PerformanceDefaultValue);
	// 			if (s is not null)
	// 			{
	// 				performanceValue = SyncService.GetMachineValue(m.Id, s.Code).ToDouble(-1);
	// 			}
	// 		}
	// 		else if (m.OEEConfiguration.PerformanceDefaultType == 2)
	// 		{
	// 			MachineParam s = m.Parameters.Find(x => x.Id == m.OEEConfiguration.PerformanceDefaultValue);
	// 			if (s is not null)
	// 			{
	// 				performanceValue = SyncService.GetMachineValue(m.Id, s.Code).ToDouble(-1);
	// 			}
	// 		}
	// 		else
	// 		{
	// 			performanceValue = m.OEEConfiguration.PerformanceDefaultValue.ToDouble(-1);
	// 		}
	// 	}
	// 	List<KeyValuePair<string, string>> ErrorList = [];
	// 	string transactionId = Guid.NewGuid().ToStr();
	// 	WorkOrder tempOrder = (await GetWorkOrder(request.WorkOrderId).ConfigureAwait(false)).FirstOrDefault();

	// 	#region Object to Insert Test

	// 	List<SubProduct> subs = request.Subproducts?.ToList();
	// 	subs ??= [];
	// 	subs.Add(new SubProduct
	// 	{
	// 		Batch = request.Batch,
	// 		ComponentId = lastProcess?.Output,
	// 		Pallet = request.Pallet,
	// 		LineId = string.Empty,
	// 		Quantity = request.Quantity
	// 	});
	// 	object ob2Ins = new
	// 	{
	// 		sf_order_transactions_product = new[]
	// 		{
	// 				new
	// 				{
	// 					TransactionId=transactionId,
	// 					OperationNo = request.ProcessId,
	// 					OrderCode = request.WorkOrderId,
	// 					StartEntryDate = request.StartEntry,
	// 					EndEntryDate = request.EndEntry,
	// 					Quantity     = 1,
	// 					OrderFactor =  1,
	// 					ProcessFactor = 1,
	// 					request.EmployeeId,
	// 					UserId = systemOperator.Id,
	// 					request.ActivityInstanceId,
	// 					request.IsPartial,
	// 					request.IssuedLot,
	// 					request.Comments,
	// 					ExternalId = string.Empty,
	// 					ExternalDate = string.Empty
	// 				}
	// 			},
	// 		sf_order_transactions_product_detail = subs.Select(x =>
	// 		new
	// 		{
	// 			TransactionId = transactionId,
	// 			ItemCode = x.ComponentId,
	// 			LotNumber = x.Batch,
	// 			x.Pallet,
	// 			BinLocationCode = request.Location,
	// 			x.Quantity,
	// 			LotStatusCode = "",
	// 			InventoryStatusCode = request.InventoryStatus,
	// 			LineNo = x.LineId,
	// 			x.WarehouseCode,
	// 			MachineCode = request.MachineId,
	// 			OriginalMachine = request.OriginalMachineId
	// 		}).ToArray()
	// 	};
	// 	objectToInsert = JsonConvert.SerializeObject(ob2Ins);

	// 	#endregion Object to Insert Test

	// 	string ExternalId = string.Empty;
	// 	if (tempOrder is not null && !string.IsNullOrEmpty(tempOrder.ExternalId))
	// 	{
	// 		lastProcess = tempOrder.Processes.Find(x => x.IsOutput);
	// 		if (lastProcess is not null && lastProcess.ProcessId.ToDouble() == request.ProcessId.ToDouble())
	// 		{
	// 			List<KeyValuePair<string, object>> customData = [];
	// 			request.Subproducts ??= [];
	// 			request.Subproducts.ForEach(sp =>
	// 			{
	// 				sp.Pallet = request.Pallet;
	// 				if (string.IsNullOrEmpty(sp.LineId))
	// 				{
	// 					sp.LineId = "0";
	// 				}
	// 			});
	// 			Employee employee = GetEmployee(request.EmployeeId);
	// 			string operador = string.Empty;
	// 			if (employee is not null)
	// 			{
	// 				operador = employee.Name;
	// 				systemOperator.EmployeeId = request.EmployeeId;
	// 			}
	// 			customData.Add(new KeyValuePair<string, object>("Comments", request.Comments));
	// 			customData.Add(new KeyValuePair<string, object>("IssuedLot", request.IssuedLot));
	// 			customData.Add(new KeyValuePair<string, object>("Employee", operador));

	// 			List<MaterialIssueProductModel> productsErp = [];
	// 			MaterialIssueProductModel firstPart = new()
	// 			{
	// 				ItemCode = lastProcess?.Output,
	// 				LineId = "0",
	// 				Quantity = request.Quantity,
	// 				LineType = "Product",
	// 				Warehouse = tempOrder.WarehouseId,
	// 				Lots = []
	// 			};
	// 			firstPart.Lots.Add(new ComponentBatch
	// 			{
	// 				WarehouseCode = tempOrder.WarehouseId,
	// 				Batch = request.Batch,
	// 				Pallet = request.Pallet,
	// 				Location = request.Location,
	// 				LineId = "0",
	// 				Quantity = request.Quantity,
	// 				InventoryStatus = request.InventoryStatus,
	// 			});
	// 			productsErp.Add(firstPart);
	// 			if (request.Rejected > 0)
	// 			{
	// 				MaterialIssueProductModel secondPart = new()
	// 				{
	// 					ItemCode = lastProcess?.Output,
	// 					LineId = "0",
	// 					Quantity = Math.Abs(request.Rejected) * -1,
	// 					LineType = "Product",
	// 					Warehouse = tempOrder.WarehouseId,
	// 					Lots = []
	// 				};
	// 				secondPart.Lots.Add(new ComponentBatch
	// 				{
	// 					WarehouseCode = tempOrder.WarehouseId,
	// 					Batch = request.Batch,
	// 					Pallet = request.Pallet,
	// 					Location = request.Location,
	// 					LineId = "0",
	// 					Quantity = Math.Abs(request.Rejected) * -1,
	// 					InventoryStatus = request.InventoryStatus,
	// 				});
	// 				productsErp.Add(secondPart);
	// 			}
	// 			request.Subproducts?.ForEach(sub =>
	// 				{
	// 					if (string.IsNullOrEmpty(sub.LineId))
	// 					{
	// 						sub.LineId = "0";
	// 					}
	// 					SubProduct orderSubproduct = tempOrder.Subproducts.Find(osp => osp.ComponentId == sub.ComponentId && osp.ProcessId == lastProcess.ProcessId);
	// 					MaterialIssueProductModel mod = new()
	// 					{
	// 						ItemCode = sub.ComponentId,
	// 						LineId = sub.LineId,
	// 						Quantity = sub.Quantity,
	// 						LineType = "ByProduct",
	// 						Warehouse = orderSubproduct is not null ? orderSubproduct.WarehouseCode : tempOrder.WarehouseId,
	// 						Lots = []
	// 					};
	// 					mod.Lots.Add(new ComponentBatch
	// 					{
	// 						Batch = sub.Batch,
	// 						Pallet = sub.Pallet,
	// 						Location = sub.Location,
	// 						LineId = sub.LineId,
	// 						Quantity = sub.Quantity,
	// 						InventoryStatus = sub.InventoryStatus,
	// 						WarehouseCode = orderSubproduct is not null ? orderSubproduct.WarehouseCode : tempOrder.WarehouseId
	// 					});
	// 					productsErp.Add(mod);

	// 					if (sub.Rejected > 0)
	// 					{
	// 						MaterialIssueProductModel rej = new()
	// 						{
	// 							ItemCode = sub.ComponentId,
	// 							LineId = sub.LineId,
	// 							Quantity = Math.Abs(sub.Rejected) * -1,
	// 							LineType = "ByProduct",
	// 							Warehouse = orderSubproduct is not null ? orderSubproduct.WarehouseCode : tempOrder.WarehouseId,
	// 							Lots = []
	// 						};
	// 						rej.Lots.Add(new ComponentBatch
	// 						{
	// 							Batch = sub.Batch,
	// 							Pallet = sub.Pallet,
	// 							Location = sub.Location,
	// 							LineId = sub.LineId,
	// 							Quantity = Math.Abs(sub.Rejected) * -1,
	// 							InventoryStatus = sub.InventoryStatus,
	// 							WarehouseCode = orderSubproduct is not null ? orderSubproduct.WarehouseCode : tempOrder.WarehouseId
	// 						});
	// 						productsErp.Add(rej);
	// 					}
	// 				});
	// 			string movEvent = BackgroundServices.PRODUCT_RECEIPT_SERVICE;
	// 			object requestParams = new
	// 			{
	// 				TransactionId = transactionId,
	// 				tempOrder.ExternalId,
	// 				ExternalComponentId = lastProcess.Output,
	// 				OperationNo = lastProcess.ProcessId,
	// 				Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
	// 				request.Batch,
	// 				request.Pallet,
	// 				request.Quantity,
	// 				request.Rejected,
	// 				CustomData = customData,
	// 				Products = productsErp,
	// 				request.Comments,
	// 				Employee = systemOperator.EmployeeId,
	// 				request.Location,
	// 				request.InventoryStatus
	// 			};
	// 			_ = BrokerDAL.TrySaveTempWorkOrderTransaction(movEvent, objectToInsert);
	// 			DataSyncHttpResponse resp = await ServiceManager.ExecuteService(
	// 				movEvent,
	// 				TriggerType.SmartFactory,
	// 				ServiceExecOrigin.Event,
	// 				systemOperator,
	// 				"POST",
	// 				string.Empty,
	// 				JsonConvert.SerializeObject(requestParams)
	// 			).ConfigureAwait(false);

	// 			CanProceed = resp.StatusCode == HttpStatusCode.OK;
	// 			if (!CanProceed)
	// 			{
	// 				ErrorList.Add(new KeyValuePair<string, string>("500", "ERP|" + resp.Message));
	// 			}
	// 			else
	// 			{
	// 				try
	// 				{
	// 					if (!string.IsNullOrEmpty(resp.Message))
	// 					{
	// 						JObject o = JObject.Parse(resp.Message);
	// 						JObject msg = JObject.Parse(o["Message"].ToString());
	// 						JObject d = JObject.Parse(msg["data"].ToString());
	// 						ExternalId = d["docNum"].ToString();
	// 					}
	// 				}
	// 				catch
	// 				{
	// 				}
	// 			}
	// 		}
	// 	}
	// 	if (CanProceed)
	// 	{
	// 		List<MessageBroker> result = null;
	// 		try
	// 		{
	// 			using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
	// 			{
	// 				result = BrokerDAL.CreateOrderProgressManual(request, performanceValue, systemOperator, ExternalId, ref transactionId);

	// 				if (request.Subproducts?.Count > 0)
	// 				{
	// 					request.Subproducts.ForEach(sub =>
	// 						BrokerDAL.EmitSubproduct(
	// 							request,
	// 							sub.ComponentId,
	// 							sub.Quantity,
	// 							sub.Rejected,
	// 							sub.Batch, sub.Pallet, transactionId, sub.LineId, sub.InventoryStatus, sub.Location));
	// 				}

	// 				scope.Complete();
	// 			}
	// 			if (result?.Count > 0)
	// 			{
	// 				result.ForEach(SyncInitializer.ForcePush);
	// 			}
	// 		}
	// 		catch (Exception ex)
	// 		{
	// 			//logger.Error(ex);
	// 			//await BrokerDAL.SaveTransactionErrorLog(transactionId, "onProductReceipt", ex.Message + "|" + ex.StackTrace, objectToInsert).ConfigureAwait(false);
	// 			throw;
	// 		}
	// 	}

	// 	return !CanProceed && ErrorList.Count > 0 ? throw new Exception(ErrorList.FirstOrDefault().Value) : true;
	// }

	/// <summary>
	/// Gets data from sf_order_transactions_material and detail tables and creates requestParams type object for push data
	/// Only retrieves transactions where ExternalId is null or empty
	///
	/// IMPORTANT: This method processes ALL pending transactions.
	/// Returns a special object that signals the processor to send individual POST requests for each transaction.
	/// </summary>
	/// <param name="systemOperator">The system operator user</param>
	/// <param name="cancel">Cancellation token</param>
	/// <returns>Object containing list of all pending transactions to process individually</returns>
	public async Task<object> GetMaterialTransactionRequestParams(User systemOperator, CancellationToken cancel = default)
	{
		// Get all transactions where ExternalId is null or empty
		List<OrderTransactionMaterial> transactions = await _orderTransactionMaterialRepo.GetOrderTransactionMaterialWithoutExternalId(cancel).ConfigureAwait(false);

		if (transactions == null || transactions.Count == 0)
		{
			throw new Exception("No transactions found without ExternalId");
		}

		// Build a list of request params for each transaction
		List<object> allTransactionParams = [];

		foreach (var transaction in transactions)
		{
			// Get work order information
			List<WorkOrder> workOrders = await GetWorkOrder(transaction.OrderId).ConfigureAwait(false);
			WorkOrder? workOrder = workOrders?.FirstOrDefault();

			if (workOrder == null)
			{
				// Skip this transaction if work order not found
				continue;
			}

			// Determine movement type based on direction
			string movType = transaction.Direction == 1 ? "Issue" :
			                 transaction.Direction == 2 ? "Return" : "Scrap";

			// Build components list from transaction details
			List<object> components = [];

			foreach (var detail in transaction.Details)
			{
				// Get component information
				Component? component = (await _componentOperation.GetComponents(detail.ItemId, true).ConfigureAwait(false))
					.Where(x => x.Status != Status.Failed)?.FirstOrDefault();

				if (component != null)
				{
					var componentObj = new
					{
						OperationNo = transaction.OperationId,
						SourceId = component.Code,
						ItemCode = component.ExternalId,
						InputQty = Math.Abs(detail.Quantity),
						LineId = detail.LineId,
						WarehouseCode = detail.WarehouseCode,
						Batches = new[]
						{
							new
							{
								ComponentId = component.Code,
								Quantity = Math.Abs(detail.Quantity),
								Batch = detail.LotNumber,
								Pallet = detail.Pallet,
								Location = detail.LocationCode,
								InventoryStatus = detail.InventoryStatus,
								BatchDate = detail.ExpDate,
								WarehouseCode = detail.WarehouseCode,
								Type = detail.Type,
								OrderId = detail.OrderId,
								LineId = detail.LineId
							}
						}
					};

					components.Add(componentObj);
				}
			}

			// Create request parameters object for this transaction
			var transactionParams = new
			{
				TransactionId = transaction.TransactionId,
				OrderType = workOrder.OrderType,
				WorkOrderId = workOrder.OrderCode,
				Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
				Type = movType,
				OperationNo = transaction.OperationId,
				Components = components,
				Comments = transaction.Comments ?? string.Empty,
				Employee = transaction.EmployeeId ?? systemOperator.EmployeeId
			};

			allTransactionParams.Add(transactionParams);
		}

		if (allTransactionParams.Count == 0)
		{
			throw new Exception("No valid transactions to process");
		}

		// Return object with special flag to indicate multiple transactions
		// The processor will detect this and send individual POST requests
		return new
		{
			ProcessIndividually = true,
			Transactions = allTransactionParams
		};
	}
private static void RemoveProductionOrderResourceByLineId(ProductionOrder workOrder, string lineId)
	{
		try
		{
			foreach (ProductionOrderOperation process in workOrder.Operations)
			{
				process.Byproducts?.RemoveAll(bp => bp.LineId.ToInt32() == lineId.ToInt32());
				process.Items?.RemoveAll(i => i.LineId.ToInt32() == lineId.ToInt32());
				process.Machines?.RemoveAll(m => m.LineId.ToInt32() == lineId.ToInt32());
				process.Byproducts?.RemoveAll(b => b.LineId.ToInt32() == lineId.ToInt32());
				process.Labor?.RemoveAll(l => l.LineId.ToInt32() == lineId.ToInt32());
				process.ToolingType?.RemoveAll(t => t.LineId.ToInt32() == lineId.ToInt32());
			}
		}
		catch
		{
		}
	}
	private static void ValidateOperationSequenceGroups(List<Common.Models.WorkOrderOperation> operations)
	{

		if (operations == null || operations.Count == 0)
			return;

		var groups = new Dictionary<string, RangeValidator>();
		var ranges = new List<RangeValidator>();

		foreach (var op in operations)
		{
			var groupName = op.OperationGroup?.Trim();

			// If there is no group
			if (string.IsNullOrWhiteSpace(groupName))
			{
				ranges.Add(new RangeValidator
				{
					Name = op.OperationCode,
					Min = op.Step.ToInt32(),
					Max = op.Step.ToInt32(),
					IsGroup = false
				});
				continue;
			}

			// when it belongs to a group
			if (!groups.ContainsKey(groupName))
			{
				groups[groupName] = new RangeValidator
				{
					Name = groupName,
					Min = op.Step.ToInt32(),
					Max = op.Step.ToInt32(),
					IsGroup = true
				};
			}
			else
			{
				var g = groups[groupName];
				g.Min = Math.Min(g.Min, op.Step.ToInt32());
				g.Max = Math.Max(g.Max, op.Step.ToInt32());
			}
		}

		// Add groups to range list
		ranges.AddRange(groups.Values);

		//Group overlap validation
		for (int i = 0; i < ranges.Count; i++)
		{
			for (int j = 0; j < ranges.Count; j++)
			{
				if (i == j) continue;

				var r1 = ranges[i];
				var r2 = ranges[j];

				bool overlaps =
					(r2.Min > r1.Min && r2.Min < r1.Max) ||
					(r2.Max > r1.Min && r2.Max < r1.Max) ||
					(r2.Min <= r1.Min && r2.Max >= r1.Max);

				if (overlaps)
				{
					throw new InvalidOperationException(
						$"Theres a group overlap with operations \"{r1.Name}\" ({r1.Min}-{r1.Max}) y \"{r2.Name}\" ({r2.Min}-{r2.Max}).");
				}
			}
		}

	}
public async Task<ProductionOrder> GetProductionOrder(string OrderCode)
	{
		return await _workOrderRepo.GetProductionOrder(OrderCode).ConfigureAwait(false);
	}
		}
