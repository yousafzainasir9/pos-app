using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Models;
using POS.Application.DTOs;

namespace POS.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<StoresController> _logger;

    public StoresController(IApplicationDbContext context, ILogger<StoresController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/stores
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StoreDto>>>> GetStores()
    {
        try
        {
            var stores = await _context.Stores
                .Where(s => s.IsActive)
                .Select(s => new StoreDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    Address = s.Address,
                    City = s.City,
                    Phone = s.Phone,
                    Email = s.Email,
                    TaxRate = s.TaxRate,
                    Currency = s.Currency,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<StoreDto>>
            {
                Success = true,
                Data = stores,
                Message = "Stores retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stores");
            return StatusCode(500, new ApiResponse<List<StoreDto>>
            {
                Success = false,
                Message = "Failed to retrieve stores",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    // GET: api/stores/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<StoreDetailDto>>> GetStore(long id)
    {
        try
        {
            var store = await _context.Stores
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (store == null)
            {
                return NotFound(new ApiResponse<StoreDetailDto>
                {
                    Success = false,
                    Message = "Store not found"
                });
            }

            var storeDetail = new StoreDetailDto
            {
                Id = store.Id,
                Name = store.Name,
                Code = store.Code,
                Address = store.Address,
                City = store.City,
                State = store.State,
                PostalCode = store.PostalCode,
                Country = store.Country,
                Phone = store.Phone,
                Email = store.Email,
                TaxNumber = store.TaxNumber,
                TaxRate = store.TaxRate,
                Currency = store.Currency,
                IsActive = store.IsActive,
                OpeningTime = store.OpeningTime,
                ClosingTime = store.ClosingTime,
                ActiveUserCount = store.Users.Count(u => u.IsActive)
            };

            return Ok(new ApiResponse<StoreDetailDto>
            {
                Success = true,
                Data = storeDetail,
                Message = "Store retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store {StoreId}", id);
            return StatusCode(500, new ApiResponse<StoreDetailDto>
            {
                Success = false,
                Message = "Failed to retrieve store",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    // GET: api/stores/current
    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<StoreDto>>> GetCurrentStore()
    {
        try
        {
            var store = await _context.Stores
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync();

            if (store == null)
            {
                return NotFound(new ApiResponse<StoreDto>
                {
                    Success = false,
                    Message = "No active store found"
                });
            }

            var storeDto = new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Code = store.Code,
                Address = store.Address,
                City = store.City,
                Phone = store.Phone,
                Email = store.Email,
                TaxRate = store.TaxRate,
                Currency = store.Currency,
                IsActive = store.IsActive
            };

            return Ok(new ApiResponse<StoreDto>
            {
                Success = true,
                Data = storeDto,
                Message = "Current store retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current store");
            return StatusCode(500, new ApiResponse<StoreDto>
            {
                Success = false,
                Message = "Failed to retrieve current store",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    // PUT: api/stores/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StoreDto>>> UpdateStore(long id, [FromBody] UpdateStoreDto dto)
    {
        try
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound(new ApiResponse<StoreDto>
                {
                    Success = false,
                    Message = "Store not found"
                });
            }

            if (!string.IsNullOrEmpty(dto.Name))
                store.Name = dto.Name;

            if (dto.Address != null)
                store.Address = dto.Address;

            if (dto.City != null)
                store.City = dto.City;

            if (dto.State != null)
                store.State = dto.State;

            if (dto.PostalCode != null)
                store.PostalCode = dto.PostalCode;

            if (dto.Country != null)
                store.Country = dto.Country;

            if (dto.Phone != null)
                store.Phone = dto.Phone;

            if (dto.Email != null)
                store.Email = dto.Email;

            if (dto.TaxNumber != null)
                store.TaxNumber = dto.TaxNumber;

            if (dto.TaxRate.HasValue)
                store.TaxRate = dto.TaxRate.Value;

            if (!string.IsNullOrEmpty(dto.Currency))
                store.Currency = dto.Currency;

            if (dto.OpeningTime.HasValue)
                store.OpeningTime = dto.OpeningTime.Value;

            if (dto.ClosingTime.HasValue)
                store.ClosingTime = dto.ClosingTime.Value;

            await _context.SaveChangesAsync(CancellationToken.None);

            var storeDto = new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Code = store.Code,
                Address = store.Address,
                City = store.City,
                Phone = store.Phone,
                Email = store.Email,
                TaxRate = store.TaxRate,
                Currency = store.Currency,
                IsActive = store.IsActive
            };

            return Ok(new ApiResponse<StoreDto>
            {
                Success = true,
                Data = storeDto,
                Message = "Store updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating store {StoreId}", id);
            return StatusCode(500, new ApiResponse<StoreDto>
            {
                Success = false,
                Message = "Failed to update store",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
