-- =============================================
-- Script: Add new categories
-- Description: Inserts Taxes, Services, and Subscriptions categories
-- Date: 2026-02-07
-- =============================================

-- Check if categories already exist before inserting
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Taxes')
BEGIN
    INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Taxes', 'Income tax, property tax, government fees', N'🏛️', '#9B59B6', 1, NULL, GETUTCDATE(), NULL);
    PRINT 'Category "Taxes" inserted.';
END
ELSE
    PRINT 'Category "Taxes" already exists.';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Services')
BEGIN
    INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Services', 'Internet, phone, professional services', N'🔧', '#3498DB', 1, NULL, GETUTCDATE(), NULL);
    PRINT 'Category "Services" inserted.';
END
ELSE
    PRINT 'Category "Services" already exists.';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Subscriptions')
BEGIN
    INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Subscriptions', 'Streaming, software, memberships', N'📱', '#E74C3C', 1, NULL, GETUTCDATE(), NULL);
    PRINT 'Category "Subscriptions" inserted.';
END
ELSE
    PRINT 'Category "Subscriptions" already exists.';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Credit Card')
BEGIN
    INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Credit Card', 'Credit card payments and fees', N'💳', '#1ABC9C', 1, NULL, GETUTCDATE(), NULL);
    PRINT 'Category "Credit Card" inserted.';
END
ELSE
    PRINT 'Category "Credit Card" already exists.';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Nafta')
BEGIN
    INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Nafta', N'Combustible y gastos de vehículo', N'⛽', '#F39C12', 1, NULL, GETUTCDATE(), NULL);
    PRINT 'Category "Nafta" inserted.';
END
ELSE
    PRINT 'Category "Nafta" already exists.';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = 'Comida')
BEGIN
    INSERT INTO Categories (Id, Name, Description, Icon, Color, IsDefault, UserId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Comida', N'Almuerzo, delivery, snacks', N'🍕', '#E67E22', 1, NULL, GETUTCDATE(), NULL);
    PRINT 'Category "Comida" inserted.';
END
ELSE
    PRINT 'Category "Comida" already exists.';

-- Verify results
SELECT Id, Name, Description, Icon, Color FROM Categories ORDER BY Name;
