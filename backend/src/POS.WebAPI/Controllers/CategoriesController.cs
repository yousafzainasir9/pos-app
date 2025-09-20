using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.DTOs;
using POS.Domain.Entities;

namespace POS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IUnitOfWork unitOfWork, ILogger<CategoriesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] bool includeSubcategories = false)
    {
        try
        {
            IQueryable<Category> query = _unitOfWork.Repository<Category>().Query()
                .Where(c => c.IsActive);

            if (includeSubcategories)
            {
                query = query.Include(c => c.Subcategories);
            }

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    Subcategories = includeSubcategories 
                        ? c.Subcategories
                            .Where(s => s.IsActive)
                            .OrderBy(s => s.DisplayOrder)
                            .Select(s => new SubcategoryDto
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Slug = s.Slug,
                                Description = s.Description,
                                DisplayOrder = s.DisplayOrder,
                                IsActive = s.IsActive,
                                CategoryId = c.Id,
                                CategoryName = c.Name
                            }).ToList()
                        : new List<SubcategoryDto>()
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(long id)
    {
        try
        {
            var category = await _unitOfWork.Repository<Category>().Query()
                .Include(c => c.Subcategories)
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    Subcategories = c.Subcategories
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.DisplayOrder)
                        .Select(s => new SubcategoryDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Slug = s.Slug,
                            Description = s.Description,
                            DisplayOrder = s.DisplayOrder,
                            IsActive = s.IsActive,
                            CategoryId = c.Id,
                            CategoryName = c.Name
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the category");
        }
    }

    [HttpGet("{categoryId}/subcategories")]
    public async Task<ActionResult<IEnumerable<SubcategoryDto>>> GetSubcategories(long categoryId)
    {
        try
        {
            var subcategories = await _unitOfWork.Repository<Subcategory>().Query()
                .Include(s => s.Category)
                .Where(s => s.CategoryId == categoryId && s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new SubcategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    Description = s.Description,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category.Name
                })
                .ToListAsync();

            return Ok(subcategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subcategories for category {CategoryId}", categoryId);
            return StatusCode(500, "An error occurred while retrieving subcategories");
        }
    }

    [HttpGet("subcategories/{subcategoryId}/products")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetSubcategoryProducts(long subcategoryId)
    {
        try
        {
            var products = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .Where(p => p.SubcategoryId == subcategoryId && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    PriceIncGst = p.PriceIncGst,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Subcategory.Category.Name,
                    SubcategoryName = p.Subcategory.Name
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for subcategory {SubcategoryId}", subcategoryId);
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }
}
