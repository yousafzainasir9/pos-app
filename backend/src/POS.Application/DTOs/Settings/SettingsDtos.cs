using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Settings;

public class SystemSettingDto
{
    public long Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpdateSettingDto
{
    [Required]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
}

// ===== GENERAL SETTINGS (Legacy - for backward compatibility) =====
public class GeneralSettingsDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string CompanyWebsite { get; set; } = string.Empty;
    public string CompanyLogo { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "Australia/Sydney";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "HH:mm";
    public string Currency { get; set; } = "AUD";
    public string CurrencySymbol { get; set; } = "$";
}

// ===== RECEIPT SETTINGS =====
public class ReceiptSettingsDto
{
    [StringLength(500, ErrorMessage = "Header text cannot exceed 500 characters")]
    public string HeaderText { get; set; } = "Thank you for your purchase!";

    [StringLength(500, ErrorMessage = "Footer text cannot exceed 500 characters")]
    public string FooterText { get; set; } = "Please visit us again";

    public bool ShowLogo { get; set; } = true;
    public bool ShowTaxDetails { get; set; } = true;
    public bool ShowItemDetails { get; set; } = true;
    public bool ShowBarcode { get; set; } = false;
    public bool ShowQRCode { get; set; } = false;
    public bool ShowCustomerInfo { get; set; } = true;
    public bool ShowCashier { get; set; } = true;
    public bool ShowSocial { get; set; } = false;
    public bool ShowPromotion { get; set; } = false;

    [Required]
    [RegularExpression("^(58mm|80mm|A4)$", ErrorMessage = "Paper size must be 58mm, 80mm, or A4")]
    public string PaperSize { get; set; } = "80mm";

    [Range(8, 24, ErrorMessage = "Font size must be between 8 and 24")]
    public int FontSize { get; set; } = 12;

    [StringLength(50)]
    public string FontFamily { get; set; } = "monospace";

    [Required]
    [RegularExpression("^(standard|compact|detailed|modern|elegant|minimalist|thermal|custom)$", ErrorMessage = "Invalid receipt template")]
    public string ReceiptTemplate { get; set; } = "standard";

    [StringLength(2000)]
    public string CustomTemplate { get; set; } = string.Empty;

    [Range(0, 50)]
    public int PrintMarginTop { get; set; } = 0;
    
    [Range(0, 50)]
    public int PrintMarginBottom { get; set; } = 10;
    
    [Range(0, 50)]
    public int PrintMarginLeft { get; set; } = 5;
    
    [Range(0, 50)]
    public int PrintMarginRight { get; set; } = 5;

    // Store Information
    [StringLength(200, ErrorMessage = "Store name cannot exceed 200 characters")]
    public string StoreName { get; set; } = "My Store";

    [StringLength(500, ErrorMessage = "Store address cannot exceed 500 characters")]
    public string StoreAddress { get; set; } = "123 Main Street, City, State 12345";

    [StringLength(50, ErrorMessage = "Store phone cannot exceed 50 characters")]
    public string StorePhone { get; set; } = "(555) 123-4567";

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(200, ErrorMessage = "Store email cannot exceed 200 characters")]
    public string StoreEmail { get; set; } = "info@mystore.com";

    [StringLength(200, ErrorMessage = "Store website cannot exceed 200 characters")]
    public string StoreWebsite { get; set; } = "www.mystore.com";

    [StringLength(500, ErrorMessage = "Logo URL cannot exceed 500 characters")]
    public string LogoUrl { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Promotion text cannot exceed 500 characters")]
    public string PromotionText { get; set; } = "🎁 Join our loyalty program and save 10% on your next purchase!";
}

// ===== EMAIL & SMTP SETTINGS =====
public class EmailSettingsDto
{
    [StringLength(200, ErrorMessage = "SMTP host cannot exceed 200 characters")]
    public string SmtpHost { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP port must be between 1 and 65535")]
    public int SmtpPort { get; set; } = 587;

    [StringLength(200, ErrorMessage = "SMTP username cannot exceed 200 characters")]
    public string SmtpUsername { get; set; } = string.Empty;

    // Password should be encrypted before storage
    [StringLength(200, ErrorMessage = "SMTP password cannot exceed 200 characters")]
    public string SmtpPassword { get; set; } = string.Empty;

    public bool SmtpUseSsl { get; set; } = true;

    [EmailAddress(ErrorMessage = "Invalid from email address")]
    [StringLength(200, ErrorMessage = "From email cannot exceed 200 characters")]
    public string FromEmail { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "From name cannot exceed 200 characters")]
    public string FromName { get; set; } = string.Empty;

    public bool EnableEmailReceipts { get; set; } = false;
    public bool EnableEmailNotifications { get; set; } = false;
    public bool EnableLowStockAlerts { get; set; } = false;
    public bool EnableDailySalesReport { get; set; } = false;

    [StringLength(50)]
    public string EmailProvider { get; set; } = "SMTP"; // SMTP, SendGrid, AWS SES
}

// ===== SYSTEM DEFAULTS & PREFERENCES =====
public class DefaultValuesDto
{
    // Transaction Defaults
    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
    public decimal DefaultTaxRate { get; set; } = 10.0m;

    [Required]
    [StringLength(50)]
    public string DefaultOrderStatus { get; set; } = "Pending";

    [Required]
    [StringLength(50)]
    public string DefaultPaymentMethod { get; set; } = "Cash";

    public bool RequireCustomerForOrder { get; set; } = false;

    // Inventory Defaults
    [Range(0, 10000, ErrorMessage = "Low stock threshold must be between 0 and 10000")]
    public int DefaultLowStockThreshold { get; set; } = 10;

    // Receipt & Printing Defaults
    [Range(1, 5, ErrorMessage = "Receipt copies must be between 1 and 5")]
    public int ReceiptPrintCopies { get; set; } = 1;

    public bool AutoPrintReceipt { get; set; } = true;
    public bool AutoOpenCashDrawer { get; set; } = true;

    // Session & Security
    [Range(5, 1440, ErrorMessage = "Session timeout must be between 5 and 1440 minutes")]
    public int SessionTimeoutMinutes { get; set; } = 60;

    [Range(4, 20, ErrorMessage = "Password minimum length must be between 4 and 20")]
    public int PasswordMinLength { get; set; } = 6;

    public bool RequireStrongPassword { get; set; } = false;

    // POS Features
    public bool EnableBarcodeLookup { get; set; } = true;
    public bool EnableQuickSale { get; set; } = true;
}

// ===== ALL SETTINGS CONTAINER =====
public class AllSettingsDto
{
    public GeneralSettingsDto General { get; set; } = new(); // Legacy - kept for backward compatibility
    public ReceiptSettingsDto Receipt { get; set; } = new();
    public EmailSettingsDto Email { get; set; } = new();
    public DefaultValuesDto Defaults { get; set; } = new();
}

// ===== BULK UPDATE =====
public class BulkUpdateSettingsDto
{
    public Dictionary<string, string> Settings { get; set; } = new();
}

// ===== SETTINGS EXPORT/IMPORT =====
public class SettingsExportDto
{
    public ReceiptSettingsDto Receipt { get; set; } = new();
    public EmailSettingsDto Email { get; set; } = new();
    public DefaultValuesDto Defaults { get; set; } = new();
    public DateTime ExportedAt { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
}
