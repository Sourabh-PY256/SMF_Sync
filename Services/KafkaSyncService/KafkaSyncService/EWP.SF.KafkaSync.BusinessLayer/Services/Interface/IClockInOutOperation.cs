using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IClockInOutOperation
{
    List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> clockListOriginal, User systemOperator, bool Validate, LevelMessage Level);
}
