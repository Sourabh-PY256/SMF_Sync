using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface ILotSerialStatusOperation
{
    List<LotSerialStatus> ListLotSerialStatus(User systemOperator, string LotSerialStatusCode = "", DateTime? DeltaDate = null);
    Task<ResponseData> MergeLotSerialStatus(LotSerialStatus LotSerialStatusInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true);
    Task<List<ResponseData>> ListUpdateLotSerialStatus(List<LotSerialStatusExternal> lotSerialStatusList, List<LotSerialStatusExternal> lotSerialStatusListOriginal, User systemOperator, bool Validate, LevelMessage Level);


}