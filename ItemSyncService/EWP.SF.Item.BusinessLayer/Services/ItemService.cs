using EWP.SF.Item.BusinessEntities;
using EWP.SF.Helper;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EWP.SF.Common.Models;

namespace EWP.SF.Item.BusinessLayer;

public class ItemService :BackgroundService,IItemService 
{
	private readonly ILogger<ItemService> _logger;
	private readonly IServiceScopeFactory _serviceFactory; 
	private  IDataSyncServiceOperation _operations ;
	private readonly bool _runSyncServicesOnInit ;
	private DataSyncService _serviceData;
	private PeriodicTimer _timer;
	public ItemService(ILogger<ItemService> logger, IServiceScopeFactory serviceFactory, IApplicationSettings settings,
	IDataSyncServiceOperation operations)
	{
		_logger = logger;
		_serviceFactory = serviceFactory;
		_operations = operations;
		_runSyncServicesOnInit = settings.GetAppSetting("RunSyncServicesOnInit").ToBool();

    }
	public void SetServiceData(DataSyncService Data)
	{
		if (Data is null)
		{
			_serviceData = null;
		}
		else if (string.Equals(Data.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
		{
			_serviceData = Data;
			_timer = new PeriodicTimer(TimeSpan.FromMinutes(_serviceData.FrequencyMin));
		}
	}

	public async Task<DataSyncHttpResponse> ManualExecution(DataSyncService Data, TriggerType Trigger, ServiceExecOrigin ExecOrigin, User SystemOperator, string EntityCode, string BodyData)
	{
		DataSyncHttpResponse response = new();
		string serviceType = string.Empty;
		try
		{
			EnableType Enable = EnableType.No;
			if (Trigger == TriggerType.Erp)
			{
				serviceType = "ERP";
				Enable = Data.ErpTriggerEnable;
			}
			else if (Trigger == TriggerType.SmartFactory || Trigger == TriggerType.DataSyncSettings)
			{
				serviceType = "Smart Factory";
				if (ExecOrigin == ServiceExecOrigin.Event)
				{
					Enable = Data.SfTriggerEnable;
				}
				else
				{
					Enable = Data.ManualSyncEnable;
					serviceType += " Manual";
				}
			}
			//
			if (Enable == EnableType.Yes)
			{
				if (Data.Status == ServiceStatus.Active && !ContextCache.IsServiceRunning(Data.Id))
				{
					await using AsyncServiceScope serviceScope = _serviceFactory.CreateAsyncScope();
					DataSyncServiceProcessor DataSyncProcesser = serviceScope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
					Task<DataSyncHttpResponse> taskService = DataSyncProcesser.ExecuteService(Data, ExecOrigin, Trigger, SystemOperator, EntityCode, BodyData);
					response = await taskService.ConfigureAwait(false);
				}
				else
				{
					response.StatusCode = System.Net.HttpStatusCode.Conflict;
					response.Message = $"{serviceType} {(ContextCache.IsServiceRunning(Data.Id) ? "is being executing" : "status is disabled")}";
				}
			}
			else
			{
				response.StatusCode = System.Net.HttpStatusCode.Conflict;
				response.Message = $"{serviceType} trigger is not enabled";
			}
		}
		catch (Exception ex)
		{
			response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
			response.Message = $"Service Error: {ex.Message}.";
			_logger.LogInformation("Service Error: {message}.", ex.Message);
		}
		return response;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_serviceData = await _operations.GetBackgroundService(BackgroundServices.ITEM_SERVICE).ConfigureAwait(false);
		if (_serviceData?.FrequencyMin > 0)
		{
			_timer = new PeriodicTimer(TimeSpan.FromMinutes(_serviceData.FrequencyMin));
		}
		if (_serviceData is not null && _runSyncServicesOnInit)
		{
			ContextCache.SetRunningService(_serviceData.Id, false);
			await _operations.DatasyncTempServiceLogAsync(_serviceData.EntityId, BackgroundServiceExecType.Start).ConfigureAwait(false);
			do
			{
				try
				{
					if (_serviceData is not null && _serviceData.Status == ServiceStatus.Active && !ContextCache.IsServiceRunning(_serviceData.Id) && _serviceData.TimeTriggerEnable == EnableType.Yes)
					{
						await _operations.DatasyncTempServiceLogAsync(_serviceData.EntityId, BackgroundServiceExecType.Process, "").ConfigureAwait(false);
						await using AsyncServiceScope serviceScope = _serviceFactory.CreateAsyncScope();
						DataSyncServiceProcessor DataSyncProcessor = serviceScope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
						_ = await DataSyncProcessor.ExecuteService(_serviceData).ConfigureAwait(false);
					}
					else if (_serviceData is not null)
					{
						await _operations.DatasyncTempServiceLogAsync(_serviceData.EntityId, BackgroundServiceExecType.Process, "Service is Disabled").ConfigureAwait(false);
					}
				}
				catch (Exception ex)
				{
					await _operations.DatasyncTempServiceLogAsync(_serviceData.EntityId, BackgroundServiceExecType.Process, ex.Message).ConfigureAwait(false);
					_logger.LogInformation("Service Error: {message}.", ex.Message);
				}
				if (!stoppingToken.IsCancellationRequested)
				{
					try
					{
						_ = await _timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
					}
					catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
					{
						// Log if necessary or handle graceful shutdown
					}
				}
			} while (!stoppingToken.IsCancellationRequested);
			await _operations.DatasyncTempServiceLogAsync(_serviceData.EntityId, BackgroundServiceExecType.End).ConfigureAwait(false);
		}
	}

}
