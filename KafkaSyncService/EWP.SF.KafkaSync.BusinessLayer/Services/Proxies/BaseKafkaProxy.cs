using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// Base class for all Kafka producer proxies.
/// Encapsulates common Kafka message building and publishing logic.
/// Extend this class for each microservice domain (Inventory, Production, etc.)
/// </summary>
public abstract class BaseKafkaProxy
{
    protected readonly IKafkaService _kafkaService;
    protected readonly ILogger _logger;
    protected readonly string _topic;

    protected BaseKafkaProxy(
        IKafkaService kafkaService,
        IConfiguration configuration,
        ILogger logger,
        string topicConfigKey,
        string defaultTopic)
    {
        _kafkaService = kafkaService;
        _logger = logger;
        _topic = configuration[topicConfigKey] ?? defaultTopic;
    }

    /// <summary>
    /// Builds a SyncMessage and publishes it to the configured Kafka topic.
    /// All Kafka proxy methods should call this instead of duplicating publish logic.
    /// </summary>
    protected async Task<EWP.SF.Common.ResponseModels.ResponseData> PublishAsync(
        string serviceCode,
        string action,
        User user,
        object payload,
        string logId = null)
    {
        var messageKey = $"{action}-{Guid.NewGuid()}";

        // Merge Action with the payload's own properties at the root level.
        // The proxy callers already shape payload as { Data, OriginalData, Validate, ... },
        // so we must NOT nest it under another "Data" key — that would double-wrap and
        // break consumer deserialization (body.Data would be an object, not the list).
        var bodyObj = JObject.FromObject(payload, JsonSerializer.CreateDefault());
        bodyObj["Action"] = action;

        var message = new SyncMessage
        {
            Service       = serviceCode,
            Trigger       = "SmartFactory",
            ExecutionType = 1, // Event
            User          = user,
            LogId         = logId,
            BodyData      = bodyObj.ToString(Formatting.None)
        };

        _logger.LogInformation(
            "[KafkaProxy] Publishing {Action} → topic: {Topic} | LogId: {LogId}",
            action, _topic, logId);

        await _kafkaService.ProduceMessageAsync(_topic, messageKey, message)
                           .ConfigureAwait(false);

        return new EWP.SF.Common.ResponseModels.ResponseData
        {
            IsSuccess = true,
            Message   = $"{action} accepted and queued in Kafka",
            Code      = "KafkaSync"
        };
    }
}
