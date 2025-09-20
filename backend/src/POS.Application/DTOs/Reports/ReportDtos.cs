using System;
using System.Collections.Generic;

namespace POS.Application.DTOs.Reports
{
    public class SalesReportDto
    {
        public string Period { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<TopProductDto> TopProducts { get; set; }
        public List<SalesByDayDto> SalesByDay { get; set; }
        public List<PaymentMethodBreakdownDto> PaymentMethodBreakdown { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SalesByDayDto
    {
        public string Date { get; set; }
        public decimal Sales { get; set; }
        public int Orders { get; set; }
    }

    public class PaymentMethodBreakdownDto
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
    }

    public class ProductPerformanceDto
    {
        public List<TopSellingProductDto> TopSellingProducts { get; set; }
        public List<CategoryPerformanceDto> CategoryPerformance { get; set; }
        public List<LowStockProductDto> LowStockProducts { get; set; }
    }

    public class TopSellingProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class CategoryPerformanceDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class LowStockProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public string LastSoldDate { get; set; }
    }

    public class ShiftReportDto
    {
        public int ShiftId { get; set; }
        public string ShiftNumber { get; set; }
        public string CashierName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal StartingCash { get; set; }
        public decimal? EndingCash { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal CashSales { get; set; }
        public decimal CardSales { get; set; }
        public decimal OtherSales { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal? CashDifference { get; set; }
        public List<TransactionDto> Transactions { get; set; }
    }

    public class TransactionDto
    {
        public string OrderNumber { get; set; }
        public DateTime Time { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class DashboardStatsDto
    {
        public decimal TodaySales { get; set; }
        public int TodayOrders { get; set; }
        public decimal WeekSales { get; set; }
        public decimal MonthSales { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string TopSellingProduct { get; set; }
    }
}
