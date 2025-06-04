using EWP.SF.Common.Models;
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	
using System.Transactions;


using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.Text;
using Range = EWP.SF.Common.Models.Range;
using SixLabors.ImageSharp;
using EWP.SF.Common.Models.Catalogs;

namespace EWP.SF.Item.BusinessLayer;
public class OrderTransactionMaterialOperation : IOrderTransactionMaterialOperation
{
    private readonly IOrderTransactionMaterialRepo _orderTransactionMaterialRepo;
    private readonly IApplicationSettings _applicationSettings;

    private readonly IWorkOrderOperation _workOrderOperation;

    private readonly IComponentOperation _componentOperation;

	public OrderTransactionMaterialOperation(IOrderTransactionMaterialRepo orderTransactionMaterialRepo, IApplicationSettings applicationSettings
	, IWorkOrderOperation workOrderOperation, IComponentOperation componentOperation)
	{
		_orderTransactionMaterialRepo = orderTransactionMaterialRepo;
		_applicationSettings = applicationSettings;
	    _workOrderOperation = workOrderOperation;
		_componentOperation = componentOperation;
	}
	/// <summary>
	///
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public ResponseData MergeOrderTransactionMaterial(OrderTransactionMaterial orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		#region Permission validation

		// if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		// {
		// 	throw new UnauthorizedAccessException(noPermission);
		// }

		#endregion Permission validation

		return _orderTransactionMaterialRepo.MergeOrderTransactionMaterial(orderTransactionInfo, systemOperator, Validate);
	}

