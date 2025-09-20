using POS.Application.DTOs.Reports;
using System;
using System.Threading.Tasks;

namespace POS.Application.Interfaces
{
    public interface IReportService
    {
        Task<SalesReportDto> GetSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<ProductPerformanceDto> GetProductPerformanceAsync(DateTime startDate, DateTime endDate);
        Task<ShiftReportDto> GetCurrentShiftReportAsync(int userId);
        Task<ShiftReportDto> GetShiftReportAsync(int shiftId);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<byte[]> ExportSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportProductPerformanceAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportShiftReportAsync(DateTime startDate, DateTime endDate);
    }
}
