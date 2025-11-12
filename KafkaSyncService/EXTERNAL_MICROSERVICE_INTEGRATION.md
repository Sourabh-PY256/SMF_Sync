# External Microservice Integration Guide

## How to Call Kafka Sync Service from Another Microservice

When the `UpdateWorkOrderComponent` method is moved to another microservice, it needs to trigger the material transaction sync via HTTP call to the Kafka service.

---

## 🌐 HTTP Endpoint Details

**Endpoint:** `POST http://localhost:8080/DataSyncService/Producer`

**Request Body:**
```json
{
  "Services": ["MaterialIssue"],
  "EntityCode": "",
  "BodyData": "",
  "MethodType": "POST"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "errorCode": 0,
  "message": "SUCCESSFULLY",
  "data": [
    {
      "service": "MaterialIssue",
      "isSuccess": true,
      "response": "Service validated successfully"
    }
  ]
}
```

---

## 📝 Code Examples for Other Microservice

### **Option 1: Direct HTTP Call (Recommended)**

Replace this code in the other microservice:

```csharp
// OLD CODE (when it was in the same microservice):
var endpointResponse = await _dataSyncServiceManager.ExecuteServiceEndpoint(
    SyncERPEntity.MATERIAL_ISSUE_SERVICE,
    string.Empty,
    string.Empty,
    "POST",
    systemOperator
).ConfigureAwait(false);

CanProceed = endpointResponse.StatusCode == HttpStatusCode.OK;
```

**NEW CODE (for external microservice):**

```csharp
// Call Kafka Sync Service via HTTP
using var httpClient = new HttpClient();

var request = new
{
    Services = new[] { "MaterialIssue" },
    EntityCode = "",
    BodyData = "",
    MethodType = "POST"
};

var jsonContent = new StringContent(
    JsonConvert.SerializeObject(request),
    System.Text.Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync(
    "http://localhost:8080/DataSyncService/Producer",
    jsonContent
).ConfigureAwait(false);

CanProceed = response.IsSuccessStatusCode;

if (!CanProceed)
{
    var errorContent = await response.Content.ReadAsStringAsync();
    // ErrorList.Add(new KeyValuePair<string, string>("500", "ERP|" + errorContent));
}
```

---

### **Option 2: Create a Helper Service Class**

Create a helper class in the other microservice:

```csharp
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace YourMicroservice.Services;

public class KafkaSyncServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _kafkaSyncServiceUrl;

    public KafkaSyncServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _kafkaSyncServiceUrl = configuration["KafkaSyncServiceUrl"] ?? "http://localhost:8080";
    }

    public async Task<bool> TriggerMaterialTransactionSync()
    {
        try
        {
            var request = new
            {
                Services = new[] { "MaterialIssue" },
                EntityCode = "",
                BodyData = "",
                MethodType = "POST"
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"{_kafkaSyncServiceUrl}/DataSyncService/Producer",
                jsonContent
            ).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Log error
            return false;
        }
    }
}
```

**Register in Program.cs or Startup.cs:**

```csharp
builder.Services.AddHttpClient<KafkaSyncServiceClient>();
builder.Services.AddScoped<KafkaSyncServiceClient>();
```

**Use in UpdateWorkOrderComponent:**

```csharp
public async Task<string> UpdateWorkOrderComponent(
    string workOrderId, 
    List<OrderComponent> componentValues, 
    string employeeId, 
    User systemOperator)
{
    // ... existing code to save transaction to database ...

    // Trigger Kafka sync via HTTP
    var kafkaSyncClient = _serviceProvider.GetRequiredService<KafkaSyncServiceClient>();
    CanProceed = await kafkaSyncClient.TriggerMaterialTransactionSync();

    if (!CanProceed)
    {
        // Handle error
    }

    return returnValue;
}
```

---

### **Option 3: Using IHttpClientFactory (Best Practice)**

**Create a typed client:**

