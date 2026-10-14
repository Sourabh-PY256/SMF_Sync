using Newtonsoft.Json.Linq;
using System.Text.Json;
using EWP.SF.KafkaSync.BusinessEntities.Models;
using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace EWP.SF.KafkaSync.API;

/// <summary>
/// Kafka relay for Notification's own internal publish loop — replaces the reference project's
/// raw Garnet LPUSH in GarnetMessageProducer (used by both DataTagService.SendTagNotification's
/// live-value broadcasts and NotificationConsumer's resolved-alert messages, which both fed the
/// same "notification-queue"). A pure relay — channel dispatch (Email/SMS/Teams/Push) and SSE
/// broadcast stay entirely inside NotificationService, which owns the consumer for this topic.
/// </summary>
[ApiController]
[Route("API/V1")]
public class NotificationMessageController(IKafkaService kafkaService) : BaseController
{
	/// <summary>
	/// Publishes a notification message (live tag-value broadcast or a resolved targeted alert) —
	/// reference project's single "notification-queue" funnel. Fire-and-forget: the consumer
	/// (NotificationService) decides per-message whether it's a dashboard broadcast, a channel
	/// alert, or both.
	/// </summary>
	[HttpPost("NotificationMessage/Publish")]
	public async Task<ResponseModel> Publish([FromBody] JToken body)
	{
		await kafkaService
			.ProduceMessageAsync("notification.publish", Guid.NewGuid().ToString(), RelayJson.ToElement(body))
			.ConfigureAwait(false);

		return new ResponseModel { IsSuccess = true };
	}
}
