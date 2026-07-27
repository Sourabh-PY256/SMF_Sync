using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EWP.SF.KafkaSync.BusinessLayer;

public sealed class SseService : ISseService
{
    private readonly Lock _clientsGate = new();
    private readonly List<HttpResponse> _clients = [];
    private readonly List<HttpResponse> _debugClients = [];

    public async Task SubscribeAsync(HttpContext context)
    {
        HttpResponse response = context.Response;
        ConfigureSseResponse(response, serviceWorkerAllowed: "/notification/subscribe");
        lock (_clientsGate)
        {
            _clients.Add(response);
        }

        string jsonConnection = JsonSerializer.Serialize(new { Message = "connected to notification SSE" });
        await WriteSseEventAsync(response, "connection", jsonConnection, context.RequestAborted).ConfigureAwait(false);

        context.RequestAborted.WaitHandle.WaitOne();
        lock (_clientsGate)
        {
            _clients.Remove(response);
        }
    }

    public async Task SubscribeDebugAsync(HttpContext context)
    {
        HttpResponse response = context.Response;
        ConfigureSseResponse(response, serviceWorkerAllowed: "/notification/debug");
        lock (_clientsGate)
        {
            _debugClients.Add(response);
        }

        string jsonConnection = JsonSerializer.Serialize(new { Message = "connected to notification SSE (debug)" });
        await WriteSseEventAsync(response, "connection", jsonConnection, context.RequestAborted).ConfigureAwait(false);

        context.RequestAborted.WaitHandle.WaitOne();
        lock (_clientsGate)
        {
            _debugClients.Remove(response);
        }
    }

    public async Task SendEventToAllAsync(string message)
    {
        List<HttpResponse> clients;
        List<HttpResponse> debugClients;
        lock (_clientsGate)
        {
            clients = [.. _clients];
            debugClients = [.. _debugClients];
        }

        List<HttpResponse> disconnected = [];
        List<HttpResponse> disconnectedDebug = [];

        foreach (HttpResponse client in clients)
        {
            try
            {
                if (!client.HttpContext.RequestAborted.IsCancellationRequested)
                {
                    await WriteSseEventAsync(client, null, message, client.HttpContext.RequestAborted).ConfigureAwait(false);
                }
            }
            catch
            {
                disconnected.Add(client);
            }
        }

        foreach (HttpResponse client in debugClients)
        {
            try
            {
                if (!client.HttpContext.RequestAborted.IsCancellationRequested)
                {
                    await WriteSseEventAsync(client, null, message, client.HttpContext.RequestAborted).ConfigureAwait(false);
                }
            }
            catch
            {
                disconnectedDebug.Add(client);
            }
        }

        lock (_clientsGate)
        {
            foreach (HttpResponse client in disconnected)
            {
                _clients.Remove(client);
            }

            foreach (HttpResponse client in disconnectedDebug)
            {
                _debugClients.Remove(client);
            }
        }
    }

    private static void ConfigureSseResponse(HttpResponse response, string serviceWorkerAllowed)
    {
        response.StatusCode = StatusCodes.Status200OK;
        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";
        response.Headers["X-Accel-Buffering"] = "no";
        response.Headers["Service-Worker-Allowed"] = serviceWorkerAllowed;
    }

    private static async Task WriteSseEventAsync(
        HttpResponse response,
        string? eventName,
        string data,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            await response.WriteAsync($"event: {eventName}\n", cancellationToken).ConfigureAwait(false);
        }

        string normalized = data
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);

        using StringReader reader = new(normalized);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            await response.WriteAsync($"data: {line}\n", cancellationToken).ConfigureAwait(false);
        }

        await response.WriteAsync("\n", cancellationToken).ConfigureAwait(false);
        await response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
