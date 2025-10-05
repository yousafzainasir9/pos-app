# System Settings - Quick Reference Card

## 📌 What is System Settings?

**System Settings** is an admin-only page for configuring system-wide POS settings.

**Access:** Admin → System Settings  
**Who Can Use:** Admin role only  
**What It Controls:** Receipt templates, email configuration, and system defaults

---

## 🎯 Three Main Sections

### 1. 📄 Receipt Template
Configure how receipts look and print

**Key Settings:**
- Header/Footer text
- Paper size (58mm, 80mm, A4)
- Font size and margins
- Show/hide: Logo, Tax, Items, Barcode, QR Code, Customer info

**Special Feature:** Live preview panel shows changes in real-time!

---

### 2. 📧 Email Settings
Configure SMTP for sending emails

**Key Settings:**
- SMTP server details (host, port, credentials)
- Sender information (from email/name)
- Enable: Email receipts, Notifications, Low stock alerts, Daily reports

**Special Feature:** Test email button to verify configuration!

---

### 3. ⚙️ Default Values
System-wide default settings

**Categories:**
- **Transaction:** Tax rate, payment method, customer requirement
- **Inventory:** Low stock threshold
- **Receipt:** Print copies, auto-print, auto-open drawer
- **Security:** Session timeout, password policies
- **POS Features:** Barcode lookup, quick sale mode

---

## 🔧 Common Actions

### Save Changes:
1. Modify settings in a tab
2. Click **Save [Tab Name] Settings** button
3. See success toast
4. Settings are saved to database

### Test Email:
1. Go to Email Settings tab
2. Configure SMTP settings
3. Enter test email address
4. Click **Send Test**
5. Check inbox

### Export Settings:
1. Click **Export** button (top right)
2. JSON file downloads automatically
3. Save for backup or migration

### Import Settings:
1. Click **Import** button (top right)
2. Select JSON file
3. Settings load into form
4. Click Save on each tab to persist

### Reset Everything:
1. Click **Reset to Defaults** button
2. Confirm in dialog
3. All settings clear
4. Default values restored

---

## 💡 Pro Tips

### Receipt Preview:
- Preview updates in **real-time** as you type
- Preview panel is **sticky** (stays visible when scrolling)
- Shows exactly how receipt will look

### Email Setup for Gmail:
```
Host: smtp.gmail.com
Port: 587
Use SSL/TLS: ✓
Password: Use "App Password" not regular password!
```

### Best Practices:
- ✅ Export settings before making big changes
- ✅ Test email before enabling features
- ✅ Use reasonable session timeout (30-60 min)
- ✅ Set appropriate low stock threshold
- ✅ Enable strong passwords for security

---

## 🚨 Important Notes

### What's NOT Here:
- ❌ Company info → Go to **Store Settings**
- ❌ Logo upload → Go to **Theme Settings**
- ❌ Colors/Theme → Go to **Theme Settings**
- ❌ Tax rate → Go to **Store Settings** (store-specific)
- ❌ Currency → Go to **Store Settings** (store-specific)

### Security:
- 🔐 Only Admins can access
- 🔐 Passwords are hidden (type="password")
- ⚠️ Email password not encrypted yet (TODO)

### Validation:
- Font size: 8-24 pixels
- Margins: 0-50mm
- Tax rate: 0-100%
- Session timeout: 5-1440 minutes
- Password length: 4-20 characters
- Receipt copies: 1-5

---

## 📊 Settings Count

- **Receipt Settings:** 17 options
- **Email Settings:** 12 options
- **Default Values:** 12 options
- **Total:** 41 configurable settings

---

## 🎨 UI Features

- ✅ Live receipt preview
- ✅ Icons and emojis for clarity
- ✅ Loading spinners during saves
- ✅ Success/error toast notifications
- ✅ Helpful hints and pro tips
- ✅ Validation with error messages
- ✅ Responsive design
- ✅ Sticky preview panel
- ✅ Info alerts for guidance

---

## 🔄 Quick Workflow

### Initial Setup:
1. Navigate to System Settings
2. Configure Receipt Template
3. Set up Email (if needed)
4. Adjust Default Values
5. Click Export to backup

### Regular Maintenance:
1. Review settings monthly
2. Test email quarterly
3. Export after major changes
4. Adjust as business needs change

### Before Going Live:
1. Configure receipt to match brand
2. Set up email for notifications
3. Set appropriate defaults
4. Test email functionality
5. Export as baseline backup

---

## ❓ Troubleshooting

### Email test fails:
- Check SMTP credentials
- Use app password for Gmail
- Verify port (587 vs 465)
- Check SSL/TLS setting

### Settings don't save:
- Click Save button!
- Check for validation errors
- Verify you're logged in as Admin
- Check browser console for errors

### Preview doesn't update:
- Check for JavaScript errors
- Try hard refresh (Ctrl+Shift+R)
- Clear browser cache

### Can't access page:
- Must be logged in as Admin
- Managers/Cashiers cannot access
- Check role in user profile

---

## 📱 Keyboard Shortcuts

- `Tab` - Navigate between fields
- `Enter` - Submit form (save)
- `Ctrl+S` - Save (if implemented)
- `Esc` - Cancel/close dialogs

---

## 🎯 Current Version

**Version:** 1.0  
**Status:** ✅ 93.6% Complete  
**Last Updated:** January 2025

**What Works:**
- All CRUD operations
- Export/Import
- Email testing
- Receipt preview
- Validation

**What's Pending:**
- Password encryption
- Audit logging
- Settings caching

---

## 📞 Need Help?

Check these docs:
- `SYSTEM_SETTINGS_IMPLEMENTATION_SUMMARY.md` - Full feature list
- `SYSTEM_SETTINGS_TESTING_GUIDE.md` - Testing checklist
- `SYSTEM_SETTINGS_STATUS.md` - Implementation status

Or contact system administrator.

---

**Quick Tip:** Always export settings before making major changes! 💾
