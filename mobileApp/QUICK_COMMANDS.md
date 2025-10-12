# 🎯 Quick Commands Reference

## 🚀 Rebuild Mobile App

### Android:
```bash
cd D:\pos-app\mobileApp
npm run android -- --reset-cache
```

### iOS (if on Mac):
```bash
cd D:\pos-app\mobileApp
npm run ios -- --reset-cache
```

### Clean Build (if issues):
```bash
cd D:\pos-app\mobileApp

# Clean Android
cd android
.\gradlew clean
cd ..

# Rebuild
npm run android -- --reset-cache
```

---

## 🗄️ Database Verification

### Check Latest Order:
```sql
SELECT TOP 1 
    Id, OrderNumber, Status, TotalAmount, CreatedAt, Notes
FROM Orders 
ORDER BY CreatedAt DESC;
```

### Check Order Items:
```sql
SELECT 
    oi.Id, oi.OrderId, p.Name as ProductName, 
    oi.Quantity, oi.UnitPriceIncGst, oi.TotalAmount
FROM OrderItems oi
JOIN Products p ON oi.ProductId = p.Id
WHERE oi.OrderId = (SELECT TOP 1 Id FROM Orders ORDER BY CreatedAt DESC);
```

### Check Payment:
```sql
SELECT TOP 1 
    Id, OrderId, Amount, PaymentMethod, Status, PaymentDate
FROM Payments 
ORDER BY PaymentDate DESC;
```

### Check Inventory:
```sql
SELECT Id, Name, SKU, StockQuantity
FROM Products
WHERE Id IN (
    SELECT ProductId FROM OrderItems 
    WHERE OrderId = (SELECT TOP 1 Id FROM Orders ORDER BY CreatedAt DESC)
);
```

### Check All Recent Orders:
```sql
SELECT TOP 10 
    Id, OrderNumber, Status, TotalAmount, 
    CONVERT(VARCHAR, CreatedAt, 120) as CreatedAt
FROM Orders 
ORDER BY CreatedAt DESC;
```

### Count Orders Today:
```sql
SELECT COUNT(*) as OrdersToday
FROM Orders
WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE);
```

---

## 🔧 Backend Commands

### Start Backend:
```bash
cd D:\pos-app\backend
.\run.bat
```

### Check Backend Status:
```bash
# Backend should be running on http://localhost:5021
# Swagger UI: http://localhost:5021/swagger
```

### View Backend Logs:
```bash
# Check Visual Studio Output window
# Look for:
# POST /api/orders - 201 Created
# POST /api/orders/{id}/payments - 200 OK
```

---

## 📱 Mobile App Debugging

### View React Native Logs:
```bash
# Android logs
adb logcat | findstr "ReactNativeJS"

# Or use React Native CLI
npx react-native log-android
```

### Clear App Cache:
```bash
cd D:\pos-app\mobileApp
npm start -- --reset-cache
```

### Clear Device Cache:
```bash
# On device/emulator:
# Settings → Apps → [Your App] → Storage → Clear Cache
```

---

## 🧪 Quick Test Commands

### 1. Verify Backend Running:
```bash
curl http://localhost:5021/api/health
# OR
curl http://localhost:5021/swagger
```

### 2. Test API Endpoint:
```bash
# Get products (with auth token)
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5021/api/products
```

### 3. Check Database Connection:
```sql
SELECT @@VERSION; -- Check SQL Server version
SELECT DB_NAME(); -- Check current database
SELECT COUNT(*) FROM Orders; -- Check orders table
```

---

## 🗂️ File Locations

### Modified Files:
```
D:\pos-app\mobileApp\src\screens\CheckoutScreen.tsx
D:\pos-app\mobileApp\src\types\order.types.ts
D:\pos-app\mobileApp\src\api\orders.api.ts
```

### Documentation Files:
```
D:\pos-app\mobileApp\MOBILE_ORDER_PLACEMENT_FIX.md
D:\pos-app\mobileApp\TESTING_ORDER_FIX.md
D:\pos-app\mobileApp\ORDER_FIX_SUMMARY.md
D:\pos-app\mobileApp\ORDER_FIX_VISUAL_GUIDE.md
D:\pos-app\mobileApp\IMPLEMENTATION_CHECKLIST.md
D:\pos-app\mobileApp\README_ORDER_FIX.md
D:\pos-app\mobileApp\QUICK_COMMANDS.md
```

---

## 🔍 Debugging Commands

### Check API Connection:
```typescript
// In CheckoutScreen.tsx, add this temporarily:
console.log('API Base URL:', apiClient.defaults.baseURL);
console.log('Order data:', JSON.stringify(orderData, null, 2));
```

### Check Auth Token:
```typescript
// Add to CheckoutScreen.tsx:
import AsyncStorage from '@react-native-async-storage/async-storage';
const token = await AsyncStorage.getItem('authToken');
console.log('Auth token:', token ? 'EXISTS' : 'MISSING');
```

