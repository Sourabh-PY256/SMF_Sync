using EWP.SF.Helper;
using EWP.SF.Item.DataAccess;
using EWP.SF.Item.BusinessLayer;
using System.Reflection;
using Microsoft.OpenApi.Models;
using EWP.SF.Item.API.Extensions;

if (Environment.GetEnvironmentVariable("ENABLE_REMOTE_DEBUG") == "true")
{
    Console.WriteLine("Waiting for debugger to attach...");
    Console.WriteLine($"Process ID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
    while (!System.Diagnostics.Debugger.IsAttached)
    {
        System.Threading.Thread.Sleep(1000);
        Console.WriteLine("Still waiting for debugger...");
    }
    Console.WriteLine("Debugger attached!");
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "wwwroot"
});

// Set URLs explicitly
builder.WebHost.UseUrls("http://*:8080");

// Print current directory and files for debugging
Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine("Files in current directory:");
foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
{
    Console.WriteLine($"  {file}");
}

// Load AppSettings manually for use within the builder
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
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
        //uilder.Services.AddScoped<IItemService, ItemService>();

        // Register Kafka services
        builder.Services.AddSingleton<IKafkaService, KafkaService>();

        // Register service consumer manager as a singleton
        builder.Services.AddSingleton<IServiceConsumerManager, ServiceConsumerManager>();

builder.Services.AddControllers();

// Register Kafka service
builder.Services.AddSingleton<IKafkaService, KafkaService>();

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
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Item Sync Service API v1");
    options.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
});

// Enable static file serving
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseServiceConsumer();

app.MapControllers();
app.Run();
