using EWP.SF.Common.Models;
using EWP.SF.Item.BusinessEntities;

namespace EWP.SF.Item.BusinessLayer;

public class DataSyncServiceManager
{
	private readonly IItemService _itemService;
	private  IDataSyncServiceOperation _operations;
	//	private readonly IoTDataSimulatorService _ioTDataSimulatorService;
	public DataSyncServiceManager(IItemService itemService,
	IDataSyncServiceOperation dataSyncService)
	//IoTDataSimulatorService ioTDataSimulatorService)
	{
		_operations = dataSyncService;
		_itemService = itemService;
		//_ioTDataSimulatorService = ioTDataSimulatorService;
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
			ExecutionOrigin = ServiceExecOrigin.Webhook,
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
		if (ContextCache.IsServiceRunning(_dataService.Id))
		{
			returnValue = 2;
		}
		return returnValue;
	}

	public async Task<DataSyncHttpResponse> ExecuteService(string ServiceType, TriggerType Trigger, ServiceExecOrigin ExecOrigin, User SystemOperator = null, string HttpMethod = "GET", string EntityCode = "", string BodyData = "")
	{
		DataSyncHttpResponse response = new();
		DataSyncService Data = await _operations.GetBackgroundService(ServiceType, HttpMethod.ToUpperInvariant()).ConfigureAwait(false);
		if (Data is null)
		{
			response.StatusCode = System.Net.HttpStatusCode.NotFound;
			response.Message = "No service instance found";
		}
		else
		{
			response = ServiceType switch
			{
				
				BackgroundServices.ITEM_SERVICE => await _itemService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				
				_ => new DataSyncHttpResponse
				{
					StatusCode = System.Net.HttpStatusCode.NotImplemented,
					Message = "Service is not implemented",
				},
			};
		}
		return response;
	}

	public void UpdateServiceData(string ServiceType, DataSyncService Data)
	{
		switch (ServiceType)
		{
			
			case BackgroundServices.ITEM_SERVICE:
				_itemService.SetServiceData(Data);
				break;
			
		}
	}
}
