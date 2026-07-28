using EWP.SF.Helper;
using EWP.SF.KafkaSync.DataAccess;
using EWP.SF.KafkaSync.BusinessLayer;
using System.Reflection;
using Microsoft.OpenApi.Models;
using EWP.SF.KafkaSync.API.Extensions;
using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.KafkaSync.BusinessLayer.Services.Proxies;
using EWP.SF.KafkaSync.BusinessLayer.Services;
using EWP.SF.KafkaSync.BusinessLayer.Services.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EWP.SF.KafkaSync.BusinessLayer.Services.Handlers;
var builder = WebApplication.CreateBuilder(args);

// Load AppSettings into the builder's configuration so it's available for DI
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.json")), optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.Kestrel.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.Logging.json")), optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.Logging.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.NLog.json")), optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.NLog.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile(Path.GetFullPath(Path.Combine("..", "Settings", "appsettings.ConnectionStrings.json")), optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.ConnectionStrings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.ReverseProxy.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile("appsettings.Docker.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

IConfiguration configuration = builder.Configuration;

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
builder.Services.AddScoped<IStockAllocationRepo, StockAllocationRepo>();

// Register services
builder.Services.AddScoped<IDataSyncServiceOperation, DataSyncServiceOperation>();
builder.Services.AddScoped<DataSyncServiceProcessor>();
builder.Services.AddScoped<DataSyncServiceManager>();
builder.Services.AddSingleton<ISseService, SseService>();
builder.Services.AddSingleton<IDataSyncNotifier, DataSyncNotifier>();
builder.Services.AddSingleton<ISyncCompletionRegistry, SyncCompletionRegistry>(); 
builder.Services.AddScoped<KafkaSyncRequestFactory>();
// builder.Services.AddScoped<IInventoryOperation, InventoryOperation>(); // Moved to proxy selection section below
// builder.Services.AddScoped<IEmployeeOperation, EmployeeOperation>(); // Moved to proxy selection section below
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

        // Register Authentication Service first
        builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });

        // Always register HTTP Proxies as themselves for Consumer use
        builder.Services.AddHttpClient<BinLocationOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<WarehouseOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<InventoryOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<EmployeeOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<ProductOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<ItemOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<SupplyOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<DemandOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<MeasureUnitOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<PositionOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<DeviceOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<ToolingTypeOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });

        builder.Services.AddHttpClient<WorkOrderChangeStatusOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<StockOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<OrderTransactionMaterialOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<MaterialReturnOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<ProductionOrderOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });
        builder.Services.AddHttpClient<ProductReceiptOperationProxy>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
            });

        if (configuration.GetValue<bool>("AppSettings:UseKafkaForSync"))
        {
            builder.Services.AddScoped<IBinLocationOperation, BinLocationKafkaProxy>();
            builder.Services.AddScoped<IWarehouseOperation, WarehouseKafkaProxy>();
            builder.Services.AddScoped<IInventoryOperation, InventoryKafkaProxy>();
            builder.Services.AddScoped<IEmployeeOperation, EmployeeKafkaProxy>();
            builder.Services.AddScoped<IComponentOperation, ProductKafkaProxy>();
            builder.Services.AddScoped<IItemOperation, ItemKafkaProxy>();
            builder.Services.AddScoped<ISupplyOperation, SupplyKafkaProxy>();
            builder.Services.AddScoped<IDemandOperation, DemandKafkaProxy>();
            builder.Services.AddScoped<IMeasureUnitOperation, MeasureUnitKafkaProxy>();
            builder.Services.AddScoped<IProfileOperation, PositionKafkaProxy>();
            builder.Services.AddScoped<IWorkOrderChangeStatusOperation, WorkOrderChangeStatusKafkaProxy>();
            builder.Services.AddScoped<IStockOperation, StockKafkaProxy>();
            builder.Services.AddScoped<IOrderTransactionMaterialOperation, OrderTransactionMaterialKafkaProxy>();
            builder.Services.AddScoped<MaterialReturnKafkaProxy>();
            builder.Services.AddScoped<IToolOperation, ToolingTypeKafkaProxy>();
            builder.Services.AddScoped<IDeviceOperation, DeviceKafkaProxy>();
            builder.Services.AddScoped<IWorkOrderOperation, ProductionOrderKafkaProxy>();
            builder.Services.AddScoped<IOrderTransactionProductOperation, ProductReceiptKafkaProxy>();

            
        }
        else if (configuration.GetValue<bool>("AppSettings:UseExternalSyncApi"))
        {
            builder.Services.AddScoped<IBinLocationOperation>(sp => sp.GetRequiredService<BinLocationOperationProxy>());
            builder.Services.AddScoped<IWarehouseOperation>(sp => sp.GetRequiredService<WarehouseOperationProxy>());
            builder.Services.AddScoped<IInventoryOperation>(sp => sp.GetRequiredService<InventoryOperationProxy>());
            builder.Services.AddScoped<IEmployeeOperation>(sp => sp.GetRequiredService<EmployeeOperationProxy>());
            builder.Services.AddScoped<IComponentOperation>(sp => sp.GetRequiredService<ProductOperationProxy>());
            builder.Services.AddScoped<IItemOperation>(sp => sp.GetRequiredService<ItemOperationProxy>());
            builder.Services.AddScoped<ISupplyOperation>(sp => sp.GetRequiredService<SupplyOperationProxy>());
            builder.Services.AddScoped<IDemandOperation>(sp => sp.GetRequiredService<DemandOperationProxy>());
            builder.Services.AddScoped<IMeasureUnitOperation>(sp => sp.GetRequiredService<MeasureUnitOperationProxy>());
            builder.Services.AddScoped<IProfileOperation>(sp => sp.GetRequiredService<PositionOperationProxy>());
            builder.Services.AddScoped<IWorkOrderChangeStatusOperation>(sp => sp.GetRequiredService<WorkOrderChangeStatusOperationProxy>());
            builder.Services.AddScoped<IStockOperation>(sp => sp.GetRequiredService<StockOperationProxy>());
            builder.Services.AddScoped<IOrderTransactionMaterialOperation>(sp => sp.GetRequiredService<OrderTransactionMaterialOperationProxy>());
            builder.Services.AddScoped<MaterialReturnOperationProxy>(sp => sp.GetRequiredService<MaterialReturnOperationProxy>());
            builder.Services.AddScoped<IToolOperation>(sp => sp.GetRequiredService<ToolingTypeOperationProxy>());
            builder.Services.AddScoped<IDeviceOperation>(sp => sp.GetRequiredService<DeviceOperationProxy>());
            builder.Services.AddScoped<IWorkOrderOperation>(sp => sp.GetRequiredService<ProductionOrderOperationProxy>());
            builder.Services.AddScoped<IOrderTransactionProductOperation>(sp => sp.GetRequiredService<ProductReceiptOperationProxy>());

        }
        else
        {
            builder.Services.AddScoped<IBinLocationOperation, BinLocationOperation>();
            builder.Services.AddScoped<IWarehouseOperation, WarehouseOperation>();
            builder.Services.AddScoped<IInventoryOperation, InventoryOperation>();
            builder.Services.AddScoped<IEmployeeOperation, EmployeeOperation>();
            builder.Services.AddScoped<IComponentOperation, ComponentOperation>();
            builder.Services.AddScoped<IWorkOrderChangeStatusOperation, WorkOrderOperation>();
            builder.Services.AddScoped<IStockOperation, StockOperation>();
            builder.Services.AddScoped<IOrderTransactionMaterialOperation, OrderTransactionMaterialOperation>();
            builder.Services.AddScoped<IToolOperation, ToolOperation>();
            builder.Services.AddScoped<IDeviceOperation, DeviceOperation>();
        }
        // builder.Services.AddScoped<IDemandOperation, DemandOperation>(); // Moved to proxy selection section below
