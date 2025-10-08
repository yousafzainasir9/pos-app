using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Models;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Domain.Enums;
using POS.Infrastructure.Data.Interceptors;

namespace POS.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrdersController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public OrdersController(
        IUnitOfWork unitOfWork, 
        ILogger<OrdersController> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetOrdersSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] OrderStatus? status)
    {
        try
        {
            var query = _unitOfWork.Repository<Order>().Query().AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Add one day to include the entire end date
                var endDate = toDate.Value.Date.AddDays(1);
                query = query.Where(o => o.OrderDate < endDate);
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // Calculate summary statistics efficiently
            var totalOrders = await query.CountAsync();
            var totalSales = await query
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            var pendingOrders = await query
                .CountAsync(o => o.Status == OrderStatus.Pending);
            var processingOrders = await query
                .CountAsync(o => o.Status == OrderStatus.Processing);

            return Ok(new
            {
                totalOrders,
                totalSales,
                pendingOrders,
                processingOrders
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders summary");
            return StatusCode(500, "An error occurred while retrieving orders summary");
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetOrders(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] OrderStatus? status,
        [FromQuery] long? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Max 100 items per page

            var query = _unitOfWork.Repository<Order>().Query()
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Store)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Add one day to include the entire end date
                var endDate = toDate.Value.Date.AddDays(1);
                query = query.Where(o => o.OrderDate < endDate);
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (customerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == customerId.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    OrderType = o.OrderType,
                    SubTotal = o.SubTotal,
                    DiscountAmount = o.DiscountAmount,
                    TaxAmount = o.TaxAmount,
                    TotalAmount = o.TotalAmount,
                    PaidAmount = o.PaidAmount,
                    ChangeAmount = o.ChangeAmount,
                    TableNumber = o.TableNumber,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer != null ? o.Customer.FullName : null,
                    CashierName = o.User.FullName,
                    StoreName = o.Store.Name,
                    CompletedAt = o.CompletedAt
                })
                .ToListAsync();

            // Calculate pagination metadata
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                data = orders,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    hasNext = page < totalPages,
                    hasPrevious = page > 1
                }
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                new ErrorResponse("INTERNAL_ERROR", "An error occurred while retrieving orders")));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(long id)
    {
        try
        {
            var order = await _unitOfWork.Repository<Order>().Query()
                .Include(o => o.Customer)
                .Include(o => o.User)
                .Include(o => o.Store)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                OrderType = order.OrderType,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                PaidAmount = order.PaidAmount,
                ChangeAmount = order.ChangeAmount,
                Notes = order.Notes,
                TableNumber = order.TableNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName,
                CashierName = order.User.FullName,
                StoreName = order.Store.Name,
                CompletedAt = order.CompletedAt,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductSKU = oi.Product.SKU,
                    Quantity = oi.Quantity,
                    UnitPriceIncGst = oi.UnitPriceIncGst,
                    DiscountAmount = oi.DiscountAmount,
                    TotalAmount = oi.TotalAmount,
                    Notes = oi.Notes,
                    IsVoided = oi.IsVoided
                }).ToList(),
                Payments = order.Payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status,
                    ReferenceNumber = p.ReferenceNumber,
                    CardLastFourDigits = p.CardLastFourDigits,
                    CardType = p.CardType,
                    PaymentDate = p.PaymentDate
                }).ToList()
            };

            return Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "An error occurred while retrieving the order");
        }
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Get current user and store
            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
            var currentUser = await _unitOfWork.Repository<User>().GetByIdAsync(currentUserId);
            if (currentUser == null || currentUser.StoreId == null)
            {
                throw new InvalidOperationException("User not associated with a store");
            }

            // Get active shift for the user
            var activeShift = await _unitOfWork.Repository<Shift>().Query()
                .Where(s => s.UserId == currentUserId && s.Status == ShiftStatus.Open)
                .FirstOrDefaultAsync();

            // Generate order number
            var orderNumber = $"ORD{DateTime.Now:yyyyMMddHHmmss}";

            // Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                OrderType = createOrderDto.OrderType,
                TableNumber = createOrderDto.TableNumber,
                CustomerId = createOrderDto.CustomerId,
                Notes = createOrderDto.Notes,
                UserId = currentUserId,
                StoreId = currentUser.StoreId.Value,
                ShiftId = activeShift?.Id,
                DiscountAmount = createOrderDto.DiscountAmount ?? 0
            };

            decimal subTotal = 0;
            decimal taxTotal = 0;

            // Add order items
            foreach (var itemDto in createOrderDto.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest($"Product with ID {itemDto.ProductId} not found");
                }

                // Check stock
                if (product.TrackInventory && product.StockQuantity < itemDto.Quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest($"Insufficient stock for product {product.Name}");
                }

                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPriceExGst = product.PriceExGst,
                    UnitGstAmount = product.GstAmount,
                    UnitPriceIncGst = product.PriceIncGst,
                    DiscountAmount = itemDto.DiscountAmount ?? 0,
                    Notes = itemDto.Notes
                };

                // Calculate totals
                var itemSubTotal = orderItem.Quantity * orderItem.UnitPriceExGst;
                var itemTax = orderItem.Quantity * orderItem.UnitGstAmount;
                var itemTotal = (itemSubTotal + itemTax) - orderItem.DiscountAmount;

                orderItem.SubTotal = itemSubTotal;
                orderItem.TaxAmount = itemTax;
                orderItem.TotalAmount = itemTotal;

                subTotal += itemSubTotal;
                taxTotal += itemTax;

                order.OrderItems.Add(orderItem);

                // Update inventory
                if (product.TrackInventory)
                {
                    product.StockQuantity -= itemDto.Quantity;
                    _unitOfWork.Repository<Product>().Update(product);

                    // Create inventory transaction
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = product.Id,
                        StoreId = order.StoreId,
                        TransactionType = InventoryTransactionType.Sale,
                        Quantity = -itemDto.Quantity,
                        StockBefore = product.StockQuantity + itemDto.Quantity,
                        StockAfter = product.StockQuantity,
                        TransactionDate = DateTime.Now,
                        UserId = currentUserId,
                        Order = order,
                        Notes = $"Sale - Order #{orderNumber}"
                    };

                    await _unitOfWork.Repository<InventoryTransaction>().AddAsync(inventoryTransaction);
                }
            }

            // Calculate order totals
            order.SubTotal = subTotal;
            order.TaxAmount = taxTotal;
            order.TotalAmount = subTotal + taxTotal - order.DiscountAmount;

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new { orderId = order.Id, orderNumber = order.OrderNumber });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "An error occurred while creating the order");
        }
    }

    [HttpPost("{id}/payments")]
    public async Task<ActionResult> ProcessPayment(long id, [FromBody] ProcessPaymentDto paymentDto)
    {
        try
        {
            var order = await _unitOfWork.Repository<Order>().Query()
                .Include(o => o.Payments)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
            {
                return BadRequest("Order cannot accept payments in its current status");
            }

            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

            // Create payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = paymentDto.Amount,
                PaymentMethod = paymentDto.PaymentMethod,
                Status = PaymentStatus.Completed,
                ReferenceNumber = paymentDto.ReferenceNumber,
                CardLastFourDigits = paymentDto.CardLastFourDigits,
                CardType = paymentDto.CardType,
                PaymentDate = DateTime.Now,
                ProcessedByUserId = currentUserId,
                Notes = paymentDto.Notes
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment);

            // Update order payment totals
            order.PaidAmount += payment.Amount;
            
            if (order.PaidAmount >= order.TotalAmount)
            {
                order.Status = OrderStatus.Completed;
                order.CompletedAt = DateTime.Now;
                order.ChangeAmount = order.PaidAmount - order.TotalAmount;
            }
            else
            {
                order.Status = OrderStatus.Processing;
            }

            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { 
                message = "Payment processed successfully", 
                orderStatus = order.Status.ToString(),
                paidAmount = order.PaidAmount,
                remainingAmount = Math.Max(0, order.TotalAmount - order.PaidAmount),
                changeAmount = order.ChangeAmount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", id);
            return StatusCode(500, "An error occurred while processing the payment");
        }
    }

    [HttpPost("{id}/void")]
    public async Task<ActionResult> VoidOrder(long id, [FromBody] VoidOrderDto voidDto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var order = await _unitOfWork.Repository<Order>().Query()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Refunded)
            {
                return BadRequest("Order is already voided or refunded");
            }

            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

            // Restore inventory
            foreach (var item in order.OrderItems.Where(oi => !oi.IsVoided))
            {
                if (item.Product.TrackInventory)
                {
                    item.Product.StockQuantity += item.Quantity;
                    _unitOfWork.Repository<Product>().Update(item.Product);

                    // Create inventory transaction
                    var inventoryTransaction = new InventoryTransaction
                    {
                        ProductId = item.Product.Id,
                        StoreId = order.StoreId,
                        TransactionType = InventoryTransactionType.Return,
                        Quantity = item.Quantity,
                        StockBefore = item.Product.StockQuantity - item.Quantity,
                        StockAfter = item.Product.StockQuantity,
                        TransactionDate = DateTime.Now,
                        UserId = currentUserId,
                        OrderId = order.Id,
                        Notes = $"Void - Order #{order.OrderNumber}"
                    };

                    await _unitOfWork.Repository<InventoryTransaction>().AddAsync(inventoryTransaction);
                }
            }

            // Update order status
            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.Now;
            order.CancellationReason = voidDto.Reason;

            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Ok(new { message = "Order voided successfully" });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error voiding order {OrderId}", id);
            return StatusCode(500, "An error occurred while voiding the order");
        }
    }

    [HttpGet("current-shift")]
    public async Task<ActionResult<object>> GetCurrentShiftOrders()
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
            
            var activeShift = await _unitOfWork.Repository<Shift>().Query()
                .Include(s => s.Orders)
                .Where(s => s.UserId == currentUserId && s.Status == ShiftStatus.Open)
                .FirstOrDefaultAsync();

            if (activeShift == null)
            {
                return Ok(new { message = "No active shift", orders = new List<OrderDto>() });
            }

            var orders = await _unitOfWork.Repository<Order>().Query()
                .Where(o => o.ShiftId == activeShift.Id)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount,
                    o.PaidAmount,
                    CustomerName = o.Customer != null ? o.Customer.FullName : "Walk-in"
                })
                .ToListAsync();

            return Ok(new
            {
                shiftId = activeShift.Id,
                shiftNumber = activeShift.ShiftNumber,
                startTime = activeShift.StartTime,
                totalOrders = orders.Count,
                totalSales = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
                orders
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current shift orders");
            return StatusCode(500, "An error occurred while retrieving shift orders");
        }
    }
}

public class VoidOrderDto
{
    public string Reason { get; set; } = string.Empty;
}
