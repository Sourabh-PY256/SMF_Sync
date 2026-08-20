using EWP.SF.Common.Models;

namespace EWP.SF.KafkaSync.DataAccess;

/// <summary>
/// Interface for managing clock-in/clock-out data access operations.
/// </summary>
public interface IClockInOutRepo
{
    void MergeClockInOutBulk(string Json, User systemOperator, bool Validate);
}
