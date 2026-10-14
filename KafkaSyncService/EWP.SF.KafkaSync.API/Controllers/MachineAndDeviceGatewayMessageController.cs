using Newtonsoft.Json.Linq;
using System.Text.Json;
using EWP.SF.KafkaSync.BusinessEntities.Models;
using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.AspNetCore.Mvc;

namespace EWP.SF.KafkaSync.API;

/// <summary>
/// Kafka relay for MachineAndDeviceGatewayService's calls into EventActionsService and
/// NotificationService — replaces the reference project's Garnet/Redis-based
/// IInternalRequest.SendCommandAsync/SendRequestAsync calls from DataTagService with Kafka
/// produce (and, for the RPC-style endpoints, a correlated consume on the matching response
/// topic). Each endpoint here is a pure relay: it carries no EventAction/Notification business
/// logic of its own, which lives entirely in the services that consume these topics.
/// </summary>
[ApiController]
[Route("API/V1")]
public class MachineAndDeviceGatewayMessageController(IKafkaService kafkaService) : BaseController
{
	private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

	#region RPC (request topic, then a correlated wait on the response topic)

	/// <summary>Evaluates a condition expression — reference project's EvaluationConditionConsumer.</summary>
	[HttpPost("MachineAndDeviceGatewayMessage/EvaluateCondition")]
	public Task<ResponseModel> EvaluateCondition([FromBody] JToken body) =>
		RelayRequestAsync(
			"machineanddevicegateway.evaluate-condition.request",
			"machineanddevicegateway.evaluate-condition.response",
			RelayJson.ToElement(body)
		);

	/// <summary>Fetches the action-type catalog — reference project's ActionTypeConsumer.</summary>
	[HttpPost("MachineAndDeviceGatewayMessage/FetchActionTypes")]
	public Task<ResponseModel> FetchActionTypes([FromBody] JToken body) =>
		RelayRequestAsync(
			"machineanddevicegateway.fetch-action-types.request",
			"machineanddevicegateway.fetch-action-types.response",
			RelayJson.ToElement(body)
		);

	/// <summary>Fetches the event-type catalog — reference project's EventTypeConsumer.</summary>
	[HttpPost("MachineAndDeviceGatewayMessage/FetchEventTypes")]
	public Task<ResponseModel> FetchEventTypes([FromBody] JToken body) =>
		RelayRequestAsync(
			"machineanddevicegateway.fetch-event-types.request",
			"machineanddevicegateway.fetch-event-types.response",
			RelayJson.ToElement(body)
		);

	/// <summary>Fetches the current event-action rule list — reference project's EventActionMngConsumerListener.</summary>
	[HttpPost("MachineAndDeviceGatewayMessage/FetchEventActionRules")]
	public Task<ResponseModel> FetchEventActionRules([FromBody] JToken body) =>
		RelayRequestAsync(
			"machineanddevicegateway.fetch-event-action-rules.request",
			"machineanddevicegateway.fetch-event-action-rules.response",
			RelayJson.ToElement(body)
		);

	/// <summary>Persists an execution-log entry and returns its id — reference project's EventActionExecuteLogListener.</summary>
	[HttpPost("MachineAndDeviceGatewayMessage/ExecuteLog")]
	public Task<ResponseModel> ExecuteLog([FromBody] JToken body) =>
		RelayRequestAsync(
			"machineanddevicegateway.execute-log.request",
			"machineanddevicegateway.execute-log.response",
			RelayJson.ToElement(body)
		);

	#endregion

	#region Fire-and-forget (produce only, no reply expected)

	/// <summary>Persists a subaction execution-detail log — reference project's EventActionExecuteDetailLogConsumer.</summary>
	[HttpPost("MachineAndDeviceGatewayMessage/ExecuteDetailLog")]
	public Task<ResponseModel> ExecuteDetailLog([FromBody] JToken body) =>
		RelayPublishAsync("machineanddevicegateway.execute-detail-log", RelayJson.ToElement(body));

	/// <summary>Requests a targeted notification be sent — reference project's NotificationConsumer.</summary>
	[HttpPost("MachineAndDeviceGatewayMessage/SendNotification")]
	public Task<ResponseModel> SendNotification([FromBody] JToken body) =>
		RelayPublishAsync("machineanddevicegateway.send-notification", RelayJson.ToElement(body));

	#endregion

	private async Task<ResponseModel> RelayRequestAsync(
		string requestTopic,
		string responseTopic,
		JsonElement request
	)
	{
		string correlationId = Guid.NewGuid().ToString();
		JsonElement response = await kafkaService
			.RequestAsync<JsonElement, JsonElement>(
				requestTopic,
				correlationId,
				request,
				responseTopic,
				DefaultTimeout,
				result => result.Message.Key == correlationId
			)
			.ConfigureAwait(false);

		// ConsumeMessageAsync returns default (ValueKind Undefined) on timeout, and the request
		// consumer replies {"__relayError": ...} when its downstream call failed — surface both as a
		// failed ResponseModel instead of an empty "success", so callers (e.g. the cache reload)
		// can't mistake a timeout / downstream 500 for "zero rules".
		if (response.ValueKind == JsonValueKind.Undefined)
		{
			return new ResponseModel
			{
				IsSuccess = false,
				ErrorCode = "RelayTimeout",
				Message = $"No reply on '{responseTopic}' within {DefaultTimeout.TotalSeconds:0}s.",
			};
		}

		if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("__relayError", out JsonElement relayError))
		{
			return new ResponseModel
			{
				IsSuccess = false,
				ErrorCode = "RelayHandlerFailed",
				Message = relayError.ToString(),
			};
		}


		return new ResponseModel { Data = RelayJson.ToToken(response) };
	}

	private async Task<ResponseModel> RelayPublishAsync(string topic, JsonElement request)
	{
		await kafkaService
			.ProduceMessageAsync(topic, Guid.NewGuid().ToString(), request)
			.ConfigureAwait(false);

		return new ResponseModel { IsSuccess = true };
	}
}
