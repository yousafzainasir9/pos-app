using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Settings;
using POS.Application.Interfaces;
using POS.Domain.Entities.Settings;
using POS.Infrastructure.Data;
using System.Net;
using System.Net.Mail;

namespace POS.Infrastructure.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly POSDbContext _context;

    public SystemSettingsService(POSDbContext context)
    {
        _context = context;
    }

    #region Get Settings

    public async Task<AllSettingsDto> GetAllSettingsAsync(CancellationToken cancellationToken = default)
    {
        return new AllSettingsDto
        {
            General = await GetGeneralSettingsAsync(cancellationToken),
            Receipt = await GetReceiptSettingsAsync(cancellationToken),
            Email = await GetEmailSettingsAsync(cancellationToken),
            Defaults = await GetDefaultValuesAsync(cancellationToken)
        };
    }

    public async Task<GeneralSettingsDto> GetGeneralSettingsAsync(CancellationToken cancellationToken = default)
    {
        // General settings removed from frontend but kept for backward compatibility
        return new GeneralSettingsDto
        {
            CompanyName = "",
            CompanyAddress = "",
            CompanyPhone = "",
            CompanyEmail = "",
            CompanyWebsite = "",
            CompanyLogo = "",
            TimeZone = "Australia/Sydney",
            DateFormat = "dd/MM/yyyy",
            TimeFormat = "HH:mm",
            Currency = "AUD",
            CurrencySymbol = "$"
        };
    }

    public async Task<ReceiptSettingsDto> GetReceiptSettingsAsync(CancellationToken cancellationToken = default)
    {
        return new ReceiptSettingsDto
        {
            HeaderText = await GetSettingAsync("Receipt.HeaderText", cancellationToken) ?? "Thank you for your purchase!",
            FooterText = await GetSettingAsync("Receipt.FooterText", cancellationToken) ?? "Please visit us again",
            ShowLogo = bool.Parse(await GetSettingAsync("Receipt.ShowLogo", cancellationToken) ?? "true"),
            ShowTaxDetails = bool.Parse(await GetSettingAsync("Receipt.ShowTaxDetails", cancellationToken) ?? "true"),
            ShowItemDetails = bool.Parse(await GetSettingAsync("Receipt.ShowItemDetails", cancellationToken) ?? "true"),
            ShowBarcode = bool.Parse(await GetSettingAsync("Receipt.ShowBarcode", cancellationToken) ?? "false"),
            ShowQRCode = bool.Parse(await GetSettingAsync("Receipt.ShowQRCode", cancellationToken) ?? "false"),
            ShowCustomerInfo = bool.Parse(await GetSettingAsync("Receipt.ShowCustomerInfo", cancellationToken) ?? "true"),
            ShowCashier = bool.Parse(await GetSettingAsync("Receipt.ShowCashier", cancellationToken) ?? "true"),
            ShowSocial = bool.Parse(await GetSettingAsync("Receipt.ShowSocial", cancellationToken) ?? "false"),
            ShowPromotion = bool.Parse(await GetSettingAsync("Receipt.ShowPromotion", cancellationToken) ?? "false"),
            PaperSize = await GetSettingAsync("Receipt.PaperSize", cancellationToken) ?? "80mm",
            FontSize = int.Parse(await GetSettingAsync("Receipt.FontSize", cancellationToken) ?? "12"),
            FontFamily = await GetSettingAsync("Receipt.FontFamily", cancellationToken) ?? "monospace",
            ReceiptTemplate = await GetSettingAsync("Receipt.ReceiptTemplate", cancellationToken) ?? "standard",
            CustomTemplate = await GetSettingAsync("Receipt.CustomTemplate", cancellationToken) ?? "",
            PrintMarginTop = int.Parse(await GetSettingAsync("Receipt.PrintMarginTop", cancellationToken) ?? "0"),
            PrintMarginBottom = int.Parse(await GetSettingAsync("Receipt.PrintMarginBottom", cancellationToken) ?? "10"),
            PrintMarginLeft = int.Parse(await GetSettingAsync("Receipt.PrintMarginLeft", cancellationToken) ?? "5"),
            PrintMarginRight = int.Parse(await GetSettingAsync("Receipt.PrintMarginRight", cancellationToken) ?? "5"),
            StoreName = await GetSettingAsync("Receipt.StoreName", cancellationToken) ?? "My Store",
            StoreAddress = await GetSettingAsync("Receipt.StoreAddress", cancellationToken) ?? "123 Main Street, City, State 12345",
            StorePhone = await GetSettingAsync("Receipt.StorePhone", cancellationToken) ?? "(555) 123-4567",
            StoreEmail = await GetSettingAsync("Receipt.StoreEmail", cancellationToken) ?? "info@mystore.com",
            StoreWebsite = await GetSettingAsync("Receipt.StoreWebsite", cancellationToken) ?? "www.mystore.com",
            LogoUrl = await GetSettingAsync("Receipt.LogoUrl", cancellationToken) ?? "",
            PromotionText = await GetSettingAsync("Receipt.PromotionText", cancellationToken) ?? "🎁 Join our loyalty program and save 10% on your next purchase!"
        };
    }

    public async Task<EmailSettingsDto> GetEmailSettingsAsync(CancellationToken cancellationToken = default)
    {
        return new EmailSettingsDto
        {
            SmtpHost = await GetSettingAsync("Email.SmtpHost", cancellationToken) ?? "",
            SmtpPort = int.Parse(await GetSettingAsync("Email.SmtpPort", cancellationToken) ?? "587"),
            SmtpUsername = await GetSettingAsync("Email.SmtpUsername", cancellationToken) ?? "",
            SmtpPassword = await GetSettingAsync("Email.SmtpPassword", cancellationToken) ?? "", // TODO: Decrypt
            SmtpUseSsl = bool.Parse(await GetSettingAsync("Email.SmtpUseSsl", cancellationToken) ?? "true"),
            FromEmail = await GetSettingAsync("Email.FromEmail", cancellationToken) ?? "",
            FromName = await GetSettingAsync("Email.FromName", cancellationToken) ?? "",
            EnableEmailReceipts = bool.Parse(await GetSettingAsync("Email.EnableEmailReceipts", cancellationToken) ?? "false"),
            EnableEmailNotifications = bool.Parse(await GetSettingAsync("Email.EnableEmailNotifications", cancellationToken) ?? "false"),
            EnableLowStockAlerts = bool.Parse(await GetSettingAsync("Email.EnableLowStockAlerts", cancellationToken) ?? "false"),
            EnableDailySalesReport = bool.Parse(await GetSettingAsync("Email.EnableDailySalesReport", cancellationToken) ?? "false"),
            EmailProvider = await GetSettingAsync("Email.EmailProvider", cancellationToken) ?? "SMTP"
        };
    }

    public async Task<DefaultValuesDto> GetDefaultValuesAsync(CancellationToken cancellationToken = default)
    {
        return new DefaultValuesDto
        {
            DefaultTaxRate = decimal.Parse(await GetSettingAsync("Defaults.TaxRate", cancellationToken) ?? "10.0"),
            DefaultLowStockThreshold = int.Parse(await GetSettingAsync("Defaults.LowStockThreshold", cancellationToken) ?? "10"),
            DefaultOrderStatus = await GetSettingAsync("Defaults.OrderStatus", cancellationToken) ?? "Pending",
            DefaultPaymentMethod = await GetSettingAsync("Defaults.PaymentMethod", cancellationToken) ?? "Cash",
            SessionTimeoutMinutes = int.Parse(await GetSettingAsync("Defaults.SessionTimeout", cancellationToken) ?? "60"),
            ReceiptPrintCopies = int.Parse(await GetSettingAsync("Defaults.ReceiptCopies", cancellationToken) ?? "1"),
            AutoPrintReceipt = bool.Parse(await GetSettingAsync("Defaults.AutoPrintReceipt", cancellationToken) ?? "true"),
            AutoOpenCashDrawer = bool.Parse(await GetSettingAsync("Defaults.AutoOpenCashDrawer", cancellationToken) ?? "true"),
            RequireCustomerForOrder = bool.Parse(await GetSettingAsync("Defaults.RequireCustomer", cancellationToken) ?? "false"),
            PasswordMinLength = int.Parse(await GetSettingAsync("Defaults.PasswordMinLength", cancellationToken) ?? "6"),
            RequireStrongPassword = bool.Parse(await GetSettingAsync("Defaults.RequireStrongPassword", cancellationToken) ?? "false"),
            EnableBarcodeLookup = bool.Parse(await GetSettingAsync("Defaults.EnableBarcodeLookup", cancellationToken) ?? "true"),
            EnableQuickSale = bool.Parse(await GetSettingAsync("Defaults.EnableQuickSale", cancellationToken) ?? "true")
        };
    }

    public async Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _context.SystemSettings
            .AsNoTracking() // Prevent EF from caching entities
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
        
        return setting?.Value;
    }

    #endregion

    #region Update Settings

    public async Task UpdateGeneralSettingsAsync(GeneralSettingsDto settings, CancellationToken cancellationToken = default)
    {
        // General settings removed - no-op for backward compatibility
        await Task.CompletedTask;
    }

    public async Task UpdateReceiptSettingsAsync(ReceiptSettingsDto settings, CancellationToken cancellationToken = default)
    {
        await UpdateSettingAsync("Receipt.HeaderText", settings.HeaderText, cancellationToken);
        await UpdateSettingAsync("Receipt.FooterText", settings.FooterText, cancellationToken);
        await UpdateSettingAsync("Receipt.ShowLogo", settings.ShowLogo.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.ShowTaxDetails", settings.ShowTaxDetails.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.ShowItemDetails", settings.ShowItemDetails.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.ShowBarcode", settings.ShowBarcode.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.ShowQRCode", settings.ShowQRCode.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.ShowCustomerInfo", settings.ShowCustomerInfo.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.ShowCashier", settings.ShowCashier.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.ShowSocial", settings.ShowSocial.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.ShowPromotion", settings.ShowPromotion.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.PaperSize", settings.PaperSize, cancellationToken);
        await UpdateSettingAsync("Receipt.FontSize", settings.FontSize.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.FontFamily", settings.FontFamily, cancellationToken);
        await UpdateSettingAsync("Receipt.ReceiptTemplate", settings.ReceiptTemplate, cancellationToken);
        await UpdateSettingAsync("Receipt.CustomTemplate", settings.CustomTemplate, cancellationToken);
        await UpdateSettingAsync("Receipt.PrintMarginTop", settings.PrintMarginTop.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.PrintMarginBottom", settings.PrintMarginBottom.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.PrintMarginLeft", settings.PrintMarginLeft.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.PrintMarginRight", settings.PrintMarginRight.ToString(), cancellationToken);
        await UpdateSettingAsync("Receipt.StoreName", settings.StoreName, cancellationToken);
        await UpdateSettingAsync("Receipt.StoreAddress", settings.StoreAddress, cancellationToken);
        await UpdateSettingAsync("Receipt.StorePhone", settings.StorePhone, cancellationToken);
        await UpdateSettingAsync("Receipt.StoreEmail", settings.StoreEmail, cancellationToken);
        await UpdateSettingAsync("Receipt.StoreWebsite", settings.StoreWebsite, cancellationToken);
        await UpdateSettingAsync("Receipt.LogoUrl", settings.LogoUrl, cancellationToken);
        await UpdateSettingAsync("Receipt.PromotionText", settings.PromotionText, cancellationToken);
    }

    public async Task UpdateEmailSettingsAsync(EmailSettingsDto settings, CancellationToken cancellationToken = default)
    {
        await UpdateSettingAsync("Email.SmtpHost", settings.SmtpHost, cancellationToken);
        await UpdateSettingAsync("Email.SmtpPort", settings.SmtpPort.ToString(), cancellationToken);
        await UpdateSettingAsync("Email.SmtpUsername", settings.SmtpUsername, cancellationToken);
        // TODO: Encrypt password before storage
        await UpdateSettingAsync("Email.SmtpPassword", settings.SmtpPassword, cancellationToken);
        await UpdateSettingAsync("Email.SmtpUseSsl", settings.SmtpUseSsl.ToString(), cancellationToken);
        await UpdateSettingAsync("Email.FromEmail", settings.FromEmail, cancellationToken);
        await UpdateSettingAsync("Email.FromName", settings.FromName, cancellationToken);
        await UpdateSettingAsync("Email.EnableEmailReceipts", settings.EnableEmailReceipts.ToString(), cancellationToken);
        await UpdateSettingAsync("Email.EnableEmailNotifications", settings.EnableEmailNotifications.ToString(), cancellationToken);
        await UpdateSettingAsync("Email.EnableLowStockAlerts", settings.EnableLowStockAlerts.ToString(), cancellationToken);
        await UpdateSettingAsync("Email.EnableDailySalesReport", settings.EnableDailySalesReport.ToString(), cancellationToken);
        await UpdateSettingAsync("Email.EmailProvider", settings.EmailProvider, cancellationToken);
    }

    public async Task UpdateDefaultValuesAsync(DefaultValuesDto settings, CancellationToken cancellationToken = default)
    {
        await UpdateSettingAsync("Defaults.TaxRate", settings.DefaultTaxRate.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.LowStockThreshold", settings.DefaultLowStockThreshold.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.OrderStatus", settings.DefaultOrderStatus, cancellationToken);
        await UpdateSettingAsync("Defaults.PaymentMethod", settings.DefaultPaymentMethod, cancellationToken);
        await UpdateSettingAsync("Defaults.SessionTimeout", settings.SessionTimeoutMinutes.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.ReceiptCopies", settings.ReceiptPrintCopies.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.AutoPrintReceipt", settings.AutoPrintReceipt.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.AutoOpenCashDrawer", settings.AutoOpenCashDrawer.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.RequireCustomer", settings.RequireCustomerForOrder.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.PasswordMinLength", settings.PasswordMinLength.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.RequireStrongPassword", settings.RequireStrongPassword.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.EnableBarcodeLookup", settings.EnableBarcodeLookup.ToString(), cancellationToken);
        await UpdateSettingAsync("Defaults.EnableQuickSale", settings.EnableQuickSale.ToString(), cancellationToken);
    }

    public async Task UpdateSettingAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        if (setting == null)
        {
            // Create new setting
            var category = key.Split('.')[0]; // Extract category from key
            setting = new SystemSetting
            {
                Key = key,
                Value = value,
                Category = category,
                Description = $"Setting for {key}"
            };
            await _context.SystemSettings.AddAsync(setting, cancellationToken);
        }
        else
        {
            // Update existing setting
            setting.Value = value;
            _context.SystemSettings.Update(setting);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BulkUpdateSettingsAsync(BulkUpdateSettingsDto settings, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in settings.Settings)
        {
            await UpdateSettingAsync(kvp.Key, kvp.Value, cancellationToken);
        }
    }

    #endregion

    #region Test Settings

    public async Task<bool> TestEmailSettingsAsync(EmailSettingsDto settings, string testEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                EnableSsl = settings.SmtpUseSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.FromName),
                Subject = "POS System - Test Email",
                Body = "This is a test email from your POS system. If you received this, your email settings are configured correctly!",
                IsBodyHtml = false
            };
            message.To.Add(testEmail);

            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Reset Settings

    public async Task ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        // Clear all existing settings
        var allSettings = await _context.SystemSettings.ToListAsync(cancellationToken);
        _context.SystemSettings.RemoveRange(allSettings);
        await _context.SaveChangesAsync(cancellationToken);

        // Settings will be created with defaults when requested
    }

    #endregion
}
