using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IOrderTransactionMaterialRepo
{
    ResponseData MergeOrderTransactionMaterial(OrderTransactionMaterial OrderMaterialInfo, User systemOperator, bool Validation, IntegrationSource intSrc = IntegrationSource.ERP);

}
    
