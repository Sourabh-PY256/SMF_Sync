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
builder.Services.AddScoped<IDataSyncRepository, DataSyncRepository>();
builder.Services.AddScoped<IItemRepo, ItemRepo>();
builder.Services.AddScoped<IProcedureRepo, ProcedureRepo>();
builder.Services.AddScoped<IActivityRepo, ActivityRepo>();
builder.Services.AddScoped<IComponentRepo, ComponentRepo>();
builder.Services.AddScoped<IWorkOrderRepo, WorkOrderRepo>();
builder.Services.AddScoped<IOrderTransactionProductRepo, OrderTransactionProductRepo>();
builder.Services.AddScoped<IOrderTransactionMaterialRepo, OrderTransactionMaterialRepo>();
builder.Services.AddScoped<IWarehouseRepo, WarehouseRepo>();
builder.Services.AddScoped<IBinLocationRepo, BinLocationRepo>();
builder.Services.AddScoped<IDemandRepo, DemandRepo>();
builder.Services.AddScoped<IAttachmentRepo, AttachmentRepo>();
builder.Services.AddScoped<IDataImportRepo, DataImportRepo>();
builder.Services.AddScoped<ICatalogRepo, CatalogRepo>();
builder.Services.AddScoped<ISchedulingRepo, SchedulingRepo>();
builder.Services.AddScoped<ISkillRepo, SkillRepo>();
builder.Services.AddScoped<IOEERepo, OEERepo>();
builder.Services.AddScoped<ILaborRepo, LaborRepo>();
builder.Services.AddScoped<ISchedulingShiftStatusRepo, SchedulingShiftStatusRepo>();
builder.Services.AddScoped<ISchedulingCalendarShiftsRepo, SchedulingCalendarShiftsRepo>();
builder.Services.AddScoped<IEmployeeRepo, EmployeeRepo>();
builder.Services.AddScoped<IInventoryRepo, InventoryRepo>();
builder.Services.AddScoped<IInventoryStatusRepo, InventoryStatusRepo>();
builder.Services.AddScoped<ILotSerialStatusRepo, LotSerialStatusRepo>();
builder.Services.AddScoped<ISupplyRepo, SupplyRepo>();
builder.Services.AddScoped<IToolRepo, ToolRepo>();
builder.Services.AddScoped<IDeviceRepo, DeviceRepo>();
builder.Services.AddScoped<IProductionLinesRepo, ProductionLinesRepo>();

// Register services
builder.Services.AddScoped<IDataSyncServiceOperation, DataSyncServiceOperation>();
builder.Services.AddScoped<DataSyncServiceProcessor>();
builder.Services.AddScoped<DataSyncServiceManager>();
builder.Services.AddScoped<IInventoryOperation, InventoryOperation>();
builder.Services.AddScoped<IInventoryStatusOperation, InventoryStatusOperation>();
builder.Services.AddScoped<ILotSerialStatusOperation, LotSerialStatusOperation>();
builder.Services.AddScoped<IProcessTypeOperation, ProcessTypeOperation>();
builder.Services.AddScoped<IProcessTypeRepo, ProcessTypeRepo>();
builder.Services.AddScoped<IProcedureOperation, ProcedureOperation>();
builder.Services.AddScoped<IActivityOperation, ActivityOperation>();
builder.Services.AddScoped<IComponentOperation, ComponentOperation>();
builder.Services.AddScoped<IWorkOrderOperation, WorkOrderOperation>();
builder.Services.AddScoped<IOrderTransactionProductOperation, OrderTransactionProductOperation>();
builder.Services.AddScoped<IOrderTransactionMaterialOperation, OrderTransactionMaterialOperation>();
builder.Services.AddScoped<IWarehouseOperation, WarehouseOperation>();
builder.Services.AddScoped<IBinLocationOperation, BinLocationOperation>();
builder.Services.AddScoped<IDemandOperation, DemandOperation>();
builder.Services.AddScoped<IAttachmentOperation, AttachmentOperation>();
builder.Services.AddScoped<IDataImportOperation, DataImportOperation>();
builder.Services.AddScoped<IItemOperation, ItemOperation>();
builder.Services.AddScoped<ISchedulingShiftStatusOperation, SchedulingShiftStatusOperation>();
builder.Services.AddScoped<ISchedulingCalendarShiftsOperation, SchedulingCalendarShiftsOperation>();
builder.Services.AddScoped<IOEEOperation, OEEOperation>();
builder.Services.AddScoped<IEmployeeOperation, EmployeeOperation>();
builder.Services.AddScoped<ISupplyOperation, SupplyOperation>();
builder.Services.AddScoped<IToolOperation, ToolOperation>();
builder.Services.AddScoped<IDeviceOperation, DeviceOperation>();
builder.Services.AddScoped<IProductionLinesOperation, ProductionLinesOperation>();

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
