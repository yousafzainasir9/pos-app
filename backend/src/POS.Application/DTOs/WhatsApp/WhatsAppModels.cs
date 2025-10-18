namespace POS.Application.DTOs.WhatsApp
{
    /// <summary>
    /// Root webhook payload from WhatsApp
    /// </summary>
    public class WhatsAppWebhookPayload
    {
        public string Object { get; set; } = string.Empty;
        public List<WhatsAppEntry> Entry { get; set; } = new();
    }
    
    /// <summary>
    /// Entry containing changes
    /// </summary>
    public class WhatsAppEntry
    {
        public string Id { get; set; } = string.Empty;
        public List<WhatsAppChange> Changes { get; set; } = new();
    }
    
    /// <summary>
    /// Change containing message data
    /// </summary>
    public class WhatsAppChange
    {
        public WhatsAppValue Value { get; set; } = new();
        public string Field { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Value containing messages
    /// </summary>
    public class WhatsAppValue
    {
        public string Messaging_Product { get; set; } = "whatsapp";
        public WhatsAppMetadata? Metadata { get; set; }
        public List<WhatsAppMessage>? Messages { get; set; }
        public List<WhatsAppContact>? Contacts { get; set; }
    }
    
    /// <summary>
    /// Metadata about the WhatsApp business
    /// </summary>
    public class WhatsAppMetadata
    {
        public string Display_Phone_Number { get; set; } = string.Empty;
        public string Phone_Number_Id { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Contact information
    /// </summary>
    public class WhatsAppContact
    {
        public WhatsAppProfile? Profile { get; set; }
        public string Wa_Id { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Contact profile
    /// </summary>
    public class WhatsAppProfile
    {
        public string Name { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Incoming message from customer
    /// </summary>
    public class WhatsAppMessage
    {
        public string From { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public WhatsAppText? Text { get; set; }
        public WhatsAppInteractive? Interactive { get; set; }
    }
    
    /// <summary>
    /// Text message content
    /// </summary>
    public class WhatsAppText
    {
        public string Body { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Interactive message response (buttons, lists)
    /// </summary>
    public class WhatsAppInteractive
    {
        public string Type { get; set; } = string.Empty;
        public WhatsAppButtonReply? Button_Reply { get; set; }
        public WhatsAppListReply? List_Reply { get; set; }
    }
    
    /// <summary>
    /// Button reply
    /// </summary>
    public class WhatsAppButtonReply
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// List reply
    /// </summary>
    public class WhatsAppListReply
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    
    // ===== OUTGOING MESSAGE MODELS =====
    
    /// <summary>
    /// Request to send a text message
    /// </summary>
    public class WhatsAppSendMessageRequest
    {
        public string messaging_product { get; set; } = "whatsapp";
        public string recipient_type { get; set; } = "individual";
        public string to { get; set; } = string.Empty;
        public string type { get; set; } = "text";
        public WhatsAppTextMessage? text { get; set; }
        public WhatsAppInteractiveMessage? interactive { get; set; }
    }
    
    /// <summary>
    /// Text message to send
    /// </summary>
    public class WhatsAppTextMessage
    {
        public string body { get; set; } = string.Empty;
        public bool preview_url { get; set; } = false;
    }
    
    /// <summary>
    /// Interactive message (buttons, lists)
    /// </summary>
    public class WhatsAppInteractiveMessage
    {
        public string type { get; set; } = string.Empty; // "button" or "list"
        public WhatsAppHeader? header { get; set; }
        public WhatsAppBody? body { get; set; }
        public WhatsAppFooter? footer { get; set; }
        public WhatsAppAction? action { get; set; }
    }
    
    /// <summary>
    /// Message header
    /// </summary>
    public class WhatsAppHeader
    {
        public string type { get; set; } = "text";
        public string text { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Message body
    /// </summary>
    public class WhatsAppBody
    {
        public string text { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Message footer
    /// </summary>
    public class WhatsAppFooter
    {
        public string text { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Interactive action (buttons or sections)
    /// </summary>
    public class WhatsAppAction
    {
        public List<WhatsAppButton>? buttons { get; set; }
        public string? button { get; set; } // For list messages
        public List<WhatsAppSection>? sections { get; set; }
    }
    
    /// <summary>
    /// Interactive button
    /// </summary>
    public class WhatsAppButton
    {
        public string type { get; set; } = "reply";
        public WhatsAppReply? reply { get; set; }
    }
    
    /// <summary>
    /// Button reply data
    /// </summary>
    public class WhatsAppReply
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// List section
    /// </summary>
    public class WhatsAppSection
    {
        public string title { get; set; } = string.Empty;
        public List<WhatsAppRow> rows { get; set; } = new();
    }
    
    /// <summary>
    /// List row
    /// </summary>
    public class WhatsAppRow
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string? description { get; set; }
    }
    
    /// <summary>
    /// API Response from WhatsApp
    /// </summary>
    public class WhatsAppApiResponse
    {
        public string Messaging_Product { get; set; } = "whatsapp";
        public List<WhatsAppContact>? Contacts { get; set; }
        public List<WhatsAppMessageStatus>? Messages { get; set; }
    }
    
    /// <summary>
    /// Message status
    /// </summary>
    public class WhatsAppMessageStatus
    {
        public string Id { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Error response from WhatsApp API
    /// </summary>
    public class WhatsAppErrorResponse
    {
        public WhatsAppError? Error { get; set; }
    }
    
    /// <summary>
    /// Error details
    /// </summary>
    public class WhatsAppError
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Code { get; set; }
        public string? Fbtrace_Id { get; set; }
    }
}
