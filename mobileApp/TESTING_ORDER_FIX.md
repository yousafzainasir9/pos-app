# 🧪 Quick Testing Guide - Order Placement Fix

## Prerequisites
1. ✅ Backend running on port 5021
2. ✅ Database connected
3. ✅ Mobile app rebuilt with changes
4. ✅ User logged into mobile app
5. ✅ Store selected in mobile app

---

## Test Steps

### Step 1: Place Test Order
1. Open mobile app
2. Browse products
3. Add 2-3 products to cart
4. Go to cart screen
5. Click "Proceed to Checkout"

### Step 2: Fill Checkout Form
1. **Name:** John Test
2. **Phone:** 0411222333
3. **Email:** (optional)
4. **Notes:** Test order from mobile app
5. **Payment Method:** Card
6. Click **"Place Order"**

### Step 3: Verify Success
✅ Should see: "Order Placed! 🎉"  
✅ Should see: Order Number (e.g., ORD20251012143022)  
✅ Cart should be empty  
✅ Redirected to home screen

---

## Verification Checklist

### In Mobile App Console:
```
✅ 🚀 API Request: POST /api/orders
✅ Creating order with data: {...}
✅ Order created successfully: {...}
✅ ✅ API Response: 201 /api/orders
✅ Payment processed successfully
```

### In Backend:
1. Open Visual Studio
2. Check Output window for:
```
✅ POST /api/orders - 201 Created
✅ POST /api/orders/{id}/payments - 200 OK
```

### In Database:
Run this SQL query:
```sql
-- Check latest order
SELECT TOP 1 
    Id,
    OrderNumber,
    CustomerName,
    TotalAmount,
    Status,
    OrderDate,
    CreatedAt
FROM Orders 
ORDER BY CreatedAt DESC;

-- Check order items
SELECT 
    oi.Id,
    oi.OrderId,
    p.Name as ProductName,
    oi.Quantity,
    oi.TotalAmount
FROM OrderItems oi
JOIN Products p ON oi.ProductId = p.Id
WHERE oi.OrderId = (SELECT TOP 1 Id FROM Orders ORDER BY CreatedAt DESC);

-- Check payment
SELECT TOP 1 
    Id,
    OrderId,
    Amount,
    PaymentMethod,
    Status,
    PaymentDate
FROM Payments 
ORDER BY PaymentDate DESC;
```

### Expected Database Results:
✅ New record in `Orders` table  
✅ New records in `OrderItems` table  
✅ New record in `Payments` table  
✅ Product `StockQuantity` decreased  
✅ `InventoryTransactions` records created

---

## Common Issues & Solutions

### Issue 1: "Please select a store first"
**Solution:** 
- Open hamburger menu
- Go to Store Selection
- Select a store
- Try again

### Issue 2: "User not authenticated"
**Solution:**
- Log out
- Log back in
- Try again

### Issue 3: "Failed to place order"
**Check:**
1. Is backend running?
2. Check backend console for errors
3. Check mobile app console logs
4. Verify API_BASE_URL in client.ts

### Issue 4: Order created but no payment
**This is OK for now!** 
- Order is created in database
- Payment might fail but order still saved
- You can manually process payment in POS

---

## Success Criteria

All of these must be TRUE:

- ✅ Success message appears
- ✅ Order number displayed
- ✅ Cart cleared
- ✅ Order in database
- ✅ OrderItems in database
- ✅ Payment in database
- ✅ Inventory updated
- ✅ Order visible in POS system

---

## Quick SQL Check

Run this to see if order was created:
```sql
SELECT COUNT(*) as TotalOrders FROM Orders;
SELECT COUNT(*) as OrdersLast5Min 
FROM Orders 
WHERE CreatedAt > DATEADD(minute, -5, GETDATE());
```

If `OrdersLast5Min` > 0, **it worked!** 🎉

---

## Test Different Scenarios

1. **Test with 1 item** ✓
2. **Test with 5 items** ✓
3. **Test with cash payment** ✓
4. **Test with card payment** ✓
5. **Test with special instructions** ✓
6. **Test without special instructions** ✓

---

## What to Report

If testing fails, provide:

1. **Mobile app console logs** (full output)
2. **Backend console logs** (errors/warnings)
3. **Error message shown** (screenshot)
4. **Database query results** (Orders table)
5. **Step where it failed**

---

## Need Help?

1. Check `MOBILE_ORDER_PLACEMENT_FIX.md` for detailed explanation
2. Enable verbose logging in CheckoutScreen.tsx
3. Check network tab if using browser dev tools
4. Verify backend endpoints in Swagger: http://localhost:5021/swagger

**The fix should work immediately after rebuilding the app!** 🚀
