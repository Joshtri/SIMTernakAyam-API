-- Migration Script: Remove HargaPerKg column from HargaPasar table
-- Date: 2025-01-23
-- Description: This script removes the deprecated HargaPerKg column from HargaPasar table
--              as the system now uses HargaPerEkor (price per chicken) instead of 
--              HargaPerKg (price per kilogram)

-- Start transaction
START TRANSACTION;

-- Drop the HargaPerKg column
ALTER TABLE "HargaPasar" DROP COLUMN IF EXISTS "HargaPerKg";

-- Commit transaction
COMMIT;

-- Verify the column is removed
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'HargaPasar';
