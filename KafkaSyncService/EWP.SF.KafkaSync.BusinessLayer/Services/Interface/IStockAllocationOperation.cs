using EWP.SF.Common.Enumerators;
using EWP.SF.KafkaSync.BusinessEntities.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IStockAllocationOperation
{
    ResponseData ListUpdateAllocationBulk(StockAllocationExternal[] stockList, User systemOperator, bool Validate, LevelMessage Level, bool nodelete = false);
    

}