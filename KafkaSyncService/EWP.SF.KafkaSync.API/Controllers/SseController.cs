using EWP.SF.Common.Models;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessEntities.Kafka;
using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace EWP.SF.KafkaSync.API;
public sealed class SseController(ISseService sseService, ILogger<SseController> logger)
{
    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/subscribe", Subscribe);
        group.MapGet("/debug", Debug);
    }

    private async Task Subscribe(HttpContext httpContext)
    {
        logger.LogInformation("SSE: Client {IP} subscribing", httpContext.Connection.RemoteIpAddress);
        await sseService.SubscribeAsync(httpContext).ConfigureAwait(false);
    }

    private async Task Debug(HttpContext httpContext)
    {
        logger.LogInformation("SSE (debug): Client {IP} subscribing", httpContext.Connection.RemoteIpAddress);
        await sseService.SubscribeDebugAsync(httpContext).ConfigureAwait(false);
    }
}