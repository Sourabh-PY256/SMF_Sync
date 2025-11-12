-- =============================================
-- Stored Procedure: SP_SF_OrderTransactionMaterial_SEL
-- Description: Retrieves order transaction material data with details
-- Parameters:
--   _TransactionId: Filter by specific transaction ID (empty string = all)
--   _ExternalId: Filter by ExternalId (empty string = filter for NULL/empty ExternalId)
-- Returns: Two result sets
--   1. Material transactions
--   2. Material transaction details
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

