using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IStockOperation
{
    ResponseData ListUpdateStockBulk(List<StockExternal> stockList, User systemOperator, bool Validate, LevelMessage Level);
    

}