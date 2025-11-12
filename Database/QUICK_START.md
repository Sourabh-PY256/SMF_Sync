# Quick Start Guide - Order Transaction Material Stored Procedures

## ✅ Your Database Tables (Already Exist)

### Table: `sf_order_transactions_material`
- **Primary Key**: `Id` (varchar(36))
- **Key Fields**: OrderCode, OperationNo, Direction, ExternalId, ExternalDate
- **Direction Values**: 'Issue', 'Return', 'Scrap'
- **Origin Values**: 'SF', 'ERP', 'APS'

### Table: `sf_order_transactions_material_detail`
- **Foreign Key**: `TransactionId` → `sf_order_transactions_material.Id`
- **Key Fields**: ItemCode, Quantity, LotNumber, WarehouseCode

## 🚀 Installation (Choose ONE method)

### Method 1: Simple Installation (RECOMMENDED)
```bash
mysql -u your_username -p ewp_sf_devsync < Database/INSTALL_PROCEDURES_ONLY.sql
```

### Method 2: MySQL Workbench
1. Open MySQL Workbench
2. Connect to `ewp_sf_devsync` database
3. File → Open SQL Script → Select `INSTALL_PROCEDURES_ONLY.sql`
4. Click Execute (⚡ icon)

### Method 3: Copy & Paste
1. Open `Database/INSTALL_PROCEDURES_ONLY.sql`
2. Copy all content
3. Paste into MySQL client
4. Execute

## 📋 What Gets Installed

Three stored procedures will be created:

1. **SP_SF_OrderTransactionMaterial_SEL** - Retrieves transactions
2. **SP_SF_OrderTransactionMaterial_UPD** - Updates ExternalId
3. **SP_SF_OrderTransactionMaterial_INS** - Creates/updates transactions

## ✅ Verification

After installation, run this to verify:

```sql
SHOW PROCEDURE STATUS 
WHERE Db = 'ewp_sf_devsync' 
AND Name LIKE 'SP_SF_OrderTransactionMaterial%';
```

You should see 3 procedures listed.

## 🔧 How It Works

### 1. Application Creates Transaction
When material is issued in your application, it calls `SP_SF_OrderTransactionMaterial_INS` to save the transaction with `ExternalId = NULL`.

### 2. Sync to ERP
The application retrieves pending transactions (where `ExternalId IS NULL`) using `SP_SF_OrderTransactionMaterial_SEL` and sends them to the ERP system.

### 3. Update After Success
After successful ERP response, the application calls `SP_SF_OrderTransactionMaterial_UPD` to update the `ExternalId` with the value received from ERP.

### 4. Kafka Message
A Kafka message is published to `producer-sync-ordertransaction` topic with the transaction details.

## 📝 Important Notes

- **ExternalId**: Tracks which transactions have been synced to ERP
- **Direction Mapping**: 
  - C# code uses: `Direction = 1` (Issue), `2` (Return)
  - Database uses: `Direction = 'Issue'`, `'Return'`, `'Scrap'`
  - The stored procedures handle this mapping automatically
- **TransactionId**: Uses UUID format (e.g., `550e8400-e29b-41d4-a716-446655440000`)

## 🐛 Troubleshooting

### Error: "Procedure does not exist"
- Make sure you ran the installation script
- Verify you're connected to the correct database (`ewp_sf_devsync`)

### Error: "Table doesn't exist"
- Your tables already exist, so this shouldn't happen
- If it does, check table names are exactly: `sf_order_transactions_material` and `sf_order_transactions_material_detail`

### Error: "Syntax error near LEAVE"
- This has been fixed in the latest version
- Make sure you're using the updated `INSTALL_PROCEDURES_ONLY.sql` file

## 📞 Need Help?

Check the following files for more details:
- `Database/INSTALL_PROCEDURES_ONLY.sql` - Main installation script
- `Database/COMPLETE_SETUP.sql` - Complete setup with table creation (not needed)
- `Database/StoredProcedures/README.md` - Detailed documentation

