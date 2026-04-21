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
    private readonly bool _use2503ForSync;
    public MeasureUnitOperationProxy(HttpClient httpClient, IConfiguration configuration, IAuthenticationService authService)
        : base(httpClient, configuration, authService, "ExternalServiceUrl")
    {
        _use2503ForSync = configuration.GetValue<bool>("AppSettings:Use2503ForSync");
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
        if (_use2503ForSync)
        {
            return await PostAsyncPO<List<ResponseData>>(endpoint, measureUnitInfoList).ConfigureAwait(false);
        }
        else
        {
            return await PostAsync<List<ResponseData>>(endpoint, measureUnitInfoList).ConfigureAwait(false);
        }
    }
}
