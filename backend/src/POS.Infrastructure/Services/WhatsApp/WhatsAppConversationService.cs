using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POS.Application.Common.Configuration;
using POS.Application.Common.Interfaces;
using POS.Application.DTOs;
using POS.Application.DTOs.WhatsApp;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Domain.Enums;

namespace POS.Infrastructure.Services.WhatsApp
{
    /// <summary>
    /// Handles WhatsApp conversation flow and state management
    /// </summary>
    public class WhatsAppConversationService : IWhatsAppConversationService
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly ISessionStorage _sessionStorage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppConversationService> _logger;

        public WhatsAppConversationService(
            IWhatsAppService whatsAppService,
            ISessionStorage sessionStorage,
            IUnitOfWork unitOfWork,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppConversationService> logger)
        {
            _whatsAppService = whatsAppService;
            _sessionStorage = sessionStorage;
            _unitOfWork = unitOfWork;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task HandleIncomingMessageAsync(string from, string message, string messageId)
        {
            try
            {
                _logger.LogInformation("[{PhoneNumber}] Received: {Message}", from, message);

                var session = await GetOrCreateSessionAsync(from);
                var normalizedMessage = message.ToLower().Trim();

                // Handle global commands first
                if (await HandleGlobalCommandAsync(from, normalizedMessage, session))
                {
                    return;
                }

                // State machine - handle based on current state
                switch (session.State)
                {
                    case SessionState.INITIAL:
                    case SessionState.AWAITING_ORDER:
                        await HandleOrderingAsync(from, message, normalizedMessage, session);
                        break;

                    case SessionState.AWAITING_NAME:
                        await HandleNameInputAsync(from, message, session);
                        break;

                    case SessionState.AWAITING_ADDRESS:
                        await HandleAddressInputAsync(from, message, session);
                        break;

                    case SessionState.AWAITING_INSTRUCTIONS:
                        await HandleInstructionsInputAsync(from, message, session);
                        break;

                    case SessionState.AWAITING_CONFIRMATION:
                        await HandleConfirmationAsync(from, normalizedMessage, session);
                        break;

                    default:
                        await SendWelcomeMessageAsync(from);
                        session.State = SessionState.AWAITING_ORDER;
                        await _sessionStorage.SaveSessionAsync(session);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message from {PhoneNumber}", from);
                await _whatsAppService.SendTextMessageAsync(from,
                    "Sorry, something went wrong. Please try again or type *cancel* to start over.");
            }
        }

        private async Task<bool> HandleGlobalCommandAsync(string from, string message, CustomerSession session)
        {
            switch (message)
            {
                case "hi":
                case "hello":
                case "start":
                case "menu":
                    await SendWelcomeMessageAsync(from);
                    session.State = SessionState.AWAITING_ORDER;
                    session.Cart.Clear();
                    await _sessionStorage.SaveSessionAsync(session);
                    return true;

                case "cart":
                    await ShowCartAsync(from, session);
                    return true;

                case "clear":
                    session.Cart.Clear();
                    await _sessionStorage.SaveSessionAsync(session);
                    await _whatsAppService.SendTextMessageAsync(from, 
                        "🗑️ Cart cleared! Type *menu* to start ordering.");
                    return true;

                case "cancel":
                    await _sessionStorage.ClearSessionAsync(from);
                    await _whatsAppService.SendTextMessageAsync(from,
                        "❌ Order cancelled. Type *hi* to start a new order anytime!");
                    return true;

                case "help":
                    await SendHelpMessageAsync(from);
                    return true;
            }

            return false;
        }

        private async Task HandleOrderingAsync(string from, string fullMessage, string normalizedMessage, CustomerSession session)
        {
            // Check for "done" command
            if (normalizedMessage == "done")
            {
                await HandleDoneCommandAsync(from, session);
                return;
            }

            // Try to parse order format: "1, 2" or "1,2" or "1 2"
            var match = Regex.Match(fullMessage, @"^(\d+)\s*[,\s]\s*(\d+)$");
            if (match.Success)
            {
                var itemNumber = int.Parse(match.Groups[1].Value);
                var quantity = int.Parse(match.Groups[2].Value);
                await AddItemToCartAsync(from, session, itemNumber, quantity);
                return;
            }

            // Invalid input
            await _whatsAppService.SendTextMessageAsync(from,
                "❌ Invalid format.\n\n" +
                "To add items, type:\n" +
                "*1, 2* (item 1, quantity 2)\n\n" +
                "Or type:\n" +
                "• *cart* - View cart\n" +
                "• *done* - Checkout\n" +
                "• *menu* - Show menu");
        }

        private async Task AddItemToCartAsync(string from, CustomerSession session, int itemNumber, int quantity)
        {
            if (quantity <= 0)
            {
                await _whatsAppService.SendTextMessageAsync(from, "Quantity must be at least 1.");
                return;
            }

            if (quantity > 50)
            {
                await _whatsAppService.SendTextMessageAsync(from, "Maximum quantity is 50 items per product.");
                return;
            }

            // Get products from database using generic repository
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();
            var activeProducts = products.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToList();

            if (itemNumber < 1 || itemNumber > activeProducts.Count)
            {
                await _whatsAppService.SendTextMessageAsync(from, 
                    $"❌ Invalid item number. Please choose 1-{activeProducts.Count}");
                return;
            }

            var product = activeProducts[itemNumber - 1];

            // Check stock if tracking inventory
            if (product.TrackInventory && product.StockQuantity < quantity)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    $"❌ Sorry, only {product.StockQuantity} {product.Name}(s) available in stock.");
                return;
            }

            // Check if product already in cart
            var existingItem = session.Cart.FirstOrDefault(c => c.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                session.Cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.PriceIncGst,
                    Quantity = quantity
                });
            }

            await _sessionStorage.SaveSessionAsync(session);

            var itemTotal = product.PriceIncGst * quantity;
            var cartTotal = session.Cart.Sum(c => c.Price * c.Quantity);

            await _whatsAppService.SendTextMessageAsync(from,
                $"✅ Added {quantity}x {product.Name}\n\n" +
                $"Item Total: ${itemTotal:F2}\n" +
                $"Cart Total: ${cartTotal:F2}\n\n" +
                "Add more items or type *done* to checkout");
        }

        private async Task HandleDoneCommandAsync(string from, CustomerSession session)
        {
            if (session.Cart.Count == 0)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "🛒 Your cart is empty!\n\nType *menu* to see available items.");
                return;
            }

            session.State = SessionState.AWAITING_NAME;
            await _sessionStorage.SaveSessionAsync(session);

            await _whatsAppService.SendTextMessageAsync(from,
                "Great! Let's get your order details.\n\n" +
                "What's your name?");
        }

        private async Task HandleNameInputAsync(string from, string name, CustomerSession session)
        {
            name = name.Trim();

            if (name.Length < 2)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "Please enter a valid name (at least 2 characters).");
                return;
            }

            if (name.Length > 100)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "Name is too long. Please enter a shorter name.");
                return;
            }

            session.CustomerName = name;
            session.State = SessionState.AWAITING_ADDRESS;
            await _sessionStorage.SaveSessionAsync(session);

            await _whatsAppService.SendTextMessageAsync(from,
                $"Thanks, {name}! 👋\n\n" +
                "What's your delivery address?");
        }

        private async Task HandleAddressInputAsync(string from, string address, CustomerSession session)
        {
            address = address.Trim();

            if (address.Length < 10)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "Please provide a complete delivery address (at least 10 characters).");
                return;
            }

            if (address.Length > 500)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "Address is too long. Please provide a shorter address.");
                return;
            }

            session.DeliveryAddress = address;
            session.State = SessionState.AWAITING_INSTRUCTIONS;
            await _sessionStorage.SaveSessionAsync(session);

            await _whatsAppService.SendTextMessageAsync(from,
                "Perfect! 📍\n\n" +
                "Any special instructions for your order?\n" +
                "(Type *skip* if none)");
        }

        private async Task HandleInstructionsInputAsync(string from, string instructions, CustomerSession session)
        {
            instructions = instructions.Trim();

            if (instructions.ToLower() != "skip")
            {
                if (instructions.Length > 500)
                {
                    await _whatsAppService.SendTextMessageAsync(from,
                        "Instructions are too long. Please keep it under 500 characters.");
                    return;
                }
                session.SpecialInstructions = instructions;
            }

            session.State = SessionState.AWAITING_CONFIRMATION;
            await _sessionStorage.SaveSessionAsync(session);

            // Send order summary
            var total = session.Cart.Sum(c => c.Price * c.Quantity);
            var summaryResult = await _whatsAppService.SendOrderSummaryAsync(from, session.CustomerName, 
                session.DeliveryAddress, session.Cart, total);

            if (!summaryResult)
            {
                _logger.LogError("Failed to send order summary to {PhoneNumber}", from);
                await _whatsAppService.SendTextMessageAsync(from,
                    "Sorry, there was an error displaying your order summary. Please try again or type *cancel*.");
                session.State = SessionState.AWAITING_ORDER;
                await _sessionStorage.SaveSessionAsync(session);
            }
        }

        private async Task HandleConfirmationAsync(string from, string message, CustomerSession session)
        {
            if (message == "confirm")
            {
                await CreateOrderAsync(from, session);
            }
            else if (message == "cancel")
            {
                await _sessionStorage.ClearSessionAsync(from);
                await _whatsAppService.SendTextMessageAsync(from,
                    "❌ Order cancelled.\n\nType *hi* to start a new order anytime!");
            }
            else
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "Please type *confirm* to place your order or *cancel* to cancel.");
            }
        }

        private async Task CreateOrderAsync(string from, CustomerSession session)
        {
            try
            {
                // Get store ID with enhanced logging
                var storeId = _settings.DefaultStoreId ?? 0;
                if (storeId == 0)
                {
                    _logger.LogWarning("Default store ID not configured for WhatsApp orders");
                    var stores = await _unitOfWork.Repository<Store>().GetAllAsync();
                    var firstStore = stores.FirstOrDefault();
                    if (firstStore != null)
                    {
                        storeId = firstStore.Id;
                        _logger.LogInformation("Using first available store: {StoreId}", storeId);
                    }
                    else
                    {
                        throw new InvalidOperationException("No stores available in the system. Please create a store first.");
                    }
                }

                // Verify store exists
                var store = await _unitOfWork.Repository<Store>().SingleOrDefaultAsync(s => s.Id == storeId);
                if (store == null)
                {
                    throw new InvalidOperationException($"Configured store (ID: {storeId}) not found in database.");
                }

                // Find or create customer
                var customers = await _unitOfWork.Repository<Customer>().GetAllAsync();
                var customer = customers.FirstOrDefault(c => c.Phone == from);

                if (customer == null)
                {
                    var nameParts = session.CustomerName.Trim().Split(new[] { ' ' }, 2);
                    customer = new Customer
                    {
                        FirstName = nameParts[0],
                        LastName = nameParts.Length > 1 ? nameParts[1] : "",
                        Phone = from,
                        Email = $"whatsapp-{from.Replace("+", "")}@cookiebarrel.temp",
                        Address = session.DeliveryAddress,
                        IsActive = true
                    };
                    await _unitOfWork.Repository<Customer>().AddAsync(customer);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Created new customer for phone: {Phone}", from);
                }
                else
                {
                    // Update customer info
                    var nameParts = session.CustomerName.Trim().Split(new[] { ' ' }, 2);
                    customer.FirstName = nameParts[0];
                    customer.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                    customer.Address = session.DeliveryAddress;
                    _unitOfWork.Repository<Customer>().Update(customer);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Updated existing customer: {CustomerId}", customer.Id);
                }

                // Get system user
                var users = await _unitOfWork.Repository<User>().GetAllAsync();
                var systemUser = users.FirstOrDefault();
                if (systemUser == null)
                {
                    throw new InvalidOperationException("No users available in the system. Please create at least one user first.");
                }

                // Create order with more logging
                var orderNumber = $"WA{DateTime.UtcNow:yyyyMMddHHmmss}";
                _logger.LogInformation("Creating order {OrderNumber} for customer {CustomerId}", orderNumber, customer.Id);

                var order = new Order
                {
                    OrderNumber = orderNumber,
                    OrderDate = DateTime.UtcNow,
                    OrderType = OrderType.Delivery,
                    Status = OrderStatus.Pending,
                    CustomerId = customer.Id,
                    StoreId = storeId,
                    UserId = systemUser.Id,
                    Notes = string.IsNullOrEmpty(session.SpecialInstructions) 
                        ? $"WhatsApp Order - {session.DeliveryAddress}"
                        : $"WhatsApp Order - {session.DeliveryAddress}\nInstructions: {session.SpecialInstructions}"
                };

                // Add order items with validation
                foreach (var cartItem in session.Cart)
                {
                    var product = await _unitOfWork.Repository<Product>().SingleOrDefaultAsync(p => p.Id == cartItem.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Product {ProductId} not found for cart item", cartItem.ProductId);
                        continue;
                    }

                    // Check stock again before creating order
                    if (product.TrackInventory && product.StockQuantity < cartItem.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}, Required: {cartItem.Quantity}");
                    }

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        UnitPriceIncGst = product.PriceIncGst,
                        DiscountAmount = 0,
                        Notes = cartItem.Notes
                    });
                }

                if (order.OrderItems.Count == 0)
                {
                    throw new InvalidOperationException("No valid order items found in cart");
                }

                // Calculate totals
                order.SubTotal = order.OrderItems.Sum(i => i.UnitPriceIncGst * i.Quantity);
                order.TaxAmount = 0;
                order.DiscountAmount = 0;
                order.TotalAmount = order.SubTotal;

                // Save order
                await _unitOfWork.Repository<Order>().AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Order {OrderNumber} created successfully with {ItemCount} items, Total: ${Total}", 
                    order.OrderNumber, order.OrderItems.Count, order.TotalAmount);

                // Update inventory if tracking
                foreach (var item in order.OrderItems)
                {
                    var product = await _unitOfWork.Repository<Product>().SingleOrDefaultAsync(p => p.Id == item.ProductId);
                    if (product != null && product.TrackInventory)
                    {
                        product.StockQuantity -= item.Quantity;
                        _unitOfWork.Repository<Product>().Update(product);
                    }
                }
                await _unitOfWork.SaveChangesAsync();

                // Update session
                session.OrderNumber = order.OrderNumber;
                session.State = SessionState.ORDER_PLACED;
                await _sessionStorage.SaveSessionAsync(session);

                // Send confirmation
                await _whatsAppService.SendOrderConfirmationAsync(from, order.OrderNumber, order.TotalAmount);

                // Clear session after delay
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                    await _sessionStorage.ClearSessionAsync(from);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating WhatsApp order for {PhoneNumber}. Error: {ErrorMessage}", from, ex.Message);
                
                // Send user-friendly error message
                var errorMessage = ex.Message.Contains("store") || ex.Message.Contains("user")
                    ? "❌ System configuration error. Please contact support."
                    : "❌ Sorry, there was an error processing your order. Please try again or contact us directly.";
                
                await _whatsAppService.SendTextMessageAsync(from, errorMessage);
                
                session.State = SessionState.AWAITING_ORDER;
                await _sessionStorage.SaveSessionAsync(session);
            }
        }

        private async Task ShowCartAsync(string from, CustomerSession session)
        {
            if (session.Cart.Count == 0)
            {
                await _whatsAppService.SendTextMessageAsync(from,
                    "🛒 Your cart is empty.\n\nType *menu* to see available items.");
                return;
            }

            var total = session.Cart.Sum(c => c.Price * c.Quantity);
            await _whatsAppService.SendCartSummaryAsync(from, session.Cart, total);
        }

        private async Task SendWelcomeMessageAsync(string from)
        {
            await _whatsAppService.SendTextMessageAsync(from,
                "👋 Welcome to Cookie Barrel!\n\n" +
                "Fresh baked goods delivered to your door! 🍪");
            
            await Task.Delay(500);
            await _whatsAppService.SendMenuAsync(from);
        }

        private async Task SendHelpMessageAsync(string from)
        {
            await _whatsAppService.SendTextMessageAsync(from,
                "📱 *Cookie Barrel Help*\n\n" +
                "*Available Commands:*\n" +
                "• *menu* - Show products\n" +
                "• *cart* - View your cart\n" +
                "• *done* - Proceed to checkout\n" +
                "• *clear* - Clear cart\n" +
                "• *cancel* - Cancel order\n" +
                "• *help* - Show this help\n\n" +
                "*How to Order:*\n" +
                "1. Type item number and quantity\n" +
                "   Example: *1, 2* (2x item #1)\n" +
                "2. Type *done* when ready\n" +
                "3. Provide your details\n" +
                "4. Confirm your order\n\n" +
                "Need assistance? Just ask!");
        }

        private async Task<CustomerSession> GetOrCreateSessionAsync(string phoneNumber)
        {
            var session = await _sessionStorage.GetSessionAsync(phoneNumber);
            if (session == null)
            {
                session = new CustomerSession
                {
                    PhoneNumber = phoneNumber,
                    State = SessionState.INITIAL,
                    StoreId = _settings.DefaultStoreId
                };
                await _sessionStorage.SaveSessionAsync(session);
            }
            return session;
        }

        public Task<CustomerSession?> GetSessionAsync(string phoneNumber)
        {
            return _sessionStorage.GetSessionAsync(phoneNumber);
        }

        public Task ClearSessionAsync(string phoneNumber)
        {
            return _sessionStorage.ClearSessionAsync(phoneNumber);
        }
    }
}
