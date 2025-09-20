# Excel Data Import Templates

This directory contains Excel templates for importing data into the POS system.

## Required Excel Files

Place your Excel files in this directory with the following names:

### 1. **products.xlsx**
Columns:
- Category
- Subcategory  
- SKU
- Name
- Description
- PriceExGst
- Cost
- StockQuantity
- LowStockThreshold
- Supplier
- Barcode
- PackSize
- IsActive

### 2. **customers.xlsx**
Columns:
- FirstName
- LastName
- Email
- Phone
- Address
- City
- State
- PostalCode
- Country
- LoyaltyCardNumber
- LoyaltyPoints
- TotalPurchases
- TotalOrders
- DateOfBirth
- Notes
- IsActive

### 3. **suppliers.xlsx**
Columns:
- Name
- ContactPerson
- Email
- Phone
- Address
- City
- State
- PostalCode
- Country
- TaxNumber
- Notes
- IsActive

### 4. **users.xlsx**
Columns:
- Username
- Email
- Password
- FirstName
- LastName
- Role (Admin/Manager/Cashier)
- Pin
- Phone
- StoreName
- IsActive

### 5. **stores.xlsx**
Columns:
- Name
- Code
- Address
- City
- State
- PostalCode
- Country
- Phone
- Email
- TaxNumber
- TaxRate
- Currency
- OpeningTime
- ClosingTime
- IsActive

## How to Use

1. Create Excel files with the exact column names above
2. Place them in the `/backend/data/excel/` directory
3. Run the migrator - it will automatically import the data
4. Missing files will use default seed data

## Notes

- The seeder will check for duplicates (by email, SKU, etc.)
- Invalid rows will be skipped with warnings in the log
- Dates should be in format: YYYY-MM-DD
- Times should be in format: HH:MM (24-hour)
- Boolean fields: TRUE/FALSE or 1/0
- Leave cells empty for NULL values
