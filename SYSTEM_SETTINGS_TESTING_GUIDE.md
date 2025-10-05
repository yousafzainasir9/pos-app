# System Settings - Quick Testing Guide

## 🧪 How to Test System Settings

### Step 1: Access System Settings
1. Login as **Admin**
2. Go to **Admin** page
3. Click on **System Settings** card
4. You should see 3 tabs: Receipt Template, Email Settings, Default Values

---

### Step 2: Test Receipt Template

#### Receipt Content:
- [ ] Change "Header Text" to "Welcome to Cookie Barrel!"
- [ ] Change "Footer Text" to "Have a great day!"
- [ ] **Preview should update immediately** ← Key test!

#### Print Format:
- [ ] Change paper size to **58mm**
- [ ] Change font size to **16**
- [ ] Adjust margins (try 10, 15, 8, 8)
- [ ] **Preview should reflect changes**

#### Display Options:
- [ ] Uncheck "Show Logo" - logo disappears from preview
- [ ] Uncheck "Show Tax Details" - tax line disappears
- [ ] Check "Show Barcode" - barcode appears
- [ ] Check "Show QR Code" - QR code appears
- [ ] Uncheck "Show Customer Info" - customer line disappears

#### Save & Verify:
- [ ] Click **Save Receipt Settings**
- [ ] Should see success toast
- [ ] Refresh page
- [ ] Settings should persist

---

### Step 3: Test Email Settings

#### SMTP Configuration:
For Gmail testing:
```
SMTP Host: smtp.gmail.com
SMTP Port: 587
SMTP Username: your-email@gmail.com
SMTP Password: your-app-password (not regular password!)
Use SSL/TLS: ✓ Checked
From Email: your-email@gmail.com
From Name: Cookie Barrel POS
```

**Note:** For Gmail, you need to create an "App Password":
1. Go to Google Account → Security
2. Enable 2-Step Verification
3. Generate App Password
4. Use that 16-character password

#### Test Email:
- [ ] Configure SMTP settings (use real credentials)
- [ ] Enter your email in test field
- [ ] Click **Send Test** button
- [ ] Should see "Test email sent successfully"
- [ ] Check your inbox for test email

#### Email Features:
- [ ] Check "Enable Email Receipts"
- [ ] Check "Enable Email Notifications"
- [ ] Check "Low Stock Alerts"
- [ ] Check "Daily Sales Report"
- [ ] Click **Save Email Settings**
- [ ] Should see success toast

---

### Step 4: Test Default Values

#### Transaction Defaults:
- [ ] Change Tax Rate to **15.0**
- [ ] Change Payment Method to **Card**
- [ ] Check "Require customer for all orders"

#### Inventory:
- [ ] Change Low Stock Threshold to **20**

#### Receipt & Printing:
- [ ] Change Receipt Print Copies to **2**
- [ ] Uncheck "Auto print receipt"
- [ ] Uncheck "Auto open cash drawer"

#### Security:
- [ ] Change Session Timeout to **30** minutes
- [ ] Change Password Min Length to **8**
- [ ] Check "Require strong passwords"

#### POS Features:
- [ ] Uncheck "Enable barcode scanner lookup"
- [ ] Uncheck "Enable quick sale mode"

#### Save:
- [ ] Click **Save Default Values**
- [ ] Should see success toast

---

### Step 5: Test Export/Import

#### Export:
- [ ] Click **Export** button (top right)
- [ ] Should download JSON file
- [ ] File name like: `system-settings-2025-01-XX.json`
- [ ] Open file and verify:
  - Receipt settings present
  - Email settings present (password should be `***ENCRYPTED***`)
  - Default values present
  - Has `exportedAt` timestamp

#### Import:
- [ ] Make some changes to settings
- [ ] Click **Import** button
- [ ] Select the exported JSON file
- [ ] Should see "Settings imported successfully"
- [ ] Settings should revert to exported values
- [ ] Click Save on each tab to persist

