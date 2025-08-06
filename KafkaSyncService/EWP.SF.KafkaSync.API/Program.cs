using EWP.SF.Helper;
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.KafkaSync.BusinessLayer;
using System.Reflection;
using Microsoft.OpenApi.Models;
using EWP.SF.KafkaSync.API.Extensions;
using EWP.SF.KafkaSync.BusinessEntities;

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
builder.Services.AddScoped<IProductionLinesRepo, ProductionLinesRepo>();
builder.Services.AddScoped<IMeasureUnitRepo, MeasureUnitRepo>();
builder.Services.AddScoped<IMachineRepo, MachineRepo>();
builder.Services.AddScoped<IStockRepo, StockRepo>();

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
builder.Services.AddScoped<IMeasureUnitOperation, MeasureUnitOperation>();
builder.Services.AddScoped<IProfileOperation, ProfileOperation>();
builder.Services.AddScoped<IStockOperation, StockOperation>();






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
        Title = "Kafka Sync Service API",
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Kafka Sync Service API v1");
        options.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

// Enable static file serving
app.UseDefaultFiles();
app.UseStaticFiles();

//app.UseHttpsRedirection();
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<KafkaTopicValidator>>();

    var entityList = typeof(SyncERPEntity)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
        .Select(fi => fi.GetRawConstantValue()?.ToString())
        .Where(value => !string.IsNullOrEmpty(value))
        .ToList();

    var topicNames = entityList
        .Select(e => $"producer-sync-{e.ToLower()}")
        .ToList();

    var kafkaBootstrapServers = configuration["KafkaSettings:BootstrapServers"];

    var topicValidator = new KafkaTopicValidator(logger, kafkaBootstrapServers);
    await topicValidator.EnsureTopicsExistAsync(topicNames);

    logger.LogInformation("All required Kafka topics validated and created successfully.");
}

// Start the service consumer
app.UseServiceConsumer();

app.MapControllers();

app.Run();
