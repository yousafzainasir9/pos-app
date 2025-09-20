using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Domain.Entities;
using POS.WebAPI.DTOs;

namespace POS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(
        IUnitOfWork unitOfWork,
        ILogger<SuppliersController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SupplierDto>>>> GetAll()
    {
        try
        {
            var suppliers = await _unitOfWork.Repository<Supplier>().Query()
                .OrderBy(s => s.Name)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactPerson = s.ContactPerson,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    City = s.City,
                    State = s.State,
                    PostalCode = s.PostalCode,
                    Country = s.Country,
                    TaxNumber = s.TaxNumber,
                    Notes = s.Notes,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<SupplierDto>>
            {
                Success = true,
                Data = suppliers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suppliers");
            return StatusCode(500, new ApiResponse<List<SupplierDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving suppliers"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetById(long id)
    {
        try
        {
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(id);
            
            if (supplier == null)
            {
                return NotFound(new ApiResponse<SupplierDto>
                {
                    Success = false,
                    Message = "Supplier not found"
                });
            }

            var dto = new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                PostalCode = supplier.PostalCode,
                Country = supplier.Country,
                TaxNumber = supplier.TaxNumber,
                Notes = supplier.Notes,
                IsActive = supplier.IsActive
            };

            return Ok(new ApiResponse<SupplierDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supplier {Id}", id);
            return StatusCode(500, new ApiResponse<SupplierDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the supplier"
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Create([FromBody] CreateSupplierDto dto)
    {
        try
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                TaxNumber = dto.TaxNumber,
                Notes = dto.Notes,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Repository<Supplier>().AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();

            var result = new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                PostalCode = supplier.PostalCode,
                Country = supplier.Country,
                TaxNumber = supplier.TaxNumber,
                Notes = supplier.Notes,
                IsActive = supplier.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, new ApiResponse<SupplierDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating supplier");
            return StatusCode(500, new ApiResponse<SupplierDto>
            {
                Success = false,
                Message = "An error occurred while creating the supplier"
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Update(long id, [FromBody] UpdateSupplierDto dto)
    {
        try
        {
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(id);
            
            if (supplier == null)
            {
                return NotFound(new ApiResponse<SupplierDto>
                {
                    Success = false,
                    Message = "Supplier not found"
                });
            }

            supplier.Name = dto.Name;
            supplier.ContactPerson = dto.ContactPerson;
            supplier.Email = dto.Email;
            supplier.Phone = dto.Phone;
            supplier.Address = dto.Address;
            supplier.City = dto.City;
            supplier.State = dto.State;
            supplier.PostalCode = dto.PostalCode;
            supplier.Country = dto.Country;
            supplier.TaxNumber = dto.TaxNumber;
            supplier.Notes = dto.Notes;
            supplier.IsActive = dto.IsActive;

            _unitOfWork.Repository<Supplier>().Update(supplier);
            await _unitOfWork.SaveChangesAsync();

            var result = new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                PostalCode = supplier.PostalCode,
                Country = supplier.Country,
                TaxNumber = supplier.TaxNumber,
                Notes = supplier.Notes,
                IsActive = supplier.IsActive
            };

            return Ok(new ApiResponse<SupplierDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating supplier {Id}", id);
            return StatusCode(500, new ApiResponse<SupplierDto>
            {
                Success = false,
                Message = "An error occurred while updating the supplier"
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        try
        {
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(id);
            
            if (supplier == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Supplier not found"
                });
            }

            _unitOfWork.Repository<Supplier>().Remove(supplier);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Supplier deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier {Id}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the supplier"
            });
        }
    }
}
