using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class ToolingTypeKafkaProxy : BaseKafkaProxy, IToolOperation
{
    public ToolingTypeKafkaProxy(
        IKafkaService kafkaService, 
        IConfiguration configuration, 
        ILogger<ToolingTypeKafkaProxy> logger)
        : base(kafkaService, configuration, logger, 
            "KafkaSettings:Topics:TOOLING_TYPE",           // appsettings key
            "shopfloor-toolingtype-sync")                  // fallback topic
    {
    }

    public async Task<List<ResponseData>> ListUpdateToolType(List<ToolTypeExternal> toolTypeList, List<ToolTypeExternal> toolTypeListOriginal, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.TOOLING_TYPE_SERVICE,
            "ListUpdateToolType",
            systemOperator,
            new
            {
                Data = toolTypeList,
                OriginalData = toolTypeListOriginal,
                Validate = Validate,
                Level = Level
            },
            logId).ConfigureAwait(false);

        return [result];
    }

    public List<ToolType> ListToolTypes(string ToolTypeCode, DateTime? DeltaDate = null)
    {
        throw new NotSupportedException("ListToolTypes is not supported through Kafka proxy.");
    }

    public List<Tool> ListTools(string ToolCode, DateTime? DeltaDate = null)
    {
        throw new NotSupportedException("ListTools is not supported through Kafka proxy.");
    }
}
