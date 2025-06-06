using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface IProductionLinesRepo
{
    ResponseData CreateProductionLine(ProductionLine productionLineInfo, User systemOperator, bool Validate = false, string Level = "Success");
    bool DeleteProductionLine(ProductionLine productionLineInfo, User systemOperator);
    ProductionLine GetProductionLine(string Code);
    List<ProductionLine> ListProductionLines(DateTime? DeltaDate = null);
}