// builder.Services.AddScoped<IOrderTransactionMaterialOperation, OrderTransactionMaterialOperation>(); // Moved to proxy selection section above
builder.Services.AddScoped<IAttachmentOperation, AttachmentOperation>();
builder.Services.AddScoped<IDataImportOperation, DataImportOperation>();
// builder.Services.AddScoped<IItemOperation, ItemOperation>(); // Moved to proxy selection section above
builder.Services.AddScoped<ISchedulingShiftStatusOperation, SchedulingShiftStatusOperation>();
builder.Services.AddScoped<ISchedulingCalendarShiftsOperation, SchedulingCalendarShiftsOperation>();
builder.Services.AddScoped<IOEEOperation, OEEOperation>();
// builder.Services.AddScoped<ISupplyOperation, SupplyOperation>(); // Moved to proxy selection section above
// builder.Services.AddScoped<IToolOperation, ToolOperation>(); // Moved to proxy selection section above
// builder.Services.AddScoped<IDeviceOperation, DeviceOperation>(); // Moved to proxy selection section above
builder.Services.AddScoped<IProductionLinesOperation, ProductionLinesOperation>();
// builder.Services.AddScoped<IMeasureUnitOperation, MeasureUnitOperation>(); // Moved to proxy selection section above
// builder.Services.AddScoped<IProfileOperation, ProfileOperation>(); // Moved to proxy selection section above
// builder.Services.AddScoped<IStockOperation, StockOperation>(); // Moved to proxy selection section above
builder.Services.AddScoped<IStockAllocationOperation, StockAllocationOperation>();






// Register Kafka services
builder.Services.AddSingleton<IKafkaService, KafkaService>();

// Register service consumer manager as a singleton
builder.Services.AddSingleton<ISyncMessageHandler, OrderTransactionMessageHandler>();
builder.Services.AddSingleton<ISyncMessageHandler, MaterialSyncMessageHandler>();
builder.Services.AddSingleton<ISyncMessageHandler, ProductReceiptMessageHandler>();
builder.Services.AddSingleton<ISyncMessageHandler, MachineLaborIssueMessageHandler>();
builder.Services.AddSingleton<ISyncMessageHandler, ProductionOrderMessageHandler>();
builder.Services.AddSingleton<ISyncMessageHandler, DefaultSyncMessageHandler>();
builder.Services.AddSingleton<IServiceConsumerManager, ServiceConsumerManager>();

//builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = 
            new DefaultContractResolver(); // PascalCase
        options.SerializerSettings.NullValueHandling = 
            NullValueHandling.Ignore; // Remove null properties
    });
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
