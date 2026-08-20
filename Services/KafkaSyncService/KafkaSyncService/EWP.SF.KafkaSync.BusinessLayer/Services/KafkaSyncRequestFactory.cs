using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.KafkaSync.BusinessLayer;
using EWP.SF.KafkaSync.BusinessLayer.Services;
using Microsoft.Extensions.Logging;

public class KafkaSyncRequestFactory
{
    private readonly IKafkaService _kafkaService;
    private readonly ISyncCompletionRegistry _registry;
    private readonly ILogger<KafkaSyncRequestFactory> _logger;

    public KafkaSyncRequestFactory(
        IKafkaService kafkaService,
        ISyncCompletionRegistry registry,
        ILogger<KafkaSyncRequestFactory> logger)
    {
        _kafkaService = kafkaService;
        _registry = registry;
        _logger = logger;
    }

    public async Task<DataSyncHttpResponse> SendAndWaitAsync(
        string service,
        SyncMessage message,
        TimeSpan timeout)
    {
        string correlationId = message.CorrelationId;

        using var cts = new CancellationTokenSource(timeout);

        // ✅ Register BEFORE produce
        var tcs = _registry.Register(correlationId, cts.Token);

        string topic = $"producer-syncSSE-{service.ToLower()}";

        await _kafkaService.ProduceMessageAsync(
            topic,
            $"producer-{service}-{correlationId}",
            message
        );

        _logger.LogInformation("Sent Kafka request {CorrelationId}", correlationId);

        try
        {
            return await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Timeout for {correlationId}");
        }
    }
}