# Order Transaction Material Stored Procedures

This folder contains the MySQL stored procedures required for the Order Transaction Material functionality.

## Required Database Tables

Before creating the stored procedures, ensure the following tables exist in your database:

### 1. `sf_order_transactions_material`
Main table for order transaction materials:

```sql
CREATE TABLE IF NOT EXISTS `sf_order_transactions_material` (
    `TransactionId` VARCHAR(50) PRIMARY KEY,
    `OrderCode` VARCHAR(50) NOT NULL,
    `OperationNo` VARCHAR(50) NOT NULL,
    `Direction` INT NOT NULL COMMENT '1=Issue, 2=Return',
    `EmployeeId` VARCHAR(50),
    `DocCode` VARCHAR(50),
    `Comments` TEXT,
    `DocDate` DATETIME,
    `LogDate` DATETIME,
    `UserId` INT,
    `ExternalId` VARCHAR(50) NULL COMMENT 'External system reference ID',
    `CreateDate` DATETIME,
    `CreateUser` INT,
    `UpdateDate` DATETIME,
    `UpdateUser` INT,
    `Origin` INT COMMENT '1=ERP, 2=Manual',
    INDEX `idx_order_operation` (`OrderCode`, `OperationNo`),
    INDEX `idx_external_id` (`ExternalId`),
    INDEX `idx_log_date` (`LogDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 2. `sf_order_transactions_material_detail`
Detail table for transaction line items:

```sql
CREATE TABLE IF NOT EXISTS `sf_order_transactions_material_detail` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `TransactionId` VARCHAR(50) NOT NULL,
    `MachineCode` VARCHAR(50),
    `OriginalItemCode` VARCHAR(50),
    `ItemCode` VARCHAR(50) NOT NULL,
    `LineNo` VARCHAR(50),
    `Quantity` DECIMAL(18,6) NOT NULL,
    `LotNumber` VARCHAR(50),
    `Pallet` VARCHAR(50),
    `BinLocationCode` VARCHAR(50),
    `InventoryStatusCode` VARCHAR(50),
    `ExpDate` DATE,
    `ManufactureDate` DATE,
    `WarehouseCode` VARCHAR(50),
    FOREIGN KEY (`TransactionId`) REFERENCES `sf_order_transactions_material`(`TransactionId`) ON DELETE CASCADE,
    INDEX `idx_transaction_id` (`TransactionId`),
    INDEX `idx_item_code` (`ItemCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

## Stored Procedures

### 1. **SP_SF_OrderTransactionMaterial_SEL**
Retrieves order transaction material data with details.

**Parameters:**
- `_TransactionId` (VARCHAR(50)): Filter by specific transaction ID (empty string = all)
- `_ExternalId` (VARCHAR(50)): Filter by ExternalId (empty string = filter for NULL/empty ExternalId)

**Returns:** Two result sets
1. Material transactions
2. Material transaction details

**Usage Examples:**
```sql
-- Get all transactions without ExternalId
CALL SP_SF_OrderTransactionMaterial_SEL('', '');

-- Get specific transaction by ID
CALL SP_SF_OrderTransactionMaterial_SEL('MTR-20240101120000-1234', NULL);
```

### 2. **SP_SF_OrderTransactionMaterial_INS**
Inserts or updates order transaction material with details.

**Parameters:**
- `_IsValidation` (TINYINT): If 1, only validates without saving
- `_TransactionId` (VARCHAR(50)): Transaction ID (NULL for new records)
- `_OrderCode` (VARCHAR(50)): Order code
- `_OperationNo` (VARCHAR(50)): Operation number
- `_Direction` (INT): Direction (1 = Issue, 2 = Return)
- `_EmployeeId` (VARCHAR(50)): Employee ID
- `_DocCode` (VARCHAR(50)): Document code
- `_Comments` (TEXT): Comments
- `_DocDate` (DATETIME): Document date
- `_User` (INT): User ID performing the operation
- `_Details` (JSON): JSON array of transaction details
- `_Origin` (INT): Integration source (1 = ERP, 2 = Manual)

**Returns:** Action, IsSuccess, Code, Message

### 3. **SP_SF_OrderTransactionMaterial_UPD**
Updates the ExternalId for a specific order transaction material.

**Parameters:**
- `_TransactionId` (VARCHAR(50)): The transaction ID to update
- `_ExternalId` (VARCHAR(50)): The external ID value to set
- `_User` (INT): The user ID performing the update

**Returns:** IsSuccess (1 = success, 0 = failure)

**Usage Example:**
```sql
-- Update ExternalId after successful ERP sync
CALL SP_SF_OrderTransactionMaterial_UPD('MTR-20240101120000-1234', 'ERP-DOC-5678', 1);
```

## Installation Instructions

### Option 1: Execute Individual Scripts
1. Connect to your MySQL database
2. Execute each script in order:
   ```bash
   mysql -u username -p database_name < SP_SF_OrderTransactionMaterial_SEL.sql
   mysql -u username -p database_name < SP_SF_OrderTransactionMaterial_UPD.sql
   mysql -u username -p database_name < SP_SF_OrderTransactionMaterial_INS.sql
   ```

### Option 2: Execute Master Script
1. Navigate to this directory
2. Execute the master script:
   ```bash
   mysql -u username -p database_name < 00_CREATE_ALL_PROCEDURES.sql
   ```

### Option 3: Using MySQL Workbench
1. Open MySQL Workbench
2. Connect to your database
3. Open each `.sql` file
4. Execute the script (Ctrl+Shift+Enter or click Execute button)

## Verification

After installation, verify the procedures were created:

```sql
SHOW PROCEDURE STATUS WHERE Db = 'ewp_sf_devsync' AND Name LIKE 'SP_SF_OrderTransactionMaterial%';
```

## Notes

- Make sure to update the database name in `00_CREATE_ALL_PROCEDURES.sql` if your database is not named `ewp_sf_devsync`
- The procedures assume the existence of related tables: `sf_work_order`, `sf_work_order_process`
- The `ExternalId` field is used to track which transactions have been synced to external ERP systems
- Transactions with NULL or empty `ExternalId` are considered pending for ERP sync

## Troubleshooting

If you encounter errors:

1. **Table doesn't exist**: Create the required tables first (see above)
2. **Permission denied**: Ensure your MySQL user has CREATE PROCEDURE privilege
3. **Syntax error**: Ensure you're using MySQL 5.7+ (JSON support required)
4. **Foreign key constraint**: Ensure referenced tables exist before creating detail table