### Check Store Selection:
```typescript
// Add to CheckoutScreen.tsx:
console.log('Selected Store ID:', selectedStoreId);
```

---

## 📊 Monitoring Queries

### Orders by Status:
```sql
SELECT Status, COUNT(*) as Count
FROM Orders
GROUP BY Status
ORDER BY Count DESC;
```

### Today's Sales:
```sql
SELECT 
    COUNT(*) as TotalOrders,
    SUM(TotalAmount) as TotalSales
FROM Orders
WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)
AND Status = 'Completed';
```

### Recent Failures (if any):
```sql
SELECT TOP 10 *
FROM Orders
WHERE Status = 'Cancelled'
ORDER BY CreatedAt DESC;
```

### Payment Methods:
```sql
SELECT PaymentMethod, COUNT(*) as Count
FROM Payments
GROUP BY PaymentMethod;
```

---

## 🚨 Emergency Commands

### Stop Everything:
```bash
# Stop React Native
Ctrl+C (in Metro Bundler terminal)

# Stop Backend
Ctrl+C (in Visual Studio or backend terminal)
```

### Restart Clean:
```bash
# 1. Stop everything
# 2. Close emulator
# 3. Clear caches
cd D:\pos-app\mobileApp
npm start -- --reset-cache

# 4. In new terminal:
npm run android -- --reset-cache
```

### Database Reset (CAUTION):
```sql
-- Delete test orders (CAREFUL!)
DELETE FROM OrderItems WHERE OrderId IN (
    SELECT Id FROM Orders WHERE Notes LIKE '%Test order%'
);
DELETE FROM Payments WHERE OrderId IN (
    SELECT Id FROM Orders WHERE Notes LIKE '%Test order%'
);
DELETE FROM Orders WHERE Notes LIKE '%Test order%';
```

---

## 📈 Success Verification

### Quick Success Check:
```bash
# 1. Check mobile app shows success
# 2. Run this SQL:
SELECT COUNT(*) FROM Orders WHERE CreatedAt > DATEADD(minute, -5, GETDATE());
# Should return > 0
```

### Complete Success Check:
```sql
-- Run all these:
SELECT COUNT(*) FROM Orders WHERE CreatedAt > DATEADD(minute, -5, GETDATE());
SELECT COUNT(*) FROM OrderItems WHERE OrderId IN (SELECT TOP 1 Id FROM Orders ORDER BY CreatedAt DESC);
SELECT COUNT(*) FROM Payments WHERE OrderId IN (SELECT TOP 1 Id FROM Orders ORDER BY CreatedAt DESC);
-- All should return > 0
```

---

## 🎯 Most Used Commands

### Development Workflow:
```bash
# 1. Start backend
cd D:\pos-app\backend && .\run.bat

# 2. Start mobile (new terminal)
cd D:\pos-app\mobileApp && npm start

# 3. Run on Android (new terminal)
cd D:\pos-app\mobileApp && npm run android
```

### Testing Workflow:
```bash
# 1. Place order in mobile app
# 2. Check database:
sqlcmd -S localhost -d POS_DB -Q "SELECT TOP 1 * FROM Orders ORDER BY CreatedAt DESC"

# 3. Check backend logs in Visual Studio
```

---

## 🔗 Useful URLs

- Backend API: http://localhost:5021
- Swagger UI: http://localhost:5021/swagger
- React Native Debugger: Run app and shake device → Debug

---

## 💡 Pro Tips

```bash
# Keep these terminals open during development:
# Terminal 1: Metro Bundler (npm start)
# Terminal 2: Backend (.\run.bat)
# Terminal 3: Free for commands

# Watch logs in real-time:
# Mobile: React Native Debugger
# Backend: Visual Studio Output window
# Database: SQL Server Profiler (optional)
```

---

## ✅ Pre-Test Checklist Commands

```bash
# 1. Backend running?
curl http://localhost:5021/swagger

# 2. Database connected?
sqlcmd -S localhost -Q "SELECT DB_NAME()"

# 3. Products exist?
sqlcmd -S localhost -d POS_DB -Q "SELECT COUNT(*) FROM Products"

# 4. Stores exist?
sqlcmd -S localhost -d POS_DB -Q "SELECT COUNT(*) FROM Stores"

# All should return success!
```

---

## 📚 Documentation Quick Links

| Document | Purpose |
|----------|---------|
| `ORDER_FIX_SUMMARY.md` | Quick 1-page overview |
| `TESTING_ORDER_FIX.md` | Step-by-step testing |
| `IMPLEMENTATION_CHECKLIST.md` | Complete test checklist |
| `ORDER_FIX_VISUAL_GUIDE.md` | Flow diagrams |
| `MOBILE_ORDER_PLACEMENT_FIX.md` | Technical deep dive |
| `README_ORDER_FIX.md` | Complete summary |
| `QUICK_COMMANDS.md` | This file |

---

## 🎯 Ready to Test?

```bash
# Quick Start:
cd D:\pos-app\mobileApp
npm run android -- --reset-cache

# Then place a test order!
```

**Good luck! 🚀**
