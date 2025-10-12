# 🎯 Mobile Orders - Quick Reference

## 📱 For Cashiers/Staff

### **When You Arrive:**
1. Open POS system
2. Look for **yellow notification widget** at top
3. If widget shows orders → Click to view
4. If no widget → No pending mobile orders

### **Widget Shows:**
- Order count (red badge)
- Customer names
- Order amounts
- Order age ("5 mins ago")

### **When New Order Arrives:**
- 🔊 Beep sound plays
- Widget updates automatically
- Order appears in list

### **To Process Orders:**
1. Click widget → Opens Orders page
2. OR: Click "Orders" in menu
3. Look for orders with **📱 Mobile** badge
4. Orders with **NEW** badge are recent
5. Recent orders highlighted in **yellow**

### **Auto-Refresh:**
- Widget updates every 30 seconds
- Orders page updates every 30 seconds
- No manual refresh needed!

---

## 📊 Visual Indicators

### **Order Badges:**
```
[📱 Mobile]    = Mobile app order (no shift)
[🖥️ POS]      = POS system order (has shift)
[NEW]          = Order less than 10 minutes old
```

### **Status Colors:**
```
🟡 Yellow      = Recent order (< 10 mins)
🟠 Orange      = Pending status
🔵 Blue        = Processing status
🟢 Green       = Completed status
🔴 Red         = Cancelled status
```

---

## 🔄 How It Works

### **Customer Side (Mobile App):**
```
Customer → Selects items → Checkout → Place Order
                                          ↓
                                    Order Created
                                    (ShiftId = null)
```

### **Staff Side (Web App):**
```
Widget on POS → Shows "3 New Orders" → Beep! 🔊
                        ↓
         Cashier clicks widget
                        ↓
              Orders Page opens
                        ↓
         Shows all pending orders
         (Mobile badge on mobile orders)
                        ↓
         Cashier processes order
                        ↓
         Marks as "Completed"
                        ↓
         Customer gets SMS ✅
```

---

## ⚙️ Settings

### **Refresh Rate:**
- Widget: 30 seconds
- Orders page: 30 seconds

### **"NEW" Badge:**
- Shows for 10 minutes
- Then disappears automatically

### **Sound:**
- Plays when new orders detected
- Only plays once per order
- Short beep (0.5 seconds)

### **Widget Visibility:**
- **Visible:** Staff, Managers, Admins
- **Hidden:** Customers

---

## 🎯 Best Practices

### **For Cashiers:**
1. ✅ Check widget when starting shift
2. ✅ Leave POS screen open to hear alerts
3. ✅ Process oldest orders first
4. ✅ Mark orders complete when ready
5. ✅ Call customer if issues

### **For Managers:**
1. ✅ Monitor order fulfillment time
2. ✅ Check Orders page regularly
3. ✅ Train staff on workflow
4. ✅ Set prep time expectations

---

## 📞 Troubleshooting

### **Widget Not Showing:**
- ✓ Are there pending mobile orders?
- ✓ Are you logged in as staff?
- ✓ Is POS page loaded?
- ✓ Try refreshing page

### **No Sound Alert:**
- ✓ Check browser sound settings
- ✓ Check system volume
- ✓ Some browsers block auto-play
- ✓ Not critical - visual alerts work!

### **Orders Not Updating:**
- ✓ Check internet connection
- ✓ Wait 30 seconds for refresh
- ✓ Manual refresh: F5 key
- ✓ Check if backend is running

---

## 🚀 Quick Actions

### **View All Orders:**
```
Click: Orders (sidebar) → See all orders
```

### **View Mobile Orders Only:**
```
Click: Orders → Look for 📱 Mobile badge
```

### **View Recent Orders:**
```
Click: Orders → Look for yellow highlights
```

### **Process Order:**
```
Click: Order row → View Details → Mark Complete
```

---

## 📋 Training Script

**"Hi team! We've added some features to help you see mobile orders faster:"**

1. **"When you open POS, look at the top for a yellow box"**
   - Shows pending mobile orders
   - Updates automatically

2. **"You'll hear a beep when new orders come in"**
   - Just a quick beep sound
   - Means a customer placed an order

3. **"Mobile orders have a phone icon 📱"**
   - Easy to spot in the Orders page
   - Recent orders are highlighted yellow

4. **"Everything updates automatically"**
   - No need to manually refresh
   - Just keep the page open

5. **"To process orders:"**
   - Click widget → Or go to Orders page
   - Click order → View details
   - Prepare items → Mark complete

**"That's it! The system will alert you when orders come in!"**

---

## ✅ Daily Checklist

### **Morning (Opening):**
- [ ] Login to POS system
- [ ] Check for overnight mobile orders
- [ ] Process pending orders
- [ ] Start shift if needed

### **During Day:**
- [ ] Keep POS screen open
- [ ] Listen for beep alerts
- [ ] Check widget regularly
- [ ] Process orders as they arrive

### **Evening (Closing):**
- [ ] Complete all pending orders
- [ ] Verify no mobile orders left
- [ ] Close shift
- [ ] Log out

---

## 🎉 Benefits

### **For Staff:**
- ✅ Never miss an order
- ✅ See orders immediately
- ✅ Know how long customer waited
- ✅ Quick access to order details

### **For Customers:**
- ✅ Orders noticed faster
- ✅ Better service
- ✅ Shorter wait times
- ✅ Can order anytime

### **For Business:**
- ✅ More orders (24/7 ordering)
- ✅ Better efficiency
- ✅ Improved customer satisfaction
- ✅ Easy to track and manage

---

## 📞 Need Help?

**Check:**
1. This guide
2. Backend error logs
3. Browser console
4. Network connection

**Common Solutions:**
- Refresh page (F5)
- Clear browser cache
- Check if backend running
- Verify user permissions

---

## 🎯 Success Metrics

**Monitor:**
- Average order fulfillment time
- Number of mobile orders per day
- Customer feedback
- Order completion rate

**Target:**
- Orders processed within 15-20 mins
- No orders missed
- All orders completed same day
- High customer satisfaction

---

**Everything you need to know about mobile orders! 🚀**

*Questions? Check the full documentation in PHASE2_IMPLEMENTATION_COMPLETE.md*
