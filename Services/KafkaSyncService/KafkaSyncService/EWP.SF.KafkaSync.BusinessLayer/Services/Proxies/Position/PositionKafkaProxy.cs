using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.Common.Models.Catalogs;
using EWP.SF.KafkaSync.BusinessEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Kafka producer proxy for Profile (Position) operations.
/// </summary>
public class PositionKafkaProxy : BaseKafkaProxy, IProfileOperation
{
    public PositionKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger<PositionKafkaProxy> logger)
        : base(kafkaService, configuration, logger,
               "KafkaSettings:Topics:POSITION",     // appsettings key
               "shopfloor-position-sync")    // fallback topic
    { }

    public Task<ResponseData> MergeProfile(CatProfile ProfileInfo, User systemOperator, bool Validate = false, bool NotifyOnce = true)
        => throw new NotSupportedException("MergeProfile is not available through the Kafka proxy.");

    public async Task<List<ResponseData>> ListUpdateProfile(
        List<PositionExternal> profileInfoList, 
        List<PositionExternal> profileInfoListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        var result = await PublishAsync(
            SyncERPEntity.POSITION_SERVICE,
            "ListUpdateProfile",
            systemOperator,
            new
            {
                Data         = profileInfoList,
                OriginalData = profileInfoListOriginal,
                Validate,
                Level
            },
            null).ConfigureAwait(false);

        return [result];
    }
}
