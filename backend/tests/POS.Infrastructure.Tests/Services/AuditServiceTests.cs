using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using POS.Application.Common.Interfaces;
using POS.Application.DTOs.Audit;
using POS.Domain.Entities.Audit;
using POS.Domain.Enums;
using POS.Infrastructure.Data;
using POS.Infrastructure.Repositories;
using POS.Infrastructure.Services;
using POS.Infrastructure.Tests.Helpers;
using Xunit;

namespace POS.Infrastructure.Tests.Services;

/// <summary>
/// Tests for AuditService - Audit logging and querying
/// </summary>
public class AuditServiceTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly Mock<ILogger<AuditService>> _mockLogger;
    private readonly AuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public AuditServiceTests()
    {
        _context = InMemoryDbContextFactory.CreateWithData();
        _unitOfWork = new UnitOfWork(_context);
        _mockLogger = new Mock<ILogger<AuditService>>();
        _auditService = new AuditService(_context);
    }

    [Fact]
    public async Task LogSecurityEventAsync_WithValidEvent_ShouldSaveToDatabase()
    {
        // Arrange
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            UserId = 1,
            UserName = "testuser",
            Description = "User logged in successfully",
            Success = true,
            IpAddress = "127.0.0.1",
            UserAgent = "Test Browser",
            StoreId = 1
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);

        // Assert
        var savedLog = await _context.SecurityLogs.FirstOrDefaultAsync(l => l.UserId == 1);
        savedLog.Should().NotBeNull();
        savedLog!.EventType.Should().Be(SecurityEventType.Login);
        savedLog.Description.Should().Be("User logged in successfully");
        savedLog.Success.Should().BeTrue();
    }

    [Fact]
    public async Task LogSecurityEventAsync_ShouldSetTimestamp()
    {
        // Arrange
        var beforeLog = DateTime.Now;
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Test login",
            Success = true
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);
        var afterLog = DateTime.Now;

        // Assert
        var savedLog = await _context.SecurityLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.Timestamp.Should().BeOnOrAfter(beforeLog);
        savedLog.Timestamp.Should().BeOnOrBefore(afterLog);
    }

    [Fact]
    public async Task LogSecurityEventAsync_LoginSuccess_ShouldLogCorrectly()
    {
        // Arrange
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            UserId = 1,
            UserName = "testuser",
            Description = "Successful login",
            Success = true,
            IpAddress = "192.168.1.100",
            StoreId = 1
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);

        // Assert
        var logs = await _context.SecurityLogs.ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].EventType.Should().Be(SecurityEventType.Login);
        logs[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task LogSecurityEventAsync_LoginFailure_ShouldLogWithWarning()
    {
        // Arrange
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.LoginFailed,
            Severity = SecuritySeverity.Warning,
            UserName = "unknownuser",
            Description = "Failed login attempt",
            Success = false,
            IpAddress = "192.168.1.100"
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);

        // Assert
        var savedLog = await _context.SecurityLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.EventType.Should().Be(SecurityEventType.LoginFailed);
        savedLog.Severity.Should().Be(SecuritySeverity.Warning);
        savedLog.Success.Should().BeFalse();
    }

    [Fact]
    public async Task LogSecurityEventAsync_MultipleEvents_ShouldSaveAll()
    {
        // Arrange
        var events = new[]
        {
            new SecurityLog
            {
                EventType = SecurityEventType.Login,
                Severity = SecuritySeverity.Info,
                Description = "Login 1",
                Success = true
            },
            new SecurityLog
            {
                EventType = SecurityEventType.Logout,
                Severity = SecuritySeverity.Info,
                Description = "Logout 1",
                Success = true
            },
            new SecurityLog
            {
                EventType = SecurityEventType.PasswordChanged,
                Severity = SecuritySeverity.Info,
                Description = "Password changed",
                Success = true
            }
        };

        // Act
        foreach (var evt in events)
        {
            await _auditService.LogSecurityEventAsync(evt);
        }

        // Assert
        var logs = await _context.SecurityLogs.ToListAsync();
        logs.Should().HaveCount(3);
    }

    [Fact]
    public async Task LogSecurityEventAsync_WithStoreId_ShouldSaveStoreId()
    {
        // Arrange
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Login at store",
            Success = true,
            StoreId = 1
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);

        // Assert
        var savedLog = await _context.SecurityLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.StoreId.Should().Be(1);
    }

    [Fact]
    public async Task LogSecurityEventAsync_WithUserAgent_ShouldSaveUserAgent()
    {
        // Arrange
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Login with user agent",
            Success = true,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);

        // Assert
        var savedLog = await _context.SecurityLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.UserAgent.Should().Be("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    }

    [Fact]
    public async Task LogSecurityEventAsync_WithIpAddress_ShouldSaveIpAddress()
    {
        // Arrange
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Login from IP",
            Success = true,
            IpAddress = "203.0.113.42"
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);

        // Assert
        var savedLog = await _context.SecurityLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.IpAddress.Should().Be("203.0.113.42");
    }

    [Fact]
    public async Task GetSecurityLogsAsync_WithNoFilters_ShouldReturnAllLogs()
    {
        // Arrange
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Log 1",
            Success = true
        });
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Logout,
            Severity = SecuritySeverity.Info,
            Description = "Log 2",
            Success = true
        });

        // Act
        var request = new SecurityLogSearchRequest { Page = 1, PageSize = 10 };
        var result = await _auditService.GetSecurityLogsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetSecurityLogsAsync_WithUserIdFilter_ShouldReturnMatchingLogs()
    {
        // Arrange
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            UserId = 1,
            Description = "User 1 login",
            Success = true
        });
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            UserId = 2,
            Description = "User 2 login",
            Success = true
        });

        // Act
        var request = new SecurityLogSearchRequest { UserId = 1, Page = 1, PageSize = 10 };
        var result = await _auditService.GetSecurityLogsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.All(l => l.UserId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetSecurityLogsAsync_WithDateRange_ShouldReturnLogsInRange()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-7);
        var toDate = DateTime.Now.AddDays(1);

        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Recent log",
            Success = true,
            Timestamp = DateTime.Now
        });

        // Act
        var request = new SecurityLogSearchRequest { StartDate = fromDate, EndDate = toDate, Page = 1, PageSize = 10 };
        var result = await _auditService.GetSecurityLogsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.All(l => l.Timestamp >= fromDate && l.Timestamp <= toDate).Should().BeTrue();
    }

    [Fact]
    public async Task GetSecurityLogsAsync_WithEventTypeFilter_ShouldReturnMatchingLogs()
    {
        // Arrange
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Login event",
            Success = true
        });
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Logout,
            Severity = SecuritySeverity.Info,
            Description = "Logout event",
            Success = true
        });

        // Act
        var request = new SecurityLogSearchRequest { EventType = SecurityEventType.Login, Page = 1, PageSize = 10 };
        var result = await _auditService.GetSecurityLogsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.All(l => l.EventType == SecurityEventType.Login).Should().BeTrue();
    }

    [Fact]
    public async Task GetSecurityLogsAsync_OrderedByTimestampDescending()
    {
        // Arrange
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "First",
            Success = true,
            Timestamp = DateTime.Now.AddMinutes(-2)
        });
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Second",
            Success = true,
            Timestamp = DateTime.Now.AddMinutes(-1)
        });
        await _auditService.LogSecurityEventAsync(new SecurityLog
        {
            EventType = SecurityEventType.Login,
            Severity = SecuritySeverity.Info,
            Description = "Third",
            Success = true,
            Timestamp = DateTime.Now
        });

        // Act
        var request = new SecurityLogSearchRequest { Page = 1, PageSize = 10 };
        var result = await _auditService.GetSecurityLogsAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCountGreaterOrEqualTo(3);
        
        // Verify descending order
        for (int i = 0; i < result.Items.Count - 1; i++)
        {
            result.Items[i].Timestamp.Should().BeOnOrAfter(result.Items[i + 1].Timestamp);
        }
    }

    [Fact]
    public async Task LogSecurityEventAsync_CriticalSeverity_ShouldLog()
    {
        // Arrange
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.AccountLocked,
            Severity = SecuritySeverity.Critical,
            UserId = 1,
            Description = "Account locked after too many failed attempts",
            Success = true
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);

        // Assert
        var savedLog = await _context.SecurityLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.Severity.Should().Be(SecuritySeverity.Critical);
        savedLog.EventType.Should().Be(SecurityEventType.AccountLocked);
    }

    [Fact]
    public async Task LogSecurityEventAsync_UnauthorizedAccess_ShouldLog()
    {
        // Arrange
        var securityLog = new SecurityLog
        {
            EventType = SecurityEventType.UnauthorizedAccess,
            Severity = SecuritySeverity.Critical,
            Description = "Unauthorized access attempt",
            Success = false
        };

        // Act
        await _auditService.LogSecurityEventAsync(securityLog);

        // Assert
        var savedLog = await _context.SecurityLogs.FirstOrDefaultAsync();
        savedLog.Should().NotBeNull();
        savedLog!.Severity.Should().Be(SecuritySeverity.Critical);
        savedLog.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetSecurityLogsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            await _auditService.LogSecurityEventAsync(new SecurityLog
            {
                EventType = SecurityEventType.Login,
                Severity = SecuritySeverity.Info,
                Description = $"Log {i}",
                Success = true
            });
        }

        // Act
        var firstPageRequest = new SecurityLogSearchRequest { Page = 1, PageSize = 10 };
        var firstPage = await _auditService.GetSecurityLogsAsync(firstPageRequest);
        
        var secondPageRequest = new SecurityLogSearchRequest { Page = 2, PageSize = 10 };
        var secondPage = await _auditService.GetSecurityLogsAsync(secondPageRequest);

        // Assert
        firstPage.Items.Should().HaveCount(10);
        secondPage.Items.Should().HaveCount(10);
        firstPage.Items.Should().NotIntersectWith(secondPage.Items);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
