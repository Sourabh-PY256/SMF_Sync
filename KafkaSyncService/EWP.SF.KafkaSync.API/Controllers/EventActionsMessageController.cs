using Newtonsoft.Json.Linq;
using System.Text.Json;
using EWP.SF.KafkaSync.BusinessEntities.Models;
using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace EWP.SF.KafkaSync.API;

/// <summary>
/// Kafka relay for EventActionsService's own outbound calls — replaces the reference project's
/// Garnet/Redis IInternalRequest.SendRequestAsync/SendCommandAsync calls from
/// EventActionManagerService and EventTypeService. A pure relay, same as
/// <see cref="MachineAndDeviceGatewayMessageController"/> — no EventAction/ExternalSites business logic here.
/// </summary>
[ApiController]
[Route("API/V1")]
public class EventActionsMessageController(IKafkaService kafkaService) : BaseController
{
	private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Publishes "a rule changed, reload your cache" — reference project's
	/// EventActionManagerService publishing to "CacheRefresh.EventActionManager", consumed by
	/// Gateway's EventActionMngRefreshConsumer (fire-and-forget, no reply expected).
	/// </summary>
	[HttpPost("EventActionsMessage/RefreshSignal")]
	public async Task<ResponseModel> RefreshSignal([FromBody] JToken body)
	{
		await kafkaService
			.ProduceMessageAsync("event-actions.refresh-signal", Guid.NewGuid().ToString(), RelayJson.ToElement(body))
			.ConfigureAwait(false);

		return new ResponseModel { IsSuccess = true };
	}

	/// <summary>
	/// Fetches external-site connection details — reference project's EventTypeService.GetApiValues
	/// calling SendCommandAsync("ExternalSiteConsumerListener", ...), answered by ExternalSites'
	/// own ExternalSitesListener.
	/// </summary>
	[HttpPost("EventActionsMessage/FetchExternalSites")]
	public async Task<ResponseModel> FetchExternalSites([FromBody] JToken body)
	{
		string correlationId = Guid.NewGuid().ToString();
		JsonElement response = await kafkaService
			.RequestAsync<JsonElement, JsonElement>(
				"event-actions.fetch-external-sites.request",
				correlationId,
				RelayJson.ToElement(body),
				"event-actions.fetch-external-sites.response",
				DefaultTimeout,
				result => result.Message.Key == correlationId
			)
			.ConfigureAwait(false);

		return new ResponseModel { Data = RelayJson.ToToken(response) };
	}
}
