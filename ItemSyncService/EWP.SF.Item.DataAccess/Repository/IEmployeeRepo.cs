using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EWP.SF.Common.ResponseModels;

namespace EWP.SF.Item.DataAccess;

/// <summary>
/// Interface for managing work center data access operations
/// </summary>
public interface IEmployeeRepo
{
    List<EmployeeSkills> CreateEmployeeSkills(string employeeId, string XML, User systemOperator);
    List<EmployeeContractsDetail> CreateEmployeeContractsDetail(string employeeId, string XML, User systemOperator);
    List<EmployeeSkills> EmployeeSkillsList(string id);
    List<EmployeeContractsDetail> ContractsDetailList(string id);
    Employee GetEmployee(string id);
    ResponseData MRGEmployee(Employee employee, bool Validation, User systemOperator);

    List<Employee> ListEmployees(string employeeId, string code, DateTime? DeltaDate = null);
}
