using POS.Application.DTOs.WhatsApp;

namespace POS.Application.Interfaces
{
    /// <summary>
    /// Service for handling WhatsApp conversation flow
    /// </summary>
    public interface IWhatsAppConversationService
    {
        /// <summary>
        /// Handle an incoming message from a customer
        /// </summary>
        Task HandleIncomingMessageAsync(string from, string message, string messageId);
        
        /// <summary>
        /// Get customer session
        /// </summary>
        Task<CustomerSession?> GetSessionAsync(string phoneNumber);
        
        /// <summary>
        /// Clear customer session
        /// </summary>
        Task ClearSessionAsync(string phoneNumber);
    }
}
