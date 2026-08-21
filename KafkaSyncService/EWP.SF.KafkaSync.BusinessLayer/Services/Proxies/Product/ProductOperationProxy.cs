using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Product operations targeting the external Product microservice.
/// Used by a dedicated Kafka consumer to forward messages to the real microservice endpoint.
/// </summary>
public class ProductOperationProxy : BaseHttpProxy, IComponentOperation
{
     private readonly bool _use2503ForSync;
    public ProductOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
            _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
    }

    public async Task<List<ResponseData>> ListUpdateProduct(
        List<ProductExternal> itemList,
        List<ProductExternal> itemListOriginal,
        User systemOperator,
        bool Validate,
        LevelMessage Level)
    {
        string endpoint = $"Product/{Validate.ToString().ToLower()}/{Level}";
        
        ResponseModel response;
        if (_use2503ForSync)
        {
            response = await PostAsyncPO<ResponseModel>(endpoint, itemList).ConfigureAwait(false);
        }
        else
        {
            response = await PostAsync<ResponseModel>(endpoint, itemList).ConfigureAwait(false);
        }

        if (response.IsSuccess)
        {
            if (response.Data is IEnumerable<ResponseData> dataList)
            {
                return dataList.ToList();
            }
            else if (response.Data != null)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<ResponseData>>(response.Data.ToString());
            }
        }
        else
        {
             return itemList.Select(x => new ResponseData
             {
                 IsSuccess = false,
                 Message = response.Message,
                 Code = x.ProductCode
             }).ToList();
        }

        return new List<ResponseData>();
    }

    public Task<ResponseData> ProcessProduct(ActionDB mode, Component component, User systemOperator)
    {
        return PostAsync<ResponseData>($"Product/Merge/{mode}", component);
    }

    public Task<ResponseData> ProcessProduct(ActionDB mode, ProductExternal externalProduct, User systemOperator)
    {
        return PostAsync<ResponseData>($"Product/Sync/{mode}", externalProduct);
    }

    public async Task<ResponseData> MergeProduct(
        ActionDB mode,
        Component componentInfo,
        User systemOperator,
        bool Validate = false,
        LevelMessage Level = LevelMessage.Success,
        bool NotifyOnce = true,
        bool isNewVersion = false,
        bool isExternalEndpoint = false,
        IntegrationSource intSource = IntegrationSource.SF)
    {
        // Fallback to the UI endpoint, matching the controller's expectation
        return await ProcessProduct(mode, componentInfo, systemOperator).ConfigureAwait(false);
    }

    // ─── Read operations (not available via HTTP proxy) ───────────────────────

    public Component GetComponentByCode(string Code)
        => throw new NotSupportedException("GetComponentByCode is not available through the HTTP proxy.");

    public Task<Component[]> GetComponents(string componentId, bool ignoreImages = false, string filter = "")
        => throw new NotSupportedException("GetComponents is not available through the HTTP proxy.");

    public Task<List<ProcessEntry>> GetProcessEntryById(string processEntryId, User systemOperator)
        => throw new NotSupportedException("GetProcessEntryById is not available through the HTTP proxy.");

    public Task<List<ProcessEntry>> GetProcessEntry(string productCode, string warehouseId, int? version, int? sequence, User systemOperator)
        => throw new NotSupportedException("GetProcessEntry is not available through the HTTP proxy.");
}