	/// <summary>
	///
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateMaterialReturn(List<MaterialReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;

		if (OrderTransactionList?.Count > 0)
		{
			NotifyOnce = OrderTransactionList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (MaterialReturnExternal orderTransaction in OrderTransactionList)
			{
				Line++;
				try
				{
					BaseId = orderTransaction.DocCode;
					List<ValidationResult> results = [];
					ValidationContext context = new(orderTransaction, null, null);
					if (!Validator.TryValidateObject(orderTransaction, context, results))
					{
						throw new Exception($"{results[0]}");
					}
					string transactionId = Guid.NewGuid().ToString();
					// Order

					string orderId = string.Empty;
					WorkOrder wo = (await _workOrderOperation.GetWorkOrder(orderTransaction.OrderCode).ConfigureAwait(false)).FirstOrDefault();
					if (wo is not null)
					{
						orderId = wo.Id;
					}
					if (string.IsNullOrEmpty(orderId))
					{
						throw new Exception("Order Doesn't Exists");
					}

					if (wo.Processes.Find(x => x.ProcessId.ToDouble() == orderTransaction.OperationNo.ToDouble()) is null)
					{
						throw new Exception($"OperationNo is required for transaction in order {orderTransaction.OrderCode}");
					}
					// Data
					OrderTransactionMaterial orderTransactionInfo = new()
					{
						// TransactionId = transactionId,
						DocCode = orderTransaction.DocCode,
						Comments = orderTransaction.Comments,
						DocDate = orderTransaction.DocDate,
						OrderId = orderId,
						OperationId = orderTransaction.OperationNo.ToStr(),
						EmployeeId = orderTransaction.EmployeeID,
						Operator = systemOperator.Id,
						Direction = 2, // 1 = MaterialIssue | 2 = MaterialReturn
						LogDate = DateTime.Now,
						Details = []
					};
					if (orderTransaction.Items.Count > 0)
					{
						OrderTransactionMaterialDetail itemDetail = null;
						orderTransaction.Items.ForEach(otItem =>
						{
							Component objItem = _componentOperation.GetComponentByCode(otItem.ItemCode);
							if (objItem is not null)
							{
								if (objItem.Status == Status.Failed)
								{
									throw new Exception(string.Format("Item {0} does not exist", otItem.ItemCode));
								}
								if (!wo.Components.Any(comp => comp.SourceId == otItem.ItemCode && otItem.LineID == otItem.LineID))
								{
								}
								if (objItem.ManagedBy == 1 && ((otItem.Lots?.Count > 0) || (otItem.SerialNumbers?.Count > 0)))
								{
									throw new Exception(string.Format("Item {0} cannot have lots nor serial numbers due its management", otItem.ItemCode));
								}
								if (objItem.ManagedBy == 2 && (otItem.Lots is null || otItem.Lots.Count == 0))
								{
									throw new Exception(string.Format("Item {0} lots are required due its management", otItem.ItemCode));
								}
								if (objItem.ManagedBy == 3 && (otItem.SerialNumbers is null || otItem.SerialNumbers.Count == 0))
								{
									throw new Exception(string.Format("Item {0} serial numbers are required due its management", otItem.ItemCode));
								}

								if (otItem.Lots.Count > 0)
								{
									decimal lotQty = otItem.Lots.Sum(s => s.Quantity);
									if (otItem.Quantity.ToDouble() != lotQty.ToDouble())
									{
										throw new Exception(string.Format("Item {0} Lot quantities must be equal to Line quantity", otItem.ItemCode));
									}
									otItem.Lots.ForEach(otItemLot =>
									{
										if (string.IsNullOrEmpty(otItemLot.LotNo))
										{
											throw new Exception(string.Format("Item {0} Lot number is required", otItem.ItemCode));
										}
										if (otItemLot.Quantity <= 0)
										{
											throw new Exception(string.Format("Item {0} Lot quantity must be greater than zero", otItem.ItemCode));
										}
										itemDetail = new OrderTransactionMaterialDetail
										{
											// TransactionId = transactionId,
											MachineId = null,
											OriginalItemId = null,
											ItemId = objItem.Id,
											BatchId = null,
											LineId = otItem.LineID.ToString(),
											LotNumber = otItemLot.LotNo,
											Pallet = otItemLot.Pallet,
											WarehouseCode = otItemLot.WarehouseCode,
											LocationCode = otItemLot.LocationCode,
											ExpDate = otItemLot.ExpDate,
											Quantity = Math.Abs(otItemLot.Quantity) * -1,
											InventoryStatus = otItemLot.InventoryStatusCode
										};
									});
								}
								else
								{
									if (otItem.Quantity == 0)
									{
										throw new Exception($"Item quantity is required: {otItem.ItemCode}");
									}
									itemDetail = new OrderTransactionMaterialDetail
									{
										// TransactionId = transactionId,
										MachineId = null,
										OriginalItemId = null,
										ItemId = objItem.Id,
										BatchId = null,
										LineId = otItem.LineID.ToString(),
										Quantity = Math.Abs(otItem.Quantity) * -1,
										WarehouseCode = otItem.WarehouseCode,
										Pallet = string.Empty,
										LocationCode = string.Empty,
										LotNumber = string.Empty,
										InventoryStatus = string.Empty
									};
								}
								if (itemDetail is not null && itemDetail.Quantity != 0)
								{
									orderTransactionInfo.Details.Add(itemDetail);
								}
							}
							else
							{
								throw new Exception($"Item code does not exist: {otItem.ItemCode}");
							}
						});
					}
					ResponseData response = MergeOrderTransactionMaterial(orderTransactionInfo, systemOperator, Validate);
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
			// 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.MaterialIssue, Action = ActionDB.IntegrateAll.ToStr() });
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
	public async Task<List<ResponseData>> ListUpdateMaterialIssue(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (OrderTransactionList?.Count > 0)
		{
			NotifyOnce = OrderTransactionList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (MaterialIssueExternal orderTransaction in OrderTransactionList)
			{
				Line++;
				try
				{
					BaseId = orderTransaction.DocCode;
					List<ValidationResult> results = [];
					ValidationContext context = new(orderTransaction, null, null);
					if (!Validator.TryValidateObject(orderTransaction, context, results))
					{
						throw new Exception($"{results[0]}");
					}
					string transactionId = Guid.NewGuid().ToString();
					// Order
					string orderId = string.Empty;
					WorkOrder wo = (await _workOrderOperation.GetWorkOrder(orderTransaction.OrderCode).ConfigureAwait(false)).FirstOrDefault();
					if (wo is not null)
					{
						orderId = wo.Id;
					}
					if (string.IsNullOrEmpty(orderId))
					{
						throw new Exception("Order Doesn't Exists");
					}

					if (wo.Processes.Find(x => x.ProcessId.ToDouble() == orderTransaction.OperationNo.ToDouble()) is null)
					{
						throw new Exception($"OperationNo is required for transaction in order {orderTransaction.OrderCode}");
					}
					// Data
					OrderTransactionMaterial orderTransactionInfo = new()
					{
						// TransactionId = transactionId,

						DocCode = orderTransaction.DocCode,
						Comments = orderTransaction.Comments,
						DocDate = orderTransaction.DocDate,
						OrderId = orderId,
						OperationId = orderTransaction.OperationNo.ToStr(),
						EmployeeId = orderTransaction.EmployeeID,
						Operator = systemOperator.Id,
						Direction = 1, // 1 = MaterialIssue | 2 = MaterialReturn
						LogDate = DateTime.Now,
						Details = []
					};
					if (orderTransaction.Items.Count > 0)
					{
						OrderTransactionMaterialDetail itemDetail = null;
						orderTransaction.Items.ForEach(otItem =>
						{
							Component objItem = _componentOperation.GetComponentByCode(otItem.ItemCode);
							if (objItem is not null)
							{
								if (objItem.Status == Status.Failed)
								{
									throw new Exception(string.Format("Item {0} does not exist", otItem.ItemCode));
								}
								if (!wo.Components.Any(comp => comp.SourceId == otItem.ItemCode && otItem.LineID == otItem.LineID))
								{
									throw new Exception(string.Format("Production order {0} does not have material {1} on LineNo {2}", wo.Id, otItem.ItemCode, otItem.LineID));
								}
								if (objItem.ManagedBy == 1 && ((otItem.Lots?.Count > 0) || (otItem.SerialNumbers?.Count > 0)))
								{
									throw new Exception(string.Format("Item {0} cannot have lots nor serial numbers due its management", otItem.ItemCode));
								}
								if (objItem.ManagedBy == 2 && (otItem.Lots is null || otItem.Lots.Count == 0))
								{
									throw new Exception(string.Format("Item {0} lots are required due its management", otItem.ItemCode));
								}
								if (objItem.ManagedBy == 3 && (otItem.SerialNumbers is null || otItem.SerialNumbers.Count == 0))
								{
									throw new Exception(string.Format("Item {0} serial numbers are required due its management", otItem.ItemCode));
								}
								if (otItem.Lots.Count > 0)
								{
									decimal lotQty = otItem.Lots.Sum(s => s.Quantity);
									if (otItem.Quantity.ToDouble() != lotQty.ToDouble())
									{
										throw new Exception(string.Format("Item {0} Lot quantities must be equal to Line quantity", otItem.ItemCode));
									}
									otItem.Lots.ForEach(otItemLot =>
									{
										if (string.IsNullOrEmpty(otItemLot.LotNo))
										{
											throw new Exception(string.Format("Item {0} Lot number is required", otItem.ItemCode));
										}
										if (otItemLot.Quantity <= 0)
										{
											throw new Exception(string.Format("Item {0} Lot quantity must be greater than zero", otItem.ItemCode));
										}
										itemDetail = new OrderTransactionMaterialDetail
										{
											// TransactionId = transactionId,
											MachineId = null,
											OriginalItemId = null,
											ItemId = objItem.Id,
											LineId = otItem.LineID.ToString(),
											LotNumber = otItemLot.LotNo,
											Pallet = otItemLot.Pallet,
											WarehouseCode = otItemLot.WarehouseCode,
											LocationCode = otItemLot.LocationCode,
											ExpDate = otItemLot.ExpDate,
											Quantity = otItem.Quantity
										};
									});
								}
								else
								{
									if (otItem.Quantity == 0)
									{
										throw new Exception($"Item quantity is required: {otItem.ItemCode}");
									}
									itemDetail = new OrderTransactionMaterialDetail
									{
										// TransactionId = transactionId,
										MachineId = null,
										OriginalItemId = null,
										ItemId = objItem.Id,
										BatchId = string.Empty,
										LotNumber = string.Empty,
										Pallet = string.Empty,
										LocationCode = string.Empty,
										InventoryStatus = string.Empty,
										LineId = otItem.LineID.ToString(),
										Quantity = otItem.Quantity,
										WarehouseCode = otItem.WarehouseCode
									};
								}
								if (itemDetail is not null && itemDetail.Quantity != 0)
								{
									orderTransactionInfo.Details.Add(itemDetail);
								}
							}
							else
							{
								throw new Exception($"Item code does not exist: {otItem.ItemCode}");
							}
						});
					}

					ResponseData response = MergeOrderTransactionMaterial(orderTransactionInfo, systemOperator, Validate);
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
			// 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.MaterialReturn, Action = ActionDB.IntegrateAll.ToStr() });
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
	public async Task<List<ResponseData>> ListUpdateMaterialScrap(List<MaterialIssueExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (OrderTransactionList?.Count > 0)
		{
			NotifyOnce = OrderTransactionList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (MaterialIssueExternal orderTransaction in OrderTransactionList)
			{
				Line++;
				try
				{
					BaseId = orderTransaction.DocCode;
					List<ValidationResult> results = [];
					ValidationContext context = new(orderTransaction, null, null);
					if (!Validator.TryValidateObject(orderTransaction, context, results))
					{
						throw new Exception($"{results[0]}");
					}
					string transactionId = Guid.NewGuid().ToString();
					// Order
					string orderId = string.Empty;
					WorkOrder wo = (await _workOrderOperation.GetWorkOrder(orderTransaction.OrderCode).ConfigureAwait(false)).FirstOrDefault();
					if (wo is not null)
					{
						orderId = wo.Id;
					}
					if (string.IsNullOrEmpty(orderId))
					{
						throw new Exception("Order Doesn't Exists");
					}

					if (wo.Processes.Find(x => x.ProcessId.ToDouble() == orderTransaction.OperationNo.ToDouble()) is null)
					{
						throw new Exception($"OperationNo is required for transaction in order {orderTransaction.OrderCode}");
					}
					// Data
					OrderTransactionMaterial orderTransactionInfo = new()
					{
						// TransactionId = transactionId,

						DocCode = orderTransaction.DocCode,
						Comments = orderTransaction.Comments,
						DocDate = orderTransaction.DocDate,
						OrderId = orderId,
						OperationId = orderTransaction.OperationNo.ToStr(),
						EmployeeId = orderTransaction.EmployeeID,
						Operator = systemOperator.Id,
						Direction = 3, // Scrap
						LogDate = DateTime.Now,
						Details = []
					};
					if (orderTransaction.Items.Count > 0)
					{
						OrderTransactionMaterialDetail itemDetail = null;
						orderTransaction.Items.ForEach(otItem =>
						{
							Component objItem = _componentOperation.GetComponentByCode(otItem.ItemCode);
							if (objItem is not null)
							{
								if (objItem.Status == Status.Failed)
								{
									throw new Exception(string.Format("Item {0} does not exist", otItem.ItemCode));
								}
								if (!wo.Components.Any(comp => comp.SourceId == otItem.ItemCode && otItem.LineID == otItem.LineID))
								{
									throw new Exception(string.Format("Production order {0} does not have material {1} on LineNo {2}", wo.Id, otItem.ItemCode, otItem.LineID));
								}
								if (objItem.ManagedBy == 1 && ((otItem.Lots?.Count > 0) || (otItem.SerialNumbers?.Count > 0)))
								{
									throw new Exception(string.Format("Item {0} cannot have lots nor serial numbers due its management", otItem.ItemCode));
								}
								if (objItem.ManagedBy == 2 && (otItem.Lots is null || otItem.Lots.Count == 0))
								{
									throw new Exception(string.Format("Item {0} lots are required due its management", otItem.ItemCode));
								}
								if (objItem.ManagedBy == 3 && (otItem.SerialNumbers is null || otItem.SerialNumbers.Count == 0))
								{
									throw new Exception(string.Format("Item {0} serial numbers are required due its management", otItem.ItemCode));
								}
								if (otItem.Lots.Count > 0)
								{
									decimal lotQty = otItem.Lots.Sum(s => s.Quantity);
									if (otItem.Quantity.ToDouble() != lotQty.ToDouble())
									{
										throw new Exception(string.Format("Item {0} Lot quantities must be equal to Line quantity", otItem.ItemCode));
									}
									otItem.Lots.ForEach(otItemLot =>
									{
										if (string.IsNullOrEmpty(otItemLot.LotNo))
										{
											throw new Exception(string.Format("Item {0} Lot number is required", otItem.ItemCode));
										}
										if (otItemLot.Quantity <= 0)
										{
											throw new Exception(string.Format("Item {0} Lot quantity must be greater than zero", otItem.ItemCode));
										}
										itemDetail = new OrderTransactionMaterialDetail
										{
											// TransactionId = transactionId,
											MachineId = null,
											OriginalItemId = null,
											ItemId = objItem.Id,
											LineId = otItem.LineID.ToString(),
											LotNumber = otItemLot.LotNo,
											Pallet = otItemLot.Pallet,
											WarehouseCode = otItemLot.WarehouseCode,
											LocationCode = otItemLot.LocationCode,
											ExpDate = otItemLot.ExpDate,
											Quantity = otItem.Quantity,
											Comments = otItemLot.Comments,
											ScrapTypeCode = otItemLot.ScrapTypeCode
										};
									});
								}
								else
								{
									if (otItem.Quantity == 0)
									{
										throw new Exception($"Item quantity is required: {otItem.ItemCode}");
									}
									itemDetail = new OrderTransactionMaterialDetail
									{
										// TransactionId = transactionId,
										MachineId = null,
										OriginalItemId = null,
										ItemId = objItem.Id,
										BatchId = string.Empty,
										LotNumber = string.Empty,
										Pallet = string.Empty,
										LocationCode = string.Empty,
										InventoryStatus = string.Empty,
										LineId = otItem.LineID.ToString(),
										Quantity = otItem.Quantity,
										WarehouseCode = otItem.WarehouseCode,
										Comments = otItem.Comments,
										ScrapTypeCode = otItem.ScrapTypeCode
									};
								}
								if (itemDetail is not null && itemDetail.Quantity != 0)
								{
									orderTransactionInfo.Details.Add(itemDetail);
								}
							}
							else
							{
								throw new Exception($"Item code does not exist: {otItem.ItemCode}");
							}
						});
					}

					ResponseData response = MergeOrderTransactionMaterial(orderTransactionInfo, systemOperator, Validate);
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
			// 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.MaterialReturn, Action = ActionDB.IntegrateAll.ToStr() });
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
}
