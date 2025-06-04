using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IOrderTransactionProductRepo
{
        ResponseData MergeOrderTransactionProduct(OrderTransactionProduct OrderProductInfo, User systemOperator, bool Validation, IntegrationSource intSrc = IntegrationSource.ERP);
        ResponseData MergeOrderTransactionProductStatus(OrderTransactionProductStatus OrderProductInfo, User systemOperator, bool Validation, IntegrationSource intSrc = IntegrationSource.ERP);
        

}
    