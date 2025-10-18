namespace POS.Application.Interfaces
{
    /// <summary>
    /// Service for sending WhatsApp messages
    /// </summary>
    public interface IWhatsAppService
    {
        /// <summary>
        /// Send a simple text message
        /// </summary>
        Task<bool> SendTextMessageAsync(string to, string message);
        
        /// <summary>
        /// Send the product menu to a customer
        /// </summary>
        Task<bool> SendMenuAsync(string to);
        
        /// <summary>
        /// Send order confirmation message
        /// </summary>
        Task<bool> SendOrderConfirmationAsync(string to, string orderNumber, decimal total);
        
        /// <summary>
        /// Send interactive message with buttons
        /// </summary>
        Task<bool> SendButtonMessageAsync(string to, string bodyText, List<(string id, string title)> buttons);
        
        /// <summary>
        /// Send cart summary
        /// </summary>
        Task<bool> SendCartSummaryAsync(string to, List<DTOs.WhatsApp.CartItem> items, decimal total);
        
        /// <summary>
        /// Send order summary for confirmation
        /// </summary>
        Task<bool> SendOrderSummaryAsync(string to, string customerName, string address, 
            List<DTOs.WhatsApp.CartItem> items, decimal total);
    }
}
