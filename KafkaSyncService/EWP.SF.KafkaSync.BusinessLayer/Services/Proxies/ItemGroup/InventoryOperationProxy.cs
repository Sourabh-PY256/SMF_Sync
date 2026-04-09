using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Inventory (ItemGroup) operations targeting the Inventory Microservice.
/// Used by the Kafka consumer to forward messages to the real microservice endpoint.
/// Shares the same base URL (InventoryServiceUrl) as BinLocationOperationProxy.
/// </summary>
public class InventoryOperationProxy : BaseHttpProxy, IInventoryOperation
{
    public InventoryOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public async Task<List<ResponseData>> ListUpdateInventoryGroup(
        List<InventoryExternal> inventoryGroupList, 
        List<InventoryExternal> inventoryGroupListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level,
        string logId = null)
    {
        // Endpoint: API/V1/ItemGroup/{validate}/{level}
        string endpoint = $"Inventory/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list directly as the body
        return await PostAsync<List<ResponseData>>(endpoint, inventoryGroupList).ConfigureAwait(false);
    }

    public async Task<ResponseData> MergeInventory(
        InventoryItemGroup InventoryInfo, 
        User systemOperator, 
        bool Validate = false, 
        bool NotifyOnce = true,
        string logId = null)
    {
        // Endpoint: Inventory/Merge
        return await PostAsync<ResponseData>("Inventory/Merge", new
        {
            InventoryInfo,
            systemOperator,
            Validate,
            NotifyOnce
        }).ConfigureAwait(false);
    }

    

    public async Task<InventoryItemGroup> GetInventory(string Code)
        => throw new NotSupportedException(
            "GetInventory is not available through the HTTP proxy.");

    public async Task<List<InventoryItemGroup>> ListInventory(
        User systemOperator, string InventoryCode = "", DateTime? DeltaDate = null)
    {
        List<ResponseModel> responseData = new List<ResponseModel>();
        try
        {
            var inventoryList = new List<InventoryItemGroup>();
            string endpoint = $"Inventory/List";
            responseData = await GetAsync<List<ResponseModel>>(endpoint).ConfigureAwait(false);
            if (responseData != null && responseData.Any())
            {
                inventoryList = responseData.SelectMany(r => r.Data as List<InventoryItemGroup>).ToList();
                return inventoryList;
            }
            return inventoryList;
        
        }
        catch (Exception ex)
        {
            
            throw;
        }
    }

    public SaleOrder[] ListSalesOrder(
        string Id, string SalesOrder, string CustomerCode, User systemOperator, 
        bool getAsMasterDetail = false, DateTime? DeltaDate = null)
        => throw new NotSupportedException(
            "ListSalesOrder is not available through the HTTP proxy.");

    InventoryItemGroup IInventoryOperation.GetInventory(string Code)
    {
        throw new NotImplementedException();
    }
}