---

### Step 6: Test Reset to Defaults

- [ ] Change several settings across all tabs
- [ ] Click **Reset to Defaults** button
- [ ] Should see confirmation dialog
- [ ] Click OK
- [ ] Should see "Settings reset to defaults"
- [ ] All settings should return to default values:
  - Header: "Thank you for your purchase!"
  - Footer: "Please visit us again"
  - Paper Size: 80mm
  - Font Size: 12
  - Tax Rate: 10%
  - Session Timeout: 60 min
  - etc.

---

### Step 7: Test Validation

#### Receipt Tab:
- [ ] Try setting font size to **100** (should fail - max is 24)
- [ ] Try setting negative margins (should fail)

#### Email Tab:
- [ ] Clear SMTP Host
- [ ] Check "Enable Email Receipts"
- [ ] Try to save - should show error about required fields

#### Defaults Tab:
- [ ] Try setting password min length to **30** (should fail - max is 20)
- [ ] Try setting tax rate to **150** (should fail - max is 100)

---

### Step 8: Test UI/UX

- [ ] Receipt preview is sticky (stays visible when scrolling)
- [ ] All icons display correctly
- [ ] Loading spinners show during save operations
- [ ] Info alert at top explains removed General Settings
- [ ] Pro tips are helpful and readable
- [ ] Form fields are logically grouped
- [ ] Checkboxes have clear labels
- [ ] Buttons are appropriately sized

---

## 🎯 Expected Results

### ✅ Success Criteria:
- All settings save correctly
- Settings persist after page refresh
- Receipt preview updates in real-time
- Email test sends successfully
- Export/Import works without errors
- Validation prevents invalid data
- Reset returns everything to defaults
- UI is clean and intuitive

### ❌ Known Limitations:
- Email password not yet encrypted (TODO)
- No audit log yet
- No settings history/versioning
- Export doesn't actually save to backend

---

## 🐛 Common Issues & Solutions

### Issue: Email test fails
**Solution:** 
- Check SMTP credentials
- For Gmail, use App Password, not regular password
- Verify port (587 for TLS, 465 for SSL)
- Check firewall settings

### Issue: Settings don't persist
**Solution:**
- Make sure you click Save button
- Check browser console for errors
- Verify backend is running
- Check database connection

### Issue: Preview doesn't update
**Solution:**
- Check browser console for errors
- Verify React state is updating
- Try hard refresh (Ctrl+Shift+R)

### Issue: Import fails
**Solution:**
- Verify JSON file format is correct
- Check for syntax errors in JSON
- Make sure file has correct structure

---

## 📊 Test Data Examples

### Sample Receipt Settings:
```json
{
  "headerText": "Thank you for shopping!",
  "footerText": "Visit us again soon!",
  "paperSize": "80mm",
  "fontSize": 12,
  "showLogo": true,
  "showBarcode": true,
  "printMarginTop": 5
}
```

### Sample Email Settings:
```json
{
  "smtpHost": "smtp.gmail.com",
  "smtpPort": 587,
  "fromEmail": "store@example.com",
  "enableEmailReceipts": true
}
```

### Sample Default Values:
```json
{
  "defaultTaxRate": 10.0,
  "defaultPaymentMethod": "Cash",
  "sessionTimeoutMinutes": 60,
  "enableQuickSale": true
}
```

---

## ✅ Testing Checklist Summary

- [ ] Page loads without errors
- [ ] All 3 tabs accessible
- [ ] Receipt preview works
- [ ] Email test sends
- [ ] All save buttons work
- [ ] Export downloads file
- [ ] Import loads settings
- [ ] Reset clears everything
- [ ] Validation works
- [ ] Settings persist
- [ ] No console errors
- [ ] UI is responsive
- [ ] Icons display correctly
- [ ] Loading states show

**Happy Testing! 🧪**
