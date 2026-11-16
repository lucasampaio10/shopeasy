using ShopEasy.Catalog.Domain.Products;

namespace ShopEasy.Catalog.Domain.Reservations;

public sealed record RequestedLine(Guid ProductId, int Quantity);

public sealed record StockAllocation(StockReservation? Reservation, string? RejectionReason)
{
    public bool IsRejected => Reservation is null;
}

/// <summary>
/// Domain service for a rule that spans several products: an order is reserved entirely or not at all.
/// </summary>
public static class StockAllocator
{
    public static StockAllocation Allocate(
        Guid orderId,
        IReadOnlyCollection<RequestedLine> requestedLines,
        IReadOnlyDictionary<Guid, Product> products)
    {
        // These rejection reasons are returned to the client as-is (see OrderStatusScreen in the
        // mobile app), so they are written in the storefront's language rather than in English.
        var lines = MergeLinesOfSameProduct(requestedLines);
        if (lines.Count == 0)
        {
            return Rejected("O pedido não tem itens.");
        }

        foreach (var line in lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
            {
                return Rejected($"O produto {line.ProductId} não existe.");
            }

            if (!product.CanReserve(line.Quantity))
            {
                return Rejected($"Estoque insuficiente para '{product.Name}'.");
            }
        }

        foreach (var line in lines)
        {
            products[line.ProductId].Reserve(line.Quantity);
        }

        return new StockAllocation(StockReservation.Create(orderId, lines), RejectionReason: null);
    }

    private static List<ReservedLine> MergeLinesOfSameProduct(IEnumerable<RequestedLine> requestedLines) =>
        requestedLines
            .GroupBy(line => line.ProductId)
            .Select(group => new ReservedLine(group.Key, group.Sum(line => line.Quantity)))
            .ToList();

    private static StockAllocation Rejected(string reason) => new(Reservation: null, reason);
}
