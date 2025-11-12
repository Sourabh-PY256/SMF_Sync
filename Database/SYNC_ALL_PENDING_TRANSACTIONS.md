# Sync All Pending Transactions - Individual POST Requests

## ✅ Changes Made

You requested to **sync ALL pending transactions** by sending **separate POST requests for each transaction** and calling the Kafka producer for each individual request.

---

## 🔧 Code Changes

### **1. Updated `GetMaterialTransactionRequestParams` Method**

**File:** `KafkaSyncService\EWP.SF.KafkaSync.BusinessLayer\Services\WorkOrderOperation.cs`

**What Changed:**
- **Before:** Processed only the FIRST transaction (`.FirstOrDefault()`)
- **After:** Processes ALL pending transactions and returns them with a special flag

**New Behavior:**
```csharp
// Get all transactions where ExternalId is null or empty
List<OrderTransactionMaterial> transactions = await _orderTransactionMaterialRepo
    .GetOrderTransactionMaterialWithoutExternalId(cancel).ConfigureAwait(false);

// Build a list of ALL transactions
List<object> allTransactionParams = [];

foreach (var transaction in transactions)
{
    // Build request params for each transaction
    var transactionParams = new
    {
        TransactionId = transaction.TransactionId,
        OrderType = workOrder.OrderType,
        OrderCode = workOrder.OrderCode,
        Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
        Type = movType,
        OperationNo = transaction.OperationId,
        Components = components,
        Comments = transaction.Comments ?? string.Empty,
        Employee = transaction.EmployeeId ?? systemOperator.EmployeeId
    };

    allTransactionParams.Add(transactionParams);
}

// Return object with special flag to indicate individual processing
return new
{
    ProcessIndividually = true,
    Transactions = allTransactionParams
};
```

**Result:** Returns an object with `ProcessIndividually = true` flag and list of all transactions.

---

### **2. Added `ProcessMaterialTransactionsIndividually` Method**

**File:** `KafkaSyncService\EWP.SF.KafkaSync.BusinessLayer\Services\Processors\DataSyncServiceProcessor.cs`

**What Changed:**
- **New method** that processes each transaction separately
- Sends **individual POST request** to ERP for each transaction
- Calls **Kafka producer** for each successful transaction

**New Behavior:**
```csharp
private async Task<DataSyncHttpResponse> ProcessMaterialTransactionsIndividually(...)
{
    int successCount = 0;
    int failureCount = 0;

    // Get services
    var kafkaService = GetOperation<IKafkaService>();
    var orderTransactionMaterialRepo = GetOperation<IOrderTransactionMaterialRepo>();

    // Process each transaction
    foreach (var transaction in transactions)
    {
        string transactionId = transaction.TransactionId;

        // Serialize single transaction
        string singleRequestBody = JsonConvert.SerializeObject(transaction);

        // Map to ERP format
        dynamic requestErpMapped = DataSyncServiceUtil.MapEntity(..., singleRequestBody);
        string requestErpJson = JsonConvert.SerializeObject(requestErpMapped);

        // Send individual POST request to ERP
        DataSyncResponse erpResult = await ErpSendRequestAsync(..., requestErpJson, ...);

        if (erpResult.StatusCode == HttpStatusCode.OK)
        {
            // Extract ExternalId from response
            string externalId = responseErp["ExternalId"];

            // Update database
            await orderTransactionMaterialRepo.UpdateOrderTransactionMaterialExternalId(
                transactionId, externalId, SystemOperator
            );

            // Publish to Kafka for this transaction
            string topic = $"producer-sync-{SyncERPEntity.ORDER_TRANSACTION_SERVICE.ToLower()}";
            await kafkaService.ProduceMessageAsync(topic, key, kafkaMessage);

            successCount++;
        }
        else
        {
            failureCount++;
        }
    }

    return HttpResponse;
}
```

**Result:** Each transaction gets its own POST request to ERP and Kafka message.

---

