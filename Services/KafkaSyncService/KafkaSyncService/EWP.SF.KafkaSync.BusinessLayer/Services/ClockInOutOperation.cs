using System.ComponentModel.DataAnnotations;

using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;

using Newtonsoft.Json;

namespace EWP.SF.KafkaSync.BusinessLayer;

/// <summary>
/// Ports the 2503 monolith's Operations.EmployeeAttendance.ListUpdateCLockInOutBulk (validation + employee-code
/// resolution + SP_SF_CheckInOut_BLK merge) so the Kafka relay can sync ERP clock-in/out records.
/// </summary>
public class ClockInOutOperation : IClockInOutOperation
{
    private readonly IClockInOutRepo _clockInOutRepo;
    private readonly IEmployeeRepo _employeeRepo;

    public ClockInOutOperation(IClockInOutRepo clockInOutRepo, IEmployeeRepo employeeRepo)
    {
        _clockInOutRepo = clockInOutRepo;
        _employeeRepo = employeeRepo;
    }

    public List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> clockListOriginal, User systemOperator, bool Validate, LevelMessage Level)
    {
        List<ResponseData> returnValue = [];
        List<Employee> employeeList = _employeeRepo.ListEmployees(string.Empty, string.Empty);
        List<ClockInOutDetailsExternal> detailsToMerge = [];

        if (clockList?.Count > 0)
        {
            int Line = 0;
            string BaseId = string.Empty;
            foreach (ClockInOutDetailsExternal cycleDetail in clockList)
            {
                Line++;
                try
                {
                    BaseId = cycleDetail.ClockInOutId;

                    List<ValidationResult> results = [];
                    ValidationContext context = new(cycleDetail, null, null);

                    if (!Validator.TryValidateObject(cycleDetail, context, results) && results.Count > 0)
                    {
                        throw new Exception($"{results[0]}");
                    }
                    if (string.IsNullOrEmpty(cycleDetail.EmployeeCode))
                    {
                        throw new Exception("Employee code is required");
                    }

                    Employee employee = employeeList.Find(x => string.Equals(x.ExternalId, cycleDetail.EmployeeCode, StringComparison.OrdinalIgnoreCase));

                    if (employee is null)
                    {
                        employee = employeeList.Find(x => string.Equals(x.Code, cycleDetail.EmployeeCode, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        cycleDetail.EmployeeCode = employee.Code;
                    }

                    if (employee is null)
                    {
                        throw new Exception($"Employee code \"{cycleDetail.EmployeeCode}\" does not exist");
                    }
                    if (!cycleDetail.StartDate.HasValue)
                    {
                        throw new Exception("Start Date is required");
                    }

                    detailsToMerge.Add(cycleDetail);
                    returnValue.Add(new ResponseData
                    {
                        Code = cycleDetail.ClockInOutId,
                        IsSuccess = true,
                        Id = cycleDetail.ClockInOutId
                    });
                }
                catch (Exception ex)
                {
                    ResponseData messageError = new()
                    {
                        Id = BaseId,
                        Message = ex.Message,
                        Code = string.IsNullOrEmpty(cycleDetail.ClockInOutId) ? $"Line:{Line}" : cycleDetail.ClockInOutId
                    };
                    returnValue.Add(messageError);
                }
            }

            string itemsJson = JsonConvert.SerializeObject(detailsToMerge, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            _clockInOutRepo.MergeClockInOutBulk(itemsJson, systemOperator, Validate);
        }

        if (!Validate)
        {
            returnValue = Level switch
            {
                LevelMessage.Warning => [.. returnValue.Where(x => !string.IsNullOrEmpty(x.Message))],
                LevelMessage.Error => [.. returnValue.Where(x => !x.IsSuccess)],
                _ => returnValue
            };
        }

        return returnValue;
    }
}
