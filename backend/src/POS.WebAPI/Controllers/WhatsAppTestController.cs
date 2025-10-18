using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using POS.Application.Common.Configuration;
using POS.Application.Interfaces;

namespace POS.WebAPI.Controllers
{
    /// <summary>
    /// Test controller for WhatsApp integration (Development only)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppTestController : ControllerBase
    {
        private readonly WhatsAppSettings _settings;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IWhatsAppConversationService _conversationService;
        private readonly ISessionStorage _sessionStorage;
        private readonly ILogger<WhatsAppTestController> _logger;

        public WhatsAppTestController(
            IOptions<WhatsAppSettings> settings,
            IWhatsAppService whatsAppService,
            IWhatsAppConversationService conversationService,
            ISessionStorage sessionStorage,
            ILogger<WhatsAppTestController> logger)
        {
            _settings = settings.Value;
            _whatsAppService = whatsAppService;
            _conversationService = conversationService;
            _sessionStorage = sessionStorage;
            _logger = logger;
        }

        /// <summary>
        /// Get configuration status
        /// </summary>
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            return Ok(new
            {
                enabled = _settings.Enabled,
                hasAccessToken = !string.IsNullOrEmpty(_settings.AccessToken),
                hasPhoneNumberId = !string.IsNullOrEmpty(_settings.PhoneNumberId),
                webhookVerifyToken = _settings.WebhookVerifyToken,
                apiVersion = _settings.ApiVersion,
                defaultStoreId = _settings.DefaultStoreId,
                sessionTimeoutHours = _settings.SessionTimeoutHours
            });
        }

        /// <summary>
        /// Send a test text message
        /// </summary>
        [HttpPost("send-text")]
        public async Task<IActionResult> SendTestMessage([FromBody] TestMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.To))
            {
                return BadRequest("Phone number is required");
            }

            var success = await _whatsAppService.SendTextMessageAsync(
                request.To,
                request.Message ?? "Test message from Cookie Barrel POS");

            return Ok(new { success, to = request.To, message = request.Message });
        }

        /// <summary>
        /// Send the menu to a phone number
        /// </summary>
        [HttpPost("send-menu")]
        public async Task<IActionResult> SendMenu([FromBody] TestMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.To))
            {
                return BadRequest("Phone number is required");
            }

            var success = await _whatsAppService.SendMenuAsync(request.To);

            return Ok(new { success, to = request.To });
        }

        /// <summary>
        /// Get all active sessions
        /// </summary>
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = await _sessionStorage.GetAllActiveSessionsAsync();
            return Ok(sessions);
        }

        /// <summary>
        /// Get a specific session
        /// </summary>
        [HttpGet("sessions/{phoneNumber}")]
        public async Task<IActionResult> GetSession(string phoneNumber)
        {
            var session = await _sessionStorage.GetSessionAsync(phoneNumber);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }

        /// <summary>
        /// Clear a specific session
        /// </summary>
        [HttpDelete("sessions/{phoneNumber}")]
        public async Task<IActionResult> ClearSession(string phoneNumber)
        {
            await _sessionStorage.ClearSessionAsync(phoneNumber);
            return Ok(new { message = "Session cleared", phoneNumber });
        }

        /// <summary>
        /// Clear all expired sessions
        /// </summary>
        [HttpPost("sessions/clear-expired")]
        public async Task<IActionResult> ClearExpiredSessions()
        {
            await _sessionStorage.ClearExpiredSessionsAsync(_settings.SessionTimeoutHours);
            return Ok(new { message = "Expired sessions cleared" });
        }

        /// <summary>
        /// Simulate an incoming message (for testing without WhatsApp)
        /// </summary>
        [HttpPost("simulate-message")]
        public async Task<IActionResult> SimulateMessage([FromBody] SimulateMessageRequest request)
        {
            if (string.IsNullOrEmpty(request.From) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("From and Message are required");
            }

            try
            {
                await _conversationService.HandleIncomingMessageAsync(
                    request.From,
                    request.Message,
                    Guid.NewGuid().ToString());

                return Ok(new
                {
                    success = true,
                    message = "Message processed",
                    from = request.From,
                    text = request.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating message");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class TestMessageRequest
    {
        public string To { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    public class SimulateMessageRequest
    {
        public string From { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
