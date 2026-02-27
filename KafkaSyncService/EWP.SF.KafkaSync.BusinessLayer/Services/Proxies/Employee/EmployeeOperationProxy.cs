using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Employee operations targeting the Employee/HR Microservice.
/// </summary>
public class EmployeeOperationProxy : BaseHttpProxy, IEmployeeOperation
{
    public EmployeeOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public async Task<List<ResponseData>> ImportEmployeesAsync(
        List<EmployeeExternal> requestValue, 
        List<EmployeeExternal> originalValue, 
        User systemOperator, 
        bool Validate = false, 
        LevelMessage Level = 0, 
        bool NotifyOnce = true, 
        bool isDataSync = false,
        string logId = null)
    {
        // Endpoint: Employee/{validate}/{level}
        string endpoint = $"employee/{Validate.ToString().ToLower()}/{Level}";
        return await PostAsync<List<ResponseData>>(endpoint, requestValue).ConfigureAwait(false);
    }

    public async Task<List<ResponseData>> MRGEmployee(
        List<Employee> requestValue, 
        User systemOperator, 
        bool Validate = false, 
        LevelMessage Level = 0, 
        bool NotifyOnce = true, 
        bool isDataSync = false,
        string logId = null)
    {
        // Endpoint: Employee/Merge
        // Note: BaseHttpProxy.PostAsync handles the list serialization
        return await PostAsync<List<ResponseData>>("Employee/Merge", new
        {
            requestValue,
            systemOperator,
            Validate,
            NotifyOnce,
            isDataSync
        }).ConfigureAwait(false);
    }

    public List<Employee> GetEmployees(string id, string code, User systemOperator, DateTime? DeltaDate = null)
        => throw new NotSupportedException("GetEmployees is not available through the HTTP proxy.");
}
