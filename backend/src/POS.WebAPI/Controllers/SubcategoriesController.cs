using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Models;
using POS.Application.DTOs.Categories;
using POS.Domain.Entities;

namespace POS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SubcategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SubcategoriesController> _logger;

    public SubcategoriesController(
        IUnitOfWork unitOfWork,
        ILogger<SubcategoriesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SubcategoryDto>>>> GetAll()
    {
        try
        {
            var subcategories = await _unitOfWork.Repository<Subcategory>().Query()
                .Include(s => s.Category)
                .Include(s => s.Products)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .Select(s => new SubcategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    Description = s.Description,
                    ImageUrl = s.ImageUrl,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.Name,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive,
                    ProductCount = s.Products.Count
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<SubcategoryDto>>
            {
                Success = true,
                Data = subcategories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategories");
            return StatusCode(500, new ApiResponse<List<SubcategoryDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving subcategories"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SubcategoryDetailDto>>> GetById(long id)
    {
        try
        {
            var subcategory = await _unitOfWork.Repository<Subcategory>().Query()
                .Include(s => s.Category)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (subcategory == null)
            {
                return NotFound(new ApiResponse<SubcategoryDetailDto>
                {
                    Success = false,
                    Message = "Subcategory not found"
                });
            }

            var dto = new SubcategoryDetailDto
            {
                Id = subcategory.Id,
                Name = subcategory.Name,
                Slug = subcategory.Slug,
                Description = subcategory.Description,
                ImageUrl = subcategory.ImageUrl,
                CategoryId = subcategory.CategoryId,
                CategoryName = subcategory.Category.Name,
                DisplayOrder = subcategory.DisplayOrder,
                IsActive = subcategory.IsActive,
                Products = subcategory.Products.Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    PriceIncGst = p.PriceIncGst,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive
                }).ToList()
            };

            return Ok(new ApiResponse<SubcategoryDetailDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategory {Id}", id);
            return StatusCode(500, new ApiResponse<SubcategoryDetailDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the subcategory"
            });
        }
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<List<SubcategoryDto>>>> GetByCategoryId(long categoryId)
    {
        try
        {
            var subcategories = await _unitOfWork.Repository<Subcategory>().Query()
                .Include(s => s.Category)
                .Where(s => s.CategoryId == categoryId)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .Select(s => new SubcategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    Description = s.Description,
                    ImageUrl = s.ImageUrl,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.Name,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive,
                    ProductCount = s.Products.Count
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<SubcategoryDto>>
            {
                Success = true,
                Data = subcategories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategories for category {CategoryId}", categoryId);
            return StatusCode(500, new ApiResponse<List<SubcategoryDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving subcategories"
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SubcategoryDto>>> Create([FromBody] CreateSubcategoryDto dto)
    {
        try
        {
            // Verify category exists
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return BadRequest(new ApiResponse<SubcategoryDto>
                {
                    Success = false,
                    Message = "Invalid category ID"
                });
            }

            var subcategory = new Subcategory
            {
                Name = dto.Name,
                Slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-"),
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Repository<Subcategory>().AddAsync(subcategory);
            await _unitOfWork.SaveChangesAsync();

            var result = new SubcategoryDto
            {
                Id = subcategory.Id,
                Name = subcategory.Name,
                Slug = subcategory.Slug,
                Description = subcategory.Description,
                ImageUrl = subcategory.ImageUrl,
                CategoryId = subcategory.CategoryId,
                CategoryName = category.Name,
                DisplayOrder = subcategory.DisplayOrder,
                IsActive = subcategory.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = subcategory.Id }, new ApiResponse<SubcategoryDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subcategory");
            return StatusCode(500, new ApiResponse<SubcategoryDto>
            {
                Success = false,
                Message = "An error occurred while creating the subcategory"
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SubcategoryDto>>> Update(long id, [FromBody] UpdateSubcategoryDto dto)
    {
        try
        {
            var subcategory = await _unitOfWork.Repository<Subcategory>().Query()
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (subcategory == null)
            {
                return NotFound(new ApiResponse<SubcategoryDto>
                {
                    Success = false,
                    Message = "Subcategory not found"
                });
            }

            // Verify category exists if changing
            if (dto.CategoryId != subcategory.CategoryId)
            {
                var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.CategoryId);
                if (category == null)
                {
                    return BadRequest(new ApiResponse<SubcategoryDto>
                    {
                        Success = false,
                        Message = "Invalid category ID"
                    });
                }
            }

            subcategory.Name = dto.Name;
            subcategory.Slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-");
            subcategory.Description = dto.Description;
            subcategory.ImageUrl = dto.ImageUrl;
            subcategory.CategoryId = dto.CategoryId;
            subcategory.DisplayOrder = dto.DisplayOrder;
            subcategory.IsActive = dto.IsActive;

            _unitOfWork.Repository<Subcategory>().Update(subcategory);
            await _unitOfWork.SaveChangesAsync();

            var result = new SubcategoryDto
            {
                Id = subcategory.Id,
                Name = subcategory.Name,
                Slug = subcategory.Slug,
                Description = subcategory.Description,
                ImageUrl = subcategory.ImageUrl,
                CategoryId = subcategory.CategoryId,
                CategoryName = subcategory.Category.Name,
                DisplayOrder = subcategory.DisplayOrder,
                IsActive = subcategory.IsActive
            };

            return Ok(new ApiResponse<SubcategoryDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subcategory {Id}", id);
            return StatusCode(500, new ApiResponse<SubcategoryDto>
            {
                Success = false,
                Message = "An error occurred while updating the subcategory"
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        try
        {
            var subcategory = await _unitOfWork.Repository<Subcategory>().Query()
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (subcategory == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Subcategory not found"
                });
            }

            if (subcategory.Products.Any())
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete subcategory with existing products"
                });
            }

            _unitOfWork.Repository<Subcategory>().Remove(subcategory);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Subcategory deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subcategory {Id}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the subcategory"
            });
        }
    }
}
