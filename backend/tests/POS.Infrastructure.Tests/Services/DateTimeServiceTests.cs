using FluentAssertions;
using POS.Infrastructure.Services;
using Xunit;

namespace POS.Infrastructure.Tests.Services;

/// <summary>
/// Tests for DateTimeService - Date and time utilities
/// </summary>
public class DateTimeServiceTests
{
    private readonly DateTimeService _dateTimeService;

    public DateTimeServiceTests()
    {
        _dateTimeService = new DateTimeService();
    }

    [Fact]
    public void Now_ShouldReturnCurrentDateTime()
    {
        // Arrange
        var before = DateTime.Now;

        // Act
        var result = _dateTimeService.Now;
        var after = DateTime.Now;

        // Assert
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcDateTime()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = _dateTimeService.UtcNow;
        var after = DateTime.UtcNow;

        // Assert
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Now_ShouldReturnCurrentDate()
    {
        // Act
        var result = _dateTimeService.Now.Date;

        // Assert
        result.Should().Be(DateTime.Today);
        result.TimeOfDay.Should().Be(TimeSpan.Zero); // Time should be midnight
    }

    [Fact]
    public void Now_MultipleCalls_ShouldReturnIncreasingValues()
    {
        // Act
        var time1 = _dateTimeService.Now;
        Thread.Sleep(10); // Small delay
        var time2 = _dateTimeService.Now;

        // Assert
        time2.Should().BeOnOrAfter(time1);
    }

    [Fact]
    public void UtcNow_MultipleCalls_ShouldReturnIncreasingValues()
    {
        // Act
        var time1 = _dateTimeService.UtcNow;
        Thread.Sleep(10); // Small delay
        var time2 = _dateTimeService.UtcNow;

        // Assert
        time2.Should().BeOnOrAfter(time1);
    }
}
