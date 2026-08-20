# Material Transaction Sync Flow - Complete Explanation

## ❓ Your Question:
**"In that case kafka producer is calling only one time, how it will again for another transaction?"**

## ✅ Answer:
The Kafka producer **IS called multiple times** - once for **EACH** material transaction that needs to be synced. Here's how:

---

## 🔄 Complete Sync Flow

### **Step 1: User Creates Material Transaction in SF**
When a user creates a material issue/return/scrap in the Smart Factory system:

```csharp
// WorkOrderOperation.cs - Line 2306
var endpointResponse = await _dataSyncServiceManager.ExecuteServiceEndpoint(
    SyncERPEntity.MATERIAL_ISSUE_SERVICE,  // Service name
    string.Empty,
    string.Empty,
    "POST",
    systemOperator
).ConfigureAwait(false);
```

This happens **EVERY TIME** a user creates a new material transaction.

---

### **Step 2: ExecuteServiceEndpoint Publishes to Kafka**
The `ExecuteServiceEndpoint` method makes an HTTP POST to the Producer endpoint:

```csharp
// DataSyncServiceManager.cs - Line 114
var response = await httpClient.PostAsync(
    "http://localhost:8080/DataSyncService/Producer",
    jsonContent
).ConfigureAwait(false);
```

---

### **Step 3: Producer Endpoint Publishes Message to Kafka**
The `/DataSyncService/Producer` endpoint receives the request and publishes to Kafka:

```csharp
// DataSyncController.cs - Line 54-67
string topic = $"producer-sync-{service.ToLower()}";  // "producer-sync-materialissue"
await kafkaService.ProduceMessageAsync(
    topic,
    $"producer-{service}-{Guid.NewGuid()}",
    new SyncMessage
    {
        Service = service,                              // "MaterialIssue"
        Trigger = TriggerType.Erp.ToString(),
        ExecutionType = (int)ServiceExecOrigin.KafkaProducer,
        EntityCode = ServiceRequest.EntityCode,
        BodyData = ServiceRequest.BodyData,
        ServiceData = validation.ServiceData
    }
).ConfigureAwait(false);
```

**This happens EVERY TIME a material transaction is created!**

---

### **Step 4: Kafka Consumer Receives Message**
The Kafka consumer is always running in the background, listening to the topic:

```csharp
// ServiceConsumerManager.cs - Line 42-84
_kafkaService.StartConsumer(topic, async (key, value) =>
{
    var message = JsonSerializer.Deserialize<SyncMessage>(value);
    
    var processor = scope.ServiceProvider.GetRequiredService<DataSyncServiceProcessor>();
    var response = await processor.SyncExecution(
        message.ServiceData,
        ServiceExecOrigin.KafkaProducer,
        TriggerType.Erp,
        message.User,
        message.EntityCode ?? string.Empty,
        message.BodyData ?? string.Empty
    ).ConfigureAwait(false);
});
```

**The consumer processes EACH message as it arrives!**

---

### **Step 5: SyncExecution Calls GetMaterialTransactionRequestParams**
For each Kafka message, the processor calls your method:

```csharp
// DataSyncServiceProcessor.cs - Line 2061-2066
case SyncERPEntity.MATERIAL_ISSUE_SERVICE:
    var workOrderOperation = GetOperation<IWorkOrderOperation>();
    object requestParams = await workOrderOperation.GetMaterialTransactionRequestParams(SystemOperator).ConfigureAwait(false);
    RequestBody = JsonConvert.SerializeObject(requestParams);
    break;
```

---

### **Step 6: GetMaterialTransactionRequestParams Processes ONE Transaction**
Your method retrieves ALL pending transactions but processes only the FIRST one:

```csharp
// WorkOrderOperation.cs - Line 2653-2677
List<OrderTransactionMaterial> transactions = await _orderTransactionMaterialRepo
    .GetOrderTransactionMaterialWithoutExternalId(cancel).ConfigureAwait(false);

// Process only the FIRST transaction
OrderTransactionMaterial? transaction = transactions.FirstOrDefault();
```

---

### **Step 7: After Successful ERP Sync, ExternalId is Updated**
After the ERP responds successfully, the transaction's ExternalId is updated:

```csharp
// DataSyncServiceProcessor.cs - Line 2597-2628
private async Task HandleMaterialIssueSuccess(...)
{
    // Update ExternalId in database
    bool updateSuccess = await _orderTransactionMaterialRepo.UpdateOrderTransactionMaterialExternalId(
        transactionId,
        externalId,
        externalDate
    ).ConfigureAwait(false);
    
    // Publish success to Kafka
    await kafkaService.ProduceMessageAsync(topic, key, kafkaMessage).ConfigureAwait(false);
}
```

