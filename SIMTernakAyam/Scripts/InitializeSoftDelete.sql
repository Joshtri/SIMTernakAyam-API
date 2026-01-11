-- =========================================
-- Soft Delete Migration Script
-- Set default IsDeleted = false for existing data
-- =========================================

BEGIN TRANSACTION;

-- Update Ayams
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Ayams' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Ayams 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Ayams table';
END

-- Update Panens
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Panens' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Panens 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Panens table';
END

-- Update Mortalitas
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Mortalitas' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Mortalitas 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Mortalitas table';
END

-- Update Kandangs
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Kandangs' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Kandangs 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Kandangs table';
END

-- Update Users
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Users 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Users table';
END

-- Update Operasionals
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Operasionals' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Operasionals 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Operasionals table';
END

-- Update Pakans
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Pakans' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Pakans 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Pakans table';
END

-- Update Vaksins
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vaksins' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Vaksins 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Vaksins table';
END

-- Update Biayas
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Biayas' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Biayas 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Biayas table';
END

-- Update JenisKegiatans
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JenisKegiatans' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE JenisKegiatans 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated JenisKegiatans table';
END

-- Update JurnalHarians
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JurnalHarians' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE JurnalHarians 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated JurnalHarians table';
END

-- Update KandangAsistens
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KandangAsistens' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE KandangAsistens 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated KandangAsistens table';
END

-- Update Notifications
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Notifications' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE Notifications 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated Notifications table';
END

-- Update HargaPasars
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HargaPasars' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    UPDATE HargaPasars 
    SET IsDeleted = 0 
    WHERE IsDeleted IS NULL OR IsDeleted = 1;
    
    PRINT 'Updated HargaPasars table';
END

COMMIT TRANSACTION;

PRINT 'Soft Delete initialization completed successfully!';
PRINT 'All existing records are set to IsDeleted = false';

-- =========================================
-- Create indexes for better performance
-- =========================================

-- Index for Ayams
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Ayams_IsDeleted' AND object_id = OBJECT_ID('Ayams'))
BEGIN
    CREATE INDEX IX_Ayams_IsDeleted ON Ayams(IsDeleted);
    PRINT 'Created index IX_Ayams_IsDeleted';
END

-- Index for Panens
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Panens_IsDeleted' AND object_id = OBJECT_ID('Panens'))
BEGIN
    CREATE INDEX IX_Panens_IsDeleted ON Panens(IsDeleted);
    PRINT 'Created index IX_Panens_IsDeleted';
END

-- Index for Mortalitas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Mortalitas_IsDeleted' AND object_id = OBJECT_ID('Mortalitas'))
BEGIN
    CREATE INDEX IX_Mortalitas_IsDeleted ON Mortalitas(IsDeleted);
    PRINT 'Created index IX_Mortalitas_IsDeleted';
END

-- Index for Operasionals
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Operasionals_IsDeleted' AND object_id = OBJECT_ID('Operasionals'))
BEGIN
    CREATE INDEX IX_Operasionals_IsDeleted ON Operasionals(IsDeleted);
    PRINT 'Created index IX_Operasionals_IsDeleted';
END

PRINT 'Performance indexes created successfully!';

-- =========================================
-- Verification Query
-- =========================================

SELECT 
    'Ayams' AS TableName,
    COUNT(*) AS TotalRecords,
    SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END) AS ActiveRecords,
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS DeletedRecords
FROM Ayams
UNION ALL
SELECT 
    'Panens',
    COUNT(*),
    SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END)
FROM Panens
UNION ALL
SELECT 
    'Mortalitas',
    COUNT(*),
    SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END)
FROM Mortalitas
UNION ALL
SELECT 
    'Operasionals',
    COUNT(*),
    SUM(CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END)
FROM Operasionals;

PRINT 'Verification complete!';
