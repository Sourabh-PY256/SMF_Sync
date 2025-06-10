using EWP.SF.Common.Models;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Helper;	

using System.ComponentModel.DataAnnotations;
using EWP.SF.Common.Constants;
namespace EWP.SF.Item.BusinessLayer;

public class OrderTransactionProductOperation : IOrderTransactionProductOperation
{
	private readonly IOrderTransactionProductRepo _orderTransactionProductRepo;

	private readonly IWorkOrderOperation _workOrderOperation;

	private readonly IComponentOperation _componentOperation;

	public OrderTransactionProductOperation(IOrderTransactionProductRepo orderTransactionProductRepo
	, IWorkOrderOperation workOrderOperation, IComponentOperation componentOperation)
	{
		_orderTransactionProductRepo = orderTransactionProductRepo;
		_workOrderOperation = workOrderOperation;
		_componentOperation = componentOperation;
	}
	/// <summary>
	/// Merges the order transaction product.
	/// </summary>
	/// <exception cref="UnauthorizedAccessException"></exception>
	public ResponseData MergeOrderTransactionProduct(OrderTransactionProduct orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
	{
		#region Permission validation

		if (!systemOperator.Permissions.Any(static x => x.Code == Permissions.PRD_PROCESS_ENTRY_MANAGE))
		{
			throw new UnauthorizedAccessException(ErrorMessage.noPermission);
		}

		#endregion Permission validation

		return _orderTransactionProductRepo.MergeOrderTransactionProduct(orderTransactionInfo, systemOperator, Validate);
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
	/// Update Product Receipt
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateProductReceipt(List<ProductReceiptExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (OrderTransactionList?.Count > 0)
		{
			NotifyOnce = OrderTransactionList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (ProductReceiptExternal orderTransaction in OrderTransactionList)
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
					if (wo.Processes.FirstOrDefault(x => x.ProcessId.ToDouble() == orderTransaction.OperationNo.ToDouble()) is null)
					{
						throw new Exception($"OperationNo is required for transaction in order {orderTransaction.OrderCode}");
					}
					// Data
					OrderTransactionProduct orderTransactionInfo = new()
					{
						// TransactionId = transactionId,
						DocCode = orderTransaction.DocCode,
						Comments = orderTransaction.Comments,
						DocDate = orderTransaction.DocDate,
						OrderId = orderId,
						OperationId = orderTransaction.OperationNo.ToStr(),
						EmployeeId = orderTransaction.EmployeeID,
						Operator = systemOperator.Id,
						Direction = 1, // 1 = Receipt | 2 = Return
						LogDate = DateTime.Now,
						Details = []
					};
					if (orderTransaction.Items.Count > 0)
					{
						OrderTransactionProductDetail itemDetail = null;
						foreach (ItemProdTransactionExternal otItem in orderTransaction.Items)
						{
							Component objItem = _componentOperation.GetComponentByCode(otItem.ItemCode);
							if (objItem is not null)
							{
								if (objItem.Status == Status.Failed)
								{
									throw new Exception($"Item code does not exist: {otItem.ItemCode}");
								}
								if (otItem.Lots.Count > 0)
								{
									foreach (ItemLotProdTransactionExternal otItemLot in otItem.Lots)
									{
										itemDetail = new OrderTransactionProductDetail
										{
											MachineId = null,
											ItemId = objItem.Id,
											LineId = otItem.LineID.ToString(),
											LotNumber = otItemLot.LotNo,
											LocationCode = otItemLot.LocationCode,
											Quantity = otItemLot.Quantity,
											Warehouse = otItemLot.WarehouseCode
										};
										if (itemDetail.Quantity != 0)
										{
											orderTransactionInfo.Details.Add(itemDetail);
										}
									}
								}
								else
								{
									if (otItem.Quantity == 0)
									{
										throw new Exception($"Item quantity is required: {otItem.ItemCode}");
									}
									itemDetail = new OrderTransactionProductDetail
									{
										// TransactionId = transactionId,
										MachineId = null,
										ItemId = objItem.Id,
										LineId = otItem.LineID.ToString(),
										Quantity = otItem.Quantity,
										Warehouse = otItem.WarehouseCode
									};
									if (itemDetail is not null && itemDetail.Quantity != 0)
									{
										orderTransactionInfo.Details.Add(itemDetail);
									}
								}
							}
							else
							{
								throw new Exception($"Item code does not exist: {otItem.ItemCode}");
							}
						}
					}
					ResponseData response = MergeOrderTransactionProduct(orderTransactionInfo, systemOperator, Validate);
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
			// 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductReceipt, Action = ActionDB.IntegrateAll.ToStr() });
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
	/// Update Product Return
	/// </summary>
	public async Task<List<ResponseData>> ListUpdateProductReturn(List<ProductReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level)
	{
		List<ResponseData> returnValue = [];
		ResponseData MessageError;
		bool NotifyOnce = true;
		if (OrderTransactionList?.Count > 0)
		{
			NotifyOnce = OrderTransactionList.Count == 1;
			int Line = 0;
			string BaseId = string.Empty;
			foreach (ProductReturnExternal orderTransaction in OrderTransactionList)
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
					if (wo.Processes.FirstOrDefault(x => x.ProcessId.ToDouble() == orderTransaction.OperationNo.ToDouble()) is null)
					{
						throw new Exception($"OperationNo is required for transaction in order {orderTransaction.OrderCode}");
					}
					// Data
					OrderTransactionProduct orderTransactionInfo = new()
					{
						// TransactionId = transactionId,
						DocCode = orderTransaction.DocCode,
						Comments = orderTransaction.Comments,
						DocDate = orderTransaction.DocDate,
						OrderId = orderId,
						OperationId = orderTransaction.OrderCode.ToStr(),
						EmployeeId = orderTransaction.EmployeeID,
						Operator = systemOperator.Id,
						Direction = 2, // 1 = Receipt | 2 = Return
						LogDate = DateTime.Now,
						Details = []
					};
					if (orderTransaction.Items.Count > 0)
					{
						OrderTransactionProductDetail itemDetail = null;
						foreach (ItemProdRetTransactionExternal otItem in orderTransaction.Items)
						{
							Component objItem = _componentOperation.GetComponentByCode(otItem.ItemCode);
							if (objItem is not null)
							{
								if (objItem.Status == Status.Failed)
								{
									throw new Exception($"Item code does not exist: {otItem.ItemCode}");
								}
								if (otItem.Lots is null || otItem.Lots.Count == 0)
								{
									throw new Exception("Product return lots are missing");
								}
								if (otItem.Lots.Count > 0)
								{
									foreach (ItemLotProdRetTransactionExternal otItemLot in otItem.Lots)
									{
										itemDetail = new OrderTransactionProductDetail
										{
											// TransactionId = transactionId,
											MachineId = null,
											ItemId = objItem.Id,
											LineId = otItem.LineID.ToString(),
											LotNumber = otItemLot.LotNo,
											LocationCode = otItemLot.LocationCode,
											Quantity = otItemLot.Quantity * -1,
											Warehouse = otItemLot.WarehouseCode,
										};
									}
								}
								else
								{
									if (otItem.Quantity == 0)
									{
										throw new Exception($"Item quantity is required: {otItem.ItemCode}");
									}
									itemDetail = new OrderTransactionProductDetail
									{
										// TransactionId = transactionId,
										MachineId = null,
										ItemId = objItem.Id,
										LineId = otItem.LineID.ToString(),
										Quantity = otItem.Quantity * -1,
										LotNumber = string.Empty,
										Warehouse = otItem.WarehouseCode
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
						}
					}
					ResponseData response = MergeOrderTransactionProduct(orderTransactionInfo, systemOperator, Validate);
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
			// 	ServiceManager.SendMessage(MessageBrokerType.CatalogChanged, new { Catalog = Entities.ProductReturn, Action = ActionDB.IntegrateAll.ToStr() });
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
