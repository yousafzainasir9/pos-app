using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using POS.Application.Common.Configuration;
using POS.Application.DTOs.WhatsApp;
using POS.Application.Interfaces;

namespace POS.WebAPI.Controllers
{
    /// <summary>
    /// WhatsApp webhook controller for receiving messages
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly WhatsAppSettings _settings;
        private readonly IWhatsAppConversationService _conversationService;
        private readonly ILogger<WhatsAppWebhookController> _logger;

        public WhatsAppWebhookController(
            IOptions<WhatsAppSettings> settings,
            IWhatsAppConversationService conversationService,
            ILogger<WhatsAppWebhookController> logger)
        {
            _settings = settings.Value;
            _conversationService = conversationService;
            _logger = logger;
        }

        /// <summary>
        /// Webhook verification endpoint (GET)
        /// </summary>
        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string? mode,
            [FromQuery(Name = "hub.verify_token")] string? token,
            [FromQuery(Name = "hub.challenge")] string? challenge)
        {
            _logger.LogInformation("Webhook verification attempt - Mode: {Mode}, Token match: {TokenMatch}", 
                mode, token == _settings.WebhookVerifyToken);

            if (mode == "subscribe" && token == _settings.WebhookVerifyToken)
            {
                _logger.LogInformation("✅ Webhook verified successfully");
                return Ok(challenge);
            }

            _logger.LogWarning("❌ Webhook verification failed");
            return Forbid();
        }

        /// <summary>
        /// Receive incoming messages (POST)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhookPayload? payload)
        {
            if (!_settings.Enabled)
            {
                _logger.LogWarning("WhatsApp integration is disabled");
                return Ok();
            }

            if (payload?.Entry == null || !payload.Entry.Any())
            {
                _logger.LogWarning("Received empty payload");
                return Ok();
            }

            try
            {
                foreach (var entry in payload.Entry)
                {
                    foreach (var change in entry.Changes ?? Enumerable.Empty<WhatsAppChange>())
                    {
                        if (change.Field != "messages")
                        {
                            continue;
                        }

                        foreach (var message in change.Value?.Messages ?? Enumerable.Empty<WhatsAppMessage>())
                        {
                            // Only process text messages for now
                            if (message.Type == "text" && message.Text != null)
                            {
                                _logger.LogInformation("Processing message from {PhoneNumber}: {Message}", 
                                    message.From, message.Text.Body);

                                // Process message asynchronously to avoid blocking webhook
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        await _conversationService.HandleIncomingMessageAsync(
                                            message.From,
                                            message.Text.Body,
                                            message.Id);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error processing message {MessageId} from {PhoneNumber}",
                                            message.Id, message.From);
                                    }
                                });
                            }
                            else if (message.Type == "interactive" && message.Interactive != null)
                            {
                                // Handle interactive message responses (buttons, lists)
                                string responseText = string.Empty;

                                if (message.Interactive.Type == "button_reply" && 
                                    message.Interactive.Button_Reply != null)
                                {
                                    responseText = message.Interactive.Button_Reply.Id;
                                }
                                else if (message.Interactive.Type == "list_reply" && 
                                         message.Interactive.List_Reply != null)
                                {
                                    responseText = message.Interactive.List_Reply.Id;
                                }

                                if (!string.IsNullOrEmpty(responseText))
                                {
                                    _ = Task.Run(async () =>
                                    {
                                        try
                                        {
                                            await _conversationService.HandleIncomingMessageAsync(
                                                message.From,
                                                responseText,
                                                message.Id);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "Error processing interactive message {MessageId}",
                                                message.Id);
                                        }
                                    });
                                }
                            }
                            else
                            {
                                _logger.LogInformation("Received non-text message type: {Type} from {PhoneNumber}",
                                    message.Type, message.From);
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook payload");
                return Ok(); // Still return 200 to Meta to avoid retries
            }
        }

        /// <summary>
        /// Health check endpoint for WhatsApp integration
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                enabled = _settings.Enabled,
                configured = !string.IsNullOrEmpty(_settings.AccessToken) && 
                           !string.IsNullOrEmpty(_settings.PhoneNumberId),
                apiVersion = _settings.ApiVersion
            });
        }
    }
}
