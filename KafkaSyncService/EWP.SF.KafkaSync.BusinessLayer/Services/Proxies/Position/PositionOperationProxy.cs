using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models.Catalogs;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Profile (Position) operations targeting the Position Microservice.
/// </summary>
public class PositionOperationProxy : BaseHttpProxy, IProfileOperation
{
    public PositionOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public Task<ResponseData> MergeProfile(CatProfile ProfileInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
        => throw new NotSupportedException("MergeProfile is not available through the HTTP proxy.");

    public async Task<List<ResponseData>> ListUpdateProfile(
        List<PositionExternal> profileInfoList, 
        List<PositionExternal> profileInfoListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        // Endpoint: Position/Bulk/{validate}/{level}
        string endpoint = $"Position/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list directly as the body
        return await PostAsync<List<ResponseData>>(endpoint, profileInfoList).ConfigureAwait(false);
    }
}