```csharp
public interface IKafkaSyncServiceClient
{
    Task<KafkaSyncResponse> TriggerMaterialTransactionSyncAsync();
}

public class KafkaSyncServiceClient : IKafkaSyncServiceClient
{
    private readonly HttpClient _httpClient;

    public KafkaSyncServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<KafkaSyncResponse> TriggerMaterialTransactionSyncAsync()
    {
        var request = new DataSyncExecuteRequest
        {
            Services = new List<string> { "MaterialIssue" },
            EntityCode = "",
            BodyData = "",
            MethodType = "POST"
        };

        var jsonContent = new StringContent(
            JsonConvert.SerializeObject(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            "/DataSyncService/Producer",
            jsonContent
        );

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResponseModel>(content);
            
            return new KafkaSyncResponse
            {
                IsSuccess = true,
                Message = result?.Message ?? "Success"
            };
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return new KafkaSyncResponse
            {
                IsSuccess = false,
                Message = errorContent
            };
        }
    }
}

public class KafkaSyncResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}

public class DataSyncExecuteRequest
{
    public List<string> Services { get; set; }
    public string EntityCode { get; set; }
    public string BodyData { get; set; }
    public string MethodType { get; set; }
}

public class ResponseModel
{
    public bool IsSuccess { get; set; }
    public int ErrorCode { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
}
```

**Register in Program.cs:**

```csharp
builder.Services.AddHttpClient<IKafkaSyncServiceClient, KafkaSyncServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["KafkaSyncServiceUrl"] ?? "http://localhost:8080");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

**Add to appsettings.json:**

```json
{
  "KafkaSyncServiceUrl": "http://localhost:8080"
}
```

**Use in UpdateWorkOrderComponent:**

```csharp
private readonly IKafkaSyncServiceClient _kafkaSyncServiceClient;

public YourService(IKafkaSyncServiceClient kafkaSyncServiceClient)
{
    _kafkaSyncServiceClient = kafkaSyncServiceClient;
}

public async Task<string> UpdateWorkOrderComponent(
    string workOrderId, 
    List<OrderComponent> componentValues, 
    string employeeId, 
    User systemOperator)
{
    // ... existing code to save transaction to database ...

    // Trigger Kafka sync via HTTP
    var syncResponse = await _kafkaSyncServiceClient.TriggerMaterialTransactionSyncAsync();
    CanProceed = syncResponse.IsSuccess;

    if (!CanProceed)
    {
        // ErrorList.Add(new KeyValuePair<string, string>("500", "ERP|" + syncResponse.Message));
    }

    return returnValue;
}
```

---

## 🔄 Complete Flow

```
1. External Microservice
   ↓ User calls UpdateWorkOrderComponent
2. Save transaction to database (ExternalId = NULL)
   ↓ HTTP POST to http://localhost:8080/DataSyncService/Producer
3. Kafka Sync Service - DataSyncController.SyncProducer
   ↓ Validates MATERIAL_ISSUE_SERVICE
   ↓ Publishes to Kafka topic: producer-sync-materialissue
4. Kafka Consumer (ServiceConsumerManager)
   ↓ Receives message
   ↓ Calls ProcessMaterialTransactionsIndividually
5. DataSyncServiceProcessor.ProcessMaterialTransactionsIndividually
   ↓ Gets ALL pending transactions (ExternalId IS NULL)
   ↓ For EACH transaction:
6. POST to ERP
   ↓ Receives response with docNum
   ↓ Publishes to Kafka topic: producer-sync-ordertransaction
7. Kafka Consumer (ServiceConsumerManager)
   ↓ Receives message
   ↓ Calls ProcessOrderTransactionService
8. DataSyncServiceProcessor.ProcessOrderTransactionService
   ↓ Updates database: ExternalId = "306781"
9. ✅ Transaction synced and ExternalId updated!
```

---

## ✅ Recommendation

**Use Option 3 (IHttpClientFactory with Typed Client)** because:
- ✅ Follows .NET best practices
- ✅ Proper HttpClient lifecycle management
- ✅ Easy to mock for unit testing
- ✅ Centralized configuration
- ✅ Type-safe responses
- ✅ Reusable across the microservice

---

## 📝 Summary

When moving `UpdateWorkOrderComponent` to another microservice:

1. **Remove dependency** on `DataSyncServiceManager` (it's internal to Kafka service)
2. **Add HTTP call** to `http://localhost:8080/DataSyncService/Producer`
3. **Use IHttpClientFactory** for proper HttpClient management
4. **Configure URL** in appsettings.json for flexibility
5. **Handle response** to check if sync was triggered successfully

The Kafka service will handle the rest automatically! 🚀

