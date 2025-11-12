# Code Changes Summary - Database Schema Alignment

## Overview
Updated C# code to align with the existing MySQL database table structure for `sf_order_transactions_material` and `sf_order_transactions_material_detail`.

## Database Schema vs C# Model Mismatches

### Key Differences Found:

1. **Direction Field**
   - **Database**: ENUM('Issue', 'Return', 'Scrap')
   - **C# Model**: int (1=Issue, 2=Return, 3=Scrap)
   - **Solution**: Added conversion logic in repository

2. **Origin Field**
   - **Database**: ENUM('SF', 'ERP', 'APS')
   - **C# Code**: IntegrationSource enum (converted to int)
   - **Solution**: Added conversion logic to map to string

3. **Type Column**
   - **Database**: Column doesn't exist in `sf_order_transactions_material_detail`
   - **C# Code**: Tried to read `rdr["Type"]`
   - **Solution**: Set to empty string, removed database read

4. **Comments Column in Detail**
   - **Stored Procedure**: Returns as `DetailComments` (aliased to avoid conflict)
   - **C# Code**: Was reading as `Comments`
   - **Solution**: Updated to read `DetailComments`

## Files Modified

### 1. OrderTransactionMaterialRepo.cs
**Location**: `KafkaSyncService\EWP.SF.KafkaSync.DataAccess\Repository\OrderTransactionMaterialRepo.cs`

#### Change 1: MergeOrderTransactionMaterial - Direction Conversion (Lines 47-60)
```csharp
// Convert Direction from INT to ENUM string (1=Issue, 2=Return, 3=Scrap)
string directionStr = OrderMaterialInfo.Direction switch
{
    1 => "Issue",
    2 => "Return",
    3 => "Scrap",
    _ => "Issue" // Default to Issue
};
command.Parameters.AddWithValue("_Direction", directionStr);
```

#### Change 2: MergeOrderTransactionMaterial - Origin Conversion (Lines 64-75)
```csharp
// Convert Origin from IntegrationSource enum to database ENUM string (SF, ERP, APS)
string originStr = intSrc switch
{
    IntegrationSource.ERP => "ERP",
    IntegrationSource.APS => "APS",
    _ => "SF" // Default to SF
};
command.Parameters.AddWithValue("_Origin", originStr);
```

#### Change 3: GetOrderTransactionMaterialByTransactionId - Direction Mapping (Lines 211-235)
```csharp
// Map Direction from ENUM string to INT (Issue=1, Return=2, Scrap=3)
string directionStr = rdr["Direction"].ToStr();
int directionInt = directionStr switch
{
    "Issue" => 1,
    "Return" => 2,
    "Scrap" => 3,
    _ => 1 // Default to Issue
};

var material = new OrderTransactionMaterial
{
    // ... other fields
    Direction = directionInt,
    // ... other fields
};
```

#### Change 4: GetOrderTransactionMaterialByTransactionId - Detail Type Fix (Lines 243-263)
```csharp
var detail = new OrderTransactionMaterialDetail
{
    // ... other fields
    Comments = rdr["DetailComments"].ToStr(),
    Type = string.Empty // Type column doesn't exist in database
};
```

#### Change 5: GetOrderTransactionMaterialWithoutExternalId - Direction Mapping (Lines 329-358)
Same Direction conversion logic as Change 3.

#### Change 6: GetOrderTransactionMaterialWithoutExternalId - Detail Type Fix (Lines 363-383)
Same Type fix as Change 4.

### 2. SQL Stored Procedures
**Location**: `Database/INSTALL_PROCEDURES_ONLY.sql`

All stored procedures updated to:
- Use `Id` field instead of `TransactionId` as primary key
- Return Direction as ENUM string
- Return Origin as ENUM string
- Alias detail Comments as `DetailComments`
- Map database fields to expected C# field names

## Data Type Mappings

| C# Model Field | Database Column | Type Conversion |
|----------------|-----------------|-----------------|
| Direction (int) | Direction (ENUM) | 1→Issue, 2→Return, 3→Scrap |
| Origin (IntegrationSource) | Origin (ENUM) | ERP→ERP, APS→APS, SF→SF |
| Type (string) | N/A | Set to empty string |
| Comments (detail) | Comments | Read as DetailComments |

## Testing Recommendations

1. **Test Material Issue Creation**
   - Verify Direction is saved correctly (1 → 'Issue')
   - Verify Origin is saved correctly (IntegrationSource.ERP → 'ERP')

2. **Test Material Retrieval**
   - Verify Direction is read correctly ('Issue' → 1)
   - Verify all detail fields are populated
   - Verify Type field doesn't cause errors

3. **Test ERP Sync**
   - Create transaction with Direction=1 (Issue)
   - Retrieve pending transactions (ExternalId is null)
   - Send to ERP
   - Update ExternalId after success
   - Verify Kafka message is published

## Potential Issues to Watch

1. **Scrap Direction**: The database supports 'Scrap' (value 3) but verify if the application uses it
2. **Type Field**: Currently set to empty string - verify if this affects ERP integration
3. **Origin Values**: Ensure all IntegrationSource enum values are mapped correctly

## Next Steps

1. ✅ Install stored procedures using `INSTALL_PROCEDURES_ONLY.sql`
2. ✅ Code changes completed in `OrderTransactionMaterialRepo.cs`
3. ⏳ Test the application end-to-end
4. ⏳ Verify ERP integration works correctly
5. ⏳ Monitor for any runtime errors related to data type mismatches

