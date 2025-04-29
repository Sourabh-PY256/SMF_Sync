

using Microsoft.AspNetCore.Mvc;
using  EWP.SF.Item.BusinessLayer;
using EWP.SF.Item.BusinessEntities;
using EWP.SF.Common.Models;
namespace EWP.SF.Item.API;

public partial class DataSyncController : BaseController
{
	private readonly ILogger<DataSyncController> _logger;
	private readonly IDataSyncService _DataSyncService;

    public DataSyncController(ILogger<DataSyncController> logger, IDataSyncService DataSyncService)
    {
        _logger = logger;
		_DataSyncService = DataSyncService;
    }
	#region DataSync

	/// <summary>
	/// Data Sync ERP Catalog info in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Catalog/Erp")]
	public async Task<ResponseModel> ListDataSyncErp([FromQuery] DataSyncCatalogFilter SearchFilter)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.ListDataSyncErp(context.User, SearchFilter);

		return returnValue;
	}

	/// <summary>
	/// Data Sync ERP Version Catalog info in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Catalog/Erp/Version")]
	public async Task<ResponseModel> ListDataSyncErpVersion([FromQuery] DataSyncCatalogFilter SearchFilter)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.ListDataSyncErpVersion(context.User, SearchFilter);

		return returnValue;
	}

	/// <summary>
	/// Data Sync ERP Database Catalog info in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Catalog/Erp/Database")]
	public async Task<ResponseModel> ListDataSyncErpDatabase([FromQuery] DataSyncCatalogFilter SearchFilter)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.ListDataSyncErpDatabase(context.User, SearchFilter);

		return returnValue;
	}

	/// <summary>
	/// Data Sync ERP Database Version Catalog info in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Catalog/Erp/Database/Version")]
	public async Task<ResponseModel> ListDataSyncErpDatabaseVersion([FromQuery] DataSyncCatalogFilter SearchFilter)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.ListDataSyncErpDatabaseVersion(context.User, SearchFilter);

		return returnValue;
	}

	/// <summary>
	/// Data Sync ERP Manufacturing Catalog info in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Catalog/Erp/Manufacturing")]
	public async Task<ResponseModel> ListDataSyncErpManufacturing([FromQuery] DataSyncCatalogFilter SearchFilter)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.ListDataSyncErpManufacturing(context.User, SearchFilter);

		return returnValue;
	}

	/// <summary>
	/// Data Sync ERP Manufacturing Version Catalog info in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Catalog/Erp/Manufacturing/Version")]
	public async Task<ResponseModel> ListDataSyncErpManufacturingVersion([FromQuery] DataSyncCatalogFilter SearchFilter)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.ListDataSyncErpManufacturingVersion(context.User, SearchFilter);

		return returnValue;
	}

	/// <summary>
	/// Data Sync ERP Instance Category Catalog info in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Catalog/Instance/Category")]
	public async Task<ResponseModel> ListDataSyncInstanceCategory([FromQuery] DataSyncCatalogFilter SearchFilter)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.ListDataSyncInstanceCategory(context.User, SearchFilter);

		return returnValue;
	}

	/// <summary>
	/// One/List Data Sync ERP(s) in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Erp/{Id?}")]
	public async Task<ResponseModel> ListDataSyncERP([FromRoute] string Id = "")
	{
		ResponseModel returnValue = new()
		{
			Data = _DataSyncService.ListDataSyncERP(Id, EnableType.Yes)
		};

		return returnValue;
	}

	/// <summary>
	/// One/List Data Sync Instance(s) in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Instance/{Id?}")]	public async Task<ResponseModel> GetServiceInstanceFullData([FromRoute] string Id)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.GetServiceInstancesFullData(context.User, Id);

		return returnValue;
	}

	/// <summary>
	/// Create/Update a Data Sync ERP
	/// </summary>
	/// <returns></returns>
	[HttpPost("DataSyncService/Erp")]
	public async Task<ResponseModel> MergeDataSyncERP([FromBody] DataSyncErp Request)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.MergeDataSyncERP(context.User, Request);

		return returnValue;
	}

	/// <summary>
	/// One/List Data Sync Service(s) in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/{Trigger?}/{Id?}")]
	public async Task<ResponseModel> ListDataSyncService([FromRoute] int Trigger, [FromRoute] string Id)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.ListDataSyncService(context.User, Id, (TriggerType)Trigger);

		return returnValue;
	}

	/// <summary>
	/// Update a Data Sync Service Instance
	/// </summary>
	/// <returns></returns>
	[HttpPost("DataSyncService/Instance")]
	public async Task<ResponseModel> MergeDataSyncService([FromServices] DataSyncServiceManager ServiceManager, [FromBody] DataSyncService DataSyncInfo)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		BusinessEntities.DataSyncService responseData = _DataSyncService.MergeDataSyncService(context.User, DataSyncInfo);
		ServiceManager.UpdateServiceData(responseData.Entity.Name, responseData);
		returnValue.Data = responseData;

		return returnValue;
	}

	/// <summary>
	/// Executes a Data Sync Service (GET)
	/// </summary>
	/// <returns></returns>
	[HttpPost("DataSyncService/Execute/Get/{ExecType}")]
	[PermissionsTypeRequired("Sync")]
	[PermissionsValidator]
	public async Task<ResponseModel> DataSyncServiceExecuteGet([FromServices] DataSyncServiceManager ServiceManager, [FromBody] DataSyncExecuteRequest ServiceRequest, [FromRoute] int ExecType)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		List<DataSyncExecuteResponse> ServicesResponse = [];
		foreach (string service in ServiceRequest.Services)
		{
			DataSyncHttpResponse requestResponse = await ServiceManager.ExecuteService(service, TriggerType.SmartFactory, ExecType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton, context.User, "GET").ConfigureAwait(false); // ExecType 1 = Event | ExecType 2 = SyncButton

			DataSyncExecuteResponse ServiceResponse = new()
			{
				Service = service,
				Response = "Service Executed",
				ResponseHttp = requestResponse
			};
			ServicesResponse.Add(ServiceResponse);
		}
		returnValue.Data = ServicesResponse;

		return returnValue;
	}

	/// <summary>
	/// Executes a Data Sync Service
	/// </summary>
	/// <returns></returns>
	[HttpPost("DataSyncService/Execute/Single/{ExecType}/{Trigger?}")]
	[PermissionsTypeRequired("Sync")]
	[PermissionsValidator]
	public async Task<ResponseModel> DataSyncServiceExecuteSingle([FromServices] DataSyncServiceManager ServiceManager, [FromBody] DataSyncExecuteRequest ServiceRequest, [FromRoute] int ExecType, [FromRoute] TriggerType Trigger = TriggerType.SmartFactory)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		List<DataSyncExecuteResponse> ServicesResponse = [];
		foreach (string service in ServiceRequest.Services)
		{
			DataSyncHttpResponse requestResponse = await ServiceManager.ExecuteService(service, Trigger, ExecType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton, context.User, "GET", ServiceRequest.EntityCode).ConfigureAwait(false); // ExecType 1 = Event | ExecType 2 = SyncButton

			DataSyncExecuteResponse ServiceResponse = new()
			{
				Service = service,
				Response = "Service Executed",
				ResponseHttp = requestResponse
			};
			ServicesResponse.Add(ServiceResponse);
		}
		returnValue.Data = ServicesResponse;

		return returnValue;
	}

	/// <summary>
	/// Executes a Data Sync Service (POST)
	/// </summary>
	/// <returns></returns>
	[HttpPost("DataSyncService/Execute/Post/{ExecType}")]
	public async Task<ResponseModel> DataSyncServiceExecutePost([FromServices] DataSyncServiceManager ServiceManager, [FromBody] DataSyncExecuteRequest ServiceRequest, [FromRoute] int ExecType)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		List<DataSyncExecuteResponse> ServicesResponse = [];
		foreach (string service in ServiceRequest.Services)
		{
			DataSyncHttpResponse requestResponse = await ServiceManager.ExecuteService(service, TriggerType.SmartFactory, ExecType == 1 ? ServiceExecOrigin.Event : ServiceExecOrigin.SyncButton, context.User, "POST", "", ServiceRequest.BodyData).ConfigureAwait(false); // ExecType 1 = Event | ExecType 2 = SyncButton

			DataSyncExecuteResponse ServiceResponse = new()
			{
				Service = service,
				Response = "Service Executed",
				ResponseHttp = requestResponse
			};
			ServicesResponse.Add(ServiceResponse);
		}
		returnValue.Data = ServicesResponse;

		return returnValue;
	}

	/// <summary>
	/// Webhook Data Sync
	/// </summary>
	/// <remarks>
	/// <br /><div class="req-res-title">DESCRIPTION</div>
	/// <p>Allows to invoke one or more data sync services</p>
	/// </remarks>
	/// <returns></returns>
	[Produces("application/json")]
	[Consumes("application/json")]
	[HttpPost("DataSyncService/Webhook")]
	[Tags("Integrators")]
	public async Task<ResponseModel> DataSyncServiceWebhook([FromServices] DataSyncServiceManager ServiceManager, [FromBody] DataSyncExecuteRequest ServiceRequest)
	{
		ResponseModel returnValue = new();

		List<DataSyncExecuteResponse> ServicesResponse = [];
		foreach (string service in ServiceRequest.Services)
		{
			int runStatus = await DataSyncServiceManager.ValidateExecuteService(service, TriggerType.Erp, ServiceExecOrigin.Webhook, "GET").ConfigureAwait(false);
			if (runStatus > 0)
			{
				_ = ServiceManager.ExecuteService(service, TriggerType.Erp, ServiceExecOrigin.Webhook, null, "GET", ServiceRequest.EntityCode);
			}
			else if (runStatus == 0)
			{
				_ = DataSyncServiceManager.InsertDataSyncServiceLog(service, "Service is disabled", new User(-1));
			}

			string responseMessage = runStatus switch
			{
				1 => "Service executed successfully",
				2 => "Service is already running",
				0 => "Service is disabled",
				_ => "Service does not exist!",
			};
			DataSyncExecuteResponse ServiceResponse = new()
			{
				Service = service,
				IsSuccess = runStatus > 0,
				Response = responseMessage
			};
			ServicesResponse.Add(ServiceResponse);
		}
		returnValue.Data = ServicesResponse;

		return returnValue;
	}

	/// <summary>
	/// Get Service Instance
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Instance/Service/{Service}")]
	
	public async Task<ResponseModel> GetDataSyncServiceInstance([FromRoute] string Service)
	{
		ResponseModel returnValue = new();

		BusinessEntities.DataSyncService ServiceData = await _DataSyncService.GetBackgroundService(Service).ConfigureAwait(false);
		returnValue.IsSuccess = true;
		returnValue.Data = ServiceData;

		return returnValue;
	}

	/// <summary>
	/// One/List Data Sync Log(s) in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Log/{LogId?}/{LogType?}")]
	
	public async Task<ResponseModel> GetDataSyncServiceLogs([FromRoute] string LogId, [FromRoute] int LogType = 0)
	{
		ResponseModel returnValue = new()
		{
			Data = await _DataSyncService.GetDataSyncServiceLogs(LogId, LogType).ConfigureAwait(false)
		};

		return returnValue;
	}

	/// <summary>
	/// Data Sync Visibility info in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Visibility")]
	
	public async Task<ResponseModel> GetSyncServiceInstanceVisibility([FromQuery] string Services, [FromQuery] TriggerType Trigger)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = await _DataSyncService.GetSyncServiceInstanceVisibility(context.User, Services, Trigger).ConfigureAwait(false);

		return returnValue;
	}

	/// <summary>
	/// Update a Data Sync ERP and Service Instances
	/// </summary>
	/// <returns></returns>
	[HttpPost("DataSyncService")]
	public async Task<ResponseModel> MergeFullData([FromServices] DataSyncServiceManager ServiceManager, [FromBody] DataSyncErp DataInfo)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		DataSyncErp responseData = _DataSyncService.MergeFullData(context.User, DataInfo);
		if (responseData.Instances.Count > 0)
		{
			responseData.Instances.ForEach(instance => ServiceManager.UpdateServiceData(instance.Entity.Name, instance));
		}
		List<DataSyncService> disabledServices = _DataSyncService.ListDisabledServices();
		disabledServices?.ForEach(service => ServiceManager.UpdateServiceData(service.EntityId, null));
		ErpFailedRecordReprocessService scopedService = (ErpFailedRecordReprocessService)StaticServiceProvider.Provider.GetService(typeof(ErpFailedRecordReprocessService));
		scopedService?.SetServiceData(responseData);
		BusinessLayer.Services.ContextCache.ERPOffset = null;
		BusinessLayer.Services.ServiceManager.NotifyServicesChanged(responseData);
		returnValue.Data = responseData;

		return returnValue;
	}

	/// <summary>
	/// List Data Sync Log(s) header by instance in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Log/Header/{ServiceInstanceId?}")]
	public async Task<ResponseModel> GetDataSyncServiceHeaderLogs([FromRoute] string ServiceInstanceId = "")
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.GetDataSyncServiceHeaderLogs(context.User, ServiceInstanceId);

		return returnValue;
	}

	/// <summary>
	/// List Data Sync Log(s) header by instance in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Log/Error/{ServiceInstanceId?}")]
	public async Task<ResponseModel> GetDataSyncServiceHeaderErrorLogs([FromRoute] string ServiceInstanceId = "")
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.GetDataSyncServiceHeaderErrorLogs(context.User, ServiceInstanceId);

		return returnValue;
	}

	/// <summary>
	/// List Data Sync Log(s) header by instance in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Log/Data/{logid?}/{logtype?}")]
	public async Task<ResponseModel> GetDataSyncServiceHeaderDataJSONLogs([FromRoute] string logid = "", [FromRoute] string logtype = "")
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = await _DataSyncService.GetDataSyncServiceHeaderDataLogs(logid, logtype, context.User).ConfigureAwait(false);

		return returnValue;
	}

	/// <summary>
	/// List Data Sync Log(s) detail by instance in application
	/// </summary>
	/// <returns></returns>
	[HttpGet("DataSyncService/Log/Detail/{LogId}")]
	public async Task<ResponseModel> GetDataSyncServiceDetailLogs([FromRoute] string LogId)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = await _DataSyncService.GetDataSyncServiceDetailLogs(context.User, LogId).ConfigureAwait(false);

		return returnValue;
	}

	[HttpGet("DataSyncService/Log/Single/{LogId}")]
	public async Task<ResponseModel> GetDataSyncServiceDetailLogsSingle([FromRoute] string LogId)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = await _DataSyncService.GetDataSyncServiceDetailLogsSingle(context.User, LogId).ConfigureAwait(false);

		return returnValue;
	}

	/// <summary>
	/// Create/Update a Data Sync Instance Mapping
	/// </summary>
	/// <returns></returns>
	[HttpPost("DataSyncService/Erp/Mapping")]
	public async Task<ResponseModel> MergeDataSyncServiceInstanceMapping([FromBody] DataSyncErpMapping Request)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = _DataSyncService.MergeDataSyncServiceInstanceMapping(context.User, Request);

		return returnValue;
	}

	/// <summary>
	/// Test Data Sync ERP Connection
	/// </summary>
	/// <returns></returns>
	[HttpPost("DataSyncService/Erp/Test")]
	public async Task<ResponseModel> TestErpConnection([FromBody] DataSyncErp Request)
	{
		ResponseModel returnValue = new();
		RequestContext context = GetContext();

		returnValue.Data = await BusinessLayer.Operations.TestErpConnection(context.User, Request).ConfigureAwait(false);

		return returnValue;
	}

	[HttpGet("DataSyncService/Erp/Timezones")]
	public async Task<ResponseModel> GetDataSyncServiceTimezones()
	{
		ResponseModel returnValue = new()
		{
			Data = _DataSyncService.GetTimezones()
		};

		return returnValue;
	}

	#endregion DataSync
}
