using POS.Domain.Enums;

namespace POS.Application.DTOs;

public class OrderDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public OrderType OrderType { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? Notes { get; set; }
    public string? TableNumber { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    
    public List<OrderItemDto> Items { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class OrderItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceIncGst { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsVoided { get; set; }
}

public class CreateOrderDto
{
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public string? TableNumber { get; set; }
    public long? CustomerId { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
    public decimal? DiscountAmount { get; set; }
}

public class CreateOrderItemDto
{
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? Notes { get; set; }
}

public class PaymentDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? CardLastFourDigits { get; set; }
    public string? CardType { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class ProcessPaymentDto
{
    public long OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? CardLastFourDigits { get; set; }
    public string? CardType { get; set; }
    public string? Notes { get; set; }
}
