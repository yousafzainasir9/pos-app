namespace POS.Application.DTOs.WhatsApp
{
    /// <summary>
    /// Customer conversation session state
    /// </summary>
    public class CustomerSession
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string State { get; set; } = "AWAITING_ORDER"; // State machine state
        public List<CartItem> Cart { get; set; } = new();
        public string DeliveryAddress { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? StoreId { get; set; } // Store they're ordering from
        public string? OrderNumber { get; set; } // Completed order number
    }
    
    /// <summary>
    /// Cart item in customer session
    /// </summary>
    public class CartItem
    {
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// Session state enum
    /// </summary>
    public static class SessionState
    {
        public const string INITIAL = "INITIAL";
        public const string AWAITING_ORDER = "AWAITING_ORDER";
        public const string AWAITING_NAME = "AWAITING_NAME";
        public const string AWAITING_ADDRESS = "AWAITING_ADDRESS";
        public const string AWAITING_INSTRUCTIONS = "AWAITING_INSTRUCTIONS";
        public const string AWAITING_CONFIRMATION = "AWAITING_CONFIRMATION";
        public const string ORDER_PLACED = "ORDER_PLACED";
    }
}