---

## 🔁 How Multiple Transactions Are Processed

### **Scenario: 3 Material Transactions Created**

| Time | Event | Database State | Kafka Activity |
|------|-------|----------------|----------------|
| **10:00 AM** | User creates Transaction #1 | `ExternalId = NULL` | Kafka message published |
| **10:00 AM** | Kafka consumer receives message | Processes Transaction #1 | Sends to ERP |
| **10:01 AM** | ERP responds successfully | `ExternalId = "ERP123"` | Success published to Kafka |
| | | | |
| **10:05 AM** | User creates Transaction #2 | `ExternalId = NULL` | **NEW Kafka message published** |
| **10:05 AM** | Kafka consumer receives message | Processes Transaction #2 | Sends to ERP |
| **10:06 AM** | ERP responds successfully | `ExternalId = "ERP124"` | Success published to Kafka |
| | | | |
| **10:10 AM** | User creates Transaction #3 | `ExternalId = NULL` | **NEW Kafka message published** |
| **10:10 AM** | Kafka consumer receives message | Processes Transaction #3 | Sends to ERP |
| **10:11 AM** | ERP responds successfully | `ExternalId = "ERP125"` | Success published to Kafka |

---

## 🎯 Key Points

### ✅ **Kafka Producer is Called Multiple Times**
- **EVERY** time a user creates a material transaction
- **EVERY** transaction triggers a new Kafka message
- **EVERY** Kafka message is processed independently

### ✅ **Why GetMaterialTransactionRequestParams Only Processes ONE**
Even though it retrieves ALL pending transactions, it only processes the FIRST one because:
1. **Safety**: If ERP sync fails, only ONE transaction is affected
2. **Tracking**: Each transaction has its own success/failure status
3. **Retry Logic**: Failed transactions can be retried individually
4. **Audit Trail**: Clear logging for each transaction

### ✅ **What Happens if Multiple Transactions Exist Without ExternalId?**
This can happen if:
- **Multiple users** create transactions simultaneously
- **ERP is down** and transactions queue up
- **Network issues** prevent sync

In this case:
1. Each transaction creation publishes a Kafka message
2. Kafka consumer processes messages **in order**
3. Each message processes ONE transaction
4. After success, ExternalId is updated
5. Next message processes the next pending transaction

---

## 📊 Example: 5 Transactions Created While ERP is Down

```
Database State at 10:00 AM (ERP is down):
┌────────────────┬──────────────┬────────────┐
│ TransactionId  │ ExternalId   │ CreateDate │
├────────────────┼──────────────┼────────────┤
│ TXN-001        │ NULL         │ 09:50 AM   │
│ TXN-002        │ NULL         │ 09:52 AM   │
│ TXN-003        │ NULL         │ 09:55 AM   │
│ TXN-004        │ NULL         │ 09:58 AM   │
│ TXN-005        │ NULL         │ 10:00 AM   │
└────────────────┴──────────────┴────────────┘

Kafka Queue:
[Message-1] → [Message-2] → [Message-3] → [Message-4] → [Message-5]
```

**When ERP comes back online at 10:05 AM:**

```
10:05:00 - Consumer processes Message-1
         - GetMaterialTransactionRequestParams retrieves ALL 5 transactions
         - Processes ONLY TXN-001 (FirstOrDefault)
         - Sends to ERP → Success
         - Updates TXN-001.ExternalId = "ERP123"

10:05:05 - Consumer processes Message-2
         - GetMaterialTransactionRequestParams retrieves 4 remaining transactions
         - Processes ONLY TXN-002 (FirstOrDefault)
         - Sends to ERP → Success
         - Updates TXN-002.ExternalId = "ERP124"

10:05:10 - Consumer processes Message-3
         - GetMaterialTransactionRequestParams retrieves 3 remaining transactions
         - Processes ONLY TXN-003 (FirstOrDefault)
         - Sends to ERP → Success
         - Updates TXN-003.ExternalId = "ERP125"

... and so on until all 5 are processed
```

---

## 🚀 Summary

**Your concern:** "Kafka producer is calling only one time"

**Reality:** Kafka producer is called **EVERY TIME** a material transaction is created!

- ✅ Each transaction creation → 1 Kafka message
- ✅ Each Kafka message → 1 sync execution
- ✅ Each sync execution → Processes 1 transaction
- ✅ After success → ExternalId updated
- ✅ Next message → Processes next pending transaction

**The system is designed to handle multiple transactions automatically through the Kafka message queue!** 🎉

