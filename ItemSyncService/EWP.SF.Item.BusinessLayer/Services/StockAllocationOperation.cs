using EWP.SF.Common.Models;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.ResponseModels;
using Newtonsoft.Json;


namespace EWP.SF.Item.BusinessLayer;


public class StockAllocationOperation : IStockAllocationOperation
{
    private readonly IStockAllocationRepo _stockallocationRepo;

    public StockAllocationOperation(IStockAllocationRepo stockallocationRepo)
    {
        _stockallocationRepo = stockallocationRepo;
    }

    /// <summary>
    ///
    /// </summary>
   /// <summary>
	///
	/// </summary>
	public ResponseData ListUpdateAllocationBulk(StockAllocationExternal[] stockList, User systemOperator, bool Validate, LevelMessage Level, bool nodelete = false)
	{
		ResponseData returnValue = new();

		if (stockList?.Length > 0 && !Validate)
		{
			if (!Validate)
			{
				returnValue = _stockallocationRepo.MergeStockAllocationBulk(JsonConvert.SerializeObject(stockList), systemOperator, Validate, nodelete);
				returnValue.Entity = "StockAllocation";
			}
			else
			{
				returnValue = new ResponseData
				{
					Action = ActionDB.IntegrateAll,
					Entity = "StockAllocation",
					Message = "",
					IsSuccess = true
				};
			}
		}

		return returnValue;
	}
}