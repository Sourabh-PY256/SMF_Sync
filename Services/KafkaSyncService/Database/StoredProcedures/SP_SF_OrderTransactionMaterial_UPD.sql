-- =============================================
-- Stored Procedure: SP_SF_OrderTransactionMaterial_UPD
-- Description: Updates the ExternalId for a specific order transaction material
-- Parameters:
--   _TransactionId: The transaction ID to update
--   _ExternalId: The external ID value to set
--   _User: The user ID performing the update
-- Returns: IsSuccess (1 = success, 0 = failure)
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

