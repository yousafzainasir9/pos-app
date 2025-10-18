namespace POS.Application.Common.Configuration
{
    /// <summary>
    /// WhatsApp Business API configuration settings
    /// </summary>
    public class WhatsAppSettings
    {
        /// <summary>
        /// WhatsApp Business API access token
        /// Get this from Meta Developer Portal
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Phone Number ID from WhatsApp Business API
        /// </summary>
        public string PhoneNumberId { get; set; } = string.Empty;
        
        /// <summary>
        /// Webhook verification token (you define this)
        /// </summary>
        public string WebhookVerifyToken { get; set; } = string.Empty;
        
        /// <summary>
        /// WhatsApp API version
        /// </summary>
        public string ApiVersion { get; set; } = "v18.0";
        
        /// <summary>
        /// Constructed base URL for API calls
        /// </summary>
        public string ApiBaseUrl => $"https://graph.facebook.com/{ApiVersion}/{PhoneNumberId}";
        
        /// <summary>
        /// Session timeout in hours
        /// </summary>
        public int SessionTimeoutHours { get; set; } = 1;
        
        /// <summary>
        /// Default store ID for WhatsApp orders (long type)
        /// </summary>
        public long? DefaultStoreId { get; set; }
        
        /// <summary>
        /// Enable/disable WhatsApp integration
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
