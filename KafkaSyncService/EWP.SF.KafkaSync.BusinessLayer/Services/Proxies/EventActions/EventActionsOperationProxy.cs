using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP call-out to EventActionsService's existing controllers (via SMFGatewayService's
/// "ExternalServiceUrl" default, same as every other OperationProxy here) — answers
/// MachineAndDeviceGatewayMessageController's relay topics. Only EvaluateLive is a new endpoint
/// (EventActionManagerController.EvaluateConditionLive); the rest are pre-existing.
/// </summary>
public class EventActionsOperationProxy : BaseHttpProxy
{
	public EventActionsOperationProxy(
		HttpClient httpClient,
		IConfiguration configuration,
		IAuthenticationService authService
	)
		: base(httpClient, configuration, authService, "ExternalServiceUrl") { }

	public Task<JToken> EvaluateConditionAsync(JToken request) =>
		PostAsync<JToken>("EventActions/EventActionManager/EvaluateLive", request);

	public Task<JToken> FetchActionTypesAsync(string code) =>
		GetAsync<JToken>($"EventActions/ActionType?code={Uri.EscapeDataString(code)}");

	public Task<JToken> FetchEventTypesAsync(string code) =>
		GetAsync<JToken>($"EventActions/EventType?code={Uri.EscapeDataString(code)}");

	public Task<JToken> FetchEventActionRulesAsync(string code) =>
		GetAsync<JToken>($"EventActions/EventActionManager?code={Uri.EscapeDataString(code)}");

	public Task<JToken> SaveExecuteLogAsync(JToken request) =>
		PostAsync<JToken>("EventActions/EventActionExecuteLog", request);

	public Task<JToken> SaveExecuteDetailLogAsync(JToken request) =>
		PostAsync<JToken>("EventActions/EventActionExecuteLog/Detail", request);
}
