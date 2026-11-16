using Microsoft.EntityFrameworkCore;
using ShopEasy.BuildingBlocks.Infrastructure.Outbox;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        modelBuilder.Entity<Order>(order =>
        {
            order.ToTable("orders");
            order.HasKey(o => o.Id);
            order.Property(o => o.Id).ValueGeneratedNever();
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            order.Property(o => o.PaymentToken).HasMaxLength(100);
            order.Property(o => o.CancellationReason).HasMaxLength(500);
            order.HasIndex(o => o.CustomerId);
            order.Ignore(o => o.DomainEvents);
            order.ComplexProperty(o => o.Total, total =>
            {
                total.Property(money => money.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
                total.Property(money => money.Currency).HasColumnName("TotalCurrency").HasMaxLength(3);
            });
            order.ComplexProperty(o => o.ShippingAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
                address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(50);
                address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20);
            });
            order.HasMany(o => o.Lines).WithOne().HasForeignKey("OrderId").OnDelete(DeleteBehavior.Cascade);
            order.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderLine>(line =>
        {
            line.ToTable("order_lines");
            line.HasKey(l => l.Id);
            line.Property(l => l.Id).ValueGeneratedNever();
            line.Property(l => l.ProductName).HasMaxLength(200);
            line.Ignore(l => l.Total);
            line.ComplexProperty(l => l.UnitPrice, price =>
            {
                price.Property(money => money.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 2);
                price.Property(money => money.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });
        });
    }
}
