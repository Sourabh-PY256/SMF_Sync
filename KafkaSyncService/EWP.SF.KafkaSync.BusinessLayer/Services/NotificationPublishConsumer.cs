using EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services;

/// <summary>
/// Listener side of NotificationMessageController's Publish relay — consumes
/// "notification.publish" and calls NotificationService's new SseController.Publish via
/// <see cref="NotificationPublishOperationProxy"/>, which broadcasts to every connected SSE
/// client. The Kafka-relay counterpart to the reference project's GarnetConsumerHostedService
/// delivering a "notification-queue" message to AppNotificationService — fire-and-forget, no
/// reply expected.
/// </summary>
public interface INotificationPublishConsumer
{
	void Start();
}

public class NotificationPublishConsumer(
	IKafkaService kafkaService,
	IServiceScopeFactory scopeFactory,
	ILogger<NotificationPublishConsumer> logger
) : INotificationPublishConsumer
{
	private const string RequestTopic = "notification.publish";

	public void Start()
	{
		logger.LogInformation("Starting notification.publish consumer for topic: {Topic}", RequestTopic);

		kafkaService.StartConsumer(
			RequestTopic,
			async (correlationId, value) =>
			{
				logger.LogInformation(
					"[NotificationPublish] Received message {CorrelationId}",
					correlationId
				);

				try
				{
					JToken request = JToken.Parse(value);

					using IServiceScope scope = scopeFactory.CreateScope();
					NotificationPublishOperationProxy proxy =
						scope.ServiceProvider.GetRequiredService<NotificationPublishOperationProxy>();

					await proxy.PublishAsync(request).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(
						ex,
						"[NotificationPublish] Failed to handle message {CorrelationId}",
						correlationId
					);
				}
			},
			groupId: "kafka-sync-notification-publish-group"
		);
	}
}
