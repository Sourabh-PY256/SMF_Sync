using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Microsoft.Extensions.Configuration;

namespace EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;

/// <summary>
/// HTTP call-out to MachineAndDeviceGatewayService's EventRuleCacheController — answers
/// EventActionsMessageController's "RefreshSignal" relay, the Kafka-relay counterpart to the
/// reference project's EventActionMngRefreshConsumer calling DataTagService.UpdateCacheData.
/// </summary>
public class MachineAndDeviceGatewayOperationProxy : BaseHttpProxy
{
	public MachineAndDeviceGatewayOperationProxy(
		HttpClient httpClient,
		IConfiguration configuration,
		IAuthenticationService authService
	)
		: base(httpClient, configuration, authService, "ExternalServiceUrl") { }

	public Task ReloadEventRuleCacheAsync() =>
		PostAsync<object>("EventRuleCache/Reload", new { });
}
