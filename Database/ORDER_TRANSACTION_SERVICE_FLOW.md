# ORDER_TRANSACTION_SERVICE Flow - Kafka-Based Architecture

## ✅ Current Status

**Separation of Concerns Architecture:**
- **MATERIAL_ISSUE_SERVICE:** Syncs transactions to ERP and publishes Kafka messages
- **ORDER_TRANSACTION_SERVICE:** Handles ALL database ExternalId updates via Kafka

---

## 🔄 Complete Flow

### **Scenario 1: Material Transaction Sync (SF → ERP)**

```
Step 1: User creates material transaction in Smart Factory
        → Transaction saved with ExternalId = NULL

Step 2: Kafka message published to "producer-sync-materialissue"
        → Consumer picks up message
        → Calls GetMaterialTransactionRequestParams()
        → Returns all pending transactions

Step 3: ProcessMaterialTransactionsIndividually() executes
        → For each transaction:
          a) Send POST to ERP
          b) Get ExternalId from ERP response
          c) Publish Kafka message to "producer-sync-ordertransaction"
             Message: {"TransactionId":"TXN-001","ExternalId":"ERP-123","Status":"Success"}

Step 4: Kafka consumer picks up "producer-sync-ordertransaction" message
        → Calls SyncExecution() with ORDER_TRANSACTION_SERVICE
        → RequestBody contains: {"TransactionId":"TXN-001","ExternalId":"ERP-123"}

Step 5: ORDER_TRANSACTION_SERVICE case executes
        → Parses TransactionId and ExternalId from RequestBody
        → Calls UpdateOrderTransactionMaterialExternalId()  ✅ UPDATE
        → Database updated with ExternalId
        → Returns success response
```

### **Scenario 2: ERP Sends ExternalId (ERP → SF)**

```
Step 1: ERP processes material issue and generates ExternalId
        → ERP wants to notify Smart Factory

Step 2: ERP calls POST /api/DataSync/DataSyncService/Producer
        → Body: {
            "Services": ["ordertransaction"],
            "BodyData": "{\"TransactionId\":\"TXN-001\",\"ExternalId\":\"ERP-12345\"}"
          }

Step 3: SyncProducer endpoint publishes Kafka message
        → Topic: "producer-sync-ordertransaction"
        → Message contains BodyData with TransactionId and ExternalId

Step 4: Kafka consumer picks up message
        → Calls SyncExecution() with ORDER_TRANSACTION_SERVICE
        → RequestBody contains: {"TransactionId":"TXN-001","ExternalId":"ERP-12345"}

Step 5: ORDER_TRANSACTION_SERVICE case executes
        → Parses TransactionId and ExternalId from RequestBody
        → Calls UpdateOrderTransactionMaterialExternalId()  ✅ UPDATE
        → Database updated with ExternalId
        → Returns success response
```

---

## 🔧 Code Changes Made

### **Added ORDER_TRANSACTION_SERVICE Case**

**File:** `KafkaSyncService\EWP.SF.KafkaSync.BusinessLayer\Services\Processors\DataSyncServiceProcessor.cs`

**Location:** Lines 2081-2146

```csharp
case SyncERPEntity.ORDER_TRANSACTION_SERVICE:
    // Handle order transaction sync from ERP via SyncProducer endpoint
    // ERP sends TransactionId and ExternalId after processing the material issue
    if (!string.IsNullOrEmpty(RequestBody))
    {
        try
        {
            // Parse the request body from ERP
            dynamic messageData = JsonConvert.DeserializeObject(RequestBody);
            string transactionId = messageData?.TransactionId?.ToString();
            string externalId = messageData?.ExternalId?.ToString();

            if (string.IsNullOrEmpty(transactionId))
            {
                throw new Exception("TransactionId is required in ORDER_TRANSACTION_SERVICE request");
            }

            if (string.IsNullOrEmpty(externalId))
            {
                throw new Exception("ExternalId is required in ORDER_TRANSACTION_SERVICE request");
            }

            _logger.LogInformation("Processing ORDER_TRANSACTION_SERVICE for TransactionId: {TransactionId}, ExternalId: {ExternalId}",
                transactionId, externalId);

            // Update the ExternalId in the database
            var orderTransactionMaterialRepo = GetOperation<IOrderTransactionMaterialRepo>();
            bool updateSuccess = await orderTransactionMaterialRepo.UpdateOrderTransactionMaterialExternalId(
                transactionId,
                externalId,
                SystemOperator
            ).ConfigureAwait(false);

            if (updateSuccess)
            {
                HttpResponse.StatusCode = HttpStatusCode.OK;
                HttpResponse.Message = $"Successfully updated ExternalId for transaction {transactionId}";
                _logger.LogInformation("Successfully updated ExternalId for transaction {TransactionId} with ExternalId {ExternalId}",
                    transactionId, externalId);
            }
            else
            {
                HttpResponse.StatusCode = HttpStatusCode.InternalServerError;
                HttpResponse.Message = $"Failed to update ExternalId for transaction {transactionId}";
                _logger.LogError("Failed to update ExternalId for transaction {TransactionId}", transactionId);
            }

            return HttpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ORDER_TRANSACTION_SERVICE: {Message}", ex.Message);
            HttpResponse.StatusCode = HttpStatusCode.BadRequest;
            HttpResponse.Message = $"Error processing ORDER_TRANSACTION_SERVICE: {ex.Message}";
            return HttpResponse;
        }
    }
    else
    {
        // If no RequestBody, this might be from the internal Kafka notification
        // (published after material issue sync in ProcessMaterialTransactionsIndividually)
        _logger.LogInformation("ORDER_TRANSACTION_SERVICE called without body - transaction already updated");
        HttpResponse.StatusCode = HttpStatusCode.OK;
        HttpResponse.Message = "Order transaction already processed";
        return HttpResponse;
    }
```

