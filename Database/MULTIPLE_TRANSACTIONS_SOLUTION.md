# Multiple Transactions Without ExternalId - Solution Options

## Current Problem

The `GetMaterialTransactionRequestParams` method:
1. Retrieves **ALL** transactions where `ExternalId IS NULL`
2. Only processes the **FIRST** transaction
3. **Ignores** all other transactions
4. Results in an infinite loop where only the first transaction is retried

## Solution Options

### Option 1: Process ONE Transaction at a Time (RECOMMENDED) ⭐

**How it works:**
- Each sync run processes exactly ONE pending transaction
- After successful ERP response, update that transaction's ExternalId
- Next sync run will pick up the next pending transaction
- Continue until all transactions are processed

**Advantages:**
- ✅ Simple and reliable
- ✅ Easy error handling (one transaction fails, others continue)
- ✅ Clear audit trail (one transaction = one log entry)
- ✅ Matches current architecture (one request = one transaction)

**Current Code (Already Correct!):**
```csharp
// Get all transactions where ExternalId is null or empty
List<OrderTransactionMaterial> transactions = await _orderTransactionMaterialRepo
    .GetOrderTransactionMaterialWithoutExternalId(cancel).ConfigureAwait(false);

if (transactions == null || transactions.Count == 0)
{
    throw new Exception("No transactions found without ExternalId");
}

// Get the first transaction
OrderTransactionMaterial? transaction = transactions.FirstOrDefault();
```

**What happens:**
1. **Run 1**: Process Transaction A → Update ExternalId = "ERP-001"
2. **Run 2**: Process Transaction B → Update ExternalId = "ERP-002"
3. **Run 3**: Process Transaction C → Update ExternalId = "ERP-003"
4. Continue until all transactions are synced

**No code changes needed!** The current implementation is correct for this approach.

---

### Option 2: Process ALL Transactions in a Batch

**How it works:**
- Each sync run processes ALL pending transactions
- Send all transactions to ERP in one request
- Update all ExternalIds after success

**Advantages:**
- ✅ Faster processing (all at once)
- ✅ Fewer API calls to ERP

**Disadvantages:**
- ❌ If one transaction fails, all fail
- ❌ Harder to track which transaction caused the error
- ❌ Requires ERP to support batch processing
- ❌ More complex error handling

**Code Changes Required:**

```csharp
public async Task<object> GetMaterialTransactionRequestParams(User systemOperator, CancellationToken cancel = default)
{
    // Get all transactions where ExternalId is null or empty
    List<OrderTransactionMaterial> transactions = await _orderTransactionMaterialRepo
        .GetOrderTransactionMaterialWithoutExternalId(cancel).ConfigureAwait(false);

    if (transactions == null || transactions.Count == 0)
    {
        throw new Exception("No transactions found without ExternalId");
    }

    // Process ALL transactions
    List<object> allTransactions = new List<object>();

    foreach (var transaction in transactions)
    {
        // Get work order information
        List<WorkOrder> workOrders = await GetWorkOrder(transaction.OrderId).ConfigureAwait(false);
        WorkOrder? workOrder = workOrders?.FirstOrDefault();

        if (workOrder == null)
        {
            continue; // Skip this transaction
        }

        // Determine movement type based on direction
        string movType = transaction.Direction == 1 ? "Issue" : "Return";

        // Build components list from transaction details
        List<object> components = new List<object>();

        foreach (var detail in transaction.Details)
        {
            // ... build component object (same as current code)
        }

        // Create request parameters object for this transaction
        object requestParams = new
        {
            TransactionId = transaction.TransactionId,
            OrderType = workOrder.OrderType,
            OrderCode = workOrder.OrderCode,
            Date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            Type = movType,
            OperationNo = transaction.OperationId,
            Components = components,
            Comments = transaction.Comments ?? string.Empty,
            Employee = systemOperator.EmployeeId
        };

        allTransactions.Add(requestParams);
    }

    // Return array of all transactions
    return new
    {
        Transactions = allTransactions
    };
}
```

**Also need to update HandleMaterialIssueSuccess:**
```csharp
// Extract ALL TransactionIds from request body
dynamic requestData = JsonConvert.DeserializeObject(requestBody);
var transactionArray = requestData["Transactions"];

foreach (var trans in transactionArray)
{
    string transactionId = trans["TransactionId"];
    // Extract ExternalId from response for this transaction
    // Update database
    // Publish to Kafka
}
```

---

### Option 3: Hybrid Approach (Process N at a time)

**How it works:**
- Process a configurable number of transactions per run (e.g., 5 at a time)
- Balance between speed and reliability

**Example:**
```csharp
// Get first 5 transactions
var transactionsToProcess = transactions.Take(5).ToList();
```

---

## Recommendation

**Use Option 1** (Process ONE at a time) because:

1. ✅ **Current code already implements this correctly**
2. ✅ **No changes needed**
3. ✅ **Simple and reliable**
4. ✅ **Easy to debug and monitor**
5. ✅ **Each transaction has its own success/failure status**

The only thing you need to ensure is that:
- The sync service runs **frequently enough** (e.g., every 30 seconds)
- If you have 100 pending transactions, they'll all be processed within a reasonable time

---

## How to Verify Current Behavior

1. **Create 3 test transactions** without ExternalId
2. **Run the sync service**
3. **Check the database** - only the first transaction should have ExternalId updated
4. **Run the sync service again**
5. **Check the database** - now the second transaction should have ExternalId updated
6. **Continue** until all 3 are processed

---

## Monitoring Recommendations

Add logging to track pending transactions:

```csharp
public async Task<object> GetMaterialTransactionRequestParams(User systemOperator, CancellationToken cancel = default)
{
    List<OrderTransactionMaterial> transactions = await _orderTransactionMaterialRepo
        .GetOrderTransactionMaterialWithoutExternalId(cancel).ConfigureAwait(false);

    if (transactions == null || transactions.Count == 0)
    {
        throw new Exception("No transactions found without ExternalId");
    }

    // LOG: How many pending transactions
    _logger.LogInformation($"Found {transactions.Count} pending transactions. Processing first one.");

    OrderTransactionMaterial? transaction = transactions.FirstOrDefault();
    
    // ... rest of code
}
```

This way you can monitor:
- How many transactions are pending
- How fast they're being processed
- If any are stuck

