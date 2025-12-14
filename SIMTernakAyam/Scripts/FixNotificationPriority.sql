### ========================================
### FIX EXISTING NOTIFICATIONS - UPDATE PRIORITY
### ========================================

### Run this SQL to fix existing notifications with empty Priority
### Execute directly in PostgreSQL or use pgAdmin

-- Update all notifications with empty/null Priority to have default 'medium'
UPDATE "Notifications"
SET "Priority" = 'medium'
WHERE "Priority" IS NULL OR "Priority" = '';

-- Verify the update
SELECT "Id", "Title", "Priority" 
FROM "Notifications" 
LIMIT 10;
