using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class BinLocationOperationProxy : BaseHttpProxy, IBinLocationOperation
{
    private readonly bool _use2503ForSync;
    public BinLocationOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService) 
        : base(httpClient, configuration, authService, "InventoryServiceUrl")
    {
         _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
    }

    public async Task<List<ResponseData>> ListUpdateBinLocation(
        List<BinLocationExternal> binLocationList, 
        List<BinLocationExternal> binLocationListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level,
        string logId = null)
    {
        // Redirecting to external microservice endpoint with path parameters
        // Example URL: API/V1/BinLocation/true/Record
        // The microservice expects [FromBody] List<BinLocationExternal> request
        string endpoint = $"BinLocation/{Validate.ToString().ToLower()}/{Level}";
        if(_use2503ForSync)
        {
            return await PostAsyncPO<List<ResponseData>>(endpoint, binLocationList).ConfigureAwait(false);            
        }
        else
        {
            return await PostAsync<List<ResponseData>>(endpoint, binLocationList).ConfigureAwait(false);            
        }
        // Sending the list directly as the body (not wrapped in an object)
    }

    public async Task<ResponseData> MergeBinLocation(
        BinLocation BinLocationInfo, 
        User systemOperator, 
        bool Validate = false, 
        bool NotifyOnce = true,
        string logId = null)
    {
        // Redirecting to external microservice endpoint
        return await PostAsync<ResponseData>("api/BinLocation/Merge", new
        {
            BinLocationInfo,
            systemOperator,
            Validate,
            NotifyOnce
        }).ConfigureAwait(false);
    }
}
