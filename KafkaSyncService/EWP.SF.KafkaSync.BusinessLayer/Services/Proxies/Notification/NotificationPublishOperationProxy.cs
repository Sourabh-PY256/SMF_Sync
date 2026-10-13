using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP call-out to NotificationService's new SseController.Publish — answers
/// NotificationMessageController's "notification.publish" relay, the Kafka-relay counterpart to
/// the reference project's AppNotificationService.SendNotificationAsync (which the reference
/// project's GarnetConsumerHostedService fed from the "notification-queue" Garnet list).
/// </summary>
public class NotificationPublishOperationProxy : BaseHttpProxy
{
	public NotificationPublishOperationProxy(
		HttpClient httpClient,
		IConfiguration configuration,
		IAuthenticationService authService
	)
		: base(httpClient, configuration, authService, "ExternalServiceUrl") { }

	public Task<object> PublishAsync(object request) => PostAsync<object>("Sse/Publish", request);
}
