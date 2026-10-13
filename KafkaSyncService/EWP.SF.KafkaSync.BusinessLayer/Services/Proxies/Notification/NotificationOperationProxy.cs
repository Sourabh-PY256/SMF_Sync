using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP call-out to NotificationService's existing Notifications/Systems/Send (via the Gateway's
/// "ExternalServiceUrl" default, same as every other OperationProxy here) — same
/// consume-then-call-out shape as <see cref="ExternalSitesOperationProxy"/>. That endpoint already
/// does resolve + synchronous Email/MsTeams send in one call (see its own doc comment for why: the
/// reference project's queue-based async dispatch was never implemented here, so this is the
/// closest existing equivalent — not a new relay-specific endpoint).
/// </summary>
public class NotificationOperationProxy : BaseHttpProxy
{
	public NotificationOperationProxy(
		HttpClient httpClient,
		IConfiguration configuration,
		IAuthenticationService authService
	)
		: base(httpClient, configuration, authService, "ExternalServiceUrl") { }

	public Task<JToken> SendNotificationAsync(JToken request) =>
		PostAsync<JToken>("Notifications/Systems/Send", request);
}
