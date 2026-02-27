using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Warehouse operations targeting the Inventory Microservice.
/// Used by the Kafka consumer to forward messages to the real microservice endpoint.
/// Shares the same base URL (InventoryServiceUrl) as BinLocationOperationProxy.
/// </summary>
public class WarehouseOperationProxy : BaseHttpProxy, IWarehouseOperation
{
    public WarehouseOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "InventoryServiceUrl")  // same service as BinLocation
    { }

    // ─── Write operations (forwarded to Inventory Microservice) ───────────────

    public async Task<List<ResponseData>> ListUpdateWarehouseGroup(
        List<WarehouseExternal> warehouseGroupList,
        List<WarehouseExternal> warehouseGroupListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level,
        string logId = null)
    {
        // Endpoint: API/V1/Warehouse/{validate}/{level}
        string endpoint = $"Warehouse/{Validate.ToString().ToLower()}/{Level}";
        return await PostAsync<List<ResponseData>>(endpoint, warehouseGroupList)
                    .ConfigureAwait(false);
    }

    public async Task<ResponseData> MergeWarehouse(
        Warehouse WarehouseInfo,
        User systemOperator,
        bool Validate   = false,
        bool NotifyOnce = true,
        string logId = null)
    {
        return await PostAsync<ResponseData>("Warehouse/Merge", new
        {
            WarehouseInfo,
            systemOperator,
            Validate,
            NotifyOnce
        }).ConfigureAwait(false);
    }

    // ─── Read operations (not available via HTTP proxy — use direct DB access) ─

    public Warehouse GetWarehouse(string Code)
        => throw new NotSupportedException(
            "GetWarehouse is not available through the HTTP proxy.");

    public List<Warehouse> ListWarehouse(
        User systemOperator, string WarehouseCode = "", DateTime? DeltaDate = null)
        => throw new NotSupportedException(
            "ListWarehouse is not available through the HTTP proxy.");
}
