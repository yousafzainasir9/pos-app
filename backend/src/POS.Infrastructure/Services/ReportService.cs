using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs.Reports;
using POS.Application.Interfaces;
using POS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly POSDbContext _context;

        public ReportService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<SalesReportDto> GetSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            // Ensure we're working with date boundaries
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddSeconds(-1);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync();

            var report = new SalesReportDto
            {
                Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                TotalSales = orders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count,
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                TopProducts = GetTopProducts(orders),
                SalesByDay = GetSalesByDay(orders, startDate, endDate),
                PaymentMethodBreakdown = GetPaymentMethodBreakdown(orders)
            };

            return report;
        }

        public async Task<ProductPerformanceDto> GetProductPerformanceAsync(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddSeconds(-1);

            var orderItems = await _context.OrderItems
                .Include(i => i.Product)
                    .ThenInclude(p => p.Subcategory)
                        .ThenInclude(s => s.Category)
                .Include(i => i.Order)
                .Where(i => i.Order.OrderDate >= startDate && i.Order.OrderDate <= endDate)
                .ToListAsync();

            var products = await _context.Products
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .ToListAsync();

            var report = new ProductPerformanceDto
            {
                TopSellingProducts = GetTopSellingProducts(orderItems),
                CategoryPerformance = GetCategoryPerformance(orderItems),
                LowStockProducts = GetLowStockProducts(products)
            };

            return report;
        }

        public async Task<ShiftReportDto> GetCurrentShiftReportAsync(int userId)
        {
            var shift = await _context.Shifts
                .Include(s => s.User)
                .Include(s => s.Orders)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(s => s.UserId == (long)userId && s.Status == Domain.Enums.ShiftStatus.Open);

            if (shift == null)
                return null;

            return CreateShiftReport(shift);
        }

        public async Task<ShiftReportDto> GetShiftReportAsync(int shiftId)
        {
            var shift = await _context.Shifts
                .Include(s => s.User)
                .Include(s => s.Orders)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(s => s.Id == (long)shiftId);

            if (shift == null)
                return null;

            return CreateShiftReport(shift);
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var todayOrders = await _context.Orders
                .Where(o => o.OrderDate >= today)
                .ToListAsync();

            var weekOrders = await _context.Orders
                .Where(o => o.OrderDate >= startOfWeek)
                .ToListAsync();

            var monthOrders = await _context.Orders
                .Where(o => o.OrderDate >= startOfMonth)
                .ToListAsync();

            var activeProducts = await _context.Products
                .CountAsync(p => p.IsActive);

            var lowStockProducts = await _context.Products
                .CountAsync(p => p.TrackInventory && p.StockQuantity <= p.LowStockThreshold);

            var topProduct = await _context.OrderItems
                .Include(i => i.Product)
                .Where(i => i.Order.OrderDate >= startOfMonth)
                .GroupBy(i => i.Product.Name)
                .OrderByDescending(g => g.Sum(i => i.Quantity))
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            return new DashboardStatsDto
            {
                TodaySales = todayOrders.Sum(o => o.TotalAmount),
                TodayOrders = todayOrders.Count,
                WeekSales = weekOrders.Sum(o => o.TotalAmount),
                MonthSales = monthOrders.Sum(o => o.TotalAmount),
                ActiveProducts = activeProducts,
                LowStockCount = lowStockProducts,
                AverageOrderValue = monthOrders.Any() ? monthOrders.Average(o => o.TotalAmount) : 0,
                TopSellingProduct = topProduct ?? "N/A"
            };
        }

        public async Task<byte[]> ExportSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = await GetSalesReportAsync(startDate, endDate);
            var csv = new StringBuilder();
            
            csv.AppendLine("Sales Report");
            csv.AppendLine($"Period,{report.Period}");
            csv.AppendLine($"Total Sales,{report.TotalSales}");
            csv.AppendLine($"Total Orders,{report.TotalOrders}");
            csv.AppendLine($"Average Order Value,{report.AverageOrderValue}");
            csv.AppendLine();
            
            csv.AppendLine("Top Products");
            csv.AppendLine("Product,Quantity Sold,Revenue");
            foreach (var product in report.TopProducts)
            {
                csv.AppendLine($"{product.ProductName},{product.QuantitySold},{product.Revenue}");
            }
            csv.AppendLine();
            
            csv.AppendLine("Daily Sales");
            csv.AppendLine("Date,Sales,Orders");
            foreach (var day in report.SalesByDay)
            {
                csv.AppendLine($"{day.Date},{day.Sales},{day.Orders}");
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportProductPerformanceAsync(DateTime startDate, DateTime endDate)
        {
            var report = await GetProductPerformanceAsync(startDate, endDate);
            var csv = new StringBuilder();
            
            csv.AppendLine("Product Performance Report");
            csv.AppendLine($"Period,{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            csv.AppendLine();
            
            csv.AppendLine("Top Selling Products");
            csv.AppendLine("Name,SKU,Quantity Sold,Revenue,Profit Margin");
            foreach (var product in report.TopSellingProducts)
            {
                csv.AppendLine($"{product.Name},{product.Sku},{product.QuantitySold},{product.Revenue},{product.ProfitMargin}%");
            }
            csv.AppendLine();
            
            csv.AppendLine("Category Performance");
            csv.AppendLine("Category,Total Sales,Total Revenue");
            foreach (var category in report.CategoryPerformance)
            {
                csv.AppendLine($"{category.CategoryName},{category.TotalSales},{category.TotalRevenue}");
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportShiftReportAsync(DateTime startDate, DateTime endDate)
        {
            var shifts = await _context.Shifts
                .Include(s => s.User)
                .Include(s => s.Orders)
                .Where(s => s.StartTime >= startDate && s.StartTime <= endDate)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Shift Report");
            csv.AppendLine($"Period,{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            csv.AppendLine();
            csv.AppendLine("Shift Number,Cashier,Start Time,End Time,Total Sales,Total Orders,Cash Sales,Card Sales");
            
            foreach (var shift in shifts)
            {
                csv.AppendLine($"{shift.ShiftNumber},{shift.User.FirstName} {shift.User.LastName},{shift.StartTime},{shift.EndTime},{shift.TotalSales},{shift.TotalOrders},{shift.CashSales},{shift.CardSales}");
            }
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        private List<TopProductDto> GetTopProducts(List<Domain.Entities.Order> orders)
        {
            return orders
                .SelectMany(o => o.OrderItems)
                .Where(i => i.Product != null)
                .GroupBy(i => new { i.ProductId, ProductName = i.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = (int)g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    QuantitySold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalAmount)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList();
        }

        private List<SalesByDayDto> GetSalesByDay(List<Domain.Entities.Order> orders, DateTime startDate, DateTime endDate)
        {
            var days = new List<SalesByDayDto>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayOrders = orders.Where(o => o.OrderDate.Date == currentDate).ToList();
                days.Add(new SalesByDayDto
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    Sales = dayOrders.Sum(o => o.TotalAmount),
                    Orders = dayOrders.Count
                });
                currentDate = currentDate.AddDays(1);
            }

            return days;
        }

        private List<PaymentMethodBreakdownDto> GetPaymentMethodBreakdown(List<Domain.Entities.Order> orders)
        {
            return orders
                .SelectMany(o => o.Payments)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodBreakdownDto
                {
                    Method = g.Key.ToString(),
                    Count = g.Count(),
                    Total = g.Sum(p => p.Amount)
                })
                .OrderByDescending(p => p.Total)
                .ToList();
        }

        private List<TopSellingProductDto> GetTopSellingProducts(List<Domain.Entities.OrderItem> orderItems)
        {
            return orderItems
                .GroupBy(i => new { i.ProductId, ProductName = i.Product?.Name ?? "Unknown", ProductSKU = i.Product?.SKU ?? "" })
                .Select(g =>
                {
                    var revenue = g.Sum(i => i.TotalAmount);
                    var cost = g.Sum(i => i.Quantity * (i.Product?.Cost ?? 0));
                    var profitMargin = revenue > 0 ? ((revenue - cost) / revenue * 100) : 0;

                    return new TopSellingProductDto
                    {
                        Id = (int)g.Key.ProductId,
                        Name = g.Key.ProductName,
                        Sku = g.Key.ProductSKU,
                        QuantitySold = g.Sum(i => i.Quantity),
                        Revenue = revenue,
                        ProfitMargin = Math.Round(profitMargin, 2)
                    };
                })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList();
        }

        private List<CategoryPerformanceDto> GetCategoryPerformance(List<Domain.Entities.OrderItem> orderItems)
        {
            return orderItems
                .Where(i => i.Product?.Subcategory?.Category != null)
                .GroupBy(i => new { CategoryId = i.Product.Subcategory.Category.Id, CategoryName = i.Product.Subcategory.Category.Name })
                .Select(g => new CategoryPerformanceDto
                {
                    CategoryId = (int)g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalSales = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.TotalAmount)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToList();
        }

        private List<LowStockProductDto> GetLowStockProducts(List<Domain.Entities.Product> products)
        {
            return products
                .Where(p => p.TrackInventory && p.StockQuantity <= p.LowStockThreshold)
                .Select(p => new LowStockProductDto
                {
                    Id = (int)p.Id,
                    Name = p.Name,
                    CurrentStock = p.StockQuantity,
                    Threshold = p.LowStockThreshold,
                    LastSoldDate = "N/A" // Would need to track this separately
                })
                .OrderBy(p => p.CurrentStock)
                .Take(10)
                .ToList();
        }

        private ShiftReportDto CreateShiftReport(Domain.Entities.Shift shift)
        {
            var cashSales = shift.Orders
                .SelectMany(o => o.Payments)
                .Where(p => p.PaymentMethod == Domain.Enums.PaymentMethod.Cash)
                .Sum(p => p.Amount);

            var cardSales = shift.Orders
                .SelectMany(o => o.Payments)
                .Where(p => p.PaymentMethod == Domain.Enums.PaymentMethod.CreditCard || 
                           p.PaymentMethod == Domain.Enums.PaymentMethod.DebitCard)
                .Sum(p => p.Amount);

            var otherSales = shift.Orders
                .SelectMany(o => o.Payments)
                .Where(p => p.PaymentMethod != Domain.Enums.PaymentMethod.Cash && 
                           p.PaymentMethod != Domain.Enums.PaymentMethod.CreditCard &&
                           p.PaymentMethod != Domain.Enums.PaymentMethod.DebitCard)
                .Sum(p => p.Amount);

            var expectedCash = shift.StartingCash + cashSales;
            var cashDifference = shift.EndingCash.HasValue ? shift.EndingCash.Value - expectedCash : (decimal?)null;

            var transactions = shift.Orders
                .SelectMany(o => o.Payments.Select(p => new TransactionDto
                {
                    OrderNumber = o.OrderNumber,
                    Time = o.OrderDate,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString()
                }))
                .OrderByDescending(t => t.Time)
                .Take(50)
                .ToList();

            return new ShiftReportDto
            {
                ShiftId = (int)shift.Id,
                ShiftNumber = shift.ShiftNumber,
                CashierName = $"{shift.User.FirstName} {shift.User.LastName}",
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                StartingCash = shift.StartingCash,
                EndingCash = shift.EndingCash,
                TotalSales = shift.TotalSales ?? 0,
                TotalOrders = shift.TotalOrders ?? 0,
                CashSales = cashSales,
                CardSales = cardSales,
                OtherSales = otherSales,
                ExpectedCash = expectedCash,
                CashDifference = cashDifference,
                Transactions = transactions
            };
        }
    }
}
