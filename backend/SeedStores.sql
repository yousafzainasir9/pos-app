-- =============================================
-- Cookie Barrel POS - Stores Seeding Script
-- =============================================
-- Run this in SQL Server Management Studio
-- Database: POSDatabase
-- =============================================

USE POSDatabase;
GO

-- Check if stores exist
PRINT '=== Checking existing stores ===';
SELECT 
    Id, 
    Name, 
    Code, 
    City,
    IsActive,
    CreatedAt
FROM Stores;

PRINT '';
PRINT 'Total Active Stores: ' + CAST((SELECT COUNT(*) FROM Stores WHERE IsActive = 1) AS VARCHAR);
PRINT '';

-- If no stores exist, seed them
IF NOT EXISTS (SELECT 1 FROM Stores WHERE IsActive = 1)
BEGIN
    PRINT '=== No active stores found. Seeding stores... ===';
    
    INSERT INTO Stores (
        Name, 
        Code, 
        Address, 
        City, 
        State, 
        PostalCode, 
        Country, 
        Phone, 
        Email, 
        TaxNumber,
        TaxRate, 
        Currency, 
        IsActive, 
        OpeningTime, 
        ClosingTime, 
        CreatedAt,
        UpdatedAt
    )
    VALUES 
    -- Store 1: Main Street
    (
        'Cookie Barrel - Main Street', 
        'CB-MS', 
        '123 Main Street', 
        'Sydney', 
        'NSW', 
        '2000', 
        'Australia', 
        '02-1234-5678', 
        'mainstreet@cookiebarrel.com.au', 
        'ABN 12345678901',
        10.0, 
        'AUD', 
        1, 
        '08:00:00', 
        '20:00:00', 
        GETDATE(),
        GETDATE()
    ),
    
    -- Store 2: Harbor Point
    (
        'Cookie Barrel - Harbor Point', 
        'CB-HP', 
        '456 Harbor Road', 
        'Sydney', 
        'NSW', 
        '2000', 
        'Australia', 
        '02-2345-6789', 
        'harbor@cookiebarrel.com.au', 
        'ABN 12345678902',
        10.0, 
        'AUD', 
        1, 
        '08:00:00', 
        '20:00:00', 
        GETDATE(),
        GETDATE()
    ),
    
    -- Store 3: Beach Side
    (
        'Cookie Barrel - Beach Side', 
        'CB-BS', 
        '789 Beach Boulevard', 
        'Bondi', 
        'NSW', 
        '2026', 
        'Australia', 
        '02-3456-7890', 
        'beach@cookiebarrel.com.au', 
        'ABN 12345678903',
        10.0, 
        'AUD', 
        1, 
        '07:00:00', 
        '21:00:00', 
        GETDATE(),
        GETDATE()
    ),
    
    -- Store 4: City Center
    (
        'Cookie Barrel - City Center', 
        'CB-CC', 
        '321 George Street', 
        'Sydney', 
        'NSW', 
        '2000', 
        'Australia', 
        '02-4567-8901', 
        'citycenter@cookiebarrel.com.au', 
        'ABN 12345678904',
        10.0, 
        'AUD', 
        1, 
        '06:30:00', 
        '22:00:00', 
        GETDATE(),
        GETDATE()
    ),
    
    -- Store 5: Westfield
    (
        'Cookie Barrel - Westfield', 
        'CB-WF', 
        '100 Market Street', 
        'Sydney', 
        'NSW', 
        '2000', 
        'Australia', 
        '02-5678-9012', 
        'westfield@cookiebarrel.com.au', 
        'ABN 12345678905',
        10.0, 
        'AUD', 
        1, 
        '09:00:00', 
        '21:00:00', 
        GETDATE(),
        GETDATE()
    );
    
    PRINT 'Stores seeded successfully!';
    PRINT '';
END
ELSE
BEGIN
    PRINT '=== Active stores already exist ===';
    PRINT '';
END

-- Display final store list
PRINT '=== Final Store List ===';
SELECT 
    Id, 
    Name, 
    Code, 
    Address,
    City,
    Phone,
    IsActive
FROM Stores
WHERE IsActive = 1
ORDER BY Name;

PRINT '';
PRINT '=== Done! ===';
PRINT 'Total Active Stores: ' + CAST((SELECT COUNT(*) FROM Stores WHERE IsActive = 1) AS VARCHAR);

GO
