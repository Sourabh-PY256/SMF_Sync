using EWP.SF.Helper;
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessLayer;
using System.Reflection;
using Microsoft.OpenApi.Models;
using EWP.SF.Item.API.Extensions;

namespace EWP.SF.Item.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load AppSettings manually for use within the builder
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.json")), optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Kestrel.json", optional: true, reloadOnChange: false)
            .AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.Logging.json")), optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Logging.json", optional: true, reloadOnChange: false)
            .AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.NLog.json")), optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.NLog.json", optional: true, reloadOnChange: false)
            .AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.ConnectionStrings.json")), optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.ConnectionStrings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.ReverseProxy.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Register AppSettings for DI to use later in the app
        ApplicationSettings appSettings = new(configuration);
        builder.Services.AddSingleton<IApplicationSettings>(appSettings);

        // Register repositories
        builder.Services.AddScoped<IUtilitiesRepository, UtilitiesRepository>();
        builder.Services.AddScoped<IDataSyncRepository, DataSyncRepository>();

        // Register services
        builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
        builder.Services.AddScoped<IDataSyncServiceOperation, DataSyncServiceOperation>();
        builder.Services.AddScoped<DataSyncServiceProcessor>();
        builder.Services.AddScoped<DataSyncServiceManager>();
        //builder.Services.AddScoped<IItemService, ItemService>();

        // Register Kafka services
        builder.Services.AddSingleton<IKafkaService, KafkaService>();

        // Register service consumer manager as a singleton
        builder.Services.AddSingleton<IServiceConsumerManager, ServiceConsumerManager>();

        builder.Services.AddControllers();

        // Configure Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Item Sync Service API",
                Version = "v1",
                Description = "API for managing shop floor operations in the Smart Factory system",
                Contact = new OpenApiContact
                {
                    Name = "Smart Factory Team"
                }
            });

            // Add XML comments if they exist
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Item Sync Service API v1");
                options.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
            });
        }

        // Enable static file serving
        app.UseDefaultFiles();
        app.UseStaticFiles();

        //app.UseHttpsRedirection();

        // Start the service consumer
        app.UseServiceConsumer();

        app.MapControllers();

        app.Run();
    }
}
