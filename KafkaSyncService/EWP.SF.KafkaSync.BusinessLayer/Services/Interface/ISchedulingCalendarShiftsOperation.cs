using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface ISchedulingCalendarShiftsOperation
{
    bool DeleteSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator);
    SchedulingCalendarShifts UpdateSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator);
}
