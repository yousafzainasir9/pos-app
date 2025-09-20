-- Check if POSDatabase exists
USE master;
GO

IF DB_ID('POSDatabase') IS NOT NULL
    PRINT 'Database POSDatabase exists!'
ELSE
    PRINT 'Database POSDatabase does NOT exist!'
GO

-- If database exists, show its structure
IF DB_ID('POSDatabase') IS NOT NULL
BEGIN
    USE POSDatabase;
    
    -- Show all tables
    PRINT '';
    PRINT '=== TABLES IN POSDatabase ===';
    SELECT 
        t.name AS TableName,
        p.rows AS RowCount
    FROM sys.tables t
    INNER JOIN sys.partitions p ON t.object_id = p.object_id
    WHERE p.index_id IN (0, 1)
    ORDER BY t.name;
    
    -- Show sample data from key tables
    PRINT '';
    PRINT '=== STORES ===';
    SELECT TOP 5 * FROM Stores WHERE IsDeleted = 0;
    
    PRINT '';
    PRINT '=== USERS ===';
    SELECT TOP 5 Id, Username, Email, FirstName, LastName, Role, IsActive, Pin FROM Users WHERE IsDeleted = 0;
    
    PRINT '';
    PRINT '=== CATEGORIES ===';
    SELECT TOP 5 * FROM Categories WHERE IsDeleted = 0;
    
    PRINT '';
    PRINT '=== PRODUCT COUNT BY CATEGORY ===';
    SELECT 
        c.Name as Category,
        COUNT(DISTINCT s.Id) as SubcategoryCount,
        COUNT(DISTINCT p.Id) as ProductCount
    FROM Categories c
    LEFT JOIN Subcategories s ON c.Id = s.CategoryId
    LEFT JOIN Products p ON s.Id = p.SubcategoryId
    WHERE c.IsDeleted = 0
    GROUP BY c.Id, c.Name
    ORDER BY c.Name;
END
GO
