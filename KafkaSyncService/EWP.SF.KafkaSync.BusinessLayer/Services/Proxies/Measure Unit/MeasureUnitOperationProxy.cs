using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using Microsoft.Extensions.Configuration;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using EWP.SF.KafkaSync.BusinessEntities;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP proxy for Measure Unit operations targeting the Measure Unit Microservice.
/// </summary>
public class MeasureUnitOperationProxy : BaseHttpProxy, IMeasureUnitOperation
{
    public MeasureUnitOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
    }

    public List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null)
        => throw new NotSupportedException("GetMeasureUnits is not available through the HTTP proxy.");

    public async Task<List<ResponseData>> ListUpdateUnitMeasure(
        List<MeasureUnitExternal> measureUnitInfoList, 
        List<MeasureUnitExternal> measureUnitInfoListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        // Endpoint: MeasureUnit/Bulk/{validate}/{level}
        string endpoint = $"MeasureUnit/{Validate.ToString().ToLower()}/{Level}";
        
        // Sending the list directly as the body
        return await PostAsync<List<ResponseData>>(endpoint, measureUnitInfoList).ConfigureAwait(false);
    }
}
