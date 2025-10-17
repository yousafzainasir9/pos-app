using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Domain.Entities.Settings;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Services;
using POS.Infrastructure.Tests.Helpers;
using Xunit;

namespace POS.Infrastructure.Tests.Services;

/// <summary>
/// Tests for SystemSettingsService - Application settings management
/// </summary>
public class SystemSettingsServiceTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly SystemSettingsService _settingsService;

    public SystemSettingsServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _settingsService = new SystemSettingsService(_context);
    }

    [Fact]
    public async Task GetSettingAsync_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var setting = new SystemSetting
        {
            Key = "TestKey",
            Value = "TestValue",
            Category = "Test"
        };
        await _context.SystemSettings.AddAsync(setting);
        await _context.SaveChangesAsync();

        // Act
        var result = await _settingsService.GetSettingAsync("TestKey");

        // Assert
        result.Should().Be("TestValue");
    }

    [Fact]
    public async Task GetSettingAsync_WithNonExistingKey_ShouldReturnNull()
    {
        // Act
        var result = await _settingsService.GetSettingAsync("NonExistingKey");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateSettingAsync_WithNewKey_ShouldCreateSetting()
    {
        // Act
        await _settingsService.UpdateSettingAsync("NewKey", "NewValue");

        // Assert
        var setting = await _settingsService.GetSettingAsync("NewKey");
        setting.Should().Be("NewValue");
    }

    [Fact]
    public async Task UpdateSettingAsync_WithExistingKey_ShouldUpdateValue()
    {
        // Arrange
        await _settingsService.UpdateSettingAsync("UpdateKey", "OriginalValue");

        // Act
        await _settingsService.UpdateSettingAsync("UpdateKey", "UpdatedValue");

        // Assert
        var setting = await _settingsService.GetSettingAsync("UpdateKey");
        setting.Should().Be("UpdatedValue");
    }

    [Fact]
    public async Task GetReceiptSettingsAsync_ShouldReturnSettings()
    {
        // Act
        var settings = await _settingsService.GetReceiptSettingsAsync();

        // Assert
        settings.Should().NotBeNull();
        settings.StoreName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetEmailSettingsAsync_ShouldReturnSettings()
    {
        // Act
        var settings = await _settingsService.GetEmailSettingsAsync();

        // Assert
        settings.Should().NotBeNull();
        settings.SmtpPort.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDefaultValuesAsync_ShouldReturnDefaults()
    {
        // Act
        var defaults = await _settingsService.GetDefaultValuesAsync();

        // Assert
        defaults.Should().NotBeNull();
        defaults.DefaultTaxRate.Should().BeGreaterThan(0);
        defaults.DefaultLowStockThreshold.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllSettingsAsync_ShouldReturnAllCategories()
    {
        // Act
        var allSettings = await _settingsService.GetAllSettingsAsync();

        // Assert
        allSettings.Should().NotBeNull();
        allSettings.General.Should().NotBeNull();
        allSettings.Receipt.Should().NotBeNull();
        allSettings.Email.Should().NotBeNull();
        allSettings.Defaults.Should().NotBeNull();
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ShouldClearAllSettings()
    {
        // Arrange
        await _settingsService.UpdateSettingAsync("TestKey1", "Value1");
        await _settingsService.UpdateSettingAsync("TestKey2", "Value2");

        // Act
        await _settingsService.ResetToDefaultsAsync();

        // Assert
        var settings = await _context.SystemSettings.ToListAsync();
        settings.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateSettingAsync_ShouldExtractCategoryFromKey()
    {
        // Act
        await _settingsService.UpdateSettingAsync("Receipt.HeaderText", "Test Header");

        // Assert
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == "Receipt.HeaderText");
        
        setting.Should().NotBeNull();
        setting!.Category.Should().Be("Receipt");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
