-- =============================================
-- Stored Procedure: SP_SF_OrderTransactionMaterial_INS
-- Description: Inserts or updates order transaction material with details
-- Parameters:
--   _IsValidation: If 1, only validates without saving
--   _TransactionId: Transaction ID (NULL for new records)
--   _OrderCode: Order code
--   _OperationNo: Operation number
--   _Direction: Direction (1 = Issue, 2 = Return)
--   _EmployeeId: Employee ID
--   _DocCode: Document code
--   _Comments: Comments
--   _DocDate: Document date
--   _User: User ID performing the operation
--   _Details: JSON array of transaction details
--   _Origin: Integration source (1 = ERP, 2 = Manual, etc.)
-- Returns: Action, IsSuccess, Code, Message
-- =============================================

DROP PROCEDURE IF EXISTS `SP_SF_OrderTransactionMaterial_INS`;

DELIMITER $$

CREATE PROCEDURE `SP_SF_OrderTransactionMaterial_INS`(
    IN _IsValidation TINYINT,
    IN _TransactionId VARCHAR(50),
    IN _OrderCode VARCHAR(50),
    IN _OperationNo VARCHAR(50),
    IN _Direction INT,
    IN _EmployeeId VARCHAR(50),
    IN _DocCode VARCHAR(50),
    IN _Comments TEXT,
    IN _DocDate DATETIME,
    IN _User INT,
    IN _Details JSON,
    IN _Origin INT
)
proc_label: BEGIN
    DECLARE _Action INT DEFAULT 1; -- 1 = Insert, 2 = Update
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

    -- Validation: Check if order exists
    IF NOT EXISTS (SELECT 1 FROM sf_work_order WHERE OrderCode = _OrderCode) THEN
        SELECT
            0 AS Action,
            0 AS IsSuccess,
            '' AS Code,
            CONCAT('Order ', _OrderCode, ' does not exist') AS Message;
        LEAVE proc_label;
    END IF;

    -- Validation: Check if operation exists for the order
    IF NOT EXISTS (
        SELECT 1 FROM sf_work_order_process
        WHERE OrderCode = _OrderCode AND OperationNo = _OperationNo
    ) THEN
        SELECT
            0 AS Action,
            0 AS IsSuccess,
            '' AS Code,
            CONCAT('Operation ', _OperationNo, ' does not exist for order ', _OrderCode) AS Message;
        LEAVE proc_label;
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
        WHERE TransactionId = _TransactionId;
        
        IF _Exists > 0 THEN
            SET _Action = 2; -- Update
            SET _NewTransactionId = _TransactionId;
        END IF;
    END IF;
    
    -- Generate new TransactionId if needed
    IF _NewTransactionId IS NULL OR _NewTransactionId = '' THEN
        SET _NewTransactionId = CONCAT('MTR-', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s'), '-', FLOOR(RAND() * 10000));
        SET _Action = 1; -- Insert
    END IF;
    
    -- Insert or Update main transaction
    IF _Action = 1 THEN
        -- Insert new transaction
        INSERT INTO sf_order_transactions_material (
            TransactionId,
            OrderCode,
            OperationNo,
            Direction,
            EmployeeId,
            DocCode,
            Comments,
            DocDate,
            LogDate,
            UserId,
            ExternalId,
            CreateDate,
            CreateUser,
            Origin
        ) VALUES (
            _NewTransactionId,
            _OrderCode,
            _OperationNo,
            _Direction,
            _EmployeeId,
            _DocCode,
            _Comments,
            _DocDate,
            NOW(),
            _User,
            NULL, -- ExternalId is NULL initially
            NOW(),
            _User,
            _Origin
        );
    ELSE
        -- Update existing transaction
        UPDATE sf_order_transactions_material
        SET 
            OrderCode = _OrderCode,
            OperationNo = _OperationNo,
            Direction = _Direction,
            EmployeeId = _EmployeeId,
            DocCode = _DocCode,
            Comments = _Comments,
            DocDate = _DocDate,
            UpdateDate = NOW(),
            UpdateUser = _User
        WHERE TransactionId = _NewTransactionId;
    END IF;
    
    -- Delete existing details if updating
    IF _Action = 2 THEN
        DELETE FROM sf_order_transactions_material_detail 
        WHERE TransactionId = _NewTransactionId;
    END IF;
    
    -- Insert details from JSON
    IF _Details IS NOT NULL AND JSON_LENGTH(_Details) > 0 THEN
        INSERT INTO sf_order_transactions_material_detail (
            TransactionId,
            MachineCode,
            OriginalItemCode,
            ItemCode,
            LineNo,
            Quantity,
            LotNumber,
            Pallet,
            BinLocationCode,
            InventoryStatusCode,
            ExpDate,
            ManufactureDate,
            WarehouseCode
        )
        SELECT 
            _NewTransactionId,
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.MachineCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.OriginalItemCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.ItemCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.LineNo')),
            CAST(JSON_UNQUOTE(JSON_EXTRACT(detail, '$.Quantity')) AS DECIMAL(18,6)),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.LotNumber')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.Pallet')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.BinLocationCode')),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.InventoryStatusCode')),
            STR_TO_DATE(JSON_UNQUOTE(JSON_EXTRACT(detail, '$.ExpDate')), '%Y-%m-%d'),
            STR_TO_DATE(JSON_UNQUOTE(JSON_EXTRACT(detail, '$.ManufactureDate')), '%Y-%m-%d'),
            JSON_UNQUOTE(JSON_EXTRACT(detail, '$.WarehouseCode'))
        FROM JSON_TABLE(
            _Details,
            '$[*]' COLUMNS(detail JSON PATH '$')
        ) AS jt;
        
        SET _DetailCount = ROW_COUNT();
    END IF;
    
    COMMIT;
    
    SET _IsSuccess = 1;
    SET _Code = _NewTransactionId;
    SET _Message = CONCAT('Transaction ', IF(_Action = 1, 'created', 'updated'), ' successfully with ', _DetailCount, ' details');
    
    -- Return result
    SELECT 
        _Action AS Action,
        _IsSuccess AS IsSuccess,
        _Code AS Code,
        _Message AS Message;
    
END$$

DELIMITER ;

