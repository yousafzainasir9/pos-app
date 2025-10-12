# 🔧 Backend Fix - Accept StoreId from Mobile Orders

## 📝 Files to Modify

### 1. **POS.Application/DTOs/OrderDtos.cs**

Add `StoreId` property to `CreateOrderDto`:

```csharp
public class CreateOrderDto
{
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public long? StoreId { get; set; }  // ← ADD THIS LINE
    public string? TableNumber { get; set; }
    public long? CustomerId { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public decimal? DiscountAmount { get; set; }
}
```

### 2. **POS.WebAPI/Controllers/OrdersController.cs**

Replace the `CreateOrder` method (starting around line 283):

```csharp
[HttpPost]
public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();

        // Get current user
        var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
        var currentUser = await _unitOfWork.Repository<User>().GetByIdAsync(currentUserId);
        if (currentUser == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Determine store ID
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
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest("Store ID is required for mobile orders");
            }
            storeId = createOrderDto.StoreId.Value;
            
            // Validate that the store exists and is active
            var store = await _unitOfWork.Repository<Store>().GetByIdAsync(storeId);
            if (store == null || !store.IsActive)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest("Invalid or inactive store");
            }
        }

        // Get active shift for the user (optional for mobile orders)
        var activeShift = await _unitOfWork.Repository<Shift>().Query()
            .Where(s => s.UserId == currentUserId && s.Status == ShiftStatus.Open)
            .FirstOrDefaultAsync();

        // Generate order number
        var orderNumber = $"ORD{DateTime.Now:yyyyMMddHHmmss}";

        // Create order
        var order = new Order
        {
            OrderNumber = orderNumber,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderType = createOrderDto.OrderType,
            TableNumber = createOrderDto.TableNumber,
            CustomerId = createOrderDto.CustomerId,
            Notes = createOrderDto.Notes,
            UserId = currentUserId,
            StoreId = storeId,  // ← Use the determined storeId
            ShiftId = activeShift?.Id,
            DiscountAmount = createOrderDto.DiscountAmount ?? 0
        };

        decimal subTotal = 0;
        decimal taxTotal = 0;

        // Add order items
        foreach (var itemDto in createOrderDto.Items)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(itemDto.ProductId);
            if (product == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest($"Product with ID {itemDto.ProductId} not found");
            }

            // Check stock
            if (product.TrackInventory && product.StockQuantity < itemDto.Quantity)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return BadRequest($"Insufficient stock for product {product.Name}");
            }

            var orderItem = new OrderItem
            {
                Order = order,
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPriceExGst = product.PriceExGst,
                UnitGstAmount = product.GstAmount,
                UnitPriceIncGst = product.PriceIncGst,
                DiscountAmount = itemDto.DiscountAmount ?? 0,
                Notes = itemDto.Notes
            };

            // Calculate totals
            var itemSubTotal = orderItem.Quantity * orderItem.UnitPriceExGst;
            var itemTax = orderItem.Quantity * orderItem.UnitGstAmount;
            var itemTotal = (itemSubTotal + itemTax) - orderItem.DiscountAmount;

            orderItem.SubTotal = itemSubTotal;
            orderItem.TaxAmount = itemTax;
            orderItem.TotalAmount = itemTotal;

            subTotal += itemSubTotal;
            taxTotal += itemTax;

            order.OrderItems.Add(orderItem);

            // Update inventory
            if (product.TrackInventory)
            {
                product.StockQuantity -= itemDto.Quantity;
                _unitOfWork.Repository<Product>().Update(product);

                // Create inventory transaction
                var inventoryTransaction = new InventoryTransaction
                {
                    ProductId = product.Id,
                    StoreId = order.StoreId,
                    TransactionType = InventoryTransactionType.Sale,
                    Quantity = -itemDto.Quantity,
                    StockBefore = product.StockQuantity + itemDto.Quantity,
                    StockAfter = product.StockQuantity,
                    TransactionDate = DateTime.Now,
                    UserId = currentUserId,
                    Order = order,
                    Notes = $"Sale - Order #{orderNumber}"
                };

                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(inventoryTransaction);
            }
        }

        // Calculate order totals
        order.SubTotal = subTotal;
        order.TaxAmount = taxTotal;
        order.TotalAmount = subTotal + taxTotal - order.DiscountAmount;

        await _unitOfWork.Repository<Order>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new { orderId = order.Id, orderNumber = order.OrderNumber });
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        _logger.LogError(ex, "Error creating order");
        return StatusCode(500, "An error occurred while creating the order");
    }
}
```

---

## 🎯 What This Does

### For POS Users (Cashiers):
- ✅ Uses their assigned `StoreId` from user profile
- ✅ Works exactly as before

### For Mobile App Users (Customers):
- ✅ Accepts `StoreId` from the order request
- ✅ Validates that store exists and is active
- ✅ Creates order for the selected store
- ✅ No need to assign StoreId to customer accounts

---

## 📊 Flow Comparison

### Before (Broken):
```
Mobile App → Sends order WITHOUT storeId
Backend → Looks for user.StoreId
Backend → Throws error: "User not associated with a store"
```

### After (Fixed):
```
Mobile App → Sends order WITH storeId
Backend → Checks if user has StoreId?
         → YES (POS user): Use user.StoreId
         → NO (Mobile user): Use DTO.StoreId
Backend → Creates order successfully ✅
```

---

## 🚀 Apply These Changes

1. **Open:** `D:\pos-app\backend\src\POS.Application\DTOs\OrderDtos.cs`
2. **Add:** `public long? StoreId { get; set; }` to `CreateOrderDto`

3. **Open:** `D:\pos-app\backend\src\POS.WebAPI\Controllers\OrdersController.cs`
4. **Replace:** The entire `CreateOrder` method with the code above

5. **Rebuild backend** in Visual Studio

6. **Test** from mobile app!

---

## ✅ Result

Mobile app orders will now work because:
- ✅ Mobile app sends `storeId` in the request
- ✅ Backend accepts it and uses it
- ✅ No need to modify user accounts
- ✅ POS orders still work as before

---

**Apply these backend changes and the mobile orders will work!** 🎉
