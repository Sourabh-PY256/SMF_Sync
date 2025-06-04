using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

/// Interface for managing work center data access operations
/// </summary>
public interface ILotSerialStatusRepo
{
    List<LotSerialStatus> ListLotSerialStatus(string LotSerialStatusCode, DateTime? DeltaDate = null);
    ResponseData MergeLotSerialStatus(LotSerialStatus LotSerialStatusInfo, User systemOperator, bool Validation);
    
}