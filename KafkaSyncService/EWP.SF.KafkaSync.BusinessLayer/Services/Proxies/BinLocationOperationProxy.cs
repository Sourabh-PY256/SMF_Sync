using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class BinLocationOperationProxy : BaseHttpProxy, IBinLocationOperation
{
    public BinLocationOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService) 
        : base(httpClient, configuration, authService, "InventoryServiceUrl")
    {
    }

    public async Task<List<ResponseData>> ListUpdateBinLocation(
        List<BinLocationExternal> binLocationList, 
        List<BinLocationExternal> binLocationListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        // Redirecting to external microservice endpoint with path parameters
        // Example URL: API/V1/BinLocation/true/Record
        // The microservice expects [FromBody] List<BinLocationExternal> request
        string endpoint = $"BinLocation/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list directly as the body (not wrapped in an object)
        return await PostAsync<List<ResponseData>>(endpoint, binLocationList).ConfigureAwait(false);
    }

    public async Task<ResponseData> MergeBinLocation(
        BinLocation BinLocationInfo, 
        User systemOperator, 
        bool Validate = false, 
        bool NotifyOnce = true)
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
