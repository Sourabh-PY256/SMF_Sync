using System.Text.Json;
using EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services;

/// <summary>
/// Listener side of EventActionsMessageController's FetchExternalSites relay — consumes
/// "event-actions.fetch-external-sites.request", calls IntegrationServices' existing
/// ExternalSitesController via <see cref="ExternalSitesOperationProxy"/>, and publishes the
/// result onto "event-actions.fetch-external-sites.response" keyed by the same correlation id so
/// the waiting caller's predicate-filtered consume matches it. Kept as its own file rather than
/// folded into ServiceConsumerManager, which is scoped to ERP sync entities.
/// </summary>
public interface IExternalSitesRequestConsumer
{
	void Start();
}

public class ExternalSitesRequestConsumer(
	IKafkaService kafkaService,
	IServiceScopeFactory scopeFactory,
	ILogger<ExternalSitesRequestConsumer> logger
) : IExternalSitesRequestConsumer
{
	private const string RequestTopic = "event-actions.fetch-external-sites.request";
	private const string ResponseTopic = "event-actions.fetch-external-sites.response";

	public void Start()
	{
		logger.LogInformation(
			"Starting ExternalSites request consumer for topic: {Topic}",
			RequestTopic
		);

		kafkaService.StartConsumer(
			RequestTopic,
			async (correlationId, value) =>
			{
				logger.LogInformation(
					"[ExternalSitesRequest] Received request {CorrelationId}",
					correlationId
				);

				JToken result;
				try
				{
					JObject request = JObject.Parse(value);
					string code = request.Value<string>("Code") ?? string.Empty;
					string type = request.Value<string>("Type") ?? string.Empty;

					using IServiceScope scope = scopeFactory.CreateScope();
					ExternalSitesOperationProxy proxy =
						scope.ServiceProvider.GetRequiredService<ExternalSitesOperationProxy>();

					result = await proxy.GetExternalSitesAsync(code, type).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(
						ex,
						"[ExternalSitesRequest] Failed to handle request {CorrelationId}",
						correlationId
					);
					result = new JArray();
				}

				// ProduceMessageAsync serializes with System.Text.Json, but `result` is a
				// Newtonsoft JToken (BaseHttpProxy's native type) — round-trip it through
				// JsonElement so it serializes as plain JSON rather than JToken's own object shape.
				JsonElement payload = JsonDocument
					.Parse(result.ToString(Newtonsoft.Json.Formatting.None))
					.RootElement;

				await kafkaService
					.ProduceMessageAsync(ResponseTopic, correlationId, payload)
					.ConfigureAwait(false);
			},
			groupId: "kafka-sync-external-sites-group"
		);
	}
}
