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
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        IUnitOfWork unitOfWork,
        ILogger<CategoriesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
    {
        try
        {
            var categories = await _unitOfWork.Repository<Category>().Query()
                .Include(c => c.Subcategories)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    SubcategoryCount = c.Subcategories.Count,
                    ProductCount = c.Subcategories.SelectMany(s => s.Products).Count()
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<CategoryDto>>
            {
                Success = true,
                Data = categories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new ApiResponse<List<CategoryDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving categories"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailDto>>> GetById(long id)
    {
        try
        {
            var category = await _unitOfWork.Repository<Category>().Query()
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (category == null)
            {
                return NotFound(new ApiResponse<CategoryDetailDto>
                {
                    Success = false,
                    Message = "Category not found"
                });
            }

            var dto = new CategoryDetailDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                Subcategories = category.Subcategories.Select(s => new SubcategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    Description = s.Description,
                    CategoryId = s.CategoryId,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive
                }).ToList()
            };

            return Ok(new ApiResponse<CategoryDetailDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {Id}", id);
            return StatusCode(500, new ApiResponse<CategoryDetailDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the category"
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var category = new Category
            {
                Name = dto.Name,
                Slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-"),
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            };

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new ApiResponse<CategoryDto>
            {
                Success = false,
                Message = "An error occurred while creating the category"
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(long id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            
            if (category == null)
            {
                return NotFound(new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = "Category not found"
                });
            }

            category.Name = dto.Name;
            category.Slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-");
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl;
            category.DisplayOrder = dto.DisplayOrder;
            category.IsActive = dto.IsActive;

            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.SaveChangesAsync();

            var result = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            return Ok(new ApiResponse<CategoryDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", id);
            return StatusCode(500, new ApiResponse<CategoryDto>
            {
                Success = false,
                Message = "An error occurred while updating the category"
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        try
        {
            var category = await _unitOfWork.Repository<Category>().Query()
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (category == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Category not found"
                });
            }

            if (category.Subcategories.Any())
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete category with existing subcategories"
                });
            }

            _unitOfWork.Repository<Category>().Remove(category);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Category deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the category"
            });
        }
    }
}
