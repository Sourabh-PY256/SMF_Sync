using EWP.SF.Item.BusinessEntities;

namespace EWP.SF.Item.BusinessLayer;

public class DataSyncServiceManager
{
	private readonly ItemService _itemService;
	//	private readonly IoTDataSimulatorService _ioTDataSimulatorService;
	public DataSyncServiceManager(ItemService itemService)
	//IoTDataSimulatorService ioTDataSimulatorService)
	{
		_operations = new Operations();
		_itemService = itemService;
		//_ioTDataSimulatorService = ioTDataSimulatorService;
	}

	public static async Task InsertDataSyncServiceLog(string serviceName, string ErrorMessage, User systemOperator)
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

	public static async Task<int> ValidateExecuteService(string ServiceType, TriggerType Trigger, ServiceExecOrigin ExecOrigin, string HttpMethod = "GET")
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
				BackgroundServices.PROCEDURE_SERVICE => await _allocationService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.ALLOCATION_SERVICE => await _allocationService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.FACILITY_SERVICE => await _facilityService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.FLOOR_SERVICE => await _floorService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.WORKCENTER_SERVICE => await _workCenterService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.PRODUCTION_LINE_SERVICE => await _productionLineService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.BIN_LOCATION_SERVICE => await _binLocationService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.DEMAND_SERVICE => await _demandService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.EMPLOYEE_SERVICE => await _employeeService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.INVENTORY_SERVICE => await _inventoryService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.INVENTORY_STATUS_SERVICE => await _inventoryStatusService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.ITEM_SERVICE => await _itemService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.LOT_SERIAL_STATUS_SERVICE => await _lotSerialStatusService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.MACHINE_SERVICE => await _machineService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.SCHEDULE_SERVICE => await _orderScheduleService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.MATERIAL_ISSUE_SERVICE => await _materialIssueService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.MATERIAL_RETURN_SERVICE => await _materialReturnService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.POSITION_SERVICE => await _positionService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.PRODUCT_SERVICE => await _productService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.PRODUCTION_ORDER_CHANGE_STATUS_SERVICE => await _productionOrderChangeStatusService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.PRODUCTION_ORDER_SERVICE => await _productionOrderService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.PRODUCT_RECEIPT_SERVICE => await _productReceiptService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.PRODUCT_RETURN_SERVICE => await _productReturnService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.SKILL_CATALOG_SERVICE => await _skillCatalogService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.STOCK_SERVICE => await _stockService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.FULL_STOCK_SERVICE => await _stockService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.SUPPLY_SERVICE => await _supplyService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.UNIT_MEASURE_SERVICE => await _unitMeasureService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.WAREHOUSE_SERVICE => await _warehouseService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.PROCESS_TYPE_SERVICE => await _processTypeService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.TOOLING_TYPE_SERVICE => await _toolTypeService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.FULL_ALLOCATION_SERVICE => await _fullAllocationService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.CLOCKINOUT_SERVICE => await _fullAllocationService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.PRODUCT_TRANSFER_SERVICE => await _productTransferService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.MATERIAL_SCRAP_SERVICE => await _materialScrapService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.ATTACHMENT_SERVICE => await _materialScrapService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.MACHINE_ISSUE_SERVICE => await _machineIssueService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
				BackgroundServices.LABOR_ISSUE_SERVICE => await _laborIssueService.ManualExecution(Data, Trigger, ExecOrigin, SystemOperator, EntityCode, BodyData).ConfigureAwait(false),
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
			case BackgroundServices.ALLOCATION_SERVICE:
				_allocationService.SetServiceData(Data);
				break;

			case BackgroundServices.FACILITY_SERVICE:
				_facilityService.SetServiceData(Data);
				break;

			case BackgroundServices.FLOOR_SERVICE:
				_floorService.SetServiceData(Data);
				break;

			case BackgroundServices.WORKCENTER_SERVICE:
				_workCenterService.SetServiceData(Data);
				break;

