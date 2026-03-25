using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class ProductionOrderOperationProxy : BaseHttpProxy, IWorkOrderOperation
{
    private readonly ILogger<ProductionOrderOperationProxy> _logger;

    public ProductionOrderOperationProxy(
        HttpClient httpClient, 
        IConfiguration configuration, 
        IAuthenticationService authService, 
        ILogger<ProductionOrderOperationProxy> logger)
        : base(httpClient, configuration, authService)
    {
        _logger = logger;
    }

    public async Task<List<WorkOrderResponse>> ListUpdateProductionOrder(
    List<WorkOrderExternal> workOrderList,
    User systemOperator,
    bool Validate,
    LevelMessage Level,
    bool isDataSynced = false,
    string logId = null)
{
    var request = new
    {
        WorkOrderList = workOrderList,
        SystemOperator = systemOperator,
        IsDataSynced = isDataSynced,
        LogId = logId
    };

    var url = $"WorkOrder/{Validate}/{Level}";

    var response = await PostAsync<List<WorkOrderResponse>>(url, request)
                        .ConfigureAwait(false);

    return response ?? new List<WorkOrderResponse>();
}

    public List<WorkOrderResponse> ListUpdateWorkOrderChangeStatus(List<ProductionOrderChangeStatusExternal> workOrderList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        var request = new
        {
            WorkOrderList = workOrderList,
            SystemOperator = systemOperator,
            Validate = Validate,
            Level = Level,
            LogId = logId
        };

        // This might need a different endpoint, but following the general pattern
        var response = Task.Run(() => PostAsync<List<WorkOrderResponse>>("ProductionOrder/ChangeStatus", request)).Result;
        return response ?? new List<WorkOrderResponse>();
    }

    public Task<double> GetTimezoneOffset(string offSetName = "") => throw new NotSupportedException();
    public Task<List<WorkOrder>> GetWorkOrder(string workOrderId) => throw new NotSupportedException();
    public Task<List<ResponseData>> ListUpdateProductTransfer(List<ProductTransferExternal> transferList, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
    public void AddWorkOrderDatesOffset(WorkOrderExternal order, double offset) => throw new NotSupportedException();
    // public async Task<object> UpdateWorkOrderComponent(TransactionMaterialSyncRequest request, User systemOperator)
    // {
        

    //     var microserviceRequest = new
    //     {
    //         TransactionId = request.OrderTransactionsMaterial[0].TransactionId,
    //         SystemOperator = systemOperator
    //     };


    //     var response = await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId", microserviceRequest).ConfigureAwait(false);
    //     return response?.Data;

    // }

    public async Task<object> UpdateWorkOrderComponent(TransactionMaterialSyncRequest request, User systemOperator)
    {
        try
        {
            var microserviceRequest = new
            {
                TransactionId = request.OrderTransactionsMaterial[0].TransactionId
            };

            var response = await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId",microserviceRequest).ConfigureAwait(false);

            if (response?.Data == null)
            {
                return null;
            }

            JObject dataObj = JObject.FromObject(response.Data);

            JToken transactions = dataObj["Transactions"];

            return transactions; 
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while fetching transaction material: {ex.Message}", ex);
        }
    }

    public async Task<object> UpdateWorkOrderProduct(TransactionProductSyncRequest request, User systemOperator)
    {
        try
        {
            var microserviceRequest = new
            {
                TransactionId = request.OrderTransactionsProduct[0].TransactionId,
            };
           
             var response = await PostAsync<ResponseModel>("WorkOrder/ProductionOrder/WithoutExternalId",microserviceRequest).ConfigureAwait(false);

            if (response?.Data == null)
            {
                return null;
            }

            JToken dataToken = NormalizeResponseData(response.Data);
            if (dataToken == null)
            {
                return null;
            }

            if (dataToken.Type == JTokenType.Object && dataToken["Transactions"] != null)
            {
                var transactions = dataToken["Transactions"];

                // Ensure it's an array
                return transactions.Type == JTokenType.Array
                    ? transactions
                    : new JArray(transactions);
            }

            
            if (dataToken.Type == JTokenType.Object)
            {
                return new JArray(dataToken);
            }

            
            if (dataToken.Type == JTokenType.Array)
            {
                return dataToken;
            }

            return null;

            
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while fetching transaction product: {ex.Message}", ex);
        }
    }

    private static JToken NormalizeResponseData(object data)
    {
        if (data == null)
        {
            return null;
        }

        if (data is JToken token)
        {
            return token;
        }

        if (data is string text)
        {
            var trimmed = text.Trim();

            if (trimmed.StartsWith("{{") && trimmed.EndsWith("}}") && trimmed.Length >= 4)
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }

            try
            {
                return JToken.Parse(trimmed);
            }
            catch (JsonReaderException)
            {
                return JValue.CreateString(text);
            }
        }

        return JToken.FromObject(data);
    }

    public async Task<object> UpdateExternalID(string externalId, string requestBody, User systemOperator)
    {
       var request = new
        {
            oprationId = externalId,
            externalId = requestBody
        };

        var response = await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/UpdateExternalID", request).ConfigureAwait(false);
        return response?.Data;
    }

    public async Task<object> UpdateProductExternalID(string externalId, string requestBody, User systemOperator)
    {
        var request = new
        {
            ExternalId = externalId,
            RequestBody = requestBody,
            SystemOperator = systemOperator
        };

        var response = await PostAsync<ResponseModel>("WorkOrder/TransactionProduct/UpdateExternalID", request).ConfigureAwait(false);
        return response?.Data;
    }

    public async Task<object> UpdateMachineIssueExternalID(string externalId, string requestBody, User systemOperator)
    {
        var request = new
        {
            oprationId = externalId,
            externalId = requestBody
        };

        var response = await PostAsync<ResponseModel>("WorkOrder/ResourceIssueMachine/UpdateExternalID", request).ConfigureAwait(false);
        return response?.Data;
    }

    public async Task<object> UpdateLaborIssueExternalID(string externalId, string requestBody, User systemOperator)
    {
        var request = new
        {
            oprationId = externalId,
            externalId = requestBody
        };

        var response = await PostAsync<ResponseModel>("WorkOrder/ResourceIssueLabor/UpdateExternalID", request).ConfigureAwait(false);
        return response?.Data;
    }

    public async Task<object> UpdateMachineIssue(MachineIssueSyncRequest request, User systemOperator)
    {
        try
        {
            var microserviceRequest = new
            {
                TransactionId = request.TransactionId
            };

            var response = await PostAsync<ResponseModel>("WorkOrder/MachineIssue/WithoutExternalId", microserviceRequest).ConfigureAwait(false);

            if (response?.Data == null)
            {
                return null;
            }

            JToken dataToken = NormalizeResponseData(response.Data);
            if (dataToken == null)
            {
                return null;
            }
            
            if (dataToken.Type == JTokenType.Array)
            {
                return dataToken;
            }

            if (dataToken.Type == JTokenType.Object)
            {
                if (dataToken["Transactions"] != null)
                {
                    var transactions = dataToken["Transactions"];

                    return transactions.Type == JTokenType.Array
                        ? transactions
                        : new JArray(transactions);
                }

                return new JArray(dataToken);
            }

            return null;
           
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while fetching machine issue: {ex.Message}", ex);
        }
    }


    public async Task<object> UpdateLaborIssue(MachineIssueSyncRequest request, User systemOperator)
    {
        try
        {
            var microserviceRequest = new
            {
                TransactionId = request.TransactionId
            };

            var response = await PostAsync<ResponseModel>("WorkOrder/LaborIssue/WithoutExternalId", microserviceRequest).ConfigureAwait(false);

            if (response?.Data == null)
            {
                return null;
            }

            JToken dataToken = NormalizeResponseData(response.Data);
            if (dataToken == null)
            {
                return null;
            }
            
            if (dataToken.Type == JTokenType.Array)
            {
                return dataToken;
            }

            if (dataToken.Type == JTokenType.Object)
            {
                if (dataToken["Transactions"] != null)
                {
                    var transactions = dataToken["Transactions"];

                    return transactions.Type == JTokenType.Array
                        ? transactions
                        : new JArray(transactions);
                }

                return new JArray(dataToken);
            }

            return null;
           
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while fetching machine issue: {ex.Message}", ex);
        }
    }

    public Task<object> GetMaterialTransactionRequestParams(User systemOperator, CancellationToken cancel = default) => throw new NotSupportedException();
    public List<ResponseData> ListUpdateCLockInOutBulk(List<ClockInOutDetailsExternal> clockList, List<ClockInOutDetailsExternal> itemListOriginal, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
}
