using EWP.SF.KafkaSync.BusinessLayer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EWP.SF.KafkaSync.API.Extensions
{
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Starts the service consumer manager
        /// </summary>
        public static IApplicationBuilder UseServiceConsumer(this IApplicationBuilder app)
        {
            // Get the service consumer manager from the service provider
            var serviceConsumerManager = app.ApplicationServices.GetRequiredService<IServiceConsumerManager>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<IServiceConsumerManager>>();
            
            // Start the consumer
            logger.LogInformation("Starting service consumer");
            serviceConsumerManager.StartConsumer();
            
            return app;
        }
    }
}