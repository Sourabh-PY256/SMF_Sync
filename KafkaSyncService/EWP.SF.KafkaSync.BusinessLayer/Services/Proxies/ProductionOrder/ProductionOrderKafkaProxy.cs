using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class ProductionOrderKafkaProxy : BaseKafkaProxy, IWorkOrderOperation
{
    public ProductionOrderKafkaProxy(
        IKafkaService kafkaService, 
        IConfiguration configuration, 
        ILogger<ProductionOrderKafkaProxy> logger)
        : base(kafkaService, configuration, logger, 
            "KafkaSettings:Topics:PRODUCTION_ORDER",       // appsettings key
            "shopfloor-productionorder-sync")              // fallback topic
    {
    }

    public async Task<List<WorkOrderResponse>> ListUpdateProductionOrder(List<WorkOrderExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level, bool isDataSynced = false, string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.PRODUCTION_ORDER_SERVICE,
            "ListUpdateProductionOrder",
            systemOperator,
            new
            {
                Data = workOrderList,
                Validate = Validate,
                Level = Level,
                IsDataSynced = isDataSynced
            },
            logId).ConfigureAwait(false);

        return [new WorkOrderResponse { IsSuccess = result.IsSuccess, Message = result.Message }];
    }

    public List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(List<ProductionOrderChangeStatusExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        // This can either go to the same topic or a dedicated one. Following the pattern of using one topic per service.
        var result = Task.Run(() => PublishAsync(
            SyncERPEntity.PRODUCTION_ORDER_SERVICE,
            "ListUpdateWorkOrderChangeStatus",
            systemOperator,
            new
            {
                Data = workOrderList,
                Validate = Validate,
                Level = Level
            },
            logId)).Result;

        return [new WorkOrderResponse { IsSuccess = result.IsSuccess, Message = result.Message }];
    }

    public Task<double> GetTimezoneOffset(string offSetName = "") => throw new NotSupportedException();
    public Task<List<WorkOrder>> GetWorkOrder(string workOrderId) => throw new NotSupportedException();
    public Task<List<ResponseData>> ListUpdateProductTransfer(List<ProductTransferExternal> transferList, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
    public void AddWorkOrderDatesOffset(WorkOrderExternal order, double offset)
    {
        if (order is not null)
		{
			if (order.PlannedStartDate.Year > 1900)
			{
				order.PlannedStartDate = order.PlannedStartDate.AddHours(offset);
			}
			if (order.PlannedEndDate.Year > 1900)
			{
				order.PlannedEndDate = order.PlannedEndDate.AddHours(offset);
			}
			if (order.DueDate.Year > 1900)
			{
				order.DueDate = order.DueDate.AddHours(offset);
			}

			order.Operations?.ForEach(op =>
				{
					if (op.PlannedStartDate.Year > 1900)
					{
						op.PlannedStartDate = op.PlannedStartDate.AddHours(offset);
					}
					if (op.PlannedEndDate.Year > 1900)
					{
						op.PlannedEndDate = op.PlannedEndDate.AddHours(offset);
					}
				});
		}
    }
    public Task<string> UpdateWorkOrderComponent(string workOrderId, List<OrderComponent> componentValues, string employeeId, User systemOperator) => throw new NotSupportedException();
    public Task<object> GetMaterialTransactionRequestParams(User systemOperator, CancellationToken cancel = default) => throw new NotSupportedException();
    public Task<object> UpdateExternalID(string externalId, string requestBody, User systemOperator) => throw new NotSupportedException();
    public List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
}
