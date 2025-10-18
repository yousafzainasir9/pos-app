# 🎨 WhatsApp Integration - Architecture Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                          WHATSAPP USER                              │
│                    📱 Sends "hi" to WhatsApp                        │
└────────────────────────────────┬────────────────────────────────────┘
                                 │
                                 ↓
┌─────────────────────────────────────────────────────────────────────┐
│                    WHATSAPP BUSINESS API                            │
│                    (Meta/Facebook Platform)                         │
│  • Receives messages from users                                     │
│  • Sends messages to users                                          │
│  • Manages phone number authentication                              │
└────────────────────────────────┬────────────────────────────────────┘
                                 │
                                 ↓
                      ┌──────────────────────┐
                      │   Webhook (HTTPS)    │
                      │  /api/whatsappwebhook│
                      └──────────┬───────────┘
                                 │
                                 ↓
┌─────────────────────────────────────────────────────────────────────┐
│                   COOKIE BARREL POS BACKEND                         │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │         WhatsAppWebhookController.cs                        │  │
│  │  • GET  - Webhook verification                               │  │
│  │  • POST - Receive messages                                   │  │
│  └──────────────────────┬──────────────────────────────────────┘  │
│                         │                                           │
│                         ↓                                           │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │      WhatsAppConversationService.cs                         │  │
│  │  📊 State Machine:                                           │  │
│  │    INITIAL → AWAITING_ORDER → AWAITING_NAME →               │  │
│  │    AWAITING_ADDRESS → AWAITING_INSTRUCTIONS →               │  │
│  │    AWAITING_CONFIRMATION → ORDER_PLACED                     │  │
│  │                                                              │  │
│  │  • Handle incoming messages                                  │  │
│  │  • Manage conversation flow                                  │  │
│  │  • Process commands (cart, done, cancel)                    │  │
│  │  • Validate inputs                                           │  │
│  │  • Create orders                                             │  │
│  └──────────────┬───────────────────────┬──────────────────────┘  │
│                 │                       │                          │
│                 ↓                       ↓                          │
│  ┌──────────────────────┐   ┌──────────────────────────┐         │
│  │ InMemorySessionStorage│   │   WhatsAppService.cs     │         │
│  │                      │   │                          │         │
│  │ • Store sessions     │   │ • SendTextMessageAsync() │         │
│  │ • Manage cart        │   │ • SendMenuAsync()        │         │
│  │ • Track state        │   │ • SendCartSummaryAsync() │         │
│  │ • Timeout handling   │   │ • SendOrderConfirm()     │──┐     │
│  └──────────────────────┘   └──────────────────────────┘  │     │
│                                           │                 │     │
│                                           └─────────────────┘     │
│                                           Calls WhatsApp API       │
│                                                                    │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                  Database Layer (UnitOfWork)                 │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │ │
│  │  │ProductRepo   │  │ OrderRepo    │  │CustomerRepo  │     │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘     │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                                  │                                 │
└──────────────────────────────────┼─────────────────────────────────┘
                                   │
                                   ↓
                    ┌──────────────────────────┐
                    │   SQL SERVER DATABASE    │
                    │  • Orders                │
                    │  • OrderItems            │
                    │  • Customers             │
                    │  • Products              │
                    │  • Stores                │
                    └──────────────────────────┘
