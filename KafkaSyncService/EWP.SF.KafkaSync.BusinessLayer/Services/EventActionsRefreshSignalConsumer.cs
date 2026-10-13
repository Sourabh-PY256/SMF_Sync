using EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.BusinessLayer.Services;

/// <summary>
/// Listener side of EventActionsMessageController's RefreshSignal relay — consumes
/// "event-actions.refresh-signal" and calls MachineAndDeviceGatewayService's
/// EventRuleCacheController.Reload via <see cref="MachineAndDeviceGatewayOperationProxy"/>. The
/// Kafka-relay counterpart to the reference project's EventActionMngRefreshConsumer calling
/// DataTagService.UpdateCacheData — fire-and-forget, no reply expected. Unlike the reference
/// project's entity-scoped refresh, MachineAndDeviceGatewayService holds a single combined cache
/// (see EventRuleCacheController's own doc comment), so every signal triggers a full reload
/// regardless of which entity changed.
/// </summary>
public interface IEventActionsRefreshSignalConsumer
{
	void Start();
}

public class EventActionsRefreshSignalConsumer(
	IKafkaService kafkaService,
	IServiceScopeFactory scopeFactory,
	ILogger<EventActionsRefreshSignalConsumer> logger
) : IEventActionsRefreshSignalConsumer
{
	private const string RequestTopic = "event-actions.refresh-signal";

	public void Start()
	{
		logger.LogInformation("Starting EventActions refresh-signal consumer for topic: {Topic}", RequestTopic);

		kafkaService.StartConsumer(
			RequestTopic,
			async (correlationId, value) =>
			{
				logger.LogInformation(
					"[EventActionsRefreshSignal] Received signal {CorrelationId}",
					correlationId
				);

				try
				{
					using IServiceScope scope = scopeFactory.CreateScope();
					MachineAndDeviceGatewayOperationProxy proxy =
						scope.ServiceProvider.GetRequiredService<MachineAndDeviceGatewayOperationProxy>();

					await proxy.ReloadEventRuleCacheAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(
						ex,
						"[EventActionsRefreshSignal] Failed to handle signal {CorrelationId}",
						correlationId
					);
				}
			},
			groupId: "kafka-sync-eventactions-refresh-group"
		);
	}
}
