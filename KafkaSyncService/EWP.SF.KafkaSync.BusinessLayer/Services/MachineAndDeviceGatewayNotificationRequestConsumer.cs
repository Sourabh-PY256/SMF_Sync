using EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services;

/// <summary>
/// Listener for MachineAndDeviceGatewayService's "machineanddevicegateway.send-notification" (the
/// reference project's DataTagService.SendNotificationAction, fire-and-forget — no reply
/// expected). Calls NotificationService's existing Notifications/Systems/Send via
/// <see cref="NotificationOperationProxy"/>. Kept as its own file, same reasoning as
/// <see cref="ExternalSitesRequestConsumer"/>.
/// </summary>
public interface IMachineAndDeviceGatewayNotificationRequestConsumer
{
	void Start();
}

public class MachineAndDeviceGatewayNotificationRequestConsumer(
	IKafkaService kafkaService,
	IServiceScopeFactory scopeFactory,
	ILogger<MachineAndDeviceGatewayNotificationRequestConsumer> logger
) : IMachineAndDeviceGatewayNotificationRequestConsumer
{
	private const string RequestTopic = "machineanddevicegateway.send-notification";

	public void Start()
	{
		logger.LogInformation(
			"Starting MachineAndDeviceGateway notification consumer for topic: {Topic}",
			RequestTopic
		);

		kafkaService.StartConsumer(
			RequestTopic,
			async (correlationId, value) =>
			{
				logger.LogInformation(
					"[MachineAndDeviceGatewayNotificationRequest] Received request {CorrelationId}",
					correlationId
				);

				try
				{
					JToken request = JToken.Parse(value);

					using IServiceScope scope = scopeFactory.CreateScope();
					NotificationOperationProxy proxy =
						scope.ServiceProvider.GetRequiredService<NotificationOperationProxy>();

					await proxy.SendNotificationAsync(request).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(
						ex,
						"[MachineAndDeviceGatewayNotificationRequest] Failed to handle request {CorrelationId}",
						correlationId
					);
				}
			},
			groupId: "kafka-sync-machineanddevicegateway-notification-group"
		);
	}
}
