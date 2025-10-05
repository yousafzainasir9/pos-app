using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Common.Models;
using POS.Application.DTOs.Settings;
using POS.Application.Interfaces;

namespace POS.WebAPI.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;

    public SystemSettingsController(ISystemSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// Get all system settings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AllSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AllSettingsDto>>> GetAllSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetAllSettingsAsync(cancellationToken);
        return Ok(ApiResponse<AllSettingsDto>.SuccessResponse(settings));
    }

    /// <summary>
    /// Get general settings
    /// </summary>
    [HttpGet("general")]
    [ProducesResponseType(typeof(ApiResponse<GeneralSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GeneralSettingsDto>>> GetGeneralSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetGeneralSettingsAsync(cancellationToken);
        return Ok(ApiResponse<GeneralSettingsDto>.SuccessResponse(settings));
    }

    /// <summary>
    /// Update general settings
    /// </summary>
    [HttpPut("general")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> UpdateGeneralSettings(
        [FromBody] GeneralSettingsDto settings,
        CancellationToken cancellationToken)
    {
        await _settingsService.UpdateGeneralSettingsAsync(settings, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("General settings updated successfully"));
    }

    /// <summary>
    /// Get receipt settings - Admin only
    /// </summary>
    [HttpGet("receipt")]
    [ProducesResponseType(typeof(ApiResponse<ReceiptSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ReceiptSettingsDto>>> GetReceiptSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetReceiptSettingsAsync(cancellationToken);
        return Ok(ApiResponse<ReceiptSettingsDto>.SuccessResponse(settings));
    }

    /// <summary>
    /// Update receipt settings - Admin only
    /// </summary>
    [HttpPut("receipt")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> UpdateReceiptSettings(
        [FromBody] ReceiptSettingsDto settings,
        CancellationToken cancellationToken)
    {
        await _settingsService.UpdateReceiptSettingsAsync(settings, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Receipt settings updated successfully"));
    }

    /// <summary>
    /// Get email settings
    /// </summary>
    [HttpGet("email")]
    [ProducesResponseType(typeof(ApiResponse<EmailSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EmailSettingsDto>>> GetEmailSettings(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetEmailSettingsAsync(cancellationToken);
        return Ok(ApiResponse<EmailSettingsDto>.SuccessResponse(settings));
    }

    /// <summary>
    /// Update email settings
    /// </summary>
    [HttpPut("email")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> UpdateEmailSettings(
        [FromBody] EmailSettingsDto settings,
        CancellationToken cancellationToken)
    {
        await _settingsService.UpdateEmailSettingsAsync(settings, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Email settings updated successfully"));
    }

    /// <summary>
    /// Test email settings
    /// </summary>
    [HttpPost("email/test")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> TestEmailSettings(
        [FromBody] EmailSettingsDto settings,
        [FromQuery] string testEmail,
        CancellationToken cancellationToken)
    {
        var success = await _settingsService.TestEmailSettingsAsync(settings, testEmail, cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(success, success ? "Test email sent successfully" : "Failed to send test email"));
    }

    /// <summary>
    /// Get default values
    /// </summary>
    [HttpGet("defaults")]
    [ProducesResponseType(typeof(ApiResponse<DefaultValuesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DefaultValuesDto>>> GetDefaultValues(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetDefaultValuesAsync(cancellationToken);
        return Ok(ApiResponse<DefaultValuesDto>.SuccessResponse(settings));
    }

    /// <summary>
    /// Update default values
    /// </summary>
    [HttpPut("defaults")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> UpdateDefaultValues(
        [FromBody] DefaultValuesDto settings,
        CancellationToken cancellationToken)
    {
        await _settingsService.UpdateDefaultValuesAsync(settings, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Default values updated successfully"));
    }

    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> ResetToDefaults(CancellationToken cancellationToken)
    {
        await _settingsService.ResetToDefaultsAsync(cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Settings reset to defaults"));
    }
}
