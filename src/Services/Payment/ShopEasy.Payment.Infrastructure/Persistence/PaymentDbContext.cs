using Microsoft.EntityFrameworkCore;
using ShopEasy.BuildingBlocks.Infrastructure.Outbox;
using ShopEasy.Payment.Application.Payments;
using ShopEasy.Payment.Domain.Payments;
using PaymentRecord = ShopEasy.Payment.Domain.Payments.Payment;

namespace ShopEasy.Payment.Infrastructure.Persistence;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        modelBuilder.Entity<PaymentRecord>(payment =>
        {
            payment.ToTable("payments");
            payment.HasKey(p => p.Id);
            payment.Property(p => p.Id).ValueGeneratedNever();
            payment.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            payment.Property(p => p.GatewayReference).HasMaxLength(100);
            payment.Property(p => p.DeclineReason).HasMaxLength(500);
            payment.Ignore(p => p.DomainEvents);
            payment.Ignore(p => p.IsApproved);

            // Last line of defence for "charge at most once": the database refuses a second payment.
            payment.HasIndex(p => p.OrderId).IsUnique();
            payment.ComplexProperty(p => p.Amount, amount =>
            {
                amount.Property(money => money.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                amount.Property(money => money.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
        });
    }
}

internal sealed class PaymentRepository(PaymentDbContext context) : IPaymentRepository
{
    public Task<bool> ExistsForOrder(Guid orderId, CancellationToken cancellationToken) =>
        context.Payments.AnyAsync(payment => payment.OrderId == orderId, cancellationToken);

    public void Add(PaymentRecord payment) => context.Payments.Add(payment);
}

internal sealed class PaymentQueries(PaymentDbContext context) : IPaymentQueries
{
    public Task<PaymentResponse?> FindByOrder(Guid orderId, CancellationToken cancellationToken) =>
        context.Payments
            .AsNoTracking()
            .Where(payment => payment.OrderId == orderId)
            .Select(payment => new PaymentResponse(
                payment.Id,
                payment.OrderId,
                payment.Status.ToString(),
                payment.Amount.Amount,
                payment.Amount.Currency,
                payment.DeclineReason,
                payment.ProcessedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
}
