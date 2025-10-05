# System Settings Integration Plan

## Current Status: Settings Are Saved But Not Used

### ❌ **Problem:**
System Settings are stored in the database but don't affect application behavior.

### ✅ **Solution:**
Integrate settings into the relevant features.

---

## Integration Checklist

### 1. Receipt Settings Integration

**Where:** `PrintReceipt.tsx`, `OrdersPage.tsx`

**Changes Needed:**
- [ ] Load receipt settings from API
- [ ] Apply header/footer text
- [ ] Respect show/hide options (logo, tax, barcode, QR, customer)
- [ ] Apply paper size and font size
- [ ] Apply margins
- [ ] Use selected template

**Impact:** Receipts will look exactly as configured in System Settings

---

### 2. Email Settings Integration

**Where:** Email sending functionality (needs to be created)

**Changes Needed:**
- [ ] Create email service that uses SMTP settings
- [ ] Send email receipts when enabled
- [ ] Send low stock alerts when enabled
- [ ] Send daily sales reports when enabled
- [ ] Use configured from email/name

**Impact:** Email features will work using configured SMTP settings

---

### 3. Default Values Integration

#### A. Tax Rate
**Where:** `POSPage.tsx`, product creation

**Changes Needed:**
- [ ] Load default tax rate from settings
- [ ] Apply to new products
- [ ] Use in POS calculations

**Impact:** New products will have correct default tax

#### B. Payment Method
**Where:** `POSPage.tsx`, checkout

**Changes Needed:**
- [ ] Load default payment method
- [ ] Pre-select in payment modal

**Impact:** Faster checkout with pre-selected payment method

#### C. Low Stock Threshold
**Where:** Product list, inventory management

**Changes Needed:**
- [ ] Load threshold from settings
- [ ] Show warnings when stock below threshold
- [ ] Trigger email alerts if enabled

**Impact:** Low stock warnings work correctly

#### D. Receipt Copies & Auto-Print
**Where:** Order completion, `PrintReceipt.tsx`

**Changes Needed:**
- [ ] Load print settings
- [ ] Auto-print if enabled
- [ ] Print multiple copies if configured

**Impact:** Receipts print automatically with correct number of copies

#### E. Session Timeout
**Where:** `AuthContext.tsx`

**Changes Needed:**
- [ ] Load timeout from settings
- [ ] Implement idle timeout
- [ ] Auto-logout after configured time

**Impact:** Users auto-logout after inactivity

#### F. Password Policies
**Where:** User registration/password change

**Changes Needed:**
- [ ] Load password settings
- [ ] Enforce minimum length
- [ ] Enforce strong password if required

**Impact:** Password security enforced

#### G. POS Features
**Where:** `POSPage.tsx`

**Changes Needed:**
- [ ] Enable/disable barcode scanner based on setting
- [ ] Enable/disable quick sale based on setting

**Impact:** POS features toggle on/off

---

## Implementation Priority

### Phase 1: High Priority (User-Visible)
1. ✅ Receipt settings → PrintReceipt integration
2. ✅ Default payment method → POS integration
3. ✅ Auto-print receipt → Order completion integration

### Phase 2: Medium Priority (Important)
4. ✅ Tax rate → Product/POS integration
5. ✅ Low stock threshold → Inventory integration
6. ✅ Session timeout → Auth integration

### Phase 3: Low Priority (Nice to Have)
7. Email settings → Email service creation
8. Password policies → User management integration
9. POS feature toggles → POS page integration

---

## How To Use After Integration

Once integrated, changing settings will:

### Receipt Template
- Change header → New header appears on all receipts
- Disable tax display → Tax line disappears from receipts
- Change font size → Receipt text gets bigger/smaller
- Enable QR code → QR code appears on receipts

### Email Settings
- Configure SMTP → Email receipts can be sent
- Enable low stock alerts → Get emails when stock low
- Enable daily reports → Get daily sales summary

### Default Values
- Change tax rate → New products have new rate
- Change payment method → POS pre-selects that method
- Enable auto-print → Receipts print automatically
- Change session timeout → Users logout after configured time

---

## Testing After Integration

1. **Receipt Settings:**
   - Change header text → Print receipt → Verify new header
   - Disable tax display → Print receipt → Verify no tax line
   - Change font size → Print receipt → Verify size change

2. **Default Values:**
   - Change tax rate → Create product → Verify correct tax
   - Change payment → Open POS → Verify pre-selected
   - Enable auto-print → Complete order → Verify auto-print

3. **Session Timeout:**
   - Set to 5 minutes → Wait 5 minutes → Verify auto-logout

---

## Status

**Current:** Settings saved but not used ❌  
**After Integration:** Settings control app behavior ✅

**Estimated Work:** 
- Phase 1: 4-6 hours
- Phase 2: 3-4 hours  
- Phase 3: 6-8 hours
- **Total:** 13-18 hours of development

---

## Would You Like Me To Implement This?

I can integrate the settings into the application so they actually work!

**Start with Phase 1?** (Receipt settings + Default payment + Auto-print)
