using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IProductionLinesOperation
{
    ProductionLine[] ListProductionLines(bool deleted = false, DateTime? DeltaDate = null);
    ProductionLine GetProductionLine(string Code, User systemOperator);
    //Task<ResponseData> CreateProductionLine(ProductionLine productionLineInfo, User systemOperator, bool Validate = false, string Level = "Success", bool NotifyOnce = true);
    bool DeleteProductionLine(ProductionLine productionLineInfo, User systemOperator);
    
}