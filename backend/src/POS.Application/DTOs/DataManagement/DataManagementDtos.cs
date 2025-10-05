namespace POS.Application.DTOs.DataManagement;

public class BackupDto
{
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeInBytes { get; set; }
    public string SizeFormatted { get; set; } = string.Empty;
    public string BackupType { get; set; } = string.Empty; // Full, Partial
}

public class BackupRequestDto
{
    public string BackupName { get; set; } = string.Empty;
    public bool IncludeAuditLogs { get; set; } = true;
    public bool IncludeSecurityLogs { get; set; } = true;
}

public class RestoreRequestDto
{
    public string BackupFileName { get; set; } = string.Empty;
}

public class ImportProductsRequestDto
{
    public string FileContent { get; set; } = string.Empty; // CSV or JSON content
    public string FileType { get; set; } = "csv"; // csv or json
    public bool UpdateExisting { get; set; } = false;
}

public class ImportCustomersRequestDto
{
    public string FileContent { get; set; } = string.Empty;
    public string FileType { get; set; } = "csv";
    public bool UpdateExisting { get; set; } = false;
}

public class ExportDataRequestDto
{
    public string EntityType { get; set; } = string.Empty; // Products, Orders, Customers, etc.
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Format { get; set; } = "csv"; // csv, json, excel
    public long? StoreId { get; set; }
}

public class ImportResultDto
{
    public int TotalRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public int UpdatedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ExportResultDto
{
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
}

public class DataStatisticsDto
{
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public int TotalUsers { get; set; }
    public long DatabaseSizeInBytes { get; set; }
    public string DatabaseSizeFormatted { get; set; } = string.Empty;
    public DateTime LastBackupDate { get; set; }
    public int BackupCount { get; set; }
}

public class ArchiveDataRequestDto
{
    public DateTime ArchiveBeforeDate { get; set; }
    public bool ArchiveOrders { get; set; } = true;
    public bool ArchiveAuditLogs { get; set; } = true;
    public bool ArchiveSecurityLogs { get; set; } = false;
    public bool DeleteAfterArchive { get; set; } = false;
}

public class ArchiveResultDto
{
    public int OrdersArchived { get; set; }
    public int AuditLogsArchived { get; set; }
    public int SecurityLogsArchived { get; set; }
    public string ArchiveFileName { get; set; } = string.Empty;
    public long ArchiveSizeInBytes { get; set; }
}
