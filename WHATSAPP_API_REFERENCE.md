# 📡 WhatsApp Integration - API Reference

## Webhook Endpoints

### GET /api/whatsappwebhook
**Purpose:** Webhook verification by Meta  
**Auth:** None (public endpoint)

**Query Parameters:**
- `hub.mode` - Should be "subscribe"
- `hub.verify_token` - Must match configured token
- `hub.challenge` - Challenge string to return

**Response:**
```
200 OK
Returns the challenge string if verification succeeds
403 Forbidden if verification fails
```

### POST /api/whatsappwebhook
**Purpose:** Receive incoming WhatsApp messages  
**Auth:** None (validated by Meta signature)

**Request Body:**
```json
{
  "object": "whatsapp_business_account",
  "entry": [{
    "changes": [{
      "field": "messages",
      "value": {
        "messages": [{
          "from": "923001234567",
          "id": "wamid.xxx",
          "type": "text",
          "text": {
            "body": "hi"
          }
        }]
      }
    }]
  }]
}
```

**Response:**
```
200 OK (always, even on error to prevent Meta retries)
```

### GET /api/whatsappwebhook/health
**Purpose:** Health check for WhatsApp integration  
**Auth:** None

**Response:**
```json
{
  "enabled": true,
  "configured": true,
  "apiVersion": "v18.0"
}
```

---

## Test Endpoints (Development Only)

### GET /api/whatsapptest/config
**Purpose:** Get configuration status  
**Auth:** None (development only)

**Response:**
```json
{
  "enabled": true,
  "hasAccessToken": true,
  "hasPhoneNumberId": true,
  "webhookVerifyToken": "cookie_barrel_webhook_secret_2025",
  "apiVersion": "v18.0",
  "defaultStoreId": "guid-here",
  "sessionTimeoutHours": 1
}
```

### POST /api/whatsapptest/send-text
**Purpose:** Send a test text message  
**Auth:** None (development only)

**Request:**
```json
{
  "to": "923001234567",
  "message": "Test message"
}
```

**Response:**
```json
{
  "success": true,
  "to": "923001234567",
  "message": "Test message"
}
```

### POST /api/whatsapptest/send-menu
**Purpose:** Send menu to a phone number  
**Auth:** None (development only)

**Request:**
```json
{
  "to": "923001234567"
}
```

**Response:**
```json
{
  "success": true,
  "to": "923001234567"
}
```

### GET /api/whatsapptest/sessions
**Purpose:** Get all active customer sessions  
**Auth:** None (development only)

**Response:**
```json
[
  {
    "phoneNumber": "923001234567",
    "customerName": "John Doe",
    "state": "AWAITING_ORDER",
    "cart": [
      {
        "productId": "guid",
        "name": "Chocolate Chip Cookie",
        "price": 50.00,
        "quantity": 2
      }
    ],
    "deliveryAddress": "",
    "specialInstructions": "",
    "lastActivity": "2025-10-18T10:30:00Z",
    "createdAt": "2025-10-18T10:00:00Z",
    "storeId": "guid",
    "orderNumber": null
  }
]
```

### GET /api/whatsapptest/sessions/{phoneNumber}
**Purpose:** Get specific customer session  
**Auth:** None (development only)

**Response:**
```json
{
  "phoneNumber": "923001234567",
  "customerName": "John Doe",
  "state": "AWAITING_ADDRESS",
  "cart": [...],
  "lastActivity": "2025-10-18T10:30:00Z"
}
```

### DELETE /api/whatsapptest/sessions/{phoneNumber}
**Purpose:** Clear a customer session  
**Auth:** None (development only)

**Response:**
```json
{
  "message": "Session cleared",
  "phoneNumber": "923001234567"
}
```

### POST /api/whatsapptest/sessions/clear-expired
**Purpose:** Clear all expired sessions  
**Auth:** None (development only)

**Response:**
```json
{
  "message": "Expired sessions cleared"
}
```

