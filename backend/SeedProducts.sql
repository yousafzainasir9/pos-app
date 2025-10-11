-- =============================================
-- Cookie Barrel POS - Products & Categories Seeding Script
-- =============================================
USE POSDatabase;
GO

PRINT '=== Seeding Categories ===';

-- Insert Categories if they don't exist
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Cookies')
BEGIN
    INSERT INTO Categories (Name, Description, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES 
    ('Cookies', 'Freshly baked cookies in various flavors', 1, 1, GETDATE(), GETDATE()),
    ('Cakes', 'Delicious cakes for every occasion', 2, 1, GETDATE(), GETDATE()),
    ('Pastries', 'Flaky and buttery pastries', 3, 1, GETDATE(), GETDATE()),
    ('Breads', 'Fresh artisan breads', 4, 1, GETDATE(), GETDATE()),
    ('Desserts', 'Sweet treats and desserts', 5, 1, GETDATE(), GETDATE());
    
    PRINT 'Categories seeded successfully!';
END
ELSE
BEGIN
    PRINT 'Categories already exist';
END

PRINT '';
PRINT '=== Seeding Products ===';

-- Get category IDs
DECLARE @CookiesId INT = (SELECT Id FROM Categories WHERE Name = 'Cookies');
DECLARE @CakesId INT = (SELECT Id FROM Categories WHERE Name = 'Cakes');
DECLARE @PastriesId INT = (SELECT Id FROM Categories WHERE Name = 'Pastries');
DECLARE @BreadsId INT = (SELECT Id FROM Categories WHERE Name = 'Breads');
DECLARE @DessertsId INT = (SELECT Id FROM Categories WHERE Name = 'Desserts');

-- Insert Products if they don't exist
IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Chocolate Chip Cookie')
BEGIN
    INSERT INTO Products (
        Name, Description, SKU, Barcode, CategoryId, SubcategoryId,
        Price, Cost, StockQuantity, ReorderLevel, Unit,
        IsActive, IsAvailableOnline, CreatedAt, UpdatedAt
    )
    VALUES 
    -- Cookies
    ('Chocolate Chip Cookie', 'Classic chocolate chip cookie with premium chocolate chunks', 'COOK-001', 'CC001', @CookiesId, NULL, 3.50, 1.20, 150, 20, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Oatmeal Raisin Cookie', 'Chewy oatmeal cookie with juicy raisins', 'COOK-002', 'OR002', @CookiesId, NULL, 3.25, 1.10, 120, 20, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Double Chocolate Cookie', 'Rich chocolate cookie with white chocolate chips', 'COOK-003', 'DC003', @CookiesId, NULL, 3.75, 1.30, 100, 15, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Peanut Butter Cookie', 'Soft peanut butter cookie with a criss-cross pattern', 'COOK-004', 'PB004', @CookiesId, NULL, 3.50, 1.25, 90, 15, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Sugar Cookie', 'Classic sugar cookie with sprinkles', 'COOK-005', 'SG005', @CookiesId, NULL, 3.00, 1.00, 130, 20, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Macadamia White Chocolate', 'Premium cookie with macadamia nuts and white chocolate', 'COOK-006', 'MW006', @CookiesId, NULL, 4.50, 1.80, 80, 10, 'Each', 1, 1, GETDATE(), GETDATE()),
    
    -- Cakes
    ('Chocolate Fudge Cake', 'Rich chocolate cake with fudge frosting', 'CAKE-001', 'CF001', @CakesId, NULL, 45.00, 18.00, 25, 5, 'Whole', 1, 1, GETDATE(), GETDATE()),
    ('Red Velvet Cake', 'Classic red velvet with cream cheese frosting', 'CAKE-002', 'RV002', @CakesId, NULL, 48.00, 20.00, 20, 5, 'Whole', 1, 1, GETDATE(), GETDATE()),
    ('Vanilla Sponge Cake', 'Light and fluffy vanilla sponge cake', 'CAKE-003', 'VS003', @CakesId, NULL, 35.00, 15.00, 30, 5, 'Whole', 1, 1, GETDATE(), GETDATE()),
    ('Carrot Cake', 'Moist carrot cake with walnuts and cream cheese frosting', 'CAKE-004', 'CC004', @CakesId, NULL, 42.00, 17.00, 18, 5, 'Whole', 1, 1, GETDATE(), GETDATE()),
    ('Black Forest Cake', 'Chocolate cake layered with cherries and cream', 'CAKE-005', 'BF005', @CakesId, NULL, 50.00, 22.00, 15, 3, 'Whole', 1, 1, GETDATE(), GETDATE()),
    
    -- Pastries
    ('Croissant', 'Buttery, flaky French croissant', 'PAST-001', 'CR001', @PastriesId, NULL, 4.50, 1.50, 100, 20, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Pain au Chocolat', 'Croissant filled with rich chocolate', 'PAST-002', 'PC002', @PastriesId, NULL, 5.00, 1.80, 80, 15, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Almond Danish', 'Sweet pastry topped with almond cream', 'PAST-003', 'AD003', @PastriesId, NULL, 5.50, 2.00, 70, 15, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Cinnamon Roll', 'Soft roll swirled with cinnamon and topped with icing', 'PAST-004', 'CR004', @PastriesId, NULL, 5.25, 1.70, 90, 15, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Blueberry Muffin', 'Fresh blueberry muffin with streusel topping', 'PAST-005', 'BM005', @PastriesId, NULL, 4.75, 1.60, 100, 20, 'Each', 1, 1, GETDATE(), GETDATE()),
    
    -- Breads
    ('Sourdough Loaf', 'Artisan sourdough bread', 'BRED-001', 'SD001', @BreadsId, NULL, 8.00, 3.00, 40, 10, 'Loaf', 1, 1, GETDATE(), GETDATE()),
    ('Baguette', 'Traditional French baguette', 'BRED-002', 'BG002', @BreadsId, NULL, 5.50, 2.00, 50, 10, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Whole Wheat Bread', 'Healthy whole wheat loaf', 'BRED-003', 'WW003', @BreadsId, NULL, 7.50, 2.80, 45, 10, 'Loaf', 1, 1, GETDATE(), GETDATE()),
    ('Brioche', 'Rich and buttery brioche loaf', 'BRED-004', 'BR004', @BreadsId, NULL, 9.00, 3.50, 30, 8, 'Loaf', 1, 1, GETDATE(), GETDATE()),
    
    -- Desserts
    ('Cheesecake Slice', 'Creamy New York style cheesecake', 'DESS-001', 'CK001', @DessertsId, NULL, 7.50, 3.00, 40, 8, 'Slice', 1, 1, GETDATE(), GETDATE()),
    ('Tiramisu', 'Italian coffee-flavored dessert', 'DESS-002', 'TM002', @DessertsId, NULL, 8.50, 3.50, 30, 5, 'Serving', 1, 1, GETDATE(), GETDATE()),
    ('Apple Pie Slice', 'Classic apple pie with cinnamon', 'DESS-003', 'AP003', @DessertsId, NULL, 6.50, 2.50, 50, 10, 'Slice', 1, 1, GETDATE(), GETDATE()),
    ('Brownie', 'Fudgy chocolate brownie', 'DESS-004', 'BW004', @DessertsId, NULL, 4.50, 1.50, 80, 15, 'Each', 1, 1, GETDATE(), GETDATE()),
    ('Eclair', 'French pastry filled with cream and topped with chocolate', 'DESS-005', 'EC005', @DessertsId, NULL, 6.00, 2.20, 60, 10, 'Each', 1, 1, GETDATE(), GETDATE());
    
    PRINT 'Products seeded successfully!';
END
ELSE
BEGIN
    PRINT 'Products already exist';
END

PRINT '';
PRINT '=== Summary ===';
SELECT 
    c.Name AS Category,
    COUNT(p.Id) AS ProductCount
FROM Categories c
LEFT JOIN Products p ON c.Id = p.CategoryId
WHERE c.IsActive = 1
GROUP BY c.Name
ORDER BY c.DisplayOrder;

PRINT '';
PRINT 'Total Active Products: ' + CAST((SELECT COUNT(*) FROM Products WHERE IsActive = 1) AS VARCHAR);
PRINT 'Total Active Categories: ' + CAST((SELECT COUNT(*) FROM Categories WHERE IsActive = 1) AS VARCHAR);

GO
