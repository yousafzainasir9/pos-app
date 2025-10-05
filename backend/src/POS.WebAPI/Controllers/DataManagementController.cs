using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.DataManagement;
using POS.Application.Interfaces;

namespace POS.WebAPI.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class DataManagementController : ControllerBase
{
    private readonly IDataManagementService _dataManagementService;

    public DataManagementController(IDataManagementService dataManagementService)
    {
        _dataManagementService = dataManagementService;
    }

    /// <summary>
    /// Get data statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<DataStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DataStatisticsDto>>> GetStatistics(CancellationToken cancellationToken)
    {
        var stats = await _dataManagementService.GetDataStatisticsAsync(cancellationToken);
        return Ok(ApiResponse<DataStatisticsDto>.SuccessResponse(stats));
    }

    #region Backup & Restore

    /// <summary>
    /// Create a new database backup
    /// </summary>
    [HttpPost("backup")]
    [ProducesResponseType(typeof(ApiResponse<BackupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BackupDto>>> CreateBackup(
        [FromBody] BackupRequestDto request,
        CancellationToken cancellationToken)
    {
        var backup = await _dataManagementService.CreateBackupAsync(request, cancellationToken);
        return Ok(ApiResponse<BackupDto>.SuccessResponse(backup, "Backup created successfully"));
    }

    /// <summary>
    /// Get all backups
    /// </summary>
    [HttpGet("backups")]
    [ProducesResponseType(typeof(ApiResponse<List<BackupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BackupDto>>>> GetBackups(CancellationToken cancellationToken)
    {
        var backups = await _dataManagementService.GetBackupsAsync(cancellationToken);
        return Ok(ApiResponse<List<BackupDto>>.SuccessResponse(backups));
    }

    /// <summary>
    /// Delete a backup
    /// </summary>
    [HttpDelete("backups/{fileName}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> DeleteBackup(string fileName, CancellationToken cancellationToken)
    {
        await _dataManagementService.DeleteBackupAsync(fileName, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Backup deleted successfully"));
    }

    #endregion

    #region Import

    /// <summary>
    /// Import products from CSV or JSON
    /// </summary>
    [HttpPost("import/products")]
    [ProducesResponseType(typeof(ApiResponse<ImportResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportProducts(
        [FromBody] ImportProductsRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _dataManagementService.ImportProductsAsync(request, cancellationToken);
        return Ok(ApiResponse<ImportResultDto>.SuccessResponse(result, "Import completed"));
    }

    /// <summary>
    /// Import customers from CSV or JSON
    /// </summary>
    [HttpPost("import/customers")]
    [ProducesResponseType(typeof(ApiResponse<ImportResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportCustomers(
        [FromBody] ImportCustomersRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _dataManagementService.ImportCustomersAsync(request, cancellationToken);
        return Ok(ApiResponse<ImportResultDto>.SuccessResponse(result, "Import completed"));
    }

    #endregion

    #region Export

    /// <summary>
    /// Export data to CSV or JSON
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportData(
        [FromBody] ExportDataRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _dataManagementService.ExportDataAsync(request, cancellationToken);
        return File(result.FileContent, result.ContentType, result.FileName);
    }

    #endregion

    #region Archive

    /// <summary>
    /// Archive old data
    /// </summary>
    [HttpPost("archive")]
    [ProducesResponseType(typeof(ApiResponse<ArchiveResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ArchiveResultDto>>> ArchiveData(
        [FromBody] ArchiveDataRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _dataManagementService.ArchiveDataAsync(request, cancellationToken);
        return Ok(ApiResponse<ArchiveResultDto>.SuccessResponse(result, "Data archived successfully"));
    }

    /// <summary>
    /// Get all archives
    /// </summary>
    [HttpGet("archives")]
    [ProducesResponseType(typeof(ApiResponse<List<BackupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BackupDto>>>> GetArchives(CancellationToken cancellationToken)
    {
        var archives = await _dataManagementService.GetArchivesAsync(cancellationToken);
        return Ok(ApiResponse<List<BackupDto>>.SuccessResponse(archives));
    }

    #endregion
}
