using POS.Application.DTOs.Settings;

namespace POS.Application.Interfaces;

public interface ISystemSettingsService
{
    // Get settings
    Task<AllSettingsDto> GetAllSettingsAsync(CancellationToken cancellationToken = default);
    Task<GeneralSettingsDto> GetGeneralSettingsAsync(CancellationToken cancellationToken = default);
    Task<ReceiptSettingsDto> GetReceiptSettingsAsync(CancellationToken cancellationToken = default);
    Task<EmailSettingsDto> GetEmailSettingsAsync(CancellationToken cancellationToken = default);
    Task<DefaultValuesDto> GetDefaultValuesAsync(CancellationToken cancellationToken = default);
    Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default);
    
    // Update settings
    Task UpdateGeneralSettingsAsync(GeneralSettingsDto settings, CancellationToken cancellationToken = default);
    Task UpdateReceiptSettingsAsync(ReceiptSettingsDto settings, CancellationToken cancellationToken = default);
    Task UpdateEmailSettingsAsync(EmailSettingsDto settings, CancellationToken cancellationToken = default);
    Task UpdateDefaultValuesAsync(DefaultValuesDto settings, CancellationToken cancellationToken = default);
    Task UpdateSettingAsync(string key, string value, CancellationToken cancellationToken = default);
    Task BulkUpdateSettingsAsync(BulkUpdateSettingsDto settings, CancellationToken cancellationToken = default);
    
    // Test settings
    Task<bool> TestEmailSettingsAsync(EmailSettingsDto settings, string testEmail, CancellationToken cancellationToken = default);
    
    // Reset settings
    Task ResetToDefaultsAsync(CancellationToken cancellationToken = default);
}
