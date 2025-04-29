using EWP.SF.Helper;
using EWP.SF.ShopFloor.DataAccess;
using EWP.SF.ShopFloor.BusinessLayer;
using System.Reflection;
using Microsoft.OpenApi.Models;

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

Console.WriteLine("Directories in current directory:");
foreach (var dir in Directory.GetDirectories(Directory.GetCurrentDirectory()))
{
    Console.WriteLine($"  {dir}");
}

if (Directory.Exists("Settings"))
{
    Console.WriteLine("Settings directory exists. Files in Settings directory:");
    foreach (var file in Directory.GetFiles("Settings"))
    {
        Console.WriteLine($"  {file}");
    }
}
else
{
    Console.WriteLine("Settings directory does not exist!");
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

// Register WarehouseRepository
builder.Services.AddScoped<IWorkCenterRepository, WorkCenterRepository>();
// Register WarehouseServices
builder.Services.AddScoped<IWorkCenterService, WorkCenterService>();
builder.Services.AddScoped<IUtilitiesRepository, UtilitiesRepository>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shop Floor Service API",
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
    app.UseSwagger(options =>
    {
        // This will add server URLs to the Swagger document
        options.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            // Check if we're being accessed through the gateway
            var forwardedPath = httpReq.Headers["X-Forwarded-Path"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedPath) && forwardedPath.Contains("/shopfloor"))
            {
                // We're being accessed through the gateway, add the /shopfloor prefix to the server URL
                swaggerDoc.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "/shopfloor" }
                };
                Console.WriteLine("Swagger accessed through gateway, setting server URL to /shopfloor");
            }
            else
            {
                // We're being accessed directly, use the default server URL
                swaggerDoc.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "/" }
                };
                Console.WriteLine("Swagger accessed directly, setting server URL to /");
            }
        });
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "Shop Floor Service API v1");
        options.RoutePrefix = "swagger"; // Set Swagger UI at /swagger

        // Add custom JavaScript to modify the Swagger UI behavior
        options.HeadContent = @"
            <script>
                window.onload = function() {
                    // Check if we're being accessed through the gateway
                    if (window.location.pathname.includes('/shopfloor/')) {
                        // We're being accessed through the gateway
                        console.log('Swagger UI accessed through gateway');

                        // Override the Swagger UI's URL building function
                        const originalBuildUrl = window.ui.buildUrl;
                        window.ui.buildUrl = function(url) {
                            // If the URL doesn't start with /shopfloor, add it
                            if (!url.startsWith('/shopfloor') && !url.startsWith('http')) {
                                url = '/shopfloor' + url;
                            }
                            return originalBuildUrl(url);
                        };
                    }
                };
            </script>
        ";
    });
}

// Enable static file serving
app.UseDefaultFiles();
app.UseStaticFiles();

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();
