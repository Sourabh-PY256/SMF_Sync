-- =============================================
-- COMPLETE SETUP SCRIPT FOR ORDER TRANSACTION MATERIAL
-- Database: ewp_sf_devsync
-- Description: Creates tables and stored procedures
-- =============================================

-- Use the correct database
USE ewp_sf_devsync;

-- =============================================
-- STEP 1: VERIFY TABLES EXIST
-- =============================================
-- Tables already exist in database with the following structure:
--
-- sf_order_transactions_material:
--   Id (varchar(36) PK), OrderCode, OperationNo (decimal), Direction (enum),
--   ExternalId, ExternalDate, Comments, CreateDate, CreateUser, CreateEmployee, Origin (enum)
--
-- sf_order_transactions_material_detail:
--   TransactionId, MachineCode, OriginalItemCode, ItemCode, LineNo (int),
--   Quantity (decimal), LotNumber, Pallet, BinLocationCode, InventoryStatusCode,
--   ExpDate, AllocationOrderCode, WarehouseCode, ScrapTypeCode, Comments
--
-- Skipping table creation as they already exist...

-- =============================================
-- STEP 2: CREATE STORED PROCEDURES
-- =============================================

-- =============================================
-- Procedure 1: SP_SF_OrderTransactionMaterial_SEL
-- =============================================

DROP PROCEDURE IF EXISTS `SP_SF_OrderTransactionMaterial_SEL`;

DELIMITER $$

CREATE PROCEDURE `SP_SF_OrderTransactionMaterial_SEL`(
    IN _TransactionId VARCHAR(36),
    IN _ExternalId VARCHAR(100)
)
BEGIN
    -- First result set: Material transactions
    SELECT
        otm.Id AS TransactionId,
        otm.OrderCode,
        otm.OperationNo,
        otm.Direction,
        otm.CreateEmployee AS EmployeeId,
        '' AS DocCode,
        otm.Comments,
        otm.ExternalDate AS DocDate,
        otm.CreateDate AS LogDate,
        otm.CreateUser AS UserId,
        IFNULL(otm.ExternalId, '') AS ExternalId
    FROM sf_order_transactions_material otm
    WHERE
        -- Filter by TransactionId if provided
        (TRIM(_TransactionId) = '' OR otm.Id = _TransactionId)
        AND
        -- Filter by ExternalId if provided
        (
            _ExternalId IS NULL
            OR TRIM(_ExternalId) = '' AND (otm.ExternalId IS NULL OR TRIM(otm.ExternalId) = '')
            OR otm.ExternalId = _ExternalId
        )
    ORDER BY otm.CreateDate ASC;  -- ASC to process OLDEST transactions first

    -- Second result set: Material transaction details
    SELECT
        otmd.TransactionId,
        otmd.MachineCode,
        otmd.OriginalItemCode,
        otmd.ItemCode,
        otmd.LineNo,
        otmd.Quantity,
        otmd.LotNumber,
        otmd.Pallet,
        otmd.BinLocationCode,
        otmd.InventoryStatusCode,
        otmd.ExpDate,
        otmd.WarehouseCode,
        otmd.AllocationOrderCode,
        otmd.ScrapTypeCode,
        otmd.Comments AS DetailComments
    FROM sf_order_transactions_material_detail otmd
    INNER JOIN sf_order_transactions_material otm ON otmd.TransactionId = otm.Id
    WHERE
        -- Filter by TransactionId if provided
        (TRIM(_TransactionId) = '' OR otmd.TransactionId = _TransactionId)
        AND
        -- Filter by ExternalId if provided
        (
            _ExternalId IS NULL
            OR TRIM(_ExternalId) = '' AND (otm.ExternalId IS NULL OR TRIM(otm.ExternalId) = '')
            OR otm.ExternalId = _ExternalId
        )
    ORDER BY otmd.TransactionId, otmd.LineNo;

END$$

DELIMITER ;

-- =============================================
-- Procedure 2: SP_SF_OrderTransactionMaterial_UPD
-- =============================================

DROP PROCEDURE IF EXISTS `SP_SF_OrderTransactionMaterial_UPD`;

DELIMITER $$

CREATE PROCEDURE `SP_SF_OrderTransactionMaterial_UPD`(
    IN _TransactionId VARCHAR(36),
    IN _ExternalId VARCHAR(100),
    IN _User INT
)
BEGIN
    DECLARE _IsSuccess INT DEFAULT 0;
    DECLARE _RowsAffected INT DEFAULT 0;

    -- Update the ExternalId for the specified transaction
    UPDATE sf_order_transactions_material
    SET
        ExternalId = _ExternalId,
        ExternalDate = NOW()
    WHERE Id = _TransactionId;

    -- Get the number of rows affected
    SET _RowsAffected = ROW_COUNT();

    -- Set success flag based on rows affected
    IF _RowsAffected > 0 THEN
        SET _IsSuccess = 1;
    END IF;

    -- Return the result
    SELECT _IsSuccess AS IsSuccess;

END$$

DELIMITER ;

-- =============================================
-- Procedure 3: SP_SF_OrderTransactionMaterial_INS
-- =============================================

DROP PROCEDURE IF EXISTS `SP_SF_OrderTransactionMaterial_INS`;

DELIMITER $$

