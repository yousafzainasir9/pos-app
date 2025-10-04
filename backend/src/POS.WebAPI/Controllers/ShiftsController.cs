using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Models;
using POS.Application.DTOs.Shifts;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data.Interceptors;

namespace POS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShiftsController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ShiftsController(
        IUnitOfWork unitOfWork,
        ILogger<ShiftsController> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    [HttpPost("open")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> OpenShift([FromBody] OpenShiftDto request)
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(currentUserId);
            
            if (user == null || user.StoreId == null)
            {
                return BadRequest(ApiResponse<ShiftDto>.ErrorResponse(
                    new ErrorResponse("INVALID_USER", "User not associated with a store")));
            }

            // Check for existing open shift
            var existingShift = await _unitOfWork.Repository<Shift>().Query()
                .FirstOrDefaultAsync(s => s.UserId == currentUserId && s.Status == ShiftStatus.Open);

            if (existingShift != null)
            {
                return BadRequest(ApiResponse<ShiftDto>.ErrorResponse(
                    new ErrorResponse("SHIFT_ALREADY_OPEN", "User already has an open shift")));
            }

            // Generate shift number
            var shiftNumber = $"SH{DateTime.Now:yyyyMMddHHmmss}";

            // Create new shift
            var shift = new Shift
            {
                ShiftNumber = shiftNumber,
                StartTime = DateTime.UtcNow,
                StartingCash = request.StartingCash,
                Status = ShiftStatus.Open,
                UserId = currentUserId,
                StoreId = user.StoreId.Value,
                Notes = request.Notes
            };

            await _unitOfWork.Repository<Shift>().AddAsync(shift);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<ShiftDto>.SuccessResponse(new ShiftDto
            {
                Id = shift.Id,
                ShiftNumber = shift.ShiftNumber,
                StartTime = shift.StartTime,
                StartingCash = shift.StartingCash,
                Status = shift.Status.ToString()
            }, "Shift opened successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening shift");
            return StatusCode(500, ApiResponse<ShiftDto>.ErrorResponse(
                new ErrorResponse("INTERNAL_ERROR", "An error occurred while opening the shift")));
        }
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<ApiResponse<ShiftSummaryDto>>> CloseShift(long id, [FromBody] CloseShiftDto request)
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
            
            var shift = await _unitOfWork.Repository<Shift>().Query()
                .Include(s => s.Orders)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound(ApiResponse<ShiftSummaryDto>.ErrorResponse(
                    new ErrorResponse("NOT_FOUND", "Shift not found")));
            }

            if (shift.UserId != currentUserId && !User.IsInRole(UserRole.Manager.ToString()))
            {
                return Forbid();
            }

            if (shift.Status != ShiftStatus.Open)
            {
                return BadRequest(ApiResponse<ShiftSummaryDto>.ErrorResponse(
                    new ErrorResponse("INVALID_SHIFT_STATUS", "Shift is not open")));
            }

            // Calculate shift totals
            var completedOrders = shift.Orders.Where(o => o.Status == OrderStatus.Completed).ToList();
            var cashSales = shift.Orders
                .SelectMany(o => o.Payments)
                .Where(p => p.PaymentMethod == PaymentMethod.Cash && p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);
            
            var cardSales = shift.Orders
                .SelectMany(o => o.Payments)
                .Where(p => (p.PaymentMethod == PaymentMethod.CreditCard || p.PaymentMethod == PaymentMethod.DebitCard) 
                        && p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);
            
            var otherSales = shift.Orders
                .SelectMany(o => o.Payments)
                .Where(p => p.PaymentMethod != PaymentMethod.Cash 
                        && p.PaymentMethod != PaymentMethod.CreditCard 
                        && p.PaymentMethod != PaymentMethod.DebitCard
                        && p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);

            // Update shift
            shift.EndTime = DateTime.UtcNow;
            shift.EndingCash = request.EndingCash;
            shift.CashSales = cashSales;
            shift.CardSales = cardSales;
            shift.OtherSales = otherSales;
            shift.TotalSales = cashSales + cardSales + otherSales;
            shift.TotalOrders = completedOrders.Count;
            shift.Status = ShiftStatus.Closed;
            shift.ClosedByUserId = currentUserId;
            shift.Notes = string.IsNullOrWhiteSpace(shift.Notes) 
                ? request.Notes 
                : $"{shift.Notes}\n\nClosing Notes: {request.Notes}";

            _unitOfWork.Repository<Shift>().Update(shift);
            await _unitOfWork.SaveChangesAsync();

            // Calculate cash difference
            var expectedCash = shift.StartingCash + shift.CashSales.GetValueOrDefault();
            var cashDifference = shift.EndingCash.GetValueOrDefault() - expectedCash;

            return Ok(ApiResponse<ShiftSummaryDto>.SuccessResponse(new ShiftSummaryDto
            {
                Id = shift.Id,
                ShiftNumber = shift.ShiftNumber,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime.Value,
                StartingCash = shift.StartingCash,
                EndingCash = shift.EndingCash.Value,
                ExpectedCash = expectedCash,
                CashDifference = cashDifference,
                CashSales = shift.CashSales.Value,
                CardSales = shift.CardSales.Value,
                OtherSales = shift.OtherSales.Value,
                TotalSales = shift.TotalSales.Value,
                TotalOrders = shift.TotalOrders.Value,
                Status = shift.Status.ToString()
            }, "Shift closed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing shift");
            return StatusCode(500, ApiResponse<ShiftSummaryDto>.ErrorResponse(
                new ErrorResponse("INTERNAL_ERROR", "An error occurred while closing the shift")));
        }
    }

    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<ShiftDto>>> GetCurrentShift()
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
            
            var shift = await _unitOfWork.Repository<Shift>().Query()
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.UserId == currentUserId && s.Status == ShiftStatus.Open);

            if (shift == null)
            {
                return NotFound(ApiResponse<ShiftDto>.ErrorResponse(
                    new ErrorResponse("NOT_FOUND", "No active shift found")));
            }

            var completedOrders = shift.Orders.Where(o => o.Status == OrderStatus.Completed).ToList();
            var totalSales = completedOrders.Sum(o => o.TotalAmount);

            return Ok(ApiResponse<ShiftDto>.SuccessResponse(new ShiftDto
            {
                Id = shift.Id,
                ShiftNumber = shift.ShiftNumber,
                StartTime = shift.StartTime,
                StartingCash = shift.StartingCash,
                Status = shift.Status.ToString(),
                TotalOrders = completedOrders.Count,
                TotalSales = totalSales
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current shift");
            return StatusCode(500, ApiResponse<ShiftDto>.ErrorResponse(
                new ErrorResponse("INTERNAL_ERROR", "An error occurred while retrieving the shift")));
        }
    }

    [HttpGet("{id}/report")]
    public async Task<ActionResult<ApiResponse<ShiftReportDto>>> GetShiftReport(long id)
    {
        try
        {
            var shift = await _unitOfWork.Repository<Shift>().Query()
                .Include(s => s.User)
                .Include(s => s.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .Include(s => s.Orders)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound(ApiResponse<ShiftReportDto>.ErrorResponse(
                    new ErrorResponse("NOT_FOUND", "Shift not found")));
            }

            var report = new ShiftReportDto
            {
                ShiftId = shift.Id,
                ShiftNumber = shift.ShiftNumber,
                CashierName = shift.User.FullName,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Status = shift.Status.ToString(),
                StartingCash = shift.StartingCash,
                EndingCash = shift.EndingCash,
                
                // Order summary
                TotalOrders = shift.Orders.Count,
                CompletedOrders = shift.Orders.Count(o => o.Status == OrderStatus.Completed),
                CancelledOrders = shift.Orders.Count(o => o.Status == OrderStatus.Cancelled),
                
                // Sales by payment method
                CashSales = shift.Orders
                    .SelectMany(o => o.Payments)
                    .Where(p => p.PaymentMethod == PaymentMethod.Cash && p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount),
                
                CardSales = shift.Orders
                    .SelectMany(o => o.Payments)
                    .Where(p => (p.PaymentMethod == PaymentMethod.CreditCard || p.PaymentMethod == PaymentMethod.DebitCard) 
                            && p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount),
                
                // Top selling products
                TopProducts = shift.Orders
                    .SelectMany(o => o.OrderItems)
                    .Where(oi => !oi.IsVoided)
                    .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                    .Select(g => new ProductSalesDto
                    {
                        ProductName = g.Key.Name,
                        Quantity = g.Sum(oi => oi.Quantity),
                        TotalSales = g.Sum(oi => oi.TotalAmount)
                    })
                    .OrderByDescending(p => p.TotalSales)
                    .Take(10)
                    .ToList(),
                
                // Hourly sales
                HourlySales = shift.Orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .GroupBy(o => o.OrderDate.Hour)
                    .Select(g => new HourlySalesDto
                    {
                        Hour = g.Key,
                        OrderCount = g.Count(),
                        TotalSales = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(h => h.Hour)
                    .ToList()
            };

            report.TotalSales = report.CashSales + report.CardSales;

            return Ok(ApiResponse<ShiftReportDto>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating shift report");
            return StatusCode(500, ApiResponse<ShiftReportDto>.ErrorResponse(
                new ErrorResponse("INTERNAL_ERROR", "An error occurred while generating the report")));
        }
    }
}
