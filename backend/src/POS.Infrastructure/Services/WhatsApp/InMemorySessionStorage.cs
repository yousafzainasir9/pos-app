using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using POS.Application.DTOs.WhatsApp;
using POS.Application.Interfaces;

namespace POS.Infrastructure.Services.WhatsApp
{
    /// <summary>
    /// In-memory storage for customer sessions
    /// For production, consider using Redis or database
    /// </summary>
    public class InMemorySessionStorage : ISessionStorage
    {
        private readonly ConcurrentDictionary<string, CustomerSession> _sessions = new();
        private readonly ILogger<InMemorySessionStorage> _logger;

        public InMemorySessionStorage(ILogger<InMemorySessionStorage> logger)
        {
            _logger = logger;
        }

        public Task<CustomerSession?> GetSessionAsync(string phoneNumber)
        {
            if (_sessions.TryGetValue(phoneNumber, out var session))
            {
                _logger.LogDebug("Session found for {PhoneNumber}, State: {State}", phoneNumber, session.State);
                return Task.FromResult<CustomerSession?>(session);
            }
            
            _logger.LogDebug("No session found for {PhoneNumber}", phoneNumber);
            return Task.FromResult<CustomerSession?>(null);
        }

        public Task SaveSessionAsync(CustomerSession session)
        {
            session.LastActivity = DateTime.UtcNow;
            _sessions[session.PhoneNumber] = session;
            _logger.LogInformation("Session saved: {PhoneNumber}, State: {State}", 
                session.PhoneNumber, session.State);
            return Task.CompletedTask;
        }

        public Task ClearSessionAsync(string phoneNumber)
        {
            if (_sessions.TryRemove(phoneNumber, out var session))
            {
                _logger.LogInformation("Session cleared for {PhoneNumber}, was in state: {State}", 
                    phoneNumber, session.State);
            }
            return Task.CompletedTask;
        }

        public Task<List<CustomerSession>> GetAllActiveSessionsAsync()
        {
            var activeSessions = _sessions.Values.ToList();
            return Task.FromResult(activeSessions);
        }

        public Task ClearExpiredSessionsAsync(int timeoutHours)
        {
            var expiredPhoneNumbers = _sessions
                .Where(kvp => DateTime.UtcNow - kvp.Value.LastActivity > TimeSpan.FromHours(timeoutHours))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var phoneNumber in expiredPhoneNumbers)
            {
                if (_sessions.TryRemove(phoneNumber, out _))
                {
                    _logger.LogInformation("Expired session removed: {PhoneNumber}", phoneNumber);
                }
            }

            if (expiredPhoneNumbers.Any())
            {
                _logger.LogInformation("Cleared {Count} expired sessions", expiredPhoneNumbers.Count);
            }

            return Task.CompletedTask;
        }
    }
}
