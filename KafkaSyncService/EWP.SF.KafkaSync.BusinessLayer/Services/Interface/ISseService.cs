using Microsoft.AspNetCore.Http;

namespace EWP.SF.KafkaSync.BusinessLayer;

public interface ISseService
{
    Task SubscribeAsync(HttpContext context);
    Task SubscribeDebugAsync(HttpContext context);
    Task SendEventToAllAsync(string message);
}
