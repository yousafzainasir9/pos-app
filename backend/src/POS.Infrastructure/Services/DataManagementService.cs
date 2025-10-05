using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using POS.Application.DTOs.DataManagement;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using System.Text;
using System.Text.Json;

namespace POS.Infrastructure.Services;

public class DataManagementService : IDataManagementService
{
    private readonly POSDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _backupPath;
    private readonly string _archivePath;

    public DataManagementService(
        POSDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        
        // Get backup paths from configuration or use defaults
        var basePath = configuration["DataManagement:BackupPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "backups");
        _backupPath = Path.Combine(basePath, "database");
        _archivePath = Path.Combine(basePath, "archives");
        
        // Ensure directories exist
        Directory.CreateDirectory(_backupPath);
        Directory.CreateDirectory(_archivePath);
    }

    #region Backup & Restore

    public async Task<BackupDto> CreateBackupAsync(BackupRequestDto request, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupName = string.IsNullOrEmpty(request.BackupName) 
            ? $"POSBackup_{timestamp}" 
            : $"{request.BackupName}_{timestamp}";
        
        var fileName = $"{backupName}.json";
        var filePath = Path.Combine(_backupPath, fileName);

        // Gather all data
        var backupData = new
        {
            BackupDate = DateTime.UtcNow,
            Version = "1.0",
            Data = new
            {
                Stores = await _context.Stores.ToListAsync(cancellationToken),
                Categories = await _context.Categories.ToListAsync(cancellationToken),
                Subcategories = await _context.Subcategories.ToListAsync(cancellationToken),
                Products = await _context.Products.ToListAsync(cancellationToken),
                Customers = await _context.Customers.ToListAsync(cancellationToken),
                Suppliers = await _context.Suppliers.ToListAsync(cancellationToken),
                Users = await _context.Users.ToListAsync(cancellationToken),
                Orders = await _context.Orders.ToListAsync(cancellationToken),
                OrderItems = await _context.OrderItems.ToListAsync(cancellationToken),
                Payments = await _context.Payments.ToListAsync(cancellationToken),
                Shifts = await _context.Shifts.ToListAsync(cancellationToken),
                InventoryTransactions = await _context.InventoryTransactions.ToListAsync(cancellationToken),
                AuditLogs = request.IncludeAuditLogs ? await _context.AuditLogs.ToListAsync(cancellationToken) : null,
                SecurityLogs = request.IncludeSecurityLogs ? await _context.SecurityLogs.ToListAsync(cancellationToken) : null
            }
        };

        // Serialize to JSON
        var json = JsonSerializer.Serialize(backupData, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });
        
        await File.WriteAllTextAsync(filePath, json, cancellationToken);

        var fileInfo = new FileInfo(filePath);
        
        return new BackupDto
        {
            FileName = fileName,
            CreatedAt = DateTime.UtcNow,
            SizeInBytes = fileInfo.Length,
            SizeFormatted = FormatBytes(fileInfo.Length),
            BackupType = "Full"
        };
    }

    public async Task<List<BackupDto>> GetBackupsAsync(CancellationToken cancellationToken = default)
    {
        var backups = new List<BackupDto>();
        
        if (!Directory.Exists(_backupPath))
            return backups;

        var files = Directory.GetFiles(_backupPath, "*.json");
        
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            backups.Add(new BackupDto
            {
                FileName = fileInfo.Name,
                CreatedAt = fileInfo.CreationTimeUtc,
                SizeInBytes = fileInfo.Length,
                SizeFormatted = FormatBytes(fileInfo.Length),
                BackupType = "Full"
            });
        }

