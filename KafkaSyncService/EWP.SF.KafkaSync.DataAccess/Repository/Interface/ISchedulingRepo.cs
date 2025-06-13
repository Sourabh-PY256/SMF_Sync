
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models;

namespace EWP.SF.KafkaSync.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface ISchedulingRepo
{
    List<SchedulingShiftStatus> GetSchedulingShiftStatus(string Code, string Type, DateTime? DeltaDate = null);

    ResponseData PutSchedulingShiftStatus(SchedulingShiftStatus request, User systemOperator, bool Validation);

    List<SchedulingShift> GetSchedulingShift(string Code, string Type, DateTime? DeltaDate = null);

    ResponseData PutSchedulingShift(SchedulingShift request, User systemOperator, bool Validation);

    void GeneratePositionShiftExplosion(string EmployeeCode);


   
}