```

## Message Flow Diagram

```
┌───────────┐                                              ┌────────────┐
│  Customer │                                              │   Backend  │
└─────┬─────┘                                              └──────┬─────┘
      │                                                           │
      │  1. "hi"                                                 │
      ├──────────────────────────────────────────────────────────>
      │                                                           │
      │                      2. Welcome + Menu                   │
      <──────────────────────────────────────────────────────────┤
      │                                                           │
      │  3. "1, 2" (Add item)                                    │
      ├──────────────────────────────────────────────────────────>
      │                                                           │
      │                      [Check Product]                     │
      │                      [Check Stock]                       │
      │                      [Add to Cart]                       │
      │                      [Save Session]                      │
      │                                                           │
      │                      4. "Added 2x ..."                   │
      <──────────────────────────────────────────────────────────┤
      │                                                           │
      │  5. "cart"                                               │
      ├──────────────────────────────────────────────────────────>
      │                                                           │
      │                      6. Cart Summary                     │
      <──────────────────────────────────────────────────────────┤
      │                                                           │
      │  7. "done"                                               │
      ├──────────────────────────────────────────────────────────>
      │                                                           │
      │                      [State → AWAITING_NAME]             │
      │                                                           │
      │                      8. "What's your name?"              │
      <──────────────────────────────────────────────────────────┤
      │                                                           │
      │  9. "John Doe"                                           │
      ├──────────────────────────────────────────────────────────>
      │                                                           │
      │                      [State → AWAITING_ADDRESS]          │
      │                                                           │
      │                      10. "Your address?"                 │
      <──────────────────────────────────────────────────────────┤
      │                                                           │
      │  11. "123 Main Street"                                   │
      ├──────────────────────────────────────────────────────────>
      │                                                           │
      │                      [State → AWAITING_INSTRUCTIONS]     │
      │                                                           │
      │                      12. "Any instructions?"             │
      <──────────────────────────────────────────────────────────┤
      │                                                           │
      │  13. "No nuts please"                                    │
      ├──────────────────────────────────────────────────────────>
      │                                                           │
      │                      [State → AWAITING_CONFIRMATION]     │
      │                                                           │
      │                      14. Order Summary                   │
      <──────────────────────────────────────────────────────────┤
      │                                                           │
      │  15. "confirm"                                           │
      ├──────────────────────────────────────────────────────────>
      │                                                           │
      │                      [Create Order in DB]                │
      │                      [Create/Update Customer]            │
      │                      [Update Stock]                      │
      │                      [State → ORDER_PLACED]              │
      │                                                           │
      │                      16. "✅ Order Confirmed!"           │
      <──────────────────────────────────────────────────────────┤
      │                                                           │
```

## Session State Machine

```
                    ┌──────────────┐
                    │   INITIAL    │
                    └──────┬───────┘
                           │
                    (User sends "hi")
                           │
                           ↓
              ┌────────────────────────┐
              │   AWAITING_ORDER       │◄──────┐
              │                        │       │
              │  Commands:             │       │
              │  • "1, 2" → Add item   │       │
              │  • "cart" → Show cart  │───────┘
              │  • "done" → Checkout   │  (View cart
              └────────────┬───────────┘   & continue)
                           │
                    (User: "done")
                           │
                           ↓
              ┌────────────────────────┐
              │   AWAITING_NAME        │
              │                        │
              │  Collect: Name         │
              └────────────┬───────────┘
                           │
                    (User: "John")
                           │
                           ↓
              ┌────────────────────────┐
              │   AWAITING_ADDRESS     │
              │                        │
              │  Collect: Address      │
              └────────────┬───────────┘
                           │
                (User: "123 Main St")
                           │
                           ↓
              ┌────────────────────────┐
              │  AWAITING_INSTRUCTIONS │
              │                        │
              │  Collect: Instructions │
              └────────────┬───────────┘
                           │
               (User: "No nuts" / "skip")
                           │
                           ↓
              ┌────────────────────────┐
              │  AWAITING_CONFIRMATION │
              │                        │
              │  Show: Order Summary   │
              │  Wait: "confirm"       │
              └────────────┬───────────┘
                           │
                           ├──confirm──→┌──────────────┐
                           │             │ ORDER_PLACED │
                           │             └──────────────┘
                           │
                           └──cancel───→[Clear Session]

          (Any state: "cancel" → Clear Session)
