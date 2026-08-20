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
    public async Task<object> UpdateExternalID(string externalId, string requestBody, User systemOperator)
    {
        try
        {
            var result = await PublishAsync(
                SyncERPEntity.PRODUCTION_ORDER_SERVICE,
                "UpdateExternalID",
                systemOperator,
                new
                {
                    oprationId = externalId,
                    externalId = requestBody
                }).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProductionOrderKafkaProxy] UpdateExternalID failed. ExternalId: {ExternalId}", externalId);
            throw new Exception($"Error while publishing UpdateExternalID: {ex.Message}", ex);
        }
    }

    public async Task<object> UpdateProductExternalID(string externalId, string requestBody, User systemOperator)
    {
        try
        {
            var result = await PublishAsync(
                SyncERPEntity.PRODUCTION_ORDER_SERVICE,
                "UpdateProductExternalID",
                systemOperator,
                new
                {
                    ExternalId = externalId,
                    RequestBody = requestBody,
                    SystemOperator = systemOperator
                }).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProductionOrderKafkaProxy] UpdateProductExternalID failed. ExternalId: {ExternalId}", externalId);
            throw new Exception($"Error while publishing UpdateProductExternalID: {ex.Message}", ex);
        }
    }

    public async Task<object> UpdateMachineIssueExternalID(string externalId, string requestBody, User systemOperator)
    {
        try
        {
            var result = await PublishAsync(
                SyncERPEntity.MACHINE_ISSUE_SERVICE,
                "UpdateMachineIssueExternalID",
                systemOperator,
                new
                {
                    oprationId = externalId,
                    externalId = requestBody
                }).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProductionOrderKafkaProxy] UpdateMachineIssueExternalID failed. ExternalId: {ExternalId}", externalId);
            throw new Exception($"Error while publishing UpdateMachineIssueExternalID: {ex.Message}", ex);
        }
    }

    public async Task<object> UpdateLaborIssueExternalID(string externalId, string requestBody, User systemOperator)
    {
        try
        {
            var result = await PublishAsync(
                SyncERPEntity.LABOR_ISSUE_SERVICE,
                "UpdateLaborIssueExternalID",
                systemOperator,
                new
                {
                    oprationId = externalId,
                    externalId = requestBody
                }).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProductionOrderKafkaProxy] UpdateLaborIssueExternalID failed. ExternalId: {ExternalId}", externalId);
            throw new Exception($"Error while publishing UpdateLaborIssueExternalID: {ex.Message}", ex);
        }
    }

    public List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();

    public Task<object> UpdateWorkOrderComponent(TransactionMaterialSyncRequest request, User systemOperator)
    {
        throw new NotImplementedException();
    }

    public Task<object> UpdateWorkOrderProduct(TransactionProductReceiptSyncRequest request, User systemOperator)
    {
        throw new NotImplementedException();
    }

    public async Task<object> UpdateMachineIssue(MachineIssueSyncRequest  request, User systemOperator)
    {
        try
        {
            var result = await PublishAsync(
                SyncERPEntity.MACHINE_ISSUE_SERVICE,
                "UpdateMachineIssue",
                systemOperator,
                request).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ProductionOrderKafkaProxy] UpdateMachineIssue failed.");
            throw new Exception($"Error while publishing UpdateMachineIssue: {ex.Message}", ex);
        }
    }
}
