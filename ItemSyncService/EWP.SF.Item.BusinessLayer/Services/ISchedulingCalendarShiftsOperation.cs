using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;

namespace EWP.SF.Item.BusinessLayer;

public interface ISchedulingCalendarShiftsOperation
{
    bool DeleteSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator);
    SchedulingCalendarShifts UpdateSchedulingCalendarShifts(SchedulingCalendarShifts requestValue, User systemOperator);
}
