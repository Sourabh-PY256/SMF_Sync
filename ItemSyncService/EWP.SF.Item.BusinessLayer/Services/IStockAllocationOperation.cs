using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.BusinessLayer;

public interface IStockAllocationOperation
{
    ResponseData ListUpdateAllocationBulk(StockAllocationExternal[] stockList, User systemOperator, bool Validate, LevelMessage Level, bool nodelete = false);
    

}