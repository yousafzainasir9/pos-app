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
    public async Task<ActionResult<ApiResponse<List<ProductListDto>>>> GetProducts(
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
                .Include(p => p.Supplier)
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
                Slug = p.Slug,
                SKU = p.SKU,
                Barcode = p.Barcode,
                Description = p.Description,
                PriceExGst = p.PriceExGst,
                GstAmount = p.GstAmount,
                PriceIncGst = p.PriceIncGst,
                Cost = p.Cost ?? 0,
                PackSize = p.PackSize,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                TrackInventory = p.TrackInventory,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                SubcategoryId = p.SubcategoryId,
                Subcategory = p.Subcategory != null ? new SubcategoryDto
                {
                    Id = p.Subcategory.Id,
                    Name = p.Subcategory.Name,
                    Slug = p.Subcategory.Slug,
                    CategoryId = p.Subcategory.CategoryId,
                    IsActive = p.Subcategory.IsActive,
                    DisplayOrder = p.Subcategory.DisplayOrder,
                    ProductCount = 0
                } : null,
                Category = p.Subcategory != null && p.Subcategory.Category != null ? new CategoryDto
                {
                    Id = p.Subcategory.Category.Id,
                    Name = p.Subcategory.Category.Name,
                    Slug = p.Subcategory.Category.Slug,
                    IsActive = p.Subcategory.Category.IsActive,
                    DisplayOrder = p.Subcategory.Category.DisplayOrder,
                    SubcategoryCount = 0,
                    ProductCount = 0
                } : null,
                SupplierId = p.SupplierId,
                Supplier = p.Supplier != null ? new SupplierDto
                {
                    Id = p.Supplier.Id,
                    Name = p.Supplier.Name,
                    IsActive = p.Supplier.IsActive
                } : null
            }).ToListAsync();

            return Ok(new ApiResponse<List<ProductListDto>>
            {
                Success = true,
                Data = products
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new ApiResponse<List<ProductListDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving products"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(long id)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .Include(p => p.Supplier)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                SKU = product.SKU,
                Barcode = product.Barcode,
                Description = product.Description,
                PriceExGst = product.PriceExGst,
                GstAmount = product.GstAmount,
                PriceIncGst = product.PriceIncGst,
                Cost = product.Cost ?? 0,
                PackNotes = product.PackNotes,
                PackSize = product.PackSize,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                TrackInventory = product.TrackInventory,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,
                DisplayOrder = product.DisplayOrder,
                SubcategoryId = product.SubcategoryId,
                SubcategoryName = product.Subcategory?.Name,
                CategoryId = product.Subcategory?.CategoryId,
                CategoryName = product.Subcategory?.Category?.Name,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name
            };

            return Ok(new ApiResponse<ProductDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the product"
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto dto)
    {
        try
        {
            var product = new Product
            {
                Name = dto.Name,
                Slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-"),
                SKU = dto.SKU,
                Barcode = dto.Barcode,
                Description = dto.Description,
                PriceExGst = dto.PriceExGst,
                GstAmount = dto.GstAmount,
                PriceIncGst = dto.PriceIncGst,
                Cost = dto.Cost,
                PackNotes = dto.PackNotes,
                PackSize = dto.PackSize,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive,
                TrackInventory = dto.TrackInventory,
                StockQuantity = dto.StockQuantity,
                LowStockThreshold = dto.LowStockThreshold,
                DisplayOrder = dto.DisplayOrder,
                SubcategoryId = dto.SubcategoryId,
                SupplierId = dto.SupplierId
            };

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var result = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                SKU = product.SKU,
                Barcode = product.Barcode,
                Description = product.Description,
                PriceExGst = product.PriceExGst,
                GstAmount = product.GstAmount,
                PriceIncGst = product.PriceIncGst,
                Cost = product.Cost ?? 0,
                PackNotes = product.PackNotes,
                PackSize = product.PackSize,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                TrackInventory = product.TrackInventory,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,
                DisplayOrder = product.DisplayOrder,
                SubcategoryId = product.SubcategoryId,
                SupplierId = product.SupplierId
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new ApiResponse<ProductDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "An error occurred while creating the product"
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(long id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            product.Name = dto.Name;
            product.Slug = dto.Slug ?? dto.Name.ToLower().Replace(" ", "-");
            product.SKU = dto.SKU;
            product.Barcode = dto.Barcode;
            product.Description = dto.Description;
            product.PriceExGst = dto.PriceExGst;
            product.GstAmount = dto.GstAmount;
            product.PriceIncGst = dto.PriceIncGst;
            product.Cost = dto.Cost;
            product.PackNotes = dto.PackNotes;
            product.PackSize = dto.PackSize;
            product.ImageUrl = dto.ImageUrl;
            product.IsActive = dto.IsActive;
            product.TrackInventory = dto.TrackInventory;
            product.StockQuantity = dto.StockQuantity;
            product.LowStockThreshold = dto.LowStockThreshold;
            product.DisplayOrder = dto.DisplayOrder;
            product.SubcategoryId = dto.SubcategoryId;
            product.SupplierId = dto.SupplierId;

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync();

            var result = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                SKU = product.SKU,
                Barcode = product.Barcode,
                Description = product.Description,
                PriceExGst = product.PriceExGst,
                GstAmount = product.GstAmount,
                PriceIncGst = product.PriceIncGst,
                Cost = product.Cost ?? 0,
                PackNotes = product.PackNotes,
                PackSize = product.PackSize,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                TrackInventory = product.TrackInventory,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,
                DisplayOrder = product.DisplayOrder,
                SubcategoryId = product.SubcategoryId,
                SupplierId = product.SupplierId
            };

            return Ok(new ApiResponse<ProductDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "An error occurred while updating the product"
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(long id)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            _unitOfWork.Repository<Product>().Remove(product);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Product deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the product"
            });
        }
    }

    [HttpGet("by-barcode/{barcode}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductByBarcode(string barcode)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().Query()
                .Include(p => p.Subcategory)
                    .ThenInclude(s => s.Category)
                .Include(p => p.Supplier)
                .Where(p => p.Barcode == barcode)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                SKU = product.SKU,
                Barcode = product.Barcode,
                Description = product.Description,
                PriceExGst = product.PriceExGst,
                GstAmount = product.GstAmount,
                PriceIncGst = product.PriceIncGst,
                Cost = product.Cost,
                PackNotes = product.PackNotes,
                PackSize = product.PackSize,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                TrackInventory = product.TrackInventory,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,
                DisplayOrder = product.DisplayOrder,
                SubcategoryId = product.SubcategoryId,
                SubcategoryName = product.Subcategory?.Name,
                CategoryId = product.Subcategory?.CategoryId,
                CategoryName = product.Subcategory?.Category?.Name,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name
            };

            return Ok(new ApiResponse<ProductDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product by barcode {Barcode}", barcode);
            return StatusCode(500, new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the product"
            });
        }
    }
}
