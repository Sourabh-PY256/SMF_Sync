using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class MoveEntryKafkaProxy : BaseKafkaProxy, IMoveEntryOperation
{
    public MoveEntryKafkaProxy(
        IKafkaService kafkaService, 
        IConfiguration configuration, 
        ILogger<MoveEntryKafkaProxy> logger)
        : base(kafkaService, configuration, logger, 
            "KafkaSettings:Topics:MOVE_ENTRY",       // appsettings key
            "shopfloor-moveentry-sync")              // fallback topic
    {
    }

    public async Task<List<ResponseData>> ListUpdateMoveEntry(List<MoveEntryExternal> moveEntryList, User systemOperator, bool Validate, LevelMessage Level, string logId = null)
    {
        var result = await PublishAsync(
            SyncERPEntity.MOVE_ENTRY_SERVICE,
            "ListUpdateMoveEntry",
            systemOperator,
            new
            {
                Data = moveEntryList,
                Validate = Validate,
                Level = Level
            },
            logId).ConfigureAwait(false);

        return [new ResponseData { IsSuccess = result.IsSuccess, Message = result.Message }];
    }
}
