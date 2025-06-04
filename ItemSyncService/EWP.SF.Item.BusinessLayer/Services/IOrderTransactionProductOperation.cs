using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IOrderTransactionProductOperation
{
    ResponseData MergeOrderTransactionProduct(OrderTransactionProduct orderTransactionInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true);
    Task<List<ResponseData>> ListUpdateProductReceipt(List<ProductReceiptExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level);
    Task<List<ResponseData>> ListUpdateProductReturn(List<ProductReturnExternal> OrderTransactionList, User systemOperator, bool Validate, LevelMessage Level);

    
}   