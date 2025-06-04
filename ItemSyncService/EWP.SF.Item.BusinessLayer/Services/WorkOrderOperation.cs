using EWP.SF.Common.Models;
using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;
using Newtonsoft.Json;
using EWP.SF.Common.Models.MigrationModels;
using System.Transactions;

namespace EWP.SF.Item.BusinessLayer;

public class WorkOrderOperation : IWorkOrderOperation
{
	private readonly IWorkOrderRepo _workOrderRepo;

	private readonly ICatalogRepo _catalogRepo;
	private readonly IApplicationSettings _applicationSettings;

	private readonly IMeasureUnitOperation _measureUnitOperation;

	private readonly IEmployeeOperation _employeeOperation;

	public WorkOrderOperation(IWorkOrderRepo workOrderRepo, ICatalogRepo catalogRepo, IApplicationSettings applicationSettings
	, IMeasureUnitOperation measureUnitOperation, IEmployeeOperation employeeOperation)
	{

		_workOrderRepo = workOrderRepo;
		_catalogRepo = catalogRepo;
		_applicationSettings = applicationSettings;
		_measureUnitOperation = measureUnitOperation;
		_employeeOperation = employeeOperation;
	}
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
					List<TimeZoneCatalog> tz = await _workOrderRepo.GetTimezones(true).ConfigureAwait(false);
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
			List<TimeZoneCatalog> tz = await _workOrderRepo.GetTimezones(true).ConfigureAwait(false);
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

		// if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_ORDERPROGRESS_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		return BrokerDAL.GetProductReturnContext(workorderId);
	}
}
