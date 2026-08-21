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
    private readonly bool _use2503ForSync;

    public ProductionOrderOperationProxy(
        HttpClient httpClient,
        IConfiguration configuration,
        IAuthenticationService authService,
        ILogger<ProductionOrderOperationProxy> logger)
        : base(httpClient, configuration, authService)
    {
        _logger = logger;
        _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
    }

    // public async Task<List<WorkOrderResponse>> ListUpdateProductionOrder(
    // List<WorkOrderExternal> workOrderList,
    // User systemOperator,
    // bool Validate,
    // LevelMessage Level,
    // bool isDataSynced = false,
    // string logId = null)
    // {
    //     // var request = new
    //     // {
    //     //     WorkOrderList = workOrderList
    //     // };

    //     foreach (var order in workOrderList)
    //     {
    //         // Fix Status casing
    //         if (!string.IsNullOrEmpty(order.Status))
    //             order.Status = System.Globalization.CultureInfo.CurrentCulture
    //                                 .TextInfo.ToTitleCase(order.Status.ToLower());

    //         foreach (var op in order.Operations ?? [])
    //         {
    //             // Null out empty regex-validated strings
    //             if (string.IsNullOrEmpty(op.OperationTimeType))
    //                 op.OperationTimeType = null;

    //             foreach (var item in op.Items ?? [])
    //             {
    //                 if (string.IsNullOrEmpty(item.IssueMethod))
    //                     item.IssueMethod = null;

    //                 if (string.IsNullOrEmpty(item.Type))
    //                     item.Type = null;

    //                 if (string.IsNullOrEmpty(item.Source))
    //                     item.Source = null;
    //             }

    //             foreach (var machine in op.Machines ?? [])
    //             {
    //                 if (string.IsNullOrEmpty(machine.IssueMode))
    //                     machine.IssueMode = null;
    //             }
    //         }
    //     }

    //     //var url = $"WorkOrder/{Validate}/{Level}";
    //     var url = $"WorkOrder/{Validate.ToString().ToLower()}/{Level}";

    //     if (_use2503ForSync)
    //     {
    //          var response = await PostAsyncPO<List<WorkOrderResponse>>(url, workOrderList)
    //                         .ConfigureAwait(false);
    //                         return response ?? new List<WorkOrderResponse>();
    //     }
    //     else
    //     {
    //         var response = await PostAsync<List<WorkOrderResponse>>(url, workOrderList)
    //                         .ConfigureAwait(false);
    //                         return response ?? new List<WorkOrderResponse>();
    //     }
    // }

    public async Task<List<WorkOrderResponse>> ListUpdateProductionOrder(
    List<WorkOrderExternal> workOrderList,
    User systemOperator,
    bool Validate,
    LevelMessage Level,
    bool isDataSynced = false,
    string logId = null)
    {
        // Allowed values per RegularExpression attributes on the model
        var validIssueMethods = new[] { "Manual", "Backflush", "Operation Issue" };
        var validIssueModes = new[] { "Manual", "Backflush" };
        var validTypes = new[] { "Material", "Consumable" };
        var validSources = new[] { "BOM", "Formula" };

        string NormalizeRegexField(string value, string[] allowedValues)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return allowedValues.FirstOrDefault(v =>
                string.Equals(v, value, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var order in workOrderList)
        {
            // Fix Status casing
            if (!string.IsNullOrEmpty(order.Status))
                order.Status = System.Globalization.CultureInfo.CurrentCulture
                                    .TextInfo.ToTitleCase(order.Status.ToLower());

            foreach (var op in order.Operations ?? [])
            {
                // Null out empty regex-validated strings
                if (string.IsNullOrEmpty(op.OperationTimeType))
                    op.OperationTimeType = null;

                foreach (var item in op.Items ?? [])
                {
                    item.IssueMethod = NormalizeRegexField(item.IssueMethod, validIssueMethods);
                    item.Type = NormalizeRegexField(item.Type, validTypes);
                    item.Source = NormalizeRegexField(item.Source, validSources);
                }

                foreach (var machine in op.Machines ?? [])
                {
                    machine.IssueMode = NormalizeRegexField(machine.IssueMode, validIssueModes);

                    foreach (var machineLabor in machine.Labor ?? [])
                        machineLabor.IssueMode = NormalizeRegexField(machineLabor.IssueMode, validIssueModes);

                    foreach (var machineTool in machine.Tooling ?? [])
                        machineTool.IssueMode = NormalizeRegexField(machineTool.IssueMode, validIssueModes);
                }

                // THIS WAS MISSING - op.Labor[].IssueMode was never touched
                foreach (var labor in op.Labor ?? [])
                    labor.IssueMode = NormalizeRegexField(labor.IssueMode, validIssueModes);

                foreach (var tooling in op.Tooling ?? [])
                    tooling.IssueMode = NormalizeRegexField(tooling.IssueMode, validIssueModes);
            }
        }

        var url = $"WorkOrder/{Validate.ToString().ToLower()}/{Level}";

        if (_use2503ForSync)
        {
            var response = await PostAsyncPO<List<WorkOrderResponse>>(url, workOrderList)
                            .ConfigureAwait(false);
            return response ?? new List<WorkOrderResponse>();
        }
        else
        {
            var response = await PostAsync<List<WorkOrderResponse>>(url, workOrderList)
                            .ConfigureAwait(false);
            return response ?? new List<WorkOrderResponse>();
        }
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
        var response = Task.Run(() => PostAsync<List<WorkOrderResponse>>($"WorkOrderChangeStatus/{Validate.ToString().ToLower()}/{Level}", request)).Result;
        return response ?? new List<WorkOrderResponse>();
    }

    public Task<double> GetTimezoneOffset(string offSetName = "") => throw new NotSupportedException();
    public Task<List<WorkOrder>> GetWorkOrder(string workOrderId) => throw new NotSupportedException();
    public Task<List<ResponseData>> ListUpdateProductTransfer(List<ProductTransferExternal> transferList, User systemOperator, bool Validate, LevelMessage Level) => throw new NotSupportedException();
    public void AddWorkOrderDatesOffset(WorkOrderExternal order, double offset) => throw new NotSupportedException();

    // public async Task<object> UpdateWorkOrderComponent(TransactionMaterialSyncRequest request, User systemOperator)
    // {
    //     try
    //     {
    //         var microserviceRequest = new
    //         {
    //             TransactionId = request.OrderTransactionsMaterial[0].TransactionId
    //         };

    //         var response = _use2503ForSync? await PostAsyncPO<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId", request).ConfigureAwait(false): await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId", request).ConfigureAwait(false);

    //         //var response = await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId", microserviceRequest).ConfigureAwait(false);

    //         if (response?.Data == null)
    //         {
    //             return null;
    //         }

    //         JObject dataObj = JObject.FromObject(response.Data);

    //         JToken transactions = dataObj["Transactions"];

    //         return transactions;
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new Exception($"Error while fetching transaction material: {ex.Message}", ex);
    //     }
    // }

    public async Task<object> UpdateWorkOrderComponent(object request, User systemOperator)
    {
        try
        {
            ResponseModel response = null;

            if (request is TransactionMaterialSyncRequest materialRequest)
            {
                var microserviceRequest = new
                {
                    TransactionId = materialRequest.OrderTransactionsMaterial?[0]?.TransactionId
                };

                response = _use2503ForSync
                    ? await PostAsyncPO<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId", microserviceRequest).ConfigureAwait(false)
                    : await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId", microserviceRequest).ConfigureAwait(false);
            }
            else if (request is TransactionRequest normalRequest)
            {
                var microserviceRequest = new
                {
                    TransactionId = normalRequest.TransactionId
                };

                response = _use2503ForSync
                    ? await PostAsyncPO<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId", normalRequest).ConfigureAwait(false)
                    : await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/WithoutExternalId", normalRequest).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("Unsupported request type");
            }

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

     public async Task<object> UpdateInventoryTransferRequest(object request, User systemOperator)
    {
        try
        {
            ResponseModel response = _use2503ForSync
                ? await PostAsyncPO<ResponseModel>("WorkOrder/InventoryTransferRequest/WithoutExternalId", request).ConfigureAwait(false)
                : await PostAsync<ResponseModel>("WorkOrder/InventoryTransferRequest/WithoutExternalId", request).ConfigureAwait(false);

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

    public async Task<object> UpdateWorkOrderProduct(TransactionProductReceiptSyncRequest request, User systemOperator)
    {
        try
        {
            var microserviceRequest = new
            {
                TransactionId = request.TransactionId,
            };

            //var response = await PostAsync<ResponseModel>("WorkOrder/ProductionOrder/WithoutExternalId", microserviceRequest).ConfigureAwait(false);
            var response = _use2503ForSync
                    ? await PostAsyncPO<ResponseModel>("WorkOrder/ProductionOrder/WithoutExternalId", microserviceRequest).ConfigureAwait(false)
                    : await PostAsync<ResponseModel>("WorkOrder/ProductionOrder/WithoutExternalId", microserviceRequest).ConfigureAwait(false);

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
        try
        {
            var request = new
            {
                oprationId = externalId,
                externalId = requestBody
            };

            //var response = await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/UpdateExternalID", request).ConfigureAwait(false);
            var response = _use2503ForSync
                    ? await PostAsyncPO<ResponseModel>("WorkOrder/TransactionMaterial/UpdateExternalID", request).ConfigureAwait(false)
                    : await PostAsync<ResponseModel>("WorkOrder/TransactionMaterial/UpdateExternalID", request).ConfigureAwait(false);

            return response?.Data;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while updating external ID: {ex.Message}", ex);
        }

    }

    public async Task<object> UpdateProductExternalID(string externalId, string requestBody, User systemOperator)
    {
        try
        {
            var request = new
            {
                ExternalId = externalId,
                RequestBody = requestBody,
                SystemOperator = systemOperator
            };

            //var response = await PostAsync<ResponseModel>("WorkOrder/TransactionProduct/UpdateExternalID", request).ConfigureAwait(false);
            var response = _use2503ForSync
                   ? await PostAsyncPO<ResponseModel>("WorkOrder/TransactionProduct/UpdateExternalID", request).ConfigureAwait(false)
                   : await PostAsync<ResponseModel>("WorkOrder/TransactionProduct/UpdateExternalID", request).ConfigureAwait(false);

            return response?.Data;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while updating product external ID: {ex.Message}", ex);
        }

    }
    public async Task<object> UpdateMachineIssueExternalID(string externalId, string requestBody, User systemOperator)
    {
        try
        {
            var request = new
            {
                oprationId = externalId,
                externalId = requestBody
            };

            //  var response = await PostAsync<ResponseModel>("WorkOrder/ResourceIssueMachine/UpdateExternalID", request).ConfigureAwait(false);
            var response = _use2503ForSync
                     ? await PostAsyncPO<ResponseModel>("WorkOrder/ResourceIssueMachine/UpdateExternalID", request).ConfigureAwait(false)
                     : await PostAsync<ResponseModel>("WorkOrder/ResourceIssueMachine/UpdateExternalID", request).ConfigureAwait(false);
            return response?.Data;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while updating machine issue external ID: {ex.Message}", ex);
        }

    }
    public async Task<object> UpdateLaborIssueExternalID(string externalId, string requestBody, User systemOperator)
    {
        try
        {
            var request = new
            {
                oprationId = externalId,
                externalId = requestBody
            };

            // var response = await PostAsync<ResponseModel>("WorkOrder/ResourceIssueLabor/UpdateExternalID", request).ConfigureAwait(false);
            var response = _use2503ForSync
                    ? await PostAsyncPO<ResponseModel>("WorkOrder/ResourceIssueLabor/UpdateExternalID", request).ConfigureAwait(false)
                    : await PostAsync<ResponseModel>("WorkOrder/ResourceIssueLabor/UpdateExternalID", request).ConfigureAwait(false);
            return response?.Data;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while updating labor issue external ID: {ex.Message}", ex);
        }

    }
    public async Task<object> UpdateMachineIssue(MachineIssueSyncRequest request, User systemOperator)
    {
        try
        {
            var microserviceRequest = new
            {
                TransactionId = request.TransactionId
            };

            //var response = await PostAsync<ResponseModel>("WorkOrder/MachineIssue/WithoutExternalId", microserviceRequest).ConfigureAwait(false);
            var response = _use2503ForSync
                    ? await PostAsyncPO<ResponseModel>("WorkOrder/MachineIssue/WithoutExternalId", microserviceRequest).ConfigureAwait(false)
                    : await PostAsync<ResponseModel>("WorkOrder/MachineIssue/WithoutExternalId", microserviceRequest).ConfigureAwait(false);

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
    public async Task<object> CallOrderErpSyncService(ProductionOrder request, User systemOperator)
    {
        try
        {
            var response = _use2503ForSync ? await PostAsyncPO<ResponseModel>("WorkOrder/SendToERP", request).ConfigureAwait(false) : await PostAsync<ResponseModel>("WorkOrder/SendToERP", request).ConfigureAwait(false);

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

            //var response = await PostAsync<ResponseModel>("WorkOrder/LaborIssue/WithoutExternalId", microserviceRequest).ConfigureAwait(false);
            var response = _use2503ForSync
                    ? await PostAsyncPO<ResponseModel>("WorkOrder/LaborIssue/WithoutExternalId", microserviceRequest).ConfigureAwait(false)
                    : await PostAsync<ResponseModel>("WorkOrder/LaborIssue/WithoutExternalId", microserviceRequest).ConfigureAwait(false);

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

    public Task<object> UpdateWorkOrderComponent(TransactionMaterialSyncRequest request, User systemOperator)
    {
        throw new NotImplementedException();
    }
}