CREATE PROCEDURE `SP_SF_OrderTransactionMaterial_INS`(
    IN _IsValidation TINYINT,
    IN _TransactionId VARCHAR(36),
    IN _OrderCode VARCHAR(100),
    IN _OperationNo DECIMAL(6,3),
    IN _Direction VARCHAR(20),
    IN _EmployeeId VARCHAR(100),
    IN _DocCode VARCHAR(50),
    IN _Comments VARCHAR(500),
    IN _DocDate DATETIME,
    IN _User INT,
    IN _Details JSON,
    IN _Origin VARCHAR(20)
)
proc_label: BEGIN
    DECLARE _Action INT DEFAULT 1;
    DECLARE _IsSuccess INT DEFAULT 0;
    DECLARE _Code VARCHAR(50) DEFAULT '';
    DECLARE _Message VARCHAR(500) DEFAULT '';
    DECLARE _NewTransactionId VARCHAR(50);
    DECLARE _Exists INT DEFAULT 0;
    DECLARE _DetailCount INT DEFAULT 0;

    -- Error handler
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SELECT
            0 AS Action,
            0 AS IsSuccess,
            '' AS Code,
            'Error occurred during transaction' AS Message;
    END;

    -- Validation: Check if order exists (skip if table doesn't exist)
    SET @table_exists = 0;
    SELECT COUNT(*) INTO @table_exists
    FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND table_name = 'sf_work_order';

    IF @table_exists > 0 THEN
        IF NOT EXISTS (SELECT 1 FROM sf_work_order WHERE OrderCode = _OrderCode) THEN
            SELECT
                0 AS Action,
                0 AS IsSuccess,
                '' AS Code,
                CONCAT('Order ', _OrderCode, ' does not exist') AS Message;
            LEAVE proc_label;
        END IF;
    END IF;

    -- If validation only, return success
    IF _IsValidation = 1 THEN
        SELECT
            1 AS Action,
            1 AS IsSuccess,
            _OrderCode AS Code,
            'Validation successful' AS Message;
        LEAVE proc_label;
    END IF;
    
    START TRANSACTION;
    
    -- Check if transaction exists
    IF _TransactionId IS NOT NULL AND _TransactionId != '' THEN
        SELECT COUNT(*) INTO _Exists
        FROM sf_order_transactions_material
        WHERE Id = _TransactionId;

        IF _Exists > 0 THEN
            SET _Action = 2;
            SET _NewTransactionId = _TransactionId;
        END IF;
    END IF;

    -- Generate new TransactionId (UUID) if needed
    IF _NewTransactionId IS NULL OR _NewTransactionId = '' THEN
        SET _NewTransactionId = UUID();
        SET _Action = 1;
    END IF;

    -- Insert or Update main transaction
    IF _Action = 1 THEN
        INSERT INTO sf_order_transactions_material (
            Id, OrderCode, OperationNo, Direction,
            Comments, ExternalDate, ExternalId,
            CreateDate, CreateUser, CreateEmployee, Origin
        ) VALUES (
            _NewTransactionId, _OrderCode, _OperationNo, _Direction,
            _Comments, _DocDate, NULL,
            NOW(), _User, _EmployeeId, _Origin
        );
    ELSE
        UPDATE sf_order_transactions_material
        SET OrderCode = _OrderCode,
            OperationNo = _OperationNo,
            Direction = _Direction,
            Comments = _Comments,
            ExternalDate = _DocDate
        WHERE Id = _NewTransactionId;
    END IF;

    -- Delete existing details if updating
    IF _Action = 2 THEN
        DELETE FROM sf_order_transactions_material_detail WHERE TransactionId = _NewTransactionId;
    END IF;
    
    -- Insert details from JSON
    IF _Details IS NOT NULL AND JSON_LENGTH(_Details) > 0 THEN
        INSERT INTO sf_order_transactions_material_detail (
            TransactionId, MachineCode, OriginalItemCode, ItemCode, LineNo,
            Quantity, LotNumber, Pallet, BinLocationCode, InventoryStatusCode,
            ExpDate, WarehouseCode, AllocationOrderCode, ScrapTypeCode, Comments
        )
        SELECT
            _NewTransactionId,
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.MachineCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.OriginalItemCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.ItemCode')),
            CAST(JSON_UNQUOTE(JSON_EXTRACT(detail, '$.LineNo')) AS SIGNED),
            CAST(JSON_UNQUOTE(JSON_EXTRACT(detail, '$.Quantity')) AS DECIMAL(20,8)),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.LotNumber')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.Pallet')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.BinLocationCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.InventoryStatusCode')),
            STR_TO_DATE(JSON_UNQUOTE(JSON_EXTRACT(detail, '$.ExpDate')), '%Y-%m-%d'),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.WarehouseCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.AllocationOrderCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.ScrapTypeCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.Comments'))
        FROM JSON_TABLE(_Details, '$[*]' COLUMNS(detail JSON PATH '$')) AS jt;

        SET _DetailCount = ROW_COUNT();
    END IF;
    
    COMMIT;
    
    SET _IsSuccess = 1;
    SET _Code = _NewTransactionId;
    SET _Message = CONCAT('Transaction ', IF(_Action = 1, 'created', 'updated'), ' with ', _DetailCount, ' details');
    
    SELECT _Action AS Action, _IsSuccess AS IsSuccess, _Code AS Code, _Message AS Message;
    
END$$

DELIMITER ;

-- =============================================
-- STEP 3: VERIFY INSTALLATION
-- =============================================

-- Show created tables
SELECT 
    TABLE_NAME,
    TABLE_TYPE,
    ENGINE,
    TABLE_ROWS
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'ewp_sf_devsync'
  AND TABLE_NAME LIKE 'sf_order_transactions_material%'
ORDER BY TABLE_NAME;

-- Show created procedures
SELECT 
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED,
    LAST_ALTERED
FROM information_schema.ROUTINES
WHERE ROUTINE_SCHEMA = 'ewp_sf_devsync'
  AND ROUTINE_NAME LIKE 'SP_SF_OrderTransactionMaterial%'
ORDER BY ROUTINE_NAME;

-- Success message
SELECT 'Setup completed successfully!' AS Status;

