-- =============================================
-- Master Script: Create All Order Transaction Material Stored Procedures
-- Description: Executes all stored procedure creation scripts
-- Database: ewp_sf_devsync (or your database name)
-- =============================================

-- Make sure you're using the correct database
USE ewp_sf_devsync;

-- Execute the scripts in order
SOURCE SP_SF_OrderTransactionMaterial_SEL.sql;
SOURCE SP_SF_OrderTransactionMaterial_UPD.sql;
SOURCE SP_SF_OrderTransactionMaterial_INS.sql;

-- Verify procedures were created
SELECT 
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED,
    LAST_ALTERED
FROM information_schema.ROUTINES
WHERE ROUTINE_SCHEMA = 'ewp_sf_devsync'
  AND ROUTINE_NAME LIKE 'SP_SF_OrderTransactionMaterial%'
ORDER BY ROUTINE_NAME;

