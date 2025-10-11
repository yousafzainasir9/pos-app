# Fix: Product Price Render Error

## Error
```
Render Error
Cannot read property 'toFixed' of undefined
```

## Root Cause
The mobile app was trying to access `product.price`, but the backend returns:
- `priceExGst` - Price excluding GST/tax
- `gstAmount` - GST/tax amount
- `priceIncGst` - Price including GST/tax (what customers see)

The mobile app's Product type didn't match the backend's ProductListDto structure.

## Solution Applied

### 1. Updated Product Types
**File:** `src/types/product.types.ts`

**Changed from:**
```typescript
interface Product {
  price: number;  // ❌ Doesn't exist in backend
  categoryId: number;
  // ...
}
```

**Changed to:**
```typescript
interface Product {
  priceExGst: number;    // ✅ Price before tax
  gstAmount: number;      // ✅ Tax amount
  priceIncGst: number;    // ✅ Price after tax (display this!)
  subcategoryId: number;  // ✅ Backend uses subcategories
  subcategory?: Subcategory;
  category?: Category;
  // ... all backend fields
}
```

### 2. Updated ProductCard Component
**File:** `src/components/products/ProductCard.tsx`

**Changed:**
```typescript
// OLD - Was accessing non-existent field
<Text>{formatPrice(product.price)}</Text>

// NEW - Uses correct field with fallback
const displayPrice = product.priceIncGst || 0;
<Text>{formatPrice(displayPrice)}</Text>
```

### 3. Updated Products API
**File:** `src/api/products.api.ts`

- Simplified type annotations
- Backend returns `{ success, data, message }` structure
- Access via `response.data.data`

## Backend Data Structure

### Products
```typescript
{
  id: 1,
  name: "Chocolate Chip Cookie",
  priceExGst: 3.18,      // Price before 10% GST
  gstAmount: 0.32,        // 10% GST
  priceIncGst: 3.50,      // Final price (display this)
  stockQuantity: 150,
  subcategoryId: 1,
  category: { id: 1, name: "Cookies" },
  // ...
}
```

### Categories
```typescript
{
  id: 1,
  name: "Cookies",
  displayOrder: 1,
  isActive: true,
  subcategoryCount: 3,
  productCount: 10
}
```

## Important Notes

### Price Fields
- **priceExGst**: Base price (before tax) - for accounting
- **gstAmount**: Tax amount - for accounting
- **priceIncGst**: Final price (inc. tax) - **DISPLAY THIS TO CUSTOMERS**

### Category Structure
Backend uses:
- **Categories** → **Subcategories** → **Products**
- Products belong to Subcategories
- Subcategories belong to Categories
- API returns both for convenience

### Example Data Flow
```
Backend sends:
{
  data: [
    {
      id: 1,
      name: "Cookie",
      priceIncGst: 3.50,  ← Customer sees this
      stockQuantity: 100
    }
  ]
}

Mobile app displays:
┌──────────────┐
│  🍪 Cookie   │
│   $3.50      │  ← From priceIncGst
│     [+]      │
└──────────────┘
```

## Files Modified

1. ✅ `src/types/product.types.ts` - Updated Product interface
2. ✅ `src/components/products/ProductCard.tsx` - Use priceIncGst
3. ✅ `src/api/products.api.ts` - Simplified response handling
4. ✅ `src/screens/HomeScreen.tsx` - Minor category handling improvement

## Testing

### Step 1: Reload App
```bash
# Press 'r' in Metro bundler
```

### Step 2: Verify Products Load
- ✅ No more render errors
- ✅ Prices display correctly (with $)
- ✅ All product info shows
- ✅ Can add to cart

### Step 3: Check Price Display
```
Product Card should show:
- Product name
- Description
- Price: $3.50 (from priceIncGst)
- Stock status
- Add to cart button
```

## Australian Tax Context

Cookie Barrel uses Australian GST (Goods and Services Tax):
- **GST Rate**: 10%
- **Price Display**: Must show price including GST to customers
- **Accounting**: Stores both ex-GST and inc-GST for tax reporting

Example:
- Base price: $3.18
- + GST (10%): $0.32
- = Final price: $3.50 ← Customer sees this

## Why Three Price Fields?

1. **priceExGst** - Required for tax accounting
2. **gstAmount** - Shows exact tax paid
3. **priceIncGst** - Legal requirement to show customers final price

This is standard for Australian businesses and allows:
- Accurate tax reporting to ATO (Australian Tax Office)
- Correct customer pricing
- Easy tax calculation changes

## Summary

The error was caused by a mismatch between:
- ❌ Mobile app expected: `product.price`
- ✅ Backend provides: `product.priceIncGst`

Now the mobile app correctly uses the backend's data structure, and products display with accurate Australian tax-inclusive pricing!

**Just reload the app (press 'r') and everything should work!** 🚀
