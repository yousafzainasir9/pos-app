# 🚀 WhatsApp Integration - Quick Start Guide

## Get Started in 30 Minutes

### Step 1: Install ngrok (5 min)

```bash
# Download from https://ngrok.com/download
# Sign up and get auth token
ngrok config add-authtoken YOUR_TOKEN
```

### Step 2: Create WhatsApp App (10 min)

1. Go to https://developers.facebook.com/
2. Create App → Business type
3. Add WhatsApp product
4. Save these:
   - Phone Number ID
   - Access Token
   - Add your phone number for testing

### Step 3: Configure Backend (5 min)

Edit `backend/src/POS.WebAPI/appsettings.Development.json`:

```json
{
  "WhatsApp": {
    "Enabled": true,
    "AccessToken": "YOUR_ACCESS_TOKEN",
    "PhoneNumberId": "YOUR_PHONE_NUMBER_ID",
    "WebhookVerifyToken": "cookie_barrel_webhook_secret_2025",
    "ApiVersion": "v18.0",
    "DefaultStoreId": "YOUR_STORE_ID"
  }
}
```

Get your Store ID:
```sql
SELECT TOP 1 Id FROM Stores WHERE IsActive = 1;
```

### Step 4: Start Services (5 min)

**Terminal 1 - API:**
```bash
cd D:\pos-app\backend\src\POS.WebAPI
dotnet run
```

**Terminal 2 - ngrok:**
```bash
ngrok http 5124
```

Copy the HTTPS URL: `https://abc123.ngrok-free.app`

### Step 5: Configure Webhook (5 min)

1. Meta Dashboard → WhatsApp → Configuration
2. Edit Webhook:
   - URL: `https://YOUR-NGROK-URL.ngrok-free.app/api/whatsappwebhook`
   - Token: `cookie_barrel_webhook_secret_2025`
3. Verify and Save
4. Subscribe to "messages"

### Step 6: Test! 🎉

Send to WhatsApp test number:
```
Hi
```

Expected response:
```
👋 Welcome to Cookie Barrel!
🍪 Cookie Barrel Menu
[Menu appears]
```

## Full Order Flow

```
You:  hi
Bot:  Welcome! [shows menu]

You:  1, 2
Bot:  ✅ Added 2x Chocolate Chip Cookie

You:  done
Bot:  What's your name?

You:  John
Bot:  Thanks John! Your address?

You:  123 Main St
Bot:  Any special instructions?

You:  skip
Bot:  [Order summary]

You:  confirm
Bot:  ✅ Order Confirmed! #WA20251018...
```

## Troubleshooting

**Webhook fails?**
- Check ngrok URL is HTTPS
- Verify token matches exactly

**No response?**
- Check API logs
- Verify access token
- Phone number verified?

**Order not saving?**
- Check DefaultStoreId set
- Database connection working?

## Testing Endpoints

```bash
# Check config
curl http://localhost:5124/api/whatsapptest/config

# Send test
curl -X POST http://localhost:5124/api/whatsapptest/send-text \
  -H "Content-Type: application/json" \
  -d '{"to":"92XXX","message":"Test"}'

# View sessions
curl http://localhost:5124/api/whatsapptest/sessions
```

## Next Steps

1. ✅ Test complete order flow
2. ✅ Check database for orders
3. ✅ Test error handling
4. 📚 Read full guide: `WHATSAPP_INTEGRATION_COMPLETE_GUIDE.md`

---

**Need Help?**  
See troubleshooting section in complete guide.

**Ready for Production?**  
Follow Day 5 in complete guide for production setup.
