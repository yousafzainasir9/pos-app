# Data Import Directory

This directory is used by the POS Migrator to import additional data into the database.

## Supported File Types

- **JSON Files** (.json) - For structured data import
- **Excel Files** (.xlsx) - For bulk data import
- **CSV Files** (.csv) - For simple tabular data

## File Naming Convention

Name your files descriptively to help the seeder identify the content:

- `customers_*.json` or `customers.xlsx` - Customer data
- `suppliers_*.json` or `suppliers.xlsx` - Supplier data
- `products_*.json` or `products.xlsx` - Product data
- `inventory_*.xlsx` - Inventory levels
- `categories_*.json` - Category structure

## Data Structure Examples

### customers.json
```json
[
  {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phone": "+61 400 123 456",
    "address": "123 Sample St",
    "city": "Sydney",
    "state": "NSW",
    "postalCode": "2000",
    "country": "Australia",
    "isActive": true,
    "loyaltyCardNumber": "LC2000001",
    "loyaltyPoints": 100
  }
]
```

### suppliers.json
```json
[
  {
    "name": "Quality Supplies Co",
    "contactPerson": "Jane Smith",
    "email": "sales@qualitysupplies.com",
    "phone": "+61 2 1234 5678",
    "address": "456 Business Park",
    "city": "Sydney",
    "state": "NSW",
    "postalCode": "2100",
    "country": "Australia",
    "taxNumber": "ABN 12 345 678 900",
    "isActive": true
  }
]
```

### products.xlsx Structure
| SKU | Name | Category | Subcategory | Price | Stock | Low Stock |
|-----|------|----------|-------------|-------|-------|-----------|
| BRE-CRO-001 | Plain Croissant | Breads | Croissants | 4.50 | 100 | 20 |

## How to Use

1. Place your data files in this directory
2. Run the POS Migrator with refresh option
3. The seeder will automatically detect and import the data

## Notes

- The seeder will skip duplicate records based on unique identifiers (email for customers, SKU for products, etc.)
- Excel files should have headers in the first row
- JSON files should be valid JSON arrays or objects
- Large files may take time to process - check logs for progress
