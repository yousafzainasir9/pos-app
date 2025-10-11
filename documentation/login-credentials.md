# Login Credentials Reference

## Default Users After Database Migration

### 📱 Mobile App (Customers)

| Username | Password | Email | Role | Description |
|----------|----------|-------|------|-------------|
| **customer** | Customer123! | customer@test.com | Customer | Test customer account for mobile app |

**Additional Customer Accounts:**
- 9 more customer accounts are created from existing customer data
- Username format: email prefix (e.g., john.smith@example.com → johnsmith)
- Default password for all: Customer123!
- Customers can order from any store (StoreId is null)

---

## Default Staff/Admin Users After Database Migration

### Username/Password Login

| Role | Username | Password | Store | Description |
|------|----------|----------|-------|-------------|
| **Admin** | admin | Admin123! | Main | Full system access |
| **Manager** | manager1 | Manager123! | Main | Store 1 manager |
| **Manager** | manager2 | Manager123! | Westfield | Store 2 manager |
| **Manager** | manager3 | Manager123! | Airport | Store 3 manager |
| **Cashier** | cashier2 | Cashier123! | Main | Store 1 cashier |
| **Cashier** | cashier3 | Cashier123! | Main | Store 1 cashier |
| **Cashier** | cashier4 | Cashier123! | Main | Store 1 cashier |
| **Cashier** | cashier5 | Cashier123! | Westfield | Store 2 cashier |
| **Cashier** | cashier6 | Cashier123! | Westfield | Store 2 cashier |
| **Cashier** | cashier7 | Cashier123! | Westfield | Store 2 cashier |
| **Cashier** | cashier8 | Cashier123! | Airport | Store 3 cashier |
| **Cashier** | cashier9 | Cashier123! | Airport | Store 3 cashier |
| **Cashier** | cashier10 | Cashier123! | Airport | Store 3 cashier |

### PIN Login

**Important**: PIN login requires selecting the correct store!

#### Store 1 - Cookie Barrel Main (Store ID: 1)
| Role | PIN | Name |
|------|-----|------|
| Admin | 9999 | System Administrator |
| Manager | 1001 | Manager 1 |
| Cashier | 2002 | Cashier 2 |
| Cashier | 2003 | Cashier 3 |
| Cashier | 2004 | Cashier 4 |

#### Store 2 - Cookie Barrel Westfield (Store ID: 2)
| Role | PIN | Name |
|------|-----|------|
| Admin | 9999 | System Administrator |
| Manager | 1002 | Manager 2 |
| Cashier | 2005 | Cashier 5 |
| Cashier | 2006 | Cashier 6 |
| Cashier | 2007 | Cashier 7 |

#### Store 3 - Cookie Barrel Airport (Store ID: 3)
| Role | PIN | Name |
|------|-----|------|
| Admin | 9999 | System Administrator |
| Manager | 1003 | Manager 3 |
| Cashier | 2008 | Cashier 8 |
| Cashier | 2009 | Cashier 9 |
| Cashier | 2010 | Cashier 10 |

## PIN Format Logic

PINs are generated based on the following pattern:
- **Admin**: `9999` (fixed, works at all stores)
- **Managers**: `1001`, `1002`, `1003`, ... (1000 + user counter)
- **Cashiers**: `2001`, `2002`, `2003`, ... (2000 + user counter)

## Quick Test Credentials

For quick testing, use:
- **Admin Login**: `admin` / `Admin123!` or PIN `9999`
- **Cashier Login**: `cashier2` / `Cashier123!` or PIN `2002` (Store 1)

## Troubleshooting Login Issues

### PIN Login Not Working?
1. **Check Store Selection**: Make sure you've selected the correct store in the dropdown
2. **Verify PIN**: Double-check the PIN matches the user's assigned store
3. **User Active**: Ensure the user account is active in the database

### Username/Password Login Not Working?
1. **Case Sensitive**: Usernames are case-sensitive
2. **Correct Password**: Ensure you're using the exact password with correct capitalization
3. **Active Account**: Verify the account is marked as active

### Backend Issues
If login fails with network errors:
1. Ensure the backend API is running (`dotnet run` in `POS.WebAPI`)
2. Check the API is running on `https://localhost:5001` or `http://localhost:5000`
3. Verify the database connection in `appsettings.Development.json`
4. Check backend logs for authentication errors

## Security Notes

These are **development credentials only**. In production:
- All default passwords should be changed
- PINs should be personalized
- Implement password complexity requirements
- Enable two-factor authentication
- Use secure password storage
- Implement account lockout policies
