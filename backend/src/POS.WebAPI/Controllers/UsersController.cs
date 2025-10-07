using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Models;
using POS.Application.DTOs;
using POS.Domain.Entities;
using BCrypt.Net;

namespace POS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class UsersController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IApplicationDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<UserDto>>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Store)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role.ToString() == role);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = $"{u.FirstName} {u.LastName}",
                    Phone = u.Phone,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    StoreId = u.StoreId,
                    StoreName = u.Store != null ? u.Store.Name : null
                })
                .ToListAsync();

            var paginatedList = new PaginatedList<UserDto>(users, totalCount, page, pageSize);

            return Ok(ApiResponse<PaginatedList<UserDto>>.SuccessResponse(paginatedList, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new ApiResponse<PaginatedList<UserDto>>
            {
                Success = false,
                Message = "Failed to retrieve users"
            });
        }
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDetailDto>>> GetUser(long id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new ApiResponse<UserDetailDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            var userDetail = new UserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                StoreId = user.StoreId,
                StoreName = user.Store?.Name,
                CreatedAt = user.CreatedOn,
                UpdatedAt = user.ModifiedOn,
                HasPin = !string.IsNullOrEmpty(user.Pin)
            };

            return Ok(ApiResponse<UserDetailDto>.SuccessResponse(userDetail, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, new ApiResponse<UserDetailDto>
            {
                Success = false,
                Message = "Failed to retrieve user"
            });
        }
    }

    // POST: api/users
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Username already exists"
                });
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Email already exists"
                });
            }

            if (!Enum.TryParse<Domain.Enums.UserRole>(dto.Role, out var userRole))
            {
                return BadRequest(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Invalid role"
                });
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Pin = !string.IsNullOrEmpty(dto.Pin) ? BCrypt.Net.BCrypt.HashPassword(dto.Pin) : null,
                Role = userRole,
                IsActive = true,
                StoreId = dto.StoreId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(CancellationToken.None);

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                StoreId = user.StoreId
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, 
                ApiResponse<UserDto>.SuccessResponse(userDto, "User created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Failed to create user"
            });
        }
    }

    // PUT: api/users/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(long id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            if (!string.IsNullOrEmpty(dto.Email))
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                {
                    return BadRequest(new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Email already exists"
                    });
                }
                user.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                user.LastName = dto.LastName;

            if (dto.Phone != null)
                user.Phone = dto.Phone;

            if (!string.IsNullOrEmpty(dto.Role))
            {
                if (Enum.TryParse<Domain.Enums.UserRole>(dto.Role, out var userRole))
                {
                    user.Role = userRole;
                }
            }

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            if (dto.StoreId.HasValue)
                user.StoreId = dto.StoreId.Value;

            await _context.SaveChangesAsync(CancellationToken.None);

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                StoreId = user.StoreId
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(userDto, "User updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Failed to update user"
            });
        }
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(long id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            user.IsActive = false;
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(ApiResponse.SuccessResponse("User deactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Failed to deactivate user"
            });
        }
    }

    // POST: api/users/{id}/reset-password
    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> ResetPassword(long id, [FromBody] ResetPasswordDto dto)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(ApiResponse.SuccessResponse("Password reset successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", id);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Failed to reset password"
            });
        }
    }

    // POST: api/users/{id}/reset-pin
    [HttpPost("{id}/reset-pin")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse>> ResetPin(long id, [FromBody] ResetPinDto dto)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Check if the new PIN already exists for another user
            var pinExists = await _context.Users
                .AnyAsync(u => u.Pin == dto.NewPin && u.Id != id);

            if (pinExists)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "PIN already exists"
                });
            }

            user.Pin = dto.NewPin;
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(ApiResponse.SuccessResponse("PIN reset successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting PIN for user {UserId}", id);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Failed to reset PIN"
            });
        }
    }

    // GET: api/users/{id}/activity
    [HttpGet("{id}/activity")]
    public async Task<ActionResult<ApiResponse<List<UserActivityDto>>>> GetUserActivity(
        long id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<List<UserActivityDto>>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            var query = _context.Orders.Where(o => o.UserId == id);

            if (from.HasValue)
                query = query.Where(o => o.OrderDate >= from.Value);

            if (to.HasValue)
                query = query.Where(o => o.OrderDate <= to.Value);

            var activities = await query
                .OrderByDescending(o => o.OrderDate)
                .Take(50)
                .Select(o => new UserActivityDto
                {
                    Timestamp = o.OrderDate,
                    Action = "Order Created",
                    Description = $"Created order {o.OrderNumber} - ${o.TotalAmount}",
                    IpAddress = null
                })
                .ToListAsync();

            return Ok(ApiResponse<List<UserActivityDto>>.SuccessResponse(activities, "User activity retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity for user {UserId}", id);
            return StatusCode(500, new ApiResponse<List<UserActivityDto>>
            {
                Success = false,
                Message = "Failed to retrieve user activity"
            });
        }
    }
}
