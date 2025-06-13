using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface ISchedulingShiftStatusOperation
{
    Task<List<ResponseData>> UpdateSchedulingShiftStatus(List<SchedulingShiftStatus> requestValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true);

}
