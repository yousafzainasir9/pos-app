using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;

namespace POS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Subcategory> Subcategories { get; }
    DbSet<Product> Products { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Store> Stores { get; }
    DbSet<User> Users { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Shift> Shifts { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
