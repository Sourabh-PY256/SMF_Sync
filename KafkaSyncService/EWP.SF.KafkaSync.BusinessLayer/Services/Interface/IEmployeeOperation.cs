using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface IEmployeeOperation
{
    Task<List<ResponseData>> ImportEmployeesAsync(List<EmployeeExternal> requestValue, List<EmployeeExternal> originalValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true, bool isDataSync = false);
    Task<List<ResponseData>> MRGEmployee(List<Employee> requestValue, User systemOperator, bool Validate = false, LevelMessage Level = 0, bool NotifyOnce = true, bool isDataSync = false);

    List<Employee> GetEmployees(string id, string code, User systemOperator, DateTime? DeltaDate = null);

}