using POS.Application.DTOs.WhatsApp;

namespace POS.Application.Interfaces
{
    /// <summary>
    /// Storage for customer conversation sessions
    /// </summary>
    public interface ISessionStorage
    {
        /// <summary>
        /// Get a customer session by phone number
        /// </summary>
        Task<CustomerSession?> GetSessionAsync(string phoneNumber);
        
        /// <summary>
        /// Save or update a customer session
        /// </summary>
        Task SaveSessionAsync(CustomerSession session);
        
        /// <summary>
        /// Clear a customer session
        /// </summary>
        Task ClearSessionAsync(string phoneNumber);
        
        /// <summary>
        /// Get all active sessions
        /// </summary>
        Task<List<CustomerSession>> GetAllActiveSessionsAsync();
        
        /// <summary>
        /// Clear expired sessions
        /// </summary>
        Task ClearExpiredSessionsAsync(int timeoutHours);
    }
}