```

## Component Diagram

```
┌────────────────────────────────────────────────────────────────┐
│                    POS.WebAPI Layer                            │
│                                                                │
│  ┌──────────────────────────┐  ┌─────────────────────────┐   │
│  │ WhatsAppWebhookController│  │ WhatsAppTestController  │   │
│  │  • Verify()             │  │  • SendText()          │   │
│  │  • ReceiveMessage()     │  │  • SendMenu()          │   │
│  │  • Health()             │  │  • GetSessions()       │   │
│  └────────────┬─────────────┘  └──────────┬──────────────┘   │
│               │                           │                   │
│  ┌────────────▼───────────────────────────▼────────────────┐ │
│  │            Configuration/WhatsAppSettings               │ │
│  │  • AccessToken                                          │ │
│  │  • PhoneNumberId                                        │ │
│  │  • WebhookVerifyToken                                   │ │
│  └─────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌────────────────────────────────────────────────────────────────┐
│                  POS.Application Layer                         │
│                                                                │
│  ┌──────────────────────┐        ┌─────────────────────┐     │
│  │    Interfaces        │        │       DTOs          │     │
│  │  • IWhatsAppService  │        │  • WhatsAppModels   │     │
│  │  • IConversation...  │        │  • SessionModels    │     │
│  │  • ISessionStorage   │        │  • CartItem         │     │
│  └──────────────────────┘        └─────────────────────┘     │
└────────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌────────────────────────────────────────────────────────────────┐
│                 POS.Infrastructure Layer                       │
│                                                                │
│  ┌──────────────────────────────────────────────────────┐    │
│  │        Services/WhatsApp/                            │    │
│  │                                                       │    │
│  │  ┌────────────────────────────────────┐             │    │
│  │  │  WhatsAppService                   │             │    │
│  │  │  • HTTP Client                     │             │    │
│  │  │  • SendTextMessage()               │             │    │
│  │  │  • SendMenu()                      │             │    │
│  │  │  • SendOrderConfirmation()         │             │    │
│  │  └────────────────────────────────────┘             │    │
│  │                                                       │    │
│  │  ┌────────────────────────────────────┐             │    │
│  │  │  WhatsAppConversationService       │             │    │
│  │  │  • State Machine Logic             │             │    │
│  │  │  • Command Handling                │             │    │
│  │  │  • Order Creation                  │             │    │
│  │  └────────────────────────────────────┘             │    │
│  │                                                       │    │
│  │  ┌────────────────────────────────────┐             │    │
│  │  │  InMemorySessionStorage            │             │    │
│  │  │  • ConcurrentDictionary            │             │    │
│  │  │  • Session CRUD                    │             │    │
│  │  │  • Timeout Management              │             │    │
│  │  └────────────────────────────────────┘             │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                                │
│  ┌──────────────────────────────────────────────────────┐    │
│  │             Repositories/UnitOfWork                   │    │
│  │  • ProductRepository                                  │    │
│  │  • OrderRepository                                    │    │
│  │  • CustomerRepository                                 │    │
│  └──────────────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌────────────────────────────────────────────────────────────────┐
│                     Database (SQL Server)                      │
│                                                                │
│  Orders     OrderItems     Customers     Products     Stores  │
└────────────────────────────────────────────────────────────────┘
```

## Data Flow - Adding Item to Cart

```
┌─────────┐
│Customer │
│ "1, 2"  │
└────┬────┘
     │
     ↓
┌────────────────────────────┐
│WhatsAppWebhookController   │
│ReceiveMessage()            │
└────┬───────────────────────┘
     │
     ↓
┌────────────────────────────────────┐
│WhatsAppConversationService         │
│HandleIncomingMessageAsync()        │
│                                    │
│ 1. Parse "1, 2"                   │
│    ↓                               │
│ 2. itemNum=1, qty=2               │
└────┬───────────────────────────────┘
     │
     ↓
┌────────────────────────────────────┐
│ GetSession(phoneNumber)            │
│    ← SessionStorage                │
└────┬───────────────────────────────┘
     │
     ↓
┌────────────────────────────────────┐
│ Get Products from DB               │
│    ← ProductRepository             │
│ activeProducts[0] = Product #1     │
└────┬───────────────────────────────┘
     │
     ↓
┌────────────────────────────────────┐
│ Validate                           │
│  • Item number valid? ✓            │
│  • Stock available? ✓              │
│  • Quantity 1-50? ✓                │
└────┬───────────────────────────────┘
     │
     ↓
┌────────────────────────────────────┐
│ Add to Session Cart                │
│  session.Cart.Add({                │
│    ProductId: guid,                │
│    Name: "Choc Chip Cookie",       │
│    Price: 50.00,                   │
│    Quantity: 2                     │
│  })                                │
└────┬───────────────────────────────┘
     │
     ↓
