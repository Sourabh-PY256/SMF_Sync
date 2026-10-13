using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP call-out to IntegrationServices' existing ExternalSitesController (via the Gateway's
/// "ExternalServiceUrl", same default every other OperationProxy here falls back to) — same
/// consume-then-call-out shape as every per-entity consumer in ServiceConsumerManager. No new
/// endpoint needed on the IntegrationServices side; it already serves GET externalsites/details.
/// </summary>
public class ExternalSitesOperationProxy : BaseHttpProxy
{
	public ExternalSitesOperationProxy(
		HttpClient httpClient,
		IConfiguration configuration,
		IAuthenticationService authService
	)
		: base(httpClient, configuration, authService, "ExternalServiceUrl") { }

	public Task<JToken> GetExternalSitesAsync(string code, string type) =>
		GetAsync<JToken>($"externalsites/details?code={Uri.EscapeDataString(code)}&type={Uri.EscapeDataString(type)}");
}
