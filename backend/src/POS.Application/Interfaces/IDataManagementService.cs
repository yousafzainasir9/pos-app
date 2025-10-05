using POS.Application.DTOs.DataManagement;

namespace POS.Application.Interfaces;

public interface IDataManagementService
{
    // Backup & Restore
    Task<BackupDto> CreateBackupAsync(BackupRequestDto request, CancellationToken cancellationToken = default);
    Task<List<BackupDto>> GetBackupsAsync(CancellationToken cancellationToken = default);
    Task RestoreBackupAsync(RestoreRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteBackupAsync(string fileName, CancellationToken cancellationToken = default);
    
    // Import
    Task<ImportResultDto> ImportProductsAsync(ImportProductsRequestDto request, CancellationToken cancellationToken = default);
    Task<ImportResultDto> ImportCustomersAsync(ImportCustomersRequestDto request, CancellationToken cancellationToken = default);
    
    // Export
    Task<ExportResultDto> ExportDataAsync(ExportDataRequestDto request, CancellationToken cancellationToken = default);
    
    // Statistics
    Task<DataStatisticsDto> GetDataStatisticsAsync(CancellationToken cancellationToken = default);
    
    // Archive
    Task<ArchiveResultDto> ArchiveDataAsync(ArchiveDataRequestDto request, CancellationToken cancellationToken = default);
    Task<List<BackupDto>> GetArchivesAsync(CancellationToken cancellationToken = default);
}