			case BackgroundServices.PRODUCTION_LINE_SERVICE:
				_productionLineService.SetServiceData(Data);
				break;

			case BackgroundServices.BIN_LOCATION_SERVICE:
				_binLocationService.SetServiceData(Data);
				break;

			case BackgroundServices.DEMAND_SERVICE:
				_demandService.SetServiceData(Data);
				break;

			case BackgroundServices.EMPLOYEE_SERVICE:
				_employeeService.SetServiceData(Data);
				break;

			case BackgroundServices.INVENTORY_SERVICE:
				_inventoryService.SetServiceData(Data);
				break;

			case BackgroundServices.INVENTORY_STATUS_SERVICE:
				_inventoryStatusService.SetServiceData(Data);
				break;

			case BackgroundServices.ITEM_SERVICE:
				_itemService.SetServiceData(Data);
				break;

			case BackgroundServices.LOT_SERIAL_STATUS_SERVICE:
				_lotSerialStatusService.SetServiceData(Data);
				break;

			case BackgroundServices.MACHINE_SERVICE:
				_machineService.SetServiceData(Data);
				break;

			case BackgroundServices.SCHEDULE_SERVICE:
				_orderScheduleService.SetServiceData(Data);
				break;

			case BackgroundServices.MATERIAL_ISSUE_SERVICE:
				_materialIssueService.SetServiceData(Data);
				break;

			case BackgroundServices.MATERIAL_RETURN_SERVICE:
				_materialReturnService.SetServiceData(Data);
				break;

			case BackgroundServices.POSITION_SERVICE:
				_positionService.SetServiceData(Data);
				break;

			case BackgroundServices.PRODUCT_SERVICE:
				_productService.SetServiceData(Data);
				break;

			case BackgroundServices.PRODUCTION_ORDER_CHANGE_STATUS_SERVICE:
				_productionOrderChangeStatusService.SetServiceData(Data);
				break;

			case BackgroundServices.PRODUCTION_ORDER_SERVICE:
				_productionOrderService.SetServiceData(Data);
				break;

			case BackgroundServices.PRODUCT_RECEIPT_SERVICE:
				_productReceiptService.SetServiceData(Data);
				break;

			case BackgroundServices.PRODUCT_RETURN_SERVICE:
				_productReturnService.SetServiceData(Data);
				break;

			case BackgroundServices.SKILL_CATALOG_SERVICE:
				_skillCatalogService.SetServiceData(Data);
				break;

			case BackgroundServices.STOCK_SERVICE:
				_stockService.SetServiceData(Data);
				break;

			case BackgroundServices.FULL_STOCK_SERVICE:
				_fullStockService.SetServiceData(Data);
				break;

			case BackgroundServices.SUPPLY_SERVICE:
				_supplyService.SetServiceData(Data);
				break;

			case BackgroundServices.UNIT_MEASURE_SERVICE:
				_unitMeasureService.SetServiceData(Data);
				break;

			case BackgroundServices.WAREHOUSE_SERVICE:
				_warehouseService.SetServiceData(Data);
				break;

			case BackgroundServices.PROCESS_TYPE_SERVICE:
				_processTypeService.SetServiceData(Data);
				break;

			case BackgroundServices.TOOLING_TYPE_SERVICE:
				_toolTypeService.SetServiceData(Data);
				break;

			case BackgroundServices.FULL_ALLOCATION_SERVICE:
				_fullAllocationService.SetServiceData(Data);
				break;

			case BackgroundServices.CLOCKINOUT_SERVICE:
				_clockinoutService.SetServiceData(Data);
				break;

			case BackgroundServices.PRODUCT_TRANSFER_SERVICE:
				_productTransferService.SetServiceData(Data);
				break;

			case BackgroundServices.MATERIAL_SCRAP_SERVICE:
				_materialScrapService.SetServiceData(Data);
				break;
				//case BackgroundServices.IOT_DATA_SIMULATOR_SERVICE:
				//	_ioTDataSimulatorService.SetServiceData(Data);
				//	break;
		}
	}
}