### **3. Updated `SendSfDataToErp` Method**

**File:** `KafkaSyncService\EWP.SF.KafkaSync.BusinessLayer\Services\Processors\DataSyncServiceProcessor.cs`

**What Changed:**
- Detects the `ProcessIndividually` flag in the request params
- Routes to `ProcessMaterialTransactionsIndividually` method when flag is true

**New Behavior:**
```csharp
// Get request params
object requestParams = await workOrderOperation.GetMaterialTransactionRequestParams(SystemOperator);

// Check if we need to process transactions individually
dynamic requestParamsObj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(requestParams));

if (requestParamsObj.ProcessIndividually != null && (bool)requestParamsObj.ProcessIndividually)
{
    // Process each transaction individually
    return await ProcessMaterialTransactionsIndividually(
        LogInfo, ServiceData, SystemOperator, ExecOrigin,
        requestParamsObj.Transactions, EntityCode, HttpResponse, onResponse
    );
}
```

**Result:** Automatically routes to individual processing when multiple transactions are detected.

---

### **4. Updated Stored Procedure Ordering**

**Files:**
- `Database/StoredProcedures/SP_SF_OrderTransactionMaterial_SEL.sql`
- `Database/COMPLETE_SETUP.sql`
- `Database/INSTALL_PROCEDURES_ONLY.sql`

**What Changed:**
- **Before:** `ORDER BY otm.CreateDate DESC` (newest first)
- **After:** `ORDER BY otm.CreateDate ASC` (oldest first)

**Why:** Ensures transactions are processed in the order they were created (FIFO - First In, First Out).

---

## 🔄 New Sync Flow

### **Scenario: 3 Pending Transactions**

```
Database State:
┌────────────────┬──────────────┬────────────┐
│ TransactionId  │ ExternalId   │ CreateDate │
├────────────────┼──────────────┼────────────┤
│ TXN-001        │ NULL         │ 09:00 AM   │
│ TXN-002        │ NULL         │ 09:05 AM   │
│ TXN-003        │ NULL         │ 09:10 AM   │
└────────────────┴──────────────┴────────────┘
```

### **Step 1: User Creates Transaction #3**
```
09:10 AM - Transaction #3 created
         → Kafka Message published to "producer-sync-materialissue"
```

### **Step 2: Kafka Consumer Processes Message**
```
09:10 AM - GetMaterialTransactionRequestParams() called
         → Retrieves ALL 3 pending transactions (ordered by CreateDate ASC)
         → Returns: { ProcessIndividually: true, Transactions: [TXN-001, TXN-002, TXN-003] }
```

### **Step 3: ProcessMaterialTransactionsIndividually Executes**
```
09:10:01 - Process TXN-001
         → POST /erp/material-issue with TXN-001 data
         → ERP Response: { ExternalId: "ERP-123", Status: "Success" }
         → Update database: TXN-001.ExternalId = "ERP-123"
         → Publish Kafka message to "producer-sync-order_transaction_service"

09:10:02 - Process TXN-002
         → POST /erp/material-issue with TXN-002 data
         → ERP Response: { ExternalId: "ERP-124", Status: "Success" }
         → Update database: TXN-002.ExternalId = "ERP-124"
         → Publish Kafka message to "producer-sync-order_transaction_service"

09:10:03 - Process TXN-003
         → POST /erp/material-issue with TXN-003 data
         → ERP Response: { ExternalId: "ERP-125", Status: "Success" }
         → Update database: TXN-003.ExternalId = "ERP-125"
         → Publish Kafka message to "producer-sync-order_transaction_service"
```

### **Final Database State:**
```
┌────────────────┬──────────────┬────────────┐
│ TransactionId  │ ExternalId   │ CreateDate │
├────────────────┼──────────────┼────────────┤
│ TXN-001        │ ERP-123      │ 09:00 AM   │
│ TXN-002        │ ERP-124      │ 09:05 AM   │
│ TXN-003        │ ERP-125      │ 09:10 AM   │
└────────────────┴──────────────┴────────────┘
```

