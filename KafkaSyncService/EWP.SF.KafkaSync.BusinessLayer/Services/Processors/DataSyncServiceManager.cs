using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer;

public class DataSyncServiceManager
{
	private readonly ILogger<DataSyncServiceManager> _logger;
	private readonly IDataSyncServiceOperation _operations;
	private readonly IServiceConsumerManager _serviceConsumerManager;

	public DataSyncServiceManager(
		ILogger<DataSyncServiceManager> logger,
		IDataSyncServiceOperation operations,
		IServiceConsumerManager serviceConsumerManager)
	{
		_logger = logger;
		_operations = operations;
		_serviceConsumerManager = serviceConsumerManager;
	}

	public  async Task InsertDataSyncServiceLog(string serviceName, string ErrorMessage, User systemOperator)
	{
		DataSyncService servicedata = await _operations.GetBackgroundService(serviceName, "GET").ConfigureAwait(false);
		_ = await _operations.InsertDataSyncServiceLog(new DataSyncServiceLog
		{
			Id = Guid.NewGuid().ToString(),
			ServiceException = ErrorMessage,
			ExecutionInitDate = DateTime.UtcNow,
			LogUser = systemOperator.Id,
			LogEmployee = systemOperator.EmployeeId,
			ServiceInstanceId = servicedata.Id,
			ExecutionOrigin = ServiceExecOrigin.KafkaProducer,
			SuccessRecords = 0,
			FailedRecords = 0
		}).ConfigureAwait(false);
	}

	public class ServiceValidationResult
{
    public int Status { get; set; } // 1 = OK, 0 = Disabled, 2 = Running, -1 = Not found, etc.
    public string Message { get; set; }
    public DataSyncService ServiceData { get; set; }
}

public async Task<ServiceValidationResult> ValidateAndGetService(
    string serviceType, 
    TriggerType trigger, 
    ServiceExecOrigin execOrigin, 
    string httpMethod = "GET")
{
    var result = new ServiceValidationResult();
    var dataService = await _operations.GetBackgroundService(serviceType, httpMethod.ToUpperInvariant()).ConfigureAwait(false);

    if (dataService == null)
    {
        result.Status = -1;
        result.Message = "Service does not exist!";
        return result;
    }

    // EnableType enable = EnableType.No;
    // if (trigger == TriggerType.Erp)
    //     enable = dataService.ErpTriggerEnable;
    // else if (trigger == TriggerType.SmartFactory)
    //     enable = execOrigin == ServiceExecOrigin.Event ? dataService.SfTriggerEnable : dataService.ManualSyncEnable;

    // if (enable != EnableType.Yes)
    // {
    //     result.Status = 0;
    //     result.Message = "Service is disabled";
    //     return result;
    // }

    if (dataService.Status != ServiceStatus.Active)
    {
        result.Status = 0;
        result.Message = "Service is not active";
        return result;
    }

    result.Status = 1;
    result.Message = "Service execution request accepted";
    result.ServiceData = dataService;
    return result;
}

	
	// }
}
