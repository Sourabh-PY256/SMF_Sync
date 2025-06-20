﻿using EWP.SF.Common.Models;
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

	public  async Task<int> ValidateExecuteService(string ServiceType, TriggerType Trigger, ServiceExecOrigin ExecOrigin, string HttpMethod = "GET")
	{
		int returnValue = 0;
		DataSyncService _dataService = await _operations.GetBackgroundService(ServiceType, HttpMethod.ToUpperInvariant()).ConfigureAwait(false);
		if (_dataService is null)
		{
			return -1;
		}
		EnableType Enable = EnableType.No;
		if (Trigger == TriggerType.Erp)
		{
			Enable = _dataService.ErpTriggerEnable;
		}
		else if (Trigger == TriggerType.SmartFactory)
		{
			if (ExecOrigin == ServiceExecOrigin.Event)
			{
				Enable = _dataService.SfTriggerEnable;
			}
			else
			{
				Enable = _dataService.ManualSyncEnable;
			}
		}
		if (Enable == EnableType.Yes && _dataService.Status == ServiceStatus.Active)
		{
			returnValue = 1;
		}
		
		return returnValue;
	}

	
	// }
}