        return await Task.FromResult(backups.OrderByDescending(b => b.CreatedAt).ToList());
    }

    public async Task RestoreBackupAsync(RestoreRequestDto request, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_backupPath, request.BackupFileName);
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Backup file not found", request.BackupFileName);

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        
        // Deserialize backup data
        // Note: In production, implement careful restore logic with transaction support
        throw new NotImplementedException("Restore functionality requires careful implementation to avoid data loss");
    }

    public async Task DeleteBackupAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_backupPath, fileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        await Task.CompletedTask;
    }

    #endregion

    #region Import

    public async Task<ImportResultDto> ImportProductsAsync(ImportProductsRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = new ImportResultDto();

        try
        {
            List<Product> products;

            if (request.FileType.ToLower() == "json")
            {
                products = JsonSerializer.Deserialize<List<Product>>(request.FileContent) ?? new List<Product>();
            }
            else // CSV
            {
                products = ParseProductsCsv(request.FileContent);
            }

            result.TotalRecords = products.Count;

            foreach (var product in products)
            {
                try
                {
                    var existing = await _context.Products
                        .FirstOrDefaultAsync(p => p.SKU == product.SKU, cancellationToken);

                    if (existing != null)
                    {
                        if (request.UpdateExisting)
                        {
                    existing.Name = product.Name;
                    existing.Description = product.Description;
                    existing.PriceIncGst = product.PriceIncGst;
                    existing.PriceExGst = product.PriceExGst;
                    existing.GstAmount = product.GstAmount;
                    existing.Cost = product.Cost;
                    existing.StockQuantity = product.StockQuantity;
                    // Update other fields as needed
                            
                            _context.Products.Update(existing);
                            result.UpdatedRecords++;
                        }
                        else
                        {
                            result.Warnings.Add($"Product with SKU {product.SKU} already exists - skipped");
                        }
                    }
                    else
                    {
                        await _context.Products.AddAsync(product, cancellationToken);
                        result.SuccessfulImports++;
                    }
                }
                catch (Exception ex)
                {
                    result.FailedImports++;
                    result.Errors.Add($"Error importing product {product.SKU}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }

    public async Task<ImportResultDto> ImportCustomersAsync(ImportCustomersRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = new ImportResultDto();

        try
        {
            List<Customer> customers;

            if (request.FileType.ToLower() == "json")
            {
                customers = JsonSerializer.Deserialize<List<Customer>>(request.FileContent) ?? new List<Customer>();
            }
            else // CSV
            {
                customers = ParseCustomersCsv(request.FileContent);
            }

            result.TotalRecords = customers.Count;

            foreach (var customer in customers)
            {
                try
                {
                    var existing = await _context.Customers
                        .FirstOrDefaultAsync(c => c.Email == customer.Email, cancellationToken);

                    if (existing != null)
                    {
                        if (request.UpdateExisting)
                        {
                            existing.FirstName = customer.FirstName;
                            existing.LastName = customer.LastName;
                            existing.Phone = customer.Phone;
                            // Update other fields
                            
                            _context.Customers.Update(existing);
                            result.UpdatedRecords++;
                        }
                        else
                        {
                            result.Warnings.Add($"Customer with email {customer.Email} already exists - skipped");
                        }
                    }
                    else
                    {
                        await _context.Customers.AddAsync(customer, cancellationToken);
                        result.SuccessfulImports++;
                    }
                }
                catch (Exception ex)
                {
                    result.FailedImports++;
                    result.Errors.Add($"Error importing customer {customer.Email}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }

    #endregion

    #region Export

    public async Task<ExportResultDto> ExportDataAsync(ExportDataRequestDto request, CancellationToken cancellationToken = default)
    {
        byte[] fileContent;
        string fileName;
        string contentType;
        int recordCount = 0;

        switch (request.EntityType.ToLower())
        {
            case "products":
                var products = await _context.Products
                    .ToListAsync(cancellationToken);
                
                recordCount = products.Count;
                fileName = $"Products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format}";
                
                if (request.Format == "json")
                {
                    fileContent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true }));
                    contentType = "application/json";
                }
                else
                {
                    fileContent = GenerateProductsCsv(products);
                    contentType = "text/csv";
                }
                break;

            case "customers":
                var customers = await _context.Customers.ToListAsync(cancellationToken);
                recordCount = customers.Count;
                fileName = $"Customers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format}";
                
                if (request.Format == "json")
                {
                    fileContent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(customers, new JsonSerializerOptions { WriteIndented = true }));
                    contentType = "application/json";
                }
                else
                {
                    fileContent = GenerateCustomersCsv(customers);
                    contentType = "text/csv";
                }
                break;

            case "orders":
                var ordersQuery = _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Payments)
                    .AsQueryable();

                if (request.StartDate.HasValue)
                    ordersQuery = ordersQuery.Where(o => o.OrderDate >= request.StartDate.Value);
                
                if (request.EndDate.HasValue)
                    ordersQuery = ordersQuery.Where(o => o.OrderDate <= request.EndDate.Value);

                if (request.StoreId.HasValue)
                    ordersQuery = ordersQuery.Where(o => o.StoreId == request.StoreId.Value);

                var orders = await ordersQuery.ToListAsync(cancellationToken);
                recordCount = orders.Count;
                fileName = $"Orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format}";
                
                if (request.Format == "json")
                {
                    fileContent = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orders, new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                    }));
                    contentType = "application/json";
                }
                else
                {
                    fileContent = GenerateOrdersCsv(orders);
                    contentType = "text/csv";
                }
                break;

            default:
                throw new ArgumentException($"Unknown entity type: {request.EntityType}");
        }

        return new ExportResultDto
        {
            FileContent = fileContent,
            FileName = fileName,
            ContentType = contentType,
            RecordCount = recordCount
        };
    }

    #endregion

    #region Statistics

    public async Task<DataStatisticsDto> GetDataStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var backups = await GetBackupsAsync(cancellationToken);
        
        return new DataStatisticsDto
        {
            TotalProducts = await _context.Products.CountAsync(cancellationToken),
            TotalCustomers = await _context.Customers.CountAsync(cancellationToken),
            TotalOrders = await _context.Orders.CountAsync(cancellationToken),
            TotalUsers = await _context.Users.CountAsync(cancellationToken),
            DatabaseSizeInBytes = 0, // Would need SQL query to get actual DB size
            DatabaseSizeFormatted = "N/A",
            LastBackupDate = backups.Any() ? backups.First().CreatedAt : DateTime.MinValue,
            BackupCount = backups.Count
        };
    }

    #endregion

    #region Archive

    public async Task<ArchiveResultDto> ArchiveDataAsync(ArchiveDataRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = new ArchiveResultDto();
        var archiveData = new Dictionary<string, object>();

        if (request.ArchiveOrders)
        {
            var oldOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Where(o => o.OrderDate < request.ArchiveBeforeDate)
                .ToListAsync(cancellationToken);

            archiveData["Orders"] = oldOrders;
            result.OrdersArchived = oldOrders.Count;

            if (request.DeleteAfterArchive)
            {
                _context.Orders.RemoveRange(oldOrders);
            }
        }

        if (request.ArchiveAuditLogs)
        {
            var oldAuditLogs = await _context.AuditLogs
                .Where(a => a.Timestamp < request.ArchiveBeforeDate)
                .ToListAsync(cancellationToken);

            archiveData["AuditLogs"] = oldAuditLogs;
            result.AuditLogsArchived = oldAuditLogs.Count;

            if (request.DeleteAfterArchive)
            {
                _context.AuditLogs.RemoveRange(oldAuditLogs);
            }
        }

        if (request.ArchiveSecurityLogs)
        {
            var oldSecurityLogs = await _context.SecurityLogs
                .Where(s => s.Timestamp < request.ArchiveBeforeDate)
                .ToListAsync(cancellationToken);

            archiveData["SecurityLogs"] = oldSecurityLogs;
            result.SecurityLogsArchived = oldSecurityLogs.Count;

            if (request.DeleteAfterArchive)
            {
                _context.SecurityLogs.RemoveRange(oldSecurityLogs);
            }
        }

        // Save archive
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var archiveFileName = $"Archive_{timestamp}.json";
        var archivePath = Path.Combine(_archivePath, archiveFileName);

        var json = JsonSerializer.Serialize(new
        {
            ArchiveDate = DateTime.UtcNow,
            ArchiveBeforeDate = request.ArchiveBeforeDate,
            Data = archiveData
        }, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });

        await File.WriteAllTextAsync(archivePath, json, cancellationToken);

        if (request.DeleteAfterArchive)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        var fileInfo = new FileInfo(archivePath);
        result.ArchiveFileName = archiveFileName;
        result.ArchiveSizeInBytes = fileInfo.Length;

        return result;
    }

    public async Task<List<BackupDto>> GetArchivesAsync(CancellationToken cancellationToken = default)
    {
        var archives = new List<BackupDto>();
        
        if (!Directory.Exists(_archivePath))
            return archives;

        var files = Directory.GetFiles(_archivePath, "*.json");
        
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            archives.Add(new BackupDto
            {
                FileName = fileInfo.Name,
                CreatedAt = fileInfo.CreationTimeUtc,
                SizeInBytes = fileInfo.Length,
                SizeFormatted = FormatBytes(fileInfo.Length),
                BackupType = "Archive"
            });
        }

        return await Task.FromResult(archives.OrderByDescending(a => a.CreatedAt).ToList());
    }

    #endregion

    #region Helper Methods

    private List<Product> ParseProductsCsv(string csvContent)
    {
        // Simple CSV parser - in production use a library like CsvHelper
        var products = new List<Product>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            var fields = lines[i].Split(',');
            if (fields.Length >= 5)
            {
                products.Add(new Product
                {
                    SKU = fields[0].Trim(),
                    Name = fields[1].Trim(),
                    Slug = fields[1].Trim().ToLower().Replace(" ", "-"),
                    Description = fields.Length > 2 ? fields[2].Trim() : "",
                    PriceIncGst = decimal.TryParse(fields[3].Trim(), out var price) ? price : 0,
                    PriceExGst = decimal.TryParse(fields[3].Trim(), out var priceEx) ? priceEx / 1.1m : 0,
                    GstAmount = decimal.TryParse(fields[3].Trim(), out var priceGst) ? priceGst - (priceGst / 1.1m) : 0,
                    Cost = fields.Length > 4 && decimal.TryParse(fields[4].Trim(), out var cost) ? cost : null,
                    SubcategoryId = 1 // Default subcategory - should be mapped properly
                });
            }
        }

        return products;
    }

    private List<Customer> ParseCustomersCsv(string csvContent)
    {
        var customers = new List<Customer>();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (fields.Length >= 4)
            {
                customers.Add(new Customer
                {
                    FirstName = fields[0].Trim(),
                    LastName = fields[1].Trim(),
                    Email = fields[2].Trim(),
                    Phone = fields[3].Trim()
                });
            }
        }

        return customers;
    }

    private byte[] GenerateProductsCsv(List<Product> products)
    {
        var csv = new StringBuilder();
        csv.AppendLine("SKU,Name,Description,PriceIncGst,PriceExGst,Cost,StockQuantity,SubcategoryId");

        foreach (var product in products)
        {
            csv.AppendLine($"\"{product.SKU}\",\"{product.Name}\",\"{product.Description}\",{product.PriceIncGst},{product.PriceExGst},{product.Cost},{product.StockQuantity},{product.SubcategoryId}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private byte[] GenerateCustomersCsv(List<Customer> customers)
    {
        var csv = new StringBuilder();
        csv.AppendLine("FirstName,LastName,Email,Phone,LoyaltyPoints");

        foreach (var customer in customers)
        {
            csv.AppendLine($"\"{customer.FirstName}\",\"{customer.LastName}\",\"{customer.Email}\",\"{customer.Phone}\",{customer.LoyaltyPoints}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private byte[] GenerateOrdersCsv(List<Order> orders)
    {
        var csv = new StringBuilder();
        csv.AppendLine("OrderNumber,OrderDate,TotalAmount,Status,CustomerName");

        foreach (var order in orders)
        {
            csv.AppendLine($"\"{order.OrderNumber}\",{order.OrderDate:yyyy-MM-dd HH:mm:ss},{order.TotalAmount},\"{order.Status}\",\"Customer\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    #endregion
}