### POST /api/whatsapptest/simulate-message
**Purpose:** Simulate incoming message without WhatsApp  
**Auth:** None (development only)

**Request:**
```json
{
  "from": "923001234567",
  "message": "hi"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Message processed",
  "from": "923001234567",
  "text": "hi"
}
```

---

## WhatsApp Business API Endpoints

These are external Meta APIs called by the backend:

### Send Message
**Endpoint:** `https://graph.facebook.com/v18.0/{phone-number-id}/messages`  
**Method:** POST  
**Headers:**
- `Authorization: Bearer {access-token}`
- `Content-Type: application/json`

**Request Body (Text Message):**
```json
{
  "messaging_product": "whatsapp",
  "recipient_type": "individual",
  "to": "923001234567",
  "type": "text",
  "text": {
    "body": "Hello from Cookie Barrel!"
  }
}
```

**Request Body (Interactive Buttons):**
```json
{
  "messaging_product": "whatsapp",
  "to": "923001234567",
  "type": "interactive",
  "interactive": {
    "type": "button",
    "body": {
      "text": "Choose an option:"
    },
    "action": {
      "buttons": [
        {
          "type": "reply",
          "reply": {
            "id": "confirm",
            "title": "Confirm"
          }
        },
        {
          "type": "reply",
          "reply": {
            "id": "cancel",
            "title": "Cancel"
          }
        }
      ]
    }
  }
}
```

**Response:**
```json
{
  "messaging_product": "whatsapp",
  "contacts": [{
    "input": "923001234567",
    "wa_id": "923001234567"
  }],
  "messages": [{
    "id": "wamid.HBgLOTIzMDE..."
  }]
}
```

---

## Session States

Customer sessions follow this state machine:

| State | Description | Next States |
|-------|-------------|-------------|
| `INITIAL` | New session created | `AWAITING_ORDER` |
| `AWAITING_ORDER` | Waiting for product selection | `AWAITING_NAME` (on "done") |
| `AWAITING_NAME` | Waiting for customer name | `AWAITING_ADDRESS` |
| `AWAITING_ADDRESS` | Waiting for delivery address | `AWAITING_INSTRUCTIONS` |
| `AWAITING_INSTRUCTIONS` | Waiting for special instructions | `AWAITING_CONFIRMATION` |
| `AWAITING_CONFIRMATION` | Waiting for order confirmation | `ORDER_PLACED` or back to `AWAITING_ORDER` |
| `ORDER_PLACED` | Order completed | Session cleared after 5 minutes |

---

## Conversation Commands

### Global Commands (Work in any state)

| Command | Action | Response |
|---------|--------|----------|
| `hi`, `hello`, `start` | Start conversation | Welcome + Menu |
| `menu` | Show menu | Product list |
| `cart` | Show cart | Cart summary |
| `clear` | Clear cart | Confirmation |
| `cancel` | Cancel order | Goodbye message |
| `help` | Show help | Command list |

### Order Format

**Format:** `{item_number}, {quantity}`

**Examples:**
- `1, 2` - Add 2 of item #1
- `3, 1` - Add 1 of item #3
- `5, 10` - Add 10 of item #5

**Validation:**
- Item number must be valid (1-N)
- Quantity must be 1-50
- Product must be in stock (if tracking inventory)

---

## Error Responses

### Invalid Format
```
❌ Invalid format.

To add items, type:
*1, 2* (item 1, quantity 2)

Or type:
• *cart* - View cart
• *done* - Checkout
• *menu* - Show menu
```

### Invalid Item Number
```
❌ Invalid item number. Please choose 1-5
```

### Out of Stock
```
❌ Sorry, only 2 Chocolate Chip Cookie(s) available in stock.
```

### Empty Cart
```
🛒 Your cart is empty!

Type *menu* to see available items.
```

### Invalid Name
```
Please enter a valid name (at least 2 characters).
```

