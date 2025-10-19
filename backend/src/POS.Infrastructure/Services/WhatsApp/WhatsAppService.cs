using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POS.Application.Common.Configuration;
using POS.Application.DTOs.WhatsApp;
using POS.Application.Interfaces;

namespace POS.Infrastructure.Services.WhatsApp
{
    /// <summary>
    /// Service for sending WhatsApp messages via WhatsApp Business API
    /// </summary>
    public class WhatsAppService : IWhatsAppService
    {
        private readonly WhatsAppSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public WhatsAppService(
            IOptions<WhatsAppSettings> settings,
            HttpClient httpClient,
            ILogger<WhatsAppService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<bool> SendTextMessageAsync(string to, string message)
        {
            if (!_settings.Enabled)
            {
                _logger.LogWarning("WhatsApp integration is disabled");
                return false;
            }

            try
            {
                var request = new WhatsAppSendMessageRequest
                {
                    to = to,
                    type = "text",
                    text = new WhatsAppTextMessage { body = message }
                };

                return await SendMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending text message to {PhoneNumber}", to);
                return false;
            }
        }

        public async Task<bool> SendMenuAsync(string to)
        {
            var menu =
                "🍪 *Cookie Barrel Menu*\n\n" +
                "*Available Items:*\n" +
                "1. Chocolate Chip Cookie - $50\n" +
                "2. Oatmeal Cookie - $45\n" +
                "3. Brownie - $80\n" +
                "4. Vanilla Cupcake - $60\n" +
                "5. Red Velvet Cupcake - $70\n\n" +
                "*How to Order:*\n" +
                "Type: *1, 2* (item number, quantity)\n" +
                "Example: *3, 2* (2 Brownies)\n\n" +
                "*Commands:*\n" +
                "• *cart* - View your cart\n" +
                "• *done* - Proceed to checkout\n" +
                "• *clear* - Clear cart\n" +
                "• *cancel* - Cancel order\n" +
                "• *menu* - Show menu again";

            return await SendTextMessageAsync(to, menu);
        }

        public async Task<bool> SendOrderConfirmationAsync(string to, string orderNumber, decimal total)
        {
            var message =
                "✅ *Order Confirmed!*\n\n" +
                $"Order Number: *{orderNumber}*\n" +
                $"Total Amount: *${total:F2}*\n" +
                $"Estimated Delivery: *30-45 minutes*\n\n" +
                "Your delicious treats are being prepared! 🍪\n\n" +
                "Thank you for choosing Cookie Barrel!\n" +
                "We'll notify you when your order is ready for delivery.";

            return await SendTextMessageAsync(to, message);
        }

        public async Task<bool> SendButtonMessageAsync(string to, string bodyText, List<(string id, string title)> buttons)
        {
            if (buttons.Count > 3)
            {
                _logger.LogWarning("WhatsApp only supports up to 3 buttons. Truncating.");
                buttons = buttons.Take(3).ToList();
            }

            try
            {
                var request = new WhatsAppSendMessageRequest
                {
                    to = to,
                    type = "interactive",
                    interactive = new WhatsAppInteractiveMessage
                    {
                        type = "button",
                        body = new WhatsAppBody { text = bodyText },
                        action = new WhatsAppAction
                        {
                            buttons = buttons.Select(b => new WhatsAppButton
                            {
                                type = "reply",
                                reply = new WhatsAppReply
                                {
                                    id = b.id,
                                    title = b.title
                                }
                            }).ToList()
                        }
                    }
                };

                return await SendMessageAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending button message to {PhoneNumber}", to);
                return false;
            }
        }

        public async Task<bool> SendCartSummaryAsync(string to, List<CartItem> items, decimal total)
        {
            if (items == null || !items.Any())
            {
                return await SendTextMessageAsync(to, "🛒 Your cart is empty.\n\nType *menu* to see available items.");
            }

            var message = new StringBuilder();
            message.AppendLine("🛒 *Your Cart*\n");

            foreach (var item in items)
            {
                var itemTotal = item.Price * item.Quantity;
                message.AppendLine($"• {item.Quantity}x {item.Name}");
                message.AppendLine($"  ${item.Price:F2} each = ${itemTotal:F2}");
                if (!string.IsNullOrEmpty(item.Notes))
                {
                    message.AppendLine($"  Note: {item.Notes}");
                }
                message.AppendLine();
            }

            message.AppendLine($"*Total: ${total:F2}*\n");
            message.AppendLine("Type *done* to proceed to checkout");
            message.AppendLine("Type *clear* to empty cart");

            return await SendTextMessageAsync(to, message.ToString());
        }

        public async Task<bool> SendOrderSummaryAsync(string to, string customerName, string address, 
            List<CartItem> items, decimal total)
        {
            var message = new StringBuilder();
            message.AppendLine("📋 *Order Summary*\n");
            message.AppendLine("*Items:*");

            foreach (var item in items)
            {
                var itemTotal = item.Price * item.Quantity;
                message.AppendLine($"• {item.Quantity}x {item.Name} - ${itemTotal:F2}");
            }

            message.AppendLine();
            message.AppendLine($"*Subtotal:* ${total:F2}");
            message.AppendLine($"*Delivery:* Free");
            message.AppendLine($"*Total:* ${total:F2}\n");
            message.AppendLine("*Delivery Details:*");
            message.AppendLine($"Name: {customerName}");
            message.AppendLine($"Address: {address}\n");
            message.AppendLine("Type *confirm* to place order");
            message.AppendLine("Type *cancel* to cancel");

            return await SendTextMessageAsync(to, message.ToString());
        }

        private async Task<bool> SendMessageAsync(WhatsAppSendMessageRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.AccessToken}");

                var url = $"{_settings.ApiBaseUrl}/messages";
                _logger.LogDebug("Sending WhatsApp message to {Url}", url);

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("WhatsApp message sent successfully to {PhoneNumber}", request.to);
                    return true;
                }

                _logger.LogError("Failed to send WhatsApp message. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception sending WhatsApp message");
                return false;
            }
        }
    }
}
