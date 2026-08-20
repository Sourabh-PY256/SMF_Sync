using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Measure Unit operations.
/// </summary>
public class MeasureUnitKafkaProxy : BaseKafkaProxy, IMeasureUnitOperation
{
    public MeasureUnitKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<MeasureUnitKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:MEASUREUNIT",     // appsettings key
               "shopfloor-measureunit-sync")    // fallback topic
    { }

    public List<MeasureUnit> GetMeasureUnits(UnitType? unitType = null, string unitId = null, DateTime? DeltaDate = null)
        => throw new NotSupportedException("GetMeasureUnits is not available through the Kafka proxy.");

    public async Task<List<ResponseData>> ListUpdateUnitMeasure(
        List<MeasureUnitExternal> measureUnitInfoList, 
        List<MeasureUnitExternal> measureUnitInfoListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.UNIT_MEASURE_SERVICE,
            "ListUpdateUnitMeasure",
            systemOperator,
            new
            {
                Data         = measureUnitInfoList,
                OriginalData = measureUnitInfoListOriginal,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return [result];
    }
}
