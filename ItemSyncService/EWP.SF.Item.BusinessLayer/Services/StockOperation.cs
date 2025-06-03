using EWP.SF.Common.Models;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using Newtonsoft.Json;


namespace EWP.SF.Item.BusinessLayer;


public class StockOperation : IStockOperation
{
    private readonly IStockRepo _stockRepo;

    public StockOperation(IStockRepo stockRepo)
    {
        _stockRepo = stockRepo;
    }

    /// <summary>
    ///
    /// </summary>
    public ResponseData ListUpdateStockBulk(List<StockExternal> stockList, User systemOperator, bool Validate, LevelMessage Level)
    {
        ResponseData returnValue = new();

        if (stockList?.Count > 0 && !Validate)
        {
            if (!Validate)
            {
                returnValue = _stockRepo.MergeStock_Bulk(JsonConvert.SerializeObject(stockList));
                returnValue.Entity = "Stock";
            }
            else
            {
                returnValue = new ResponseData
                {
                    Action = ActionDB.IntegrateAll,
                    Entity = "Stock",
                    Message = "",
                    IsSuccess = true
                };
            }
        }

        return returnValue;
    }
}