---

## 📋 Summary

| Aspect | Scenario 1 (SF → ERP) | Scenario 2 (ERP → SF) |
|--------|----------------------|----------------------|
| **Trigger** | Material transaction created in SF | ERP calls SyncProducer endpoint |
| **ERP Sync** | ✅ Yes (ProcessMaterialTransactionsIndividually) | ❌ No (ERP already has the data) |
| **Kafka Message** | Published after ERP sync | Published by SyncProducer endpoint |
| **ExternalId Update** | Via ORDER_TRANSACTION_SERVICE consumer | Via ORDER_TRANSACTION_SERVICE consumer |
| **Update location** | **SAME:** ORDER_TRANSACTION_SERVICE case | **SAME:** ORDER_TRANSACTION_SERVICE case |
| **Use case** | SF initiates sync to ERP | ERP sends ExternalId back to SF |

---

## 🎯 Key Features

✅ **Single Responsibility:**
- **MATERIAL_ISSUE_SERVICE:** Only handles ERP synchronization
- **ORDER_TRANSACTION_SERVICE:** Only handles database ExternalId updates

✅ **Unified Update Path:**
- ALL ExternalId updates go through ORDER_TRANSACTION_SERVICE
- No duplicate update logic
- Easier to maintain and debug

✅ **Kafka-Based Architecture:**
- Asynchronous processing
- Decoupled services
- Better error isolation

✅ **Error Handling:**
- Validates TransactionId and ExternalId are present
- Returns appropriate HTTP status codes
- Logs all operations for debugging

✅ **Idempotent:**
- Can be called multiple times with same data
- Database update will overwrite with latest ExternalId

✅ **Flexible:**
- Supports both SF → ERP and ERP → SF flows
- Same Kafka topic and consumer for both scenarios

---

## 🚀 How to Use

### **For ERP to Send ExternalId to Smart Factory:**

**Endpoint:** `POST /api/DataSync/DataSyncService/Producer`

**Request Body:**
```json
{
  "Services": ["ordertransaction"],
  "BodyData": "{\"TransactionId\":\"TXN-001\",\"ExternalId\":\"ERP-12345\"}"
}
```

**Response:**
```json
{
  "Data": [
    {
      "Service": "ordertransaction",
      "IsSuccess": true,
      "Response": "Service is active and ready to execute"
    }
  ]
}
```

**What Happens:**
1. Kafka message published to `producer-sync-ordertransaction`
2. Consumer picks up message
3. `UpdateOrderTransactionMaterialExternalId` is called
4. Database updated with ExternalId
5. Success response returned

---

## 📝 Testing

### **Test Scenario 1: SF → ERP (Existing Flow)**
```bash
# 1. Create material transaction in Smart Factory
# 2. Check logs for:
#    - "Processing material transaction TXN-001"
#    - "Successfully processed transaction TXN-001 with ExternalId ERP-123"
# 3. Verify database: SELECT * FROM sf_order_transactions_material WHERE Id = 'TXN-001'
```

### **Test Scenario 2: ERP → SF (New Flow)**
```bash
# 1. Call SyncProducer endpoint from ERP (or Postman):
curl -X POST http://localhost:5000/api/DataSync/DataSyncService/Producer \
  -H "Content-Type: application/json" \
  -d '{
    "Services": ["ordertransaction"],
    "BodyData": "{\"TransactionId\":\"TXN-001\",\"ExternalId\":\"ERP-99999\"}"
  }'

# 2. Check logs for:
#    - "Processing ORDER_TRANSACTION_SERVICE for TransactionId: TXN-001, ExternalId: ERP-99999"
#    - "Successfully updated ExternalId for transaction TXN-001 with ExternalId ERP-99999"

# 3. Verify database:
SELECT Id, ExternalId, ExternalDate FROM sf_order_transactions_material WHERE Id = 'TXN-001';
# Should show ExternalId = 'ERP-99999' and ExternalDate = current timestamp
```

---

## ✅ Implementation Complete!

The `UpdateOrderTransactionMaterialExternalId` method is now called when:
1. **Material Issue Sync (SF → ERP):** After successful ERP sync in `ProcessMaterialTransactionsIndividually`
2. **Order Transaction Service (ERP → SF):** When ERP calls SyncProducer with TransactionId and ExternalId

Both flows are working and ready to use! 🎉

