namespace POS.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Cancelled = 4,
    Refunded = 5,
    PartiallyRefunded = 6,
    OnHold = 7
}

public enum OrderType
{
    DineIn = 1,
    TakeAway = 2,
    Delivery = 3,
    Pickup = 4
}

public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    MobilePayment = 4,
    GiftCard = 5,
    LoyaltyPoints = 6,
    Other = 7
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4,
    PartiallyRefunded = 5,
    Cancelled = 6
}

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Cashier = 3,
    Staff = 4,
    ReadOnly = 5,
    Customer = 6
}

public enum ShiftStatus
{
    Open = 1,
    Closed = 2,
    Suspended = 3,
    Reconciled = 4
}

public enum InventoryTransactionType
{
    Purchase = 1,
    Sale = 2,
    Return = 3,
    Adjustment = 4,
    Transfer = 5,
    Damage = 6,
    Theft = 7,
    InitialStock = 8
}
