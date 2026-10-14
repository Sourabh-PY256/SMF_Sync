using EWP.SF.KafkaSync.BusinessEntities.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IStockRepo

{
    ResponseData MergeStock_Bulk(string request);
}