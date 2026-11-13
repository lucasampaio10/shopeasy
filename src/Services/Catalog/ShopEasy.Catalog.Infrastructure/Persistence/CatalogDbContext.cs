using Microsoft.EntityFrameworkCore;
using ShopEasy.BuildingBlocks.Infrastructure.Outbox;
using ShopEasy.Catalog.Domain.Products;
using ShopEasy.Catalog.Domain.Reservations;

namespace ShopEasy.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        modelBuilder.Entity<Product>(product =>
        {
            product.ToTable("products");
            product.HasKey(p => p.Id);
            product.Property(p => p.Id).ValueGeneratedNever();
            product.Property(p => p.Name).HasMaxLength(200);
            product.Property(p => p.Sku).HasMaxLength(50);
            product.HasIndex(p => p.Sku).IsUnique();
            product.Property(p => p.Description).HasMaxLength(2000);
            product.Property(p => p.Version).IsRowVersion();
            product.Ignore(p => p.DomainEvents);
            product.ComplexProperty(p => p.Price, price =>
            {
                price.Property(money => money.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
                price.Property(money => money.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
            });
        });

        modelBuilder.Entity<StockReservation>(reservation =>
        {
            reservation.ToTable("stock_reservations");
            reservation.HasKey(r => r.Id);
            reservation.Property(r => r.Id).ValueGeneratedNever();
            reservation.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            reservation.Ignore(r => r.DomainEvents);
            reservation.Ignore(r => r.OrderId);
            reservation.OwnsMany(r => r.Lines, line =>
            {
                line.ToTable("stock_reservation_lines");
                line.WithOwner().HasForeignKey("ReservationId");
                line.HasKey("ReservationId", nameof(ReservedLine.ProductId));
            });
        });
    }
}
