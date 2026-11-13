using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.Catalog.Domain.Reservations;

public enum ReservationStatus
{
    Active,
    Released,
}

public sealed record ReservedLine(Guid ProductId, int Quantity);

/// <summary>
/// Stock held for one order. Its identity is the order id, which makes reserving and releasing
/// idempotent when the same message is delivered more than once.
/// </summary>
public sealed class StockReservation : AggregateRoot<Guid>
{
    private readonly List<ReservedLine> _lines = [];

    private StockReservation()
    {
    }

    public ReservationStatus Status { get; private set; }

    public DateTime ReservedAtUtc { get; private set; }

    public IReadOnlyCollection<ReservedLine> Lines => _lines.AsReadOnly();

    public Guid OrderId => Id;

    internal static StockReservation Create(Guid orderId, IEnumerable<ReservedLine> lines)
    {
        var reservation = new StockReservation
        {
            Id = orderId,
            Status = ReservationStatus.Active,
            ReservedAtUtc = DateTime.UtcNow,
        };
        reservation._lines.AddRange(lines);

        return reservation;
    }

    /// <returns><c>true</c> when the stock has to be returned now; <c>false</c> if it already was.</returns>
    public bool Release()
    {
        if (Status == ReservationStatus.Released)
        {
            return false;
        }

        Status = ReservationStatus.Released;
        return true;
    }
}

public interface IStockReservationRepository
{
    Task<StockReservation?> GetByOrderId(Guid orderId, CancellationToken cancellationToken);

    void Add(StockReservation reservation);
}