**All 3 transactions synced with individual POST requests!** ✅
**3 Kafka messages published (one for each transaction)!** ✅

---

## ⚠️ Important Notes

### **1. Individual POST Requests**
Each transaction gets its own POST request to ERP:
- **Transaction #1** → POST request #1 → Response #1 → Kafka message #1
- **Transaction #2** → POST request #2 → Response #2 → Kafka message #2
- **Transaction #3** → POST request #3 → Response #3 → Kafka message #3

**ERP Endpoint Format (per transaction):**
```
POST /api/material-issue
Content-Type: application/json

{
  "TransactionId": "TXN-001",
  "OrderCode": "WO-001",
  "Type": "Issue",
  "Components": [...]
}

Response:
{
  "ExternalId": "ERP-123",
  "Status": "Success"
}
```

### **2. Kafka Messages**
Two types of Kafka messages are published:

**A. Initial trigger (when transaction is created):**
- Topic: `producer-sync-materialissue`
- Triggers the sync process

**B. Success notification (after ERP sync):**
- Topic: `producer-sync-order_transaction_service`
- Published for EACH successfully synced transaction
- Contains: TransactionId, ExternalId, Timestamp, Status

### **3. Error Handling**
If one transaction fails:
- **Failed transaction:** Logged as error, ExternalId remains NULL
- **Other transactions:** Continue processing independently
- **Final response:** Shows success/failure count for all transactions

Example:
```
Processed 3 transactions. Success: 2, Failed: 1
Errors:
Transaction TXN-002: HTTP 400 - Invalid component code
```

### **4. Performance Characteristics**
- **HTTP Requests:** 3 transactions = 3 separate POST requests to ERP
- **Kafka Messages:** 3 transactions = 3 success notification messages
- **Database Updates:** 3 separate UPDATE queries (one per transaction)
- **Benefit:** Independent processing - one failure doesn't block others

---

## 🚀 Next Steps

### **1. Update Stored Procedures**
Run the updated SQL script:
```bash
mysql -u your_username -p ewp_sf_devsync < Database/INSTALL_PROCEDURES_ONLY.sql
```

### **2. Build and Test**
Build the application and test with multiple pending transactions:
```bash
# Create 2-3 material transactions in Smart Factory
# They will remain pending (ExternalId = NULL)

# Create one more transaction
# This triggers the Kafka message

# Check logs to see individual processing
```

### **3. Monitor Logs**
Check application logs for individual transaction processing:
```
Processing material transaction TXN-001
Successfully processed transaction TXN-001 with ExternalId ERP-123

Processing material transaction TXN-002
Successfully processed transaction TXN-002 with ExternalId ERP-124

Processing material transaction TXN-003
Successfully processed transaction TXN-003 with ExternalId ERP-125

Batch processing complete. Success: 3, Failed: 0
```

### **4. Verify Database**
Check that all transactions have ExternalIds:
```sql
SELECT Id, OrderCode, ExternalId, CreateDate
FROM sf_order_transactions_material
ORDER BY CreateDate DESC
LIMIT 10;
```

### **5. Check Kafka Messages**
Verify Kafka messages were published:
- Topic: `producer-sync-order_transaction_service`
- Should have one message per successfully synced transaction

---

## 📋 Summary

| Aspect | Before | After |
|--------|--------|-------|
| **Transactions per Kafka trigger** | 1 | ALL pending |
| **HTTP requests to ERP** | 1 (only newest) | N (one per pending transaction) |
| **Request format** | Single object | Single object (per request) |
| **Response format** | Single object | Single object (per request) |
| **Processing order** | Newest first (DESC) | Oldest first (ASC) |
| **Kafka messages published** | 1 (trigger) | N+1 (trigger + success per transaction) |
| **Error isolation** | N/A | One failure doesn't block others |

**Result:** All pending material transactions are now synced individually with separate POST requests and Kafka messages! 🎉

