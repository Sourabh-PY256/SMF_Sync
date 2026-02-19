using EWP.SF.Common.Enumerators;
using EWP.SF.Common.Models;
using EWP.SF.Common.ResponseModels;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

public class BinLocationKafkaProxy : IBinLocationOperation
{
    private readonly IKafkaService _kafkaService;
    private readonly ILogger<BinLocationKafkaProxy> _logger;
    private readonly string _inventorySyncTopic;

    public BinLocationKafkaProxy(
        IKafkaService kafkaService, 
        IConfiguration configuration, 
        ILogger<BinLocationKafkaProxy> logger)
    {
        _kafkaService = kafkaService;
        _logger = logger;
        _inventorySyncTopic = configuration["KafkaSettings:Topics:InventorySync"] ?? "inventory-sync-binlocation";
    }

    public async Task<List<ResponseData>> ListUpdateBinLocation(
        List<BinLocationExternal> binLocationList, 
        List<BinLocationExternal> binLocationListOriginal, 
        User systemOperator, 
        bool Validate, 
        LevelMessage Level)
    {
        _logger.LogInformation("BinLocationKafkaProxy: Offloading ListUpdateBinLocation to Kafka topic {Topic}", _inventorySyncTopic);

        var messageKey = $"BinLocationUpdate-{Guid.NewGuid()}";
        
        // Wrap the payload in a standard SyncMessage structure
        var payload = new SyncMessage
        {
            Service = SyncERPEntity.BIN_LOCATION_SERVICE,
            Trigger = "SmartFactory",
            ExecutionType = 1, // Event
            User = systemOperator,
            BodyData = JsonConvert.SerializeObject(new 
            {
                Action = "ListUpdateBinLocation",
                Data = binLocationList,
                OriginalData = binLocationListOriginal,
                Validate,
                Level
            })
        };

        await _kafkaService.ProduceMessageAsync(_inventorySyncTopic, messageKey, payload).ConfigureAwait(false);

        // Returning a generic "Accepted" response since it's asynchronous
        return new List<ResponseData> 
        { 
            new ResponseData 
            { 
                IsSuccess = true, 
                Message = "Operation accepted and queued in Kafka",
                Code = "KafkaSync"
            } 
        };
    }

    public async Task<ResponseData> MergeBinLocation(
        BinLocation BinLocationInfo, 
        User systemOperator, 
        bool Validate = false, 
        bool NotifyOnce = true)
    {
        _logger.LogInformation("BinLocationKafkaProxy: Offloading MergeBinLocation to Kafka topic {Topic}", _inventorySyncTopic);

        var messageKey = $"BinLocationMerge-{Guid.NewGuid()}";
        
        var payload = new SyncMessage
        {
            Service = SyncERPEntity.BIN_LOCATION_SERVICE,
            Trigger = "SmartFactory",
            ExecutionType = 1, // Event
            User = systemOperator,
            BodyData = JsonConvert.SerializeObject(new 
            {
                Action = "MergeBinLocation",
                Data = BinLocationInfo,
                Validate,
                NotifyOnce
            })
        };

        await _kafkaService.ProduceMessageAsync(_inventorySyncTopic, messageKey, payload).ConfigureAwait(false);

        return new ResponseData 
        { 
            IsSuccess = true, 
            Message = "Operation accepted and queued in Kafka",
            Code = "KafkaSync"
        };
    }
}
