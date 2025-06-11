using EWP.SF.KafkaSync.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models;

namespace EWP.SF.KafkaSync.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface ISchedulingCalendarShiftsRepo
{
    SchedulingCalendarShifts PutSchedulingCalendarShifts(SchedulingCalendarShifts request, User systemOperator);

    List<SchedulingCalendarShifts> GetSchedulingCalendarShifts(string Id, string AssetCode, string IdParent, int AssetLevel, string AssetLevelCode, string Origin = null);

    bool DeleteSchedulingCalendarShifts(SchedulingCalendarShifts request);
}