using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POS.Application.Common.Constants;
using POS.Application.Common.Exceptions;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Models;
using POS.Application.DTOs.Auth;
using POS.Application.Interfaces;
using POS.Domain.Entities;
using POS.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace POS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly ISecurityService _securityService;

    public AuthController(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        ISecurityService securityService)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _securityService = securityService;
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication token and user information</returns>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>().Query()
                .Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
                throw AuthenticationException.InvalidCredentials();
            }

            var token = GenerateJwtToken(user);
            var refreshToken = _securityService.GenerateSecureToken(AuthConstants.RefreshTokenByteSize);
            var refreshTokenHash = _securityService.HashToken(refreshToken);

            // Update user with refresh token hash
            user.RefreshToken = refreshTokenHash;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(AuthConstants.RefreshTokenExpiryDays);
            user.LastLoginAt = DateTime.UtcNow;

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {Username} logged in successfully", user.Username);

            var response = new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = AuthConstants.AccessTokenExpiryMinutes * 60,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.ToString(),
                    StoreId = user.StoreId,
                    StoreName = user.Store?.Name
                }
            };

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful"));
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            throw;
        }
    }

    /// <summary>
    /// Authenticate user with PIN code
    /// </summary>
    /// <param name="request">PIN login credentials</param>
    /// <returns>Authentication token and user information</returns>
    [HttpPost("pin-login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> PinLogin([FromBody] PinLoginRequestDto request)
    {
        try
        {
            var user = await _unitOfWork.Repository<User>().Query()
                .Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.Pin == request.Pin && u.IsActive && u.StoreId == request.StoreId);

            if (user == null)
            {
                _logger.LogWarning("Failed PIN login attempt for store: {StoreId}", request.StoreId);
                throw AuthenticationException.InvalidPin();
            }

            var token = GenerateJwtToken(user);
            var refreshToken = _securityService.GenerateSecureToken(AuthConstants.RefreshTokenByteSize);
            var refreshTokenHash = _securityService.HashToken(refreshToken);

            // Update user with refresh token hash
            user.RefreshToken = refreshTokenHash;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(AuthConstants.RefreshTokenExpiryDays);
            user.LastLoginAt = DateTime.UtcNow;

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Check for active shift
            var activeShift = await _unitOfWork.Repository<Shift>().Query()
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.Status == ShiftStatus.Open);

            _logger.LogInformation("User {Username} logged in via PIN for store {StoreId}", user.Username, request.StoreId);

            var response = new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = AuthConstants.AccessTokenExpiryMinutes * 60,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.ToString(),
                    StoreId = user.StoreId,
                    StoreName = user.Store?.Name,
                    HasActiveShift = activeShift != null,
                    ActiveShiftId = activeShift?.Id
                }
            };

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "PIN login successful"));
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PIN login for store {StoreId}", request.StoreId);
            throw;
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New authentication token</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var refreshTokenHash = _securityService.HashToken(request.RefreshToken);

            var user = await _unitOfWork.Repository<User>().Query()
                .Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenHash);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token attempted");
                throw AuthenticationException.InvalidRefreshToken();
            }

            var token = GenerateJwtToken(user);
            var newRefreshToken = _securityService.GenerateSecureToken(AuthConstants.RefreshTokenByteSize);
            var newRefreshTokenHash = _securityService.HashToken(newRefreshToken);

            // Update user with new refresh token hash
            user.RefreshToken = newRefreshTokenHash;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(AuthConstants.RefreshTokenExpiryDays);

            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Refresh token renewed for user {Username}", user.Username);

            var response = new LoginResponseDto
            {
                Token = token,
                RefreshToken = newRefreshToken,
                ExpiresIn = AuthConstants.AccessTokenExpiryMinutes * 60,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role.ToString(),
                    StoreId = user.StoreId,
                    StoreName = user.Store?.Name
                }
            };

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Token refreshed successfully"));
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw;
        }
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var userId))
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;

                    _unitOfWork.Repository<User>().Update(user);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("User {UserId} logged out successfully", userId);
                }
            }

            return Ok(ApiResponse.SuccessResponse("Logged out successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw;
        }
    }

    #region Private Methods

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? 
            throw new InvalidOperationException("JWT SecretKey not configured"));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        if (user.StoreId.HasValue)
        {
            claims.Add(new Claim("StoreId", user.StoreId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(AuthConstants.AccessTokenExpiryMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion
}
