using System.Text.Json;
using EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services;

/// <summary>
/// Listener side of MachineAndDeviceGatewayMessageController's EventActionsService-bound relays —
/// consumes the 5 RPC request topics plus the 1 fire-and-forget topic, calls EventActionsService's
/// existing controllers via <see cref="EventActionsOperationProxy"/>, and (for the RPC ones)
/// publishes the result onto the matching response topic keyed by the same correlation id. Kept as
/// its own file, same reasoning as <see cref="ExternalSitesRequestConsumer"/> — answers the
/// reference project's EvaluationConditionConsumer / ActionTypeConsumer / EventTypeConsumer /
/// EventActionMngConsumerListener / EventActionExecuteLogListener / EventActionExecuteDetailLogConsumer.
/// </summary>
public interface IMachineAndDeviceGatewayEventActionsRequestConsumer
{
	void Start();
}

public class MachineAndDeviceGatewayEventActionsRequestConsumer(
	IKafkaService kafkaService,
	IServiceScopeFactory scopeFactory,
	ILogger<MachineAndDeviceGatewayEventActionsRequestConsumer> logger
) : IMachineAndDeviceGatewayEventActionsRequestConsumer
{
	private const string GroupId = "kafka-sync-machineanddevicegateway-eventactions-group";

	public void Start()
	{
		StartRpc(
			"machineanddevicegateway.evaluate-condition.request",
			"machineanddevicegateway.evaluate-condition.response",
			(proxy, request) => proxy.EvaluateConditionAsync(request)
		);

		StartRpc(
			"machineanddevicegateway.fetch-action-types.request",
			"machineanddevicegateway.fetch-action-types.response",
			(proxy, request) => proxy.FetchActionTypesAsync(CodeOf(request))
		);

		StartRpc(
			"machineanddevicegateway.fetch-event-types.request",
			"machineanddevicegateway.fetch-event-types.response",
			(proxy, request) => proxy.FetchEventTypesAsync(CodeOf(request))
		);

		StartRpc(
			"machineanddevicegateway.fetch-event-action-rules.request",
			"machineanddevicegateway.fetch-event-action-rules.response",
			(proxy, request) => proxy.FetchEventActionRulesAsync(CodeOf(request))
		);

		StartRpc(
			"machineanddevicegateway.execute-log.request",
			"machineanddevicegateway.execute-log.response",
			(proxy, request) => proxy.SaveExecuteLogAsync(request)
		);

		StartFireAndForget(
			"machineanddevicegateway.execute-detail-log",
			(proxy, request) => proxy.SaveExecuteDetailLogAsync(request)
		);
	}

	private static string CodeOf(JToken request) => request.Value<string>("Code") ?? string.Empty;

	private void StartRpc(
		string requestTopic,
		string responseTopic,
		Func<EventActionsOperationProxy, JToken, Task<JToken>> call
	)
	{
		logger.LogInformation(
			"Starting MachineAndDeviceGateway->EventActions request consumer for topic: {Topic}",
			requestTopic
		);

		kafkaService.StartConsumer(
			requestTopic,
			async (correlationId, value) =>
			{
				logger.LogInformation(
					"[MachineAndDeviceGatewayEventActionsRequest:{Topic}] Received request {CorrelationId}",
					requestTopic,
					correlationId
				);

				JToken result;
				try
				{
					JToken request = JToken.Parse(value);

					using IServiceScope scope = scopeFactory.CreateScope();
					EventActionsOperationProxy proxy =
						scope.ServiceProvider.GetRequiredService<EventActionsOperationProxy>();

					result = await call(proxy, request).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(
						ex,
						"[MachineAndDeviceGatewayEventActionsRequest:{Topic}] Failed to handle request {CorrelationId}",
						requestTopic,
						correlationId
					);
					result = new JObject { ["__relayError"] = ex.Message };
				}

				// ProduceMessageAsync serializes with System.Text.Json, but `result` is a Newtonsoft
				// JToken (BaseHttpProxy's native type) — round-trip it through JsonElement so it
				// serializes as plain JSON rather than JToken's own object shape.
				JsonElement payload = JsonDocument
					.Parse(result.ToString(Newtonsoft.Json.Formatting.None))
					.RootElement;

				await kafkaService
					.ProduceMessageAsync(responseTopic, correlationId, payload)
					.ConfigureAwait(false);
			},
			groupId: GroupId
		);
	}

	private void StartFireAndForget(
		string requestTopic,
		Func<EventActionsOperationProxy, JToken, Task<JToken>> call
	)
	{
		logger.LogInformation(
			"Starting MachineAndDeviceGateway->EventActions request consumer for topic: {Topic}",
			requestTopic
		);

		kafkaService.StartConsumer(
			requestTopic,
			async (correlationId, value) =>
			{
				logger.LogInformation(
					"[MachineAndDeviceGatewayEventActionsRequest:{Topic}] Received request {CorrelationId}",
					requestTopic,
					correlationId
				);

				try
				{
					JToken request = JToken.Parse(value);

					using IServiceScope scope = scopeFactory.CreateScope();
					EventActionsOperationProxy proxy =
						scope.ServiceProvider.GetRequiredService<EventActionsOperationProxy>();

					await call(proxy, request).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(
						ex,
						"[MachineAndDeviceGatewayEventActionsRequest:{Topic}] Failed to handle request {CorrelationId}",
						requestTopic,
						correlationId
					);
				}
			},
			groupId: GroupId
		);
	}
}
