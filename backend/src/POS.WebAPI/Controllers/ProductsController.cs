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
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IUnitOfWork unitOfWork, ILogger<ProductsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] long? categoryId,
        [FromQuery] long? subcategoryId,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var query = _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || 
                                        (p.SKU != null && p.SKU.Contains(search)) ||
                                        (p.Barcode != null && p.Barcode.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Subcategory.CategoryId == categoryId.Value);
            }

            if (subcategoryId.HasValue)
            {
                query = query.Where(p => p.SubcategoryId == subcategoryId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var products = await query.Select(p => new ProductListDto
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
            }).ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(long id)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    SKU = p.SKU,
                    Barcode = p.Barcode,
                    Description = p.Description,
                    PriceExGst = p.PriceExGst,
                    GstAmount = p.GstAmount,
                    PriceIncGst = p.PriceIncGst,
                    Cost = p.Cost,
                    PackNotes = p.PackNotes,
                    PackSize = p.PackSize,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    TrackInventory = p.TrackInventory,
                    StockQuantity = p.StockQuantity,
                    LowStockThreshold = p.LowStockThreshold,
                    SubcategoryId = p.SubcategoryId,
                    SubcategoryName = p.Subcategory.Name,
                    CategoryId = p.Subcategory.CategoryId,
                    CategoryName = p.Subcategory.Category.Name
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductDto productDto)
    {
        try
        {
            var product = new Product
            {
                Name = productDto.Name,
                Slug = productDto.Slug,
                SKU = productDto.SKU,
                Barcode = productDto.Barcode,
                Description = productDto.Description,
                PriceExGst = productDto.PriceExGst,
                GstAmount = productDto.GstAmount,
                PriceIncGst = productDto.PriceIncGst,
                Cost = productDto.Cost,
                PackNotes = productDto.PackNotes,
                PackSize = productDto.PackSize,
                ImageUrl = productDto.ImageUrl,
                IsActive = productDto.IsActive,
                TrackInventory = productDto.TrackInventory,
                StockQuantity = productDto.StockQuantity,
                LowStockThreshold = productDto.LowStockThreshold,
                SubcategoryId = productDto.SubcategoryId
            };

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            productDto.Id = product.Id;
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(long id, [FromBody] ProductDto productDto)
    {
        try
        {
            if (id != productDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = productDto.Name;
            product.Slug = productDto.Slug;
            product.SKU = productDto.SKU;
            product.Barcode = productDto.Barcode;
            product.Description = productDto.Description;
            product.PriceExGst = productDto.PriceExGst;
            product.GstAmount = productDto.GstAmount;
            product.PriceIncGst = productDto.PriceIncGst;
            product.Cost = productDto.Cost;
            product.PackNotes = productDto.PackNotes;
            product.PackSize = productDto.PackSize;
            product.ImageUrl = productDto.ImageUrl;
            product.IsActive = productDto.IsActive;
            product.TrackInventory = productDto.TrackInventory;
            product.StockQuantity = productDto.StockQuantity;
            product.LowStockThreshold = productDto.LowStockThreshold;
            product.SubcategoryId = productDto.SubcategoryId;

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(long id)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _unitOfWork.Repository<Product>().Remove(product);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "An error occurred while deleting the product");
        }
    }

    [HttpGet("by-barcode/{barcode}")]
    public async Task<ActionResult<ProductDto>> GetProductByBarcode(string barcode)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .Where(p => p.Barcode == barcode)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    SKU = p.SKU,
                    Barcode = p.Barcode,
                    PriceIncGst = p.PriceIncGst,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    ImageUrl = p.ImageUrl,
                    SubcategoryName = p.Subcategory.Name,
                    CategoryName = p.Subcategory.Category.Name
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product by barcode {Barcode}", barcode);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }
}
