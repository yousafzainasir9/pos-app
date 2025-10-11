# Fix: addItem is not a function Error

## Error
```
Uncaught Error
addItem is not a function (it is undefined)
```

## Root Cause

### Issue 1: Wrong Import Name
HomeScreen was trying to import `addItem` but the cart slice exports `addToCart`:

```typescript
// cartSlice.ts exports:
export const { addToCart, removeFromCart, updateQuantity, clearCart } = cartSlice.actions;

// HomeScreen.tsx was trying to import:
import { addItem } from '../store/slices/cartSlice';  // ❌ Doesn't exist!
```

### Issue 2: Wrong Price Field
Cart slice was still using the old `product.price` field instead of `product.priceIncGst`:

```typescript
// OLD - Field doesn't exist
sum + item.product.price * item.quantity

// NEW - Correct field
sum + item.product.priceIncGst * item.quantity
```

## Solution Applied

### 1. Fixed HomeScreen Import
**File:** `src/screens/HomeScreen.tsx`

```typescript
// OLD ❌
import { addItem } from '../store/slices/cartSlice';
dispatch(addItem(product));

// NEW ✅
import { addToCart } from '../store/slices/cartSlice';
dispatch(addToCart(product));
```

### 2. Fixed Cart Calculations
**File:** `src/store/slices/cartSlice.ts`

```typescript
const calculateTotals = (items: CartItem[]) => {
  // Use priceIncGst (price with tax already included)
  const subtotal = items.reduce(
    (sum, item) => sum + (item.product.priceIncGst || 0) * item.quantity,
    0
  );
  
  // GST is already included, so calculate it backwards
  // Australian GST is 10%, so price inc GST = price ex GST * 1.1
  const gstAmount = subtotal - (subtotal / 1.1);
  const totalAmount = subtotal;
  
  return { subtotal, gstAmount, totalAmount };
};
```

## Understanding Australian GST

### Price Structure
```
Price Ex GST: $3.18
+ GST (10%):  $0.32
= Price Inc GST: $3.50  ← Customer pays this
```

### Cart Display
```
Items:
  Chocolate Chip Cookie x 2
  @ $3.50 each = $7.00

Subtotal (inc GST): $7.00
GST included:       $0.64
Total:             $7.00
```

The total equals subtotal because GST is already included in the item prices.

### Calculating GST from Price Inc GST
```javascript
// To extract GST from a price that already includes it:
priceIncGst = $3.50
priceExGst = $3.50 / 1.1 = $3.18
gstAmount = $3.50 - $3.18 = $0.32

// Or simplified:
gstAmount = priceIncGst - (priceIncGst / 1.1)
```

## Cart Slice Actions

The cart slice provides these actions:

| Action | Description | Usage |
|--------|-------------|-------|
| `addToCart(product)` | Add product or increase quantity | Product card + button |
| `removeFromCart(productId)` | Remove product from cart | Cart screen remove |
| `updateQuantity({ productId, quantity })` | Set specific quantity | Cart screen +/- |
| `clearCart()` | Empty the cart | After checkout |

## Files Modified

1. ✅ `src/screens/HomeScreen.tsx` - Fixed import from `addItem` → `addToCart`
2. ✅ `src/store/slices/cartSlice.ts` - Fixed price field and GST calculation

## Testing

### Step 1: Reload App
```bash
# App should auto-reload, or press 'r' in Metro
```

### Step 2: Test Add to Cart
1. Browse products
2. Click + button on a product
3. Should see cart badge update (number appears)
4. No error should occur

### Step 3: Check Cart Badge
- Tab bar should show cart icon
- Badge should show total quantity of items
- Example: 3 items = badge shows "3"

### Step 4: Verify Cart Screen
1. Go to Cart tab
2. Should see added products
3. Prices should be correct
4. Can increase/decrease quantities
5. Can remove items

## Example Cart State

```typescript
{
  items: [
    {
      product: {
        id: 1,
        name: "Chocolate Chip Cookie",
        priceIncGst: 3.50,
        // ...
      },
      quantity: 2
    }
  ],
  subtotal: 7.00,      // 2 x $3.50
  gstAmount: 0.64,     // GST component: $7.00 - ($7.00 / 1.1)
  totalAmount: 7.00    // Same as subtotal (GST included)
}
```

## Why This Pattern?

### Customer Perspective
- Sees: $3.50 per cookie
- Pays: $3.50 per cookie
- Simple and clear

### Business Perspective
- Knows: $3.18 base price + $0.32 GST
- Reports: Correct GST to tax office
- Complies: Australian tax law

### Technical Implementation
- Store: priceIncGst for display
- Calculate: GST backwards for reporting
- Display: Clear breakdown in cart/receipt

## Common Cart Actions Flow

### Adding Product
```
1. User clicks + on product card
2. dispatch(addToCart(product))
3. Cart checks if product exists
4. If yes: increase quantity
5. If no: add new item with qty = 1
6. Recalculate totals
7. Update cart badge
```

### Checkout Flow
```
1. User goes to Cart tab
2. Reviews items and quantities
3. Clicks "Checkout"
4. Goes to checkout screen
5. Enters payment/customer info
6. Submits order
7. Cart is cleared
```

## Summary

The error occurred because:
- ❌ Wrong function name: `addItem` instead of `addToCart`
- ❌ Wrong price field: `product.price` instead of `product.priceIncGst`

Both issues are now fixed:
- ✅ Correct import and function call
- ✅ Correct price field with proper GST calculation
- ✅ Cart works perfectly with Australian tax system

**Just reload the app and try adding products to cart!** 🛒