### Invalid Address
```
Please provide a complete delivery address (at least 10 characters).
```

### System Error
```
Sorry, something went wrong. Please try again or type *cancel* to start over.
```

---

## Response Templates

### Welcome Message
```
👋 Welcome to Cookie Barrel!

Fresh baked goods delivered to your door! 🍪

🍪 *Cookie Barrel Menu*

*Available Items:*
1. Chocolate Chip Cookie - ₹50
2. Oatmeal Cookie - ₹45
3. Brownie - ₹80
4. Vanilla Cupcake - ₹60
5. Red Velvet Cupcake - ₹70

*How to Order:*
Type: *1, 2* (item number, quantity)
Example: *3, 2* (2 Brownies)

*Commands:*
• *cart* - View your cart
• *done* - Proceed to checkout
• *clear* - Clear cart
• *cancel* - Cancel order
• *menu* - Show menu again
```

### Cart Summary
```
🛒 *Your Cart*

• 2x Chocolate Chip Cookie
  ₹50.00 each = ₹100.00

• 1x Brownie
  ₹80.00 each = ₹80.00

*Total: ₹180.00*

Type *done* to proceed to checkout
Type *clear* to empty cart
```

### Order Confirmation
```
✅ *Order Confirmed!*

Order Number: *WA20251018123456*
Total Amount: *₹180.00*
Estimated Delivery: *30-45 minutes*

Your delicious treats are being prepared! 🍪

Thank you for choosing Cookie Barrel!
We'll notify you when your order is ready for delivery.
```

---

## Rate Limits

**WhatsApp Business API Limits:**
- Free tier: 1,000 conversations/month
- Messages per second: 80/sec per phone number
- Message length: 4096 characters max

**Recommended Internal Limits:**
- 10 messages per user per minute
- 100 concurrent sessions
- 1 hour session timeout

---

## Logging

**Log Levels:**

**DEBUG:**
- Session lookups
- State transitions
- API requests

**INFO:**
- Messages received
- Orders created
- Sessions created/cleared

**WARNING:**
- Invalid inputs
- Stock issues
- Configuration issues

**ERROR:**
- API failures
- Database errors
- Unexpected exceptions

**Example Log Entry:**
```
[INFO] [923001234567] Received: hi
[INFO] Session saved: 923001234567, State: AWAITING_ORDER
[INFO] WhatsApp message sent successfully to 923001234567
[INFO] WhatsApp order created: WA20251018123456 for 923001234567
```

---

## Testing with cURL

### Verify Webhook
```bash
curl "http://localhost:5124/api/whatsappwebhook?hub.mode=subscribe&hub.verify_token=cookie_barrel_webhook_secret_2025&hub.challenge=test123"
# Should return: test123
```

### Send Test Message
```bash
curl -X POST http://localhost:5124/api/whatsapptest/send-text \
  -H "Content-Type: application/json" \
  -d '{
    "to": "923001234567",
    "message": "Test from cURL"
  }'
```

### Simulate Conversation
```bash
# Start conversation
curl -X POST http://localhost:5124/api/whatsapptest/simulate-message \
  -H "Content-Type: application/json" \
  -d '{"from":"923001234567","message":"hi"}'

# Add item
curl -X POST http://localhost:5124/api/whatsapptest/simulate-message \
  -H "Content-Type: application/json" \
  -d '{"from":"923001234567","message":"1, 2"}'

# View sessions
curl http://localhost:5124/api/whatsapptest/sessions
```

---

## Database Schema

### Orders Table
WhatsApp orders are stored with these characteristics:
- `OrderNumber`: Starts with "WA" (e.g., WA20251018123456)
- `OrderType`: `Delivery`
- `Status`: `Pending` initially
- `Notes`: Contains delivery address and special instructions
- `CashierId`: `00000000-0000-0000-0000-000000000000` (no cashier)