┌────────────────────────────────────┐
│ SaveSession()                      │
│    → SessionStorage                │
└────┬───────────────────────────────┘
     │
     ↓
┌────────────────────────────────────┐
│ WhatsAppService                    │
│ SendTextMessage(                   │
│   "✅ Added 2x Cookie              │
│    Cart Total: ₹100"               │
│ )                                  │
└────┬───────────────────────────────┘
     │
     ↓
┌────────────────────────────────────┐
│ WhatsApp Business API              │
│ POST /messages                     │
└────┬───────────────────────────────┘
     │
     ↓
┌─────────┐
│Customer │
│Receives │
│Message  │
└─────────┘
```

## Database Schema Integration

```
┌──────────────────────────────────────────────────────────┐
│                    Orders Table                          │
├──────────────────────────────────────────────────────────┤
│ Id (PK)              │ Guid                              │
│ OrderNumber          │ "WA20251018123456"               │
│ OrderType            │ Delivery                          │
│ Status               │ Pending → Processing → Completed │
│ CustomerId (FK)      │ → Customers.Id                   │
│ StoreId (FK)         │ → Stores.Id                      │
│ CashierId            │ 00000000-0000... (No cashier)    │
│ SubTotal             │ 100.00                           │
│ TotalAmount          │ 100.00                           │
│ Notes                │ "WhatsApp: 123 Main St..."       │
│ OrderDate            │ DateTime                          │
└──────────────────────────────────────────────────────────┘
                         │
                         │ One-to-Many
                         ↓
┌──────────────────────────────────────────────────────────┐
│                  OrderItems Table                        │
├──────────────────────────────────────────────────────────┤
│ Id (PK)              │ Long                             │
│ OrderId (FK)         │ → Orders.Id                      │
│ ProductId (FK)       │ → Products.Id                    │
│ Quantity             │ 2                                │
│ UnitPriceIncGst      │ 50.00                            │
│ TotalAmount          │ 100.00                           │
│ Notes                │ "No nuts"                        │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│                  Customers Table                         │
├──────────────────────────────────────────────────────────┤
│ Id (PK)              │ Long                             │
│ Name                 │ "John Doe"                       │
│ Phone                │ "923001234567"                   │
│ Email                │ "whatsapp-92...@temp"            │
│ Address              │ "123 Main Street, Karachi"       │
│ IsActive             │ true                             │
└──────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────┐
│                   Products Table                         │
├──────────────────────────────────────────────────────────┤
│ Id (PK)              │ Long                             │
│ Name                 │ "Chocolate Chip Cookie"          │
│ PriceIncGst          │ 50.00                            │
│ StockQuantity        │ 100                              │
│ IsActive             │ true                             │
│ TrackInventory       │ true                             │
└──────────────────────────────────────────────────────────┘
```

## Session Storage Structure

```
InMemorySessionStorage (ConcurrentDictionary)
│
├── Key: "923001234567"
│   └── Value: CustomerSession
│       ├── PhoneNumber: "923001234567"
│       ├── State: "AWAITING_ADDRESS"
│       ├── CustomerName: "John Doe"
│       ├── DeliveryAddress: ""
│       ├── SpecialInstructions: ""
│       ├── Cart: [
│       │   {
│       │     ProductId: guid,
│       │     Name: "Choc Chip Cookie",
│       │     Price: 50.00,
│       │     Quantity: 2
│       │   }
│       │ ]
│       ├── LastActivity: 2025-10-18T10:30:00Z
│       ├── CreatedAt: 2025-10-18T10:00:00Z
│       ├── StoreId: guid
│       └── OrderNumber: null
│
├── Key: "923009876543"
│   └── Value: CustomerSession
│       └── [Another customer's session]
│
└── Key: "923001111111"
    └── Value: CustomerSession
        └── [Yet another customer's session]

[Expired sessions cleaned up hourly]
```

---

**This architecture ensures:**
- ✅ Scalability (add Redis for production)
- ✅ Maintainability (clean separation of concerns)
- ✅ Testability (interfaces for mocking)
- ✅ Reliability (error handling at each layer)
- ✅ Performance (async operations, caching)
