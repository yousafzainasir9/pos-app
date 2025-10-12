# 🔧 Backend 500 Error - User StoreId Issue

## ❌ The Error

```
500 Internal Server Error
"An error occurred while creating the order"
```

## 🔍 Root Cause

Looking at the backend code (line 291-294):

```csharp
var currentUser = await _unitOfWork.Repository<User>().GetByIdAsync(currentUserId);
if (currentUser == null || currentUser.StoreId == null)
{
    throw new InvalidOperationException("User not associated with a store");
}
```

**The Problem:**
- Backend expects the logged-in USER to have a `StoreId` in their profile
- Mobile app users (customers) don't have a `StoreId` - they select it in the app
- Backend is throwing an exception because `currentUser.StoreId == null`

## 🎯 The Issue

The backend was designed for **POS cashiers** who belong to one store.  
But **mobile app customers** don't belong to a store - they choose which store to order from!

---

## ✅ Solution Options

### Option 1: Send StoreId in Order Data (RECOMMENDED)
The mobile app already sends `customerId` but the backend ignores it and uses `currentUser.StoreId` instead.

**Change needed in backend `OrdersController.cs`:**

```csharp
// BEFORE (line 291-299):
var currentUser = await _unitOfWork.Repository<User>().GetByIdAsync(currentUserId);
if (currentUser == null || currentUser.StoreId == null)
{
    throw new InvalidOperationException("User not associated with a store");
}
// ...
StoreId = currentUser.StoreId.Value,

// AFTER - Use store from DTO if user doesn't have one:
var currentUser = await _unitOfWork.Repository<User>().GetByIdAsync(currentUserId);
if (currentUser == null)
{
    throw new InvalidOperationException("User not found");
}

// For mobile app users, accept storeId from request
long storeId;
if (currentUser.StoreId.HasValue)
{
    // POS user - use their assigned store
    storeId = currentUser.StoreId.Value;
}
else
{
    // Mobile app user - they must provide storeId
    if (!createOrderDto.StoreId.HasValue)
    {
        throw new InvalidOperationException("Store ID is required for mobile orders");
    }
    storeId = createOrderDto.StoreId.Value;
}

// Later in code:
StoreId = storeId,
```

### Option 2: Add StoreId to CreateOrderDto

Add `StoreId` field to the DTO:

```csharp
public class CreateOrderDto
{
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public long? StoreId { get; set; }  // ← ADD THIS
    public string? TableNumber { get; set; }
    public long? CustomerId { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public decimal? DiscountAmount { get; set; }
}
```

Then update mobile app to send it:

```typescript
const orderData: CreateOrderDto = {
  orderType: OrderTypeEnum.TakeAway,
  storeId: selectedStoreId,  // ← ADD THIS
  customerId: user?.id || undefined,
  notes: notes.trim() || 'Customer: ' + customerName + ' | Phone: ' + customerPhone,
  items: [...],
  discountAmount: 0,
};
```

---

## 🚀 Quick Check

**Please check the backend Visual Studio Output:**

Look for the actual exception message. It should say something like:
```
User not associated with a store
```

Or it might be a different error. Please share the full error from Visual Studio!

---

## 📝 Action Required

1. **Open Visual Studio**
2. **Check Output window** (View → Output)
3. **Look for the exception details**
4. **Share the full error message**

This will tell us exactly what's failing on the backend.

---

## 🎯 Most Likely Fix Needed

We need to modify the backend to:
1. Accept `StoreId` in the `CreateOrderDto`
2. Use that StoreId for mobile app orders
3. Only require `currentUser.StoreId` for POS orders

Would you like me to prepare the backend fix once we confirm the error?
