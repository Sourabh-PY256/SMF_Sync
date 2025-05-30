using EWP.SF.Item.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface ISchedulingShiftStatusRepo
{
    List<SchedulingShiftStatus> GetSchedulingShiftStatus(string Code, string Type, DateTime? DeltaDate = null);

    ResponseData PutSchedulingShiftStatus(SchedulingShiftStatus request, User systemOperator, bool Validation);
   
}