### Customers Table
- Created/updated automatically from phone number
- `Phone`: WhatsApp phone number
- `Name`: Provided during order
- `Address`: Delivery address
- `Email`: Temporary email (whatsapp-{phone}@cookiebarrel.temp)

---

## Security Considerations

### Production Checklist
- [ ] Use permanent access token (not temporary)
- [ ] Store tokens in environment variables
- [ ] Implement webhook signature verification
- [ ] Add rate limiting
- [ ] Whitelist Meta IP ranges
- [ ] Use HTTPS for webhook URL
- [ ] Monitor for suspicious activity
- [ ] Regular security audits

### Meta Webhook Signature Verification
```csharp
// TODO: Implement in production
private bool VerifySignature(string signature, string payload)
{
    // X-Hub-Signature-256 header verification
    // Using app secret from Meta dashboard
}
```

---

## Performance Optimization

### Recommended for Production

**1. Use Redis for Session Storage:**
```csharp
// Replace InMemorySessionStorage with RedisSessionStorage
builder.Services.AddSingleton<ISessionStorage, RedisSessionStorage>();
```

**2. Implement Caching:**
```csharp
// Cache product list
builder.Services.AddMemoryCache();
```

**3. Add Message Queue:**
```csharp
// Queue messages for async processing
builder.Services.AddHostedService<MessageProcessorService>();
```

**4. Database Optimization:**
```sql
-- Add indexes for faster queries
CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
CREATE INDEX IX_Customers_Phone ON Customers(Phone);
```

---

## Monitoring and Alerts

### Key Metrics to Track

**Response Time:**
- Target: < 2 seconds per message
- Alert if > 5 seconds

**Success Rate:**
- Target: > 95% successful deliveries
- Alert if < 90%

**Session Completion:**
- Target: > 60% checkout from cart
- Track abandonment rate

**Error Rate:**
- Target: < 1% errors
- Alert on spike

### Health Check Endpoint
```bash
# Add to monitoring system
curl https://your-api.com/api/whatsappwebhook/health
```

### Log Monitoring Queries
```
# Search for errors
grep "ERROR" logs/whatsapp-*.txt

# Count orders today
grep "WhatsApp order created" logs/whatsapp-$(date +%Y%m%d).txt | wc -l

# Failed message deliveries
grep "Failed to send WhatsApp message" logs/whatsapp-*.txt
```

---

## Customer Support Commands (Future)

### Planned Enhancements

**Order Status:**
```
You: status WA20251018123456
Bot: Order Status: Preparing
     Estimated delivery: 25 minutes
```

**Order History:**
```
You: my orders
Bot: Your recent orders:
     1. WA20251018123456 - ₹180 - Delivered
     2. WA20251015092341 - ₹250 - In Transit
```

**Repeat Order:**
```
You: repeat last order
Bot: Repeat order WA20251018123456?
     2x Chocolate Chip Cookie - ₹100
     [Confirm/Cancel buttons]
```

---

## Internationalization

### Multi-Language Support (Future)

**Urdu Support:**
```
آپ کا نام کیا ہے؟
(What is your name?)
```

**Auto Language Detection:**
```csharp
private string DetectLanguage(string message)
{
    // Detect Urdu, English, or other
    return languageCode;
}
```

---

## Development Tips

### Testing Without WhatsApp
Use the simulate endpoint to test flows:
```bash
# Complete order flow script
./test-whatsapp-flow.sh
```

### Debugging Sessions
```bash
# View active session
curl http://localhost:5124/api/whatsapptest/sessions/923001234567

# Clear stuck session
curl -X DELETE http://localhost:5124/api/whatsapptest/sessions/923001234567
```

### Local Testing with ngrok
```bash
# Start ngrok with custom subdomain (paid feature)
ngrok http 5124 --subdomain=cookiebarrel-dev

# Or use free random URL
ngrok http 5124
```

---

**Last Updated:** October 18, 2025  
**API Version:** 1.0  
**WhatsApp API Version:** v18.0
